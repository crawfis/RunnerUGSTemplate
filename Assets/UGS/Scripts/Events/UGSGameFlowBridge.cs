using CrawfisSoftware.Events;
using CrawfisSoftware.GameFlow.Events;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.UGS.Events
{
    /// <summary>
    /// Bridges UGS events to/from GameFlow events. This class connects the two event domains.
    ///
    /// --- BOOT: UGS to GameFlow ---
    /// [BRIDGE] PlayerAuthenticated -> GameplayReady
    /// [BRIDGE] RemoteConfigUpdated -> LoadingScreenHideRequested
    /// [BRIDGE] PlayerSignedOut -> GameplayNotReady
    ///
    /// --- GAME END: GameFlow to UGS ---
    /// [BRIDGE] GameEnding -> ScoreUpdating
    /// [BRIDGE] GameEnded -> LeaderboardOpening
    /// </summary>
    internal class UGSGameFlowBridge : MonoBehaviour
    {
        private Dictionary<UGS_EventsEnum, GameFlowEvents> _autoUGS2GameFlowEvents = new Dictionary<UGS_EventsEnum, GameFlowEvents>()
        {
            { UGS_EventsEnum.PlayerAuthenticated, GameFlowEvents.GameplayReady },
            { UGS_EventsEnum.PlayerSignedOut, GameFlowEvents.GameplayNotReady },

            // Remote config update requests the loading screen to hide; flow auto-fires LoadingScreenHiding.
            { UGS_EventsEnum.RemoteConfigUpdated, GameFlowEvents.LoadingScreenHideRequested },

            // Difficulty settings fetched from remote config
            { UGS_EventsEnum.DifficultySettingsFetched, GameFlowEvents.DifficultySettingsApplied },
        };

        private Dictionary<GameFlowEvents, UGS_EventsEnum> _autoGameFlow2UGSEvents = new Dictionary<GameFlowEvents, UGS_EventsEnum>()
        {
            { GameFlowEvents.GameEnding, UGS_EventsEnum.ScoreUpdating },
            { GameFlowEvents.GameEnded, UGS_EventsEnum.LeaderboardOpening },

            // Alternative: kick leaderboard earlier
            //{ GameFlowEvents.GameScenesUnloaded, UGS_EventsEnum.LeaderboardOpening },
        };

        protected virtual void Awake()
        {
            EventsPublisherUGS.Instance.SubscribeToAllEnumEvents(AutoFireGameFlowEventFromUGSEvent);
            EventsPublisherGameFlow.Instance.SubscribeToAllEnumEvents(AutoFireUGSEventFromGameFlowEvent);
        }

        protected virtual void OnDestroy()
        {
            EventsPublisherUGS.Instance.UnsubscribeToAllEnumEvents(AutoFireGameFlowEventFromUGSEvent);
            EventsPublisherGameFlow.Instance.UnsubscribeToAllEnumEvents(AutoFireUGSEventFromGameFlowEvent);
        }

        private void AutoFireGameFlowEventFromUGSEvent(string eventName, object sender, object data)
        {
            ReadOnlySpan<char> input = eventName.AsSpan();
            int index = input.LastIndexOf('/');
            if (index < 0) return;
            string result = input.Slice(index + 1).ToString();
            UGS_EventsEnum uGS_Event = Enum.Parse<UGS_EventsEnum>(result);
            if (_autoUGS2GameFlowEvents.TryGetValue(uGS_Event, out GameFlowEvents autoEvent))
            {
                EventsPublisherGameFlow.Instance.PublishEvent(autoEvent, sender, data);
            }
        }

        private void AutoFireUGSEventFromGameFlowEvent(string eventName, object sender, object data)
        {
            ReadOnlySpan<char> input = eventName.AsSpan();
            int index = input.LastIndexOf('/');
            if (index < 0) return;
            string result = input.Slice(index + 1).ToString();
            GameFlowEvents gameFlowEvent = Enum.Parse<GameFlowEvents>(result);
            if (_autoGameFlow2UGSEvents.TryGetValue(gameFlowEvent, out UGS_EventsEnum autoEvent))
            {
                EventsPublisherUGS.Instance.PublishEvent(autoEvent, sender, data); 
            }
        }
    }
}