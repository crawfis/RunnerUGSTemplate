using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Blocks.Common;

namespace Blocks.Achievements.UI
{
    /// <summary>
    /// The UI control for the view containing all achievements
    /// </summary>
    [UxmlElement]
    public partial class AchievementsContainer : VisualElement
    {
        ScrollView m_ScrollView;
        VisualElement m_GridContainer;
        List<AchievementCardElement> m_AchievementElements;
        bool m_IsDevelopmentMode;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AchievementsContainer()
        {
            Initialize(false);
        }

        /// <summary>
        /// ctor which sets client choice
        /// </summary>
        /// <param name="useTrustedClient">Use local or cloud code module client</param>
        public AchievementsContainer(bool useTrustedClient, bool isDevelopmentMode = false)
        {
            Initialize(useTrustedClient, isDevelopmentMode);
        }

        void Initialize(bool useTrustedClient, bool isDevelopmentMode = false)
        {
            m_AchievementElements = new List<AchievementCardElement>();
            m_IsDevelopmentMode = isDevelopmentMode;

            AchievementsObserver.Instance.UseTrustedClient = useTrustedClient;
            AchievementsObserver.Instance.RuntimeAchievementData.propertyChanged += OnRuntimeAchievementDataPropertyChanged;

            var title = new Label();
            title.text = "ACHIEVEMENTS";
            title.AddToClassList(BlocksTheme.Header);
            title.AddToClassList(BlocksTheme.SpaceBottom);

            m_ScrollView = new ScrollView();
            m_ScrollView.AddToClassList(BlocksTheme.ScrollView);

            m_GridContainer = new VisualElement();
            m_GridContainer.AddToClassList(AchievementsTheme.AchievementsGridView);
            m_ScrollView.Add(m_GridContainer);

            AddToClassList(BlocksTheme.Modal);

            Add(title);
            Add(m_ScrollView);

            UpdateAchievements(AchievementsObserver.Instance.RuntimeAchievementData.Achievements);
        }

        void OnRuntimeAchievementDataPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            if (e.propertyName == nameof(RuntimeAchievementData.Achievements))
            {
                UpdateAchievements(AchievementsObserver.Instance.RuntimeAchievementData.Achievements);
            }
        }

        void UpdateAchievements(List<Achievement> achievements)
        {
            for (var i = 0; i < achievements.Count; ++i)
            {
                if (m_AchievementElements.Count > i)
                {
                    m_AchievementElements[i].Bind(achievements[i]);
                }
                else
                {
                    var achievementElement = new AchievementCardElement();
                    achievementElement.Initialize(m_IsDevelopmentMode);
                    achievementElement.Bind(achievements[i]);
                    m_AchievementElements.Add(achievementElement);
                    m_GridContainer.Add(achievementElement);
                }
            }

            if (m_AchievementElements.Count > achievements.Count)
            {
                for (var i = achievements.Count; i < m_AchievementElements.Count; ++i)
                {
                    m_AchievementElements[i].Unbind();
                }
            }
        }
    }
}
