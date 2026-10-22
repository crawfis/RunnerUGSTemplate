# Known Issues and Landmines

Unity and environment caveats, and failures that are silent until you know where to look.

## Two silent failure links in the coin chain

The coin chain runs `CoinCollected` → `CurrencyTotalChanged` → `UGS_CoinUpdated`, and banks the
coins at `GameEnding` (the README's "How the Glue Works" walks it). Two links in it live outside C#
and fail silently, so check them before debugging code:
`PlayerCurrencyController` must be in a loaded scene — it sits on `GameFlow/PlayerCurrency` in
`UGS_Boot_0_Initialization`, and it is the **only** subscriber to `CurrencySyncRequested`, so
without it every event in the chain still fires and nothing is ever written — and a `COIN` currency must
exist in the environment being signed in to, which `Assets/UGS/Economy/COIN.ecc` defines and the
Deployment window publishes. Economy configuration is per-environment.

## The Cloud Code publish profile

`Assets/UGS/CloudCode/TempleRunUGSCloud~/Project/Properties/PublishProfiles/FolderProfile.pubxml`
is tracked and the tooling needs it: do not delete it, because a run without it fails.

## Test Without UGS

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

## Singletons

- `Blackboard.Instance` - Global game state; `GameTime.Instance` - pausable clock
- The event buses are NOT singletons any more — `EventsFor<T>` is static, needs no scene
  object, and is safe to use from `Awake()`
- Only access `Blackboard.Instance` after its `Awake()` has run

## Design notes

- Turn distance is a difficulty setting (`DifficultyConfig.SafePreTurnDistance`, consumed
  by `TurnController`): on easy difficulties the player may turn before the visual
  intersection. Early turns are not a bug — check the difficulty config before "fixing"
  them.

## Dead and placeholder files (don't be misled by grep hits)

- `Assets/GameFlow/Scripts/Config/BlackboardGameFlow.cs` - fully commented out; there is
  no GameFlow blackboard — `Blackboard` lives in TempleRun
- `Assets/TempleRun/Scripts/Config/DifficultyConfig.cs` - fully commented out; the live
  class is `Runtime/Config/DifficultyConfig.cs` in the common package (namespace
  `CrawfisSoftware.Config`)
