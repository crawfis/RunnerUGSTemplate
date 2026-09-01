---
name: audit-events
description: Audit the codebase for event system anti-patterns. Scans for missing OnDestroy unsubscriptions, direct coupling instead of events, unused events, potential circular auto-chains, and other violations of the event-driven architecture. Run this after adding new features.
allowed-tools: Read, Grep, Glob
argument-hint: [scope]
---

# Audit Events

Scan the codebase for violations of the event-driven architecture. This skill checks for anti-patterns and reports issues.

## Arguments

- `$ARGUMENTS` - Optional scope: `all` (default), `subscriptions`, `coupling`, `unused`, `circular`, or a specific file/folder path

## Audit Checks

### Check 1: Missing OnDestroy Unsubscriptions

Search for classes that call `.Subscribe(` (on a bus alias or an `EventId`), or a
dispatcher `.Attach()`, but do NOT have the matching `.Unsubscribe(` / `.Detach()` in
`OnDestroy()`.

**Pattern to find:**
```
Grep for "Bus.Subscribe(", ".Subscribe(", "Attach()" in *.cs files
For each file found, verify the matching Unsubscribe/Detach appears in an OnDestroy method
```

**Report format:**
```
MISSING UNSUBSCRIPTION:
  [File:Line] subscribes to [EventName] but never unsubscribes
  Fix: Add the matching Unsubscribe in OnDestroy()
```

### Check 2: Direct Coupling (bypassing events)

Search for direct method calls or references that should go through the event system:
- `GetComponent<>()` calls that reach across scene boundaries
- `FindObjectOfType<>()` for cross-system communication
- Direct singleton references like `GameController.Instance.StartGame()` (should be an event)
- `SendMessage()` or `BroadcastMessage()` calls

**Exclude from this check:**
- The `EventsFor<T>` bus aliases and `EventId` handles (these ARE the event system;
  `EventsPublisher.Instance` survives only inside `QuitController`'s editor-only diagnostics)
- `Blackboard.Instance` / `GameTime.Instance` (legitimate shared state)
- References within the same class or same scene

**Report format:**
```
DIRECT COUPLING:
  [File:Line] directly calls [TargetClass.Method] instead of publishing an event
  Suggestion: Publish [DomainEvents.SuggestedEvent] instead
```

### Check 3: Unused Events

For each event in all five enums, search if it is:
- Published anywhere (`Publish([EnumName].[EventName]`)
- Subscribed to anywhere (`Subscribe([EnumName].[EventName]`)
- Referenced in an auto-chain or bridge pair table

**Report format:**
```
UNUSED EVENT:
  [EnumName].[EventName] = [value]
  - Published: [yes/no, file locations]
  - Subscribed: [yes/no, file locations]
  - Auto-chained: [yes/no, source/target]
  - Bridge mapped: [yes/no, source/target]
```

### Check 4: Circular Auto-Chain Detection

Trace all auto-chain and bridge mappings to detect cycles:
1. Build a directed graph of all mappings from:
   - `GameFlowAutoEventFlow.cs`
   - `TempleRunAutoEventFlow.cs`
   - `UGSAutoEventFlow.cs` (ugs package)
   - `Input2TempleRunAutoEventBridge.cs`
   - `TempleRunGameFlowBridge.cs`
   - `Assets/UGSGlue/UGSGameFlowBridge.cs` and `Assets/UGSGlue/TempleRunUGSBridge.cs`
   - `GameServiceEventsUGSBridge.cs` (ugs package)
2. Run cycle detection on the graph
3. Report any cycles found

**Report format:**
```
CIRCULAR CHAIN DETECTED:
  [Event1] -> [Event2] -> [Event3] -> [Event1]
  Files involved: [list of auto-flow/bridge files]
```

### Check 5: Subscription/Publish Mismatch

Check for events that are published but never subscribed to (dead events) or subscribed to but never published (waiting forever).

**Report format:**
```
NEVER SUBSCRIBED (published but no listener):
  [EnumName].[EventName] published in [File] but no subscribers found

NEVER PUBLISHED (subscribed but never fires):
  [EnumName].[EventName] subscribed in [File] but never published
```

### Check 6: Domain Isolation Violations (Cross-Domain Event References)

Each domain's code may ONLY reference events from its own domain. Cross-domain event
references are ONLY permitted inside the five bridge files
(`Input2TempleRunAutoEventBridge.cs`, `TempleRunGameFlowBridge.cs`,
`Assets/UGSGlue/TempleRunUGSBridge.cs`, `Assets/UGSGlue/UGSGameFlowBridge.cs`, and
`GameServiceEventsUGSBridge.cs` in the ugs package).

Two boundaries are already compile-enforced by asmdefs: game assemblies cannot reference
`CrawfisSoftware.UGS` at all, so a game-side `UGS_EventsEnum.` reference will not even
build. The checks below cover what asmdefs cannot: within-domain discipline, and the
deliberately asmdef-free `Assets/UGSGlue/`.

**Scan for these violations:**

1. **TempleRun code referencing GameFlowEvents:**
   - Grep for `GameFlowEvents\.` in `Assets/TempleRun/**/*.cs`
   - Any match is a violation (TempleRun should only use `TempleRunEvents` and `UserInitiatedEvents`)

2. **GameFlow code referencing TempleRunEvents (outside bridges):**
   - Grep for `TempleRunEvents\.` in `Assets/GameFlow/**/*.cs`
   - Exclude `TempleRunGameFlowBridge.cs` — that file is allowed
   - Any other match is a violation

3. **Game code referencing the contract outside the glue:**
   - Grep for `GameServiceEvents\.` in `Assets/GameFlow/**/*.cs` and `Assets/TempleRun/**/*.cs`
   - Any match is a violation — the contract is named only in `Assets/UGSGlue/`
     (and inside the packages)

4. **UGSGlue discipline:**
   - `Assets/UGSGlue/*.cs` may name `GameFlowEvents`, `TempleRunEvents`, and
     `GameServiceEvents` — but NEVER `UGS_EventsEnum` (grep to confirm)
   - `TempleRunUGSBridge` must stay one-way: it publishes the contract, never TempleRun events

5. **Package boundary (informational):**
   - The ugs package may name `GameServiceEvents` only in `GameServiceEventsUGSBridge.cs`;
     nothing in it may name `GameFlowEvents` or `TempleRunEvents` (it cannot see them)

6. **Additional domains** (added via `/add-event-domain`): run the same check for each —
   the domain's enum name may appear outside its own `Assets/<Domain>/` folder ONLY in
   bridge files. The authoritative domain list is the set of `[EventEnum]`-marked enums
   (`CrawfisSoftware > Events > List Domains`).

7. **Registry drift:** compare the `[EventEnum]` enums List Domains reports against the
   Domain Registry table in `CLAUDE.md` (Architecture Overview). Flag any domain missing
   from the table, or any table row with no matching enum.

**Report format:**
```
DOMAIN ISOLATION VIOLATION:
  [File:Line] references [ForeignDomain]Events from [CurrentDomain] code
  Fix: Add bridge mapping in [BridgeFile] and subscribe to a local domain event instead
```

## Output Summary

At the end, provide a summary:
```
Event System Audit Results:
  Missing unsubscriptions: [count]
  Direct coupling violations: [count]
  Unused events: [count]
  Circular chains: [count]
  Publish/subscribe mismatches: [count]
  Domain isolation violations: [count]
  Registry drift: [count]

  Total issues: [count]
  Severity: [CLEAN / WARNINGS / CRITICAL]
```

## Files to Scan

- Event enums: `Assets/GameFlow/Scripts/Events/GameFlowEvents.cs`, `Assets/TempleRun/Scripts/Events/TempleRunEvents.cs`, `Assets/TempleRun/Scripts/Events/UserInitiatedEvents.cs`; and from packages, `Runtime/GameServiceEvents.cs` (contracts) and `Runtime/Events/UGS_EventsEnum.cs` (ugs)
- Auto-flows: `Assets/GameFlow/Scripts/Events/GameFlowAutoEventFlow.cs`, `Assets/TempleRun/Scripts/Events/TempleRunAutoEventFlow.cs`, and `Runtime/Events/UGSAutoEventFlow.cs` (ugs package)
- Bridges: `Assets/TempleRun/Scripts/Events/Input2TempleRunAutoEventBridge.cs`, `Assets/GameFlow/Scripts/TempleRunSpecific/TempleRunGameFlowBridge.cs`, `Assets/UGSGlue/UGSGameFlowBridge.cs`, `Assets/UGSGlue/TempleRunUGSBridge.cs`, and `Runtime/Events/GameServiceEventsUGSBridge.cs` (ugs package)
- Any additional `[EventEnum]` enums, `*AutoEventFlow` classes, and `*Bridge` classes from domains added later
- All C# scripts: `Assets/**/*.cs`
