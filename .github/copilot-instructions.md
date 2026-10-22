# GitHub Copilot instructions

> Thin pointer, kept in sync with `GEMINI.md`. The real guidance lives in AGENTS.md
> — extend that, not this file.

This repository keeps its AI-agent guidance in one root file, written for **any** coding
agent (not just the tools it is named after):

**[AGENTS.md](../AGENTS.md)** holds the mandatory rules (event system, domain isolation, skills
workflow), the code conventions, and a map of the reference docs under `docs/`
(`docs/ARCHITECTURE.md`, `docs/CODEBASE_MAP_AND_KEY_FILES.md`,
`docs/EVENT_BUS_PATTERNS_AND_PAYLOADS.md`, `docs/UNITY_CLI_AND_PIPELINE_PACKAGE.md`).

Read it before changing code. The non-negotiable core:

- **ALL cross-system communication goes through the event system** — the six static buses
  (`EventsFor<T>`, aliased `GameFlowBus`/`TempleRunBus`/`UserInputBus`/`CountdownBus`/`GameServiceBus`/`UGSBus`) — never
  direct method calls, `FindObjectOfType`, `SendMessage`, or cross-scene `GetComponent`.
- **Domain isolation:** `Assets/TempleRun/**` may reference only `TempleRunEvents` /
  `UserInitiatedEvents`; `Assets/GameFlow/**` only `GameFlowEvents`; `Assets/Countdown/**`
  only `CountdownEvents`; the UGS package only `UGS_EventsEnum`. Cross-domain event
  references live ONLY in the bridge files: `Input2TempleRunAutoEventBridge.cs`,
  `TempleRunGameFlowBridge.cs`, `CountdownGameFlowBridge.cs`, `Countdown2TempleRunBridge.cs`,
  the two bridges in `Assets/UGSGlue/`, and `GameServiceEventsUGSBridge.cs` in the ugs
  package. The game and UGS never name each other's events — they meet at
  `GameServiceEvents` (contracts package).
- **One subscribe/publish style.** Use the bus alias directly and cast the payload on the
  handler's first line; the `static readonly EventId<T> X = Bus.Id<T>(...)` mint pattern was
  removed by owner ruling (2026-09) and must not be reintroduced. `[EventPayload]` on the
  enum member stays the contract.
- **Every `Subscribe` (usually in `Awake()`) has a matching `Unsubscribe` in
  `OnDestroy()`.**
- **Events come first.** Add or change events by following the step-by-step procedures in
  `.claude/skills/<name>/SKILL.md` (plain markdown, tool-agnostic). When a doc mentions a
  slash command such as `/add-event`, that means: follow the corresponding skill file.
