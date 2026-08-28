using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Achievements.UI
{
    /// <summary>
    /// The UI control for the achievement notification
    /// </summary>
    [UxmlElement]
    public partial class AchievementNotificationElement : AchievementBaseElement
    {
        NotificationState m_NotificationState;
        Queue<Achievement> m_Queue = new Queue<Achievement>();

        enum NotificationState
        {
            Hidden,
            Showing,
            Shown,
            Hiding
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public AchievementNotificationElement()
        {
            AddToClassList(AchievementsTheme.AchievementsNotification);
            AddToClassList(AchievementsTheme.AchievementsNotificationHidden);
            AddToClassList(AchievementsTheme.AchievementsNotificationTransition);

            m_NotificationState = NotificationState.Hidden;

            AchievementsObserver.Instance.AchievementUnlocked += OnAchievementUnlocked;
            RegisterCallback<TransitionEndEvent>(OnTransitionEnded);
        }

        void OnAchievementUnlocked(Achievement achievement)
        {
            m_Queue.Enqueue(achievement);
            if (m_NotificationState == NotificationState.Hidden)
            {
                ShowNextNotification();
            }
        }

        void ShowNextNotification()
        {
            BindNotification();
            ShowNotification();
        }

        void BindNotification()
        {
            var achievement = m_Queue.Dequeue();
            Bind(achievement);
        }

        void ShowNotification()
        {
            m_NotificationState = NotificationState.Showing;
            RemoveFromClassList(AchievementsTheme.AchievementsNotificationHidden);
        }

        void HideNotification()
        {
            m_NotificationState = NotificationState.Hiding;
            AddToClassList(AchievementsTheme.AchievementsNotificationHidden);
        }

        void OnNotificationShown()
        {
            m_NotificationState = NotificationState.Shown;
            schedule.Execute(HideNotification).StartingIn(2000);
        }

        void OnNotificationHidden()
        {
            m_NotificationState = NotificationState.Hidden;
            if (m_Queue.Count > 0)
            {
                ShowNextNotification();
            }
        }

        void OnTransitionEnded(TransitionEndEvent evt)
        {
            if (m_NotificationState == NotificationState.Showing)
            {
                OnNotificationShown();
            }
            else if (m_NotificationState == NotificationState.Hiding)
            {
                OnNotificationHidden();
            }
        }
    }
}
