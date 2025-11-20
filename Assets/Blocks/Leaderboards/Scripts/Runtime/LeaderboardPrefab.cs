using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Leaderboards
{
    /// <summary>
    /// The monobehaviour controlling the leaderboards UI.
    /// Allows for drag & drop into scene behaviour.
    /// </summary>
    public class LeaderboardPrefab : MonoBehaviour
    {
        [SerializeField]
        bool m_InitOnAwake = true;

        [SerializeField]
        string m_LeaderboardId;

        [SerializeField]
        UIDocument m_UI;

        [SerializeField]
        bool m_UseTrustedClient = false;
        LeaderboardContainer m_LeaderboardContainer;

        /// <summary>
        /// The id of the leaderboard to be used with the leaderboard service API
        /// </summary>
        public string LeaderboardId => m_LeaderboardId;

        /// <summary>
        /// The root element of the leaderboard UI
        /// </summary>
        public LeaderboardContainer LeaderboardContainer => m_LeaderboardContainer;

        void Awake()
        {
            if (m_InitOnAwake)
            {
                Initialize(m_LeaderboardId, m_UseTrustedClient);
            }
        }

        public void Initialize()
        {
            Initialize(m_LeaderboardId, m_UseTrustedClient);
        }

        public void Initialize(bool useTrustedClient)
        {
            Initialize(m_LeaderboardId, useTrustedClient);
        }

        void Initialize(string leaderboardId, bool useTrustedClient)
        {
            if (string.IsNullOrEmpty(leaderboardId))
            {
                Debug.LogWarning("Leaderboard ID is empty when initializing. Default data will be used.");
            }

            m_LeaderboardContainer = new LeaderboardContainer(leaderboardId, useTrustedClient);
            m_UI.rootVisualElement.Add(m_LeaderboardContainer);
        }

        /// <summary>
        /// Update the leaderboard scores
        /// </summary>
        public async Task UpdateScoresAsync()
        {
            await m_LeaderboardContainer.UpdateScoresAsync();
        }
    }
}
