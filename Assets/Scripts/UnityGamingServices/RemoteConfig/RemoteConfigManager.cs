using CrawfisSoftware.GameConfig;

using System;

using Unity.Services.RemoteConfig;

using UnityEngine;

namespace CrawfisSoftware.UGS
{
    public class RemoteConfigManager
    {
        private bool _logRemoteConfigValues = true;

        private GameDifficultyManager _gameDifficultyManager;
        private FeatureFlagsManager _featureFlagsManager;
        private GameBalanceManager _gameBalanceManager;
        private CampaignEventConfigManager _eventConfigManager;
        private bool _isInitialized = false;

        public FeatureFlags FeatureFlags => _featureFlagsManager?.FeatureFlags ?? default;
        public GameBalance GameBalance => _gameBalanceManager?.GameBalance ?? default;
        public CampaignEventConfig EventConfig => _eventConfigManager?.EventConfig ?? default;
        public bool IsInitialized => _isInitialized;

        public void Initialize(string difficultyLevel, bool logValues = true)
        {
            if (_isInitialized) return;

            _logRemoteConfigValues = logValues;
            
            InitializeManagers(difficultyLevel);
            InitializeRemoteConfig();

            _isInitialized = true;
        }

        private void InitializeManagers(string difficultyLevel)
        {
            _gameDifficultyManager = new GameDifficultyManager();
            _featureFlagsManager = new FeatureFlagsManager();
            _gameBalanceManager = new GameBalanceManager();
            _eventConfigManager = new CampaignEventConfigManager();

            _gameDifficultyManager.Initialize(difficultyLevel, _logRemoteConfigValues);
            _featureFlagsManager.Initialize(_logRemoteConfigValues);
            _gameBalanceManager.Initialize(_logRemoteConfigValues);
            _eventConfigManager.Initialize(_logRemoteConfigValues);
        }

        private void InitializeRemoteConfig()
        {
            if (RemoteConfigService.Instance != null)
            {
                RemoteConfigService.Instance.FetchCompleted += OnRemoteConfigFetched;
                var userAttributes = CreateUserAttributes();
                var appAttributes = CreateAppAttributes();
                RemoteConfigService.Instance.FetchConfigs(userAttributes, appAttributes);
            }
        }
        private UserAttributes CreateUserAttributes()
        {
            return new UserAttributes
            {
                DeviceType = SystemInfo.deviceType.ToString(),
                Platform = Application.platform.ToString(),
                AppVersion = Application.version,
                PlayerLevel = PlayerPrefs.GetInt("PlayerLevel", 1),
                Country = Application.systemLanguage.ToString()
            };
        }

        private AppAttributes CreateAppAttributes()
        {
            return new AppAttributes
            {
                AppVersion = Application.version,
                BuildNumber = Application.buildGUID,
                UnityVersion = Application.unityVersion,
                IsDebugBuild = Debug.isDebugBuild
            };
        }

        private void OnRemoteConfigFetched(ConfigResponse response)
        {
            RemoteConfigService.Instance.FetchCompleted -= OnRemoteConfigFetched;
            if (response.status == ConfigRequestStatus.Success)
            {
                Debug.Log($"Remote config fetched with response: {response.status}");
                ApplyRemoteConfig();
                if (_logRemoteConfigValues)
                {
                    LogRemoteConfigValues();
                }
            }
            else
            {
                Debug.LogWarning($"Remote Config fetch failed: {response.status}");
                EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.RemoteConfigFailed, this, response.status);
            }
        }

        private void ApplyRemoteConfig()
        {
            try
            {
                _gameDifficultyManager.UpdateFromRemoteConfig();
                _featureFlagsManager?.UpdateFromRemoteConfig();
                _gameBalanceManager?.UpdateFromRemoteConfig();
                _eventConfigManager?.UpdateFromRemoteConfig();
                EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.RemoteConfigUpdated, this, UnityEngine.Time.time);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to apply remote configuration: {e.Message}");
                EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.RemoteConfigFailed, this, e.Message);
            }
        }

        public bool IsFeatureEnabled(string featureName)
        {
            return _featureFlagsManager?.IsFeatureEnabled(featureName) ?? false;
        }

        private void LogRemoteConfigValues()
        {
            Debug.Log("=== Remote Config Values ===");
            _featureFlagsManager?.LogValues();
            _gameBalanceManager?.LogValues();
            _eventConfigManager?.LogValues();
        }

        public void Dispose()
        {
            if (RemoteConfigService.Instance != null)
            {
                RemoteConfigService.Instance.FetchCompleted -= OnRemoteConfigFetched;
            }
            _isInitialized = false;
        }
    }
}