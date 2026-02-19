# CLAUDE.md - AI Assistant Guide for RunnerUGS

This file provides guidance for AI assistants working with the RunnerUGS codebase. For detailed architecture diagrams, visual walkthroughs, and complete documentation, see [README.md](README.md).

## Quick Reference

### Essential Commands
```
Play in Editor:     CrawfisSoftware > Play Scene 0 Always (toggle ON)
Event Logging:      CrawfisSoftware > Events > Event Logging Enabled
Cloud Code:         Services > Cloud Code > Generate All Modules Bindings
Build Profiles:     File > Build Profiles > [Windows | Test_UGS_Windows | Test_GameOnly_Windows]
```

### Critical Paths
| Purpose | Path |
|---------|------|
| Entry Scene | `Assets/GameFlow/Scenes/Boot/0_BootStrap` |
| GameFlow Events | `Assets/GameFlow/Scripts/Events/GameFlowEvents.cs` |
| TempleRun Events | `Assets/TempleRun/Scripts/Events/TempleRunEvents.cs`, `UserInitiatedEvents.cs` |
| UGS Events | `Assets/UGS/Scripts/Events/UGS_EventsEnum.cs` |
| Event Publishers | `Assets/GameFlow/Scripts/Events/EventsPublisherGameFlow.cs`, `Assets/TempleRun/Scripts/Events/EventsPublisherTempleRun.cs`, `Assets/UGS/Scripts/Events/EventsPublisherUGS.cs` |
| Auto-Event Flow | `Assets/GameFlow/Scripts/Events/GameFlowAutoEventFlow.cs`, `Assets/TempleRun/Scripts/Events/TempleRunAutoEventFlow.cs`, `Assets/_Common/Events/AutoEventFlowBase.cs` |
| Game State | `Assets/GameFlow/Scripts/Config/Blackboard.cs` |

## MANDATORY: Event System Enforcement

**ALL communication between systems MUST go through the EventsPublisher event system. No exceptions.**

### Rules for ANY Code Change

1. **No direct cross-system method calls.** Components MUST NOT call methods on components in other scenes or domains. Use events instead.
2. **No `FindObjectOfType`, `GetComponent` across scene boundaries, `SendMessage`, or `BroadcastMessage`** for cross-system communication.
3. **Every new feature, behavior, or action** that communicates across systems MUST have corresponding events in the appropriate enum.
4. **Every subscription MUST have a matching unsubscription** in `OnDestroy()`.
5. **Domain isolation: Cross-domain event references are ONLY allowed in bridge files.** See [Domain Isolation Rule](#domain-isolation-rule) below.

### Domain Isolation Rule

**Each domain's code may ONLY subscribe to, publish, or reference events from its own domain.** Cross-domain event references are permitted ONLY inside bridge classes.

| Code Location | May Reference |
|---------------|---------------|
| `Assets/TempleRun/**/*.cs` | `TempleRunEvents`, `UserInitiatedEvents` only |
| `Assets/GameFlow/**/*.cs` (non-bridge) | `GameFlowEvents` only |
| `Assets/UGS/**/*.cs` (non-bridge) | `UGS_EventsEnum` only |
| `TempleRunGameFlowBridge.cs` | `TempleRunEvents` + `GameFlowEvents` (bridge duty) |
| `UGSGameFlowBridge.cs` | `UGS_EventsEnum` + `GameFlowEvents` (bridge duty) |

**Violations — what NOT to do:**
- TempleRun scripts subscribing to or publishing `GameFlowEvents` (e.g., `EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameStarted, ...)` in a TempleRun file)
- GameFlow scripts subscribing to or publishing `TempleRunEvents` (e.g., `EventsPublisherTempleRun.Instance.PublishEvent(TempleRunEvents.CountdownTick, ...)` in a GameFlow file)
- UGS scripts subscribing to or publishing `GameFlowEvents` directly (should go through `UGSGameFlowBridge`)
- GameFlow scripts subscribing to or publishing `UGS_EventsEnum` directly (should go through `UGSGameFlowBridge`)

**How to fix a violation:** If TempleRun code needs to react to a GameFlow event, add a bridge mapping in `TempleRunGameFlowBridge.cs` that translates the GameFlow event into a TempleRun event, then subscribe to the TempleRun event in your TempleRun code. The same applies for UGS <-> GameFlow.

### Required Skills Workflow

When adding any new feature or behavior, you MUST follow this workflow:

1. **`/list-events`** — First, review existing events to understand the current landscape and avoid duplicates
2. **`/add-event`** — Add new events to the correct domain enum with proper naming and numbering
3. **`/add-auto-chain`** — Wire automatic event progressions (e.g., Requested -> Starting) if needed
4. **`/add-bridge-mapping`** — Wire cross-domain bridges if the feature spans domains
5. **`/audit-events`** — After implementation, verify no anti-patterns were introduced

**Do NOT skip these steps.** Even for "simple" features, the event infrastructure must be established BEFORE writing the feature logic. The event definitions drive the architecture.

### When to Use Each Skill

| Situation | Required Skills |
|-----------|----------------|
| Adding any new feature | `/list-events` then `/add-event` then implement |
| Feature spans two domains | `/add-bridge-mapping` after `/add-event` |
| Events should auto-progress | `/add-auto-chain` after `/add-event` |
| After any implementation work | `/audit-events` to verify compliance |
| Before starting work on events | `/list-events` to understand current state |

## Architecture Overview

Unity 6 endless runner demonstrating **event-driven architecture** with Unity Gaming Services.

**Four Event Domains:**
- `GameFlowEvents` - Application lifecycle (loading screens, menus, game sessions, pause/resume, config/difficulty, save/load, quit)
- `TempleRunEvents` - Gameplay-specific (player lifecycle, countdown, turns, slides, jumps, lane changes, collisions, coins, power-ups, track/spline generation, teleportation)
- `UserInitiatedEvents` - Raw input events (turn requests, pause toggle)
- `UGS_EventsEnum` - Unity Gaming Services events (initialization, authentication, remote config, leaderboards, achievements, rewarded ads)

**Four Singleton Publishers:**
- `EventsPublisherGameFlow.Instance`
- `EventsPublisherTempleRun.Instance`
- `EventsPublisherUserInitiated.Instance`
- `EventsPublisherUGS.Instance`

## Event System Patterns

### Subscribing to Events

```csharp
private void Awake()
{
    EventsPublisherGameFlow.Instance.SubscribeToEvent(
        GameFlowEvents.GameStarting,
        OnGameStarting
    );
}

private void OnDestroy()
{
    EventsPublisherGameFlow.Instance.UnsubscribeToEvent(
        GameFlowEvents.GameStarting,
        OnGameStarting
    );
}

private void OnGameStarting(string eventName, object sender, object data)
{
    // Handle event - cast data if needed: var score = (float)data;
}
```

**CRITICAL: Always unsubscribe in OnDestroy()** - failure causes null reference errors after scene unload.

### Publishing Events

```csharp
// Without data
EventsPublisherGameFlow.Instance.PublishEvent(
    GameFlowEvents.MainMenuShown,
    this,
    null
);

// With data payload
float score = Blackboard.Instance.DistanceTracker.DistanceTravelled;
EventsPublisherTempleRun.Instance.PublishEvent(
    TempleRunEvents.PlayerDied,
    this,
    score
);

// With tuple data
EventsPublisherTempleRun.Instance.PublishEvent(
    TempleRunEvents.ActiveTrackChanging,
    this,
    (direction, segmentDistance)
);
```

### Auto-Event Flow Pattern

Events auto-chain through dictionary mappings in `Assets/GameFlow/Scripts/Events/GameFlowAutoEventFlow.cs` and `Assets/TempleRun/Scripts/Events/TempleRunAutoEventFlow.cs`:

```csharp
// In GameFlowAutoEventFlow.cs - GameFlow domain auto-chains
_autoGameFlow2GameFlowEvents = new Dictionary<GameFlowEvents, GameFlowEvents>()
{
    { GameFlowEvents.GameStartRequested, GameFlowEvents.GameStarting },
    { GameFlowEvents.GameScenesLoaded, GameFlowEvents.GameStartRequested },
    { GameFlowEvents.GameplayReady, GameFlowEvents.MainMenuShowRequested },
    { GameFlowEvents.GameEnding, GameFlowEvents.GameScenesUnloadRequested },
    // ... more mappings
};
```

When `GameScenesLoaded` fires, it automatically triggers `GameStartRequested` → `GameStarting`.

Note: Countdown events (`CountdownStartRequested`, `CountdownTick`, etc.) are now in `TempleRunEvents` since they are gameplay-specific.

### Adding New Events

**Step 1: Determine the correct domain**
- `GameFlowEvents` - For app/session lifecycle (loading, menus, pause, config, quit)
- `TempleRunEvents` - For gameplay mechanics (player actions, countdown, track, collisions)
- `UserInitiatedEvents` - For raw input events
- `UGS_EventsEnum` - For UGS service callbacks

**Step 2: Add to appropriate enum with a unique value**

```csharp
// Example: Adding to GameFlowEvents.cs (values grouped by category)
public enum GameFlowEvents
{
    // ... existing events ...
    // ---------- My New Feature ----------
    MyFeatureRequested = 130,
    MyFeatureStarting = 131,
    MyFeatureStarted = 132,
    MyFeatureFailed = 133,
}
```

**Step 3: (Optional) Add auto-chaining in the appropriate flow class**

```csharp
// In GameFlowAutoEventFlow.cs or TempleRunAutoEventFlow.cs
{ GameFlowEvents.MyFeatureRequested, GameFlowEvents.MyFeatureStarting },
```

**Step 4: Subscribe and publish as needed**

### Event Naming Conventions
- `*Requested` - User or system initiated a request
- `*Starting` / `*ing` - Action is beginning (async operation in progress)
- `*Started` / `*ed` - Action completed successfully
- `*Failed` - Action failed
- `*Cancelled` - Action was cancelled

## Coding Conventions

### Namespaces
```
CrawfisSoftware.Events           - Event system core (GameFlowEvents, UserInitiatedEvents, publishers)
CrawfisSoftware.TempleRun        - Gameplay logic (TempleRunEvents enum)
CrawfisSoftware.TempleRun.Events - Gameplay auto-event flows (TempleRunAutoEventFlow)
CrawfisSoftware.UI               - UI controllers
CrawfisSoftware.UGS              - Unity Gaming Services integration
CrawfisSoftware.UGS.Events       - UGS events (UGS_EventsEnum, EventsPublisherUGS)
CrawfisSoftware.GameConfig       - Global constants
CrawfisSoftware.SceneManagement  - Scene loading utilities
```

### Field Naming
```csharp
[SerializeField] private string _sceneName;      // Private: underscore prefix
public float TurnAvailableDistance { get; }      // Properties: PascalCase
private readonly Dictionary<...> _mapping = ...; // readonly: underscore prefix
```

### XML Documentation
```csharp
/// <summary>
/// Brief description of the class purpose.
///    Dependencies: List dependencies
///    Subscribes: List events subscribed to
///    Publishes: List events published
/// </summary>
internal class MyController : MonoBehaviour
```

### MonoBehaviour Lifecycle
- `Awake()` - Subscriptions and initialization
- `OnDestroy()` - Cleanup and unsubscriptions
- `Start()` - Only when dependent on other Awake() completions

## Key Files Reference

| Category | Files |
|----------|-------|
| **GameFlow Domain** | |
| Event Enums | `Assets/GameFlow/Scripts/Events/GameFlowEvents.cs` |
| Event Publishers | `Assets/GameFlow/Scripts/Events/EventsPublisherGameFlow.cs` |
| Auto-Event Flow | `Assets/GameFlow/Scripts/Events/GameFlowAutoEventFlow.cs` |
| Bridges | `Assets/GameFlow/Scripts/Events/TempleRunGameFlowBridge.cs` |
| Game State | `Assets/GameFlow/Scripts/Config/Blackboard.cs`, `GameConstants.cs`, `GameState.cs` |
| UI Controllers | `Assets/GameFlow/Scripts/UI/UIPanelController.cs`, `MainMenuPanelController.cs` |
| Game Control | `Assets/GameFlow/Scripts/GameControl/GameController.cs`, `PauseController.cs` |
| Scene Management | `Assets/GameFlow/Scripts/SceneManagement/LoadSceneAfterGameControlEvent.cs` |
| **TempleRun Domain** | |
| Event Enums | `Assets/TempleRun/Scripts/Events/TempleRunEvents.cs`, `UserInitiatedEvents.cs` |
| Event Publishers | `Assets/TempleRun/Scripts/Events/EventsPublisherTempleRun.cs`, `EventsPublisherUserInitiated.cs` |
| Auto-Event Flow | `Assets/TempleRun/Scripts/Events/TempleRunAutoEventFlow.cs` |
| Config | `Assets/TempleRun/Scripts/Config/TempleRunGameConfig.cs`, `DifficultyConfig.cs` |
| Player Controllers | `Assets/TempleRun/Scripts/Player/TurnController.cs`, `DeathWatcher.cs`, `PlayerLifeController.cs` |
| Track Generation | `Assets/TempleRun/Scripts/Track/TrackManager.cs`, `SplineCreator2D.cs` |
| Input | `Assets/TempleRun/Scripts/Input/MovementInputActions.cs`, `SwipeDetectorActions.cs` |
| **UGS Domain** | |
| Event Enums | `Assets/UGS/Scripts/Events/UGS_EventsEnum.cs` |
| Event Publishers | `Assets/UGS/Scripts/Events/EventsPublisherUGS.cs` |
| Auto-Event Flow | `Assets/UGS/Scripts/Events/UGSAutoEventFlow.cs` |
| Bridges | `Assets/UGS/Scripts/Events/UGSGameFlowBridge.cs` |
| Initialization | `Assets/UGS/Scripts/Initialization/GameManagerUGS.cs`, `PlayerAuthenticationManager.cs` |
| Remote Config | `Assets/UGS/Scripts/RemoteConfig/RemoteConfigManager.cs`, `GameBalance.cs` |
| Leaderboards | `Assets/UGS/Scripts/Leaderboard/LeaderboardController.cs` |
| Achievements | `Assets/UGS/Scripts/Achievements/AchievementsPrefab.cs` |
| **Shared/Common** | |
| Auto-Event Base | `Assets/_Common/Events/AutoEventFlowBase.cs` |
| Test Utilities | `Assets/_Common/Test/Test_AutoFireEvent.cs` |
| Utilities | `Assets/_Common/Utility/Logger.cs`, `EventLoggerDump.cs` |

## Gotchas and Warnings

### Event Subscriptions
- **ALWAYS** unsubscribe in `OnDestroy()` - failure causes errors after scene unload
- Event handler signature: `(string eventName, object sender, object data)`
- Cast data explicitly: `var score = (float)data;` or `var tuple = ((Direction, float))data;`

### Scene Loading
- All scenes load **additively** from the persistent Boot scene
- **Never** use `LoadSceneMode.Single` unless intentionally resetting everything
- "Play Scene 0 Always" setting resets on Unity restart - re-enable it

### Auto-Event Flow
- Auto-events fire immediately by default (configurable delay in `AutoEventFlowBase`)
- Circular dependencies will cause infinite loops - verify mappings
- Some events are intentionally NOT auto-chained (documented in comments)

### Singletons
- `Blackboard.Instance` - Global game state
- `EventsPublisher*.Instance` - Event buses
- Only access after `Awake()` has run (use `[DefaultExecutionOrder(-10000)]` on publishers)

## Testing

### Test Without UGS
1. Open `0_BootStrap` scene
2. Disable the `Load_UGS_Init` GameObject
3. Play

### Enable Event Logging
`CrawfisSoftware > Events > Event Logging Enabled`

### Build Profiles
| Profile | Purpose |
|---------|---------|
| **Windows** | Full production build with UGS |
| **Test_UGS_Windows** | UGS testing with dummy game (random score) |
| **Test_GameOnly_Windows** | Gameplay without UGS services |

## Common Tasks

### Adding a New UGS Feature
1. **`/list-events UGS`** — Review existing UGS events
2. **`/add-event`** — Add events to `UGS_EventsEnum` for the new service callbacks
3. **`/add-bridge-mapping`** — Bridge UGS events to GameFlow (via `UGSGameFlowBridge`)
4. **`/add-auto-chain`** — Wire UGS auto-progressions if needed
5. Create `UGS_Boot_N_[Feature]` scene
6. Add scene to Build Profile scene list
7. Create event adapters bridging UGS SDK callbacks to `EventsPublisherUGS`
8. Wire loading in `0_BootStrap` via `LoadSceneAdditively` component
9. **`/audit-events`** — Verify all subscriptions have matching unsubscriptions

### Adding New Gameplay Feature
1. **`/list-events TempleRun`** — Review existing TempleRun events
2. **`/add-event`** — Add events to `TempleRunEvents` for the new mechanic
3. **`/add-auto-chain`** — Wire auto-progressions (e.g., Requested -> Starting)
4. **`/add-bridge-mapping`** — Bridge to GameFlow if the feature affects game session state
5. Create scene in `Assets/TempleRun/Scenes/Gameplay/`
6. Add to Build Profile
7. Subscribe to relevant events in `Awake()`, unsubscribe in `OnDestroy()`
8. Publish state changes as events via `EventsPublisherTempleRun.Instance`
9. Keep visuals/audio separate from logic
10. **`/audit-events`** — Verify compliance

### Adding New GameFlow Feature
1. **`/list-events GameFlow`** — Review existing GameFlow events
2. **`/add-event`** — Add events to `GameFlowEvents`
3. **`/add-auto-chain`** — Wire auto-progressions
4. Implement the feature, subscribing/publishing via `EventsPublisherGameFlow.Instance`
5. **`/audit-events`** — Verify compliance

### Modifying UI Panels
1. Find panel in `Assets/GameFlow/Scripts/UI/`
2. Panels subscribe to `GameFlowEvents` for show/hide
3. Use `UIPanelController` as base class pattern
4. If adding new panel states, use **`/add-event`** to add show/hide events to `GameFlowEvents`

## Folder Structure

The codebase is organized into **four primary domains** with clear separation of concerns:

```
Assets/
├── _Common/                          # Shared infrastructure
│   ├── Events/                       # AutoEventFlowBase (base class for all auto-flows)
│   ├── Test/                         # Test utilities
│   │   └── Scenes/                   # Test boot scenes
│   └── Utility/                      # Logger, EventLoggerDump, DebugLog
│
├── GameFlow/                         # Application lifecycle domain
│   ├── Scripts/
│   │   ├── Events/                   # GameFlowEvents, EventsPublisherGameFlow, GameFlowAutoEventFlow
│   │   │                             # TempleRunGameFlowBridge (bridges TempleRun ↔ GameFlow)
│   │   ├── Config/                   # Blackboard, GameConstants, GameState, PlayerPrefKeys
│   │   ├── GameControl/              # GameController, PauseController, QuitController
│   │   ├── UI/                       # UIPanelController, MainMenuPanelController
│   │   └── SceneManagement/          # LoadSceneAfterGameControlEvent, FireEventAfterSceneLoads
│   ├── Scenes/
│   │   └── Boot/                     # Boot scenes (0_BootStrap, Game_Boot_*)
│   ├── Audio/                        # UI sound effects
│   └── UI Toolkit/                   # UXML, USS for GameFlow UI
│
├── TempleRun/                        # Gameplay domain
│   ├── Scripts/
│   │   ├── Events/                   # TempleRunEvents, EventsPublisherTempleRun, TempleRunAutoEventFlow
│   │   │                             # UserInitiatedEvents, EventsPublisherUserInitiated
│   │   ├── Config/                   # TempleRunGameConfig, DifficultyConfig, DifficultySettings
│   │   ├── Player/                   # TurnController, DeathWatcher, PlayerLifeController
│   │   ├── Track/                    # TrackManager, SplineCreator2D, DistanceTracker
│   │   ├── TrackVisuals/             # PrefabSpawner (SimplePlane, Voxels)
│   │   ├── Input/                    # MovementInputActions, SwipeDetectorActions
│   │   └── Audio/                    # TurnAudioFeedback, Metronome
│   ├── Scenes/
│   │   └── Gameplay/                 # Gameplay scenes (TempleRunGameplay, etc.)
│   ├── Graphics/                     # Models, Textures, Materials, Shaders, VFX, Animations
│   ├── Audio/                        # Gameplay music and SFX
│   ├── Prefabs/                      # Gameplay prefabs
│   ├── Scriptables/                  # ScriptableObjects for TempleRun
│   └── UI Toolkit/                   # UXML, USS for gameplay UI
│
└── UGS/                              # Unity Gaming Services domain
    ├── Scripts/
    │   ├── Events/                   # UGS_EventsEnum, EventsPublisherUGS, UGSAutoEventFlow
    │   │                             # UGSGameFlowBridge (bridges UGS → GameFlow)
    │   ├── Initialization/           # GameManagerUGS, PlayerAuthenticationManager, UGS_State
    │   ├── Authentication/           # PlayerSignInController
    │   ├── RemoteConfig/             # RemoteConfigManager, GameBalance, FeatureFlags
    │   ├── Leaderboard/              # LeaderboardController, LeaderboardPlayerController
    │   ├── Achievements/             # AchievementsPrefab, DistanceBasedAchievements
    │   ├── Economy/                  # PlayerEconomyManager, PlayerEconomyManagerClient
    │   ├── PlayerData/               # PlayerDataManager, PlayerDataManagerClient
    │   └── Managers/                 # (Reserved for future managers)
    ├── Scenes/
    │   ├── Boot/                     # UGS boot scenes (UGS_Boot_*)
    │   ├── Test/                     # Test-specific UGS scenes
    │   └── UGS/                      # UGS UI scenes (Achievements, Leaderboards)
    ├── CloudCode/
    │   └── TempleRunUGSCloud~/       # .NET 6.0 Cloud Code project (30+ functions)
    ├── Editor/                       # RemoteConfig editor data
    └── Prefabs/                      # UGS-related prefabs
```

### Domain Responsibilities

- **_Common**: Shared base classes and utilities used across all domains
- **GameFlow**: Application lifecycle - boot, initialization, menus, pause, quit, scene management
- **TempleRun**: Gameplay mechanics - player movement, track generation, input, audio
- **UGS**: Unity Gaming Services - authentication, leaderboards, achievements, remote config

### Event Flow Architecture

```
USER INPUT (UserInitiatedEvents in TempleRun)
    ↓
TEMPLERUN GAMEPLAY (TempleRunEvents)
    ↓ (via TempleRunGameFlowBridge in GameFlow)
GAMEFLOW SESSION (GameFlowEvents)
    ↓ (via UGSGameFlowBridge in UGS)
UGS SERVICES (UGS_EventsEnum)
```
