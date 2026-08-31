# CLAUDE.md - AI Assistant Guide for RunnerUGS

This file is the concrete working guide for AI assistants — **any** AI assistant or coding
agent, not just Claude — working with the RunnerUGS codebase. Start with
[AGENTS.md](AGENTS.md) for how to approach work here; this file holds the rules,
conventions, and paths. For detailed architecture diagrams, visual walkthroughs, and
complete documentation, see [README.md](README.md).

> Sibling repo: this is the Unity-Gaming-Services variant of
> [EndlessRunnerTemplate](https://github.com/crawfis/EndlessRunnerTemplate). Both repos now
> resolve the same EventsPublisher package and use the same static `EventsFor<T>` buses, so
> code and guidance port between them directly. The differences that remain are real ones:
> this repo has a fourth domain (UGS) and six dispatch classes rather than four.

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
                    All five domain enums are marked [EventEnum], so the menu sweeps and
                    lists them in EDIT MODE — per domain: prefix, enum, member / payload /
                    sticky / replay counts. Three live in Assets (GameFlow, TempleRun,
                    UserInitiated); GameServiceEvents and UGS_EventsEnum come from packages
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
| Contract Events | `Runtime/GameServiceEvents.cs` in the **`com.crawfissoftware.contracts`** package — the vocabulary the game and the services layer share |
| UGS Events | `Runtime/Events/UGS_EventsEnum.cs` in the **`com.crawfissoftware.ugs`** package. Read-only here: edit it in the [EventDrivenUGS](https://github.com/crawfis/EventDrivenUGS) repo |
| UGS Glue | `Assets/UGSGlue/` — `UGSGameFlowBridge.cs`, `TempleRunUGSBridge.cs`, `Test_SubmitLeaderboardScore.cs`, `UGS_Glue.unity` (build index 1). This game's half of the contract |
| Event Buses | `EventsFor<T>` from the `com.crawfissoftware.eventspublisher` package, aliased per file as `GameFlowBus` / `TempleRunBus` / `UserInputBus` / `GameServiceBus` / `UGSBus`. The buses are static — there are no `EventsPublisher*` singletons and no scene object hosting them |
| Auto-Event Flow | `Assets/GameFlow/Scripts/Events/GameFlowAutoEventFlow.cs`, `Assets/TempleRun/Scripts/Events/TempleRunAutoEventFlow.cs`, and `Runtime/Events/UGSAutoEventFlow.cs` in the UGS package |
| Cross-Domain Bridges | `Assets/GameFlow/Scripts/TempleRunSpecific/TempleRunGameFlowBridge.cs` (TempleRun ↔ GameFlow), `Assets/TempleRun/Scripts/Events/Input2TempleRunAutoEventBridge.cs` (input → gameplay), `Assets/UGSGlue/UGSGameFlowBridge.cs` (GameFlow ↔ GameServiceEvents), `Assets/UGSGlue/TempleRunUGSBridge.cs` (gameplay → GameServiceEvents, one-way), `Runtime/Events/GameServiceEventsUGSBridge.cs` in the UGS package (GameServiceEvents ↔ UGS) |
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
| `Assets/TempleRun/**/*.cs` (non-bridge) | `TempleRunEvents` only |
| `Assets/GameFlow/**/*.cs` (non-bridge) | `GameFlowEvents` only |
| UGS package `Runtime/**/*.cs` (non-bridge) | `UGS_EventsEnum` only |
| `Input2TempleRunAutoEventBridge.cs` | `UserInitiatedEvents` + `TempleRunEvents` (bridge duty) |
| `TempleRunGameFlowBridge.cs` | `TempleRunEvents` + `GameFlowEvents` (bridge duty) |
| `Assets/UGSGlue/TempleRunUGSBridge.cs` | `TempleRunEvents` + `GameServiceEvents` (bridge duty; one-way, gameplay -> contract) |
| `Assets/UGSGlue/UGSGameFlowBridge.cs` | `GameFlowEvents` + `GameServiceEvents` (bridge duty) |
| `GameServiceEventsUGSBridge.cs` (UGS package) | `GameServiceEvents` + `UGS_EventsEnum` (bridge duty) |

**The game and UGS no longer name each other's events at all.** They meet in the middle at
`GameServiceEvents`, a third enum in `com.crawfissoftware.contracts` that neither owns. The game's half
of that translation lives in `Assets/UGSGlue/`; the services half is `GameServiceEventsUGSBridge` inside
the UGS package. That is what lets either side be replaced without the other being edited.

**Violations — what NOT to do:**
- TempleRun scripts subscribing to or publishing `GameFlowEvents` (e.g., `GameFlowBus.Subscribe(GameFlowEvents.GameStarted, ...)` in a TempleRun file)
- GameFlow scripts subscribing to or publishing `TempleRunEvents` (e.g., `TempleRunBus.Publish(TempleRunEvents.CountdownTick, ...)` in a GameFlow file)
- UGS package code naming `GameFlowEvents` or `TempleRunEvents` at all — it cannot even see them
- Game code naming `UGS_EventsEnum` directly (go through `GameServiceEvents` and the UGSGlue bridges)

**How to fix a violation:** If TempleRun code needs to react to a GameFlow event, add a bridge mapping in `TempleRunGameFlowBridge.cs` that translates the GameFlow event into a TempleRun event, then subscribe to the TempleRun event in your TempleRun code. For anything crossing to UGS, add the mapping in `Assets/UGSGlue/` against `GameServiceEvents` — and if the event you need does not exist, it is added to the contracts package, deliberately rarely.

> The UGS domain is no longer in this repository. It ships as three UPM packages resolved by git
> URL in `Packages/manifest.json` — `com.crawfissoftware.contracts`, `.common` and `.ugs`, from
> [EventDrivenUGS](https://github.com/crawfis/EventDrivenUGS) — so their code is **read-only here**;
> change it in that repo and update the package. The game's own assemblies (`CrawfisSoftware.TempleRun`,
> `.GameFlow`, `.ThirdParty`) reference `CrawfisSoftware.Contracts` but never `CrawfisSoftware.UGS`,
> so a game-to-UGS reference is a compile error. Everything *within* a domain, and the deliberately
> asmdef-free `Assets/UGSGlue/`, is still only enforced by review and `/audit-events`; run it.

The rule's purpose is **replaceability**: a domain that talks only through events can be
swapped for a completely different implementation — or stubbed out with a trivial fake —
without touching code on the other side. This repo proves it at full scale, in both
directions: the `Test_GameOnly_Windows` build profile — equivalently, opening
`Assets/GameFlow/Scenes/Boot/0_BootStrap_Game_Only` — runs the entire game with the UGS domain
absent, and `Test_UGS_Windows` runs the UGS services against a dummy game with random scores. Domains load from their own scenes,
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

| Domain | Enum | Bus alias | Purpose | Lives in | Bridges |
|--------|------|-----------|---------|----------|---------|
| **GameFlow** | `GameFlowEvents` | `GameFlowBus` | App lifecycle: loading, menus, sessions, pause, config/difficulty, save/load, quit | `Assets/GameFlow/` | ↔ TempleRun via `TempleRunGameFlowBridge`; ↔ GameServiceEvents via `UGSGameFlowBridge` |
| **TempleRun** | `TempleRunEvents` | `TempleRunBus` | Gameplay: player lifecycle, countdown, movement, collisions, coins/power-ups, track/spline generation, teleportation | `Assets/TempleRun/` | ↔ GameFlow via `TempleRunGameFlowBridge`; → GameServiceEvents via `TempleRunUGSBridge` |
| **UserInitiated** | `UserInitiatedEvents` | `UserInputBus` | Raw input requests (turns, lanes, jump, slide, dash, pause, quit) | `Assets/TempleRun/` | → TempleRun via `Input2TempleRunAutoEventBridge` |
| **GameService** | `GameServiceEvents` | `GameServiceBus` | The game/service **contract**: score, currency total, session start/end, services status, remote config applied. Owned by neither side | `com.crawfissoftware.contracts` package | ↔ GameFlow and ← TempleRun via `Assets/UGSGlue/`; ↔ UGS via `GameServiceEventsUGSBridge` |
| **UGS** | `UGS_EventsEnum` | `UGSBus` | Unity Gaming Services: init, auth, remote config, leaderboards, achievements, **economy/currency**, rewarded ads | `com.crawfissoftware.ugs` package | ↔ GameServiceEvents via `GameServiceEventsUGSBridge` — and nothing else |

Two invariants keep this registry trustworthy:
- **Placement:** three domain enums live in `Assets/*/Scripts/Events/`; the other two come from
  packages (`GameServiceEvents` from contracts, `UGS_EventsEnum` from ugs). All five are marked
  `[EventEnum]`, so **List Domains** reports the same five as this table: a domain in one list
  and not the other is drift. The `EventsPublisher*` singleton subclasses are gone — the buses
  are static and need no scene object.
- **Registration:** `/add-event-domain` adds a row here as part of its checklist, and
  `/audit-events` flags drift between this table and the code.

## Event System Patterns

### Subscribing to Events

```csharp
private void Awake()
{
    GameFlowBus.Subscribe(
        GameFlowEvents.GameStarting,
        OnGameStarting
    );
}

private void OnDestroy()
{
    GameFlowBus.Unsubscribe(
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
GameFlowBus.Publish(
    GameFlowEvents.MainMenuShown,
    this,
    null
);

// With data payload
float score = Blackboard.Instance.DistanceTracker.DistanceTravelled;
TempleRunBus.Publish(
    TempleRunEvents.PlayerDied,
    this,
    score
);

// With a struct payload (ActiveTrackChanging carries a TrackSegmentInfo)
TempleRunBus.Publish(
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

> **Chains are declared as a flat list of pairs, not a dictionary.** All six dispatch classes
> (three auto-flows, three bridges) share one implementation in
> `Runtime/Events/AutoEventFlowBase.cs` in the common package: `EventChainDispatcher<TSource, TDest>` does
> subscribe-to-all, lookup and publish; `AutoEventFlowBase<TSource, TDest>` is the
> MonoBehaviour wrapper for a single direction. A bridge covering several directions cannot
> inherit repeatedly, so `TempleRunGameFlowBridge` (TempleRun↔GameFlow plus the TempleRun→UGS
> passthrough) holds three dispatchers and `UGSGameFlowBridge` holds two.
>
> The pair list exists so **one event may declare several consequences** — a dictionary
> allowed exactly one successor each. That ceiling never produced bugs directly; it produced
> workarounds, where a developer finding a source event's slot taken published the second
> consequence by hand inside a controller. Targets fire in declaration order, synchronously.
>
> **Never chain a `*Requested` that arrives raw from input to its `*Starting`** — chaining runs
> before any controller validates, which silently defeats cooldowns and boundary checks.

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
CrawfisSoftware.GameFlow.Events     - GameFlowEvents, GameFlowAutoEventFlow, TempleRunGameFlowBridge
CrawfisSoftware.GameFlow.UI         - GameFlow UI controllers
CrawfisSoftware.GameFlow.*          - Per-area: .GameConfig (GameConstants), .SceneManagement, .GameControl
CrawfisSoftware.TempleRun           - Gameplay logic (TempleRunEvents, Blackboard, controllers)
CrawfisSoftware.TempleRun.Events    - TempleRun auto-event flow + Input2TempleRunAutoEventBridge
CrawfisSoftware.TempleRun.GameConfig - Difficulty / game-config managers
CrawfisSoftware.Contracts           - GameServiceEvents, ServicesStatus (contracts package; the game/service contract)
CrawfisSoftware.UGS                 - Unity Gaming Services integration (ugs package: managers, initialization)
CrawfisSoftware.UGS.Events          - UGS_EventsEnum, UGSAutoEventFlow, GameServiceEventsUGSBridge (ugs package)
CrawfisSoftware.UGS.Economy         - PlayerCurrencyManager and the currency backends (ugs package)
CrawfisSoftware.UGS.Achievements    - achievements model, service and UI (ugs package)
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
| Auto-Event Flow | `Assets/GameFlow/Scripts/Events/GameFlowAutoEventFlow.cs` |
| Bridges | `Assets/GameFlow/Scripts/TempleRunSpecific/TempleRunGameFlowBridge.cs` (TempleRun ↔ GameFlow + the TempleRun → UGS passthrough) |
| Game State / Config | `Assets/GameFlow/Scripts/Config/GameState.cs`, `GameConstants.cs`, `PlayerPrefKeys.cs` |
| Level System | `Assets/GameFlow/Scripts/Config/LevelConfig.cs`, `LevelConfigApplier.cs`, `LevelRegistry.cs`, `LevelProgressManager.cs`, `LevelProgressData.cs`; assets in `Assets/TempleRun/Scriptables/Levels/` |
| UI Controllers | `Assets/GameFlow/Scripts/UI/GameFlowUIPanelController.cs` (loading / game-over overlays), `MainMenuController.cs`, `MainMenuPanelController.cs`, `LevelSelectorController.cs`, `LevelSelectorPanelController.cs` |
| Game Control | `Assets/GameFlow/Scripts/GameControl/QuitController.cs`, `UnloadNonActiveScenes.cs`, `LoadSceneAdditively.cs` |
| Scene Management | `Assets/GameFlow/Scripts/SceneManagement/DynamicLevelSceneLoader.cs`, `LoadSceneAfterGameControlEvent.cs`, `FireEventAfterSceneLoads.cs`, `FireEventWhenSceneCloses.cs`, `CloseSceneOnEvent.cs` |
| **TempleRun Domain** | |
| Event Enums | `Assets/TempleRun/Scripts/Events/TempleRunEvents.cs`, `UserInitiatedEvents.cs` |
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
| **UGS Domain** (the `com.crawfissoftware.ugs` package — read-only here) | |
| Event Enum | `Runtime/Events/UGS_EventsEnum.cs` |
| Auto-Event Flow | `Runtime/Events/UGSAutoEventFlow.cs` |
| Contract Bridge | `Runtime/Events/GameServiceEventsUGSBridge.cs` — the only place `GameServiceEvents` and `UGS_EventsEnum` are named together |
| Initialization | `Runtime/Initialization/` — `PlayerAuthenticationManager`, `UGS_State`, `LocalStorageSystem`, `NetworkConnectivityHandler`, `UnityEventsToEventsPublisher` |
| Authentication | `Runtime/Authentication/PlayerSignInController.cs`, `PlayerSignIn.cs` (the modal, named in UXML by its fully qualified type name) |
| Remote Config | `Runtime/RemoteConfig/` — `RemoteConfigManager` (the only fetch; it also publishes the `difficulty_settings` table), `App/UserAttributes`, `RemoteConfigConstants`. The typed views and `DifficultyObserver` were removed in ugs 0.5.0 — nothing constructed any of them |
| Leaderboards | `Runtime/Leaderboard/` — `LeaderboardQuery`, `LeaderboardPanel`, `LeaderboardPlayerController` |
| Achievements | `Runtime/Achievements/` — model, `Service/` (`IAchievementBackend` + Cloud Save and Cloud Code backends), `UI/`, plus `DistanceBasedAchievements` and `CoinBasedAchievements` |
| Economy | `Runtime/Economy/` — `PlayerCurrencyManager`, `PlayerCurrencyController`, and `Service/` (`ICurrencyBackend`, `EconomyCurrencyBackend`, `CloudCodeCurrencyBackend`, `CurrencyBalanceUpdate`) |
| Editor | `Editor/Achievements/` — `AchievementDefinitionCatalog` and its `.rc` exporter |
| **Game-side UGS glue** | |
| Bridges | `Assets/UGSGlue/UGSGameFlowBridge.cs` (GameFlow ↔ GameServiceEvents), `Assets/UGSGlue/TempleRunUGSBridge.cs` (gameplay → GameServiceEvents) |
| Scene | `Assets/UGSGlue/UGS_Glue.unity` (build index 1), `Test_SubmitLeaderboardScore.cs` |
| Cloud Code | `Assets/UGS/CloudCode/TempleRunUGSCloud~/` (.NET module: models + 6 services, 11 endpoints). Its `.sln`/`.csproj` are tracked, and so is `Project/Properties/PublishProfiles/FolderProfile.pubxml` — **do not delete that file**; the tooling needs it and a run without it fails. A run rewrites it with CRLF line endings and identical content, so discard that diff rather than committing it. Deleting `TestProject` means removing its entry from the `.sln` too, or the solution stops building |
| **Shared/Common** (the `com.crawfissoftware.common` package) | |
| Auto-Event Base | `Runtime/Events/AutoEventFlowBase.cs` — `EventChainDispatcher<TSource, TDest>` + `AutoEventFlowBase<TSource, TDest>`; the one dispatch implementation, shared by every flow and bridge class in both repos |
| Shared Config | `Runtime/Config/DifficultyConfig.cs` (namespace `CrawfisSoftware.Config` — the LIVE difficulty config) |
| Scene Management | `Runtime/SceneManagement/` — `LoadSceneAdditively`, `LoadSceneAfterGameControlEvent`, `CloseSceneOnEvent`, `FireEventWhenSceneCloses` |
| Test Utilities | `Runtime/Test/Test_AutoFireEvent.cs`, `Test_AutoFireEventOnStart.cs` |
| Utilities | `Runtime/Utility/` — `Logger`, `DebugEventFileLogger`, `DebugLog`, `TimedEvent`, `TextureExtensions`; `Runtime/Events/EventHistory.cs` |
| **Contract** (the `com.crawfissoftware.contracts` package) | |
| Contract | `Runtime/GameServiceEvents.cs`, `Runtime/ServicesStatus.cs` |
| Vendored | `Assets/ThirdParty/CrawfisSoftware/` (Random providers used by `Blackboard`, editor tools incl. Play Scene 0 Always); `Assets/CloudCode/GeneratedModuleBindings/` (generated Cloud Code bindings — regenerated by the tooling, no live consumer in the game) |

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
- (`AutoEventFlowBase.cs` was an empty placeholder until the dispatch consolidation; it now
  holds the shared implementation, in the common package, and every flow and bridge uses it.)
- `Assets/GameFlow/Scripts/Config/BlackboardGameFlow.cs` - fully commented out; there is
  no GameFlow blackboard — `Blackboard` lives in TempleRun
- `Assets/TempleRun/Scripts/Config/DifficultyConfig.cs` - fully commented out; the live
  class is `Runtime/Config/DifficultyConfig.cs` in the common package (namespace
  `CrawfisSoftware.Config`)
- The old `Assets/UGS/Scripts/Initialization/Unused/` dead-code folder is gone with the rest of
  `Assets/UGS/Scripts/`; it was not carried into the package

## Testing

### Test Without UGS
1. Open `Assets/GameFlow/Scenes/Boot/0_BootStrap_Game_Only`
2. Play

Or select the `Test_GameOnly_Windows` build profile, which uses that bootstrap.

> **Do not do it by disabling `Load_UGS_Init` in `0_BootStrap`.** That used to work and no longer
> does. `0_BootStrap` also carries `Load_UGS_Glue`, which loads `Assets/UGSGlue/UGS_Glue.unity`,
> and the `UGSGameFlowBridge` in that scene is the **only** publisher of
> `GameFlowEvents.GameplayReady` in this bootstrap — it fires it on
> `GameServiceEvents.ServicesStatusChanged == Ready`. With UGS init disabled that status never arrives,
> `GameplayReady` never fires, the main menu is never requested, and the boot sits on the loading
> screen. Disabling `Load_UGS_Glue` as well does not help either: then nothing publishes
> `GameplayReady` at all. The game-only bootstrap exists precisely because it wires that path
> itself: its `InitialGameReadyEvent` object carries a `Test_AutoFireEventOnStart` with
> `_eventName: GameFlowEvents/GameplayReady`, so the menu appears with no services involved.

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
8. Publish state changes as events via `TempleRunBus`
9. Keep visuals/audio separate from logic
10. **`/audit-events`** — Verify compliance

### Adding New GameFlow Feature
1. **`/list-events GameFlow`** — Review existing GameFlow events
2. **`/add-event`** — Add events to `GameFlowEvents`
3. **`/add-auto-chain`** — Wire auto-progressions
4. Implement the feature, subscribing/publishing via `GameFlowBus`
5. **`/audit-events`** — Verify compliance

### Modifying UI Panels
1. Find panel in `Assets/GameFlow/Scripts/UI/`
2. Panels subscribe to `GameFlowEvents` for show/hide
3. Follow the `GameFlowUIPanelController` / `*PanelController` pattern — the UI was
   migrated from UIDocument to Panel Renderer (see
   `docs/playbooks/uidocument-to-panel-renderer.md`)
4. If adding new panel states, use **`/add-event`** to add show/hide events to `GameFlowEvents`

## Folder Structure

The codebase is organized into **three in-repo domains** plus the UGS domain and shared
infrastructure, which now arrive as UPM packages rather than living here:

```
Assets/
├── UGSGlue/                          # This game's half of the GameServiceEvents contract (asmdef-free on purpose)
│                                     #   UGSGameFlowBridge, TempleRunUGSBridge, Test_SubmitLeaderboardScore,
│                                     #   UGS_Glue.unity (build index 1)
│
├── GameFlow/                         # Application lifecycle domain
│   ├── Scripts/
│   │   ├── Events/                   # GameFlowEvents, GameFlowAutoEventFlow
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
├── UGS/                              # What is left of the UGS domain in this repo: assets, not code
│   ├── Scenes/
│   │   ├── Boot/                     # 0_BootStrap (ENTRY, build index 0), UGS_Boot_0_Initialization,
│   │   │                             #   UGS_Boot_1_RemoteConfig, UGS_Boot_2_Authentication,
│   │   │                             #   UGS_Boot_3_Achievements, UGS_Boot_4_Leaderboards
│   │   ├── Test/                     # 0_BootStrap_UGS_Only, DummyGame_Boot_0_Initialization, Test_SubmitScoreAndEnd
│   │   └── UGS/                      # Achievements, AchievementNotifications, Leaderboards
│   ├── CloudCode/TempleRunUGSCloud~/ # .NET Cloud Code module (models + 16 services)
│   ├── Economy/                      # COIN.ecc - the Economy currency definition; deploy it from the
│   │                                 #   Deployment window. The id comes from the filename
│   ├── Editor/                       # RemoteConfig editor data
│   └── Prefabs/                      # AchievementsPrefab, AchievementsNotificationPrefab, LeaderboardPanel
│                                     #   (the scripts they instance come from the UGS package)
│
├── CloudCode/                        # GeneratedModuleBindings/ only - the two Blocks module references were deleted
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
- **Vendored** (`UGS/ThirdParty/Blocks`, CloudCode bindings, ThirdParty, LevelPlay, …): sample and utility code outside the domain rule

### Event Flow Architecture

```
USER INPUT (UserInitiatedEvents, in TempleRun)
    ↓  Input2TempleRunAutoEventBridge
TEMPLERUN GAMEPLAY (TempleRunEvents)
    ├─→ TempleRunGameFlowBridge ──→ GAMEFLOW SESSION (GameFlowEvents)
    │                                     ↕  Assets/UGSGlue/UGSGameFlowBridge
    └─→ Assets/UGSGlue/TempleRunUGSBridge ──→ THE CONTRACT (GameServiceEvents)
                                                  ↕  GameServiceEventsUGSBridge   (UGS package)
                                              UGS SERVICES (UGS_EventsEnum)
```

Neither end names the other. The game speaks `GameServiceEvents`; the services layer speaks
`GameServiceEvents`; the enum belongs to neither and lives in its own package. Everything
game-specific — that a "score" is metres run, that a "currency total" is coins — is translated in
`Assets/UGSGlue/`.

**Worked example, one coin:** `CoinCollectionController` publishes `TempleRunEvents.CoinCollected`
carrying the run's **running total**. `TempleRunUGSBridge` maps it to
`GameServiceEvents.CurrencyTotalChanged`; `GameServiceEventsUGSBridge` maps that to
`UGS_EventsEnum.UGS_CoinUpdated`. `PlayerCurrencyController` remembers the number but does not
bank it yet. At the end of a run `GameFlowEvents.GameEnding` becomes
`GameServiceEvents.SessionEnding`, which fans out to **two** UGS events — `ScoreUpdating` (submit the leaderboard score) and
`CurrencySyncRequested` (bank the coins). The banked lifetime balance comes back as
`CurrencyBalanceChanged`, which is what `CoinBasedAchievements` reads — so a coin achievement
means a lifetime total, not one run's.

Two links in that chain live outside C# and fail silently, so check them before debugging code:
`PlayerCurrencyController` must be in a loaded scene — it sits on `GameFlow/PlayerCurrency` in
`UGS_Boot_0_Initialization`, and it is the **only** subscriber to `CurrencySyncRequested`, so
without it every event above still fires and nothing is ever written — and a `COIN` currency must
exist in the environment being signed in to, which `Assets/UGS/Economy/COIN.ecc` defines and the
Deployment window publishes. Economy configuration is per-environment.
