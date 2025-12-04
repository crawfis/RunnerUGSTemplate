using CrawfisSoftware.Events;
using CrawfisSoftware.TempleRun;
using CrawfisSoftware.TempleRun.Assets.Scripts.UnityGamingServices.RemoteConfig;
using CrawfisSoftware.TempleRun.GameConfig;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Unity.Services.RemoteConfig;

using UnityEngine;

namespace CrawfisSoftware.UGS
{
    public class GameDifficultyManager : MonoBehaviour
    {
        private Dictionary<string, DifficultyConfig> _difficultyConfigs = new Dictionary<string, DifficultyConfig>();

        public string CurrentDifficulty { get; private set; }
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

        public void Awake()
        {
            if (UGS_State.IsRemoteConfigUpdated)
            {
                UpdateFromRemoteConfig(RemoteConfigConstants.difficultySettingsKey);
            }
            else
            {
                EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.RemoteConfigUpdated, OnRemoteConfigUpdated);
            }
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameDifficultyChanging, OnDifficultyChanging);
        }
        private void OnDestroy()
        {
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.RemoteConfigUpdated, OnRemoteConfigUpdated);
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameDifficultyChanging, OnDifficultyChanging);
        }

        public void SetDifficulty(string difficultyName)
        {
            Debug.Log($"Attempting to set game difficulty to '{difficultyName}'");
            if(_difficultyConfigs.ContainsKey(difficultyName))
            {
                CurrentDifficulty = difficultyName;
                //Blackboard.Instance.GameConfig = _difficultyConfigs[difficultyName];
                EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.GameDifficultyChanged, this, _difficultyConfigs[CurrentDifficulty]);
                //Debug.Log($"Successfully set game difficulty to '{difficultyName}'");
            }
        }

        public void UpdateFromRemoteConfig(string remoteConfigKey)
        {
            try
            {
                Debug.Log("Attempting to update difficulty settings from remote config");
                var remoteConfig = RemoteConfigService.Instance.appConfig;
                var keys = remoteConfig.GetKeys();
                if (RemoteConfigService.Instance.appConfig.HasKey(remoteConfigKey) == true)
                {
                    string difficultyJson = RemoteConfigService.Instance.appConfig.GetJson(remoteConfigKey);
                    DifficultySettings _remoteDifficultySettings = JsonUtility.FromJson<DifficultySettings>(difficultyJson);
                    if(_remoteDifficultySettings != null && _remoteDifficultySettings.Configs != null)
                    {
                        Clear();
                        foreach (var config in _remoteDifficultySettings.Configs)
                        {
                            AddConfig(config);
                        }
                        Debug.Log($"Remote difficulty settings added {_difficultyConfigs.Count} configs");
                        SetDifficulty(CurrentDifficulty);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to update difficulty settings from remote config: {e.Message}");
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

        public void OnRemoteConfigUpdated(string eventName, object sender, object data)
        {
            UpdateFromRemoteConfig(RemoteConfigConstants.difficultySettingsKey);
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
    }
}