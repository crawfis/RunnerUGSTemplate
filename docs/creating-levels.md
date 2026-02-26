# Creating Levels with New Prefabs

This guide walks through the complete workflow for creating a new level in RunnerUGS — from placing new prefabs in the project all the way to seeing them in a playtest.

---

## Table of Contents

1. [Concepts Overview](#1-concepts-overview)
2. [Track Manager Variants](#2-track-manager-variants)
3. [Step 1 — Prepare Your Prefabs](#step-1--prepare-your-prefabs)
4. [Step 2 — Register Prefabs in SpawnPrefabRegistry](#step-2--register-prefabs-in-spawnprefabregistry)
5. [Step 3 — Add Segments to the Registry JSON](#step-3--add-segments-to-the-registry-json)
6. [Step 4 — Create the Level with the Track Level Editor](#step-4--create-the-level-with-the-track-level-editor)
7. [Step 5 — Wire the Spawners in the Scene](#step-5--wire-the-spawners-in-the-scene)
8. [Step 6 — Set the Active Level at Runtime](#step-6--set-the-active-level-at-runtime)
9. [Step 7 — Playtest and Iterate](#step-7--playtest-and-iterate)
10. [Reference: JSON Schema](#reference-json-schema)
11. [Reference: SpawnMode Guide](#reference-spawnmode-guide)
12. [Reference: Difficulty Config](#reference-difficulty-config)

---

## 1. Concepts Overview

The track generation system has three layers:

```
JSON Data Layer
  TrackSegments_Registry.json   ← shared library of all segment definitions
  TrackLevel_*.json             ← level-specific filter + settings

Runtime Selection Layer
  TrackManager                  ← picks the next segment from the library
  TrackSegmentLibrary           ← weighted selection engine

3D Geometry + Spawning Layer
  SplineCreator2D               ← converts segment data to Vector3 splines
  ObstacleSpawner               ← places obstacles on each spline segment
  CoinSpawner                   ← places coin lines
  PowerUpSpawner                ← places power-ups
  PrefabSpawnerAbstract         ← places visual track tiles
```

A **segment** is a straight piece of track with a turn at the end. Each segment defines:
- Its **length** (distance before the turn)
- Its **direction** (Left, Right, or Both)
- Its **spawn mode** (how obstacles/coins/power-ups are placed on it)
- Optional **spawn slots** (exact positions for Preset or Hybrid modes)

A **level** is a JSON file that selects a subset of segments from the registry using **tags** and configures lane count, lane width, and difficulty.

---

## 2. Track Manager Variants

The TrackManager is the component on the `TrackManager` GameObject in the `TempleRunTrackPCG` scene. Three variants are available:

### `TrackManager` (default)
The general-purpose manager. Reads segment definitions from JSON. Segment lengths come from the definition's `Length` field (or a random value between `MinTrackLength` and `MaxTrackLength` for the fallback path).

**Direction logic:** 40% Left, 40% Right, 20% Left (randomised when no definition overrides it).

**Best for:** Most levels. Use this unless you have a specific tiling or fixed-pattern requirement.

**Inspector fields:**
| Field | Description |
|-------|-------------|
| `_numberOfLookAheadTracks` | How many segments to keep in the queue ahead of the player (default 12) |
| `_trackSegmentLibraryJson` | Optional fallback TextAsset if `Blackboard.TrackLevelDefinition` is not set |

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

The `SpawnPrefabRegistry` ScriptableObject decouples JSON `PrefabTag` strings from Unity asset GUIDs. Spawners look up prefabs here at runtime.

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

## Step 3 — Add Segments to the Registry JSON

Open `Assets/TempleRun/Resources/TrackSegments_Registry.json` in any text editor or the Unity Inspector.

Each entry in `"Segments"` is one track segment. Here is a complete annotated example:

```json
{
  "Id": "desert_left_20",
  "Direction": "Left",
  "Length": 20.0,
  "Weight": 1.5,
  "MaxRepeat": 2,
  "DifficultyRating": 3.0,
  "Tags": ["desert", "medium"],
  "Role": "Normal",
  "SpeedMultiplier": 1.0,
  "SpawnMode": "Hybrid",
  "VisualTheme": "desert",
  "SpawnSeed": 0,
  "BlockedLanes": [],
  "LaneHeights": [],
  "ActiveLanes": [],
  "SpawnSlots": [
    {
      "NormalizedPosition": 0.35,
      "Lane": -1,
      "Height": 0,
      "Type": "Obstacle",
      "PrefabTag": "rock_low",
      "Weight": 1.0,
      "Required": true
    },
    {
      "NormalizedPosition": 0.65,
      "Lane": 0,
      "Height": 0,
      "Type": "Coin",
      "PrefabTag": "coin_gold",
      "Weight": 0.8,
      "Required": false
    }
  ]
}
```

### Field reference

| Field | Type | Description |
|-------|------|-------------|
| `Id` | string | Unique identifier — no spaces, use underscores |
| `Direction` | string | `"Left"`, `"Right"`, or `"Both"` |
| `Length` | float | Segment length in world units |
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

Instead of hand-editing JSON you can run:

```
/generate-segments
```

The skill prompts you for direction, length range, difficulty range, and tags, then generates and appends well-formed entries to the registry.

---

## Step 4 — Create the Level with the Track Level Editor

### Opening the editor
**Menu:** `CrawfisSoftware > Track Level Editor`

The window is split into three panels:

```
┌─────────────┬────────────────────────────┬──────────────────┐
│  LEVELS     │  LEVEL PROPERTIES          │  SEGMENT PREVIEW │
│             │                            │                  │
│ [Level 01]  │  Level Name: Desert Run    │  difficulty bar  │
│ [Level 02]  │  Level Number: 6           │                  │
│ > Level 06  │  Difficulty: ──●──── 4.5   │  [L] left_20 ... │
│             │                            │  [R] right_14 .. │
│ + New Level │  Lane Count: 3             │  ...             │
│             │  Lane Width: 2.0           │                  │
│             │                            │                  │
│             │  Registry File: TrackSeg.. │                  │
│             │  Start Segment: start      │                  │
│             │                            │                  │
│             │  ☑ opening                 │                  │
│             │  ☑ desert                  │                  │
│             │  ☐ temple                  │                  │
│             │  ☐ beginner                │                  │
│             │                            │                  │
│             │  [Save] [Revert]  [Dupl]   │                  │
└─────────────┴────────────────────────────┴──────────────────┘
```

### Creating a new level

1. Click **+ New Level** — a file named `TrackLevel_NN_New.json` is created in `Assets/TempleRun/Resources/`
2. Set **Level Name** (shown in game menus)
3. Set **Level Number** (used for progression ordering)
4. Set **Difficulty Rating** (0–10, informational — actual spawn rates come from `DifficultyConfig`)
5. Set **Registry File** — filename without extension, e.g. `TrackSegments_Registry`
6. Set **Start Segment ID** — the `Id` of the segment that always plays first (e.g., `"start"`)
7. **Tick Active Tags** — any segment whose `Tags` list contains at least one ticked tag will be included in this level
8. Click **Save**

The file is written immediately to the Resources folder and imported by AssetDatabase.

### Reading the preview panel

The right panel shows every segment that would be active for this level, colour-coded by difficulty (green = easy, red = hard).

Each row:
```
[L] desert_left_20   d=3.0  L=  20 [H] (Challenge) S=3
 ↑  ↑                ↑       ↑    ↑   ↑              ↑
dir id               diff   len  mode  role        slot count
```

- **`[L]`** / **`[R]`** / **`[B]`** = turn direction
- **`[P]`** = Preset, **`[H]`** = Hybrid (absent = Procedural)
- **`(Challenge)`** = Role, if not Normal
- **`S=3`** = 3 spawn slots defined

Below the segment list:
- **Difficulty histogram** — bar chart of segments per difficulty bucket (0–9)
- **L= R= B=** — direction balance count
- **Preset/Hybrid count** — how many segments have pre-authored layouts
- **Roles summary** — e.g., `Challenge:4  Reward:2`

### Tips

- **Balance Left/Right** — aim for equal counts to prevent the track feeling biased.
- **Cover the full difficulty range** — a histogram with even coverage allows smooth ramping.
- **Duplicate an existing level** to start from a known baseline — use the **Duplicate** button.
- **Revert** discards unsaved changes and reloads from disk.

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

`TrackManager` reads the level from `Blackboard.Instance.TrackLevelDefinition`. This is set by whatever level-selection flow your game uses. Two common approaches:

### A. Assign directly from a MonoBehaviour
```csharp
// In a level-selection controller, before the game scene loads:
var levelJson  = Resources.Load<TextAsset>("TrackLevel_06_Desert");
var level      = JsonUtility.FromJson<TrackSegmentLibraryDefinition>(levelJson.text);
Blackboard.Instance.TrackLevelDefinition = level;
```

### B. Let TrackManager fall back to an Inspector TextAsset
Drag a level JSON `TextAsset` directly onto `TrackManager._trackSegmentLibraryJson` in the Inspector. This is the quickest setup for prototyping a single level.

### C. Use the fallback resource name
If neither of the above is set, `TrackManager` loads `Resources/TrackSegments.json` as a legacy single-file fallback. This is a catch-all for testing only.

---

## Step 7 — Playtest and Iterate

### Quick playtest without UGS
1. Open `0_BootStrap` scene
2. Disable the `Load_UGS_Init` GameObject in the Hierarchy
3. Enable **CrawfisSoftware > Play Scene 0 Always** (toggle in menu bar)
4. Press Play

### Enable event logging
Turn on `CrawfisSoftware > Events > Event Logging Enabled`. Every `TrackSegmentCreated` and `SplineSegmentCreated` event will be printed to the Console, showing the segment ID, direction, length, and spawn mode. Good for verifying the correct segments are selected.

### Common iteration checklist

| Problem | Likely cause | Fix |
|---------|-------------|-----|
| No obstacles on a segment | `ObstacleSpawnRate` = 0, or SpawnMode = Preset with no slots | Check `DifficultyConfig.ObstacleSpawnRate`; add slots or switch to Procedural |
| Tag ticked but segment not appearing | Tag mismatch (case-sensitive) | Verify tag strings match exactly in both registry and editor |
| Segments repeating too much | `MaxRepeat` too high or too few segments with that tag | Increase `MaxRepeat` gate or add more segments |
| Track turning same direction repeatedly | `GetNewDirection()` not seeded or only one direction in library | Ensure both Left and Right segments are tagged and ticked |
| Wrong prefab spawning | PrefabTag not in registry or wrong registry assigned | Add entry to `SpawnPrefabRegistry` and confirm the asset is assigned to the spawner |
| Tiles have visible seams | Segment length not a multiple of `TileLength` | Switch to `TrackManagerForTiles`, or set all segment lengths to multiples of tile size |

---

## Reference: JSON Schema

### TrackSegments_Registry.json (top level)
```json
{
  "Version": "2.0",
  "Segments": [ /* array of TrackSegmentDefinition */ ]
}
```

### TrackLevel_*.json (top level)
```json
{
  "Version": "2.0",
  "LevelName": "Desert Run",
  "LevelNumber": 6,
  "DifficultyRating": 4.5,
  "LaneCount": 3,
  "LaneWidth": 2.0,
  "SegmentRegistryFile": "TrackSegments_Registry",
  "StartSegmentId": "start",
  "ActiveSegmentTags": ["desert", "opening"],
  "ActiveSegmentIds": [],
  "Segments": [],
  "Connections": []
}
```

`ActiveSegmentTags` — OR logic. A segment is included if it has **any** of the listed tags.
`ActiveSegmentIds` — Include specific segments by ID regardless of tags.
`Connections` — Optional graph edges: `[{ "FromId": "seg_a", "ToId": "seg_b" }]`. If defined for a segment, only listed `ToId` segments can follow it.
`Segments` — Leave empty in the file. Populated at runtime by merging the registry.

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

> **Note:** `MinTrackLength` and `MaxTrackLength` only affect the **fallback** path in `TrackManager` (when no segment library is loaded). When using JSON segments, the length comes directly from the segment definition's `Length` field.
