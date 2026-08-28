# Third-party notices — Unity Building Blocks

Everything under this folder is **vendored third-party source**, not code this project wrote.
It originates from three Unity Technologies "Building Block" tutorial projects distributed
through the Unity Asset Store.

| Product | Asset Store product id | Version | Files kept here |
|---------|------------------------|---------|-----------------|
| Unity Building Block — Achievements   | 341918 | 1.0 | 63 |
| Unity Building Block — Leaderboards   | 341926 | 1.1 | 11 |
| Unity Building Block — Player Account | 341928 | 1.0 |  7 |

Every file still carries its original `AssetOrigin` block in its `.meta`, so the product,
version and upstream path are recoverable per file:

```
grep -A5 "AssetOrigin:" <file>.meta
```

## Licence

These files are **not** covered by this repository's `LICENSE.txt` (CC0-1.0). They are Unity
Asset Store content, licensed under the Asset Store EULA as *Extension Assets*. The upstream
packages shipped no `LICENSE` or `NOTICE` file of their own — only per-block `README.md`
usage docs, which were not kept.

If this project is ever published as a UPM package or otherwise redistributed, the licence
terms for these files need to be checked before this folder travels with it.

## How this folder came to be

The full import lived at `Assets/Blocks/` (135 files, ~2.9 MB) with four of its own assembly
definitions. It was pruned to the transitive closure actually reachable from `Assets/UGS`
— 81 files — and moved here so `Assets/UGS` is self-contained.

The internal directory shape is **deliberately preserved**. `BlocksRuntimeTheme.tss` and the
USS files under `Common/Content/` reference each other by *relative path*, not by GUID
(`@import url("Content/stylesheets/core.uss")`, `url("../textures/thumbnail.png")`,
`PlayerAccountStyle.uss` → `../../../Textures/Error@64.png`). Flattening or re-nesting this
tree silently breaks the theme: a failed `@import` invalidates the whole stylesheet and every
panel that uses it falls back to Unity's default theme. Move files inside this folder only if
you also fix those 27 path strings.

The four upstream runtime assembly definitions (`Blocks.Common`, `Blocks.Achievements`,
`Blocks.Leaderboards`, `Blocks.PlayerAccount`) were removed; this source now compiles into
`CrawfisSoftware.UGS`. The editor-side importer for `.ach` files kept its own assembly, renamed
to `CrawfisSoftware.UGS.Editor`.

**Namespaces were left as `Blocks.*`** so the vendored source stays byte-identical to upstream
and can be diffed against a fresh Asset Store import. One of them is load-bearing beyond C#:
`Assets/UGS/UI/PlayerAccountLogin.uxml` instantiates
`<Blocks.PlayerAccount.PlayerSignIn/>` **by fully-qualified type name**, so renaming that
namespace breaks the sign-in screen at runtime with no compile error.

## Local modifications

Four files diverge from the pristine import. Re-importing any block from the Asset Store would
overwrite them:

| File | Changed in |
|------|-----------|
| `Leaderboards/Scripts/Runtime/LeaderboardsObserver.cs` | `beff9e5` |
| `Common/BlocksPanelSettings.asset` | `3d60225` |
| `Achievements/Scripts/Editor/UI/Templates/Achievements.ach.txt` | `beff9e5` |
| `Achievements/Deployment/Achievements.ach` | `16cfbf7` — this project's own achievement definitions, not upstream sample data |

`Achievements.ach` is the only in-repo record of the achievement set. `DistanceBasedAchievements.cs`
hard-codes the id `first_achievement`, which is defined nowhere else; the runtime reads these
definitions back out of Remote Config, so the file has to be deployed before achievements work
in a fresh Unity project.

## Forks that live outside this folder

Two Blocks classes were forked into `Assets/UGS/Scripts/Achievements/` rather than kept here,
because they were rewritten for `PanelRenderer` instead of `UIDocument`:
`AchievementsPrefab` and `AchievementsNotificationPrefab`. Their upstream originals were
deleted with the rest of the unused import. Same for
`Assets/UGS/UI/PlayerAccountLogin.uxml`, a stripped-down copy of the
Player Account sample scene's UXML.
