using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Properties;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Leaderboards
{
    /// <summary>
    /// The model which contains all the data used by the leaderboards UI controls
    /// </summary>
    public class LeaderboardData : IDisposable, IDataSourceViewHashProvider, INotifyBindablePropertyChanged
    {
        [SerializeField, DontCreateProperty]
        List<LeaderboardEntry> m_Scores = new List<LeaderboardEntry>();

        /// <summary>
        /// The list of LeaderboardEntry
        /// </summary>
        [CreateProperty]
        public List<LeaderboardEntry> Scores
        {
            get => m_Scores;
            private set
            {
                if (m_Scores != value)
                {
                    m_Scores = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <inheritdoc/>
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        /// <summary>
        /// Set the list of scores in the leaderboard
        /// </summary>
        /// <param name="scores">The list of scores to set</param>
        public void SetScores(List<LeaderboardEntry> scores)
        {
            Scores = scores;
        }

        /// <summary>
        /// Set default data in to the scores.
        /// Used for viewing a leaderboard in UI Builder
        /// </summary>
        public void SetDummyData()
        {
            Scores.Clear();
            for (var i = 0; i < 50; i++)
            {
                Scores.Add(new LeaderboardEntry($"Player {i + 1}", $"Player {i + 1}", i, 100 - i * 10));
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            propertyChanged = null;
        }

        /// <inheritdoc/>
        public long GetViewHashCode()
        {
            return HashCode.Combine(Scores);
        }

        void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(propertyName));
        }
    }
}
