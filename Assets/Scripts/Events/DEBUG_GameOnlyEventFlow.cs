using CrawfisSoftware.UGS;
using CrawfisSoftware.UGS.Events;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.Events
{
    internal class DEBUG_GameOnlyEventFlow : UGSAndGameEventFlow
    {
        Dictionary<GameFlowEvents, GameFlowEvents> _newAutoGameFlow2GameFlowEvents = new Dictionary<GameFlowEvents, GameFlowEvents>()
        {
            // ========================================================================================
            // GAME-ONLY FULL TIMELINE (documented; uncomment to simulate missing controllers)
            // ========================================================================================

            // --- Loading Screen ---
            //{ GameFlowEvents.LoadingScreenShowRequested, GameFlowEvents.LoadingScreenShowing },
            //{ GameFlowEvents.LoadingScreenShowing, GameFlowEvents.LoadingScreenShown },      // simulate UI
            //{ GameFlowEvents.LoadingScreenShown, GameFlowEvents.LoadingScreenHideRequested },
            { GameFlowEvents.LoadingScreenHideRequested, GameFlowEvents.LoadingScreenHiding },
            //{ GameFlowEvents.LoadingScreenHiding, GameFlowEvents.LoadingScreenHidden },      // simulate UI

            // --- Ready -> Menu ---
            { GameFlowEvents.LoadingScreenHidden, GameFlowEvents.GameplayReady },
            { GameFlowEvents.GameplayReady, GameFlowEvents.MainMenuShowRequested },
            { GameFlowEvents.MainMenuShowRequested, GameFlowEvents.MainMenuShowing },
            //{ GameFlowEvents.MainMenuShowing, GameFlowEvents.MainMenuShown },                // fired by UI controller

            // --- Press Play (typical UI triggers) ---
            // UI should publish GameStartRequested when Play pressed.
            //{ GameFlowEvents.GameStartRequested, GameFlowEvents.MainMenuHideRequested },
            //{ GameFlowEvents.MainMenuHideRequested, GameFlowEvents.MainMenuHiding },
            //{ GameFlowEvents.MainMenuHiding, GameFlowEvents.MainMenuHidden },                // fired by UI controller
            //{ GameFlowEvents.MainMenuHidden, GameFlowEvents.GameScenesLoadRequested },
            //{ GameFlowEvents.GameScenesLoadRequested, GameFlowEvents.GameScenesLoading },
            //{ GameFlowEvents.GameScenesLoading, GameFlowEvents.GameScenesLoaded },           // fired by Scene loader

            // If you’re bypassing UI that normally does the above, you can use these:
            //{ GameFlowEvents.MainMenuShown, GameFlowEvents.GameScenesLoadRequested },
            //{ GameFlowEvents.GameScenesLoaded, GameFlowEvents.GameStartRequested },

            // --- Start -> Countdown ---
            { GameFlowEvents.GameScenesLoaded, GameFlowEvents.GameStartRequested },
            { GameFlowEvents.GameStartRequested, GameFlowEvents.GameStarting },

            { GameFlowEvents.GameStarting, GameFlowEvents.CountdownStartRequested },
            { GameFlowEvents.CountdownStartRequested, GameFlowEvents.CountdownStarting },
            //{ GameFlowEvents.CountdownStarting, GameFlowEvents.CountdownStarted },           // fired by Countdown controller
            //{ GameFlowEvents.CountdownStarted, GameFlowEvents.CountdownTick },               // fired repeatedly
            //{ GameFlowEvents.CountdownEnding, GameFlowEvents.CountdownEnded },               // fired by Countdown controller
            { GameFlowEvents.CountdownEnding, GameFlowEvents.CountdownEnded },

            // --- End -> Unload -> Loop ---
            //{ GameFlowEvents.GameEndRequested, GameFlowEvents.GameEnding },
            { GameFlowEvents.GameEnding, GameFlowEvents.GameScenesUnloadRequested },
            { GameFlowEvents.GameScenesUnloadRequested, GameFlowEvents.GameScenesUnloading },
            //{ GameFlowEvents.GameScenesUnloading, GameFlowEvents.GameScenesUnloaded },       // fired by Scene loader
            //{ GameFlowEvents.GameScenesUnloaded, GameFlowEvents.GameEnded },
            //{ GameFlowEvents.GameEnded, GameFlowEvents.GameplayReady },                      // loop back

            // ========================================================================================
            // Pause/Resume chains (publish PauseRequested / ResumeRequested from input)
            // ========================================================================================
            { GameFlowEvents.PauseRequested, GameFlowEvents.Pausing },
            { GameFlowEvents.Pausing, GameFlowEvents.Paused },

            { GameFlowEvents.ResumeRequested, GameFlowEvents.Resuming },
            { GameFlowEvents.Resuming, GameFlowEvents.Resumed },

            // ========================================================================================
            // Quit chains
            // ========================================================================================
            { GameFlowEvents.QuitRequested, GameFlowEvents.Quitting },
            //{ GameFlowEvents.Quitting, GameFlowEvents.QuitCompleted }, // typically fired by Quit handler

            // ========================================================================================
            // ALT PATHS (commented) - flip behavior fast
            // ========================================================================================

            // --- ALT: bypass menu entirely ---
            //{ GameFlowEvents.GameplayReady, GameFlowEvents.GameScenesLoadRequested },

            // --- ALT: bypass countdown ---
            //{ GameFlowEvents.GameStarting, GameFlowEvents.GameStarted },

            // --- ALT: auto-complete scene transitions (no scene loader) ---
            //{ GameFlowEvents.GameScenesLoading, GameFlowEvents.GameScenesLoaded },
            //{ GameFlowEvents.GameScenesUnloading, GameFlowEvents.GameScenesUnloaded },
        };

        protected override void Awake()
        {
            base._autoGameFlow2GameFlowEvents = _newAutoGameFlow2GameFlowEvents;

            base._autoUGS2GameFlowEvents.Clear(); // No UGS in this flow
            base._autoGameFlow2UGSEvents.Clear(); // No UGS in this flow

            EventsPublisherGameFlow.Instance.SubscribeToAllEnumEvents(AutoFireGameFlowEventFromGameFlowEvent);
        }

        protected override void OnDestroy()
        {
            EventsPublisherGameFlow.Instance.UnsubscribeToAllEnumEvents(AutoFireGameFlowEventFromGameFlowEvent);
        }

        protected override void Start()
        {
            StartCoroutine(HideLoadingScene());
        }

        private IEnumerator HideLoadingScene()
        {
            yield return new WaitForSecondsRealtime(3f);

            // OLD: LoadingScreenHiding
            // NEW: request hide -> auto fires LoadingScreenHiding
            EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.LoadingScreenHideRequested, this, null);
        }
    }
}