using System;
using System.Threading.Tasks;
using Blocks.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Achievements.UI
{
    /// <summary>
    /// The UI control displaying the progress of a multi-staged achievement
    /// </summary>
    [UxmlElement]
    partial class AchievementProgressBar : VisualElement
    {
        VisualElement m_TopContainer;
        Button m_ButtonMinus;
        Button m_ButtonPlus;
        Label m_ProgressLabel;
        ProgressBar m_ProgressBar;

        bool m_IsDevelopmentMode;
        Achievement m_Achievement;

        Action m_ProgressCompleted;

        /// <summary>
        /// The state of the progress bar.
        /// Depends on the bound achievements record and definition
        /// </summary>
        public bool IsActive
        {
            get
            {
                if (m_Achievement?.Record == null || m_Achievement?.Definition == null)
                {
                    return false;
                }

                return !m_Achievement.Record.Unlocked && m_Achievement.Definition.ProgressTarget > 1;
            }
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public AchievementProgressBar()
        {
            AddToClassList(AchievementsTheme.AchievementsProgressBar);

            m_TopContainer = new VisualElement();
            m_TopContainer.AddToClassList(AchievementsTheme.AchievementsProgressBarTopContainer);
            Add(m_TopContainer);

            m_ButtonMinus = AddButton("-", OnButtonMinusClicked);
            m_TopContainer.Add(m_ButtonMinus);

            m_ProgressLabel = new Label();
            m_ProgressLabel.text = " 0 / 0";
            m_ProgressLabel.AddToClassList(BlocksTheme.Label);
            m_TopContainer.Add(m_ProgressLabel);

            m_ButtonPlus = AddButton("+", OnButtonPlusClicked);
            m_TopContainer.Add(m_ButtonPlus);

            m_ProgressBar = new ProgressBar();
            m_ProgressBar.AddToClassList(BlocksTheme.ProgressBar);
            Add(m_ProgressBar);
        }

        static Button AddButton(string label, Action onClick)
        {
            var button = new Button();
            button.text = label;
            button.clicked += onClick;
            button.AddToClassList(BlocksTheme.ButtonExtraSmall);
            button.AddToClassList(BlocksTheme.Button);

            return button;
        }

        /// <summary>
        /// Initialize the progress bar
        /// </summary>
        /// <param name="isDevelopmentMode">Toggle the increase/decrease progress controls</param>
        /// <param name="progressCompleted">The action to perform when progress is completed</param>
        public void Initialize(bool isDevelopmentMode, Action progressCompleted)
        {
            m_IsDevelopmentMode = isDevelopmentMode;
            m_ProgressCompleted = progressCompleted;
        }

        /// <summary>
        /// Bind the control to an achievement
        /// </summary>
        /// <param name="achievement">The achievement to bind</param>
        public void Bind(Achievement achievement)
        {
            m_Achievement = achievement;
            UpdateGUI();
        }

        /// <summary>
        /// Unbind the control from its achievement
        /// </summary>
        public void Unbind()
        {
            m_Achievement = null;
        }

        /// <summary>
        /// Update the UI
        /// </summary>
        public void UpdateGUI()
        {
            if (!m_IsDevelopmentMode)
            {
                m_ButtonMinus.style.display = DisplayStyle.None;
                m_ButtonPlus.style.display = DisplayStyle.None;
            }

            if (m_Achievement?.Record == null
                || m_Achievement?.Definition == null)
            {
                return;
            }

            style.display = IsActive
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            m_ProgressLabel.text = $"{m_Achievement.Record.ProgressCount} / {m_Achievement.Definition.ProgressTarget}";
            m_ProgressBar.value = m_Achievement.Record.ProgressCount;
            m_ProgressBar.highValue = m_Achievement.Definition.ProgressTarget;
        }

        void OnButtonMinusClicked()
        {
            if (m_Achievement.Record.ProgressCount <= 0)
            {
                Debug.LogWarning("Cannot set progress for an achievement below 0.");
                return;
            }

            var count = m_Achievement.Record.ProgressCount - 1;
            _ = UpdateProgressAsync(count);
        }

        void OnButtonPlusClicked()
        {
            var count = m_Achievement.Record.ProgressCount + 1;

            if (count >= m_Achievement.Definition.ProgressTarget)
            {
                m_ButtonMinus.SetEnabled(false);
                m_ButtonPlus.SetEnabled(false);
                m_ProgressCompleted?.Invoke();
                m_ButtonMinus.SetEnabled(true);
                m_ButtonPlus.SetEnabled(true);
            }
            else
            {
                _ = UpdateProgressAsync(count);
            }
        }

        async Task UpdateProgressAsync(int count)
        {
            m_ButtonMinus.SetEnabled(false);
            m_ButtonPlus.SetEnabled(false);
            await AchievementsObserver.Instance.UpdateAchievementProgressAsync(m_Achievement, count);
            m_ButtonMinus.SetEnabled(true);
            m_ButtonPlus.SetEnabled(true);

            UpdateGUI();
        }
    }
}
