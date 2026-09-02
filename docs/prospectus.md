# What RunnerUGS Is Ready to Say in Public

*Paper & talk prospectus · September 2026*

Three publishable claims live in the RunnerUGSTemplate / EventDrivenUGS pair, with most of
their evidence already committed. This document ranks them, maps the evidence, and lays out
the venue timing, the one experiment worth running, and what to fix before writing.

```
Input         UserInitiatedEvents  ·   9
      →  Input2TempleRun
Gameplay      TempleRunEvents      · 121
      ⇄  TempleRunGameFlow
Session       GameFlowEvents       ·  76
      ⇄  UGSGlue
The Contract  GameServiceEvents    ·  12
      ⇄  GameServiceEventsUGS
Services      UGS_EventsEnum       ·  47
```

Five `[EventEnum]` domains, eight dispatch classes on one pair-table base. Either end runs
with the other absent — and that is the story.

## 01 — The claims: three theses, one recommendation

Each of these could carry a submission on its own. They stand on the same codebase, but
they aim at different audiences, and the strongest move is to lead with the one whose
evidence is cheapest to complete.

### Thesis A — Making AI coding assistants follow the architecture

CLAUDE.md states the law, seven skill files are the procedures, `/audit-events` is the
verifier, asmdefs are the compile-time fence, and pointer files route every agent brand to
the same stack. The repo is a working, inspectable instance of "how do you make an agent
*preserve* an architecture" — and it supports a cheap, real experiment (§4).

**Verdict: Lead.** Novel, timely, and the study is runnable in days.

### Thesis B — Replaceability you can demo: the event-contract seam

`GameServiceEvents` is a vocabulary neither side owns. The game ships a build profile with
the services layer absent; the services ship one with the game replaced by a dummy that
publishes the same contract events with random scores. Plus the lesson inside `Sticky`
events: **broadcast the current status, not just the moment it changed**, so a scene that
loads late still hears the answer instead of missing an event that fired before it existed.

**Verdict: Strong second** — and the best *talk* of the three.

### Thesis C — A capstone template that proves its own claims

CSE 5912 experience report: three template generations, domain-swap assignments, the audit
skill as a grading instrument, CC0 availability. Weakest alone — it wants cohort data —
but it is the framing that makes A and B land at education venues.

**Verdict: Fold** into A or B as the context section, until a cohort has used the current
form.

## 02 — The evidence map: what is already committed, and what it proves

| Evidence | Where it lives | Supports |
|---|---|---|
| Both directions provable in a build: `Test_GameOnly_Windows` runs the whole game with UGS absent; `Test_UGS_Windows` runs the services against a dummy game publishing `SessionEnding` with random scores | Build profiles; `Test_SubmitLeaderboardScore` | B, C |
| The contract enum with reasoned payload rules — difficulty payload deliberately undeclared because "a contract that references the game's types is not a contract" | `GameServiceEvents.cs` XML docs | B |
| Retained (`Sticky`) status events — `ServicesStatusChanged`, `DifficultySettingsApplied`, `CurrencyBalanceChanged` — each annotated in the code with the boot-stall or blank-HUD bug it removes | Contract + enum sources, REMOTECONFIG_FLOW.md | B (Sticky) |
| Pair tables over dictionaries: one source, several consequences — `SessionEnding` → `ScoreUpdating` + `CurrencySyncRequested`; the dictionary ceiling produced workarounds, not bugs | Common package; CLAUDE.md rationale | A, B |
| The silent-failure catalog: the only `CurrencySyncRequested` subscriber must be scene-hosted; Economy config is per-environment; scene names and Remote Config keys must match exactly with nothing checking them at compile time; "four defects a build could not show" | ugs package README, sample README | B, C |
| The agent stack: 631-line CLAUDE.md, AGENTS.md working-style contract, 7 tool-agnostic skills, GEMINI/Copilot pointer files, audit-as-verification | Repo root, `.claude/skills/` | A |
| The extraction record: scenes rebuilt for a UPM sample via a batch audit → GUID remap → re-audit pipeline with a pass/fail finish line: zero unresolved references | EventDrivenUGS `docs/samples-handoff.md`, `Tools~/` | A, B |
| Live-service validation: a play session banked coins into a real Economy `COIN` balance and the dashboard agreed; remote difficulty applied end-to-end | Changelogs; project memory | B |
| Lineage: TempleRun1-NoArt → EndlessRunnerTemplate → RunnerUGS, each generation adding a glue layer; sibling repo kept GUID-compatible for porting | READMEs, both repos | C |

> **Honesty item for any demo:** the 2026-09-01 sweep found that the `Test_UGS_Windows`
> profile no longer loads any publisher of `GameplayReady` (the UGSGlue extraction moved it
> into `UGS_Glue.unity`, which that profile omits). Fix and play-test before this profile
> is shown on stage or cited in print — it is one scene-list entry plus one loader.

## 03 — Venues and timing: where each claim lands

| Venue | Form | Fit | Cycle (typical — verify current deadlines) |
|---|---|---|---|
| **GDC** (Programming or Education Summit) — the natural home for Thesis B as a session | 45-min talk | B (best fit) | Submissions ~Aug; conference March |
| **SIGCSE TS / ITiCSE** — experience report or paper; ITiCSE if timing slips | 6-page paper | A + C | SIGCSE ~Jul–Aug; ITiCSE ~Jan |
| **FDG** (Foundations of Digital Games) — game-engineering + education tracks both apply | Full or short paper | B, C | ~Jan–Feb |
| **IEEE Software** — practitioner case study: "designing for replaceable domains" | Magazine article | B | Rolling |
| **ICSE SEET / FSE Industry** — if the §4 study runs with numbers | Paper | A | ~Oct (ICSE) |
| **Onward! (SPLASH)** — essay form suits the "agents need constitutions" argument | Essay | A | ~Apr |

Sequencing that compounds: GDC submission (B, talk) and the §4 study (A) can proceed in
parallel this fall; the study's numbers then upgrade the SIGCSE/ICSE-SEET submission from
anecdote to result. The talk's demo materials — two build profiles, the coin trace — are
reusable as the paper's supporting materials, and CC0 licensing means reviewers can simply
be handed the repos.

## 04 — The experiment: the study that puts numbers behind Thesis A

The question reviewers will ask is "do the rules and skills actually change what an AI
assistant writes?" The repo is unusually well set up to answer it, because the checker
already exists.

| Design element | Plan |
|---|---|
| **Tasks** (4–6, real feature shapes) | Add a magnet power-up variant; add a streak achievement; add an analytics domain; add a new input method; wire a "continue run" rewarded-ad flow (the enum members already exist, unpublished) |
| **Conditions** | (1) bare repo, guidance stripped · (2) CLAUDE.md only · (3) full stack: CLAUDE.md + AGENTS.md + skills + an audit pass the agent is told to run |
| **Agents** | 2–3 brands (the pointer files exist precisely so Copilot/Gemini follow the same rules), at least three runs of each combination |
| **Metrics** (scored by `/audit-events` + compile) | Domain-isolation violations · missing `OnDestroy` unsubscribes · events placed in the wrong enum · the documented trap (auto-chaining a raw input `*Requested` past validation) · compile success · diff size |
| **Cost** | Days, not weeks: no human subjects, the audit skill does the grading, and every run is a throwaway branch |

> Side benefit: the study doubles as a stress test of the guidance itself — every violation
> committed in condition (3) is, by definition, a hole in the rules worth fixing. The
> experiment improves the repos even if the paper never ships.

## 05 — Outlines

### The talk (Thesis B, 40 min)

> **The built deck lives at [talk/delete-half-your-game.html](talk/delete-half-your-game.html)**
> — a self-contained HTML slide deck in the same format as the sibling repo's, expanding this
> outline to 26 slides plus a four-slide appendix (the counted numbers, a Q&A sheet, the
> 30-minute cut, and the five places a live-services change lands). Keys: `←`/`→` advance,
> `N` speaker notes, `O` index, `T` timer, `B` blank, `F` fullscreen; append `?all` to the URL
> to reveal every fragment; print to PDF for a handout. Every number on its slides was
> re-counted against `main` on 2026-09-02 and the commands are in the appendix.

1. **Cold open** (3 min): build the game with the services layer deleted — it ships. Build
   the services with the game deleted — they run against a dummy game. No slides yet, just
   the two profiles.
2. **The registry** (5 min): five domains, one rule — cross-domain references live only in
   bridges. Why the rule buys replaceability, not tidiness.
3. **One coin's journey** (7 min): live event log from `CoinCollected` to a
   dashboard-visible Economy balance, naming every hop and both halves of the glue.
4. **The contract** (6 min): why an enum neither side owns beat interfaces and DI here;
   what is deliberately *not* in it; the payload rule.
5. **Late joiners still need the answer** (6 min): the boot that stalled with no error, and
   how a retained (`Sticky`) status event makes the race impossible rather than unlikely.
6. **What compiles isn't what runs** (6 min): the silent-failure catalog — scene hosting,
   per-environment config, names that must match exactly with no compiler to catch them.
7. **AI assistants on the team** (5 min): the rulebook — CLAUDE.md, the skills, the audit —
   in 5 minutes, ending on a live `/audit-events` pass.
8. **Take-homes** (2 min) + both repos, CC0.

### The paper (A with C as context, 6 pages)

1. **Context:** the capstone, the template lineage, why replaceability is the learning
   objective.
2. **Architecture:** domains, static buses, chains as pair tables, bridges, the contract
   package. One figure: the domain chain.
3. **The layers of enforcement:** compile-time (asmdefs), step-by-step (skills), checking
   (the audit skill), and cultural (docs are part of every change). What each layer catches
   that the others cannot.
4. **Study:** §4 design, results, and a catalog of the violations seen.
5. **Lessons:** retained status events; one event with several consequences; the
   silent-failure catalog; what the tools still get wrong even with all the guidance in
   place.
6. **Related work, limitations, availability.** Comparisons to write honestly: UnityEvents
   / ScriptableObject event channels, MessagePipe, Zenject signals, VContainer — and the
   emerging CLAUDE.md/AGENTS.md convention literature.

## 06 — Working titles

- **Delete Half Your Game and Ship It Anyway** — talk · Thesis B — the cold open as the title
- **Skills as Guardrails: Teaching AI Coding Agents to Preserve an Architecture** — paper · Thesis A
- **Events at the Seams: A Contract-Enum Architecture for Replaceable Domains** — paper · Thesis B, IEEE Software register
- **Publish the State, Not the Moment** — short talk / blog — the Sticky-event insight on its own

## 07 — What to fix first

- **Fix the Test_UGS wiring gap** (load `UGS_Glue` from `0_BootStrap_UGS_Only` and add it
  to the profile), then play-test both test profiles — they are the paper's central demo
  and must work.
- **Tag the package releases** in EventDrivenUGS (`v0.4.0` at the contracts/common bump,
  `v0.5.0` at ugs) so citations and manifests can pin what the text describes.
- **Regenerate EVENT_SYSTEM_AUDIT_REPORT.md** via `/audit-events` — the current snapshot
  predates the package extraction, and a fresh clean report is quotable evidence.
- **Run the §4 study** before the SIGCSE/ICSE-SEET cycle; even a small N moves the paper
  from claim to result.
- **Screenshots:** re-capture the authentication, leaderboard, and achievements shots the
  README flags as out of date, plus the two missing game-only shots — figures will come
  from these.
- **Related-work scan** with current sources (event-channel SO patterns, DI frameworks,
  agent-rules conventions) — none of it should be written from memory.
- **Decide the co-author/student question** for Thesis C: if a fall team builds on the
  template, their swap projects become the evaluation section.

---

*RunnerUGSTemplate + EventDrivenUGS · both CC0 · prepared from the 2026-09-01 documentation sweep.*
