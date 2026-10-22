# Codebase Map and Key Files

Where things live: the commands, critical paths, namespaces, every key file by area, and the
folder tree. The rules are in [AGENTS.md](../AGENTS.md); the design is in
[ARCHITECTURE.md](ARCHITECTURE.md).

## Quick reference

```
Play in Editor:     CrawfisSoftware > Play Scene 0 Always (toggle ON)
Entry Scene:        Assets/UGS/Scenes/Boot/0_BootStrap (build index 0);
                    game-only variant: Assets/GameFlow/Scenes/Boot/0_BootStrap_Game_Only
Event Logging:      CrawfisSoftware > Events > Log Events   (same menu: Clear Now,
                    List Current Subscribers, Clear Events on Exiting Play Mode);
                    or add a DebugEventFileLogger for a file dump
List Domains:       CrawfisSoftware > Events > List Domains (EventsPublisher 2.5.0+).
                    All six domain enums are marked [EventEnum], so the menu sweeps and
                    lists them in EDIT MODE — per domain: prefix, enum, member / payload /
                    sticky / replay counts. Four live in Assets (GameFlow, TempleRun,
                    UserInitiated, Countdown); GameServiceEvents and UGS_EventsEnum come
                    from packages
Track Import:       CrawfisSoftware > Track > Import JSON -> ScriptableObjects (one-shot)
Cloud Code:         Services > Cloud Code > Generate All Modules Bindings
Build Profiles:     File > Build Profiles > [Windows | Test_UGS_Windows | Test_GameOnly_Windows]
```

## Critical paths

| Purpose | Path |
|---------|------|
| Entry Scene | `Assets/UGS/Scenes/Boot/0_BootStrap` (build index 0); game-only: `Assets/GameFlow/Scenes/Boot/0_BootStrap_Game_Only` |
| GameFlow Events | `Assets/GameFlow/Scripts/Events/GameFlowEvents.cs` |
| TempleRun Events | `Assets/TempleRun/Scripts/Events/TempleRunEvents.cs`, `UserInitiatedEvents.cs` |
| Countdown Events | `Assets/Countdown/Scripts/Events/CountdownEvents.cs` (its own assembly, `CrawfisSoftware.Countdown.asmdef`) |
| Contract Events | `Runtime/GameServiceEvents.cs` in the **`com.crawfissoftware.contracts`** package — the vocabulary the game and the services layer share |
| UGS Events | `Runtime/Events/UGS_EventsEnum.cs` in the **`com.crawfissoftware.ugs`** package. Read-only here: edit it in the [EventDrivenUGS](https://github.com/crawfis/EventDrivenUGS) repo |
| UGS Glue | `Assets/UGSGlue/` — `UGSGameFlowBridge.cs`, `TempleRunUGSBridge.cs`, `Test_SubmitLeaderboardScore.cs`, `UGS_Glue.unity` (build index 1). This game's half of the contract |
| Event Buses | `EventsFor<T>` from the `com.crawfissoftware.eventspublisher` package, aliased per file as `GameFlowBus` / `TempleRunBus` / `UserInputBus` / `CountdownBus` / `GameServiceBus` / `UGSBus`. The buses are static — there are no `EventsPublisher*` singletons and no scene object hosting them |
| Auto-Event Flow | `Assets/GameFlow/Scripts/Events/GameFlowAutoEventFlow.cs`, `Assets/TempleRun/Scripts/Events/TempleRunAutoEventFlow.cs`, `Assets/Countdown/Scripts/Events/CountdownAutoEventFlow.cs`, and `Runtime/Events/UGSAutoEventFlow.cs` in the UGS package |
| Cross-Domain Bridges | `Assets/GameFlow/Scripts/TempleRunSpecific/TempleRunGameFlowBridge.cs` (TempleRun ↔ GameFlow), `Assets/TempleRun/Scripts/Events/Input2TempleRunAutoEventBridge.cs` (input → gameplay), `Assets/GameFlow/Scripts/CountdownSpecific/CountdownGameFlowBridge.cs` (GameFlow → Countdown), `Assets/Countdown/Scripts/TempleRunSpecific/Countdown2TempleRunBridge.cs` (Countdown → gameplay), `Assets/UGSGlue/UGSGameFlowBridge.cs` (GameFlow ↔ GameServiceEvents), `Assets/UGSGlue/TempleRunUGSBridge.cs` (gameplay → GameServiceEvents, one-way), `Runtime/Events/GameServiceEventsUGSBridge.cs` in the UGS package (GameServiceEvents ↔ UGS) |
| Game State | `Assets/GameFlow/Scripts/Config/GameState.cs`, `Assets/TempleRun/Scripts/Config/Blackboard.cs` |

## Namespaces
```
CrawfisSoftware.Events              - UserInitiatedEvents (+ the EventsPublisher core and the common package's dispatcher)
CrawfisSoftware.GameFlow.Events     - GameFlowEvents, GameFlowAutoEventFlow, TempleRunGameFlowBridge, CountdownGameFlowBridge
CrawfisSoftware.GameFlow.UI         - GameFlow UI controllers
CrawfisSoftware.GameFlow.*          - Per-area: .GameConfig (GameConstants), .SceneManagement, .GameControl
CrawfisSoftware.TempleRun           - Gameplay logic (TempleRunEvents, Blackboard, controllers)
CrawfisSoftware.TempleRun.Events    - TempleRun auto-event flow + Input2TempleRunAutoEventBridge
CrawfisSoftware.TempleRun.GameConfig - Difficulty / game-config managers
CrawfisSoftware.Countdown           - Ceremony logic (CountdownController)
CrawfisSoftware.Countdown.Events    - CountdownEvents, CountdownAutoEventFlow, Countdown2TempleRunBridge
CrawfisSoftware.Countdown.UI        - CountdownUIController
CrawfisSoftware.Contracts           - GameServiceEvents, ServicesStatus (contracts package; the game/service contract)
CrawfisSoftware.UGS                 - Unity Gaming Services integration (ugs package: managers, initialization)
CrawfisSoftware.UGS.Events          - UGS_EventsEnum, UGSAutoEventFlow, GameServiceEventsUGSBridge (ugs package)
CrawfisSoftware.UGS.Economy         - PlayerCurrencyManager and the currency backends (ugs package)
CrawfisSoftware.UGS.Achievements    - achievements model, service and UI (ugs package)
CrawfisSoftware.Config              - Shared, domain-neutral config (the LIVE DifficultyConfig in _Common)
CrawfisSoftware.Utility             - Shared utilities
```

## Key files reference

| Category | Files |
|----------|-------|
| **GameFlow Domain** | |
| Event Enums | `Assets/GameFlow/Scripts/Events/GameFlowEvents.cs` |
| Auto-Event Flow | `Assets/GameFlow/Scripts/Events/GameFlowAutoEventFlow.cs` |
| Bridges | `Assets/GameFlow/Scripts/TempleRunSpecific/TempleRunGameFlowBridge.cs` (TempleRun ↔ GameFlow; the old TempleRun → UGS passthrough now lives in `Assets/UGSGlue/TempleRunUGSBridge.cs`), `Assets/GameFlow/Scripts/CountdownSpecific/CountdownGameFlowBridge.cs` (GameFlow → Countdown) |
| Game State / Config | `Assets/GameFlow/Scripts/Config/GameState.cs`, `GameConstants.cs` |
| Level System | `Assets/GameFlow/Scripts/Config/LevelConfig.cs`, `LevelConfigApplier.cs`, `LevelRegistry.cs`, `LevelProgressManager.cs`, `LevelProgressData.cs`; assets in `Assets/TempleRun/Scriptables/Levels/` |
| UI Controllers | `Assets/GameFlow/Scripts/UI/GameFlowUIPanelController.cs` (loading / game-over overlays), `MainMenuController.cs`, `MainMenuPanelController.cs`, `LevelSelectorController.cs`, `LevelSelectorPanelController.cs`, `CoinBalanceHUDController.cs` |
| Game Control | `Assets/GameFlow/Scripts/GameControl/QuitController.cs`, `UnloadNonActiveScenes.cs` (`LoadSceneAdditively` comes from the common package) |
| Scene Management | `Assets/GameFlow/Scripts/SceneManagement/DynamicLevelSceneLoader.cs`, `FireEventAfterSceneLoads.cs` (`LoadSceneAfterGameControlEvent`, `FireEventWhenSceneCloses`, `CloseSceneOnEvent` come from the common package) |
| **TempleRun Domain** | |
| Event Enums | `Assets/TempleRun/Scripts/Events/TempleRunEvents.cs`, `UserInitiatedEvents.cs` |
| Auto-Event Flow | `Assets/TempleRun/Scripts/Events/TempleRunAutoEventFlow.cs`, `Input2TempleRunAutoEventBridge.cs` (input → gameplay) |
| Config | `Assets/TempleRun/Scripts/Config/Blackboard.cs`, `TempleRunGameConfig.cs`, `GameDifficultyManager.cs`, `DifficultySettings.cs`, `SetGameDifficulty.cs`, `LoadDefaultGameConfigs.cs`, `SpawnPrefabRegistry.cs`, `TempleRunConstants.cs`, `PlayerPrefKeys.cs`, per-mechanic configs (`CoinConfig.cs`, `DashConfig.cs`, `JumpConfig.cs`, `LaneConfig.cs`, `SlideConfig.cs`, `PowerUpDefinition.cs`, `PowerUpType.cs`) |
| Player Controllers | `Assets/TempleRun/Scripts/Player/TurnController.cs`, `JumpController.cs`, `SlideController.cs`, `DashController.cs`, `LaneChangeController.cs`, `PlayerLifeController.cs`, `PowerUpBuffController.cs`, `DistanceController.cs`, `MoveCharacterByDistance.cs`, `PauseController.cs`, `PlayerPauseController.cs`, `AIController.cs` |
| Player Support | collision detectors (`ObstacleCollisionDetector.cs`, `CollectableCollisionDetector.cs`, `TurnCollisionDetector.cs`), `CoinCollectionController.cs`, motion shaping (`JumpArcController.cs`, `SlideArcController.cs`, `DashSpeedController.cs`, `LaneOffsetController.cs`), failure/teleport (`PlayerFailedController.cs`, `PlayerFailureAutoTurnController.cs`, `TeleportController.cs`, `CharacterTeleporter.cs`); `Assets/TempleRun/Scripts/GameTime.cs` |
| Power-Up Effects | `Assets/TempleRun/Scripts/PowerUps/IPowerUpEffect.cs`, `PowerUpEffectBase.cs`, `SpeedBoostEffect.cs`, `ScoreMultiplierEffect.cs`, `CoinMagnetEffect.cs`, `CoinDoublerEffect.cs`, `ShieldEffect.cs` |
| Track Generation | `Assets/TempleRun/Scripts/Track/TrackManager.cs` (+ `TrackManagerAbstract.cs`, `TrackManagerForTiles.cs`, `TrackManagerList.cs`), `PathProvider.cs`, `SegmentTransitionController.cs`, `SegmentAdvanceTrigger.cs`, `TrackSegmentLibrary.cs`, `TrackLibraryLoader.cs`, `TrackSegmentInfo.cs`, `Direction.cs`, `DistanceTracker.cs`, `DistanceInterestService.cs` |
| Segment Selection | `Assets/TempleRun/Scripts/Track/Selection/` — `ISegmentSelector.cs`, `ISegmentPool.cs`, `WeightedDifficultySelector.cs` (default), `AuthoredSequenceSelector.cs` |
| Track Geometry | `Assets/TempleRun/Scripts/Track/Geometry/` — `IPathSegmentBuilder.cs`, `AxisAligned90Builder.cs`, `ArcTurnBuilder.cs`, `PathPose.cs`, `PathSpan.cs`, `PathSegmentResult.cs`, `CardinalDirections.cs`; `SegmentGeometryData.cs` |
| Spawners | `Assets/TempleRun/Scripts/Track/SpawnerBase.cs`, `CoinSpawner.cs`, `ObstacleSpawner.cs`, `PowerUpSpawner.cs`, `PowerUpIdentifier.cs` |
| Track Visuals | `Assets/TempleRun/Scripts/TrackVisuals/PrefabSpawnerAbstract.cs`, `SimplePlane/SplinePrefabSpawner.cs`, `SimplePlane/TextureScaler.cs`, `Voxels/VoxelPrefabSpawner.cs` |
| Track Data | `Assets/TempleRun/Scriptables/Track/` — `Segments/*.asset` (one `TrackSegmentSO` per segment), `TrackSegmentRegistry.asset`, `TrackLevel_01..05_*.asset`, `TrackLevelRegistry.asset` |
| Gameplay UI | `Assets/TempleRun/Scripts/UI/GUIController.cs` (distance HUD; the countdown overlay lives in the Countdown domain) |
| Audio / Animation | `Assets/TempleRun/Scripts/Audio/` (`TurnAudioFeedback.cs`, `Metronome.cs`, …); `Assets/TempleRun/Scripts/Animation/CapsuleAnimationLink.cs` |
| Input | `Assets/TempleRun/Scripts/Input/MovementInputActions.cs`, `SwipeDetectorActions.cs`, `DashInputActions.cs`, `AccelerometerInputActions.cs`, `PauseQuitInputActions.cs`; `GameControls.cs` + `LeftRightJumpSlide.cs` are source-generated from the `.inputactions` assets — regenerate, don't hand-edit |
| Editor Tools | `Assets/TempleRun/Editor/TrackDataImporter.cs` (one-shot JSON -> SO importer) |
| **Countdown Domain** | |
| Assembly | `Assets/Countdown/CrawfisSoftware.Countdown.asmdef` — references `CrawfisSoftware.TempleRun`; `CrawfisSoftware.GameFlow` references it (GameFlow → Countdown → TempleRun) |
| Event Enum | `Assets/Countdown/Scripts/Events/CountdownEvents.cs` (`CountdownStartRequested` 0 … `CountdownEnded` 5) |
| Event Bus | `EventsFor<CountdownEvents>`, aliased as `CountdownBus` |
| Auto-Event Flow | `Assets/Countdown/Scripts/Events/CountdownAutoEventFlow.cs` (`StartRequested -> Starting`, `Ending -> Ended`) |
| Bridges | `Assets/GameFlow/Scripts/CountdownSpecific/CountdownGameFlowBridge.cs` (GameFlow -> Countdown), `Assets/Countdown/Scripts/TempleRunSpecific/Countdown2TempleRunBridge.cs` (Countdown -> TempleRun) |
| Ceremony / UI | `Assets/Countdown/Scripts/CountdownController.cs`, `Assets/Countdown/Scripts/UI/CountdownUIController.cs`, `Assets/Countdown/UI Toolkit/Countdown.uxml` (both controllers sit in the `TempleRunGameplay` scene; the flow and both bridges sit on the `CountdownDomain` object in `Game_Boot_2_Play`) |
| **UGS Domain** (the `com.crawfissoftware.ugs` package — read-only here) | |
| Event Enum | `Runtime/Events/UGS_EventsEnum.cs` |
| Auto-Event Flow | `Runtime/Events/UGSAutoEventFlow.cs` |
| Contract Bridge | `Runtime/Events/GameServiceEventsUGSBridge.cs` — the only place `GameServiceEvents` and `UGS_EventsEnum` are named together |
| Initialization | `Runtime/Initialization/` — `PlayerAuthenticationManager`, `UGS_State`, `NetworkConnectivityHandler`, `UnityEventsToEventsPublisher`; also `Runtime/Config/UGSConstants.cs` and `Runtime/UI/` (theme, panel settings) |
| Authentication | `Runtime/Authentication/PlayerSignInController.cs`, `PlayerSignIn.cs` (the modal, named in UXML by its fully qualified type name) |
| Remote Config | `Runtime/RemoteConfig/` — `RemoteConfigManager` (the only fetch; it also publishes the `difficulty_settings` table), `App/UserAttributes`, `RemoteConfigConstants`. The typed views and `DifficultyObserver` were removed in ugs 0.5.0 — nothing constructed any of them |
| Leaderboards | `Runtime/Leaderboard/` — `LeaderboardQuery`, `LeaderboardPanel`, `LeaderboardPlayerController` |
| Achievements | `Runtime/Achievements/` — model, `Service/` (`IAchievementBackend` + Cloud Save and Cloud Code backends), `UI/`, plus `DistanceBasedAchievements` and `CoinBasedAchievements` |
| Economy | `Runtime/Economy/` — `PlayerCurrencyManager`, `PlayerCurrencyController`, and `Service/` (`ICurrencyBackend`, `EconomyCurrencyBackend`, `CloudCodeCurrencyBackend`, `CurrencyBalanceUpdate`) |
| Editor | `Editor/Achievements/` — `AchievementDefinitionCatalog` and its `.rc` exporter |
| **Game-side UGS glue** | |
| Bridges | `Assets/UGSGlue/UGSGameFlowBridge.cs` (GameFlow ↔ GameServiceEvents), `Assets/UGSGlue/TempleRunUGSBridge.cs` (gameplay → GameServiceEvents) |
| Scene | `Assets/UGSGlue/UGS_Glue.unity` (build index 1), `Test_SubmitLeaderboardScore.cs` |
| Cloud Code | `Assets/UGS/CloudCode/TempleRunUGSCloud~/` (.NET module: models + 4 services, 7 endpoints — AdRewards, HandleProfileChange, PlayerData, PlayerEconomy). Its `.sln`/`.csproj` are tracked, and so is `Project/Properties/PublishProfiles/FolderProfile.pubxml` — **do not delete that file**; the tooling needs it and a run without it fails. A run rewrites it with CRLF line endings and identical content, so discard that diff rather than committing it. Deleting `TestProject` means removing its entry from the `.sln` too, or the solution stops building |
| **Shared/Common** (the `com.crawfissoftware.common` package) | |
| Auto-Event Base | `Runtime/Events/AutoEventFlowBase.cs` — `EventChainDispatcher<TSource, TDest>` + `AutoEventFlowBase<TSource, TDest>`; the one dispatch implementation, shared by every flow and bridge class in both repos |
| Shared Config | `Runtime/Config/DifficultyConfig.cs` (namespace `CrawfisSoftware.Config` — the LIVE difficulty config) |
| Scene Management | `Runtime/SceneManagement/` — `LoadSceneAdditively`, `LoadSceneAfterGameControlEvent`, `CloseSceneOnEvent`, `FireEventWhenSceneCloses` |
| Test Utilities | `Runtime/Test/Test_AutoFireEvent.cs`, `Test_AutoFireEventOnStart.cs` |
| Utilities | `Runtime/Utility/` — `Logger`, `DebugEventFileLogger`, `TimedEvent`, `TextureExtensions`, `Wait` (`Wait.ForSecondsRealtime`, the unscaled Awaitable wait — see Gotchas, *Async and coroutines*); `Runtime/Events/EventHistory.cs` |
| **Contract** (the `com.crawfissoftware.contracts` package) | |
| Contract | `Runtime/GameServiceEvents.cs`, `Runtime/ServicesStatus.cs` |
| Vendored | `Assets/ThirdParty/CrawfisSoftware/` (Random providers used by `Blackboard`, editor tools incl. Play Scene 0 Always); `Assets/CloudCode/GeneratedModuleBindings/` (generated Cloud Code bindings — regenerated by the tooling, no live consumer in the game) |

## Folder structure

The codebase is organized into **four in-repo domains** plus the UGS domain and shared
infrastructure, which now arrive as UPM packages rather than living here:

```
Assets/
├── UGSGlue/                          # This game's half of the GameServiceEvents contract (asmdef-free on purpose)
│                                     #   UGSGameFlowBridge, TempleRunUGSBridge, Test_SubmitLeaderboardScore,
│                                     #   UGS_Glue.unity (build index 1)
│
├── Countdown/                        # Session-ceremony domain (the pre-run 3...2...1); own assembly
│   ├── CrawfisSoftware.Countdown.asmdef  # references CrawfisSoftware.TempleRun; GameFlow references this
│   ├── Scripts/
│   │   ├── Events/                   # CountdownEvents, CountdownAutoEventFlow
│   │   ├── TempleRunSpecific/        # Countdown2TempleRunBridge (CountdownEnded -> PlayerActivateRequested)
│   │   ├── UI/                       # CountdownUIController
│   │   └── CountdownController.cs    # Runs the ceremony: Starting, Started, Tick, Ending
│   └── UI Toolkit/                   # Countdown.uxml (the overlay)
│
├── GameFlow/                         # Application lifecycle domain
│   ├── Scripts/
│   │   ├── Events/                   # GameFlowEvents, GameFlowAutoEventFlow
│   │   ├── TempleRunSpecific/        # TempleRunGameFlowBridge (TempleRun <-> GameFlow)
│   │   ├── CountdownSpecific/        # CountdownGameFlowBridge (GameStarting -> CountdownStartRequested)
│   │   ├── Config/                   # GameState, GameConstants, LevelConfig(+Applier), LevelRegistry,
│   │   │                             #   LevelProgressManager/Data (BlackboardGameFlow is dead code)
│   │   ├── GameControl/              # QuitController, UnloadNonActiveScenes
│   │   ├── UI/                       # GameFlowUIPanelController, MainMenu(+Panel)Controller, LevelSelector(+Panel)Controller
│   │   └── SceneManagement/          # DynamicLevelSceneLoader, FireEventAfterSceneLoads (the rest of the
│   │                                 #   scene plumbing comes from the common package)
│   ├── Scenes/Boot/                  # 0_BootStrap_Game_Only, Game_Boot_0_Initialization, Game_Boot_0_Test_Initialization,
│   │                                 #   Game_Boot_1_UI, Game_Boot_2_Play
│   ├── Audio/                        # UI sound effects
│   └── UI Toolkit/                   # UXML, USS for GameFlow UI
│
├── TempleRun/                        # Gameplay domain
│   ├── Scripts/
│   │   ├── Events/                   # TempleRunEvents, UserInitiatedEvents, TempleRunAutoEventFlow,
│   │   │                             #   Input2TempleRunAutoEventBridge
│   │   ├── Config/                   # Blackboard, TempleRunGameConfig, GameDifficultyManager, PlayerPrefKeys, per-mechanic configs, SpawnPrefabRegistry
│   │   ├── Player/                   # Turn/Jump/Slide/Dash/Lane/Life controllers, collision detectors, distance,
│   │   │                             #   pause, teleport, AI
│   │   ├── PowerUps/                 # IPowerUpEffect strategy: PowerUpEffectBase + five concrete effects
│   │   ├── Track/                    # TrackManager (+variants), PathProvider, SegmentTransitionController, spawners, SO classes,
│   │   │   │                         #   TrackLibraryLoader, TrackSegmentLibrary
│   │   │   ├── Geometry/             # IPathSegmentBuilder, AxisAligned90Builder, ArcTurnBuilder, PathPose/PathSpan
│   │   │   └── Selection/            # ISegmentSelector/ISegmentPool + WeightedDifficulty/AuthoredSequence selectors
│   │   ├── TrackVisuals/             # PrefabSpawnerAbstract; SimplePlane/ and Voxels/ spawners
│   │   ├── Input/                    # Movement/Swipe/Dash/Accelerometer/PauseQuit actions; generated GameControls + LeftRightJumpSlide
│   │   ├── UI/                       # GUIController (distance HUD)
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
│   ├── CloudCode/TempleRunUGSCloud~/ # .NET Cloud Code module (models + 4 services, 7 endpoints)
│   ├── Economy/                      # COIN.ecc - the Economy currency definition; deploy it from the
│   │                                 #   Deployment window. The id comes from the filename
│   ├── Editor/                       # RemoteConfig editor data
│   └── Prefabs/                      # AchievementsPrefab, AchievementsNotificationPrefab, LeaderboardPanel
│                                     #   (the scripts they instance come from the UGS package)
│
├── CloudCode/                        # GeneratedModuleBindings/TempleRunUGSCloud only. The two Blocks modules -
│                                     #   their .ccmr references, server sources and generated bindings - are all gone
├── ThirdParty/CrawfisSoftware/       # Vendored utilities: Random providers (Blackboard depends on them), editor tools
│                                     #   (Play Scene 0 Always, screenshots, dev-build toggle)
└── (also: LevelPlay/, MobileDependencyResolver/, Push Notifications/, AddressableAssetsData/,
     Prefabs/, Resources/, UI Toolkit/, Settings/ [build profiles])
```
