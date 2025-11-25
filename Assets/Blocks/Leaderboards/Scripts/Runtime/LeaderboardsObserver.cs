using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Blocks.Leaderboards
{
    /// <summary>
    /// The class handling and connecting the required services to make the leaderboards work.
    /// Is a singleton.
    /// </summary>
    public class LeaderboardsObserver : IDisposable
    {
        IUnityServices m_Registry;
        ServiceObserver<IAuthenticationService> m_AuthenticationServiceObserver;
        ServiceObserver<ILeaderboardsService> m_LeaderboardServiceObserver;
        ServiceObserver<ICloudCodeService> m_CloudCodeServiceObserver;
        ILeaderboardsService m_LeaderboardService;
        IAuthenticationService m_AuthenticationService;
        ICloudCodeService m_CloudCodeService;
        string m_LeaderboardId;
        bool m_UseTrustedClient;
        ILeaderboardsClient m_LeaderboardsClient;

        static LeaderboardsObserver s_Instance;

        /// <summary>
        /// The set of data for the global leaderboard tab
        /// </summary>
        public LeaderboardData globalData { get; }

        /// <summary>
        /// The set of data for the self leaderboard tab
        /// </summary>
        public LeaderboardData selfData { get; }

        /// <summary>
        /// Accessor for the singleton instance
        /// </summary>
        public static LeaderboardsObserver Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new LeaderboardsObserver();
                }

                return s_Instance;
            }
        }

        /// <summary>
        /// Whether to use the local client or the trusted client
        /// </summary>
        public bool UseTrustedClient
        {
            get => m_UseTrustedClient;
            set
            {
                if (m_UseTrustedClient != value)
                {
                    m_UseTrustedClient = value;
                    UpdateScores();
                }
            }
        }

        /// <summary>
        /// The ID of the leaderboard to use with the service
        /// </summary>
        public string LeaderboardId
        {
            get => m_LeaderboardId;
            set
            {
                m_LeaderboardId = value;
                if (IsSignedIn())
                {
                    _ = UpdateScoresAsync();
                }
            }
        }

        LeaderboardsObserver()
        {
            globalData = new LeaderboardData();
            selfData = new LeaderboardData();

            InitializeAuthenticationServiceObserver();
            InitializeLeaderboardServiceObserver();
            InitializeCloudCodeServiceObserver();

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// This addresses an issue when entering play mode and the UI Builder is open.
        /// </summary>
        /// <param name="state"></param>
        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                s_Instance = null;
            }
        }
#endif

        void SetLeaderboardClient(bool useTrustedClient)
        {
            m_LeaderboardsClient = useTrustedClient
                ? new TrustedLeaderboardClient(m_LeaderboardService, m_CloudCodeService)
                : new LocalLeaderboardsClient(m_LeaderboardService);
        }

        bool IsSignedIn()
        {
            return m_LeaderboardService != null
                && m_AuthenticationService != null
                && m_AuthenticationService.IsSignedIn;
        }

        void InitializeAuthenticationServiceObserver()
        {
            m_AuthenticationServiceObserver = new ServiceObserver<IAuthenticationService>();
            if (m_AuthenticationServiceObserver.Service == null)
            {
                m_AuthenticationServiceObserver.Initialized += OnAuthenticationServiceInitialized;
            }
            else
            {
                OnAuthenticationServiceInitialized(m_AuthenticationServiceObserver.Service);
            }
        }

        void OnAuthenticationServiceInitialized(IAuthenticationService authenticationService)
        {
            CleanupAuthenticationServiceObserver();

            m_AuthenticationService = authenticationService;
            //if (IsSignedIn())
            //{
            //    UpdateScores();
            //}
            //else
            //{
            //    m_AuthenticationService.SignedIn += UpdateScores;
            //}
        }

        void UpdateScores()
        {
            if (IsSignedIn())
            {
                SetLeaderboardClient(UseTrustedClient);
                _ = UpdateScoresAsync();
            }
        }

        void InitializeLeaderboardServiceObserver()
        {
            m_LeaderboardServiceObserver = new ServiceObserver<ILeaderboardsService>();
            if (m_LeaderboardServiceObserver.Service == null)
            {
                m_LeaderboardServiceObserver.Initialized += OnLeaderboardServiceInitialized;
            }
            else
            {
                OnLeaderboardServiceInitialized(m_LeaderboardServiceObserver.Service);
            }
        }

        void OnLeaderboardServiceInitialized(ILeaderboardsService service)
        {
            CleanupLeaderboardsServiceObserver();
            m_LeaderboardService = service;
        }

        void InitializeCloudCodeServiceObserver()
        {
            m_CloudCodeServiceObserver = new ServiceObserver<ICloudCodeService>();
            if (m_CloudCodeServiceObserver.Service == null)
            {
                m_CloudCodeServiceObserver.Initialized += OnCloudCodeServiceInitialized;
            }
            else
            {
                OnCloudCodeServiceInitialized(m_CloudCodeServiceObserver.Service);
            }
        }

        void OnCloudCodeServiceInitialized(ICloudCodeService cloudCode)
        {
            CleanupCloudCodeServiceObserver();
            m_CloudCodeService = cloudCode;
        }

        /// <summary>
        /// Add a score to the leaderboard for the player
        /// </summary>
        /// <param name="score"></param>
        public async Task AddPlayerScoreAsync(int score)
        {
            await m_LeaderboardsClient.AddPlayerScoreAsync(LeaderboardId, score);
        }

        /// <summary>
        /// Update the scores of the global and self leaderboards
        /// </summary>
        public async Task UpdateScoresAsync()
        {
            if (string.IsNullOrEmpty(m_LeaderboardId))
            {
                if (Application.isPlaying)
                {
                    Debug.LogWarning("No leaderboard ID specified. Filling entries with placeholder data.");
                }

                globalData.SetDummyData();
                selfData.SetDummyData();
                return;
            }

            var globalScores = await GetScores(50, true);
            var playerScores = await GetScores(10, false);

            globalData.SetScores(globalScores);
            selfData.SetScores(playerScores);
        }

        async Task<List<LeaderboardEntry>> GetScores(int limit, bool global)
        {
            if (m_LeaderboardService == null)
            {
                Debug.LogError("Leaderboards service not initialized");
                return new List<LeaderboardEntry>();
            }

            return await m_LeaderboardsClient.GetScoresAsync(m_LeaderboardId, limit, global);
        }

        void CleanupAuthenticationServiceObserver()
        {
            m_AuthenticationServiceObserver.Initialized -= OnAuthenticationServiceInitialized;
            m_AuthenticationServiceObserver.Dispose();
            m_AuthenticationServiceObserver = null;
        }

        void CleanupLeaderboardsServiceObserver()
        {
            m_LeaderboardServiceObserver.Initialized -= OnLeaderboardServiceInitialized;
            m_LeaderboardServiceObserver.Dispose();
            m_LeaderboardServiceObserver = null;
        }

        void CleanupCloudCodeServiceObserver()
        {
            m_CloudCodeServiceObserver.Initialized -= OnCloudCodeServiceInitialized;
            m_CloudCodeServiceObserver.Dispose();
            m_CloudCodeServiceObserver = null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (m_LeaderboardServiceObserver != null)
            {
                CleanupLeaderboardsServiceObserver();
            }
        }
    }
}
