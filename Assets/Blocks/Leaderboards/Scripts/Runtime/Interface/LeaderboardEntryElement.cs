using Blocks.Common;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Leaderboards
{
    /// <summary>
    /// The control for a single leaderboard entry
    /// </summary>
    [UxmlElement]
    partial class LeaderboardEntryElement : VisualElement
    {
        const string k_CurrentUser = "current-user";

        LeaderboardEntry m_Model;

        Label m_LabelRank;
        Label m_LabelName;
        Label m_LabelScore;

        /// <summary>
        /// Default ctor
        /// </summary>
        public LeaderboardEntryElement()
        {
            m_LabelRank = new Label();
            m_LabelRank.AddToClassList(BlocksTheme.Label);
            m_LabelRank.AddToClassList(LeaderboardsTheme.NumberListElementNumber);
            m_LabelName = new Label();
            m_LabelName.AddToClassList(BlocksTheme.Label);
            m_LabelName.AddToClassList(LeaderboardsTheme.NumberListElementText);
            m_LabelScore = new Label();
            m_LabelScore.AddToClassList(BlocksTheme.Label);
            m_LabelScore.AddToClassList(LeaderboardsTheme.NumberListElementRight);

            Add(m_LabelRank);
            Add(m_LabelName);
            Add(m_LabelScore);

            AddToClassList(LeaderboardsTheme.NumberListElement);
        }

        /// <summary>
        /// Bind the entry to the element
        /// </summary>
        /// <param name="entry">The entry to bind</param>
        public void Bind(LeaderboardEntry entry)
        {
            m_Model = entry;
            m_LabelRank.text = $"#{m_Model.Rank + 1}";
            m_LabelName.text = m_Model.PlayerName;
            m_LabelScore.text = $"{m_Model.Score.ToString()} pts";

            RemoveFromClassList(k_CurrentUser);

            // authentication service is not initialized in UI Builder
            if (Application.isPlaying
                && AuthenticationService.Instance.IsSignedIn
                && entry.PlayerName == AuthenticationService.Instance.PlayerName)
            {
                AddToClassList(k_CurrentUser);
            }
        }
    }
}
