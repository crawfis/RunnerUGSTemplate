# AGENTS.md

Guidance for AI assistants working in this repository — any assistant, not just Claude.
This file is about *how to approach the work*, not the code specifics. The concrete
architecture, conventions, and event-system rules live in [CLAUDE.md](CLAUDE.md):
**read that file in full before changing code** — despite the name it is written for
every AI tool, and its event-system rules are mandatory for every change.

## Working here from any AI tool

- **Skills are plain markdown, usable from anywhere.** The event-workflow procedures live
  in `.claude/skills/<name>/SKILL.md` (`list-events`, `add-event`, `add-auto-chain`,
  `add-bridge-mapping`, `add-event-domain`, `audit-events`, `generate-segments`). Claude
  Code runs them as slash commands; from any other tool (Copilot, Cursor, Codex, Gemini,
  …), open the skill file and follow it as a checklist — the steps are ordinary
  Read/Grep/Edit work and assume nothing Claude-specific. Wherever a doc says
  `/add-event`, read it as "follow `.claude/skills/add-event/SKILL.md`".
- **Pointer files:** `GEMINI.md` and `.github/copilot-instructions.md` exist only to route
  those tools to this file pair. Keep them thin pointers — extend AGENTS.md / CLAUDE.md
  instead, and mirror any change to the shared pointer text in both.

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
| [CLAUDE.md](CLAUDE.md) | AI-assistant rules: event-system enforcement, conventions, file reference | rules, conventions, or key paths change |
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
