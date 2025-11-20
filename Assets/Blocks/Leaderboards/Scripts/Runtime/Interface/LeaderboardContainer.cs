using System.Threading.Tasks;
using Blocks.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Leaderboards
{
    /// <summary>
    /// Control for the Leaderboard UI
    /// Includes two tabs: Global and Self
    /// </summary>
    [UxmlElement]
    public partial class LeaderboardContainer : VisualElement
    {
        TabView m_TabView;
        Tab m_TabGlobal;
        Tab m_TabSelf;

        [UxmlAttribute]
        string LeaderboardId;

        /// <summary>
        /// Default ctor
        /// </summary>
        public LeaderboardContainer()
        {
            Initialize(false);
        }

        /// <summary>
        /// Ctor with leaderboard id and client choice
        /// </summary>
        /// <param name="leaderboardId">Leaderboard id</param>
        /// <param name="useTrustedClient">Use trusted client or local client</param>
        public LeaderboardContainer(string leaderboardId, bool useTrustedClient)
        {
            LeaderboardId = leaderboardId;
            Initialize(useTrustedClient);
        }

        void Initialize(bool useTrustedClient)
        {
            LeaderboardsObserver.Instance.UseTrustedClient = useTrustedClient;
            LeaderboardsObserver.Instance.LeaderboardId = LeaderboardId;

            m_TabGlobal = CreateTab("Global", "TabGlobal", LeaderboardsObserver.Instance.globalData);
            m_TabSelf = CreateTab("Self", "TabSelf", LeaderboardsObserver.Instance.selfData);

            m_TabView = new TabView();
            m_TabView.Add(m_TabGlobal);
            m_TabView.Add(m_TabSelf);

            m_TabView.AddToClassList(BlocksTheme.Modal);
            m_TabView.activeTab.parent.AddToClassList(LeaderboardsTheme.TabContainer);
            Add(m_TabView);

            // Update with dummy data for UI Builder
            if (!Application.isPlaying)
            {
                _ = UpdateScoresAsync();
            }
        }

        /// <summary>
        /// Update the scores
        /// </summary>
        public async Task UpdateScoresAsync()
        {
            await LeaderboardsObserver.Instance.UpdateScoresAsync();
        }

        static Tab CreateTab(string label, string tabName, LeaderboardData data)
        {
            var tab = new Tab()
            {
                label = label,
                name = tabName
            };

            tab.tabHeader.AddToClassList(LeaderboardsTheme.TabButton);

            var underline = tab.Q(className: Tab.tabHeaderUnderlineUssClassName);
            underline.AddToClassList(LeaderboardsTheme.TabButtonUnderline);

            var leaderboardView = new LeaderboardView(data);
            tab.Add(leaderboardView);

            return tab;
        }
    }
}
