---
name: list-events
description: List all events in the RunnerUGS event system, grouped by domain and category. Shows enum values, auto-chain mappings, bridge mappings, and subscriber/publisher locations. Use to understand the current event landscape before adding features.
allowed-tools: Read, Grep, Glob
argument-hint: [domain|all]
---

# List Events

Display a comprehensive view of all events in the event system.

## Arguments

- `$ARGUMENTS` - Optional domain filter: `GameFlow`, `TempleRun`, `Countdown`, `UserInitiated`, `UGS`, or `all` (default)

## Procedure

### Step 1: Read the requested enum file(s)

| Domain | File |
|--------|------|
| GameFlow | `Assets/GameFlow/Scripts/Events/GameFlowEvents.cs` |
| TempleRun | `Assets/TempleRun/Scripts/Events/TempleRunEvents.cs` |
| Countdown | `Assets/Countdown/Scripts/Events/CountdownEvents.cs` |
| UserInitiated | `Assets/TempleRun/Scripts/Events/UserInitiatedEvents.cs` |
| GameService | `Runtime/GameServiceEvents.cs` in the `com.crawfissoftware.contracts` package |
| UGS | `Runtime/Events/UGS_EventsEnum.cs` in the `com.crawfissoftware.ugs` package (read-only here) |

> The authoritative domain list is the set of `[EventEnum]`-marked enums —
> `CrawfisSoftware > Events > List Domains` sweeps them in edit mode. If a domain exists
> that isn't in this table (added via `/add-event-domain`), include it in the listing and
> update this table.

### Step 2: Read auto-chain mappings

Read the relevant auto-flow file(s) and extract all dictionary entries.

| Domain | File |
|--------|------|
| GameFlow | `Assets/GameFlow/Scripts/Events/GameFlowAutoEventFlow.cs` |
| TempleRun | `Assets/TempleRun/Scripts/Events/TempleRunAutoEventFlow.cs` |
| Countdown | `Assets/Countdown/Scripts/Events/CountdownAutoEventFlow.cs` |
| UGS | `Runtime/Events/UGSAutoEventFlow.cs` in the `com.crawfissoftware.ugs` package (read-only here) |

### Step 3: Read bridge mappings

Read bridge files and extract the cross-domain pair tables:
- `Assets/TempleRun/Scripts/Events/Input2TempleRunAutoEventBridge.cs` (input -> gameplay)
- `Assets/GameFlow/Scripts/TempleRunSpecific/TempleRunGameFlowBridge.cs`
- `Assets/GameFlow/Scripts/CountdownSpecific/CountdownGameFlowBridge.cs` (session -> ceremony)
- `Assets/Countdown/Scripts/TempleRunSpecific/Countdown2TempleRunBridge.cs` (ceremony -> gameplay)
- `Assets/UGSGlue/UGSGameFlowBridge.cs` and `Assets/UGSGlue/TempleRunUGSBridge.cs`
- `Runtime/Events/GameServiceEventsUGSBridge.cs` in the `com.crawfissoftware.ugs` package

### Step 4: Format output

For each domain, output a table grouped by category:

```
## [Domain] Events ([count] events)
Bus: EventsFor<[Domain]Events> (static; aliased [Domain]Bus)

### [Category Name]
| Event | Value | Auto-Chain | Bridge | Notes |
|-------|-------|------------|--------|-------|
| FeatureRequested | 130 | -> FeatureStarting | | |
| FeatureStarting | 131 | | | Published by controller |
| FeatureStarted | 132 | | -> OtherDomain.X | |
| FeatureFailed | 133 | | | |
```

**Auto-Chain column**: Show `-> TargetEvent` if this event auto-triggers another.
**Bridge column**: Show `-> Domain.Event` if this event bridges to another domain.
**Notes**: Show `(target of auto-chain from X)` or `(target of bridge from Domain.X)` for events that are targets.

### Step 5: Show available value ranges

At the end, show the next available value ranges for adding new events:

```
## Available Value Ranges

| Domain | Last Used | Next Available Range |
|--------|-----------|---------------------|
| GameFlow | 141 (SessionCoinsChanged) | 150+ |
| TempleRun | 350 (SegmentGeometryReady) | 360+ |
| Countdown | 5 (CountdownEnded) | 10+ |
| UserInitiated | (implicit values — just append) | — |
| UGS | (implicit values — append in category) | — |
| GameService | 40 (CurrencyBalanceChanged) | 50+ (deliberately rare) |
```

(Re-derive these from the enums each run — the table above is a snapshot.)

### Step 6: Show flow summary (if `all`)

When listing all domains, include the cross-domain flow:

```
## Cross-Domain Event Flow

UserInput -> TempleRun (via Input2TempleRunAutoEventBridge):
  [list all mappings]

TempleRun -> GameFlow / GameFlow -> TempleRun (via TempleRunGameFlowBridge):
  [list both pair tables]

GameFlow -> Countdown (via CountdownGameFlowBridge, one-way):
  [list the pair table]

Countdown -> TempleRun (via Countdown2TempleRunBridge, one-way):
  [list the pair table]

TempleRun -> GameService (via TempleRunUGSBridge, one-way):
  [list the pair table]

GameFlow <-> GameService (via UGSGameFlowBridge):
  [list both pair tables, plus the hand-written ServicesStatusChanged handling]

GameService <-> UGS (via GameServiceEventsUGSBridge, ugs package):
  [list both pair tables, plus the hand-written status/balance handling]
```
