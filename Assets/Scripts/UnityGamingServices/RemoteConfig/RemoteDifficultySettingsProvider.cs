using CrawfisSoftware.GameConfig;

using System;

using Unity.Services.RemoteConfig;

using UnityEngine;

//namespace CrawfisSoftware.UGS
//{
//    public class RemoteDifficultySettingsProvider : IDifficultySettingsProvider
//    {
//        private DifficultySettings _remoteDifficultySettings;
//        private readonly IDifficultySettingsProvider _fallbackProvider;
//        private bool _isLoaded = false;

//        public bool IsLoaded => _isLoaded;
//        public event Action<DifficultySettings> OnDifficultySettingsUpdated;

//        public RemoteDifficultySettingsProvider(IDifficultySettingsProvider fallbackProvider)
//        {
//            _fallbackProvider = fallbackProvider;
//            InitializeDefaultValues();
//        }

//        public DifficultyConfig GetDifficultyConfig(DifficultyLevel level)
//        {
//            if (_isLoaded && _remoteDifficultySettings != null)
//            {
//                return _remoteDifficultySettings.GetDifficulty(level);
//            }
            
//            return _fallbackProvider?.GetDifficultyConfig(level) ?? new DifficultyConfig();
//        }

//        public void UpdateFromRemoteConfig()
//        {
//            try
//            {
//                Debug.Log("Attempting to update difficulty settings from remote config");
//                if (RemoteConfigService.Instance.appConfig.HasKey("difficulty_settings") == true)
//                {
//                    string difficultyJson = RemoteConfigService.Instance.appConfig.GetJson("difficulty_settings");
//                    JsonUtility.FromJsonOverwrite(difficultyJson, _remoteDifficultySettings);
//                    _isLoaded = true;
//                    OnDifficultySettingsUpdated.Invoke(_remoteDifficultySettings);
//                    Debug.Log("Remote difficulty settings loaded successfully");
//                }
//            }
//            catch (Exception e)
//            {
//                Debug.LogError($"Failed to update difficulty settings from remote config: {e.Message}");
//                _isLoaded = false;
//            }
//        }

//        private void InitializeDefaultValues()
//        {
//            _remoteDifficultySettings = _fallbackProvider?.GetDifficultyConfig(DifficultyLevel.Medium) != null 
//                ? new DifficultySettings() 
//                : CreateDefaultDifficultySettings();
//        }

//        private DifficultySettings CreateDefaultDifficultySettings()
//        {
//            return new DifficultySettings
//            {
//                Easy = new DifficultyConfig { DifficultyName = "Easy", InitialSpeed = 3f, MaxSpeed = 6f, Acceleration = 0.15f, StartRunway = 15, NumberOfLives = 5 },
//                Medium = new DifficultyConfig { DifficultyName="Medium", InitialSpeed = 5f, MaxSpeed = 8f, Acceleration = 0.2f, StartRunway = 10, NumberOfLives = 3 },
//                Hard = new DifficultyConfig { DifficultyName = "Hard", InitialSpeed = 6f, MaxSpeed = 10f, Acceleration = 0.25f, StartRunway = 8, NumberOfLives = 2 },
//                Insane = new DifficultyConfig { DifficultyName = "Insane", InitialSpeed = 8f, MaxSpeed = 15f, Acceleration = 0.35f, StartRunway = 5, NumberOfLives = 1 },
//                Exergame = new DifficultyConfig { DifficultyName = "Exergame", InitialSpeed = 4f, MaxSpeed = 7f, Acceleration = 0.18f, StartRunway = 12, NumberOfLives = 10 }
//            };
//        }
//    }
//}