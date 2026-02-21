using CrawfisSoftware.Events;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.TempleRun.Events
{
    /// <summary>
    /// Auto-chain TempleRun-specific events. Keep this focused on TempleRun internal lifecycles;
    /// cross-system bridges live in TempleRunGameFlowBridge.
    /// </summary>
    internal class TempleRunAutoEventFlow : AutoEventFlowBase
    {
        [SerializeField] private Dictionary<TempleRunEvents, TempleRunEvents> _autoTempleRun2TempleRunEvents = new Dictionary<TempleRunEvents, TempleRunEvents>()
        {
            // ================================================================================
            // PAUSE / RESUME BRIDGES (mirror GameFlowAutoEventFlow)
            // ================================================================================
            { TempleRunEvents.PlayerPauseRequested, TempleRunEvents.PlayerPausing },
            { TempleRunEvents.PlayerPausing, TempleRunEvents.PlayerPaused },
            { TempleRunEvents.PlayerResumeRequested, TempleRunEvents.PlayerResuming },
            { TempleRunEvents.PlayerResuming, TempleRunEvents.PlayerResumed },

            // ================================================================================
            // COUNTDOWN BRIDGE (mirror GameFlowAutoEventFlow)
            // ================================================================================
            { TempleRunEvents.CountdownStartRequested, TempleRunEvents.CountdownStarting },
            // CountdownStarting -> CountdownTick(s) -> CountdownEnding -> CountdownEnded: published elsewhere

            // ================================================================================
            // GAME START BRIDGE
            // ================================================================================
            { TempleRunEvents.TempleRunStartRequested, TempleRunEvents.TempleRunStarting },
            { TempleRunEvents.TempleRunStarting, TempleRunEvents.TempleRunStarted },

            // ================================================================================
            // GAME END BRIDGE
            // ================================================================================
            { TempleRunEvents.PlayerDied, TempleRunEvents.TempleRunEndRequested },
            { TempleRunEvents.TempleRunEndRequested, TempleRunEvents.TempleRunEnding },
            { TempleRunEvents.TempleRunEnding, TempleRunEvents.TempleRunEnded },

            // ================================================================================
            // LANE CHANGE AUTO-CHAINS
            // ================================================================================
            { TempleRunEvents.LaneChangeLeftRequested, TempleRunEvents.LaneChangingLeft },
            { TempleRunEvents.LaneChangeRightRequested, TempleRunEvents.LaneChangingRight },
            // LaneChangingLeft -> LaneChangedLeft: Published by LaneOffsetController (after lerp completes)
            // LaneChangingRight -> LaneChangedRight: Published by LaneOffsetController (after lerp completes)

            // ================================================================================
            // SLIDE AUTO-CHAINS
            // ================================================================================
            //{ TempleRunEvents.SlideRequested, TempleRunEvents.SlideStarting },
            // SlideStarting -> SlideStarted: Published by SlideController (when slide starts)
            // SlideEnding -> SlideEnded: Published by SlideController (when slide completes)

            // ================================================================================
            // DASH AUTO-CHAINS
            // ================================================================================
            { TempleRunEvents.DashRequested, TempleRunEvents.DashStarting },
            // DashStarting -> DashStarted: Published by DashController (when dash initiates)
            // DashEnding -> DashEnded: Published by DashController (when dash completes)

            // ================================================================================
            // JUMP AUTO-CHAINS
            // ================================================================================
            { TempleRunEvents.JumpRequested, TempleRunEvents.JumpStarting },
            // JumpStarting -> JumpStarted: Published by JumpArcController (at arc apex)
            // JumpStarted -> JumpLanded: Published by JumpArcController (when arc completes)

            // ================================================================================
            // OBSTACLE AUTO-CHAINS
            // ================================================================================
            { TempleRunEvents.ObstacleHit, TempleRunEvents.PlayerFailingAtObstacle },
        };

        protected virtual void Awake()
        {
            EventsPublisherTempleRun.Instance.SubscribeToAllEnumEvents(AutoFireTempleRunEventFromTempleRunEvent);
        }

        protected virtual void OnDestroy()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToAllEnumEvents(AutoFireTempleRunEventFromTempleRunEvent);
        }

        private void AutoFireTempleRunEventFromTempleRunEvent(string eventName, object sender, object data)
        {
            if (_autoTempleRun2TempleRunEvents.TryGetValue((TempleRunEvents)Enum.Parse(typeof(TempleRunEvents), eventName), out TempleRunEvents autoEvent))
            {
                //Debug.Log($"Auto firing event TempleRunEvents.{eventName} to TempleRunEvents.{autoEvent.ToString()}");
                DelayedFire(_delayBetweenEvents, autoEvent.ToString(), sender, data);
            }
        }
    }
}
