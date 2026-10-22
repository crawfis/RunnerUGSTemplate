# Adding a Mechanic or Feature

The steps for each kind of feature. The event workflow itself (`/list-events`, `/add-event`,
`/add-auto-chain`, `/add-bridge-mapping`, `/audit-events`, `/verify-unity`) is in
[AGENTS.md](../AGENTS.md#required-skills-workflow); run it for every feature. The worked
gameplay walkthrough lives in the sibling repo,
[EndlessRunnerTemplate](https://github.com/crawfis/EndlessRunnerTemplate/blob/main/docs/ADDING_A_MECHANIC.md).

## A gameplay feature

1. `/list-events TempleRun`, then `/add-event` for the new mechanic, then `/add-auto-chain`.
2. `/add-bridge-mapping` if the feature affects game session state.
3. Create a scene in `Assets/TempleRun/Scenes/Gameplay/` and add it to the Build Profile.
4. Subscribe in `Awake()`, unsubscribe in `OnDestroy()`, publish state changes on `TempleRunBus`.
5. Keep visuals and audio separate from logic. Then `/audit-events`.

## A GameFlow feature

`/list-events GameFlow`, `/add-event` to `GameFlowEvents`, `/add-auto-chain`, implement through
`GameFlowBus`, then `/audit-events`.

## A UI panel

Panels live in `Assets/GameFlow/Scripts/UI/` and subscribe to `GameFlowEvents` for show and hide.
Follow the `GameFlowUIPanelController` / `*PanelController` pattern (see
[the Panel Renderer playbook](playbooks/uidocument-to-panel-renderer.md)). For a new panel state,
`/add-event` the show and hide events.

## A UGS feature

1. **`/list-events UGS`** — Review existing UGS events
2. **`/add-event`** — Add events to `UGS_EventsEnum` for the new service callbacks (in the
   EventDrivenUGS repo — the ugs package is read-only here)
3. **`/add-bridge-mapping`** — If the game must react, add a `GameServiceEvents` crossing and
   map it on both sides (`GameServiceEventsUGSBridge` in the package, `Assets/UGSGlue/` here)
4. **`/add-auto-chain`** — Wire UGS auto-progressions if needed
5. Create `UGS_Boot_N_[Feature]` scene
6. Add scene to Build Profile scene list
7. Create event adapters publishing the UGS SDK callbacks on `UGSBus`
8. Wire loading in `0_BootStrap` via `LoadSceneAdditively` component
9. **`/audit-events`** — Verify all subscriptions have matching unsubscriptions
