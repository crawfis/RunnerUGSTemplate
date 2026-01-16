# Endless Runner with Unity Gaming Services Template
_Based on the design goals explained in this template: `crawfis/TempleRun1-NoArt_[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Includes:
- Unity Building Block - Player Authentication(https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-building-block-player-account-341928)
- Unity Building Block - Achievements (https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-building-block-achievements-341918)
- Unity Building Block - Leaderboards (https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-building-block-leaderboards-341926)

_(with additional inspiration from Unity's Gem Hunter UGS sample project)_


This is an initial project template I use in my CSE 5912: Game Design and Development Capstone course at The Ohio State University
## Main Goals of This Repo

**All of the goals of (_`crawfis/TempleRun1-NoArt_) plus:**
- Main menu and other screens (panels)
- Integration of player authentication, leaderboards and achievements using the () event system.
- Even greater separation of concerns:
  - Build Profiles for UGS Only, Game Only and UGS + Game 
  - Many more events
  - Many more additive scenes
- Remote Config support
- Cloud Save support
- Secure Cloud Code support
- Ready to add Economy and Currencies

---
## Build Profiles
There are 3 build profiles currently for Windows.
- Windows - This is the main or production build profile
- Test_UGS_Windows - This profile bypasses the Temple Run game and just posts a score. useful when just working with UGS.
- Test_GameOnly_Window - Should function just like the original TempleRun demo but with windows and some other panels (like Game Over)

## Scene Loading Overview
When running, the **0_BootStrap** scene is loaded first. It additively adds two scenes:
- `UGS_Boot_0_Initialization`: to start several UGS activities
- `Game_Boot_0_Initialization`: to start the initial game loading





---
## Game Development Tasks
First, integrate a **main menu**. A starter package project for this that has localization support is here. Use **UI Toolkit** for everything.
- `crawfis/ConsumerUI_RxGames`: Unity Localization Sample
I will talk about how to use **Google Sheets (or Excel)** to create new localization tables, and the scripts included here to fetch translation tables from Google Sheets if there is interest. This example also shows how to load dynamic UI elements (ignore the fake data for that).
You should also create an initial **Credit screen**. Data drive and update it when you use any 3rd party assets. Keep track of asset licenses as well (or url’s to them). Also add these tasks to this scrum:
- Change the **Company** and **Product** and version number (use `0.1.0` format) in the Player Settings.
- Change the **Root namespace** in the Editor Settings
- Determine each team member’s interests. Pick several of the features below to learn about and implement. A few extra features to just research and plan out for future reference. Focus on reusable components while learning some good code structure and how to keep separation of concerns in your software.
Pick ones that you are interested in programming and/or may be used in your final (undefined) game. Create a scrum session for these features or user stories and then break down into (very) small tasks. Tasks for research and design should also be included, particularly for more complex or unknown tasks. The artifacts you develop show your team what you have accomplished.
