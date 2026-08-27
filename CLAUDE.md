# CLAUDE.md - AI Assistant Guide for RunnerUGS

This file is the concrete working guide for AI assistants — **any** AI assistant or coding
agent, not just Claude — working with the RunnerUGS codebase. Start with
[AGENTS.md](AGENTS.md) for how to approach work here; this file holds the rules,
conventions, and paths. For detailed architecture diagrams, visual walkthroughs, and
complete documentation, see [README.md](README.md).

> Sibling repo: this is the Unity-Gaming-Services variant of
> [EndlessRunnerTemplate](https://github.com/crawfis/EndlessRunnerTemplate). The sibling is
> on a newer EventsPublisher API (static `EventsFor<T>` buses, typed payloads, Sticky
> delivery); THIS repo uses the singleton-publisher API documented below. Do not port
> sibling code or guidance verbatim — translate to this API, or propose the upgrade as its
> own explicit project.

## Quick Reference

### Essential Commands
```
Play in Editor:     CrawfisSoftware > Play Scene 0 Always (toggle ON)
Entry Scene:        Assets/UGS/Scenes/Boot/0_BootStrap (build index 0);
                    game-only variant: Assets/GameFlow/Scenes/Boot/0_BootStrap_Game_Only
Event Logging:      CrawfisSoftware > Events > Log Events   (same menu: Clear Now,
                    List Current Subscribers, Clear Events on Exiting Play Mode);
                    or add a DebugEventFileLogger for a file dump
List Domains:       CrawfisSoftware > Events > List Domains (EventsPublisher 2.5.0+).
                    All four domain enums are marked [EventEnum], so the menu sweeps and
                    lists them in EDIT MODE — per domain: prefix, enum, member / payload /
                    sticky / replay counts
Track Import:       CrawfisSoftware > Track > Import JSON -> ScriptableObjects (one-shot)
Cloud Code:         Services > Cloud Code > Generate All Modules Bindings
Build Profiles:     File > Build Profiles > [Windows | Test_UGS_Windows | Test_GameOnly_Windows]
```

### Critical Paths
| Purpose | Path |
|---------|------|
| Entry Scene | `Assets/UGS/Scenes/Boot/0_BootStrap` (build index 0); game-only: `Assets/GameFlow/Scenes/Boot/0_BootStrap_Game_Only` |
| GameFlow Events | `Assets/GameFlow/Scripts/Events/GameFlowEvents.cs` |
| TempleRun Events | `Assets/TempleRun/Scripts/Events/TempleRunEvents.cs`, `UserInitiatedEvents.cs` |
| UGS Events | `Assets/UGS/Scripts/Events/UGS_EventsEnum.cs` |
| Event Publishers | `Assets/GameFlow/Scripts/Events/EventsPublisherGameFlow.cs`, `Assets/TempleRun/Scripts/Events/EventsPublisherTempleRun.cs`, `Assets/UGS/Scripts/Events/EventsPublisherUGS.cs` |
| Auto-Event Flow | `Assets/GameFlow/Scripts/Events/GameFlowAutoEventFlow.cs`, `Assets/TempleRun/Scripts/Events/TempleRunAutoEventFlow.cs`, `Assets/UGS/Scripts/Events/UGSAutoEventFlow.cs` |
| Cross-Domain Bridges | `Assets/GameFlow/Scripts/TempleRunSpecific/TempleRunGameFlowBridge.cs` (incl. the TempleRun → UGS passthrough), `Assets/UGS/Scripts/Events/UGSGameFlowBridge.cs`, `Assets/TempleRun/Scripts/Events/Input2TempleRunAutoEventBridge.cs` |
| Game State | `Assets/GameFlow/Scripts/Config/GameState.cs`, `Assets/TempleRun/Scripts/Config/Blackboard.cs` |

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
| `TempleRunGameFlowBridge.cs` | `TempleRunEvents` + `GameFlowEvents` + `UGS_EventsEnum` (bridge duty; the last via the TempleRun → UGS passthrough dictionary) |
| `UGSGameFlowBridge.cs` | `UGS_EventsEnum` + `GameFlowEvents` (bridge duty) |

**Violations — what NOT to do:**
- TempleRun scripts subscribing to or publishing `GameFlowEvents` (e.g., `EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameStarted, ...)` in a TempleRun file)
- GameFlow scripts subscribing to or publishing `TempleRunEvents` (e.g., `EventsPublisherTempleRun.Instance.PublishEvent(TempleRunEvents.CountdownTick, ...)` in a GameFlow file)
- UGS scripts subscribing to or publishing `GameFlowEvents` directly (should go through `UGSGameFlowBridge`)
- GameFlow scripts subscribing to or publishing `UGS_EventsEnum` directly (should go through `UGSGameFlowBridge`)

**How to fix a violation:** If TempleRun code needs to react to a GameFlow event, add a bridge mapping in `TempleRunGameFlowBridge.cs` that translates the GameFlow event into a TempleRun event, then subscribe to the TempleRun event in your TempleRun code. The same applies for UGS <-> GameFlow.

> The four domains compile into `Assembly-CSharp` — no `.asmdef` separates them, so the
> compiler will NOT catch a violation (only the vendored `Assets/Blocks/` samples carry
> asmdefs). Isolation is enforced by review and `/audit-events`; run it.

The rule's purpose is **replaceability**: a domain that talks only through events can be
swapped for a completely different implementation — or stubbed out with a trivial fake —
without touching code on the other side. This repo proves it at full scale, in both
directions: the `Test_GameOnly_Windows` build profile (or disabling `Load_UGS_Init` in the
bootstrap) runs the entire game with the UGS domain absent, and `Test_UGS_Windows` runs the
UGS services against a dummy game with random scores. Domains load from their own scenes,
so replacing a domain is loading a different scene that speaks the same events.

### Required Skills Workflow

Each step below is a **skill**: a step-by-step procedure stored as plain markdown in
`.claude/skills/<name>/SKILL.md`. In Claude Code, invoke it as the slash command shown. In
any other AI tool (Copilot, Cursor, Codex, Gemini, …), open the skill file and follow it as
a checklist — the steps are ordinary read/search/edit work and assume nothing
Claude-specific. Anywhere this repo's docs say `/some-skill`, read it as "follow
`.claude/skills/some-skill/SKILL.md`".

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
| Feature needs a whole NEW domain (rare) | `/add-event-domain` — decision gate inside; then `/add-event` for its events |
| Authoring track segments | Edit the `TrackSegmentSO` / `TrackLevelSO` assets in the Inspector, or use `/generate-segments` for bulk creation |

## Architecture Overview

Unity 6 endless runner demonstrating **event-driven architecture** with Unity Gaming Services.

**Domain Registry** — the authoritative list of event domains:

| Domain | Enum | Publisher (singleton) | Purpose | Publisher hosted in | Bridges |
|--------|------|----------------------|---------|--------------------|---------|
| **GameFlow** | `GameFlowEvents` | `EventsPublisherGameFlow.Instance` | App lifecycle: loading, menus, sessions, pause, config/difficulty, save/load, quit | all three bootstraps (`0_BootStrap`, `0_BootStrap_Game_Only`, `0_BootStrap_UGS_Only`) | ↔ TempleRun via `TempleRunGameFlowBridge`; ↔ UGS via `UGSGameFlowBridge` |
| **TempleRun** | `TempleRunEvents` | `EventsPublisherTempleRun.Instance` | Gameplay: player lifecycle, countdown, movement, collisions, coins/power-ups, track/spline generation, teleportation | `Game_Boot_2_Play` | ↔ GameFlow, plus a TempleRun → UGS **passthrough dictionary**, both in `TempleRunGameFlowBridge` |
| **UserInitiated** | `UserInitiatedEvents` | `EventsPublisherUserInitiated.Instance` | Raw input requests (turns, lanes, jump, slide, dash, pause, quit) | all three bootstraps | → TempleRun via `Input2TempleRunAutoEventBridge` |
| **UGS** | `UGS_EventsEnum` | `EventsPublisherUGS.Instance` | Unity Gaming Services: init, auth, remote config, leaderboards, achievements, economy, rewarded ads | `UGS_Boot_0_Initialization` (+ the UGS-only test boot) | ↔ GameFlow via `UGSGameFlowBridge` |

Two invariants keep this registry trustworthy:
- **Placement:** domain enums live only in `Assets/*/Scripts/Events/` folders, each with an
  `EventsPublisher*` singleton subclass beside it — the set of those subclasses IS the
  authoritative domain list. All four are marked `[EventEnum]`, so **List Domains** reports
  the same four from the registry: a domain in one list and not the other is drift.
- **Registration:** `/add-event-domain` adds a row here as part of its checklist, and
  `/audit-events` flags drift between this table and the code.

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

// With a struct payload (ActiveTrackChanging carries a TrackSegmentInfo)
EventsPublisherTempleRun.Instance.PublishEvent(
    TempleRunEvents.ActiveTrackChanging,
    this,
    _trackSegments.Peek()
);
```

Auto-flows and bridges use `SubscribeToAllEnumEvents` / `UnsubscribeToAllEnumEvents` to
hear every event of one enum and dispatch through their dictionaries. Application code
should subscribe to specific events instead.

### Auto-Event Flow Pattern

Events auto-chain through dictionary mappings in `GameFlowAutoEventFlow.cs`,
`TempleRunAutoEventFlow.cs`, and `UGSAutoEventFlow.cs`:

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

> Note: `AutoEventFlowBase.cs` under `Assets/_Common/Events/` is an empty (zero-byte)
> placeholder — nothing derives from it. The six dispatch classes (three auto-flows, three
> bridges) each re-implement the same subscribe-to-all-then-dictionary-dispatch pattern
> inline. Consolidating that shared logic into `AutoEventFlowBase` is a good,
> self-contained refactoring exercise.

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

> `UserInitiatedEvents` and `UGS_EventsEnum` use implicit values (no explicit numbers) —
> for those, just append in the right category block.

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
CrawfisSoftware.Events              - UserInitiatedEvents + EventsPublisherUserInitiated (+ package core)
CrawfisSoftware.GameFlow.Events     - GameFlowEvents, EventsPublisherGameFlow, GameFlowAutoEventFlow, TempleRunGameFlowBridge
CrawfisSoftware.GameFlow.UI         - GameFlow UI controllers
CrawfisSoftware.GameFlow.*          - Per-area: .GameConfig (GameConstants), .SceneManagement, .GameControl
CrawfisSoftware.TempleRun           - Gameplay logic (TempleRunEvents, Blackboard, EventsPublisherTempleRun, controllers)
CrawfisSoftware.TempleRun.Events    - TempleRun auto-event flow + Input2TempleRunAutoEventBridge
CrawfisSoftware.TempleRun.GameConfig - Difficulty / game-config managers
CrawfisSoftware.UGS                 - Unity Gaming Services integration (managers, EventsPublisherUGS)
CrawfisSoftware.UGS.Events          - UGS_EventsEnum and UGS event wiring
CrawfisSoftware.Config              - Shared, domain-neutral config (the LIVE DifficultyConfig in _Common)
CrawfisSoftware.Utility             - Shared utilities
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

### Transform Conventions
- **Prefer `transform.localPosition`** over `transform.position` when reading or writing positions
- **Prefer `transform.localRotation`** over `transform.rotation` when reading or writing rotations
- **When setting parent**, use `transform.SetParent(parent, worldPositionStays: false)` to avoid adjusting position

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
| Bridges | `Assets/GameFlow/Scripts/TempleRunSpecific/TempleRunGameFlowBridge.cs` (TempleRun ↔ GameFlow + the TempleRun → UGS passthrough) |
| Game State / Config | `Assets/GameFlow/Scripts/Config/GameState.cs`, `GameConstants.cs`, `PlayerPrefKeys.cs` |
| Level System | `Assets/GameFlow/Scripts/Config/LevelConfig.cs`, `LevelConfigApplier.cs`, `LevelRegistry.cs`, `LevelProgressManager.cs`, `LevelProgressData.cs`; assets in `Assets/TempleRun/Scriptables/Levels/` |
| UI Controllers | `Assets/GameFlow/Scripts/UI/GameFlowUIPanelController.cs` (loading / game-over overlays), `MainMenuController.cs`, `MainMenuPanelController.cs`, `LevelSelectorController.cs`, `LevelSelectorPanelController.cs` |
| Game Control | `Assets/GameFlow/Scripts/GameControl/QuitController.cs`, `UnloadNonActiveScenes.cs`, `LoadSceneAdditively.cs` |
| Scene Management | `Assets/GameFlow/Scripts/SceneManagement/DynamicLevelSceneLoader.cs`, `LoadSceneAfterGameControlEvent.cs`, `FireEventAfterSceneLoads.cs`, `FireEventWhenSceneCloses.cs`, `CloseSceneOnEvent.cs` |
| **TempleRun Domain** | |
| Event Enums | `Assets/TempleRun/Scripts/Events/TempleRunEvents.cs`, `UserInitiatedEvents.cs` |
| Event Publishers | `Assets/TempleRun/Scripts/Events/EventsPublisherTempleRun.cs`, `EventsPublisherUserInitiated.cs` |
| Auto-Event Flow | `Assets/TempleRun/Scripts/Events/TempleRunAutoEventFlow.cs`, `Input2TempleRunAutoEventBridge.cs` (input → gameplay) |
| Config | `Assets/TempleRun/Scripts/Config/Blackboard.cs`, `TempleRunGameConfig.cs`, `GameDifficultyManager.cs`, `DifficultySettings.cs`, `SetGameDifficulty.cs`, `LoadDefaultGameConfigs.cs`, `SpawnPrefabRegistry.cs`, `TempleRunConstants.cs`, per-mechanic configs (`CoinConfig.cs`, `DashConfig.cs`, `JumpConfig.cs`, `LaneConfig.cs`, `SlideConfig.cs`, `PowerUpDefinition.cs`, `PowerUpType.cs`) |
| Player Controllers | `Assets/TempleRun/Scripts/Player/TurnController.cs`, `JumpController.cs`, `SlideController.cs`, `DashController.cs`, `LaneChangeController.cs`, `PlayerLifeController.cs`, `PowerUpBuffController.cs`, `CountdownController.cs`, `DistanceController.cs`, `MoveCharacterByDistance.cs`, `PauseController.cs`, `PlayerPauseController.cs`, `AIController.cs` |
| Player Support | collision detectors (`ObstacleCollisionDetector.cs`, `CollectableCollisionDetector.cs`, `TurnCollisionDetector.cs`), `CoinCollectionController.cs`, motion shaping (`JumpArcController.cs`, `SlideArcController.cs`, `DashSpeedController.cs`, `LaneOffsetController.cs`), failure/teleport (`PlayerFailedController.cs`, `PlayerFailureAutoTurnController.cs`, `TeleportController.cs`, `CharacterTeleporter.cs`); `Assets/TempleRun/Scripts/GameTime.cs` |
| Power-Up Effects | `Assets/TempleRun/Scripts/PowerUps/IPowerUpEffect.cs`, `PowerUpEffectBase.cs`, `SpeedBoostEffect.cs`, `ScoreMultiplierEffect.cs`, `CoinMagnetEffect.cs`, `CoinDoublerEffect.cs`, `ShieldEffect.cs` |
| Track Generation | `Assets/TempleRun/Scripts/Track/TrackManager.cs` (+ `TrackManagerAbstract.cs`, `TrackManagerForTiles.cs`, `TrackManagerList.cs`), `PathProvider.cs`, `SegmentTransitionController.cs`, `SegmentAdvanceTrigger.cs`, `TrackSegmentLibrary.cs`, `TrackLibraryLoader.cs`, `TrackSegmentInfo.cs`, `Direction.cs`, `DistanceTracker.cs`, `DistanceInterestService.cs` |
| Segment Selection | `Assets/TempleRun/Scripts/Track/Selection/` — `ISegmentSelector.cs`, `ISegmentPool.cs`, `WeightedDifficultySelector.cs` (default), `AuthoredSequenceSelector.cs` |
| Track Geometry | `Assets/TempleRun/Scripts/Track/Geometry/` — `IPathSegmentBuilder.cs`, `AxisAligned90Builder.cs`, `ArcTurnBuilder.cs`, `PathPose.cs`, `PathSpan.cs`, `PathSegmentResult.cs`, `CardinalDirections.cs`; `SegmentGeometryData.cs` |
| Spawners | `Assets/TempleRun/Scripts/Track/SpawnerBase.cs`, `CoinSpawner.cs`, `ObstacleSpawner.cs`, `PowerUpSpawner.cs`, `PowerUpIdentifier.cs` |
| Track Visuals | `Assets/TempleRun/Scripts/TrackVisuals/PrefabSpawnerAbstract.cs`, `SimplePlane/SplinePrefabSpawner.cs`, `SimplePlane/TextureScaler.cs`, `Voxels/VoxelPrefabSpawner.cs` |
| Track Data | `Assets/TempleRun/Scriptables/Track/` — `Segments/*.asset` (one `TrackSegmentSO` per segment), `TrackSegmentRegistry.asset`, `TrackLevel_01..05_*.asset`, `TrackLevelRegistry.asset` |
| Gameplay UI | `Assets/TempleRun/Scripts/UI/GUIController.cs`, `CountdownUIController.cs` |
| Audio / Animation | `Assets/TempleRun/Scripts/Audio/` (`TurnAudioFeedback.cs`, `Metronome.cs`, …); `Assets/TempleRun/Scripts/Animation/CapsuleAnimationLink.cs` |
| Input | `Assets/TempleRun/Scripts/Input/MovementInputActions.cs`, `SwipeDetectorActions.cs`, `DashInputActions.cs`, `AccelerometerInputActions.cs`, `PauseQuitInputActions.cs`; `GameControls.cs` + `LeftRightJumpSlide.cs` are source-generated from the `.inputactions` assets — regenerate, don't hand-edit |
| Editor Tools | `Assets/TempleRun/Editor/TrackDataImporter.cs` (one-shot JSON -> SO importer) |
| **UGS Domain** | |
| Event Enums | `Assets/UGS/Scripts/Events/UGS_EventsEnum.cs` |
| Event Publishers | `Assets/UGS/Scripts/Events/EventsPublisherUGS.cs` |
| Auto-Event Flow | `Assets/UGS/Scripts/Events/UGSAutoEventFlow.cs` |
| Bridges | `Assets/UGS/Scripts/Events/UGSGameFlowBridge.cs` |
| Initialization | `Assets/UGS/Scripts/Initialization/GameManagerUGS.cs`, `PlayerAuthenticationManager.cs`, `UGS_State.cs`, `LocalStorageSystem.cs`, `NetworkConnectivityHandler.cs`, `UnityEventsToEventsPublisher.cs` (`Unused/` holds dead code) |
| Authentication | `Assets/UGS/Scripts/Authentication/PlayerSignInController.cs` |
| Remote Config | `Assets/UGS/Scripts/RemoteConfig/RemoteConfigManager.cs`, `GameBalance.cs`, `GameBalanceManager.cs`, `FeatureFlags.cs`, `FeatureFlagsManager.cs`, `CampaignEventConfig.cs`, `CampaignEventConfigManager.cs`, `DifficultyObserver.cs`, `LocalDifficultySettingsProvider.cs`, `AppAttributes.cs`, `UserAttributes.cs`, `RemoteConfigConstants.cs`, `ServiceObserverHelpers.cs` |
| Leaderboards | `Assets/UGS/Scripts/Leaderboard/LeaderboardController.cs`, `LeaderboardPlayerController.cs` |
| Achievements | `Assets/UGS/Scripts/Achievements/AchievementsPrefab.cs`, `DistanceBasedAchievements.cs`, `CoinBasedAchievements.cs` |
| Economy / Player Data | `Assets/UGS/Scripts/Economy/PlayerEconomyManager.cs` + `PlayerEconomyManagerClient.cs`; `Assets/UGS/Scripts/PlayerData/PlayerDataManager.cs` + `PlayerDataManagerClient.cs`; `Assets/UGS/Scripts/Config/UGSConstants.cs` |
| Cloud Code | `Assets/UGS/CloudCode/TempleRunUGSCloud~/` (.NET Cloud Code project: models + 15 services) |
| **Shared/Common** | |
| Auto-Event Base | `Assets/_Common/Events/AutoEventFlowBase.cs` (empty placeholder — see note above) |
| Shared Config | `Assets/_Common/Config/DifficultyConfig.cs` (namespace `CrawfisSoftware.Config` — the LIVE difficulty config) |
| Test Utilities | `Assets/_Common/Test/Test_AutoFireEvent.cs`, `Test_AutoFireEventOnStart.cs`, `Test_SubmitLeaderboardScore.cs` |
| Utilities | `Assets/_Common/Utility/Logger.cs`, `EventLoggerDump.cs`, `DebugEventFileLogger.cs`, `DebugLog.cs`, `TimedEvent.cs`, `TextureExtensions.cs`; `Assets/_Common/Events/EventHistory.cs` |
| Vendored | `Assets/ThirdParty/CrawfisSoftware/` (Random providers used by `Blackboard`, editor tools incl. Play Scene 0 Always); `Assets/Blocks/` (Unity Blocks samples — beware duplicate class names like `AchievementsPrefab`, `PlayerSignInController`); `Assets/CloudCode/` (Blocks cloud-code modules + generated bindings) |

## Gotchas and Warnings

### Event Subscriptions
- **ALWAYS** unsubscribe in `OnDestroy()` - failure causes errors after scene unload
- Event handler signature: `(string eventName, object sender, object data)`
- Cast data explicitly: `var score = (float)data;` or `var segment = (TrackSegmentInfo)data;`
  (the `ActiveTrackChanging` payload — see `TurnController.cs`)

### Scene Loading
- All scenes load **additively** from the persistent Boot scene
- **Never** use `LoadSceneMode.Single` unless intentionally resetting everything
- "Play Scene 0 Always" setting resets on Unity restart - re-enable it

### Auto-Event Flow
- Auto-chained events publish synchronously, inside the source event's publish call —
  there is no delay mechanism
- Circular dependencies will cause infinite loops - verify mappings with `/audit-events`
- Some events are intentionally NOT auto-chained (documented in comments)

### Singletons
- `Blackboard.Instance` - Global game state
- `EventsPublisher*.Instance` - Event buses
- Only access after `Awake()` has run (use `[DefaultExecutionOrder(-10000)]` on publishers)

### Design Notes
- Turn distance is a difficulty setting (`DifficultyConfig.SafePreTurnDistance`, consumed
  by `TurnController`): on easy difficulties the player may turn before the visual
  intersection. Early turns are not a bug — check the difficulty config before "fixing"
  them.

### Dead / Placeholder Files (don't be misled by grep hits)
- `Assets/_Common/Events/AutoEventFlowBase.cs` - empty file (the consolidation exercise
  noted above)
- `Assets/GameFlow/Scripts/Config/BlackboardGameFlow.cs` - fully commented out; there is
  no GameFlow blackboard — `Blackboard` lives in TempleRun
- `Assets/TempleRun/Scripts/Config/DifficultyConfig.cs` - fully commented out; the live
  class is `Assets/_Common/Config/DifficultyConfig.cs` (namespace `CrawfisSoftware.Config`)
- `Assets/UGS/Scripts/Initialization/Unused/` - dead code kept for reference

## Testing

### Test Without UGS
1. Open `Assets/UGS/Scenes/Boot/0_BootStrap` (the build entry scene)
2. Disable the `Load_UGS_Init` GameObject
3. Play

Or open the game-only bootstrap directly: `Assets/GameFlow/Scenes/Boot/0_BootStrap_Game_Only`.

### Enable Event Logging
`CrawfisSoftware > Events > Log Events` (or add a `DebugEventFileLogger` for a file dump)

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
3. Follow the `GameFlowUIPanelController` / `*PanelController` pattern — the UI was
   migrated from UIDocument to Panel Renderer (see
   `docs/playbooks/uidocument-to-panel-renderer.md`)
4. If adding new panel states, use **`/add-event`** to add show/hide events to `GameFlowEvents`

## Folder Structure

The codebase is organized into **four primary domains** with clear separation of concerns,
plus vendored packages outside the domain rule:

```
Assets/
├── _Common/                          # Shared infrastructure
│   ├── Config/                       # DifficultyConfig (domain-neutral, namespace CrawfisSoftware.Config — the LIVE one)
│   ├── Events/                       # AutoEventFlowBase (empty placeholder), EventHistory
│   ├── Test/                         # Test_AutoFireEvent, Test_AutoFireEventOnStart, Test_SubmitLeaderboardScore
│   └── Utility/                      # Logger, EventLoggerDump, DebugEventFileLogger, DebugLog, TimedEvent, TextureExtensions
│
├── GameFlow/                         # Application lifecycle domain
│   ├── Scripts/
│   │   ├── Events/                   # GameFlowEvents, EventsPublisherGameFlow, GameFlowAutoEventFlow
│   │   ├── TempleRunSpecific/        # TempleRunGameFlowBridge (TempleRun <-> GameFlow + TempleRun -> UGS passthrough)
│   │   ├── Config/                   # GameState, GameConstants, PlayerPrefKeys, LevelConfig(+Applier), LevelRegistry,
│   │   │                             #   LevelProgressManager/Data (BlackboardGameFlow is dead code)
│   │   ├── GameControl/              # QuitController, UnloadNonActiveScenes, LoadSceneAdditively
│   │   ├── UI/                       # GameFlowUIPanelController, MainMenu(+Panel)Controller, LevelSelector(+Panel)Controller
│   │   └── SceneManagement/          # DynamicLevelSceneLoader, LoadSceneAfterGameControlEvent, FireEventAfterSceneLoads,
│   │                                 #   FireEventWhenSceneCloses, CloseSceneOnEvent
│   ├── Scenes/Boot/                  # 0_BootStrap_Game_Only, Game_Boot_0_Initialization, Game_Boot_0_Test_Initialization,
│   │                                 #   Game_Boot_1_UI, Game_Boot_2_Play
│   ├── Audio/                        # UI sound effects
│   └── UI Toolkit/                   # UXML, USS for GameFlow UI
│
├── TempleRun/                        # Gameplay domain
│   ├── Scripts/
│   │   ├── Events/                   # TempleRunEvents, UserInitiatedEvents, both publishers, TempleRunAutoEventFlow,
│   │   │                             #   Input2TempleRunAutoEventBridge
│   │   ├── Config/                   # Blackboard, TempleRunGameConfig, GameDifficultyManager, per-mechanic configs, SpawnPrefabRegistry
│   │   ├── Player/                   # Turn/Jump/Slide/Dash/Lane/Life controllers, collision detectors, countdown, distance,
│   │   │                             #   pause, teleport, AI
│   │   ├── PowerUps/                 # IPowerUpEffect strategy: PowerUpEffectBase + five concrete effects
│   │   ├── Track/                    # TrackManager (+variants), PathProvider, SegmentTransitionController, spawners, SO classes,
│   │   │   │                         #   TrackLibraryLoader, TrackSegmentLibrary
│   │   │   ├── Geometry/             # IPathSegmentBuilder, AxisAligned90Builder, ArcTurnBuilder, PathPose/PathSpan
│   │   │   └── Selection/            # ISegmentSelector/ISegmentPool + WeightedDifficulty/AuthoredSequence selectors
│   │   ├── TrackVisuals/             # PrefabSpawnerAbstract; SimplePlane/ and Voxels/ spawners
│   │   ├── Input/                    # Movement/Swipe/Dash/Accelerometer/PauseQuit actions; generated GameControls + LeftRightJumpSlide
│   │   ├── UI/                       # GUIController (distance HUD), CountdownUIController
│   │   ├── Audio/                    # TurnAudioFeedback, Metronome
│   │   ├── Animation/                # CapsuleAnimationLink
│   │   └── GameTime.cs               # Pausable gameplay clock (singleton)
│   ├── Scenes/                       # Gameplay/: TempleRunGameplay, TempleRunTrackPCG, TempleRunTrackVisuals, TempleRunPlayerVisuals,
│   │                                 #   TempleRunObstacles, TempleRunCollectables, TempleRunEnvironment, TempleRunSfx,
│   │                                 #   TempleRunGuiOverlay; PrefabScene (authoring-only)
│   ├── Graphics/  Audio/  Prefabs/  Materials/
│   ├── Scriptables/                  # Per-mechanic config assets; Track/ (Segments/*.asset, TrackSegmentRegistry, TrackLevel_01..05,
│   │                                 #   TrackLevelRegistry); Levels/ (LevelConfig assets + LevelRegistry)
│   ├── UI Toolkit/                   # UXML, USS for gameplay UI
│   └── Editor/                       # TrackDataImporter (one-shot JSON -> SO converter)
│
├── UGS/                              # Unity Gaming Services domain
│   ├── Scripts/
│   │   ├── Events/                   # UGS_EventsEnum, EventsPublisherUGS, UGSAutoEventFlow, UGSGameFlowBridge
│   │   ├── Initialization/           # GameManagerUGS, PlayerAuthenticationManager, UGS_State, LocalStorageSystem,
│   │   │                             #   NetworkConnectivityHandler, UnityEventsToEventsPublisher (Unused/ = dead code)
│   │   ├── Authentication/           # PlayerSignInController
│   │   ├── RemoteConfig/             # RemoteConfigManager, GameBalance(+Manager), FeatureFlags(+Manager),
│   │   │                             #   CampaignEventConfig(+Manager), DifficultyObserver, LocalDifficultySettingsProvider,
│   │   │                             #   App/UserAttributes, RemoteConfigConstants
│   │   ├── Leaderboard/              # LeaderboardController, LeaderboardPlayerController
│   │   ├── Achievements/             # AchievementsPrefab, DistanceBasedAchievements, CoinBasedAchievements
│   │   ├── Economy/                  # PlayerEconomyManager(+Client)
│   │   ├── PlayerData/               # PlayerDataManager(+Client)
│   │   └── Config/                   # UGSConstants
│   ├── Scenes/
│   │   ├── Boot/                     # 0_BootStrap (ENTRY, build index 0), UGS_Boot_0_Initialization, UGS_Boot_1_RemoteConfig,
│   │   │                             #   UGS_Boot_2_Authentication, UGS_Boot_3_Achievements, UGS_Boot_4_Leaderboards
│   │   ├── Test/                     # 0_BootStrap_UGS_Only, DummyGame_Boot_0_Initialization, Test_SubmitScoreAndEnd,
│   │   │                             #   UGS_Boot_0_Test_Init_UGS_Only
│   │   └── UGS/                      # Achievements, AchievementNotifications, Leaderboards
│   ├── CloudCode/TempleRunUGSCloud~/ # .NET Cloud Code project (models + 15 services)
│   ├── Editor/                       # RemoteConfig editor data
│   └── Prefabs/
│
├── Blocks/                           # Vendored Unity Blocks samples (Achievements, Leaderboards, PlayerAccount, Common) —
│                                     #   own asmdefs, duplicate class names; not part of the four domains
├── CloudCode/                        # Second cloud-code root: BlocksAdminModule~/, BlocksGameModule~/, GeneratedModuleBindings/
├── ThirdParty/CrawfisSoftware/       # Vendored utilities: Random providers (Blackboard depends on them), editor tools
│                                     #   (Play Scene 0 Always, screenshots, dev-build toggle)
└── (also: LevelPlay/, MobileDependencyResolver/, Push Notifications/, AddressableAssetsData/,
     Prefabs/, Resources/, UI Toolkit/, Settings/ [build profiles])
```

### Domain Responsibilities

- **_Common**: Shared base classes and utilities used across all domains
- **GameFlow**: Application lifecycle - boot, initialization, menus, level select, pause, quit, scene management
- **TempleRun**: Gameplay mechanics - player movement, track generation, power-ups, input, audio
- **UGS**: Unity Gaming Services - authentication, leaderboards, achievements, remote config, economy, player data
- **Vendored** (Blocks, CloudCode bindings, ThirdParty, LevelPlay, …): sample and utility code outside the domain rule

### Event Flow Architecture

```
USER INPUT (UserInitiatedEvents in TempleRun)
    ↓ (via Input2TempleRunAutoEventBridge)
TEMPLERUN GAMEPLAY (TempleRunEvents)
    ↓ (via TempleRunGameFlowBridge in GameFlow) ──→ TempleRun → UGS passthrough
GAMEFLOW SESSION (GameFlowEvents)                   (DistanceUpdated, CoinCollected —
    ↓ (via UGSGameFlowBridge in UGS)                 a third dictionary, same bridge file)
UGS SERVICES (UGS_EventsEnum)
```
