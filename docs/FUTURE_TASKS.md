# Future Task Catalog: From Working Services to a Live Game

The sibling repo's [Student Task Catalog](https://github.com/crawfis/EndlessRunnerTemplate/blob/main/docs/STUDENT_TASKS.md)
holds 127 tasks, sections **A–P**, for making the plain runner yours. Nearly all of them
port here unchanged — same packages, same buses, same architecture (see
[section X](#x-the-sibling-catalog-still-applies--and-some-tasks-get-a-cloud-upgrade)).
This catalog is the part that *cannot* live in the sibling: the tasks that need a cloud
behind the game. Its sections continue the sibling's lettering — task sections **Q through
W**, with **X** as the closing map between the catalogs — so a task id is unambiguous
across both repos: A6 is always the sibling's dodge roll, T3 is always Friends.

If you arrived from the sibling's **section O** ("Live Services with UGS"), its nine
sketches live here now, grown into full entries: O1 → Q1, O2 → T1, O3 → T2, O4 → R1 and
R3, O5 → R4, O6 → U1, O7 → U2, O8 → S2, O9 → W1 — and its N4/N5 (networked play and ghost
racing) are V3 and U4.

Effort tags match the sibling: **S** = a few days, **M** = a week or two, **L** = a
multi-week centerpiece. Every task also carries **where-the-work-lands tags**, because in
this repo that is the first question, not an afterthought:

| Tag | The work happens in | Editable how |
|-----|--------------------|--------------|
| *(game)* | This repo: `Assets/` — game domains, `Assets/UGSGlue/`, boot scenes, prefabs | Directly |
| *(package)* | The `com.crawfissoftware.ugs` package in [EventDrivenUGS](https://github.com/crawfis/EventDrivenUGS) | Fork it; see Q2. Read-only in this repo |
| *(contract)* | `GameServiceEvents` in `com.crawfissoftware.contracts` | Same fork; **deliberately rare** — see below |
| *(module)* | The Cloud Code C# module, `Assets/UGS/CloudCode/TempleRunUGSCloud~/` | Directly (it's in this repo); redeploy + regenerate bindings after |
| *(dashboard)* | [cloud.unity.com](https://cloud.unity.com) — services configuration, per environment | A person clicking. Not code at all |

**The one rule still applies.** Systems communicate through events; a feature starts with
`/list-events` → `/add-event` → implement → `/audit-events`
(see [CLAUDE.md](../CLAUDE.md#required-skills-workflow)). Two rules are stricter here than
in the sibling: nothing under `Assets/GameFlow/` or `Assets/TempleRun/` may name a UGS type
or event — the game and the services meet only at `GameServiceEvents`, translated in
`Assets/UGSGlue/` — and the contract grows *deliberately rarely*, because every member is a
crossing someone maintains forever.

**The setup tax is real.** Services work means project linking, environments, deployments,
and dashboard configuration before any code runs. Budget Q1 before everything, and remember
that Economy and Remote Config are **per-environment**: a currency deployed to
`development` does not exist in `production`.

---

## Where the services stand today

Half the ground is already prepared. Before choosing a task, know which kind you are
picking: **finish something started**, **extend something working**, or **stand up
something new**.

| Service | SDK installed? | State in this template |
|---------|:--:|------------------------|
| Authentication | ✓ | **Working** — anonymous sign-in, session resume, sign-in modal (`PlayerSignIn`) |
| Leaderboards | ✓ | **Working** — score submit on session end, panel UI |
| Achievements (Cloud Save / Cloud Code) | ✓ | **Working** — catalog, two swappable backends, toasts, claim flow |
| Remote Config | ✓ | **Working** — one fetch (`RemoteConfigManager`), `difficulty_settings` table applied live |
| Economy | ✓ | **Working, minimal** — one currency (`COIN`), single-currency manager, two backends |
| Cloud Code | ✓ | **Working** — .NET module, 4 services / 7 endpoints (AdRewards, HandleProfileChange, PlayerData, PlayerEconomy) |
| Cloud Save | ✓ | **Partially used** — one achievements backend only; no player profile sync |
| Analytics | ✓ | Installed, **unused** |
| LevelPlay (rewarded ads) | ✓ | **Half-built** — `RewardAd*` events exist, `AdRewards` endpoint exists, **no ad SDK adapter** |
| Push Notifications | ✓ | Installed, **untouched** |
| In-App Purchasing | ✓ | Installed, **untouched** |
| Cloud Content Delivery | ✓ (management SDK) | Installed, **untouched**; Addressables data folder exists |
| Cloud Build | ✓ | Installed, **untouched** |
| Multiplayer Center | ✓ | Installed — the questionnaire tool; recommends and installs the stack below |
| Friends | ✗ | Not installed |
| Lobby | ✗ | Not installed |
| Matchmaker | ✗ | Not installed |
| Relay / Netcode for GameObjects | ✗ | Not installed |
| Vivox (voice) | ✗ | Not installed |

---

## Working a task with an AI assistant

Each task below ends with a short **hand-off brief** — enough context that you can paste
the task into an AI assistant and get useful work instead of confident guesses. The
ground rules for that hand-off:

**Give it the right context.** Paste (or point it at) [CLAUDE.md](../CLAUDE.md), the task's
entry from this file, and the *Read first* files the entry names. Require the skills
workflow: in Claude Code the skills run as slash commands; in any other tool, the same
procedures are plain markdown checklists in `.claude/skills/*/SKILL.md`.

**Know the split.** An assistant is good at the C# — enum members, bridge mappings, service
adapters, Cloud Code endpoints, tests, docs, and running `/audit-events`. Three things stay
with a person, every time:

1. **The dashboard.** Linking projects, creating environments, configuring services,
   reading analytics — cloud.unity.com is clicked, not coded. (Some configuration does
   deploy from files — `COIN.ecc`, remote config data, the Cloud Code module — and an
   assistant can edit those; *publishing* them through the Deployment window is still a
   person.)
2. **Scenes and prefabs.** Wiring a component into a scene, building a prefab, adding a
   scene to a build profile — that is Unity editor work. Do not let an assistant hand-edit
   `.unity` or `.prefab` files; the merge conflicts alone will cost more than the typing
   saved. The assistant writes the component and tells you exactly where it goes; you place
   it.
3. **Devices and playtests.** Real ad fill, push delivery, and "does it feel right" happen
   on hardware, in front of people.

**Know where the code lands before it writes any.** The tags on each task say which of the
five places the work happens. Anything tagged *(package)* or *(contract)* means editing
your [EventDrivenUGS](https://github.com/crawfis/EventDrivenUGS) fork — set that loop up
once (Q2) before the first such task.

**The standard recipe** for any new service capability (memorize it; most tasks below are
this recipe with different nouns):

1. `/list-events UGS` — see what vocabulary already exists.
2. Add the service's lifecycle events to `UGS_EventsEnum` (*package*).
3. Write an adapter that calls the SDK and publishes its callbacks on `UGSBus` (*package*).
4. If the game must react — and only then — add a crossing to `GameServiceEvents`
   (*contract*) and map it on both sides: `GameServiceEventsUGSBridge` in the package,
   `Assets/UGSGlue/` here. A service feature whose UI lives entirely in the UGS domain
   (like the leaderboard and achievements panels) needs **no contract change** — prefer
   that shape when you can.
5. Create a `UGS_Boot_N_[Feature]` scene under `Assets/UGS/Scenes/Boot/`, wire it into
   `0_BootStrap` with a `LoadSceneAdditively` component, add it to the build profiles
   (*game*, and scene work is yours).
6. `/audit-events` before every merge.

**New service area vs. new enum members.** Adding to a service that exists (a new
leaderboard call, a currency) means new members in `UGS_EventsEnum`. Standing up a genuinely
separate area — networked multiplayer above all — deserves its **own domain** in your game
fork instead: run `/add-event-domain` and let its decision gate decide. An in-fork domain
bridges to GameFlow or TempleRun directly with its own bridge class, the way
`UserInitiatedEvents` does, and the packages stay untouched.

---

## Q. Foundations — do these before the rest

1. **Stand it up (S/M · dashboard + game).** Clone the template, link your own Unity
   project, create environments, deploy everything, and prove the full loop: sign in → run
   → score on the leaderboard → achievement toast → coin balance on the HUD. Sounds
   trivial; teaches the entire cloud workflow — project linking, environments, the
   Deployment window, Cloud Code binding generation — and every other task in this file
   assumes it. The one-coin walkthrough at the end of
   [CLAUDE.md](../CLAUDE.md#event-flow-architecture) is your verification script: if the
   banked balance comes back after a run, the whole chain is alive.
   *Read first:* [ConfigureUnityGamingServicesand-RunnerUGSTemplate.md](ConfigureUnityGamingServicesand-RunnerUGSTemplate.md)
   (click-by-click), README "Getting Started" steps 3 and 6–8.
   *Human steps:* nearly all of it — this is the one task a person does almost alone; the
   assistant is for decoding error messages.
   *Done when:* a run's coins land in the HUD balance after the next sign-in, in **your**
   Unity project, not the template author's.

2. **The package-change loop (S · package).** The UGS domain is three UPM packages
   resolved by git URL from EventDrivenUGS, so they are read-only in this repo — and half
   the tasks below need to edit them. Set up the loop once: fork EventDrivenUGS, clone the
   fork beside your game clone, and switch the three `com.crawfissoftware.*` entries in
   `Packages/manifest.json` from git URLs to local `file:` paths while developing. When a
   change is done: bump the package's `package.json` version, push the fork, switch the
   manifest back to your fork's git URL. Prove the loop with something trivial — a log line
   in `PlayerAuthenticationManager` — before betting a real task on it.
   *Read first:* `Packages/manifest.json` here; the EventDrivenUGS repo's README.
   *Done when:* your log line appears in Play Mode via the `file:` path **and** via your
   fork's git URL.

3. **Fake services, real game (M · game).** The template proves replaceability in two
   directions: `Test_GameOnly_Windows` runs the game with the services domain *absent*, and
   `Test_UGS_Windows` runs the real services against a dummy game. Build the third rig:
   the real game against **fake services**. One new scene (no UGS package scenes loaded)
   holding a stub that speaks only `GameServiceEvents`: publishes
   `ServicesStatusChanged(Ready)` on start — which is all `UGSGameFlowBridge` needs to fire
   `GameplayReady` — answers `SessionEnding` with a log, and publishes a made-up
   `CurrencyBalanceChanged` so the HUD shows a balance. Unlike the game-only bootstrap,
   everything *looks* signed in. This is demo-day insurance for flaky wifi, the test double
   for play-mode tests, and the sharpest proof the contract means what it claims.
   *Read first:* `Assets/UGSGlue/UGSGameFlowBridge.cs`, `GameServiceEvents.cs` (contracts
   package), `Assets/GameFlow/Scenes/Boot/0_BootStrap_Game_Only.unity`'s
   `Test_AutoFireEventOnStart` trick (described in CLAUDE.md's Testing section).
   *Human steps:* the new scene and boot variant, plus a build profile.
   *Done when:* full game plays, HUD balance shows a number, no network, no UGS package
   scene in memory.

4. **One contract event, end to end (M · package + contract + game).** The contract grows
   deliberately rarely — so learn the full ceremony on a small, genuinely useful member
   before a big task forces it. Suggested: the game never learns its leaderboard rank.
   Add `GameServiceEvents.ScoreRankChanged` (payload: `int` — contract payloads are
   primitives on purpose), publish the rank from the leaderboards code after a submit
   (*package*), map it in `GameServiceEventsUGSBridge` (*package*), map it to a new
   `GameFlowEvents` member in `Assets/UGSGlue/UGSGameFlowBridge.cs` (*game*), and show
   "Global rank #123" on the game-over panel. You will touch every layer of the
   architecture exactly once, which is the point.
   *Read first:* the doc comment atop `GameServiceEvents.cs` — it explains what belongs in
   the contract and what never will; `Runtime/Leaderboard/` in the ugs package;
   `Assets/UGSGlue/UGSGameFlowBridge.cs`.
   *Done when:* the rank renders, and the diff shows one new contract member — not five.

## R. Remote Config, Game Overrides & Live Tuning

1. **More knobs behind Remote Config (M/L · package + contract + game + dashboard).**
   Today one table travels: `difficulty_settings`. Move more tuning online — coin value,
   power-up durations (`PowerUpDefinition`), jump and dash timing — so the live game
   retunes without a rebuild. The design problem is the interesting part: the existing
   `DifficultySettingsAvailable` contract event carries a game-side config type, which the
   contract's own doc comment calls out as the wart it is. Fix the pattern while you
   extend it: carry raw JSON (a string is a primitive) across the contract and parse it
   game-side, or argue in writing why typed-per-table is worth the coupling.
   `RemoteConfigManager` stays the **only** fetch.
   *Read first:* `Runtime/RemoteConfig/RemoteConfigManager.cs` (ugs package), the
   `DifficultySettingsAvailable` doc comment in `GameServiceEvents.cs`, the per-mechanic
   configs in `Assets/TempleRun/Scripts/Config/`.
   *Human steps:* author the new config values in the dashboard (or deployable remote
   config data under `Assets/UGS/Editor/`), per environment.
   *Done when:* changing a power-up duration in the dashboard changes the game on next
   launch, no rebuild.

2. **Game Overrides: audiences and staged rollout (M · dashboard + package).** Remote
   Config's dashboard layer — **Game Overrides** — targets *who* gets *which* values:
   platform, app version, custom attributes, a random 10% cohort, a scheduled window. The
   client half mostly exists: `UserAttributes` (ugs package, `Runtime/RemoteConfig/App/`)
   is what audiences filter on — extend it (total runs played, platform, version) so the
   dashboard has something to target. Then run a staged rollout for real: a difficulty
   tweak served to 20% of installs, verified by logging which value arrived.
   *Read first:* `Runtime/RemoteConfig/App/UserAttributes.cs`; Unity's Game Overrides
   docs.
   *Human steps:* the override itself — audience, rollout %, schedule — is pure dashboard.
   *Done when:* two machines (or two environments) demonstrably receive different values
   from the same build.

3. **A real A/B test (M · dashboard + game).** Two variants of one tunable — two scoring
   models (sibling E7) or two `SafePreTurnDistance` values — served to two cohorts via a
   Game Overrides A/B test, compared on evidence: leaderboard distributions, or the
   analytics funnel if W1 is running. The deliverable is the write-up: hypothesis, cohort
   sizes, the numbers, and the tuning decision you'd ship. Small honest data beats big
   pretend data — say what a class-sized sample can and cannot conclude.
   *Read first:* R2's setup; `Assets/GameFlow/Scripts/Config/` for where the tested value
   lives.
   *Done when:* the write-up exists and names a winner (or honestly calls a tie).

4. **A seasonal event (L · dashboard + game + package).** The full live-ops loop in
   miniature, switched on by one Remote Config flag: while active, a themed track level
   appears in the selector (`LevelRegistry` + a new `LevelConfig` asset), runs earn an
   event currency (S1's machinery), scores go to an event leaderboard (T1's), and a
   countdown shows time remaining. Ship it, run it for two weeks, **retire it** — the
   retirement is the part real studios get wrong, so design the off-switch first: what
   happens to unspent event currency, and what does the player see on day fifteen?
   *Read first:* R1, S1, T1; `Assets/GameFlow/Scripts/Config/LevelRegistry.cs` and the
   level assets in `Assets/TempleRun/Scriptables/Levels/`.
   *Human steps:* the flag and schedule in the dashboard; the themed scene and selector
   assets in the editor.
   *Done when:* flipping one dashboard flag stands the whole event up and takes it all
   down.

## S. Economy & Money

1. **A second currency: gems (M/L · package + contract + dashboard).** The deep
   architecture task hiding inside "add gems." `PlayerCurrencyManager` is deliberately
   single-currency — one `CurrencyId`, defaulting to `COIN` — and the contract carries
   bare numbers: `CurrencyTotalChanged` is an `int`, `CurrencyBalanceChanged` a `long`,
   neither says *which* currency. Adding a rare premium currency forces the real decision:
   a second manager instance and a parallel pair of contract events per currency (the
   contract grows linearly with currencies — smells), or one pair of events carrying a
   small `(currencyId, amount)` struct declared **in the contracts package** the way
   `ServicesStatus` is (the contract changes shape once — argue it's worth it). Write the
   argument down; it's worth more than the code. Then: `GEM.ecc` beside `COIN.ecc` (the id
   comes from the filename), deploy per environment, gems earned from achievements, spent
   in the shop (S2).
   *Read first:* `Runtime/Economy/PlayerCurrencyManager.cs` and `Service/` (ugs package),
   `Assets/UGS/Economy/COIN.ecc`, the `CurrencyBalanceChanged` doc comment in
   `GameServiceEvents.cs`.
   *Human steps:* deploy the new currency from the Deployment window, in **every**
   environment you sign in to.
   *Done when:* both balances survive a sign-out/sign-in, and the write-up says why the
   contract looks the way it now does.

2. **A skins shop on Economy (M/L · package + game + dashboard).** The sibling's shop
   task (E5) designs the economy; this one gives it a real backend. UGS Economy's other
   two halves — **inventory items** (owned skins) and **virtual purchases** (COIN → skin,
   priced server-side) — deploy from definition files beside `COIN.ecc`. Client side: new
   purchase lifecycle events in `UGS_EventsEnum`, an adapter around the Economy purchase
   and inventory calls, and a shop panel. Keep the panel inside the UGS domain like the
   leaderboard panel and the contract never changes; if the game must react to an owned
   skin (equipping it), *that* crossing is the one carefully-argued contract addition.
   *Read first:* `Runtime/Economy/Service/EconomyCurrencyBackend.cs` for the SDK idiom;
   sibling task E5 for the design homework; `PlayerEconomyService.cs` in the Cloud Code
   module.
   *Human steps:* item and purchase definitions deployed per environment; shop scene
   wiring.
   *Done when:* a purchase fails with insufficient funds the way it should, succeeds when
   it should, and the owned skin is still owned tomorrow.

3. **Finish rewarded ads (M/L · game + package + module + dashboard).** The most
   satisfying kind of task: both ends exist and the middle is missing. `UGS_EventsEnum`
   already has `RewardAdWatching / RewardAdWatched / RewardAdFailedToShow /
   RewardAdClosedWithoutReward`; the Cloud Code module already has an `AdRewardsService`
   endpoint to grant the reward server-side (so a client can't just *say* it watched); the
   LevelPlay SDK is already in the manifest. Missing: the adapter that shows an ad and
   publishes those events from LevelPlay's callbacks, and a placement worth watching an ad
   for. The placement to build: **revive** — `PlayerReviveRequested / PlayerRevived`
   already sit unused in `TempleRunEvents`, so the offer panel ("watch an ad, keep
   running") completes a chain two features were waiting on. Decide deliberately what
   crosses the contract: the *ad* is a service detail; the *reward* is what the game
   hears.
   *Read first:* `AdRewardsService.cs` in the module; the Rewarded Ads block of
   `UGS_EventsEnum.cs`; `PlayerLifeController.cs` and the revive members of
   `TempleRunEvents.cs`; LevelPlay's Unity integration docs (test ads first).
   *Human steps:* LevelPlay dashboard setup and ad-unit keys; the offer panel scene work;
   real fill needs a device build.
   *Done when:* in the editor with test ads — die, watch, revive, and the server-granted
   reward arrives via the `AdRewards` endpoint, not a client-side grant.

4. **In-app purchasing, honestly stubbed (M · game + dashboard).** Unity's IAP package is
   installed and untouched. Sell gem packs (needs S1): a catalog, the purchase flow
   against the fake store (IAP's editor test store — no real money, no store account),
   and a written note on what *shipping* it would take: store listings, receipt
   validation (a Cloud Code endpoint is the natural home — stretch), refunds, and the
   ethics of selling currency to players in a class project. The plumbing is the task;
   the note keeps it honest.
   *Read first:* Unity IAP initialization docs (v5 changed the API surface — check the
   installed version's samples, not old tutorials); S1's gem wallet.
   *Done when:* a fake-store purchase credits gems through the same Economy path S1 built
   — not a parallel one.

5. **Daily reward with a streak (M · package + module).** A claim-once-per-day reward
   whose streak logic lives server-side — the client's clock is a liar, so a Cloud Code
   endpoint decides eligibility and streak length (`PlayerDataService` already
   reads/writes per-player data) and grants through Economy. Client: a claim panel inside
   the UGS domain (no contract change), new claim lifecycle events in `UGS_EventsEnum`,
   and the day-boundary edge cases named in a test list (23:59 claim, timezone hop, a
   missed day, a clock set backward).
   *Read first:* `PlayerDataService.cs` and `PlayerEconomyService.cs` in the module;
   the achievements claim flow (`Runtime/Achievements/`) as the pattern to copy.
   *Done when:* claiming twice in a day fails server-side, and the streak survives a
   reinstall (it lives in the cloud, not PlayerPrefs).

## T. Leaderboards, Achievements, Friends & Identity

1. **Leaderboards that mean something (M · package + dashboard).** One all-time distance
   board exists. Add boards worth climbing: a **weekly** board (leaderboards support
   reset schedules and versions — read last week's results after a reset), **per-level**
   boards keyed by level number, and a bucketed view for newcomers. Then the design half:
   kill any board nobody can climb. A fresh player who sees rank #4,812 closes the panel;
   a player who sees "3rd this week among Level 2 runners" plays again.
   *Read first:* `Runtime/Leaderboard/LeaderboardQuery.cs` and `LeaderboardPanel.cs`
   (ugs package); Unity's leaderboards configuration docs (tiering, reset cron).
   *Human steps:* each board's configuration is dashboard/deployment work, per
   environment.
   *Done when:* the weekly board resets on schedule and the panel can show both the live
   week and the previous one.

2. **Achievements that teach the game (M/L · package + game).** Replace the placeholder
   distance/coin achievements with a set that is a curriculum for playing well — tiers
   for near-misses, combos, power-up mastery (sibling A9, A10, D-section mechanics).
   The architectural catch is where the numbers come from: `DistanceBasedAchievements`
   listens to stats that already cross the contract (`ScoreUpdated`), but a near-miss
   count does not cross today, and the contract should not grow one member per statistic.
   Design the crossing once — a generic stat event, or bank stats through the
   `PlayerData` endpoint and let the service read them — and write down why.
   *Read first:* `Runtime/Achievements/` model + `DistanceBasedAchievements.cs` (ugs
   package); `Editor/Achievements/AchievementDefinitionCatalog.cs` and its exporter — the
   catalog is data, so new achievements are entries, not classes.
   *Done when:* a new player can read the achievement list top to bottom and know what
   skilled play looks like.

3. **Friends (M/L · package + dashboard).** Install `com.unity.services.friends` and
   stand up relationships: send/accept/decline a friend request, block, and presence
   (who's online now). Model it on the existing services: lifecycle events in
   `UGS_EventsEnum` (`FriendRequestSent / FriendAdded / FriendPresenceChanged / …`), an
   adapter publishing SDK callbacks on `UGSBus`, a `UGS_Boot` scene, and a friends panel
   living wholly inside the UGS domain — no contract change. Requests need someone to
   address, so player names (T5) come first or nobody can find anybody.
   *Read first:* `Runtime/Leaderboard/` as the panel-and-adapter pattern to copy; the
   Friends SDK samples.
   *Human steps:* two test accounts (two editors, or editor + build) to actually
   befriend; panel scene wiring.
   *Done when:* two signed-in players see each other online, and the relationship
   survives both restarting.

4. **The friends leaderboard (S/M · package).** The payoff for T3: a "Friends" tab on the
   existing leaderboard panel showing only your friends' scores — the Leaderboards SDK
   queries scores for a list of player ids, and T3 has the list. Rank #2 among people you
   know beats rank #4,812 among strangers every single time; this small view is worth
   more retention than most M-sized features.
   *Read first:* `LeaderboardQuery.cs`, T3's relationship list.
   *Done when:* the tab shows exactly your friends, including the ones with no score yet
   (show them at the bottom — an empty row is an invitation).

5. **Display names & profiles (M · package + module + dashboard).** Leaderboards full of
   anonymous GUIDs are a wall of strangers. Authentication has a player-name API; the
   Cloud Code module already ships a `HandleProfileChangeService` endpoint — read it
   first and finish what it starts. Add name entry to the sign-in modal
   (`PlayerSignIn` — note the UXML names it by fully qualified type name), show names in
   the leaderboard rows, and handle the unhappy paths: name taken, name profane (Unity
   appends a discriminator — decide whether to show it), name changed mid-session.
   *Read first:* `HandleProfileChangeService.cs` in the module;
   `Runtime/Authentication/PlayerSignIn.cs` and `PlayerSignInController.cs` (ugs
   package).
   *Done when:* your run shows up on someone else's leaderboard under the name you chose.

6. **A third achievements backend (S/M · package).** `IAchievementBackend` already has two
   implementations — Cloud Save and Cloud Code — chosen by configuration. Write the
   third: a local-file backend for offline play, which also completes the Q3 fake-services
   rig (fake services with *real, persistent* achievements). Then write the half-page
   that matters: what the interface got right that made this easy, and what leaked
   through it that made anything hard. The same exercise exists for `ICurrencyBackend` if
   a teammate wants the twin task.
   *Read first:* `Runtime/Achievements/Service/` — the interface and both backends (ugs
   package).
   *Done when:* the swap is a configuration change, zero call-site edits — or the
   write-up says exactly why not.

## U. Cloud Save, Cloud Code & Player Data

1. **Cloud Save profiles (M/L · package + contract + game).** Progress follows the
   player, not the machine: level unlocks, stars, best distances, and settings sync
   through Cloud Save at sign-in and session end. The hooks are waiting —
   `GameFlowEvents` has a whole unused Save/Load block (`SaveRequested / Saved /
   SaveLoadRequested / SaveLoaded`, values 100–113) — and the crossing design is the
   lesson: the profile is game data the service must not understand, so it crosses the
   contract as a JSON string (primitives only), and the service stores what it's given.
   Then the classic conflict, made concrete by this data: device A unlocked level 3
   offline, device B banked a better score — resolve by **merging** (unlocks union, best
   scores max), not last-write-wins, and justify it.
   *Read first:* the Save/Load block in `Assets/GameFlow/Scripts/Events/GameFlowEvents.cs`;
   `Assets/GameFlow/Scripts/Config/LevelProgressManager.cs` (what's persisted locally
   today); the Cloud Save achievements backend in the ugs package (the SDK idiom).
   *Done when:* progress made on one machine appears on a second machine at sign-in, and
   the merge test (divergent progress on both) loses nothing.

2. **Server-authoritative scores (L · module + package).** Don't trust the client. A new
   module endpoint accepts the run's whole stat block — distance, duration, coins, jump
   and slide counts — sanity-checks it (distance possible in that duration at max speed?
   coins ≤ what that distance could spawn? counts plausible?), and only then writes the
   leaderboard **server-side**; the client's direct leaderboard write is removed. Then
   prove it: make a cheat build that submits a lie, and show the rejection in the server
   logs. A genuine introduction to anti-cheat thinking, including its honest limit — you
   are raising the price of cheating, not abolishing it.
   *Read first:* the existing module services (all four in
   `Assets/UGS/CloudCode/TempleRunUGSCloud~/Project/Services/`); `EventHistory.cs`
   (common package) — the run's event stream is the stat block's natural source;
   the leaderboard submit path in `Runtime/Leaderboard/`.
   *Human steps:* module publish + `Services > Cloud Code > Generate All Modules
   Bindings` after every endpoint change (and leave `FolderProfile.pubxml` alone — the
   tooling needs it; discard its CRLF-only rewrite rather than committing it).
   *Done when:* the honest build's score lands, the cheat build's is refused, and both
   events are visible in the Cloud Code logs.

3. **Cloud Code triggers (S/M · module + dashboard).** Endpoints answer when called;
   **triggers** run when something *happens* — an Economy purchase, a Cloud Save write.
   Wire one visible loop: when a purchase completes (S2), a trigger grants a bonus or
   stamps a "first purchase" flag into player data. Small on purpose: the point is
   knowing this tool exists before designing U2- or S5-scale systems as client-driven
   polling.
   *Read first:* Unity's Cloud Code triggers docs; `PlayerEconomyService.cs`.
   *Done when:* the trigger fires from the real event, seen in the logs — with no client
   call anywhere in the chain.

4. **Ghost racing through the cloud (M/L · game + package).** The sibling's ghost-replay
   task (L4) records a run as its event stream plus the seed; this task makes it social
   without netcode: serialize the recording, store it in Cloud Save with **public**
   access keyed to the player, and let anyone pick a rival off the leaderboard panel,
   download their ghost, and race it live as a translucent runner. For an endless runner
   this is often *more* fun than real multiplayer — the rival is at their best, and it
   ships without a single networking headache. The sibling catalog files this as N5;
   this is its landing pad.
   *Read first:* sibling task L4 (the prerequisite recorder); `EventHistory.cs`; Cloud
   Save public-access docs; `LeaderboardPanel.cs` for where the "race this ghost" button
   lives.
   *Done when:* two accounts on two machines can race each other's yesterday.

5. **Community tracks (L · game + package).** The track system is data all the way down —
   segments and levels are ScriptableObjects, and `TrackDataImporter` already converts
   JSON to them. So a track a student authors is a JSON file, and a JSON file can travel:
   upload an authored level to Cloud Save public data, browse and download other
   players', and play them. Runtime import needs the importer's logic out of the Editor
   assembly — that refactor is half the task. And user content means a moderation stance
   even at class scale: the safest v1 ships **no free-text fields at all** (a track is
   geometry and a generated id; names come from T5's already-filtered display names).
   *Read first:* `Assets/TempleRun/Editor/TrackDataImporter.cs`;
   `Assets/TempleRun/Scripts/Track/TrackLibraryLoader.cs` and the level SOs in
   `Assets/TempleRun/Scriptables/Track/`.
   *Done when:* a level authored on one machine is played on another with no rebuild and
   no free-text anywhere in the payload.

## V. Multiplayer: Lobby, Matchmaker, Relay & Voice

None of these SDKs are installed yet — the **Multiplayer Center** package is, and its
questionnaire recommends and installs the stack (expect: Netcode for GameObjects + Relay +
Lobby for this game's shape). Two structural warnings before any of it, both from hard
experience. First, the sibling catalog's warning still holds: `Blackboard` and most
gameplay state are singletons that assume one player — **making state per-player is the
real work**, and it is tractable precisely because the state travels as events. Second,
netcode is a genuinely separate area with its own lifecycle: run `/add-event-domain` and
give it its **own domain in your game fork** (say, `NetworkEvents`), bridging to GameFlow
and TempleRun through its own bridge classes. The UGS packages stay untouched; the
seeded, injectable RNG is what makes runner netcode honest — every client generates the
identical track from the seed, and only inputs and positions cross the wire.

1. **The multiplayer spike (S · a document).** Before any package installs: run the
   Multiplayer Center questionnaire, read what it recommends and *why*, and write the
   teardown — the event map for a two-player race (which events become networked, which
   stay local), what is replicated (inputs? positions? both?), who owns the clock, and
   the scope fence (two players, one race mode, no reconnection). The deliverable is the
   document; the team that skips it builds a lobby for a game that can't race yet.
   *Read first:* sibling section N's intro; `Blackboard.cs` with per-player eyes.
   *Done when:* a teammate who wasn't in the room can read it and start V2 or V3.

2. **Lobby, standing alone (M/L · game fork + dashboard).** Lobby needs no netcode to be
   worth shipping: create/browse/join by code, ready flags, and — the trick that makes it
   playable *today* — when everyone is ready, the lobby distributes one **seed**, every
   client runs the identical track locally, and final scores post back for a results
   screen. A real shared race with zero position replication. Player names (T5) first, or
   the lobby list is GUIDs. Lobby state arrives by polling or its events; wrap either in
   your `NetworkEvents` domain so the rest of the game never knows.
   *Read first:* V1's spike; the Lobby SDK samples; T5.
   *Done when:* three players in one lobby run the same track at the same time and see
   one honest results screen.

3. **A real two-player race: Relay + Netcode (L · game fork + dashboard).** The
   centerpiece — the sibling catalog files it as N4, and this is its landing pad. Relay carries the traffic (no port forwarding, no server to rent), Netcode
   for GameObjects replicates each player's input/position/state, and the remote runner
   renders in your world on the identical seeded track. Race rules: furthest distance
   when time expires, or last alive. Keep the fence from V1: two players, then — only if
   it's boring — more. Decide early what death means for the loser (spectate the winner
   is cheap and kind). Every replicated thing is a `NetworkEvents` matter; if a TempleRun
   controller learns the word "network," the domain boundary has failed.
   *Read first:* V1's spike (mandatory); NGO's "distributed authority vs client-server"
   docs — pick one on purpose; V2's lobby as the front door.
   *Human steps:* Relay/NGO dashboard setup; player prefab work; two-machine testing
   forever.
   *Done when:* two machines race the same track and both agree who won.

4. **Matchmaker (M/L · game fork + dashboard).** Replace "share a join code" with a
   queue: submit a ticket, get matched, land in a V3 race via Relay. The honest design
   problem is the small player pool — a matchmaker tuned for millions, fed by a
   classroom, matches nobody. So the queue design *is* the task: wide skill tolerance,
   short timeout falling back to "invite a friend," backfill on. Write down what your
   matchmaking rules promise and measure the real wait times during a playtest.
   *Read first:* V3 (the thing being matched into); Matchmaker + Relay integration docs.
   *Done when:* two players who never exchanged a code end up racing — and a lone player
   gets a graceful out, not an infinite spinner.

5. **Voice in the lobby: Vivox (M · game fork + dashboard).** Voice while waiting and
   racing: join the lobby's voice channel on entry, positional or plain, push-to-talk
   default (open mics are a griefing surface), per-player mute. Then the paragraph that
   matters more than the code: voice between strangers is a moderation commitment — mute
   persistence and "who can hear me" UI are features, not polish.
   *Read first:* V2's lobby lifecycle (the channel binds to it); Vivox Unity docs.
   *Done when:* two ready players can talk, one can mute the other, and the mute
   survives into the race.

## W. Analytics, Push, Content Delivery & Build Plumbing

1. **Analytics-driven tuning (M · game + dashboard).** The Analytics SDK is installed and
   dormant. Instrument the funnel with custom events — death position and segment id,
   power-up pickup and use, session length, revive taken — then make one tuning change
   *argued from the data* (the dashboard chart is the deliverable's spine). Two
   design notes: analytics requires a **consent** flow before collection starts, even in
   a class project — build the real opt-in; and an analytics forwarder is naturally a
   pure *listener* (like `DebugEventFileLogger`) — subscribe to existing domain events
   and forward; run the `/add-event-domain` gate before inventing new events for it, and
   expect the gate to say no.
   *Read first:* `Runtime/Utility/DebugEventFileLogger.cs` (common package) — the
   listener shape to copy; the Analytics SDK's consent/`StartDataCollection` docs.
   *Human steps:* dashboards, and the patience for event data's ingestion lag (hours,
   not seconds).
   *Done when:* the write-up shows a chart, names the change it justifies, and the
   change shipped.

2. **Push notifications (M/L · game + dashboard — needs the mobile port).** The SDK is
   installed; `Assets/Push Notifications/` holds nothing but an Editor stub. This one is
   gated on the sibling's mobile-port task (K1): delivery is a device feature, and
   Android needs Firebase configuration before the first token. Client half: register,
   surface the token, handle a notification arriving with the game open. Campaign half
   (dashboard): one re-engagement message worth sending — "the weekly board resets
   tonight" (T1) beats "we miss you" every time. And the restraint *is* the design:
   document when you will not send.
   *Read first:* the Push Notifications SDK setup docs (platform config is most of it);
   T1 or R4 for something true to say.
   *Human steps:* Firebase/APNs configuration, dashboard campaign authoring, and a
   physical phone.
   *Done when:* a scheduled campaign lands on a device with the app closed, and tapping
   it opens the game.

3. **A theme shipped without a rebuild: CCD + Addressables (M/L · game + dashboard).**
   The CCD management SDK is installed and `Assets/AddressableAssetsData/` already
   exists. Package a visual theme (sibling B7's biome work) as an Addressables group,
   host the bundles in a CCD bucket, and select the live theme with a Remote Config
   value — new look, zero store update. Learn the release discipline that comes with it:
   badges promote a build from a development bucket to production, and *that* promotion
   — not an upload — is the release act.
   *Read first:* `Assets/AddressableAssetsData/` settings as they stand; Unity's
   Addressables + CCD integration docs; R1 for the selection flag.
   *Human steps:* buckets, badges, and the promotion click.
   *Done when:* changing the dashboard flag re-themes an already-installed build on next
   launch.

4. **A build that builds itself (M/L · dashboard or game).** Two roads, pick one: Unity
   **Cloud Build** (the package is installed — link the repo, configure a target,
   build on push) or GitHub Actions (the sibling's L8 task ports here verbatim, including
   its warning: Unity licensing in CI is the day-eater, not the YAML). Either way the
   bar is the same and it is the bar every team wants at week ten and few have: every
   merge to main produces a clickable Windows build, and a red build blocks the merge.
   *Read first:* sibling task L8; `Settings/` build profiles (the CI must build
   `Test_GameOnly_Windows` too — it's the profile that catches game/services coupling).
   *Done when:* a pull request shows a green build check, and the team can download
   yesterday's main and run it.

## X. The sibling catalog still applies — and some tasks get a cloud upgrade

Sections **A through M** of the sibling catalog — mechanics, track generation,
characters, art, audio, UI, the explorer pivot — port to this repo directly: same
event buses, same interfaces, same skills workflow. One added rule when working them
here: nothing under `Assets/GameFlow/` or `Assets/TempleRun/` may name a UGS type or
event. If a gameplay feature needs the cloud, it crosses at `GameServiceEvents` through
`Assets/UGSGlue/` — and `/audit-events` checks the glue too.

Some sibling tasks are *better* here, because a service completes them. If your team is
choosing from both catalogs, these pairs are the high-value combinations — a gameplay
task and its cloud half make one honest vertical slice:

| Sibling task | What the cloud adds here |
|---|---|
| A12 Revive / second chance | S3 — pay for the revive with a rewarded ad, granted server-side |
| E1 Mission system | T2 achievements as the visible tier ladder; W1 measures completion rates |
| E4 Daily challenge & streaks | R1 serves the daily seed to everyone; S5 makes the streak server-truthful |
| E5 Shop & monetization design | S1 gems, S2 Economy inventory & purchases, S4 IAP — the stub becomes real |
| E11 Arcade initials & local scores | T5 real display names, T1 real boards — the arcade table goes online |
| G6 Seeded run variation | V2 shared-seed lobby races; R3 seeded A/B cohorts |
| L3 Save system | U1 — the pluggable backend it asks for is Cloud Save |
| L4 Ghost replay | U4 — the recording becomes a downloadable rival |
| L6 In-game event console | W1 — the same instrumentation, pointed at Analytics |
| L8 CI/CD | W4 — or trade GitHub Actions for Cloud Build |
| K1 Mobile port | W2 push notifications and S3 real ad fill both wait on it |
| N1/N2 Couch multiplayer | V3 networked race, V4 matchmade — the per-player-state refactor pays twice |
| B7 Biome / theme system | W3 ships themes over CCD; R4 schedules them as seasons |

## Choosing well

- **Q1 first, no exceptions.** Every task above assumes a linked project, an environment,
  and a deployed configuration. And keep the environment discipline forever after:
  currencies, config, and boards exist *per environment* — "it works in development" is
  a sentence about development.
- **Set up Q2 before the first *(package)* task.** Discovering that the file you need to
  edit is read-only is a bad Tuesday. And package edits are shared machinery — before
  adding a member, ask whether a different game using these packages would still want
  it; if not, it belongs on the game side of the fence.
- **Count the contract.** After any task, the number of new `GameServiceEvents` members
  should usually be zero, occasionally one. If your design added five, re-read the doc
  comment at the top of that file and come back.
- **Budget the human hours separately from the AI hours.** Dashboard configuration,
  scene wiring, device testing, and playtesting do not get faster because an assistant
  wrote the C# quickly — schedule them like the real work they are.
- **Pick a vertical slice across both catalogs.** The pairs table in section X is the
  menu: one gameplay task plus its cloud half beats either alone, and forces the
  integration this architecture exists to teach.
- **Run `/audit-events` before every merge.** Here it guards the glue too.
