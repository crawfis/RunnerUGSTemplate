---
name: sync-templerun
description: Classify and port Assets/TempleRun drift between this repo and the sibling EndlessRunnerTemplate. Runs a normalized diff of the two local checkouts, separates intended divergence from real drift, and walks through porting file by file. Use when the sibling has gameplay work this repo should pick up, or vice versa.
allowed-tools: Read, Grep, Glob, Bash
---

# Sync TempleRun with EndlessRunnerTemplate

The two repos share the TempleRun gameplay domain by **deliberate porting between two
editable trees** — not a package, not a submodule. Asset GUIDs match across the repos
(shared ancestry), so file-level porting is safe and scene/prefab references survive
copies. The sibling is the usual upstream for gameplay work; this repo is upstream for
nothing in TempleRun except its own intended divergences (listed below).

Checkout locations (override with `SIB=` / `UGS=` env vars for the compare script):

- Sibling: `C:\Repos\Github\EndlessRunnerTemplate`
- This repo: `C:\Repos\Github\RunnerUGSTemplate`

## Step 1 — Classify

```bash
bash .claude/skills/sync-templerun/compare.sh > /tmp/drift.tsv
cut -f1 /tmp/drift.tsv | sort | uniq -c                      # summary
grep -v '^identical\|^eol-bom-only' /tmp/drift.tsv | sort -t$'\t' -k4,4nr
```

Statuses: `identical` and `eol-bom-only` need no action (`eol-bom-only` is a checkout
artifact — both repos store LF; clones differ in `core.autocrlf`). `drift`, `only-sibling`
and `only-ugs` rows are the work list. Columns 2–3 are each side's last commit date for
the file — the newer side is usually, but not always, the direction to port.

## Step 2 — Subtract the intended divergences

Check every drift row against the **Expected differences** table at the bottom of this
file. Rows in the table are settled decisions: do not "fix" them. Anything NOT in the
table is unreviewed drift — triage it in Step 3.

## Step 3 — Triage each remaining file

For each file, view the normalized diff and decide a direction:

```bash
SIB=/c/Repos/Github/EndlessRunnerTemplate; UGS=/c/Repos/Github/RunnerUGSTemplate
norm() { sed -e '1s/^\xEF\xBB\xBF//' -e 's/\r$//' "$1"; }
diff -u <(norm "$SIB/Assets/TempleRun/$f") <(norm "$UGS/Assets/TempleRun/$f")
```

- One side changed since the last sync, the other didn't → fast-forward (adopt the newer
  side's bytes wholesale).
- Both sides changed → hand-merge, and wherever the two designs agree, keep the
  **sibling's** shape so the file returns to byte parity (parity is what keeps the next
  sync cheap). If the collision is a real design fork, consider adding it to the
  Expected differences table instead of merging.
- The difference is churn (editor version re-serialization, package-version fields,
  float precision in materials) → leave it; add to the table if it will persist.

## Step 4 — Port

- **Existing file**: copy the file content only. NEVER copy its `.meta` (the target's
  meta already holds the shared GUID).
- **New file**: copy the file AND its `.meta`, so the GUID stays shared across repos and
  future scene references resolve in both.
- **Scenes / prefabs / .asset files**: whole-file adoption only — never hand-edit or
  hand-merge Unity YAML. If both sides changed the same scene, stop and flag it for
  manual work in the Unity editor.
- **C# into this repo**: mind the asmdef walls. `CrawfisSoftware.TempleRun` references
  Common, ThirdParty, EventsPublisher, InputSystem and the GTMY audio manager — nothing
  else. A ported script referencing anything outside those will not compile here (in the
  sibling everything is Assembly-CSharp, so its code can quietly reach further).
- Both repos resolve the same EventsPublisher commit (check `Packages/packages-lock.json`
  hashes if in doubt); sibling code using the typed `EventId<T>` API compiles here as-is.

## Step 5 — Verify

1. Compile without waiting for Unity: the generated csprojs are gitignored, so new files
   can be appended freely (forward slashes work):
   ```bash
   # add each new .cs as <Compile Include="Assets/TempleRun/..."/> before </Project>, then
   dotnet build CrawfisSoftware.TempleRun.csproj --nologo -v q
   dotnet build CrawfisSoftware.GameFlow.csproj --nologo -v q
   dotnet build CrawfisSoftware.TempleRun.Editor.csproj --nologo -v q
   dotnet build Assembly-CSharp.csproj --nologo -v q      # UGSGlue lives here
   ```
2. If scenes/prefabs were ported, confirm every referenced GUID resolves: extract
   `guid: <32 hex>` from the ported files and check each against the union of `.meta`
   files in `Assets/`, `Packages/`, and `Library/PackageCache/`
   (`0000000000000000f000000000000000` is Unity's built-in-resources pseudo-GUID —
   always "missing", always fine).
3. Focus Unity so it reimports; if a ported scene was open, reopen it. Play test.
4. Re-run Step 1: the drift list should now equal the Expected differences table.

## Step 6 — Record

- Commit with the established message shape: `sync(templerun): <what moved>`.
- If the intended-divergence set changed, update the table below and its date stamp in
  the same commit.

## Expected differences (last reviewed 2026-09-01)

The difficulty pipeline is the one real design fork: this repo's difficulty tables come
from Remote Config (a remote table latches and overrides the local one); the sibling's
come from each level's difficulty variants (with fallback to the level's first variant).
Reconciling that is a design decision, not a sync task.

| File | Why it differs |
|---|---|
| `Scripts/Config/GameDifficultyManager.cs` | The fork itself: remote-latch here vs level-variant resolution there |
| `Scripts/Config/Blackboard.cs` | Here: `TempleRunConfigApplied`/`TempleRunLevelApplied` handlers + `SelectedLevel`; there: single-writer `GameConfig` via typed `DifficultyChanging` |
| `Scripts/Config/LoadDefaultGameConfigs.cs` | Here publishes the local table unconditionally (the remote latch protects); there it stands down when a level already published one |
| `Scripts/Track/TrackManager.cs` | Shared base, plus here-only `TempleRunConfigApplied` early-init (`_isInitialized`) |
| `Scripts/Events/TempleRunEvents.cs` | Same members/values; bridged- and difficulty-section comments are repo-truthful, and `DifficultySettingsApplied = 320` carries the remote-table doc + Sticky only here |
| `Scriptables/Levels/Level_01_Config.asset`, `Level_02_Config.asset` | Serialized shape follows each repo's GameFlow `LevelConfig` (single `Difficulty` here, `Difficulties` list there) |
| `Scenes/Gameplay/TempleRunGuiOverlay.unity` | Here adds `CoinBalanceHUDController` and the UGS panel settings |
| `UI Toolkit/TempleRunDistances.uxml` | Here adds the coin-balance panel the services layer fills |
| `Scripts/Input/GameControls.inputactions` | Input System version churn (`"priority"` fields); disappears when the sibling upgrades the package |
| `Materials/BlackShiny.mat`, `Gold.mat`, `RedShiny.mat`, `TiledTexture.mat` | Editor re-serialization churn (float precision, `_XRMotionVectorsPass`) |
| `CrawfisSoftware.TempleRun.asmdef`, `Editor/...Editor.asmdef` (only here) | This repo compiles domains as assemblies; the sibling uses Assembly-CSharp |
| `Scripts/Config/PlayerPrefKeys.cs` (only here) | Same class both repos; the sibling keeps it under `Assets/GameFlow/` (visible without asmdefs), here it must live inside the TempleRun assembly |

One style note: TempleRun files synced from the sibling use the typed
`TempleRunBus.Id<T>(...)` / `EventId<T>` subscription API; the rest of this repo uses the
classic `Subscribe(enum, handler)` form. Both are valid — match the file you are editing,
and do not "normalize" synced files back to the classic form (that re-creates drift).
