using CrawfisSoftware.Events;
using CrawfisSoftware.TempleRun.GameConfig;
using CrawfisSoftware.UGS.RemoteConfig;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.UGS
{
    public class GameDifficultyManager : MonoBehaviour
    {
        public string CurrentDifficulty { get; private set; } = "Easy";
        public DifficultyConfig CurrentDifficultyConfig
        {
            get
            {
                if (_difficultyConfigs.ContainsKey(CurrentDifficulty))
                {
                    return _difficultyConfigs[CurrentDifficulty];
                }
                else
                {
                    Debug.LogWarning($"Current difficulty '{CurrentDifficulty}' not found. Returning null.");
                    return null;
                }
            }
        }
        public IEnumerable<string> AvailableDifficulties => _difficultyConfigs.Keys;
        public IEnumerable<DifficultyConfig> AvailableDifficultyConfigs => _difficultyConfigs.Values;

        private readonly Dictionary<string, DifficultyConfig> _difficultyConfigs = new Dictionary<string, DifficultyConfig>();

        public void Awake()
        {
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameDifficultyChanging, OnDifficultyChanging);
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.DifficultySettingsChanged, OnDifficultySettingsChanged);

        }
        private void OnDestroy()
        {
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameDifficultyChanging, OnDifficultyChanging);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.DifficultySettingsChanged, OnDifficultySettingsChanged);
        }

        public void SetDifficulty(string difficultyName)
        {
            Debug.Log($"Attempting to set game difficulty from {CurrentDifficulty} to {difficultyName}");
            if (_difficultyConfigs.ContainsKey(difficultyName))
            {
                CurrentDifficulty = difficultyName;
                //Blackboard.Instance.GameConfig = _difficultyConfigs[difficultyName];
                EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.GameDifficultyChanged, this, _difficultyConfigs[CurrentDifficulty]);
            }
            else
            {
                Debug.LogWarning($"SetDifficulty failed: difficulty '{difficultyName}' not found in available configurations.");
            }
        }

        public void PopulateDifficulties(IList<DifficultyConfig> difficulties)
        {
            //var difficulties = DifficultyObserver.Instance.RuntimeDifficultySettings;
            Clear();
            foreach (var config in difficulties)
            {
                AddConfig(config);
            }

        }
        public void Clear()
        {
            _difficultyConfigs?.Clear();
        }
        public void AddConfig(DifficultyConfig difficultyConfig)
        {
            _difficultyConfigs[difficultyConfig.DifficultyName] = difficultyConfig;
        }

        public void OnDifficultyChanging(string eventName, object sender, object data)
        {
            string newDifficulty = data as string;
            if (string.IsNullOrEmpty(newDifficulty))
            {
                EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.GameDifficultyChangeFailed, this, CurrentDifficultyConfig);
                return;
            }
            SetDifficulty(newDifficulty);
        }

        public void OnDifficultySettingsChanged(string eventName, object sender, object data)
        {
            var difficultyConfigs = data as IList<DifficultyConfig>;
            if (difficultyConfigs == null)
            {
                throw new ArgumentException("OnDifficultySettingsChanged event data must be of type IList<DifficultyConfig>");
            }
            PopulateDifficulties(difficultyConfigs);
        }
    }
}