using CrawfisSoftware.Events;

using System.Collections;

using UnityEngine;

namespace CrawfisSoftware.TempleRun.Debugging
{
    /// <summary>
    /// Debug helper for a boot scene that auto-drives the GameFlow/TempleRun sequence without UI.
    /// Publishes minimal events to move from boot -> load -> configured -> countdown end.
    /// GameFlow auto-flows and bridges then start TempleRun.
    /// </summary>
    internal class DEBUG_BootTempleRunFlow : MonoBehaviour
    {
        [SerializeField] private float _initialDelay = 0.2f;
        [SerializeField] private float _configDelay = 0.2f;
        [SerializeField] private float _sceneLoadDelay = 0.2f;
        [SerializeField] private float _countdownDelay = 0.2f;

        private void Start()
        {
            StartCoroutine(RunFlow());
        }

        private IEnumerator RunFlow()
        {
            if (_initialDelay > 0f) yield return new WaitForSecondsRealtime(_initialDelay);

            // Kick hide-loading and configuration applied so GameConfigInitializer runs.
            EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.LoadingScreenHideRequested, this, null);
            yield return new WaitForSecondsRealtime(_configDelay);
            EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.GameConfigApplied, this, null);

            // Drive scene load and mark loaded (auto-flow will request start/countdown).
            EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.GameScenesLoadRequested, this, null);
            yield return new WaitForSecondsRealtime(_sceneLoadDelay);
            EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.GameScenesLoaded, this, null);

            // Complete countdown via TempleRun events to trigger GameController -> GameFlow GameStarted.
            yield return new WaitForSecondsRealtime(_countdownDelay);
            EventsPublisherTempleRun.Instance.PublishEvent(TempleRunEvents.CountdownEnding, this, null);
            EventsPublisherTempleRun.Instance.PublishEvent(TempleRunEvents.CountdownEnded, this, null);
        }
    }
}
