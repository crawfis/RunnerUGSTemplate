using CrawfisSoftware.Events;
using CrawfisSoftware.GameFlow.Events;
using CrawfisSoftware.TempleRun;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.TempleRun.Events
{
    internal class TempleRunGameFlowBridge : AutoEventFlowBase
    {
        [SerializeField] private Dictionary<TempleRunEvents, GameFlowEvents> _autoTempleRun2GameFlowEvents = new Dictionary<TempleRunEvents, GameFlowEvents>()
        {
            // TempleRun paused -> request GameFlow pause (for menus/UI)
            { TempleRunEvents.PlayerPaused, GameFlowEvents.PauseRequested },

            // Countdown ended -> game officially started (absorbed from GameController)
            { TempleRunEvents.CountdownEnded, GameFlowEvents.GameStarted },

            // Player died -> game ending (absorbed from GameController)
            { TempleRunEvents.PlayerDied, GameFlowEvents.GameEnding },

            // Difficulty: TempleRun publishes, GameFlow's GameDifficultyManager processes
            { TempleRunEvents.TempleRunDifficultySettingsApplied, GameFlowEvents.DifficultySettingsApplied },
            { TempleRunEvents.TempleRunDifficultyChanging, GameFlowEvents.DifficultyChanging },
            { TempleRunEvents.TempleRunDifficultyChangeRequested, GameFlowEvents.DifficultyChangeRequested },
        };

        [SerializeField] private Dictionary<GameFlowEvents, TempleRunEvents> _autoGameFlow2TempleRunEvents = new Dictionary<GameFlowEvents, TempleRunEvents>()
        {
            // Bridge start: when the broader game signals started, fire TempleRun start requested
            { GameFlowEvents.GameStarted, TempleRunEvents.TempleRunStartRequested },

            // Note: GameFlow.Resumed -> PlayerResumeRequested REMOVED to prevent double-resume.
            // Obstacle recovery pause/resume is self-contained in TempleRun domain.
            // If a GameFlow pause menu needs to resume TempleRun, add explicit manual bridging.

            // GameFlow starting -> kick off countdown in TempleRun
            { GameFlowEvents.GameStarting, TempleRunEvents.CountdownStartRequested },

            // Config/scenes bridged to TempleRun domain
            { GameFlowEvents.GameConfigApplied, TempleRunEvents.TempleRunConfigApplied },
            { GameFlowEvents.GameScenesLoaded, TempleRunEvents.TempleRunScenesReady },

            // Difficulty responses from GameDifficultyManager back to TempleRun
            // Note: DifficultySettingsApplied flows TempleRun->GameFlow only (no reverse to avoid loop)
            { GameFlowEvents.DifficultyChanged, TempleRunEvents.TempleRunDifficultyChanged },
            { GameFlowEvents.DifficultyChangeFailed, TempleRunEvents.TempleRunDifficultyChangeFailed },
        };

        protected virtual void Awake()
        {
            EventsPublisherTempleRun.Instance.SubscribeToAllEnumEvents(AutoFireGameFlowEventFromTempleRunEvent);
            EventsPublisherGameFlow.Instance.SubscribeToAllEnumEvents(AutoFireTempleRunEventFromGameFlowEvent);

            // Manual cross-publisher bridge: UserInitiated → GameFlow
            EventsPublisherUserInitiated.Instance.SubscribeToEvent(
                UserInitiatedEvents.QuitRequested, OnQuitRequested);
        }

        protected virtual void OnDestroy()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToAllEnumEvents(AutoFireGameFlowEventFromTempleRunEvent);
            EventsPublisherGameFlow.Instance.UnsubscribeToAllEnumEvents(AutoFireTempleRunEventFromGameFlowEvent);

            EventsPublisherUserInitiated.Instance.UnsubscribeToEvent(
                UserInitiatedEvents.QuitRequested, OnQuitRequested);
        }

        private void OnQuitRequested(string eventName, object sender, object data)
        {
            DelayedFire(_delayBetweenEvents, GameFlowEvents.GameEndRequested.ToString(), sender, data);
        }

        private void AutoFireGameFlowEventFromTempleRunEvent(string eventName, object sender, object data)
        {
            if (_autoTempleRun2GameFlowEvents.TryGetValue((TempleRunEvents)Enum.Parse(typeof(TempleRunEvents), eventName), out GameFlowEvents autoEvent))
            {
                DelayedFire(_delayBetweenEvents, autoEvent.ToString(), sender, data);
            }
        }

        private void AutoFireTempleRunEventFromGameFlowEvent(string eventName, object sender, object data)
        {
            if (_autoGameFlow2TempleRunEvents.TryGetValue((GameFlowEvents)Enum.Parse(typeof(GameFlowEvents), eventName), out TempleRunEvents autoEvent))
            {
                DelayedFire(_delayBetweenEvents, autoEvent.ToString(), sender, data);
            }
        }
    }
}