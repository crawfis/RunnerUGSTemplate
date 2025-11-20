using CrawfisSoftware.TempleRun;

using System;
using System.Collections.Generic;

using Unity.Services.RemoteConfig;

using UnityEngine;

namespace CrawfisSoftware.GameConfig
{
    public class GameDifficultyManager
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

        public GameDifficultyManager()
        {
            //_difficultyConfigs = new Dictionary<string, DifficultyConfig>();
        }
        public GameDifficultyManager(List<DifficultyConfig> difficultyConfigs)
        {
            foreach(var config in difficultyConfigs)
            {
                AddConfig(config);
            }
        }

        public void SetDifficulty(string difficultyName)
        {
            Debug.Log($"Attempting to set game difficulty to '{difficultyName}'");
            if(_difficultyConfigs.ContainsKey(difficultyName))
            {
                Blackboard.Instance.GameConfig = _difficultyConfigs[difficultyName];
                EventsPublisherTempleRun.Instance.PublishEvent(GamePlayEvents.GameDifficultyChanged, this, _difficultyConfigs[difficultyName]);
                Debug.Log($"Successfully set game difficulty to '{difficultyName}'");
            }
        }

        public void UpdateFromRemoteConfig()
        {
            try
            {
                Debug.Log("Attempting to update difficulty settings from remote config");
                if (RemoteConfigService.Instance.appConfig.HasKey("difficulty_settings") == true)
                {
                    string difficultyJson = RemoteConfigService.Instance.appConfig.GetJson("difficulty_settings");
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
            //_difficultyConfigs = null;
        }
        public void AddConfig(DifficultyConfig difficultyConfig)
        {
            //if (_difficultyConfigs == null)
            //{
            //    _difficultyConfigs = new Dictionary<string, DifficultyConfig>();
            //}
            _difficultyConfigs[difficultyConfig.DifficultyName] = difficultyConfig;
        }

        internal void Initialize(string difficultyLevel, bool logRemoteConfigValues)
        {
            Clear();
            CurrentDifficulty = difficultyLevel;
            Blackboard.Instance.GameDifficultyManager = this;
            // Todo: Set default difficulties here or load from local config file
            // Todo: Set Difficulty based on player prefs or progression
        }
        //private void Awake()
        //{
        //    EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.GameDifficultyChanged, OnDifficultyChanged);
        //}
        //private void OnDestroy()
        //{
        //    EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.GameDifficultyChanged, OnDifficultyChanged);
        //}
        //private void Start()
        //{
        //    // Set difficulty based on player preference or progression
        //    var selectedDifficulty = GetPlayerSelectedDifficulty();
        //    //gameConfig.SetDifficulty(selectedDifficulty);
        //    gameConfig.Initialize(selectedDifficulty);
        //}

        //private DifficultyLevel GetPlayerSelectedDifficulty()
        //{
        //    if (!_usePlayerPrefsDEBUG) return _difficultyLevel;
        //    // This could come from player prefs, progression system, etc.
        //    return (DifficultyLevel)PlayerPrefs.GetInt("SelectedDifficulty", 1); // Default to Medium
        //}

        //public void OnDifficultyChanged(string eventName, object sender, object data)
        //{
        //    int difficultyIndex = (int)data;
        //    var difficulty = (DifficultyLevel)difficultyIndex;
        //    gameConfig.SetDifficulty(difficulty);
        //    PlayerPrefs.SetInt("SelectedDifficulty", difficultyIndex);
        //}
    }
}