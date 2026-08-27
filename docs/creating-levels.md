# Creating Levels with New Prefabs

This guide walks through the complete workflow for creating a new level in RunnerUGS — from placing new prefabs in the project all the way to seeing them in a playtest.

---

## Table of Contents

1. [Concepts Overview](#1-concepts-overview)
2. [Track Manager Variants](#2-track-manager-variants)
3. [Step 1 — Prepare Your Prefabs](#step-1--prepare-your-prefabs)
4. [Step 2 — Register Prefabs in SpawnPrefabRegistry](#step-2--register-prefabs-in-spawnprefabregistry)
5. [Step 3 — Author Segment Assets and Register Them](#step-3--author-segment-assets-and-register-them)
6. [Step 4 — Create the Level Asset](#step-4--create-the-level-asset)
7. [Step 5 — Wire the Spawners in the Scene](#step-5--wire-the-spawners-in-the-scene)
8. [Step 6 — Set the Active Level at Runtime](#step-6--set-the-active-level-at-runtime)
9. [Step 7 — Playtest and Iterate](#step-7--playtest-and-iterate)
10. [Reference: Track Data Model](#reference-track-data-model)
11. [Reference: SpawnMode Guide](#reference-spawnmode-guide)
12. [Reference: Difficulty Config](#reference-difficulty-config)

---

## 1. Concepts Overview

The track generation system has three layers:

```
ScriptableObject Data Layer (authoring — edited in the Inspector)
  TrackSegmentSO                ← one asset per segment definition
  TrackSegmentRegistrySO        ← shared pool of all segments
  TrackLevelSO                  ← per-level ruleset: filters the pool by tag/id
  TrackLevelRegistrySO          ← maps a level number to its ruleset

Runtime Selection Layer
  TrackLibraryLoader            ← reads the SO assets into runtime definitions
  TrackManager                  ← picks the next segment from the library
  TrackSegmentLibrary           ← weighted selection engine

3D Geometry + Spawning Layer
  PathProvider                  ← converts segment data to Entrance→Pivot→Exit splines
  ObstacleSpawner               ← places obstacles on each spline segment
  CoinSpawner                   ← places coin lines
  PowerUpSpawner                ← places power-ups
  PrefabSpawnerAbstract         ← places visual track tiles
```

A **segment** is a run of track built on three points — **Entrance → Pivot → Exit**. Each
segment defines:
- Its **geometry** — `ToPivotDistance` (Entrance to the turn point) and `ExitDistance`
  (post-turn run-out); total length is the sum of the two
- Its **direction** (`Straight`, `Left`, `Right`, or `Either` — a T-junction resolved by the
  player's swipe)
- Its **spawn mode** (how obstacles/coins/power-ups are placed on it)
- Optional **spawn slots** (exact positions for Preset or Hybrid modes)

A **level** is a `TrackLevelSO` asset that selects a subset of segments from the registry
using **tags** (or explicit ids) and configures lane count, lane width, and difficulty.

The SO assets are pure data. At `TrackManager` initialization, `TrackLibraryLoader` reads
them into fresh runtime definitions and normalizes them — the authored assets are never
mutated at runtime.

---

## 2. Track Manager Variants

The TrackManager is the component on the `TrackManager` GameObject in the `TempleRunTrackPCG` scene. Three variants are available:

### `TrackManager` (default)
The general-purpose manager. Reads segment definitions from the ScriptableObject library
(resolved by `TrackLibraryLoader` at init). Segment lengths come from the definition's
geometry (or a random value between `MinTrackLength` and `MaxTrackLength` for the fallback
path when no level is selected).

**Direction logic:** 40% Left, 40% Right, 20% Left (randomised when no definition overrides it).

**Best for:** Most levels. Use this unless you have a specific tiling or fixed-pattern requirement.

**Inspector fields:**
| Field | Description |
|-------|-------------|
| `_numberOfLookAheadTracks` | How many segments to keep in the queue ahead of the player (default 12) |
| `_trackLevels` | The `TrackLevelRegistrySO` asset that maps the selected level number to its ruleset |

---

### `TrackManagerForTiles`
Extends `TrackManager`. Overrides two behaviours:

- **Tile-snapped lengths** — segment length is rounded to the nearest multiple of `Blackboard.Instance.TileLength` (default 4 units). Prevents tile seams appearing mid-segment.
- **Strict alternation** — directions strictly alternate Left → Right → Left → … so the track never turns the same way twice.

**Best for:** Tile-based visual themes (voxel runners, block worlds) where tile count must be an integer.

**Inspector fields:** Same as `TrackManager`.

---

### `TrackManagerList`
Extends `TrackManager`. Overrides segment length selection to pick from a predefined list of lengths stored in an `IntListScriptable` asset instead of using a continuous random range.

**Best for:** Precise, hand-tuned rhythms where you want to guarantee that specific distances appear (e.g., "always 4, 8, or 16 units").

**Inspector fields:**
| Field | Description |
|-------|-------------|
| `_segmentLengths` | Assign an `IntListScriptable` ScriptableObject containing the allowed lengths |

> **Switching managers:** In `TempleRunTrackPCG`, remove the existing TrackManager component and add the variant you want. Only one TrackManager should be active at a time.

---

## Step 1 — Prepare Your Prefabs

### Obstacle prefabs
- Origin at **bottom-centre**, forward along **+Z**.
- Add a `Collider` set to **Is Trigger = true**.
- Tag the root object as `"Obstacle"`.
- Keep the mesh within the bounds that fit in one lane or the full track width.

### Coin prefabs
- Origin at **centre**, any facing.
- Add a `Collider` set to **Is Trigger = true**.
- Tag the root object as `"Coin"`.

### Power-up prefabs
- Origin at **centre**.
- Add a `Collider` set to **Is Trigger = true**.
- Tag the root object as `"PowerUp"`.
- Add a `PowerUpIdentifier` component and assign a `PowerUpDefinition` asset.

### Track tile prefabs
- Origin at **bottom-centre-rear** (where the previous segment ended).
- Scale to fit exactly `TileLength` units in Z.
- No special collision tags required.

### Folder convention
Place prefabs in:
```
Assets/TempleRun/Prefabs/Obstacles/
Assets/TempleRun/Prefabs/Collectables/
Assets/TempleRun/Prefabs/PowerUps/
Assets/TempleRun/Prefabs/Track/
```

---

## Step 2 — Register Prefabs in SpawnPrefabRegistry

The `SpawnPrefabRegistry` ScriptableObject decouples the `PrefabTag` strings authored in segment spawn slots from Unity asset GUIDs. Spawners look up prefabs here at runtime.

### Creating a registry asset
1. In the Project window: **right-click → Create → TempleRun → Spawn Prefab Registry**
2. Name it (e.g., `DesertSpawnRegistry`)
3. Place it in `Assets/TempleRun/Scriptables/`

### Adding entries
In the Inspector, add one entry per prefab variant:

| Tag | Prefab | Notes |
|-----|--------|-------|
| `rock_low` | Rock01_Low.prefab | Jumps over |
| `rock_high` | Rock01_High.prefab | Duck under |
| `cactus` | Cactus01.prefab | Lane-wide |
| `coin_gold` | Coin_Gold.prefab | |
| `coin_ruby` | Coin_Ruby.prefab | Double value |
| `shield` | Shield_PowerUp.prefab | |

Tags must match the `PrefabTag` values you put in segment `SpawnSlots` (see Step 3).

> **Fallback:** If a tag is not found in the registry, the spawner uses its default prefab fields from the Inspector, then generates a coloured primitive if those are also unassigned.

---

## Step 3 — Author Segment Assets and Register Them

Segments are `TrackSegmentSO` assets in `Assets/TempleRun/Scriptables/Track/Segments/`,
edited directly in the Inspector.

### Creating a segment

1. In the Project window: **right-click → Create → CrawfisSoftware → TempleRun → Track Segment**
2. Name the asset after its `Id` (e.g., `desert_left_20`)
3. Place it in `Assets/TempleRun/Scriptables/Track/Segments/`
4. Fill in the fields in the Inspector (see reference below)
5. **Add it to the shared pool**: select
   `Assets/TempleRun/Scriptables/Track/TrackSegmentRegistry.asset` and add the new segment
   to its `Segments` array — a segment that isn't in the registry can never be selected

Example values for a medium-difficulty desert left turn of total length 20:
`Direction = Left`, `ToPivotDistance = 19`, `ExitDistance = 1`, `Weight = 1.5`,
`MaxRepeat = 2`, `DifficultyRating = 3`, `Tags = [desert, medium]`, `SpawnMode = Hybrid`.

### Field reference

| Field | Type | Description |
|-------|------|-------------|
| `Id` | string | Unique identifier — no spaces, use underscores |
| `Direction` | enum | `Straight`, `Left`, `Right`, or `Either` (T-junction — player chooses); Inspector dropdown |
| `ToPivotDistance` | float | Entrance → Pivot (the turn point). For a `Straight`, this is the whole segment |
| `ExitDistance` | float | Pivot → Exit run-out after the turn. `0` for Straight; `> 0` for turns. Total length = `ToPivotDistance + ExitDistance` — there is no separate `Length` field |
| `Weight` | float | Relative selection probability (higher = more common) |
| `MaxRepeat` | int | Max consecutive appearances before it's excluded (0 = unlimited) |
| `DifficultyRating` | float | 0–10 difficulty gate; used with `targetDifficulty` param |
| `Tags` | string[] | Used to include this segment in a level — match `ActiveSegmentTags` |
| `Role` | string | `"Normal"`, `"Opening"`, `"Checkpoint"`, `"Challenge"`, `"Reward"`, `"Boss"`, `"Tutorial"` |
| `SpeedMultiplier` | float | 1.0 = no change; > 1.0 = faster during this segment |
| `SpawnMode` | string | `"Procedural"`, `"Preset"`, or `"Hybrid"` — see [SpawnMode Guide](#reference-spawnmode-guide) |
| `VisualTheme` | string | Hint to visual spawners (e.g., `"desert"`, `"temple"`) |
| `SpawnSeed` | int | Deterministic seed for this segment's RNG (0 = use master random) |
| `BlockedLanes` | int[] | Lane indices the player cannot use on this segment |
| `ActiveLanes` | int[] | Force only these lanes to be available |
| `SpawnSlots` | object[] | Exact object placements — see below |
| `TeleportDistance`, `TurnFailureDistance`, `TurnRadius` | float | Leave at 0 — derived automatically at load (normalization) |

### SpawnSlot fields

| Field | Type | Description |
|-------|------|-------------|
| `NormalizedPosition` | float | 0.0 = segment start, 1.0 = segment end |
| `Lane` | int | Lane index: `-1` = left, `0` = centre, `1` = right (extends beyond ±1 for wider tracks) |
| `Height` | float | Y-offset above track; `0` = use spawner's default height |
| `Type` | string | `"Obstacle"`, `"Hazard"`, `"Coin"`, `"PowerUp"` |
| `PrefabTag` | string | Key into `SpawnPrefabRegistry`; empty = spawner default |
| `Weight` | float | Selection probability when `Required = false` |
| `Required` | bool | `true` = always spawns; `false` = only if `random() <= Weight` |

### Using the `/generate-segments` skill

Instead of hand-authoring assets one at a time you can run:

```
/generate-segments
```

The skill prompts you for direction, length range, difficulty range, and tags, then generates well-formed `TrackSegmentSO` assets and registers them in the `TrackSegmentRegistrySO`.

---

## Step 4 — Create the Level Asset

A level is a `TrackLevelSO` asset — a thin ruleset that selects segments from the shared
registry. The existing levels
(`Assets/TempleRun/Scriptables/Track/TrackLevel_01_Beginner.asset` …
`TrackLevel_05_Expert.asset`) are the best starting reference.

### Creating a new level

1. In the Project window: **right-click → Create → CrawfisSoftware → TempleRun → Track Level**
   (or duplicate an existing `TrackLevel_*` asset with Ctrl+D to start from a known baseline)
2. Set **Level Name** (shown in game menus)
3. Set **Level Number** — this is how the level is found at runtime; it must match the
   `LevelNumber` of the GameFlow `LevelConfig` that selects it (see Step 6)
4. Set **Difficulty Rating** (0–10, informational — actual spawn rates come from `DifficultyConfig`)
5. Set **Lane Count** and **Lane Width**
6. Assign **Registry** — the shared `TrackSegmentRegistry.asset`
7. Set **Start Segment Id** — the `Id` of the segment that always plays first (e.g., `"start"`)
8. Fill **Active Segment Tags** — any segment whose `Tags` list contains at least one listed
   tag is included in this level. (Alternatively, list exact ids in **Active Segment Ids** —
   when non-empty, it takes precedence over tags. Leave both empty to include the whole pool.)
9. **Register the level**: select
   `Assets/TempleRun/Scriptables/Track/TrackLevelRegistry.asset` and add the new
   `TrackLevelSO` to its `Levels` array

### Tips

- **Balance Left/Right** — aim for equal counts to prevent the track feeling biased.
- **Cover the full difficulty range** — even coverage allows smooth ramping.
- **Check your tag spelling** — tags are matched case-sensitively against segment `Tags`;
  a typo silently produces an empty (or fallback) pool.

---

## Step 5 — Wire the Spawners in the Scene

Open the `TempleRunObstacles` and `TempleRunCollectables` scenes.

### ObstacleSpawner

Locate (or add) the `ObstacleSpawner` MonoBehaviour.

| Inspector Field | What to Assign |
|-----------------|----------------|
| **Full Width Obstacle Prefab** | A barrier that spans all lanes (player must jump) |
| **Lane Obstacle Prefab** | A barrier in one lane (player can dodge left/right) |
| **Full Width Probability** | 0–1. `0.3` = 30% chance of full-width. |
| **Min Distance From Segment Start** | Dead zone at the start of the segment (prevents spawning at turn points) |
| **Min Distance From Segment End** | Dead zone at the end |
| **Obstacle Height / Depth** | Collider dimensions when using default primitive fallback |
| **Platform Height** | Y offset above the track surface |
| **Prefab Registry** | Drag your `SpawnPrefabRegistry` asset here |

### CoinSpawner

| Inspector Field | What to Assign |
|-----------------|----------------|
| **Coin Config** | Drag your `CoinConfig` ScriptableObject |
| **Prefab Registry** | Drag your `SpawnPrefabRegistry` asset |

`CoinConfig` fields:

| Field | Description |
|-------|-------------|
| `CoinPrefab` | Default coin prefab (used for Procedural mode and slot fallback) |
| `PlatformHeight` | Y offset above track |
| `MinDistanceFromSegmentStart` | Dead zone at start |
| `MinDistanceFromSegmentEnd` | Dead zone at end |
| `MinCoinsPerLine` | Minimum coins in a procedurally spawned line |
| `MaxCoinsPerLine` | Maximum coins |
| `SpacingBetweenCoins` | Distance between consecutive coins |
| `CoinValue` | Value added to score on collection |

### PowerUpSpawner

| Inspector Field | What to Assign |
|-----------------|----------------|
| **Power Up Definitions** | Array of `PowerUpDefinition` ScriptableObjects |
| **Min Distance From Segment Start** | Dead zone |
| **Min Distance From Segment End** | Dead zone |
| **Platform Height** | Y offset |
| **Prefab Registry** | Drag your `SpawnPrefabRegistry` asset |

`PowerUpDefinition` fields:

| Field | Description |
|-------|-------------|
| `PowerUpId` | String key — must match `PrefabTag` in SpawnSlots |
| `Prefab` | The power-up prefab |
| `TintColor` | Fallback colour when no prefab assigned |
| `SpawnWeight` | Relative probability in weighted random selection |

### PrefabSpawnerAbstract (visual tracks)

The visual track tiles are placed by components extending `PrefabSpawnerAbstract`. These subscribe to `SplineSegmentCreated` automatically. Assign your tile prefab to the `_prefab` field and adjust `_widthScale` / `_heightScale` to fit your art.

---

## Step 6 — Set the Active Level at Runtime

The level is selected by a plain **`int` level number** that travels through the event
system — GameFlow never references a track type, and the track system never references
GameFlow:

```
GameFlow: LevelConfigApplier publishes LevelApplied(int LevelNumber)
    ↓  TempleRunGameFlowBridge
TempleRun: TempleRunLevelApplied → stored on Blackboard.SelectedLevel
    ↓  at TrackManager init
TrackLibraryLoader.Load(_trackLevels, Blackboard.Instance.SelectedLevel)
    → finds the TrackLevelSO with a matching LevelNumber
    → merges the registry pool filtered by its tags/ids
    → returns the runtime TrackSegmentLibrary
```

To make your new level playable:

1. **Assign the level registry** — in the `TempleRunTrackPCG` scene, the `TrackManager`
   component's `_trackLevels` field must reference
   `Assets/TempleRun/Scriptables/Track/TrackLevelRegistry.asset` (which must contain your
   `TrackLevelSO` — Step 4.9)
2. **Match the level number** — the GameFlow `LevelConfig` used by level selection must have
   the same `LevelNumber` as your `TrackLevelSO`

If no level is selected (or no `TrackLevelSO` matches the number), `TrackManager` falls back
to purely procedural segments using `MinTrackLength` / `MaxTrackLength` — handy for quick
testing, but none of your authored segments will appear.

---

## Step 7 — Playtest and Iterate

### Quick playtest without UGS
1. Open `0_BootStrap` scene
2. Disable the `Load_UGS_Init` GameObject in the Hierarchy
3. Enable **CrawfisSoftware > Play Scene 0 Always** (toggle in menu bar)
4. Press Play

### Enable event logging
Turn on `CrawfisSoftware > Events > Log Events`. Every `TrackSegmentCreated` and `SplineSegmentCreated` event will be printed to the Console, showing the segment ID, direction, length, and spawn mode. Good for verifying the correct segments are selected.

### Common iteration checklist

| Problem | Likely cause | Fix |
|---------|-------------|-----|
| No obstacles on a segment | `ObstacleSpawnRate` = 0, or SpawnMode = Preset with no slots | Check `DifficultyConfig.ObstacleSpawnRate`; add slots or switch to Procedural |
| Tag listed but segment not appearing | Tag mismatch (case-sensitive), or segment missing from the registry | Verify tag strings match exactly in the `TrackSegmentSO` and `TrackLevelSO`, and that the segment is in `TrackSegmentRegistry.asset` |
| Segments repeating too much | `MaxRepeat` too high or too few segments with that tag | Increase `MaxRepeat` gate or add more segments |
| Track turning same direction repeatedly | `GetNewDirection()` not seeded or only one direction in library | Ensure both Left and Right segments are tagged and ticked |
| Wrong prefab spawning | PrefabTag not in registry or wrong registry assigned | Add entry to `SpawnPrefabRegistry` and confirm the asset is assigned to the spawner |
| Tiles have visible seams | Segment length not a multiple of `TileLength` | Switch to `TrackManagerForTiles`, or set all segment lengths to multiples of tile size |

---

## Reference: Track Data Model

Four ScriptableObject types in `Assets/TempleRun/Scriptables/Track/`:

### `TrackSegmentSO` (one asset per segment)

The authored fields for a single segment — see the
[field reference in Step 3](#step-3--author-segment-assets-and-register-them).

### `TrackSegmentRegistrySO` (`TrackSegmentRegistry.asset`)

A single `Segments` array of `TrackSegmentSO` references — the shared pool every level
draws from.

### `TrackLevelSO` (`TrackLevel_*.asset`)

| Field | Description |
|-------|-------------|
| `LevelNumber` | How the level is resolved at runtime — must match the GameFlow `LevelConfig.LevelNumber` |
| `LevelName` | Shown in menus |
| `DifficultyRating` | 0–10, informational |
| `LaneCount`, `LaneWidth` | Lane configuration for this level |
| `Registry` | Reference to the shared `TrackSegmentRegistrySO` |
| `StartSegmentId` | The segment that always plays first |
| `ActiveSegmentTags` | OR logic — a segment is included if it has **any** of the listed tags |
| `ActiveSegmentIds` | Include specific segments by id; when non-empty, takes precedence over tags. Both empty = whole pool |

### `TrackLevelRegistrySO` (`TrackLevelRegistry.asset`)

A single `Levels` array of `TrackLevelSO` references. Assigned to
`TrackManager._trackLevels`; this is the asset that maps the selected level number to a
track ruleset.

> **Segment connections:** the runtime `TrackSegmentDefinition` supports optional
> `Connections` (graph edges restricting which segments may follow which). These are not
> currently exposed on the authoring SOs.

---

## Reference: SpawnMode Guide

| Mode | Behaviour | When to Use |
|------|-----------|-------------|
| `Procedural` (default) | Random placement driven by `DifficultyConfig` spawn rates. Ignores `SpawnSlots`. | General segments where exact layout doesn't matter |
| `Preset` | Only the objects defined in `SpawnSlots` are placed. Spawn rates are ignored. | Hand-authored challenge segments with a specific obstacle pattern |
| `Hybrid` | `Required` slots are placed first, then normal procedural fill runs on top | Segments with a guaranteed obstacle or coin but otherwise random |

**Tip:** Start most segments as `Procedural`. Move to `Preset` only for segments where the specific obstacle pattern is part of the design challenge.

---

## Reference: Difficulty Config

Spawn rates and movement parameters come from `DifficultyConfig` ScriptableObjects, one per difficulty level, stored in the `TempleRunGameConfig.DifficultyConfigs[]` array.

| Property | Default | Effect |
|----------|---------|--------|
| `InitialSpeed` | 5.0 | Player speed at game start |
| `MaxSpeed` | 80.0 | Speed cap after acceleration |
| `Acceleration` | 0.2 | Speed increase per second |
| `StartRunway` | 8 | Initial segment count before first obstacle |
| `MinTrackLength` | 4 | Minimum random segment length (fallback path only) |
| `MaxTrackLength` | 19 | Maximum random segment length (fallback path only) |
| `ObstacleSpawnRate` | 1.0 | Per-segment probability (0–1) of spawning an obstacle |
| `CoinSpawnRate` | 0.8 | Per-segment probability of spawning a coin line |
| `PowerUpSpawnRate` | 0.15 | Per-segment probability of spawning a power-up |
| `NumberOfLives` | 2 | Player lives before game over |

> **Note:** `MinTrackLength` and `MaxTrackLength` only affect the **fallback** path in `TrackManager` (when no segment library is loaded). When using authored segments, the length comes from the segment's geometry (`ToPivotDistance + ExitDistance`).
