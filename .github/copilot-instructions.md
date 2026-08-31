# GitHub Copilot instructions

> Thin pointer, kept in sync with `GEMINI.md`. The real guidance lives in the two files
> below — extend those, not this file.

This repository keeps its AI-agent guidance in two root files, written for **any** coding
agent (not just the tools they are named after):

1. **[AGENTS.md](../AGENTS.md)** — how to approach work here (design-first, docs are part
   of the change).
2. **[CLAUDE.md](../CLAUDE.md)** — the mandatory concrete guide: event-system rules,
   coding conventions, key file paths.

Read both before changing code. The non-negotiable core:

- **ALL cross-system communication goes through the event system** — the four singleton
  static buses (`EventsFor<T>`, aliased `GameFlowBus`/`TempleRunBus`/`UserInputBus`/`SignalsBus`/`UGSBus`) — never
  direct method calls, `FindObjectOfType`, `SendMessage`, or cross-scene `GetComponent`.
- **Domain isolation:** `Assets/TempleRun/**` may reference only `TempleRunEvents` /
  `UserInitiatedEvents`; `Assets/GameFlow/**` only `GameFlowEvents`; `Assets/UGS/**` only
  `UGS_EventsEnum`. Cross-domain event references live ONLY in the two bridges:
  `TempleRunGameFlowBridge.cs` and `UGSGameFlowBridge.cs`.
- **Every `SubscribeToEvent` (usually in `Awake()`) has a matching `UnsubscribeToEvent` in
  `OnDestroy()`.**
- **Events come first.** Add or change events by following the step-by-step procedures in
  `.claude/skills/<name>/SKILL.md` (plain markdown, tool-agnostic). When a doc mentions a
  slash command such as `/add-event`, that means: follow the corresponding skill file.
