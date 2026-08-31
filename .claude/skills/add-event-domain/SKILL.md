---
name: add-event-domain
description: Create a new event domain — a new [EventEnum] enum, an optional auto-flow, and a bridge — for a genuinely separate bounded context (a new backend, analytics, netcode). Rare; includes the decision gate, where the domain should live, and the registration checklist for docs and skills.
allowed-tools: Read, Write, Edit, Grep, Glob
argument-hint: <DomainName> [purpose]
---

# Add Event Domain

Stand up a new event domain alongside `GameFlowEvents`, `TempleRunEvents`,
`UserInitiatedEvents`, and `UGS_EventsEnum`. This is rare and structural — most features
are a new *category* inside an existing enum, not a new domain.

## Arguments

- `$ARGUMENTS` — the domain name (PascalCase, e.g. `Analytics`) and optionally its purpose.

## Step 0: Decision gate — do you actually need a domain?

A domain is a bounded context with its own enum, its own publisher, and an isolation
boundary other code may cross only through a bridge. Create one ONLY if all three hold:

1. **Separate concern with its own lifecycle** — not app flow (GameFlow), not gameplay
   (TempleRun), not raw input (UserInitiated), not Unity Gaming Services (UGS).
2. **The rest of the game must stay decoupled from it** — you want to add, remove, or swap
   it without touching other domains' code. The bridge is what buys that. The operational
   test: could a trivial **stub** — same events consumed and published, same data shapes,
   nothing real behind them — sit in its place and keep the game running? This repo's UGS
   domain is the worked proof at full scale: the `Test_GameOnly_Windows` build profile
   runs the entire game with the UGS domain absent.
3. **It will grow a family of events** — several lifecycle groups, not one or two events.

A strong **capture-point purpose** reinforces criterion 2: a stream worth logging and
replaying as a unit is itself a reason the rest of the game must stay decoupled from it
(timestamped `UserInitiatedEvents` plus the run's random seed reconstruct a playthrough).

If any of these fail → STOP and use `/add-event` instead: a new mechanic, panel, or
service call is a category in an existing enum.

## Procedure

### Step 1: Choose names and placement

| Piece | Convention | Example (`Analytics`) |
|-------|------------|----------------------|
| Enum | `<Name>Events` | `AnalyticsEvents` |
| Enum file | `Assets/<Name>/Scripts/Events/<Name>Events.cs` | `Assets/Analytics/Scripts/Events/AnalyticsEvents.cs` |
| Bus alias | `<Name>Bus`, aliased per file from `EventsFor<T>` | `AnalyticsBus` |
| Auto-flow (optional) | `<Name>AutoEventFlow`, next to the enum | `AnalyticsAutoEventFlow` |
| Bridge | `<Name>GameFlowBridge` (or whichever pair applies) | `AnalyticsGameFlowBridge` |

### Step 2: Create the enum

Explicit values, categories separated by `// ---------- Name ----------` comments with
gaps of ~10 between categories; naming `*Requested` / `*Starting`/`*ing` /
`*Started`/`*ed` / `*Failed` / `*Cancelled`.

### Step 3: Mark the enum `[EventEnum]`

There is **no publisher to create**. The `EventsPublisher*` singletons this step used to
describe are retired: the buses are static generics, `EventsFor<TEnum>`, so a domain needs
no MonoBehaviour, no `[DefaultExecutionOrder]` and no scene object.

```csharp
[EventEnum]
public enum AnalyticsEvents { ... }
```

`[EventEnum]` is what makes `CrawfisSoftware > Events > List Domains` find it in edit mode.
Alias the bus per file, the way every other domain does:

```csharp
using AnalyticsBus = CrawfisSoftware.Events.EventsFor<CrawfisSoftware.Analytics.AnalyticsEvents>;
```

### Step 4: Decide where the domain lives

Nothing needs hosting, so the only placement question left is which side of the fence the
code sits on:

| If the domain… | Put it in |
|---|---|
| is specific to this game | `Assets/<Domain>/Scripts/Events/` |
| is a backing service the game should be able to run without | its own UPM package, talking to the game only through `GameSignals` — the way `com.crawfissoftware.ugs` does |

If you pick the second, the game must not reference the domain's enum at all. Add the
crossing to `GameSignals` in `com.crawfissoftware.contracts` and bridge it on both sides —
`Assets/UGSGlue/` is the worked example.

### Step 5 (optional): Auto-flow class

Follow the pattern of `UGSAutoEventFlow` (dictionary of same-domain mappings +
subscribe-to-all dispatch). Skip until the domain actually has same-domain progressions.

### Step 6: Bridge class — required as soon as the domain talks to another

Follow `UGSGameFlowBridge.cs`: direction dictionaries plus `SubscribeToAllEnumEvents` on
both publishers. **Never reference the new domain's enum from another domain's code** —
the bridge is the only crossing point. Precedent for shortcuts: `TempleRunGameFlowBridge`
carries a third, passthrough dictionary (TempleRun → UGS) to avoid relaying through
GameFlow — acceptable, but it lives in a bridge file. Add mappings with
`/add-bridge-mapping`, and add the new bridge to that skill's "Available Bridges" table.

### Step 7: Register the domain everywhere the current four are listed

This is the step people forget. Update every place that enumerates domains:

- `CLAUDE.md` — the Domain Registry table, Domain Isolation table, Namespaces block,
  Key Files table, folder tree
- Skills — `list-events`, `add-event`, `add-auto-chain` (their domain/file tables),
  `add-bridge-mapping` (Available Bridges), `audit-events` (isolation + registry checks)
- Pointer files — `GEMINI.md` and `.github/copilot-instructions.md` (the domain-isolation
  bullet; keep the two mirrored)
- `README.md`'s architecture section

### Step 8: Verify

Run `/audit-events` — the isolation check generalizes: the new enum's name may appear
outside its own `Assets/<Name>/` folder ONLY in bridge files. Then play a session with
event logging enabled and inspect the trace.

### Step 9: Summarize

```
Created domain: [Name]
  Enum:      Assets/[Name]/Scripts/Events/[Name]Events.cs  ([N] events)
  Publisher: EventsPublisher[Name]  (hosted in: [scenes])
  Flow:      [path, or "none yet — no same-domain progressions"]
  Bridge:    [path]  ([n] mappings [Name]->X, [m] mappings X->[Name])
  Registered in: CLAUDE.md, 6 skills, 2 pointer files, README.md
```
