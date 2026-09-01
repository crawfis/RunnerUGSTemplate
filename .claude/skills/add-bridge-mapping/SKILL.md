---
name: add-bridge-mapping
description: Add a cross-domain event bridge mapping between two event domains (e.g., TempleRun to GameFlow, UGS to GameFlow). Use when a feature in one domain needs to trigger behavior in another domain.
allowed-tools: Read, Edit, Grep, Glob
argument-hint: <SourceEvent> -> <TargetEvent>
---

# Add Bridge Mapping

Add a cross-domain event mapping to one of the bridge classes. Bridges connect different event domains so they can communicate without direct coupling.

## Arguments

- `$ARGUMENTS` - Source event -> Target event (e.g., `TempleRunEvents.CoinCollected -> GameFlowEvents.ScoreUpdated`)

## Domain Isolation Rule

**Bridges are the ONLY place where cross-domain event references are allowed.** Domain code must never directly subscribe to or publish events from another domain. If TempleRun code needs to react to a GameFlow lifecycle event, the bridge must translate it into a TempleRun-domain event first.

After adding a bridge mapping, remind the user that any existing code that directly references the foreign-domain event should be refactored to subscribe to the new local-domain event instead.

## Available Bridges

| Bridge Class | File | Connects |
|-------------|------|----------|
| **Input2TempleRunAutoEventBridge** | `Assets/TempleRun/Scripts/Events/Input2TempleRunAutoEventBridge.cs` | UserInitiated -> TempleRun (one-way) |
| **TempleRunGameFlowBridge** | `Assets/GameFlow/Scripts/TempleRunSpecific/TempleRunGameFlowBridge.cs` | TempleRun <-> GameFlow (bidirectional) |
| **TempleRunUGSBridge** | `Assets/UGSGlue/TempleRunUGSBridge.cs` | gameplay -> GameServiceEvents (one-way; e.g. `DistanceUpdated`, `CoinCollected`) |
| **UGSGameFlowBridge** | `Assets/UGSGlue/UGSGameFlowBridge.cs` | GameFlow <-> GameServiceEvents (bidirectional) |
| **GameServiceEventsUGSBridge** | `Runtime/Events/GameServiceEventsUGSBridge.cs` in the `com.crawfissoftware.ugs` package | GameServiceEvents <-> UGS (read-only here — edit in the EventDrivenUGS repo) |

The game and UGS never name each other's events: anything crossing that boundary goes
through `GameServiceEvents` (the contracts package), mapped on the game side in
`Assets/UGSGlue/` and on the services side in `GameServiceEventsUGSBridge`. If the contract
lacks the crossing you need, it is added to the contracts package — deliberately rarely.

If you are adding a whole new integration domain (analytics, another backend), create it
with `/add-event-domain` — it walks through the enum, the `[EventEnum]` marking, and the
bridge class — then add the new bridge to the table above.

## CRITICAL: Prefer the pair table

**Do NOT add individual `Subscribe` / `Unsubscribe` calls for mappings that forward the
payload unchanged.** Those MUST go into the bridge's `(From, To)` pair tables, dispatched by
`EventChainDispatcher` (common package). One source event may appear in several pairs — that
fan-out is the point of the pair list.

The one sanctioned exception: a translation that must **transform** the payload cannot be a
pair (the dispatcher forwards data untouched). Those are hand-written subscriptions with a
matching `Unsubscribe` in `OnDestroy()` — see `UGSGameFlowBridge` (reading the Sticky
`ServicesStatusChanged` status and reacting per value) and `GameServiceEventsUGSBridge`
(unwrapping `CurrencyBalanceUpdate` to a plain `long`).

## Procedure

### Step 1: Identify source and target domains

From the user's request, determine:
- Which domain the source event belongs to
- Which domain the target event belongs to
- Which bridge class handles this direction

### Step 2: Verify events exist

Read both enum files to confirm the source and target events exist. If they don't, tell the user to run `/add-event` first.

### Step 3: Read the bridge file

Read the appropriate bridge class to understand:
- The existing pair tables
- The direction tables (e.g., `TempleRunToGameFlow` vs `GameFlowToTempleRun`)
- Comment style used

### Step 4: Add the mapping

Add the new `(Source, Target)` pair to the correct direction table. Include a comment
explaining why this crossing exists.

**TempleRunGameFlowBridge has two pair tables:**
- `TempleRunToGameFlow` — TempleRun fires, GameFlow receives
- `GameFlowToTempleRun` — GameFlow fires, TempleRun receives

**UGSGameFlowBridge has two pair tables (plus the hand-written status subscription):**
- `GameFlowToGameService` — GameFlow fires, the contract receives
- `GameServiceToGameFlow` — the contract fires, GameFlow receives

**TempleRunUGSBridge has one:** `GameplayToGameService` (one-way by design).
**GameServiceEventsUGSBridge has two:** `GameServiceToUGS` and `UGSToGameService`.

### Step 5: Check for circular paths

Verify the new mapping doesn't create a circular event chain:
- A -> B (bridge) -> C (auto-chain) -> A would loop infinitely
- Trace the full path from source to any auto-chained targets

### Step 6: Summarize

```
Added bridge mapping:
  [SourceDomain].[SourceEvent] -> [TargetDomain].[TargetEvent]
  In: [BridgeClassName]
  Direction: [Source] -> [Target]

Event flow path:
  [trace the full chain including any auto-chains that will fire]
```

## Example

The real coin mapping, `TempleRunEvents.CoinCollected -> GameFlowEvents.SessionCoinsChanged`:

1. Verify both events exist in their enums
2. Open `TempleRunGameFlowBridge.cs`
3. Add to `TempleRunToGameFlow`:
   ```csharp
   // Coin count for UI outside the gameplay domain (running total, not a delta)
   (TempleRunEvents.CoinCollected, GameFlowEvents.SessionCoinsChanged),
   ```
4. Trace: CoinCollected -> (bridge) -> SessionCoinsChanged -> HUD subscribers. The same
   source also crosses to the contract in `TempleRunUGSBridge`
   (`CoinCollected -> GameServiceEvents.CurrencyTotalChanged`) — one event, two bridges,
   both declarative.
