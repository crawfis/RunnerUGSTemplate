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

Search for classes that call `SubscribeToEvent` or `SubscribeToAllEnumEvents` but do NOT have a corresponding `UnsubscribeToEvent` or `UnsubscribeToAllEnumEvents` in `OnDestroy()`.

**Pattern to find:**
```
Grep for SubscribeToEvent in *.cs files
For each file found, verify it also contains UnsubscribeToEvent in an OnDestroy method
```

**Report format:**
```
MISSING UNSUBSCRIPTION:
  [File:Line] subscribes to [EventName] but never unsubscribes
  Fix: Add UnsubscribeToEvent in OnDestroy()
```

### Check 2: Direct Coupling (bypassing events)

Search for direct method calls or references that should go through the event system:
- `GetComponent<>()` calls that reach across scene boundaries
- `FindObjectOfType<>()` for cross-system communication
- Direct singleton references like `GameController.Instance.StartGame()` (should be an event)
- `SendMessage()` or `BroadcastMessage()` calls

**Exclude from this check:**
- `EventsPublisher*.Instance` references (these ARE the event system)
- `Blackboard.Instance` (legitimate shared state)
- References within the same class or same scene

**Report format:**
```
DIRECT COUPLING:
  [File:Line] directly calls [TargetClass.Method] instead of publishing an event
  Suggestion: Publish [DomainEvents.SuggestedEvent] instead
```

### Check 3: Unused Events

For each event in all four enums, search if it is:
- Published anywhere (`PublishEvent([EnumName].[EventName]`)
- Subscribed to anywhere (`SubscribeToEvent([EnumName].[EventName]`)
- Referenced in an auto-chain or bridge mapping

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
   - `UGSAutoEventFlow.cs`
   - `TempleRunGameFlowBridge.cs`
   - `UGSGameFlowBridge.cs`
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

Each domain's code may ONLY reference events from its own domain. Cross-domain event references are ONLY permitted inside bridge files (`TempleRunGameFlowBridge.cs`, `UGSGameFlowBridge.cs`).

**Scan for these violations:**

1. **TempleRun code referencing GameFlowEvents:**
   - Grep for `GameFlowEvents\.` in `Assets/TempleRun/**/*.cs`
   - Any match is a violation (TempleRun should only use `TempleRunEvents` and `UserInitiatedEvents`)

2. **GameFlow code referencing TempleRunEvents (outside bridges):**
   - Grep for `TempleRunEvents\.` in `Assets/GameFlow/**/*.cs`
   - Exclude `TempleRunGameFlowBridge.cs` — that file is allowed
   - Any other match is a violation

3. **GameFlow code referencing UGS_EventsEnum:**
   - Grep for `UGS_EventsEnum\.` in `Assets/GameFlow/**/*.cs`
   - Exclude `TempleRunGameFlowBridge.cs` — it legitimately holds the TempleRun -> UGS
     passthrough dictionary (see `/add-bridge-mapping`)
   - Any other match is a violation (UGS <-> GameFlow bridging lives in `UGSGameFlowBridge.cs` under `Assets/UGS/`)

4. **UGS code referencing GameFlowEvents (outside bridges):**
   - Grep for `GameFlowEvents\.` in `Assets/UGS/**/*.cs`
   - Exclude `UGSGameFlowBridge.cs` — that file is allowed
   - Any other match is a violation

5. **Any code referencing TempleRunEvents in UGS or UGS_EventsEnum in TempleRun:**
   - Grep for `TempleRunEvents\.` in `Assets/UGS/**/*.cs`
   - Grep for `UGS_EventsEnum\.` in `Assets/TempleRun/**/*.cs`
   - Any match is a violation (TempleRun -> UGS crossings are allowed only via the
     passthrough dictionary in `TempleRunGameFlowBridge`, which lives under GameFlow)

6. **Additional domains** (added via `/add-event-domain`): run the same check for each —
   the domain's enum name may appear outside its own `Assets/<Domain>/` folder ONLY in
   bridge files. The authoritative domain list is the set of `EventsPublisher*` singleton
   subclasses.

7. **Registry drift:** compare the `EventsPublisher*` subclasses found in `Assets/`
   against the Domain Registry table in `CLAUDE.md` (Architecture Overview). Flag any
   domain missing from the table, or any table row with no matching publisher.

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

- Event enums: `Assets/GameFlow/Scripts/Events/GameFlowEvents.cs`, `Assets/TempleRun/Scripts/Events/TempleRunEvents.cs`, `Assets/TempleRun/Scripts/Events/UserInitiatedEvents.cs`, `Assets/UGS/Scripts/Events/UGS_EventsEnum.cs`
- Auto-flows: `Assets/GameFlow/Scripts/Events/GameFlowAutoEventFlow.cs`, `Assets/TempleRun/Scripts/Events/TempleRunAutoEventFlow.cs`, `Assets/UGS/Scripts/Events/UGSAutoEventFlow.cs`
- Bridges: `Assets/GameFlow/Scripts/TempleRunSpecific/TempleRunGameFlowBridge.cs`, `Assets/UGS/Scripts/Events/UGSGameFlowBridge.cs`
- Any additional `EventsPublisher*` subclasses, `*AutoEventFlow` classes, and `*Bridge` classes from domains added later
- All C# scripts: `Assets/**/*.cs`
