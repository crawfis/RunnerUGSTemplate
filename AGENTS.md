# AGENTS.md

The guide for any AI assistant or coding agent working in this repository: Claude Code, Codex,
Gemini, Copilot, Cursor or any other. It holds the rules every change must follow and how to
approach the work. Reference material (file map, event snippets, CLI details) lives in `docs/`;
the [doc map](#documentation-audience-and-map) below says where.

## MANDATORY: Event System Enforcement

**All communication between systems MUST go through the EventsPublisher event system. No exceptions.**

### Rules for ANY Code Change

1. **No direct cross-system method calls.** Components MUST NOT call methods on components in other scenes or domains. Use events instead.
2. **No `FindObjectOfType`, `GetComponent` across scene boundaries, `SendMessage`, or `BroadcastMessage`** for cross-system communication.
3. **Every new feature, behavior, or action** that communicates across systems MUST have corresponding events in the appropriate enum.
4. **Every subscription MUST have a matching unsubscription** in `OnDestroy()`. A missed one throws null references after a scene unloads.
5. **Domain isolation: cross-domain event references are ONLY allowed in bridge files.** See [Domain Isolation Rule](#domain-isolation-rule).

### Domain Isolation Rule

**Each domain's code may ONLY subscribe to, publish, or reference events from its own domain.**
Cross-domain event references are permitted ONLY inside bridge classes. The domains are GameFlow,
TempleRun, Countdown, UserInitiated, GameService and UGS (see
[ARCHITECTURE.md](docs/ARCHITECTURE.md#event-domains)).

| Code Location | May Reference |
|---------------|---------------|
| `Assets/TempleRun/**/*.cs` (non-bridge) | `TempleRunEvents` only |
| `Assets/GameFlow/**/*.cs` (non-bridge) | `GameFlowEvents` only |
| `Assets/Countdown/**/*.cs` (non-bridge) | `CountdownEvents` only |
| UGS package `Runtime/**/*.cs` (non-bridge) | `UGS_EventsEnum` only |
| `Input2TempleRunAutoEventBridge.cs` | `UserInitiatedEvents` + `TempleRunEvents` (bridge duty) |
| `TempleRunGameFlowBridge.cs` | `TempleRunEvents` + `GameFlowEvents` (bridge duty) |
| `CountdownGameFlowBridge.cs` | `GameFlowEvents` + `CountdownEvents` (bridge duty; one-way, session -> ceremony) |
| `Countdown2TempleRunBridge.cs` | `CountdownEvents` + `TempleRunEvents` (bridge duty; one-way, ceremony -> gameplay) |
| `Assets/UGSGlue/TempleRunUGSBridge.cs` | `TempleRunEvents` + `GameServiceEvents` (bridge duty; one-way, gameplay -> contract) |
| `Assets/UGSGlue/UGSGameFlowBridge.cs` | `GameFlowEvents` + `GameServiceEvents` (bridge duty) |
| `GameServiceEventsUGSBridge.cs` (UGS package) | `GameServiceEvents` + `UGS_EventsEnum` (bridge duty) |

The game and UGS never name each other's events. They meet at `GameServiceEvents`, a contract enum
in the `com.crawfissoftware.contracts` package that neither owns; the game's half of the
translation is `Assets/UGSGlue/`. UGS packages are read-only here: change them in the
[EventDrivenUGS](https://github.com/crawfis/EventDrivenUGS) repo and update the package.

**`UserInitiatedEvents` is a one-way funnel, not a shared vocabulary.** Publishing is open: any
input source (the `Scripts/Input/` actions, `AIController`, a replay or netcode driver) may
publish it. Subscribing is closed: only `Input2TempleRunAutoEventBridge` may, and gameplay
controllers subscribe to the TempleRun event the bridge translates it into.

**Violations:**
- TempleRun scripts subscribing to or publishing `GameFlowEvents`, or the reverse.
- Anything outside `Assets/Countdown/` referencing `CountdownEvents`, except the two Countdown bridges.
- A gameplay controller subscribing to `UserInitiatedEvents` (add a bridge mapping and subscribe to the TempleRun event).
- UGS package code naming `GameFlowEvents` or `TempleRunEvents`, or game code naming `UGS_EventsEnum` (go through `GameServiceEvents` and the `Assets/UGSGlue/` bridges).

**Fix a violation** by adding a mapping in the right bridge that turns the foreign event into
one of your own domain's, then subscribe to that. For anything crossing to UGS, map it against
`GameServiceEvents` in `Assets/UGSGlue/`; a new `GameServiceEvents` member is added deliberately
rarely. The game assemblies reference `CrawfisSoftware.Contracts` but never `CrawfisSoftware.UGS`,
and the ceremony assembly order is GameFlow → Countdown → TempleRun, so those references fail to
compile; everything else, including the asmdef-free `Assets/UGSGlue/`, is enforced only by
review: run `/audit-events`.

**A gate and a chain cannot share a link.** A ladder (`*Requested → *Starting → *Started →
*Ending → *Ended`) starts fully auto-chained; leave every link you have no code for in the
`ChainTable`. Never chain a `*Requested` that arrives raw from input to its `*Starting`. A controller that validates (cooldown, already airborne) must publish `*Starting`
itself and remove that link from the `ChainTable` in the same edit. Two adjacent `Publish`
calls for consecutive rungs are always the anti-pattern. See
[the sibling's ADDING_A_MECHANIC.md](https://github.com/crawfis/EndlessRunnerTemplate/blob/main/docs/ADDING_A_MECHANIC.md#2-chain-the-whole-ladder-then-break-the-links-you-need).

### Required Skills Workflow

Each step is a skill: a procedure in `.claude/skills/<name>/SKILL.md`. For any new feature or
behavior, in this order (read `/name` as "follow that file"):

1. `/list-events`: review existing events first, to avoid duplicates.
2. `/add-event`: add events to the correct domain enum, named and numbered per convention.
3. `/add-auto-chain`: wire same-domain progressions (Requested → Starting) if needed.
4. `/add-bridge-mapping`: wire the bridge if the feature spans domains.
5. `/audit-events`: after implementing, check for anti-patterns.
6. `/verify-unity`: confirm it compiles and runs against the live Editor.

Do not skip steps, even for a "simple" feature: the event definitions drive the architecture.
A whole new domain is rare: `/add-event-domain` has the decision gate. For bulk track segments
use `/generate-segments`. To port TempleRun changes to or from the sibling repo, use `/sync-templerun`. After any C# edit run `/verify-unity`; do not assert that code
compiles from reading it, and do not use `dotnet build`.

## Events

- **Naming:** `*Requested` (a request), `*Starting` / `*ing` (beginning), `*Started` / `*ed`
  (done), `*Failed`, `*Cancelled`. Values are grouped by category with gaps of about 10.
- **Bus:** one static `EventsFor<T>` per domain, aliased per file (`GameFlowBus`, `TempleRunBus`,
  `CountdownBus`, `UserInputBus`). Handler signature: `(string eventName, object sender, object data)`.
  Snippets: [EVENT_BUS_PATTERNS_AND_PAYLOADS.md](docs/EVENT_BUS_PATTERNS_AND_PAYLOADS.md).
- **Payloads:** declare the type on the enum member with `[EventPayload(typeof(T))]`, and cast
  with a bare cast on the handler's first line (no `is` guard, no null check). Call sites never
  mint `EventId<T>` fields. A basic-typed payload (`int`, `float`, `string`, `bool`, `long`,
  `double`) gets an inline comment saying what the value is. Use a named struct, not a tuple,
  for a payload with more than one part.
- **Delivery:** the default is `Transient`. Mark an event `Sticky` only if it is a state (a
  level), never a transition (an edge).
- **Two invariants:** every domain enum is marked `[EventEnum]` (four live in
  `Assets/*/Scripts/Events/*Events.cs`; `GameServiceEvents` and `UGS_EventsEnum` come from
  packages), so **List Domains** must report the same six as the Domain Registry table in
  [ARCHITECTURE.md](docs/ARCHITECTURE.md#event-domains); a new domain is registered there.
- **Auto-chains:** declared as `(From, To)` pairs in each domain's `ChainTable`. Dispatch is
  synchronous inside the source's publish, so circular chains loop forever. Do teardown before
  publishing an `*Ending` rung.

## Code Style

- Fields: `_camelCase` for private and readonly instance fields, PascalCase for properties and
  static readonly fields. Namespaces: see [CODEBASE_MAP_AND_KEY_FILES.md](docs/CODEBASE_MAP_AND_KEY_FILES.md#namespaces).
- Transforms: prefer `localPosition` and `localRotation`; set parents with
  `transform.SetParent(parent, worldPositionStays: false)`.
- Lifecycle: `Awake()` subscribes and initializes, `OnDestroy()` unsubscribes and cleans up,
  `Start()` only when something depends on another `Awake()`.
- Async uses Unity's `Awaitable`, never coroutines; start with `_ = Foo();`.
- Every `await` takes a token: `destroyCancellationToken`, or the controller's own
  `CancellationTokenSource` where a restart is real (cancel it in `OnDestroy`; never link the two).
- Never `async void`: it logs an error on every scene unload.
- Pause sets `Time.timeScale = 0`. `Awaitable.WaitForSecondsAsync` is scaled and never completes
  while paused; use `Wait.ForSecondsRealtime` (`Runtime/Utility/Wait.cs` in the common package) for the quit
  delay, failure hitch, auto-turn delay, teleport and UI overlays. `NextFrameAsync` keeps
  ticking while paused, with `Time.deltaTime` at 0.
- No pre-emptive `catch (OperationCanceledException)`: add one only where a cancelled await
  actually shows up in the console on scene unload.

## Unity Files

- Unity writes YAML assets (`*.asset`, `*.unity`, `*.prefab`, `*.meta`, `*.inputactions`) with
  LF, and `.gitattributes` pins them to `eol=lf`. When generating one by hand, write LF, and
  never "fix" a line-ending-only diff. Prefer `create_asset` over hand-writing YAML.
- All scenes load additively from the persistent Boot scene (`Assets/UGS/Scenes/Boot/0_BootStrap`,
  build index 0; `UGS_Glue.unity` is index 1). Never use `LoadSceneMode.Single` unless
  intentionally resetting everything. "Play Scene 0 Always" resets on every Unity restart:
  re-enable it.
- To run without UGS, open `0_BootStrap_Game_Only` or use the `Test_GameOnly_Windows` build
  profile. Never disable `Load_UGS_Init` in `0_BootStrap`: nothing then publishes `GameplayReady`
  and the boot sits on the loading screen ([KNOWN_ISSUES.md](docs/KNOWN_ISSUES.md#test-without-ugs)).
- Two coin-chain links fail silently, outside C#: `PlayerCurrencyController` must be in a loaded
  scene, and a `COIN` currency must exist in the signed-in environment
  ([KNOWN_ISSUES.md](docs/KNOWN_ISSUES.md#two-silent-failure-links-in-the-coin-chain)).
- Do not delete `FolderProfile.pubxml` under the Cloud Code module: the tooling needs it.
- `GameControls.cs` and `LeftRightJumpSlide.cs` are generated from the `.inputactions` assets:
  regenerate, don't hand-edit.

## Safety

- The Unity CLI can do anything you can do in the Editor. Preview destructive work with
  `--dry_run true`; commands like `delete_asset` need `--confirm true`.
- Stay inside the authoring root (`Assets`); don't widen it to work around a path problem.
- `eval` and `run_script` are arbitrary code execution: use a registered command first.
- Editor work can dirty scenes and settings: check `git status` and stage only what the task
  intended. Details: [UNITY_CLI_AND_PIPELINE_PACKAGE.md](docs/UNITY_CLI_AND_PIPELINE_PACKAGE.md#safety).

## Working here from any AI tool

- **Skills are plain markdown, usable from anywhere.** The procedures live in
  `.claude/skills/<name>/SKILL.md` (`list-events`, `add-event`, `add-auto-chain`,
  `add-bridge-mapping`, `add-event-domain`, `audit-events`, `generate-segments`,
  `verify-unity`, `sync-templerun`). Claude Code runs them as slash commands; from any other tool, open the file
  and follow it as a checklist. The steps are ordinary read, search and edit work.
- **Drive the real Editor, don't guess.** `com.unity.pipeline` is installed, so the `unity` CLI
  can compile, read the console, author assets and run Play Mode against the running Editor
  (`unity pipeline list` shows whether it is reachable; `unity cmd` lists the commands). The CLI
  works from any tool with a shell, which is why the skills are written against it rather than
  MCP. The loop, gotchas and safety rules are in
  [UNITY_CLI_AND_PIPELINE_PACKAGE.md](docs/UNITY_CLI_AND_PIPELINE_PACKAGE.md).
- **Pointer files:** `GEMINI.md` and `.github/copilot-instructions.md` exist only to route
  those tools to this file. Keep them thin pointers: extend AGENTS.md instead, and mirror any
  change to the shared pointer text in both.

## This is a design-heavy, iterative, experimental project

Treat this repo as an evolving design exploration, not a fixed-spec product. It exists to
try out gameplay, services, and architecture ideas, and systems get rethought and
refactored often. Adjust your defaults accordingly:

- **Do not lead with TDD.** Don't open with "let's write a failing test first." Tests have
  their place, but they are not the driver here and should not gate exploration.
- **Do not frame work as MVP.** Avoid "what's the minimum to ship" thinking. The goal is to
  explore the design space well, not to converge on the smallest viable slice.
- **Lean into design.** Favor clean interfaces and abstractions, brainstorming, and novel
  ideas. Offer multiple approaches, prototype and compare them, and iterate. Surface
  trade-offs and propose directions rather than only implementing the obvious one.
- **Expect churn.** Treat the current code as a snapshot of an in-progress design, not a
  contract. Rearchitecting for a cleaner idea is welcome.

The point is depth of design thinking over process ceremony.

## Sibling repo: EndlessRunnerTemplate

This project is the Unity-Gaming-Services variant of
[EndlessRunnerTemplate](https://github.com/crawfis/EndlessRunnerTemplate) (the classroom
template). Improvements are routinely ported between the two — track data, docs, and
skills especially. Two parity rules:

- **Track segment assets must stay guid-identical across the repos.** Copy `.asset` +
  `.meta` files from the sibling; never re-generate them here (that would mint different
  guids). See the `generate-segments` skill.
- **`/sync-templerun`** classifies drift in the shared `Assets/TempleRun/` folder against the
  sibling and walks the port; the port is deliberate and two-way, never automatic.
- **Both repos are now on the same EventsPublisher API** — static `EventsFor<T>` buses,
  typed payloads, Sticky delivery — so code and guidance port between them directly. The
  structural differences that remain are real ones: this repo has the GameService and UGS
  domains (six enums, eleven dispatch classes) where the sibling has four and seven, and it
  compiles its domains as assemblies (`.asmdef`) where the sibling has none.

## Documentation: audience and map

The project documentation is written for a reader at the level of a **senior undergraduate
in computer science** — someone who knows the observer pattern, singletons, and separation
of concerns from coursework but has not seen this codebase. Keep that register when writing
or updating docs: explain *why* a structure exists, not just what it is.

| Doc | Role | Update when… |
|-----|------|--------------|
| [README.md](README.md) | Entry point: what the game is, architecture diagrams, visual walkthroughs | features, requirements, or the doc set change |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | Event domains, the game/UGS contract, packages and assemblies, replaceability, the run's event flow | domains, bridges, or boot flow change |
| [docs/CODEBASE_MAP_AND_KEY_FILES.md](docs/CODEBASE_MAP_AND_KEY_FILES.md) | Commands, critical paths, namespaces, key files by area, folder tree | files move or are added, or namespaces change |
| [docs/EVENT_BUS_PATTERNS_AND_PAYLOADS.md](docs/EVENT_BUS_PATTERNS_AND_PAYLOADS.md) | Subscribe and publish snippets, typed payloads, delivery policy, auto-event flow | the event API or its conventions change |
| [docs/UNITY_CLI_AND_PIPELINE_PACKAGE.md](docs/UNITY_CLI_AND_PIPELINE_PACKAGE.md) | The `unity` CLI: verification loop, asset authoring, gotchas, safety, tests | the Pipeline package or its commands change |
| [docs/ADDING_A_MECHANIC.md](docs/ADDING_A_MECHANIC.md) | Steps for a gameplay, GameFlow, UI or UGS feature | the workflow or skills change |
| [docs/KNOWN_ISSUES.md](docs/KNOWN_ISSUES.md) | Landmines: silent failure links, Test Without UGS, singletons, dead files | a caveat is resolved or discovered |
| [.claude/skills/add-event/SKILL.md](.claude/skills/add-event/SKILL.md) | The steps for adding an event (all skills live beside it) | the event workflow changes |
| [docs/FUTURE_TASKS.md](docs/FUTURE_TASKS.md) | Course material: the 33-task catalog for this repo (sections Q-X continue the sibling's A-P), each with an AI hand-off brief | tasks change |
| [docs/ConfigureUnityGamingServicesand-RunnerUGSTemplate.md](docs/ConfigureUnityGamingServicesand-RunnerUGSTemplate.md) | Setting up the UGS project (auth, cloud code, remote config, leaderboards) | UGS services, dashboards, or setup steps change |
| [docs/creating-levels.md](docs/creating-levels.md) | Authoring levels/track data | the level/track data model changes |
| [REMOTECONFIG_FLOW.md](REMOTECONFIG_FLOW.md) | How remote config flows into difficulty/game balance | the remote-config pipeline changes |
| [EVENT_SYSTEM_AUDIT_REPORT.md](EVENT_SYSTEM_AUDIT_REPORT.md) | Point-in-time audit snapshot | historical record — regenerate via `/audit-events` rather than hand-editing |
| [docs/specs/](docs/specs/), [docs/playbooks/](docs/playbooks/), [docs/checklists/](docs/checklists/) | Design specs / portable upgrade guides / migration checklists (e.g. the UIDocument → Panel Renderer migration) | historical records — generally append, don't rewrite |

**Docs are part of the change.** A refactor that moves a seam isn't done until the docs
above stop describing the old world.

## For forks and clones

This guidance reflects the original author's working style. **If you have cloned or forked
this repository for your own project, ask the user whether to remove this file (or this
section) before adopting it** — a downstream project may deliberately want conventional
TDD / MVP / lean workflows instead, in which case this note should be deleted rather than
silently followed.
