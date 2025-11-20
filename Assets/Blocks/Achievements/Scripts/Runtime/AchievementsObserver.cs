using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Blocks.Achievements
{
    /// <summary>
    /// The class handling and connecting the required services to make achievements work.
    /// Is a singleton
    /// </summary>
    public class AchievementsObserver : IDisposable
    {
        IAchievementService m_AchievementService;

        ServiceObserver<IAuthenticationService> m_AuthenticationServiceObserver;
        ServiceObserver<ICloudCodeService> m_CloudCodeServiceObserver;
        ServiceObserver<ICloudSaveService> m_CloudSaveServiceObserver;

        IAuthenticationService m_AuthenticationService;
        ICloudCodeService m_CloudCodeService;
        ICloudSaveService m_CloudSaveService;
        RemoteConfigService m_RemoteConfigService;

        bool m_UseTrustedClient;

        static AchievementsObserver s_Instance;

        /// <summary>
        /// The runtime achievement data
        /// </summary>
        public RuntimeAchievementData RuntimeAchievementData { get; }

        /// <summary>
        /// Determines whether to use local client or cloud code module
        /// </summary>
        public bool UseTrustedClient
        {
            set
            {
                if (m_UseTrustedClient != value)
                {
                    m_UseTrustedClient = value;
                    SetAchievementService(value);
                }
            }
        }

        /// <summary>
        /// Accessor for the singleton instance
        /// </summary>
        public static AchievementsObserver Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new AchievementsObserver();
                }

                return s_Instance;
            }
        }

        /// <summary>
        /// Event invoked when an achievement is unlocked
        /// </summary>
        public event Action<Achievement> AchievementUnlocked;

        AchievementsObserver()
        {
            RuntimeAchievementData = new RuntimeAchievementData();

            if (!Application.isPlaying)
            {
                RuntimeAchievementData.SetDummyData();
                return;
            }

            m_AuthenticationServiceObserver = InitializeServiceObserver<IAuthenticationService>(OnAuthenticationServiceInitialized);
            m_CloudCodeServiceObserver = InitializeServiceObserver<ICloudCodeService>(OnCloudCodeServiceInitialized);
            m_CloudSaveServiceObserver = InitializeServiceObserver<ICloudSaveService>(OnCloudSaveServiceInitialized);
            m_RemoteConfigService = RemoteConfigService.Instance;

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// This exists to fix an issue of entering play mode when the UI Builder is open.
        /// </summary>
        /// <param name="state"></param>
        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                s_Instance.Dispose();
                s_Instance = null;
            }
        }
#endif

        /// <summary>
        /// Unlock an achievement by ID
        /// </summary>
        /// <param name="achievementId">The ID of the achievement to unlock</param>
        public async Task UnlockAchievementAsync(string achievementId)
        {
            var achievement = RuntimeAchievementData.Achievements.FirstOrDefault(ach => ach.Id == achievementId);
            if (achievement == null)
            {
                throw new ArgumentOutOfRangeException(nameof(achievementId), $"Could not find achievement with id: {achievementId}");
            }

            await UnlockAchievementAsync(achievement);
        }

        /// <summary>
        /// Unlock an achievement
        /// </summary>
        /// <param name="achievement">The achievement to unlock</param>
        public async Task UnlockAchievementAsync(Achievement achievement)
        {
            if (await m_AchievementService.UnlockAchievementAsync(achievement))
            {
                AchievementUnlocked?.Invoke(achievement);
            }
        }

        /// <summary>
        /// Reset all achievements
        /// </summary>
        public async Task ResetAchievementsAsync()
        {
            await m_AchievementService.ResetAllAchievementsAsync();
            await GetAchievementsAsync();
        }

        /// <summary>
        /// Update the progress for an achievement
        /// </summary>
        /// <param name="achievement">achievement to update</param>
        /// <param name="count">value to set the progress</param>
        public async Task UpdateAchievementProgressAsync(Achievement achievement, int count)
        {
            await m_AchievementService.UpdateAchievementProgressAsync(achievement, count);
        }

        static ServiceObserver<T> InitializeServiceObserver<T>(Action<T> onInitialized)
        {
            var serviceObserver = new ServiceObserver<T>();
            if (serviceObserver.Service == null)
            {
                var observer = serviceObserver;
                Action<T> initAndDeregister = null;
                initAndDeregister = _ =>
                {
                    onInitialized(observer.Service);
                    observer.Initialized -= initAndDeregister;
                };
                serviceObserver.Initialized += initAndDeregister;
            }
            else
            {
                onInitialized(serviceObserver.Service);
            }

            return serviceObserver;
        }

        static void CleanupServiceObserver<T>(ref ServiceObserver<T> serviceObserver)
        {
            if (serviceObserver != null)
            {
                serviceObserver.Dispose();
                serviceObserver = null;
            }
        }

        void OnAuthenticationServiceInitialized(IAuthenticationService authenticationService)
        {
            m_AuthenticationService = authenticationService;
            CleanupServiceObserver(ref m_AuthenticationServiceObserver);

            if (IsSignedIn())
            {
                OnSignedIn();
            }
            else
            {
                Action signedIn = null;
                signedIn = () =>
                {
                    OnSignedIn();
                    m_AuthenticationService.SignedIn -= signedIn;
                };
                m_AuthenticationService.SignedIn += signedIn;
            }
        }

        void OnCloudCodeServiceInitialized(ICloudCodeService cloudCodeService)
        {
            m_CloudCodeService = cloudCodeService;
            CleanupServiceObserver(ref m_CloudCodeServiceObserver);
        }

        void OnCloudSaveServiceInitialized(ICloudSaveService cloudSaveService)
        {
            m_CloudSaveService = cloudSaveService;
            CleanupServiceObserver(ref m_CloudSaveServiceObserver);
        }

        bool IsSignedIn()
        {
            return m_RemoteConfigService != null
                && m_CloudCodeService != null
                && m_CloudSaveService != null
                && m_AuthenticationService != null
                && m_AuthenticationService.IsSignedIn;
        }

        void SetAchievementService(bool useTrustedClient)
        {
            m_AchievementService = useTrustedClient
                ? new TrustedAchievementClient(m_AuthenticationService, m_CloudCodeService, m_CloudSaveService, m_RemoteConfigService)
                : new LocalAchievementClient(m_AuthenticationService, m_CloudSaveService, m_RemoteConfigService);
        }

        void OnSignedIn()
        {
            if (IsSignedIn())
            {
                SetAchievementService(m_UseTrustedClient);
                _ = GetAchievementsAsync();
            }
        }

        async Task GetAchievementsAsync()
        {
            var achievements = await m_AchievementService
                .GetAchievementsAsync(m_AuthenticationService.PlayerId);
            RuntimeAchievementData.SetAchievements(achievements);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            CleanupServiceObserver(ref m_AuthenticationServiceObserver);
            CleanupServiceObserver(ref m_CloudCodeServiceObserver);
            CleanupServiceObserver(ref m_CloudSaveServiceObserver);
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        }
    }
}
