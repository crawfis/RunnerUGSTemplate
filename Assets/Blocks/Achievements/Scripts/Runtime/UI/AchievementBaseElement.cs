using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Achievements.UI
{
    /// <summary>
    /// The base for the achievement cards in the achievements container and the achievement notification
    /// </summary>
    public class AchievementBaseElement : VisualElement
    {
        const string k_HiddenIcon = "eye-crossed";
        const string k_NotFoundIcon = "thumbnail_black";

        protected Achievement Achievement { get; private set; }
        public static List<Texture2D> Icons;

        protected Label m_TitleLabel;
        VisualElement m_Icon;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AchievementBaseElement()
        {
            AddToClassList(AchievementsTheme.AchievementsBase);

            m_TitleLabel = new Label("Title");
            m_TitleLabel.AddToClassList(AchievementsTheme.AchievementsCardTitle);
            Add(m_TitleLabel);

            m_Icon = new VisualElement();
            m_Icon.AddToClassList(AchievementsTheme.AchievementsCardThumbnail);
            Add(m_Icon);
        }

        /// <summary>
        /// Bind the control to an achievement
        /// </summary>
        /// <param name="achievement"></param>
        public virtual void Bind(Achievement achievement)
        {
            Achievement = achievement;
            if (achievement.Record != null)
            {
                Achievement.Record.Changed += UpdateGUI;
            }

            UpdateGUI();
        }

        /// <summary>
        /// Initialize non-UI related information
        /// </summary>
        public virtual void Initialize()
        {
            style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Unbind the control from the achievement
        /// </summary>
        public virtual void Unbind()
        {
            if (Achievement != null)
            {
                Achievement.Record.Changed -= UpdateGUI;
                Achievement = null;
            }
            style.display = DisplayStyle.None;
        }

        static Texture2D GetIcon(string name)
        {
            try
            {
                var icon = Icons.Find(texture2D =>
                        texture2D.name == name)
                    ?? Icons.Find(texture2D =>
                        texture2D.name == k_NotFoundIcon);
                return icon;
            }
            catch (Exception)
            {
                Debug.LogError($"Could not load icon {name}");
            }

            return null;
        }

        protected virtual void UpdateGUI()
        {
            if (Achievement.Record == null)
            {
                return;
            }

            m_TitleLabel.text = Achievement.Definition.Title;

            m_Icon.style.backgroundImage = GetIcon(
                Achievement.Definition.IsHidden && !Achievement.Record.Unlocked
                    ? k_HiddenIcon
                    : Achievement.Definition.Icon);
        }

    }
}
