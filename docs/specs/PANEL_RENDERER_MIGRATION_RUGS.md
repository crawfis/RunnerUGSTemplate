# Port: UIDocument → PanelRenderer migration into RUGS

This ports the `UIDocument → PanelRenderer` work already completed in the sibling
**EndlessRunnerTemplate (ERT)** repo back into **RunnerUGSTemplate (RUGS)**. RUGS is on Unity
6000.5, where `UIDocument` is obsolete and Unity auto-migrates it (nulling references) — so the
same migration is needed here.

**Reference:** the portable, project-agnostic how-to with all the hard-won gotchas is in
[../playbooks/uidocument-to-panel-renderer.md](../playbooks/uidocument-to-panel-renderer.md).
ERT's finished controllers are the working reference implementation to diff against
(`C:\Repos\EndlessRunnerTemplate\Assets\...\UI\*.cs`).

---

## ⚠ CRITICAL DIFFERENCE FROM ERT — do NOT port the event-flow fixes

ERT is **non-UGS**. During its migration, two event-flow gaps were found and fixed with auto-chains
in `GameFlowAutoEventFlow.cs`:

- `LoadingScreenHidden → GameplayReady` (boot → Main Menu)
- `GameEnded → GameplayReady` (post-game → Main Menu)

**These were gap-fills for the missing UGS loop. RUGS still has UGS, which already closes both loops
via `UGSGameFlowBridge`:**

- `{ PlayerAuthenticated → GameplayReady }` (boot)
- `{ GameEnded → LeaderboardOpening }` → … → `PlayerAuthenticated → GameplayReady` (return)

**Do NOT add the two ERT auto-chains to RUGS.** Doing so would double-fire `GameplayReady` at boot
and collide with the leaderboard/return flow. This port is **UI-rendering only** — the RUGS event
graph stays as-is. (If you ever build a non-UGS test profile of RUGS, that path — not the UGS
profile — would need ERT's chains; keep them out of the default UGS flow.)

---

## Scope in RUGS

`grep -rl "UIDocument\|rootVisualElement" Assets --include=*.cs` finds three groups:

### Group A — core UI (direct port from ERT; same class names, GUID-preserved)
Apply ERT's finished versions/pattern 1:1:
- `Assets/GameFlow/Scripts/UI/MainMenuController.cs` (query)
- `Assets/GameFlow/Scripts/UI/LevelSelectorController.cs` (query)
- `Assets/GameFlow/Scripts/UI/MainMenuPanelController.cs` (show/hide)
- `Assets/GameFlow/Scripts/UI/LevelSelectorPanelController.cs` (show/hide)
- `Assets/GameFlow/Scripts/UI/GameFlowUIPanelController.cs` (show/hide ×2: loading + gameOver)
- `Assets/TempleRun/Scripts/UI/CountdownUIController.cs` (query + show/hide)
- `Assets/TempleRun/Scripts/UI/GUIController.cs` (query, HUD)

> Diff each against ERT before pasting — RUGS versions may retain UGS-specific bits ERT stripped
> (e.g. `MainMenuController`'s sign-out button). Keep RUGS's logic; only change the UI mechanism.

### Group B — UGS-domain UI (no ERT equivalent; migrate with the same pattern)
- `Assets/UGS/Scripts/Achievements/AchievementsPrefab.cs`
- `Assets/UGS/Scripts/Authentication/PlayerSignInController.cs`

### Group C — Unity "Blocks" sample UI (vendor/sample code — decide per file)
- `Assets/Blocks/Achievements/**`, `Assets/Blocks/Leaderboards/**`, `Assets/Blocks/PlayerAccount/**`
  (incl. their `TestScenes/`). These are Unity Gaming Services *Use Case sample* scripts. Prefer to
  **leave them on `UIDocument`** unless they actually break, or migrate them in a separate pass — do
  not let them expand the blast radius of the core migration.

## The portable rules (summary — see playbook for detail)

1. Field type `UIDocument → PanelRenderer`. No `rootVisualElement` — get the tree only from
   `RegisterUIReloadCallback` / `UnregisterUIReloadCallback`; cache `root`; **re-cache queried
   elements on every callback** (a reload rebuilds the tree).
2. **Show/hide via `root.style.display`, keeping the PanelRenderer ENABLED at all times.** Do **not**
   toggle `PanelRenderer.enabled` for show/hide — that hits Unity bug **UUM-146174** (a panel
   disabled before first init, in `Awake` *or* authored disabled in the scene, never re-fires
   `UIReloaded` → blank until a manual toggle). Track a `_visible` bool; apply it in the callback and
   on each show/hide event.
3. **Force `_panel.enabled = true` in `OnEnable`** so a scene-authored disabled checkbox can't
   silently break rendering.
4. Keep every event subscription's matching `OnDestroy` unsubscribe (separate from the UI reload
   callbacks).

## Phased execution (mirrors ERT)

- **Phase 1–2 (code):** convert Group A (and Group B) controllers on a branch
  `feature/panel-renderer`. Grep must show zero `UIDocument`/`rootVisualElement` in migrated files.
- **Phase 3 (scenes, in the Unity Inspector — never hand-edit `.unity` YAML):** for each panel
  GameObject, remove `UI Document`, add `Panel Renderer`, reassign PanelSettings/SourceAsset/
  SortOrder, re-wire the controller field(s). **Leave every PanelRenderer's `Enabled` checkbox ON**
  (hide "start hidden" panels via `style.display`, never by disabling the component). RUGS has more
  scenes than ERT (UGS boot/UI scenes) — inventory them: find every scene with a `UIDocument`
  (`u!114`) component.
- **Phase 4 (play-test, UGS profile):** boot → menu renders (no toggle); Play → level select →
  countdown → gameplay + HUD; die → game over → **leaderboard/achievements loop → back to menu**
  (the UGS return path — verify it still closes); no NullRefs; no blank panels. Also test a
  Test_GameOnly profile if present.
- Follow RUGS's mandated event-skill workflow where relevant (`/audit-events` after) — but remember
  **no new event chains are needed** for this UI port.

## Verification checklist
- [ ] `grep -rl "UIDocument\|rootVisualElement" Assets --include=*.cs` → only Group C (Blocks) left, if you deferred it
- [ ] No `PanelRenderer.enabled = false` anywhere; show/hide is `style.display`; `enabled = true` forced in `OnEnable`
- [ ] `GameFlowAutoEventFlow.cs` UNCHANGED (no ERT `LoadingScreenHidden→GameplayReady` / `GameEnded→GameplayReady`)
- [ ] Every migrated scene's PanelRenderers have `Enabled` checked
- [ ] UGS post-game return-to-menu loop still works end to end

---

## Ready-to-paste prompt (run in the RUGS repo)

> Continue a UI migration in the Unity 6000.5 project at `C:\Repos\RunnerUGSTemplate` (RUGS). Port the
> already-completed `UIDocument → PanelRenderer` migration from the sibling repo
> `C:\Repos\EndlessRunnerTemplate` (ERT) into RUGS.
>
> READ FIRST: `docs/specs/PANEL_RENDERER_MIGRATION_RUGS.md` and `docs/playbooks/uidocument-to-panel-renderer.md`
> in this repo. ERT's finished controllers under `C:\Repos\EndlessRunnerTemplate\Assets\GameFlow\Scripts\UI\`
> and `...\TempleRun\Scripts\UI\` are the reference implementation — diff against them.
>
> DO (Phases 1–2, C# only, on a new branch `feature/panel-renderer` off `main`):
> - Convert the 7 core controllers (Group A) and the 2 UGS-domain controllers (Group B) to
>   PanelRenderer using the playbook's final pattern: field `UIDocument → PanelRenderer`; tree only via
>   `RegisterUIReloadCallback`/`UnregisterUIReloadCallback` (no `rootVisualElement`), re-caching queried
>   elements each callback; **show/hide via `root.style.display` with the PanelRenderer kept enabled**
>   (never toggle `enabled` — UUM-146174); track a `_visible` desired-state; **force `_panel.enabled = true`
>   in `OnEnable`**. Preserve each controller's existing (possibly UGS-specific) logic — change only the
>   UI mechanism. Keep every event subscription's `OnDestroy` unsubscribe.
> - Leave the `Assets/Blocks/**` sample UI on UIDocument (Group C) unless it breaks.
>
> DO NOT:
> - Do NOT add ERT's event-flow auto-chains. RUGS has UGS; `UGSGameFlowBridge` already provides
>   `PlayerAuthenticated → GameplayReady` (boot) and `GameEnded → LeaderboardOpening → … → GameplayReady`
>   (return). Adding `LoadingScreenHidden→GameplayReady` or `GameEnded→GameplayReady` would double-fire
>   and break the leaderboard return loop. `GameFlowAutoEventFlow.cs` must stay unchanged.
> - Do NOT hand-edit `.unity` YAML for the component swap.
>
> CONSTRAINTS: you cannot run Unity — be rigorous about C# and note editor assumptions. Commit per phase
> with Co-Authored-By trailers; do not push. After the code, produce a Phase-3 checklist: every scene
> with a `UIDocument` (`u!114`) component (RUGS has more scenes than ERT — inventory them), each panel
> GameObject, its UXML/PanelSettings, and which controller field(s) to re-wire — and note that every
> PanelRenderer's Enabled checkbox must stay ON (hide via `style.display`). Phases 3 (Inspector swap) and
> 4 (play-test, UGS profile — verify the post-game return-to-menu loop) are the user's.
