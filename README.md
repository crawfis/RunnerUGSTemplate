# Endless Runner with Unity Gaming Services

This template combines updated gameplay from [`crawfis/EndlessRunnerTemplate`](https://github.com/crawfis/EndlessRunnerTemplate), which itself adapts [`crawfis/TempleRun1-NoArt`](https://github.com/crawfis/TempleRun1-NoArt), and layers on Unity Gaming Services (UGS) using a decoupled event-driven architecture.

The repo is used in CSE 5912: Game Design and Development Capstone at The Ohio State University as a starting point for teams to learn UGS integrations while keeping gameplay and services cleanly separated.

## Unity Services & Event System

Core communication uses [`CrawfisSoftware.EventsPublisher`](https://github.com/crawfis/EventsPublisher) to keep systems decoupled. The following Unity Building Blocks are included and modified to publish and consume events through that framework:

- Player Authentication: <https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-building-block-player-account-341928>
- Leaderboards: <https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-building-block-leaderboards-341926>
- Achievements: <https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-building-block-achievements-341918>

Additional inspiration is drawn from Unity’s Gem Hunter UGS sample project.

## Goals & Features

- Maintain and extend all gameplay goals from the upstream TempleRun1-NoArt and EndlessRunnerTemplate projects.
- Main menu and supporting UI panels built with UI Toolkit.
- Player authentication, leaderboards, and achievements wired through the event system for loose coupling.
- Strong separation of concerns with additive scenes and a clear boot flow.
- Build profiles for UGS-only, game-only, and combined experiences.
- Remote Config, Cloud Save, and Secure Cloud Code integration points, ready to extend with Economy and currencies.

## Build Profiles (Windows)

- `Windows`: production build profile.
- `Test_UGS_Windows`: bypasses gameplay and posts a score for UGS-only work.
- `Test_GameOnly_Window`: mirrors the original Temple Run demo with added panels (for example, Game Over).

## Scene Loading Overview

On launch, the `0_BootStrap` scene loads and additively pulls in:

- `UGS_Boot_0_Initialization`: starts UGS activities.
- `Game_Boot_0_Initialization`: starts initial game loading.

## Game Development Tasks

- Add a **main menu** using **UI Toolkit**. A starter with localization support: [`crawfis/ConsumerUI_RxGames`](https://github.com/crawfis/ConsumerUI_RxGames).
- Create an initial **Credits** screen. Keep it data-driven and track third-party assets and licenses (or URLs).
- Update Player Settings (**Company**, **Product**, version `0.1.0`).
- Update Editor Settings (**Root namespace**).
- Determine each team member’s interests and pick features to implement or research. Build a scrum board of user stories and break them into small tasks (including research/design for unknown areas).

Artifacts from these tasks demonstrate progress and help the team plan future extensions.
