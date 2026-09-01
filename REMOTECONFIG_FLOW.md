# RemoteConfig Flow Documentation

> **Status note (2026-08-31).** The UGS domain is no longer in this repository — it ships as
> the `com.crawfissoftware.ugs` package, so the paths below read `Runtime/...` rather than
> `Assets/UGS/Scripts/...`, and the buses are static `EventsFor<T>` aliases rather than
> `EventsPublisher*` singletons. The cross-domain hop also goes through `GameServiceEvents` now, not
> directly between UGS and GameFlow.
>
> The publisher changed. `DifficultyObserver` is gone: nothing ever constructed it, so
> `DifficultySettingsFetched` was never published and this whole flow was design rather than
> behaviour. `RemoteConfigManager` now publishes it from the response it already fetches, which
> also removes a second Remote Config round trip for a payload the first one had downloaded.
>
> Two things still gate it at runtime, and neither is code: the active environment needs a
> `difficulty_settings` key, and a missing key is deliberately silent - the game simply keeps the
> local table `LoadDefaultGameConfigs` supplies.

## Overview

The RunnerUGS RemoteConfig system uses an **event-driven architecture** to decouple the UGS domain from gameplay domains. Difficulty settings are fetched from Unity Gaming Services RemoteConfig and propagated through the event system to TempleRun gameplay logic.

**Key Principle:** All cross-domain communication flows through **bridges** that translate events between domains. No domain directly accesses another domain's code or state.

---

## Architecture Diagram

```
┌────────────────────────────────────────────────────────────────────────────┐
│                          REMOTECONFIG DATA FLOW                             │
└────────────────────────────────────────────────────────────────────────────┘

PHASE 1: USER AUTHENTICATION
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    ┌─────────────────────────────────────┐
    │ PlayerAuthenticationManager         │
    │ (Runtime/Initialization/, ugs pkg)            │
    │                                     │
    │ Authenticates player with UGS       │
    └─────────────────────────────────────┘
                    │
                    ▼
    Publishes: UGS_EventsEnum.PlayerAuthenticated
    (via UGSBus)


PHASE 2: REMOTE CONFIG FETCH (PARALLEL WITH AUTH)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    ┌─────────────────────────────────────┐
    │ RemoteConfigManager                 │
    │ (Runtime/RemoteConfig/, ugs pkg)  │
    │                                     │
    │ 1. Fetches from RemoteConfig API    │
    │ 2. Publishes RemoteConfigFetched    │
    │ 3. Reads the difficulty_settings key│
    │    off the response it already has  │
    │ 4. Deserializes to:                 │
    │    List<DifficultyConfig>           │
    │    (from CrawfisSoftware.Config)    │
    │ 5. Absent key -> silence, not error │
    └─────────────────────────────────────┘
                    │
                    ▼
    Publishes: UGS_EventsEnum.DifficultySettingsFetched
    (via UGSBus)
    Data: List<DifficultyConfig>


PHASE 3: UGS → CONTRACT → GAMEFLOW (two bridges)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    ┌─────────────────────────────────────┐
    │ GameServiceEventsUGSBridge          │
    │ (Runtime/Events/, ugs package)      │
    │                                     │
    │ Pair table:                         │
    │ { DifficultySettingsFetched →       │
    │   GameServiceEvents.                │
    │     DifficultySettingsAvailable }   │
    └─────────────────────────────────────┘
                    │
                    ▼
    Publishes: GameServiceEvents.DifficultySettingsAvailable
    (via GameServiceBus — the contract; neither side owns it)
    Data: List<DifficultyConfig>, passed through unchanged

    ┌─────────────────────────────────────┐
    │ UGSGameFlowBridge                   │
    │ (Assets/UGSGlue/)                   │
    │                                     │
    │ Pair table:                         │
    │ { DifficultySettingsAvailable →     │
    │   GameFlowEvents.                   │
    │     DifficultySettingsApplied }     │
    └─────────────────────────────────────┘
                    │
                    ▼
    Publishes: GameFlowEvents.DifficultySettingsApplied  (STICKY)
    (via GameFlowBus)
    Data: List<DifficultyConfig>


PHASE 4: GAMEFLOW → TEMPLERUN BRIDGE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    ┌─────────────────────────────────────┐
    │ TempleRunGameFlowBridge             │
    │ (Assets/GameFlow/Scripts/           │
    │  TempleRunSpecific/)                │
    │                                     │
    │ Pair table:                         │
    │ { DifficultySettingsApplied →       │
    │   TempleRunEvents.                  │
    │     DifficultySettingsApplied }     │
    │                                     │
    │ Translates GameFlow events to       │
    │ TempleRun events                    │
    └─────────────────────────────────────┘
                    │
                    ▼
    Publishes: TempleRunEvents.DifficultySettingsApplied
    (via TempleRunBus)
    Data: List<DifficultyConfig>


PHASE 5: TEMPLERUN APPLIES SETTINGS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    ┌─────────────────────────────────────┐
    │ GameDifficultyManager               │
    │ (Assets/TempleRun/Scripts/          │
    │  Config/)                           │
    │                                     │
    │ OnDifficultySettingsChanged():      │
    │ 1. Receives List<DifficultyConfig>  │
    │ 2. Iterates through configs         │
    │ 3. Stores in _difficultyConfigs     │
    │    Dictionary                       │
    │ 4. Makes available to gameplay      │
    │    logic                            │
    │ 5. Blackboard can access            │
    │    CurrentDifficultyConfig           │
    └─────────────────────────────────────┘
                    │
                    ▼
    Gameplay is ready with RemoteConfig
    difficulty settings applied

```

---

## Detailed Component Breakdown

### Phase 1: Authentication
**Component:** `PlayerAuthenticationManager`
**Location:** `Runtime/Initialization/` (ugs package)
**Responsibility:** Sign in player with UGS
**Event Published:**
- `UGS_EventsEnum.PlayerAuthenticated` (data: null)

**Code Flow:**
```csharp
// Player successfully authenticates
UGSBus.Publish(
    UGS_EventsEnum.PlayerAuthenticated,
    this,
    null
);
```

---

### Phase 2: RemoteConfig Fetch
**Component:** `RemoteConfigManager`
**Location:** `Runtime/RemoteConfig/` (ugs package)
**Responsibility:** Fetch RemoteConfig once, and announce the difficulty table it carries
**Event Published:**
- `UGS_EventsEnum.DifficultySettingsFetched` (data: `List<DifficultyConfig>`)

**Key Changes from Coupling Remediation:**
- ✅ Uses the shared `DifficultyConfig` from `CrawfisSoftware.Config`, not TempleRun's
- ✅ No direct coupling to TempleRun domain
- ✅ One fetch, not two: the table comes out of the response the manager already awaited

**Code Flow:**
```csharp
// In RemoteConfigManager.cs - called from ApplyRemoteConfig(), after a successful fetch
private void PublishDifficultySettings()
{
    RuntimeConfig appConfig = RemoteConfigService.Instance.appConfig;
    string key = RemoteConfigConstants.difficultySettingsKey;   // "difficulty_settings"

    // Absent is not a failure: the game ships its own configs and only lets the
    // environment override them, so silence leaves those local defaults standing.
    if (!appConfig.HasKey(key)) return;

    // config[key] is the raw JSON token, not one of the typed getters: the value is an
    // array of objects, which RuntimeConfig has no accessor for.
    List<DifficultyConfig> difficulties = appConfig.config[key]?.ToObject<List<DifficultyConfig>>();
    if (difficulties == null || difficulties.Count == 0) return;

    UGSBus.Publish(UGS_EventsEnum.DifficultySettingsFetched, this, difficulties);
}
```

(The old `GetDefinitions()` second fetch — and the `DifficultyObserver` that called it — were
removed in ugs 0.5.0; the manager reads the table off the response it already has.)

**RemoteConfig Schema:**
The RemoteConfig service stores difficulty settings under key `"difficulty_settings"`
(`RemoteConfigConstants.difficultySettingsKey`) as a JSON array:
```json
{
  "difficulty_settings": [
    {
      "DifficultyName": "Easy",
      "InitialSpeed": 5,
      "MaxSpeed": 40,
      // ... other DifficultyConfig fields
    },
    {
      "DifficultyName": "Medium",
      "InitialSpeed": 5,
      "MaxSpeed": 80,
      // ... other DifficultyConfig fields
    }
  ]
}
```

---

### Phase 3: UGS → Contract → GameFlow (two bridges)

The UGS domain and the game no longer name each other's events. The hop goes through
`GameServiceEvents` (the contracts package), crossed by one bridge on each side:

**`GameServiceEventsUGSBridge`** — `Runtime/Events/` in the ugs package (read-only here)
- **Receives:** `UGS_EventsEnum.DifficultySettingsFetched`
- **Publishes:** `GameServiceEvents.DifficultySettingsAvailable`

**`UGSGameFlowBridge`** — `Assets/UGSGlue/` (this game's half of the seam)
- **Receives:** `GameServiceEvents.DifficultySettingsAvailable`
- **Publishes:** `GameFlowEvents.DifficultySettingsApplied` *(Sticky)*

Both are declarative pair tables on the shared `EventChainDispatcher` (common package) —
no dictionaries, no hand subscriptions for this mapping, payload forwarded unchanged:

```csharp
// GameServiceEventsUGSBridge (ugs package)
private static readonly (UGS_EventsEnum From, GameServiceEvents To)[] UGSToGameService =
{
    (UGS_EventsEnum.RemoteConfigUpdated, GameServiceEvents.RemoteConfigApplied),
    (UGS_EventsEnum.DifficultySettingsFetched, GameServiceEvents.DifficultySettingsAvailable),
};

// UGSGameFlowBridge (Assets/UGSGlue)
private static readonly (GameServiceEvents From, GameFlowEvents To)[] GameServiceToGameFlow =
{
    (GameServiceEvents.RemoteConfigApplied, GameFlowEvents.LoadingScreenHideRequested),
    (GameServiceEvents.DifficultySettingsAvailable, GameFlowEvents.DifficultySettingsApplied),
    (GameServiceEvents.CurrencyBalanceChanged, GameFlowEvents.CurrencyBalanceChanged),
};
```

**Why two bridges?**
- UGS publishes UGS-specific events; the game publishes game-specific ones
- The contract enum is owned by neither, so either side can be replaced without editing the other
- Event data (the `DifficultyConfig` list) passes through unchanged

### Phase 4: GameFlow → TempleRun Bridge
**Component:** `TempleRunGameFlowBridge`
**Location:** `Assets/GameFlow/Scripts/TempleRunSpecific/`
**Responsibility:** Translate GameFlow events to TempleRun events
**Events:**
- **Receives:** `GameFlowEvents.DifficultySettingsApplied`
- **Publishes:** `TempleRunEvents.DifficultySettingsApplied`

**Code Flow:**
```csharp
// In TempleRunGameFlowBridge.cs — one entry in the GameFlowToTempleRun pair table
private static readonly (GameFlowEvents From, TempleRunEvents To)[] GameFlowToTempleRun =
{
    // ...
    (GameFlowEvents.DifficultySettingsApplied, TempleRunEvents.DifficultySettingsApplied),
};
```

**Why This Bridge?**
- GameFlow doesn't directly publish TempleRun events
- TempleRun domain remains isolated from GameFlow
- Clean translation layer ensures architectural integrity
- Data remains unchanged as it crosses domain boundaries

---

### Phase 5: Apply Settings in TempleRun
**Component:** `GameDifficultyManager`
**Location:** `Assets/TempleRun/Scripts/Config/`
**Responsibility:** Store and manage difficulty configurations for gameplay
**Events:**
- **Receives:** `TempleRunEvents.DifficultySettingsApplied` (the REMOTE table, Sticky),
  `TempleRunEvents.TempleRunDifficultySettingsApplied` (the local table), and
  `TempleRunEvents.TempleRunDifficultyChangeRequested`
- **Publishes:** `TempleRunEvents.TempleRunDifficultyChanging` (data: the chosen
  `DifficultyConfig`) or `TempleRunEvents.DifficultyChangeFailed`

**Key Changes from Coupling Remediation:**
- ✅ Moved from GameFlow to TempleRun domain
- ✅ Now subscribes to `TempleRunEvents` only
- ✅ Uses `TempleRunBus` exclusively
- ✅ No cross-domain event subscriptions

**Code Flow:**
```csharp
// In GameDifficultyManager.cs (TempleRun domain)
private void Awake()
{
    TempleRunBus.Subscribe(TempleRunEvents.TempleRunDifficultyChangeRequested, OnDifficultyChanging);
    TempleRunBus.Subscribe(TempleRunEvents.TempleRunDifficultySettingsApplied, OnDifficultySettingsChanged);
    TempleRunBus.Subscribe(TempleRunEvents.DifficultySettingsApplied, OnRemoteDifficultySettingsApplied);
}

public void OnDifficultySettingsChanged(
    string eventName,
    object sender,
    object data)
{
    var difficultyConfigs = data as IList<DifficultyConfig>;
    if (difficultyConfigs == null)
    {
        throw new ArgumentException(
            "OnDifficultySettingsChanged event data must be of type IList<DifficultyConfig>");
    }

    // Store configurations
    PopulateDifficulties(difficultyConfigs);
}

public void PopulateDifficulties(IList<DifficultyConfig> difficulties)
{
    Clear();
    foreach (var config in difficulties)
    {
        AddConfig(config);
    }
}

public void AddConfig(DifficultyConfig difficultyConfig)
{
    // Store in dictionary by difficulty name
    _difficultyConfigs[difficultyConfig.DifficultyName] = difficultyConfig;
}
```

**Access During Gameplay:**
```csharp
// Gameplay systems can now access difficulty settings
public string CurrentDifficulty { get; private set; } = "Easy";

public DifficultyConfig CurrentDifficultyConfig
{
    get
    {
        if (_difficultyConfigs.ContainsKey(CurrentDifficulty))
        {
            return _difficultyConfigs[CurrentDifficulty];
        }
        return null;
    }
}

// Used by systems like TrackManager, ObstacleSpawner, etc.
var config = GameDifficultyManager.Instance.CurrentDifficultyConfig;
float maxSpeed = config.MaxSpeed;
```

---

## Event Types & Values

### UGS Events
**File:** `Runtime/Events/UGS_EventsEnum.cs` (ugs package)

```csharp
public enum UGS_EventsEnum
{
    // Related to RemoteConfig
    DifficultySettingsFetched,  // Published when RemoteConfig is fetched
    // ... other events
}
```

### GameFlow Events
**File:** `Assets/GameFlow/Scripts/Events/GameFlowEvents.cs`

```csharp
public enum GameFlowEvents
{
    // Related to RemoteConfig/Difficulty
    DifficultyChangeRequested = 90,
    DifficultyChanging = 91,
    DifficultyChanged = 92,
    DifficultyChangeFailed = 93,
    [EventDelivery(EventDelivery.Sticky)]
    DifficultySettingsApplied = 94,   // Bridged from the contract
    // ... other events
}
```

### TempleRun Events
**File:** `Assets/TempleRun/Scripts/Events/TempleRunEvents.cs`

```csharp
public enum TempleRunEvents
{
    // Related to RemoteConfig/Difficulty
    [EventDelivery(EventDelivery.Sticky)]
    DifficultySettingsApplied = 320,  // Bridged from GameFlow; the REMOTE table
    DifficultyChanging = 321,
    DifficultyChanged = 322,
    DifficultyChangeFailed = 323,
    // ... other events
}
```

---

## Data Flow Summary

| Step | Component | Event Published | Data Type | Destination |
|------|-----------|-----------------|-----------|-------------|
| 1 | PlayerAuthenticationManager | PlayerAuthenticated | null | UGSBus |
| 2 | RemoteConfigManager | DifficultySettingsFetched | List<DifficultyConfig> | UGSBus |
| 3 | GameServiceEventsUGSBridge | DifficultySettingsAvailable | List<DifficultyConfig> | GameServiceBus |
| 4 | UGSGameFlowBridge | DifficultySettingsApplied **(Sticky)** | List<DifficultyConfig> | GameFlowBus |
| 5 | TempleRunGameFlowBridge | DifficultySettingsApplied **(Sticky)** | List<DifficultyConfig> | TempleRunBus |
| 6 | GameDifficultyManager | (stores them, and latches so the local table cannot overwrite them) | Dictionary<string, DifficultyConfig> | Local storage |

Steps 4 and 5 are declared `[EventDelivery(EventDelivery.Sticky)]`, and the flow does not work
without it. Step 2 happens during boot, while `TempleRunGameFlowBridge` lives in `Game_Boot_2_Play`
and `GameDifficultyManager` in a gameplay scene, so both subscribe long after the publish. A
retained event is delivered on subscribe; a transient one would reach neither of them, ever.

---

## Key Architectural Principles

### 1. **Domain Isolation**
- Each domain (UGS, GameFlow, TempleRun) only subscribes to events from its own event system
- Cross-domain communication ONLY through bridges
- No direct method calls or singleton access across domains

### 2. **Shared Configuration**
- `DifficultyConfig` lives in `Runtime/Config/` in the `com.crawfissoftware.common` package,
  namespace `CrawfisSoftware.Config` (neutral domain, owned by no game or service)
- All domains can reference this shared type without creating coupling
- Before remediation: UGS imported from TempleRun (❌ coupled)
- After remediation: UGS imports from _Common (✅ decoupled)

### 3. **Event-Driven Data Flow**
- RemoteConfig data flows as event payloads
- No direct polling or state access
- Each domain receives configuration via subscribed events

### 4. **Bridge Pattern**
- Declarative pair tables (`EventChainDispatcher`, common package) translate between event systems
- Auto-fire mechanism automatically translates events
- Data payload passes through unchanged
- No domain logic in bridges

---

## Example Scenario: Difficulty Change During Gameplay

```
Player selects "Hard" difficulty

    1. A config UI (SetGameDifficulty) publishes:
       TempleRunEvents.TempleRunDifficultyChangeRequested
       (data: "Hard")

    2. GameDifficultyManager.OnDifficultyChanging() looks "Hard" up in its
       table — the remote table if one arrived and latched, else the local one

    3. GameDifficultyManager publishes:
       TempleRunEvents.TempleRunDifficultyChanging
       (data: DifficultyConfig for Hard)
       — or TempleRunEvents.DifficultyChangeFailed if the name is unknown

    4. Blackboard stores it:
       GameConfig = (received DifficultyConfig)

    5. Gameplay adjusts based on new difficulty:
       - ObstacleSpawner uses GameConfig.ObstacleSpawnRate
       - TrackManager uses GameConfig.MaxTrackLength
       - Player movement uses GameConfig.MaxSpeed
       - etc.
```

---

## No Longer Problematic: Before vs After

### ❌ BEFORE (Coupled)
```
RemoteConfig (UGS)
    ↓
DifficultyObserver (UGS)
    └─ using CrawfisSoftware.TempleRun.GameConfig;
    └─ uses DifficultyConfig from TempleRun (❌ cross-domain type import)
    ↓
GameDifficultyManager (was in GameFlow folder)
    └─ subscribes to GameFlowEvents directly (❌ cross-domain event subscription)
    └─ publishes GameFlowEvents (❌ wrong domain)
```

### ✅ AFTER (Event-Driven)
```
RemoteConfig (UGS)
    ↓
RemoteConfigManager (UGS)
    └─ using CrawfisSoftware.Config;  ✅ shared namespace
    └─ uses DifficultyConfig from the common package (✅ no cross-domain coupling)
    └─ publishes UGS_EventsEnum.DifficultySettingsFetched ✅
    ↓
GameServiceEventsUGSBridge (UGS package)
    └─ translates to GameServiceEvents.DifficultySettingsAvailable ✅ (the contract; neither side owns it)
    ↓
UGSGameFlowBridge (Assets/UGSGlue)
    └─ translates to GameFlowEvents.DifficultySettingsApplied ✅
    ↓
TempleRunGameFlowBridge
    └─ translates to TempleRunEvents.DifficultySettingsApplied ✅
    ↓
GameDifficultyManager (now in TempleRun domain)
    └─ subscribes to TempleRunEvents only ✅
    └─ publishes TempleRunEvents only ✅
```

---

## Files Involved

| File | Purpose |
|------|---------|
| `Runtime/RemoteConfig/RemoteConfigManager.cs` (ugs package) | Fetch RemoteConfig, publish DifficultySettingsFetched |
| `Runtime/Events/GameServiceEventsUGSBridge.cs` (ugs package) | Bridge UGS ↔ the GameServiceEvents contract |
| `Assets/UGSGlue/UGSGameFlowBridge.cs` | Bridge the contract ↔ GameFlow events |
| `Assets/GameFlow/Scripts/TempleRunSpecific/TempleRunGameFlowBridge.cs` | Bridge GameFlow → TempleRun events |
| `Assets/TempleRun/Scripts/Config/GameDifficultyManager.cs` | Apply difficulty settings in TempleRun domain |
| `Runtime/Config/DifficultyConfig.cs` (common package) | Shared data type for all domains |
| `Runtime/Events/UGS_EventsEnum.cs` (ugs package) | UGS event definitions |
| `Assets/GameFlow/Scripts/Events/GameFlowEvents.cs` | GameFlow event definitions |
| `Assets/TempleRun/Scripts/Events/TempleRunEvents.cs` | TempleRun event definitions |

---

## Summary

The RemoteConfig system demonstrates the **event-driven architecture** principles of RunnerUGS:

1. **Domains are isolated** - Each domain only uses its own events
2. **Data flows through events** - RemoteConfig difficulty settings propagate as event payloads
3. **Bridges translate between domains** - GameServiceEventsUGSBridge, UGSGameFlowBridge, and TempleRunGameFlowBridge enable cross-domain communication
4. **Shared types don't create coupling** - DifficultyConfig in _Common is referenced by all domains without coupling
5. **No direct access** - UGS never directly accesses TempleRun code or state

This architecture ensures clean separation of concerns, testability, and maintainability across the complex multi-domain codebase.
