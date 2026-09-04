# Phase 3 — Scene/Prefab Inspector swap: `UI Document` → `Panel Renderer` (RUGS)

Phases 1–2 (C#) are done on `feature/panel-renderer`. This is the **manual Unity Editor** pass.
**Never hand-edit `.unity` / `.prefab` YAML** — Unity renumbers component ids and nulls serialized
references during the swap.

## Per-panel procedure

1. Select the GameObject, note its `UI Document` **Panel Settings**, **Source Asset**, **Sort Order**
   (values below are pre-recorded from the YAML so you can verify after re-assigning).
2. Remove `UI Document`; add `Panel Renderer`.
3. Re-assign Panel Settings / Source Asset / Sort Order.
4. Re-wire the listed controller field(s) — the C# field type changed, so Unity shows them as
   `None`/missing until re-dragged.
5. **Leave the Panel Renderer's `Enabled` checkbox ON.** Panels that "start hidden" are hidden via
   `root.style.display` in code, never by disabling the component (Unity bug **UUM-146174**: a
   PanelRenderer disabled before its first init never re-fires `UIReloaded` → blank panel until a
   manual Inspector toggle). The controllers additionally force `enabled = true` in `OnEnable`, so a
   stray disabled checkbox will be corrected at runtime — but fix it in the scene anyway.
6. Save the scene/prefab.

---

## Inventory — every `UIDocument` (`u!114`, script fileID `19102`) in the project

### ✅ To migrate (7 scene panels + 1 prefab) — 4 scenes + 1 prefab

#### 1. `Assets/GameFlow/Scenes/Boot/Game_Boot_1_UI.unity` — 5 panels

| GameObject | Panel Settings | Source Asset (UXML) | Sort Order | Controller field(s) to re-wire |
|---|---|---|---|---|
| `MainMenu` | `Assets/Settings/GUI/PS_Menu.asset` | `GameFlow/UI Toolkit/UI/UXML/Menu/MainMenu.uxml` | 0 | `MainMenuController._panel` (on **MainMenu** itself) **and** `MainMenuPanelController.menuUI` (on **PanelController-Menu**) — both pointed at this one document |
| `LevelSelection` | `Assets/Settings/GUI/PS_Menu.asset` | `GameFlow/UI Toolkit/UI/UXML/Menu/LevelSelector.uxml` | 0 | `LevelSelectorController._panel` (was `_uiDocument`) **and** `LevelSelectorPanelController._levelSelectorUI` — both on **LevelSelection** itself |
| `LoadingPanel` | `Assets/Settings/GUI/PS_Loading.asset` | `GameFlow/UI Toolkit/UI/UXML/LoadingScreen.uxml` | **1000** | `GameFlowUIPanelController.loadingUI` (on **UIRoot**) |
| `Overlay-GameOver` | `Assets/Settings/GUI/PS_Overlay.asset` | `GameFlow/UI Toolkit/UI/UXML/Overlays/GameOver.uxml` | 0 | `GameFlowUIPanelController.gameOverUI` (on **UIRoot**) |
| `Feedback` | `Assets/Settings/GUI/PS_Feedback.asset` | ⚠ GUID `9a4f16c469f4061439e5f81ebbdf0b59` — **no matching `.meta` in `Assets/`**; the reference may already be broken. Check the Inspector before removing the component and re-assign whatever UXML it shows (or leave empty if it is genuinely missing). | 0 | *none* — no controller references this panel |

> `LevelSelectorController` also has `_levelRegistry` (a ScriptableObject) — that field is untouched
> and should survive; verify it is still assigned after the swap.

#### 2. `Assets/TempleRun/Scenes/Gameplay/TempleRunGameplay.unity` — 1 panel

| GameObject | Panel Settings | Source Asset | Sort Order | Field to re-wire |
|---|---|---|---|---|
| `CountdownController` (UIDocument + controller on the same GameObject) | `Assets/Settings/GUI/PS_Overlay.asset` | `GameFlow/UI Toolkit/UI/UXML/Overlays/Countdown.uxml` | 0 | `CountdownUIController._countdownPanel` (renamed from `_countdownUI`) |

> **Paths moved after this checklist was written (2026-09, Countdown domain extraction).** The
> countdown became its own domain: the UXML is now `Assets/Countdown/UI Toolkit/Countdown.uxml`
> and the controller is `Assets/Countdown/Scripts/UI/CountdownUIController.cs`. The GameObject,
> its scene (`TempleRunGameplay`), the Panel Settings asset and the field name are unchanged —
> only the two asset paths above are stale.

#### 3. `Assets/TempleRun/Scenes/Gameplay/TempleRunGuiOverlay.unity` — 1 panel

| GameObject | Panel Settings | Source Asset | Sort Order | Field to re-wire |
|---|---|---|---|---|
| `UI` (UIDocument + `GUIController` on the same GameObject) | `Assets/Settings/GUI/New Panel Settings.asset` | `TempleRun/UI Toolkit/TempleRunDistances.uxml` | 0 | `GUIController._panel` (renamed from `_uiDocument`) |

#### 4. `Assets/UGS/Scenes/Boot/UGS_Boot_2_Authentication.unity` — 1 panel

| GameObject | Panel Settings | Source Asset | Sort Order | Field to re-wire |
|---|---|---|---|---|
| `PlayerSignInController` (UIDocument + controller on the same GameObject) | `Assets/Settings/GUI/PS_Login.asset` | `Assets/UI Toolkit/Blocks_Modified/PlayerAccountLogin.uxml` | 0 | `PlayerSignInController.signInPanel` (renamed from `uiDocument`) |

> Behaviour change to expect: on sign-in the controller no longer calls
> `gameObject.SetActive(false)` — the GameObject stays active and the panel hides via the `hidden`
> class + `style.display`. (Deactivating would tear the tree down and unregister the reload callback.)

#### 5. `Assets/UGS/Prefabs/UGS/AchievementsPrefab.prefab` — 1 panel (**edit the prefab asset, not the scene instance**)

| GameObject | Panel Settings | Source Asset | Sort Order | Field to re-wire |
|---|---|---|---|---|
| root of `AchievementsPrefab` (UIDocument + `CrawfisSoftware.UGS.Achievements.AchievementsPrefab` on the same GameObject) | `Assets/Blocks/Common/BlocksPanelSettings.asset` | **none** (`sourceAsset: 0`) — the tree is built in code by `AchievementsContainer`; leave Source Asset empty | 0 | `AchievementsPrefab.m_UiPanel` (renamed from `m_UiDocument`) |

Instantiated in `Assets/UGS/Scenes/Boot/UGS_Boot_3_Achievements.unity` — after editing the prefab,
open that scene and confirm the instance has no leftover overrides / missing references.

> ⚠ With no Source Asset, confirm `UIReloaded` still fires for an empty tree. If the callback never
> arrives, the container will not be parented — in that case assign a minimal empty UXML as the
> Source Asset (a single root `VisualElement`), which is the safest fix and needs no code change.

### ⛔ Leave on `UIDocument` — Group C, Unity Gaming Services *Use Case sample* code

These are vendor/sample scripts under `Assets/Blocks/**`; their C# was intentionally not migrated,
so their scene/prefab components **must stay `UI Document`**:

- `Assets/Blocks/Achievements/Prefabs/AchievementsPrefab.prefab`
- `Assets/Blocks/Achievements/Prefabs/AchievementsNotificationPrefab.prefab`
- `Assets/Blocks/Leaderboards/Prefabs/LeaderboardPrefab.prefab`
- `Assets/Blocks/Achievements/TestScenes/AchievementsTestScene.unity`
- `Assets/Blocks/Leaderboards/TestScenes/Leaderboards.unity`
- `Assets/Blocks/PlayerAccount/TestScenes/PlayerAccountScene.unity`
- `Assets/UGS/Scenes/UGS/Achievements.unity`, `Leaderboards.unity`, `AchievementNotifications.unity`
  — these contain **instances of the Blocks prefabs above** (with `m_PanelSettings` / `m_SortingOrder`
  overrides), not their own UIDocuments. Nothing to do.

---

## Post-swap verification (still Phase 3)

- [ ] Every migrated GameObject has a `Panel Renderer` with **Enabled ✔**, correct Panel Settings,
      Source Asset and Sort Order (`LoadingPanel` must keep Sort Order **1000**).
- [ ] No component shows *"Missing (Mono Script)"* or a `None (Panel Renderer)` field.
- [ ] `Game_Boot_1_UI`: `UIRoot` has both `loadingUI` and `gameOverUI` assigned;
      `PanelController-Menu.menuUI` and `MainMenu`'s own `MainMenuController._panel` both point at
      the **MainMenu** panel; `LevelSelection` hosts both level-selector controllers.
- [ ] Console clean on scene open (no `UIDocument is obsolete` migration prompts left).

## Phase 4 — play-test (UGS profile)

- [ ] Boot → loading screen shows, then Main Menu **renders without any manual Inspector toggle**.
- [ ] Play → Level Selector populates (cards, best scores, lock info) → Back returns to Main Menu.
- [ ] Level start → countdown counts down and disappears → gameplay HUD distances update.
- [ ] Die → Game Over overlay → **leaderboard / achievements loop → back to Main Menu**
      (the UGS return path: `GameEnded → LeaderboardOpening → … → PlayerAuthenticated → GameplayReady`).
- [ ] Achievements panel opens/closes; sign-out from the Main Menu re-shows the sign-in panel.
- [ ] No `NullReferenceException`s, no blank panels, no double-fired `GameplayReady`.
- [ ] `GameFlowAutoEventFlow.cs` is unchanged — ERT's `LoadingScreenHidden→GameplayReady` and
      `GameEnded→GameplayReady` chains must **not** be added (UGS already closes both loops).
- [ ] Repeat on the `Test_GameOnly_Windows` profile if you use it (no UGS): note that without UGS the
      boot and post-game loops are the ones ERT's chains fixed — expect the menu **not** to return
      there; that is pre-existing, not caused by this migration.

---

## Phase 5 — the Blocks-sourced notification panel (outstanding)

Phases 1–4 covered the panels this project authors. Two panels arrive a different way: the
shipped UGS scenes **instantiate prefabs from `Assets/Blocks/`**, the vendored Unity samples,
and those still carry `UIDocument`. A grep of `Assets/UGS` finds nothing, because the
`UIDocument` is inside the prefab, not the scene.

| Blocks prefab | Instantiated by | In Build Settings | State |
|---|---|---|---|
| `Blocks/Achievements/Prefabs/AchievementsPrefab.prefab` | `UGS/Scenes/UGS/Achievements.unity` | yes | ✅ already forked → `UGS/Prefabs/UGS/AchievementsPrefab.prefab` |
| `Blocks/Achievements/Prefabs/AchievementsNotificationPrefab.prefab` | `UGS/Scenes/UGS/AchievementNotifications.unity` | yes | ⬜ **outstanding** |
| `Blocks/Achievements/TestScenes/AchievementsTestScene.unity` | — | no | ignore: Blocks-only, never built |

### Fork, do not edit

`AchievementsPrefab` set the precedent: the sample was **copied** into `Assets/UGS/` and
migrated there, leaving `Assets/Blocks/` pristine. Re-importing the Blocks sample would
silently overwrite an in-place edit, so keep the vendored copy untouched.

### Done in code

`Assets/UGS/Scripts/Achievements/AchievementsNotificationPrefab.cs` — a PanelRenderer fork in
namespace `CrawfisSoftware.UGS.Achievements`, mirroring the `AchievementsPrefab` fork:
`PanelRenderer m_UiPanel` replaces `UIDocument m_UiDocument`; the tree is reached through
`OnUIReload` instead of `rootVisualElement`; `enabled = true` is forced in `OnEnable`
(UUM-146174); re-parenting is idempotent and repeats on every reload.

### Remaining — in Unity, never by editing YAML

- [ ] Duplicate `Blocks/Achievements/Prefabs/AchievementsNotificationPrefab.prefab` into
      `Assets/UGS/Prefabs/UGS/`.
- [ ] On the copy: remove `UI Document`, add `Panel Renderer`, and point its script field at the
      forked `CrawfisSoftware.UGS.Achievements.AchievementsNotificationPrefab`.
- [ ] Re-assign, from the original's serialized values:

      | Field | Value |
      |---|---|
      | Panel Settings | `Assets/Blocks/Common/BlocksPanelSettings.asset` |
      | Source Asset | **none** — the notification builds its tree in code (`new AchievementNotificationElement()`), so this was already null |
      | Sort Order | **1** |

- [ ] Re-assign `m_Icons` (the `Texture2D[]`) and leave `InitOnAwake` as it was.
- [ ] Wire the new `m_UiPanel` field to the Panel Renderer on the same GameObject.
- [ ] **Leave `Enabled` ✔.**
- [ ] Repoint `UGS/Scenes/UGS/AchievementNotifications.unity` at the forked prefab.
- [ ] Play-test: earn an achievement and confirm the notification appears; no blank panel, no
      `NullReferenceException`.
- [ ] Confirm `grep -r "UIDocument" Assets/UGS Assets/GameFlow Assets/TempleRun` is empty — any
      remaining hits should be inside `Assets/Blocks/` only.
