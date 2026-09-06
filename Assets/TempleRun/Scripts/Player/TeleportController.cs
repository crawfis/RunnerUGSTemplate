using System.Collections;

using UnityEngine;
using TempleRunBus = CrawfisSoftware.Events.EventsFor<CrawfisSoftware.TempleRun.TempleRunEvents>;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Start and end the teleportation when the current spline is changing. Allows for a cinematic
    /// teleportation or a smoother teleportation and rotation.
    ///    Dependency: EventsFor<TempleRunEvents>
    ///    Subscribes: TempleRunEvents.CurrentSplineChanging
    ///    Publishes: TeleportStarted — DistanceController halts movement
    ///    Publishes: TeleportEnded — DistanceController resumes and snaps to LandingDistance
    /// </summary>
    public class TeleportController : MonoBehaviour
    {
        [SerializeField] private float _teleportDuration = 1.0f;
        private void Awake()
        {
            TempleRunBus.Subscribe(TempleRunEvents.CurrentSplineChanging, OnActiveSplineChanging);
        }

        private void OnDestroy()
        {
            TempleRunBus.Unsubscribe(TempleRunEvents.CurrentSplineChanging, OnActiveSplineChanging);
        }

        private void OnActiveSplineChanging(string EventName, object sender, object data)
        {
            // Only a turn's exit is teleported onto; an approach is run along. The section says
            // which it is, so this test and any other half of the same rule read one named thing.
            var section = (SplineSection)data;
            if (!section.TeleportOwnsTransform)
                return;
            StartCoroutine(TeleportWithDelay(section));
        }

        private IEnumerator TeleportWithDelay(SplineSection section)
        {
            TempleRunBus.Publish(TempleRunEvents.TeleportStarted, this, new TeleportInfo(_teleportDuration, section));
            yield return new WaitForSecondsRealtime(_teleportDuration);
            TempleRunBus.Publish(TempleRunEvents.TeleportEnded, this, section);
            // No resume published here. A teleport never paused: the freeze during a teleport
            // is DistanceController._isMoving, toggled by TeleportStarted/TeleportEnded above.
            // Publishing a resume released a pause this class never took - and if the player
            // had paused mid-teleport, it un-paused them.
        }
    }
}