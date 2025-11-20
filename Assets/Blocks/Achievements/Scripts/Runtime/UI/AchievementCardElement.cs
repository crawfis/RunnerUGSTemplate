using System;
using System.Threading.Tasks;
using Blocks.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Achievements.UI
{
    /// <summary>
    /// The UI control for an individual achievement in the AchievementContainer
    /// </summary>
    [UxmlElement]
    public partial class AchievementCardElement : AchievementBaseElement
    {
        const string k_HiddenTitle = "[hidden]";
        const string k_HiddenDescription = "Unlock this achievement to see its description.";

        Label m_DescriptionLabel;
        Label m_UnlockedLabel;
        Button m_UnlockBtn;
        AchievementProgressBar m_ProgressBar;

        bool m_IsDevelopmentMode;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AchievementCardElement()
        {
            AddToClassList(AchievementsTheme.AchievementsCard);

            m_DescriptionLabel = new Label("Description");
            m_DescriptionLabel.AddToClassList(AchievementsTheme.AchievementsCardDescription);
            Add(m_DescriptionLabel);

            m_ProgressBar = new AchievementProgressBar();
            Add(m_ProgressBar);

            m_UnlockBtn = new Button();
            m_UnlockBtn.text = "CLAIM";
            m_UnlockBtn.AddToClassList(BlocksTheme.Button);
            m_UnlockBtn.AddToClassList(BlocksTheme.ButtonSmall);
            m_UnlockBtn.clicked += OnUnlock;
            Add(m_UnlockBtn);

            m_UnlockedLabel =  new Label("UNLOCKED");
            m_UnlockedLabel.AddToClassList(AchievementsTheme.AchievementsCardUnlockedLabel);
            Add(m_UnlockedLabel);
        }

        /// <inheritdoc/>
        public override void Bind(Achievement achievement)
        {
            m_ProgressBar.Bind(achievement);
            base.Bind(achievement);
        }

        /// <summary>
        /// Initialize non-UI features
        /// </summary>
        /// <param name="isDevelopmentMode">Should this show development features</param>
        public void Initialize(bool isDevelopmentMode = false)
        {
            base.Initialize();
            m_ProgressBar.Initialize(isDevelopmentMode, OnAchievementProgressCompleted);
            m_IsDevelopmentMode = isDevelopmentMode;
        }

        void OnUnlock()
        {
            _ = UnlockAchievement_Async();
        }

        /// <inheritdoc/>
        public override void Unbind()
        {
            base.Unbind();
            m_ProgressBar.Unbind();
        }

        async Task UnlockAchievement_Async()
        {
            await AchievementsObserver.Instance.UnlockAchievementAsync(Achievement);
            UpdateGUI();
        }

        protected override void UpdateGUI()
        {
            base.UpdateGUI();

            if (Achievement?.Record != null)
            {
                m_DescriptionLabel.text = Achievement.Definition.Description;

                m_UnlockBtn.style.display = m_IsDevelopmentMode && !Achievement.Record.Unlocked && !m_ProgressBar.IsActive
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
                m_UnlockedLabel.style.display = Achievement.Record.Unlocked
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

                if (Achievement.Definition.IsHidden && !Achievement.Record.Unlocked)
                {
                    m_TitleLabel.text = k_HiddenTitle;
                    m_DescriptionLabel.text = k_HiddenDescription;
                }
                else
                {
                    m_TitleLabel.text = Achievement.Definition.Title;
                    m_DescriptionLabel.text = Achievement.Definition.Description;
                }
            }

            m_ProgressBar.UpdateGUI();
        }

        void OnAchievementProgressCompleted()
        {
            _ = UnlockAchievement_Async();
        }
    }
}
