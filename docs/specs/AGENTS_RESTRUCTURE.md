# Plan: port the AGENTS.md restructure from EndlessRunnerTemplate

**Status:** planned, not started. Runs after EndlessRunnerTemplate's restructure has merged; that
repo's `docs/specs/AGENTS_RESTRUCTURE.md` is the main plan, and this one lists only what differs.
**Baseline tag:** `pre-agents-restructure`, the 897-line CLAUDE.md as it shipped. Diff `AGENTS.md`,
`CLAUDE.md` and the new docs against that tag to see the before and after.
**Why:** the same as the sibling: students without Claude Code must find every rule in AGENTS.md,
but today AGENTS.md (91 lines) says "read CLAUDE.md in full", and CLAUDE.md is 897 lines.

> **How to use:** this doc is written to double as a session prompt: paste it whole into a fresh
> Claude Code session, or point one at the file. First read EndlessRunnerTemplate's
> `docs/specs/AGENTS_RESTRUCTURE.md` and its merged AGENTS.md, CLAUDE.md and new docs (on GitHub or
> in `../EndlessRunnerTemplate`), then this repo's AGENTS.md and CLAUDE.md. It is a documentation
> change: no scene, prefab or asset changes, and the only C# edit is one comment that cites
> CLAUDE.md (`Assets/TempleRun/Scripts/Events/TempleRunEvents.cs`). Work on a branch named
> `agents-restructure`, one PR.

## Decisions (Roger, 2026-10-22)

All seven decisions in the sibling's plan apply here unchanged: the 300-line, 24 KB limit for
AGENTS.md; every link updated in the same change with anchor headings kept; the same doc names; the
pointer files and the `audit-events` and `add-event-domain` skills repointed; the `EventId<T>` ruling
note dropped (it appears here at CLAUDE.md lines 220 and 260 and in `audit-events`; line 283
restates the rule, which AGENTS.md says once); this repo second. Decision 7 is Endless-only.

## What to copy, what to adapt

- **Copy byte for byte** from the sibling: `docs/UNITY_CLI_AND_PIPELINE_PACKAGE.md` and
  `docs/EVENT_BUS_PATTERNS_AND_PAYLOADS.md`. Then add this repo's differences in its own files, not
  in those two, so the copies stay identical. CLAUDE.md lines 570-667 here match the sibling's CLI
  section apart from the heading; check that before copying.
- **Same text, adapted:** the AGENTS.md rule block (the 5 MANDATORY rules, Required Skills Workflow,
  events, code style, async, scene loading, safety). The Domain Isolation table differs: keep this
  repo's 11 rows.
- **New here, existing in the sibling:** `docs/ARCHITECTURE.md`, `docs/KNOWN_ISSUES.md` and
  `docs/ADDING_A_MECHANIC.md`. Start each from the sibling's structure and fill it with this repo's
  blocks (map below); append CLAUDE.md lines 709-722 ("Adding a New UGS Feature") to
  `ADDING_A_MECHANIC.md` as its UGS steps.
- **Port from the sibling** (missing here): the Delivery Policy section (it lives in the shared
  patterns doc) and the Unity YAML and line endings rule (one AGENTS.md line).
- **UGS-only, keep:** the `sync-templerun` skill (add it to the skill list and the doc map),
  "UGS packages are read-only here", the assembly layout, Test Without UGS, and the two silent
  failure links (currency controller, COIN).

## Section map

Actions and short names as in the sibling's plan: AGENTS, DOC, COVERED, CUT; MAP, PAT, CLI, ARCH,
ISS, plus MECH = `docs/ADDING_A_MECHANIC.md`.

| CLAUDE.md lines | Heading | Action | Destination and notes |
|---|---|---|---|
| 1-7 | Title and intro | CUT | Replaced by `@AGENTS.md`; the old pointer is circular |
| 9-15 | Sibling repo note | COVERED | AGENTS.md "Sibling repo" section; add `sync-templerun` to its skill list |
| 17-20 | Course material (FUTURE_TASKS) | AGENTS | A row in the doc map |
| 22-45 | Quick Reference, Essential Commands | AGENTS / DOC | Lines 26-29 to AGENTS; menus, scenes and build profiles to MAP |
| 47-60 | Critical Paths | DOC | MAP; merge with Key Files (438-498), the same data twice |
| 62-72 | MANDATORY rules 1-5 | AGENTS | Verbatim, identical to the sibling |
| 74-90 | Domain Isolation table | AGENTS | Keep the 11-row table |
| 92-95 | Contract enum paragraph | DOC | ARCH |
| 97-104 | Violations, how to fix | AGENTS | Three bullets plus the fix line |
| 106-121 | Package read-only, assembly layout | DOC | ARCH; one line "UGS packages are read-only here" to AGENTS |
| 123-129 | Replaceability, test profiles | DOC | ARCH (README.md line 355 covers the profiles) |
| 131-149 | Required Skills Workflow | AGENTS / COVERED | 133-138 already in AGENTS.md; steps 140-149 to AGENTS |
| 151-163 | When to Use Each Skill | COVERED | The steps above plus skill descriptions; the `/sync-templerun` row to AGENTS |
| 165-187 | Architecture Overview, Domain Registry | DOC | ARCH. `audit-events` and `add-event-domain` check this table: update their paths |
| 191-223 | Subscribing; the ruling note | DOC / CUT | Snippet to PAT; ruling note CUT |
| 225-253 | Publishing Events | DOC | PAT |
| 255-299 | Typed Payloads | AGENTS / DOC | Three rules to AGENTS, snippets to PAT; ruling clause CUT |
| 301-323 | Auto-Event Flow Pattern | DOC | PAT; 320-323 (countdown "used to occupy 30-36") CUT, history |
| 325-339 | Chains as a pair list | DOC | PAT |
| 341-342 | Never chain a raw `*Requested` | AGENTS | One line; a hard rule |
| 344-378 | Adding New Events | COVERED | `.claude/skills/add-event/SKILL.md`; a doc map row for non-Claude tools |
| 380-385 | Event Naming Conventions | AGENTS | Five lines |
| 389-408 | Namespaces | DOC | MAP (README.md line 87 has a copy; link it) |
| 410-436 | Field naming, XML docs, transforms, lifecycle | AGENTS | About 20 lines |
| 438-498 | Key Files Reference | DOC | MAP. The pubxml landmine (489) to ISS, one line in AGENTS |
| 502-509 | Gotchas: Event Subscriptions | AGENTS | Repeats rule 4; keep the named-struct line (506-509) |
| 511-534 | Async and coroutines | AGENTS | The sibling's five bullets; CUT the "from 0.5.0" notes and the controller list |
| 536-539 | Scene Loading | AGENTS | Two lines; "Play Scene 0 Always" is in README.md line 311 |
| 541-545 | Auto-Event Flow gotchas | AGENTS | One line |
| 547-557 | Singletons, Design Notes | DOC | ISS |
| 559-568 | Dead and placeholder files | DOC / CUT | 562-566 to ISS (both files still commented out); 560-561 and 567-568 CUT, already gone |
| 570-595 | CLI intro, Pipeline vs MCP | DOC | CLI (copied from the sibling) |
| 597-614 | The verification loop | COVERED | `.claude/skills/verify-unity/SKILL.md` |
| 616-629 | Authoring assets | COVERED | `.claude/skills/generate-segments/SKILL.md` |
| 631-651 | CLI gotchas | DOC | CLI, version-free heading |
| 653-667 | Safety | AGENTS / DOC | Four bullets to AGENTS; full text to CLI |
| 669-680 | Testing: verify, run the suite | COVERED / DOC | verify-unity; `unity test` flags to CLI |
| 682-697 | Test Without UGS | DOC | ISS (landmine: never disable `Load_UGS_Init`); one line in AGENTS |
| 699-707 | Event logging, Build Profiles | COVERED | README.md lines 321 and 355-389 |
| 709-722 | Adding a New UGS Feature | DOC | MECH, as its UGS steps 5-8 |
| 724-741 | Gameplay and GameFlow feature steps | COVERED | Required Skills Workflow |
| 743-749 | Modifying UI Panels | COVERED | `docs/playbooks/uidocument-to-panel-renderer.md` |
| 751-834 | Folder Structure | DOC | MAP |
| 836-850 | Domain Responsibilities | DOC / CUT | ARCH; 848-850 (the Blocks tree "is gone") CUT |
| 852-880 | Event Flow Architecture | DOC | ARCH, heading text kept for the anchor; 870-875 ("old round-trip is gone") CUT |
| 882-891 | Worked example, one coin | COVERED | README.md line 226 ("How the Glue Works") |
| 892-897 | Two silent failure links | DOC | ISS; these are real landmines |

## The Claude layer and the checks

CLAUDE.md becomes the sibling's Claude layer plus `/sync-templerun` in the skill line. Run the
sibling's checks here too, with these paths: the files that link CLAUDE.md today are the two skills,
`.github/copilot-instructions.md`, AGENTS.md, `TempleRunEvents.cs`, `EVENT_SYSTEM_AUDIT_REPORT.md`,
`GEMINI.md`, `docs/FUTURE_TASKS.md`, `docs/creating-levels.md`, `docs/prospectus.md` and
`docs/talk/delete-half-your-game.html`. Finally, `diff` the two shared docs against the sibling's:
they must be identical.
