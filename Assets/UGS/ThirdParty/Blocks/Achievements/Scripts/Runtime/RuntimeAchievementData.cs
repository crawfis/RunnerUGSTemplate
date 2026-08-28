using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Achievements
{
    /// <summary>
    /// The model which contains all the data used by the achievements UI controls
    /// </summary>
    public class RuntimeAchievementData : IDisposable, IDataSourceViewHashProvider, INotifyBindablePropertyChanged
    {
        [SerializeField, DontCreateProperty]
        List<Achievement> m_Achievements = new List<Achievement>();

        /// <summary>
        /// The list of achievements
        /// </summary>
        [CreateProperty]
        public List<Achievement> Achievements
        {
            get => m_Achievements;
            private set
            {
                if (m_Achievements != value)
                {
                    m_Achievements = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <inheritdoc/>
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        /// <summary>
        /// Default ctor
        /// </summary>
        public RuntimeAchievementData()
        {
            Achievements = new List<Achievement>();
        }

        /// <summary>
        /// Set the list of achievements
        /// </summary>
        /// <param name="achievements">The list of achievements to set</param>
        public void SetAchievements(List<Achievement> achievements)
        {
            Achievements = achievements;
        }

        /// <summary>
        /// Fills the list of achievements with fake data
        /// Used when viewing the control in UI Builder
        /// </summary>
        public void SetDummyData()
        {
            var achievements = new List<Achievement>();
            for (var i = 0; i < 10; ++i)
            {
                achievements.Add(
                    new Achievement(
                        new AchievementDefinition()
                        {
                            Description = "Achievement description",
                            Id = $"Achievement {i + 1}",
                            IsHidden = false,
                            Title = $"Achievement Title {i + 1}"
                        }));
            }

            Achievements = achievements;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            propertyChanged = null;
        }

        /// <inheritdoc/>
        public long GetViewHashCode()
        {
            return HashCode.Combine(Achievements);
        }

        void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(propertyName));
        }
    }
}
