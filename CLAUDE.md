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
| Entry Scene | `Assets/Scenes/Boot/0_BootStrap` |
| Game Event Enums | `Assets/Scripts/Events/GameFlowEvents.cs`, `TempleRunEvents.cs`, `UserInitiatedEvents.cs` |
| UGS Event Enums | `Assets/Scripts/UnityGamingServices/Events/UGS_EventsEnum.cs` |
| Event Publishers | `Assets/Scripts/Events/EventsPublisher*.cs`, `Assets/Scripts/UnityGamingServices/Events/EventsPublisherUGS.cs` |
| Auto-Event Flow | `Assets/Scripts/Events/GameFlowAutoEventFlow.cs`, `TempleRunAutoEventFlow.cs` |
| Game State | `Assets/Scripts/GameConfig/Blackboard.cs` |

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

Events auto-chain through dictionary mappings in `GameFlowAutoEventFlow.cs` and `TempleRunAutoEventFlow.cs`:

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
| Event Enums | `Assets/Scripts/Events/GameFlowEvents.cs`, `TempleRunEvents.cs`, `UserInitiatedEvents.cs` |
| UGS Event Enums | `Assets/Scripts/UnityGamingServices/Events/UGS_EventsEnum.cs` |
| Event Publishers | `Assets/Scripts/Events/EventsPublisherGameFlow.cs`, `EventsPublisherTempleRun.cs`, `EventsPublisherUserInitiated.cs` |
| UGS Publisher | `Assets/Scripts/UnityGamingServices/Events/EventsPublisherUGS.cs` |
| Auto-Event Flow | `Assets/Scripts/Events/GameFlowAutoEventFlow.cs`, `TempleRunAutoEventFlow.cs`, `AutoEventFlowBase.cs` |
| Game State | `Assets/Scripts/GameConfig/Blackboard.cs`, `GameConstants.cs`, `DifficultyConfig.cs` |
| Example Subscribers | `Assets/Scripts/Player/TurnController.cs`, `DeathWatcher.cs`, `PlayerLifeController.cs` |
| UI Controllers | `Assets/Scripts/UI/UIPanelController.cs`, `MainMenuPanelController.cs` |
| UGS Integration | `Assets/Scripts/UnityGamingServices/` subdirectories |

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
1. Create `UGS_Boot_N_[Feature]` scene
2. Add scene to Build Profile scene list
3. Create event adapters bridging UGS callbacks to EventsPublisher
4. Wire loading in `0_BootStrap` via `LoadSceneAdditively` component

### Adding New Gameplay Feature
1. Create scene in `Scenes/Game/`
2. Add to Build Profile
3. Subscribe to relevant events in `Awake()`
4. Publish state changes as events
5. Keep visuals/audio separate from logic

### Modifying UI Panels
1. Find panel in `Assets/Scripts/UI/`
2. Panels subscribe to `GameFlowEvents` for show/hide
3. Use `UIPanelController` as base class pattern
