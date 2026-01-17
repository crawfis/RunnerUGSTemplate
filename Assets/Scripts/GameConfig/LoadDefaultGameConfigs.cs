using CrawfisSoftware.Events;

using UnityEngine;

namespace CrawfisSoftware.TempleRun.GameConfig
{
    internal class LoadDefaultGameConfigs : MonoBehaviour
    {
        [SerializeField] private TempleRunGameConfig _gameConfig;
        [SerializeField] private string _difficultyLevel = "Easy";

        private void Start()
        {
            EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.DifficultySettingsApplied, this, _gameConfig.DifficultyConfigs);
            EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.DifficultyChangeRequested, this, _difficultyLevel);
        }
    }
}