# RemoteConfig Flow Documentation

> **Status note (2026-08-31).** The UGS domain is no longer in this repository — it ships as
> the `com.crawfissoftware.ugs` package, so the paths below read `Runtime/...` rather than
> `Assets/UGS/Scripts/...`, and the buses are static `EventsFor<T>` aliases rather than
> `EventsPublisher*` singletons. The cross-domain hop also goes through `GameSignals` now, not
> directly between UGS and GameFlow.
>
> One caveat this document does not yet reflect: `DifficultyObserver` is constructed by
> nothing in the package, so `GameSignals.DifficultySettingsAvailable` is not published today.
> The flow described here is the design, not fully live behaviour.

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
    │ (Runtime/  [ugs package]...)            │
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
    │ DifficultyObserver                  │
    │ (Runtime/RemoteConfig/  [ugs package])  │
    │                                     │
    │ 1. Detects player signed in         │
    │ 2. Calls GetDifficultySettingsAsync()│
    │ 3. Fetches from RemoteConfig API    │
    │ 4. Deserializes to:                 │
    │    List<DifficultyConfig>           │
    │    (from CrawfisSoftware.Config)    │
    │ 5. Stores in RuntimeDifficultySettings
    └─────────────────────────────────────┘
                    │
                    ▼
    Publishes: UGS_EventsEnum.DifficultySettingsFetched
    (via UGSBus)
    Data: List<DifficultyConfig>


PHASE 3: UGS → GAMEFLOW BRIDGE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    ┌─────────────────────────────────────┐
    │ UGSGameFlowBridge                   │
    │ (Runtime/Events/  [ugs package])        │
    │                                     │
    │ Line 32:                            │
    │ { DifficultySettingsFetched →       │
    │   GameFlowEvents.                   │
    │     DifficultySettingsApplied }     │
    │                                     │
    │ Translates UGS events to            │
    │ GameFlow events                     │
    └─────────────────────────────────────┘
                    │
                    ▼
    Publishes: GameFlowEvents.DifficultySettingsApplied
    (via GameFlowBus)
    Data: List<DifficultyConfig>


PHASE 4: GAMEFLOW → TEMPLERUN BRIDGE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    ┌─────────────────────────────────────┐
    │ TempleRunGameFlowBridge             │
    │ (Assets/GameFlow/Scripts/           │
    │  TempleRunSpecific/)                │
    │                                     │
    │ Line 52:                            │
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
**Location:** `Runtime/  [ugs package]Initialization/`
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
**Component:** `DifficultyObserver`
**Location:** `Runtime/RemoteConfig/  [ugs package]`
**Responsibility:** Fetch difficulty settings from RemoteConfig service
**Event Published:**
- `UGS_EventsEnum.DifficultySettingsFetched` (data: `List<DifficultyConfig>`)

**Key Changes from Coupling Remediation:**
- ✅ Now imports `CrawfisSoftware.Config` (shared namespace)
- ✅ Uses shared `DifficultyConfig` type instead of TempleRun's
- ✅ No direct coupling to TempleRun domain

**Code Flow:**
```csharp
// In DifficultyObserver.cs
void OnSignedIn()
{
    if (IsSignedIn())
    {
        _ = GetDifficultySettingsAsync();
    }
}

async Task GetDifficultySettingsAsync()
{
    var difficulties = await GetDefinitions();
    RuntimeDifficultySettings = difficulties;

    // Publish event with fetched configs
    UGSBus.Publish(
        UGS_EventsEnum.DifficultySettingsFetched,
        this,
        difficulties  // List<DifficultyConfig>
    );
}

async Task<List<DifficultyConfig>> GetDefinitions()
{
    var configs = await RemoteConfigService.Instance
        .FetchConfigsAsync(new EmptyStruct(), new EmptyStruct());

    var difficultiesJobject = configs.config["difficulty"];

    // Deserialize to shared DifficultyConfig type
    var difficulties = difficultiesJobject
        .ToObject<List<DifficultyConfig>>();

    return difficulties ?? new List<DifficultyConfig>();
}
```

**RemoteConfig Schema:**
The RemoteConfig service stores difficulty settings under key `"difficulty"` as JSON:
```json
{
  "difficulty": [
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

### Phase 3: UGS → GameFlow Bridge
**Component:** `UGSGameFlowBridge`
**Location:** `Runtime/Events/  [ugs package]`
**Responsibility:** Translate UGS events to GameFlow events
**Events:**
- **Receives:** `UGS_EventsEnum.DifficultySettingsFetched`
- **Publishes:** `GameFlowEvents.DifficultySettingsApplied`

**Code Flow:**
```csharp
// In UGSGameFlowBridge.cs
private Dictionary<UGS_EventsEnum, GameFlowEvents>
    _autoUGS2GameFlowEvents = new Dictionary<UGS_EventsEnum, GameFlowEvents>()
{
    // Line 32: Difficulty settings bridge mapping
    { UGS_EventsEnum.DifficultySettingsFetched,
      GameFlowEvents.DifficultySettingsApplied },
};

private void AutoFireGameFlowEventFromUGSEvent(
    string eventName,
    object sender,
    object data)
{
    if (_autoUGS2GameFlowEvents.TryGetValue(
        (UGS_EventsEnum)Enum.Parse(typeof(UGS_EventsEnum), eventName),
        out GameFlowEvents autoEvent))
    {
        // Translate and publish
        DelayedFire(_delayBetweenEvents,
            autoEvent.ToString(),
            sender,
            data);  // Data passed through unchanged
    }
}
```

**Why This Bridge?**
- UGS publishes UGS-specific events
- GameFlow doesn't directly access UGS events
- Bridge enables clean separation of domains
- Event data (DifficultyConfig list) passes through unchanged

---

### Phase 4: GameFlow → TempleRun Bridge
**Component:** `TempleRunGameFlowBridge`
**Location:** `Assets/GameFlow/Scripts/TempleRunSpecific/`
**Responsibility:** Translate GameFlow events to TempleRun events
**Events:**
- **Receives:** `GameFlowEvents.DifficultySettingsApplied`
- **Publishes:** `TempleRunEvents.DifficultySettingsApplied`

**Code Flow:**
```csharp
// In TempleRunGameFlowBridge.cs
[SerializeField] private Dictionary<GameFlowEvents, TempleRunEvents>
    _autoGameFlow2TempleRunEvents = new Dictionary<GameFlowEvents, TempleRunEvents>()
{
    // Line 52: Difficulty settings bridge mapping
    { GameFlowEvents.DifficultySettingsApplied,
      TempleRunEvents.DifficultySettingsApplied },
};

private void AutoFireTempleRunEventFromGameFlowEvent(
    string eventName,
    object sender,
    object data)
{
    if (_autoGameFlow2TempleRunEvents.TryGetValue(
        (GameFlowEvents)Enum.Parse(typeof(GameFlowEvents), eventName),
        out TempleRunEvents autoEvent))
    {
        // Translate and publish
        DelayedFire(_delayBetweenEvents,
            autoEvent.ToString(),
            sender,
            data);  // Data passed through unchanged
    }
}
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
- **Receives:** `TempleRunEvents.DifficultySettingsApplied`
- **Publishes:** `TempleRunEvents.DifficultyChanged` (when difficulty selected)

**Key Changes from Coupling Remediation:**
- ✅ Moved from GameFlow to TempleRun domain
- ✅ Now subscribes to `TempleRunEvents` only
- ✅ Uses `TempleRunBus` exclusively
- ✅ No cross-domain event subscriptions

**Code Flow:**
```csharp
// In GameDifficultyManager.cs (TempleRun domain)
public void Awake()
{
    TempleRunBus.Subscribe(
        TempleRunEvents.DifficultyChanging,
        OnDifficultyChanging);
    TempleRunBus.Subscribe(
        TempleRunEvents.DifficultySettingsApplied,
        OnDifficultySettingsChanged);
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
**File:** `Runtime/Events/  [ugs package]UGS_EventsEnum.cs`

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
    DifficultySettingsApplied = XXX,  // Bridged from UGS
    DifficultyChanged = XXX,          // When difficulty is selected
    DifficultyChanging = XXX,         // When difficulty change is requested
    DifficultyChangeFailed = XXX,     // When difficulty change fails
    // ... other events
}
```

### TempleRun Events
**File:** `Assets/TempleRun/Scripts/Events/TempleRunEvents.cs`

```csharp
public enum TempleRunEvents
{
    // Related to RemoteConfig/Difficulty
    DifficultySettingsApplied = 320,  // Bridged from GameFlow
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
| 2 | DifficultyObserver | DifficultySettingsFetched | List<DifficultyConfig> | UGSBus |
| 3 | UGSGameFlowBridge | DifficultySettingsApplied | List<DifficultyConfig> | GameFlowBus |
| 4 | TempleRunGameFlowBridge | DifficultySettingsApplied | List<DifficultyConfig> | TempleRunBus |
| 5 | GameDifficultyManager | (stores configs) | Dictionary<string, DifficultyConfig> | Local storage |

---

## Key Architectural Principles

### 1. **Domain Isolation**
- Each domain (UGS, GameFlow, TempleRun) only subscribes to events from its own event system
- Cross-domain communication ONLY through bridges
- No direct method calls or singleton access across domains

### 2. **Shared Configuration**
- `DifficultyConfig` lives in `Assets/_Common/Config/` (neutral domain)
- All domains can reference this shared type without creating coupling
- Before remediation: UGS imported from TempleRun (❌ coupled)
- After remediation: UGS imports from _Common (✅ decoupled)

### 3. **Event-Driven Data Flow**
- RemoteConfig data flows as event payloads
- No direct polling or state access
- Each domain receives configuration via subscribed events

### 4. **Bridge Pattern**
- Bidirectional mapping dictionaries translate between event systems
- Auto-fire mechanism automatically translates events
- Data payload passes through unchanged
- No domain logic in bridges

---

## Example Scenario: Difficulty Change During Gameplay

```
Player selects "Hard" difficulty during menu

    1. Menu publishes: UserInitiatedEvents.DifficultyChangeRequested
       (data: "Hard")

    2. GameDifficultyManager.OnDifficultyChanging() receives:
       TempleRunEvents.DifficultyChanging
       (data: "Hard")

    3. GameDifficultyManager calls:
       SetDifficulty("Hard")

    4. GameDifficultyManager publishes:
       TempleRunEvents.DifficultyChanged
       (data: DifficultyConfig for Hard)

    5. Blackboard subscribes to this event:
       GameConfig = (received DifficultyConfig)

    6. Gameplay adjusts based on new difficulty:
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
DifficultyObserver (UGS)
    └─ using CrawfisSoftware.Config;  ✅ shared namespace
    └─ uses DifficultyConfig from _Common (✅ no cross-domain coupling)
    └─ publishes UGS_EventsEnum.DifficultySettingsFetched ✅
    ↓
UGSGameFlowBridge
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
| `Runtime/RemoteConfig/  [ugs package]DifficultyObserver.cs` | Fetch RemoteConfig, publish DifficultySettingsFetched |
| `Runtime/Events/  [ugs package]UGSGameFlowBridge.cs` | Bridge UGS → GameFlow events |
| `Assets/GameFlow/Scripts/TempleRunSpecific/TempleRunGameFlowBridge.cs` | Bridge GameFlow → TempleRun events |
| `Assets/TempleRun/Scripts/Config/GameDifficultyManager.cs` | Apply difficulty settings in TempleRun domain |
| `Assets/_Common/Config/DifficultyConfig.cs` | Shared data type for all domains |
| `Runtime/Events/  [ugs package]UGS_EventsEnum.cs` | UGS event definitions |
| `Assets/GameFlow/Scripts/Events/GameFlowEvents.cs` | GameFlow event definitions |
| `Assets/TempleRun/Scripts/Events/TempleRunEvents.cs` | TempleRun event definitions |

---

## Summary

The RemoteConfig system demonstrates the **event-driven architecture** principles of RunnerUGS:

1. **Domains are isolated** - Each domain only uses its own events
2. **Data flows through events** - RemoteConfig difficulty settings propagate as event payloads
3. **Bridges translate between domains** - UGSGameFlowBridge and TempleRunGameFlowBridge enable cross-domain communication
4. **Shared types don't create coupling** - DifficultyConfig in _Common is referenced by all domains without coupling
5. **No direct access** - UGS never directly accesses TempleRun code or state

This architecture ensures clean separation of concerns, testability, and maintainability across the complex multi-domain codebase.
