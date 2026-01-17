using System;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.Events
{
    internal class GameFlowAutoEventFlow : AutoEventFlowBase
    {
        [SerializeField] private Dictionary<GameFlowEvents, GameFlowEvents> _autoGameFlow2GameFlowEvents = new Dictionary<GameFlowEvents, GameFlowEvents>()
        {
            // Loading Screen bridges
            { GameFlowEvents.LoadingScreenShowRequested, GameFlowEvents.LoadingScreenShowing },
            { GameFlowEvents.LoadingScreenHideRequested, GameFlowEvents.LoadingScreenHiding },

            // Main Menu bridges
            { GameFlowEvents.MainMenuShowRequested, GameFlowEvents.MainMenuShowing },
            { GameFlowEvents.MainMenuHideRequested, GameFlowEvents.MainMenuHiding },
            //{ GameFlowEvents.MainMenuHiding, GameFlowEvents.MainMenuHidden },

            // Session bridges
            { GameFlowEvents.GameStartRequested, GameFlowEvents.GameStarting },
            //{ GameFlowEvents.GameEndRequested, GameFlowEvents.GameEnding }, // Keep data-rich GameEnding published by controller

            // Countdown bridge
            { GameFlowEvents.CountdownStartRequested, GameFlowEvents.CountdownStarting },

            // Scene bridges
            { GameFlowEvents.GameScenesLoadRequested, GameFlowEvents.GameScenesLoading },
            { GameFlowEvents.GameScenesUnloadRequested, GameFlowEvents.GameScenesUnloading },

            // Config / Difficulty bridges
            { GameFlowEvents.GameConfigChangeRequested, GameFlowEvents.GameConfigApplying },
            { GameFlowEvents.DifficultyChangeRequested, GameFlowEvents.DifficultyChanging },

            // Save / Load bridges (optional)
            //{ GameFlowEvents.SaveLoadRequested, GameFlowEvents.SaveLoading },
            //{ GameFlowEvents.SaveLoading, GameFlowEvents.SaveLoaded },
            //{ GameFlowEvents.SaveRequested, GameFlowEvents.Saving },
            //{ GameFlowEvents.Saving, GameFlowEvents.Saved },

            // Quit bridge
            { GameFlowEvents.QuitRequested, GameFlowEvents.Quitting },

            // Pause/Resume auto chains (publish PauseRequested/ResumeRequested from input)
            { GameFlowEvents.PauseRequested, GameFlowEvents.Pausing },
            { GameFlowEvents.Pausing, GameFlowEvents.Paused },

            { GameFlowEvents.ResumeRequested, GameFlowEvents.Resuming },
            { GameFlowEvents.Resuming, GameFlowEvents.Resumed },

            // ========================================================================================
            // NORMAL FULL GAME TIMELINE (documented here, only some entries are auto-fired)
            // ========================================================================================

            // --- Ready gate -> Main Menu ---
            { GameFlowEvents.GameplayReady, GameFlowEvents.MainMenuShowRequested },

            // --- Start -> Countdown -> Gameplay ---
            { GameFlowEvents.GameScenesLoaded, GameFlowEvents.GameStartRequested },
            { GameFlowEvents.GameStarting, GameFlowEvents.CountdownStartRequested },

            // --- End -> Unload -> Return to menu ---
            { GameFlowEvents.GameEnding, GameFlowEvents.GameScenesUnloadRequested },
        };

        protected virtual void Awake()
        {
            EventsPublisherGameFlow.Instance.SubscribeToAllEnumEvents(AutoFireGameFlowEventFromGameFlowEvent);
        }

        protected virtual void OnDestroy()
        {
            EventsPublisherGameFlow.Instance.UnsubscribeToAllEnumEvents(AutoFireGameFlowEventFromGameFlowEvent);
        }

        private void AutoFireGameFlowEventFromGameFlowEvent(string eventName, object sender, object data)
        {
            if (_autoGameFlow2GameFlowEvents.TryGetValue((GameFlowEvents)Enum.Parse(typeof(GameFlowEvents), eventName), out GameFlowEvents autoEvent))
            {
                DelayedFire(_delayBetweenEvents, autoEvent.ToString(), sender, data);
            }
        }
    }
}
