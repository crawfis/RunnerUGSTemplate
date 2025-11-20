using Blocks.Common;
using Unity.Services.Leaderboards.Models;
using UnityEngine.UIElements;

namespace Blocks.Leaderboards
{
    /// <summary>
    /// This is the UI control for the a single leaderboard.
    /// </summary>
    [UxmlElement]
    partial class LeaderboardView : VisualElement
    {
        ListView m_ListView;
        LeaderboardData m_Data;

        /// <summary>
        /// Default ctor
        /// </summary>
        public LeaderboardView()
        {
            Initialize(new LeaderboardData());
        }

        /// <summary>
        /// Ctor with leaderboard data
        /// </summary>
        /// <param name="data">Leaderboard data</param>
        public LeaderboardView(LeaderboardData data)
        {
            Initialize(data);
        }

        void Initialize(LeaderboardData data)
        {
            m_Data = data;
            m_Data.propertyChanged += OnDataOnPropertyChanged;

            m_ListView = new ListView
            {
                makeItem = CreateListElement,
                bindItem = BindListElement,
                fixedItemHeight = 32
            };
            m_ListView.AddToClassList(BlocksTheme.ScrollView);

            Add(m_ListView);

            UpdateScores();
        }

        void OnDataOnPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            if (e.propertyName == nameof(LeaderboardData.Scores))
            {
                UpdateScores();
            }
        }

        void UpdateScores()
        {
            m_ListView.itemsSource = m_Data.Scores;
            m_ListView.RefreshItems();

            if (m_Data.Scores.Count == 0)
            {
                var emptyListLabel = m_ListView.Q<Label>();
                emptyListLabel.AddToClassList(BlocksTheme.Label);
                emptyListLabel.text = "No leaderboard entries to display.";
            }
        }

        static VisualElement CreateListElement()
        {
            var listElement = new LeaderboardEntryElement();
            return listElement;
        }

        void BindListElement(VisualElement element, int index)
        {
            if (element is LeaderboardEntryElement leaderboardEntryElement)
            {
                leaderboardEntryElement.Bind((LeaderboardEntry)m_ListView.itemsSource[index]);
            }
        }
    }
}
