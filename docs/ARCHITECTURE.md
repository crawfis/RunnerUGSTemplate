# Architecture

How the pieces fit together: the event domains, how the game meets Unity Gaming Services
through a contract, and what makes a domain replaceable. The rules are in
[AGENTS.md](../AGENTS.md); the file map is in
[CODEBASE_MAP_AND_KEY_FILES.md](CODEBASE_MAP_AND_KEY_FILES.md).

## Event domains

Unity 6 endless runner demonstrating **event-driven architecture** with Unity Gaming Services.

**Domain Registry** — the authoritative list of event domains:

| Domain | Enum | Bus alias | Purpose | Lives in | Bridges |
|--------|------|-----------|---------|----------|---------|
| **GameFlow** | `GameFlowEvents` | `GameFlowBus` | App lifecycle: loading, menus, sessions, pause, config/difficulty, save/load, quit | `Assets/GameFlow/` | ↔ TempleRun via `TempleRunGameFlowBridge`; ↔ GameServiceEvents via `UGSGameFlowBridge` |
| **TempleRun** | `TempleRunEvents` | `TempleRunBus` | Gameplay: player lifecycle and activation, movement, collisions, coins/power-ups, track/spline generation, teleportation | `Assets/TempleRun/` | ↔ GameFlow via `TempleRunGameFlowBridge`; → GameServiceEvents via `TempleRunUGSBridge`; ← Countdown via `Countdown2TempleRunBridge` |
| **Countdown** | `CountdownEvents` | `CountdownBus` | Session ceremony: the pre-run 3…2…1 (start, ticks, end) and its overlay. Its own assembly, `CrawfisSoftware.Countdown` | `Assets/Countdown/` | ← GameFlow via `CountdownGameFlowBridge` (`GameStarting → CountdownStartRequested`); → TempleRun via `Countdown2TempleRunBridge` (`CountdownEnded → PlayerActivateRequested`) |
| **UserInitiated** | `UserInitiatedEvents` | `UserInputBus` | Raw input requests (turns, lanes, jump, slide, dash, pause, quit) | `Assets/TempleRun/` | → TempleRun via `Input2TempleRunAutoEventBridge` |
| **GameService** | `GameServiceEvents` | `GameServiceBus` | The game/service **contract**: score, currency total, session start/end, services status, remote config applied. Owned by neither side | `com.crawfissoftware.contracts` package | ↔ GameFlow and ← TempleRun via `Assets/UGSGlue/`; ↔ UGS via `GameServiceEventsUGSBridge` |
| **UGS** | `UGS_EventsEnum` | `UGSBus` | Unity Gaming Services: init, auth, remote config, leaderboards, achievements, **economy/currency**, rewarded ads | `com.crawfissoftware.ugs` package | ↔ GameServiceEvents via `GameServiceEventsUGSBridge` — and nothing else |

Two invariants keep this registry trustworthy:
- **Placement:** four domain enums live in `Assets/*/Scripts/Events/`; the other two come from
  packages (`GameServiceEvents` from contracts, `UGS_EventsEnum` from ugs). All six are marked
  `[EventEnum]`, so **List Domains** reports the same six as this table: a domain in one list
  and not the other is drift. The `EventsPublisher*` singleton subclasses are gone — the buses
  are static and need no scene object.
- **Registration:** `/add-event-domain` adds a row here as part of its checklist, and
  `/audit-events` flags drift between this table and the code.

## The contract and the packages

**The game and UGS no longer name each other's events at all.** They meet in the middle at
`GameServiceEvents`, a third enum in `com.crawfissoftware.contracts` that neither owns. The game's half
of that translation lives in `Assets/UGSGlue/`; the services half is `GameServiceEventsUGSBridge` inside
the UGS package. That is what lets either side be replaced without the other being edited.

> The UGS domain is no longer in this repository. It ships as three UPM packages resolved by git
> URL in `Packages/manifest.json` — `com.crawfissoftware.contracts`, `.common` and `.ugs`, from
> [EventDrivenUGS](https://github.com/crawfis/EventDrivenUGS) — so their code is **read-only here**;
> change it in that repo and update the package. The game's own assemblies (`CrawfisSoftware.TempleRun`,
> `.GameFlow`, `.Countdown`, `.ThirdParty`) reference `CrawfisSoftware.Contracts` but never
> `CrawfisSoftware.UGS`, so a game-to-UGS reference is a compile error. Everything *within* a domain,
> and the deliberately asmdef-free `Assets/UGSGlue/`, is still only enforced by review and
> `/audit-events`; run it.
>
> **Assembly layout (this repo only — the sibling has no asmdefs).** The Countdown domain owns
> `Assets/Countdown/CrawfisSoftware.Countdown.asmdef`, which references `CrawfisSoftware.TempleRun`
> (its outbound bridge names `TempleRunEvents`); `CrawfisSoftware.GameFlow` in turn references
> `CrawfisSoftware.Countdown` (it hosts `CountdownGameFlowBridge`). The dependency direction is
> therefore **GameFlow → Countdown → TempleRun**, and it is acyclic on purpose: gameplay never
> names the ceremony, and the ceremony never names the session. A reference added in the opposite
> direction will not compile — which is the point.

### Why isolation: replaceability

The rule's purpose is **replaceability**: a domain that talks only through events can be
swapped for a completely different implementation — or stubbed out with a trivial fake —
without touching code on the other side. This repo proves it at full scale, in both
directions: the `Test_GameOnly_Windows` build profile — equivalently, opening
`Assets/GameFlow/Scenes/Boot/0_BootStrap_Game_Only` — runs the entire game with the UGS domain
absent, and `Test_UGS_Windows` runs the UGS services against a dummy game with random scores. Domains load from their own scenes,
so replacing a domain is loading a different scene that speaks the same events.

## Domain responsibilities

- **Common** (the `com.crawfissoftware.common` package, not a folder here): shared base classes and
  utilities used across all domains — the one dispatch implementation, the live `DifficultyConfig`
- **GameFlow**: Application lifecycle - boot, initialization, menus, level select, pause, quit, scene management
- **TempleRun**: Gameplay mechanics - player movement and activation, track generation, power-ups, input, audio
- **Countdown**: The session ceremony - the pre-run 3...2...1 and its overlay. Started by GameFlow's
  `GameStarting`, and its end reaches gameplay only as `PlayerActivateRequested`. Replace this domain
  with a cutscene, a "tap to start" gate, or nothing at all and neither neighbour is edited
- **UGS** (the `com.crawfissoftware.ugs` package): Unity Gaming Services - authentication,
  leaderboards, achievements, remote config, economy. What remains under `Assets/UGS/` is assets:
  scenes, prefabs, the Cloud Code module, `COIN.ecc`
- **Vendored** (CloudCode bindings, `Assets/ThirdParty/`, LevelPlay, …): sample and utility code
  outside the domain rule.

## Event Flow Architecture

```
USER INPUT (UserInitiatedEvents, in TempleRun)
    ↓  Input2TempleRunAutoEventBridge
TEMPLERUN GAMEPLAY (TempleRunEvents)
    ├─→ TempleRunGameFlowBridge ──→ GAMEFLOW SESSION (GameFlowEvents)
    │        ▲                            │    ↕  Assets/UGSGlue/UGSGameFlowBridge
    │        │                            │  CountdownGameFlowBridge (GameStarting)
    │        │                            ▼
    │   Countdown2TempleRunBridge ←── COUNTDOWN CEREMONY (CountdownEvents)
    │   (CountdownEnded -> PlayerActivateRequested)
    │
    └─→ Assets/UGSGlue/TempleRunUGSBridge ──→ THE CONTRACT (GameServiceEvents)
                                                  ↕  GameServiceEventsUGSBridge   (UGS package)
                                              UGS SERVICES (UGS_EventsEnum)
```

The countdown loop is a detour, not a relay. `GameStarting` starts the ceremony, and GameFlow
declares `GameStarted` through its own `GameStarting → GameStarted` chain rather than waiting for
the countdown to come back. `GameStarted` brings the run's systems up (`TempleRunStartRequested`); the ceremony's
end releases the player (`PlayerActivateRequested → PlayerActivating → PlayerActivated`). Gameplay
cannot tell whether a countdown, a cutscene, or nothing at all sat in that gap.

Neither end names the other. The game speaks `GameServiceEvents`; the services layer speaks
`GameServiceEvents`; the enum belongs to neither and lives in its own package. Everything
game-specific — that a "score" is metres run, that a "currency total" is coins — is translated in
`Assets/UGSGlue/`.
