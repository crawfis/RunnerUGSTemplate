using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.Events
{
    internal class DEBUG_GameOnlyEventFlow : MonoBehaviour
    {
        [SerializeField] private float _delayBetweenEvents = 0.1f;

        Dictionary<GameFlowEvents,GameFlowEvents> _autoEvents = new Dictionary<GameFlowEvents, GameFlowEvents>()
        {
            { GameFlowEvents.LoadingScreenShowing, GameFlowEvents.LoadingScreenShown }, // => Show Loading
            { GameFlowEvents.LoadingScreenShown, GameFlowEvents.LoadingScreenHidding }, // => Start Hidding Loading
            { GameFlowEvents.LoadingScreenHidding, GameFlowEvents.LoadingScreenHidden },
            { GameFlowEvents.LoadingScreenHidden, GameFlowEvents.GameplayReady },
            { GameFlowEvents.GameplayReady, GameFlowEvents.MainMenuShowing }, // => Show Menu
            { GameFlowEvents.MainMenuShowing, GameFlowEvents.MainMenuShown }, 
            { GameFlowEvents.GameScenesLoading, GameFlowEvents.MainMenuHidden }, // => Press Play, Hide Menu
            { GameFlowEvents.GameScenesLoading, GameFlowEvents.GameScenesLoaded },
            { GameFlowEvents.GameScenesLoaded, GameFlowEvents.GameStarting },
            { GameFlowEvents.GameStarting, GameFlowEvents.CountdownStarting },
            //{ GameFlowEvents.CountdownStarting, GameFlowEvents.CountdownStarted },
            //{ GameFlowEvents.CountdownStarted, GameFlowEvents.CountdownTick },
            //{ GameFlowEvents.CountdownTick, GameFlowEvents.GameStarted },
            { GameFlowEvents.GameEnding, GameFlowEvents.GameScenesUnloading },
            { GameFlowEvents.GameScenesUnloading, GameFlowEvents.GameScenesUnloaded },
            { GameFlowEvents.GameScenesUnloaded, GameFlowEvents.GameEnded },
            { GameFlowEvents.GameEnded, GameFlowEvents.GameplayReady  }, //=> Show Menu
            { GameFlowEvents.Pause, GameFlowEvents.Resume  },
            { GameFlowEvents.Quitting, GameFlowEvents.Quitted },
        };

        private void Awake()
        {
            EventsPublisherGameFlow.Instance.SubscribeToAllEnumEvents(AutoFireEvent);
        }

        private void AutoFireEvent(string eventName, object sender, object data)
        {
            if (_autoEvents.TryGetValue((GameFlowEvents)Enum.Parse(typeof(GameFlowEvents), eventName), out GameFlowEvents autoEvent))
            {
                StartCoroutine(DelayPublishingNextAutoEvent(autoEvent, sender, data));
            }
        }

        private IEnumerator DelayPublishingNextAutoEvent(GameFlowEvents eventName, object sender, object data)
        {
            yield return new WaitForSeconds(_delayBetweenEvents);
            EventsPublisherGameFlow.Instance.PublishEvent(eventName, sender, data);
        }
    }
}