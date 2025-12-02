using CrawfisSoftware.UGS;

using System.Collections;
using System.Collections.Generic;

using Unity.Services.Core;

using UnityEngine;

namespace CrawfisSoftware.Events
{
    internal class DEBUG_GameOnlyEventFlow : UGSAndGameEventFlow
    {
        Dictionary<GameFlowEvents,GameFlowEvents> _newAutoGameFlow2GameFlowEvents = new Dictionary<GameFlowEvents, GameFlowEvents>()
        {
            //{ GameFlowEvents.LoadingScreenShowing, GameFlowEvents.LoadingScreenShown }, // => Show Loading
            //{ GameFlowEvents.LoadingScreenShown, GameFlowEvents.LoadingScreenHidding }, // => Start Hidding Loading
            //{ GameFlowEvents.LoadingScreenHidding, GameFlowEvents.LoadingScreenHidden },
            { GameFlowEvents.LoadingScreenHidden, GameFlowEvents.GameplayReady },
            { GameFlowEvents.GameplayReady, GameFlowEvents.MainMenuShowing }, // => Show Menu
            //{ GameFlowEvents.MainMenuShowing, GameFlowEvents.MainMenuShown }, 
            //{ GameFlowEvents.GameplayNotReady, GameFlowEvents.MainMenuHidden }, // => Hide Menu
            //{ GameFlowEvents.GameScenesLoading, GameFlowEvents.MainMenuHidden }, // => Press Play, Hide Menu
            //{ GameFlowEvents.MainMenuHidden, GameFlowEvents.GameScenesLoaded },
            { GameFlowEvents.GameScenesLoaded, GameFlowEvents.GameStarting },
            { GameFlowEvents.GameStarting, GameFlowEvents.CountdownStarting },
            //{ GameFlowEvents.CountdownStarting, GameFlowEvents.CountdownStarted },
            //{ GameFlowEvents.CountdownStarted, GameFlowEvents.CountdownTick },
            { GameFlowEvents.CountdownEnding, GameFlowEvents.CountdownEnded },
            //{ GameFlowEvents.CountdownEnded, GameFlowEvents.GameStarted },
            //{ GameFlowEvents.GameEnding, GameFlowEvents.GameFlowEvents.GameEnded },
            //{ GameFlowEvents.GameScenesUnloading, GameFlowEvents.GameScenesUnloaded },
            { GameFlowEvents.GameEnding, GameFlowEvents.GameScenesUnloading },
            { GameFlowEvents.GameEnded, GameFlowEvents.GameplayReady  }, //=> Show Menu
            //{ GameFlowEvents.Pause, GameFlowEvents.Resume  },
            //{ GameFlowEvents.Quitting, GameFlowEvents.Quitted }, // Quitted is fired by the Quitting GameObject in the 0_BootStrap scene
        };

        protected override void Awake()
        {
            base._autoGameFlow2GameFlowEvents = _newAutoGameFlow2GameFlowEvents;
            base._autoUGS2UGSEvents.Clear(); // No UGS events in this flow
            base._autoUGS2GameFlowEvents.Clear(); // No UGS events in this flow
            base._autoGameFlow2UGSEvents.Clear(); // No UGS events in this flow

            //EventsPublisherGameFlow.Instance.SubscribeToAllEnumEvents(AutoFireUGSEventFromGameFlowEvent);
            EventsPublisherGameFlow.Instance.SubscribeToAllEnumEvents(AutoFireGameFlowEventFromGameFlowEvent);
        }

        protected override void OnDestroy()
        {
            //EventsPublisherGameFlow.Instance.UnsubscribeToAllEnumEvents(AutoFireUGSEventFromGameFlowEvent);
            EventsPublisherGameFlow.Instance.UnsubscribeToAllEnumEvents(AutoFireGameFlowEventFromGameFlowEvent);
        }

        protected override void Start()
        {
            StartCoroutine(HideLoadingScene());
        }

        private IEnumerator HideLoadingScene()
        {
            yield return new WaitForSecondsRealtime(3f);
            EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.LoadingScreenHidding, this, null);
        }
    }
}