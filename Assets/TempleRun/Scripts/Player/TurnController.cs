using UnityEngine;
using TempleRunBus = CrawfisSoftware.Events.EventsFor<CrawfisSoftware.TempleRun.TempleRunEvents>;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Checks whether a turn request is the proper direction and within the turn distance. If so,
    /// it fires a turn successful event.
    ///    Dependencies: Blackboard, DistanceTracker, EventsFor<TempleRunEvents>
    ///    Subscribes: TempleRunEvents.TurnLeftRequested, TurnRightRequested (from bridge
    ///                translating UserInitiated). If it is a valid turn publishes corresponding turn events.
    ///    Subscribes: ActiveTrackChanging - moves the turn window to the new segment. The
    ///                window's far edge is the run-absolute TrackSegmentInfo.TurnFailureDistance
    ///                carried by that message; this class owns only the safe-distance decision,
    ///                and AIController reads the same message rather than reading this class.
    ///    Publishes: TurnLeftStarting, TurnLeftCompleted, TurnRightStarting, TurnRightCompleted
    ///    Publishes: SegmentRequested (data: Direction) when direction is committed at an Either junction
    /// </summary>
    public class TurnController : MonoBehaviour
    {
        public float TurnAvailableDistance { get { return _turnAvailableDistance; } }

        private float _safeTurnDistance = 1f;
        private float _turnAvailableDistance;
        // Possible Bug: If Direction is changed to a Flag, then _nextTrackDirection needs to be masked.
        private Direction _nextTrackDirection;

        public void ForceTurn()
        {
            Direction chosenDirection;
            TempleRunEvents startingEvent;
            TempleRunEvents completedEvent;

            switch (_nextTrackDirection)
            {
                case Direction.Right:
                    chosenDirection = Direction.Right;
                    startingEvent   = TempleRunEvents.TurnRightStarting;
                    completedEvent  = TempleRunEvents.TurnRightCompleted;
                    break;
                case Direction.Either:
                case Direction.Left:
                default:
                    chosenDirection = Direction.Left;
                    startingEvent   = TempleRunEvents.TurnLeftStarting;
                    completedEvent  = TempleRunEvents.TurnLeftCompleted;
                    break;
            }
            OnTurnRequested(this, null, chosenDirection, startingEvent, completedEvent);
        }

        private void Awake()
        {
            // Subscribe to TempleRun domain events, not UserInitiated.
            // This allows turns to be triggered from any source: player input, AI, replay, network.
            // The bridge translates UserInitiated.UserLeftTurnRequested -> TempleRunEvents.TurnLeftRequested
            TempleRunBus.Subscribe(TempleRunEvents.TurnLeftRequested, OnLeftTurnRequested);
            TempleRunBus.Subscribe(TempleRunEvents.TurnRightRequested, OnRightTurnRequested);
            TempleRunBus.Subscribe(TempleRunEvents.ActiveTrackChanging, OnTrackChanging);
            _safeTurnDistance = Blackboard.Instance.GameConfig.SafePreTurnDistance;
        }

        private void OnTurnRequested(object sender, object data, Direction chosenDirection,
                                     TempleRunEvents startingEvent, TempleRunEvents completedEvent)
        {
            float distance = Blackboard.Instance.DistanceTracker.DistanceTravelled;
            if (distance > _turnAvailableDistance)
            {
                TempleRunBus.Publish(startingEvent,  this, distance);

                // ONLY at an Either junction. A Left or Right segment has one exit, already built
                // when the segment was created, so there is nothing to commit - and publishing this
                // for an ordinary turn is destructive: TrackManager would clear
                // _awaitingEitherDirection and generate straight past a junction still waiting for
                // its direction, while PathProvider would resolve that junction's exit using the
                // direction of an unrelated turn somewhere else on the track.
                //
                // Position between starting and completed is load-bearing: PathProvider resolves the
                // junction's exit geometry from this, and SegmentTransitionController consumes that
                // geometry when it sees the completed event. Publishing returns only once the event
                // has been delivered, so the geometry is in place by the time completed is published.
                if (_nextTrackDirection == Direction.Either)
                    TempleRunBus.Publish(TempleRunEvents.SegmentRequested, this, chosenDirection);

                TempleRunBus.Publish(completedEvent, this, distance);
            }
        }

        private void OnLeftTurnRequested(string eventName, object sender, object data)
        {
            if (_nextTrackDirection == Direction.Left || _nextTrackDirection == Direction.Either)
            {
                OnTurnRequested(sender, data, Direction.Left,
                                TempleRunEvents.TurnLeftStarting, TempleRunEvents.TurnLeftCompleted);
            }
        }

        private void OnRightTurnRequested(string eventName, object sender, object data)
        {
            if (_nextTrackDirection == Direction.Right || _nextTrackDirection == Direction.Either)
            {
                OnTurnRequested(sender, data, Direction.Right,
                                TempleRunEvents.TurnRightStarting, TempleRunEvents.TurnRightCompleted);
            }
        }

        private void OnTrackChanging(string eventName, object sender, object data)
        {
            var trackSegment = (TrackSegmentInfo)data;
            _nextTrackDirection  = trackSegment.Direction;
            // The window's far edge arrives run-absolute on the message. This used to be a private
            // running sum of segment lengths, and the sum had to be anchored to the segment's start
            // rather than to the turn points: summing turn points lost (Length - turn point) per
            // segment, walking the window earlier and earlier, and a Straight's float.MaxValue
            // saturated the total and disabled every later turn. TrackManager now owns that
            // arithmetic once, so neither trap can be re-entered here.
            _turnAvailableDistance = trackSegment.TurnFailureDistance - _safeTurnDistance;
        }

        private void OnDestroy()
        {
            TempleRunBus.Unsubscribe(TempleRunEvents.TurnLeftRequested, OnLeftTurnRequested);
            TempleRunBus.Unsubscribe(TempleRunEvents.TurnRightRequested, OnRightTurnRequested);
            TempleRunBus.Unsubscribe(TempleRunEvents.ActiveTrackChanging, OnTrackChanging);
        }
    }
}
