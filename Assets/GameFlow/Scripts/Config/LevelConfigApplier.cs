using CrawfisSoftware.GameFlow.Events;

using UnityEngine;

namespace CrawfisSoftware.GameFlow.Config
{
    /// <summary>
    /// Applies the selected level's DifficultyConfig to the game config pipeline.
    /// Bridges level selection to the existing difficulty system by publishing
    /// GameConfigApplied, which is already bridged to TempleRunConfigApplied
    /// via TempleRunGameFlowBridge.
    ///    Subscribes: GameFlowEvents.LevelSelected
    ///    Publishes: GameFlowEvents.GameConfigApplied (data: DifficultyConfig)
    /// </summary>
    internal class LevelConfigApplier : MonoBehaviour
    {
        private void Awake()
        {
            EventsPublisherGameFlow.Instance.SubscribeToEvent(
                GameFlowEvents.LevelSelected, OnLevelSelected);
        }

        private void OnDestroy()
        {
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(
                GameFlowEvents.LevelSelected, OnLevelSelected);
        }

        private void OnLevelSelected(string eventName, object sender, object data)
        {
            var levelConfig = data as LevelConfig;
            if (levelConfig?.Difficulty != null)
            {
                EventsPublisherGameFlow.Instance.PublishEvent(
                    GameFlowEvents.GameConfigApplied, this, levelConfig.Difficulty);
            }
        }
    }
}
