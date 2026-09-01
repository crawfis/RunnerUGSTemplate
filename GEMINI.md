# GEMINI.md

> Thin pointer, kept in sync with `.github/copilot-instructions.md`. The real guidance
> lives in the two files below — extend those, not this file.

This repository keeps its AI-agent guidance in two root files, written for **any** coding
agent (not just the tools they are named after):

1. **[AGENTS.md](AGENTS.md)** — how to approach work here (design-first, docs are part of
   the change).
2. **[CLAUDE.md](CLAUDE.md)** — the mandatory concrete guide: event-system rules, coding
   conventions, key file paths.

Read both before changing code. The non-negotiable core:

- **ALL cross-system communication goes through the event system** — the five static buses
  (`EventsFor<T>`, aliased `GameFlowBus`/`TempleRunBus`/`UserInputBus`/`GameServiceBus`/`UGSBus`) — never
  direct method calls, `FindObjectOfType`, `SendMessage`, or cross-scene `GetComponent`.
- **Domain isolation:** `Assets/TempleRun/**` may reference only `TempleRunEvents` /
  `UserInitiatedEvents`; `Assets/GameFlow/**` only `GameFlowEvents`; the UGS package only
  `UGS_EventsEnum`. Cross-domain event references live ONLY in the bridge files:
  `Input2TempleRunAutoEventBridge.cs`, `TempleRunGameFlowBridge.cs`, the two bridges in
  `Assets/UGSGlue/`, and `GameServiceEventsUGSBridge.cs` in the ugs package. The game and
  UGS never name each other's events — they meet at `GameServiceEvents` (contracts package).
- **Every `Subscribe` (usually in `Awake()`) has a matching `Unsubscribe` in
  `OnDestroy()`.**
- **Events come first.** Add or change events by following the step-by-step procedures in
  `.claude/skills/<name>/SKILL.md` (plain markdown, tool-agnostic). When a doc mentions a
  slash command such as `/add-event`, that means: follow the corresponding skill file.
