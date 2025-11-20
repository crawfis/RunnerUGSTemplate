using System.Linq;
using Blocks.Achievements.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Achievements
{
    /// <summary>
    /// The monobehaviour allowing drag & drop behaviour for the AchievementNotificationElement
    /// </summary>
    public class AchievementsNotificationPrefab : MonoBehaviour
    {
        [SerializeField]
        bool InitOnAwake = true;
        [SerializeField]
        Texture2D[] m_Icons;
        [SerializeField]
        UIDocument m_UiDocument;

        /// <summary>
        /// The UI control for the notification
        /// </summary>
        public AchievementNotificationElement AchievementsNotification;

        void Awake()
        {
            if (InitOnAwake)
            {
                Init();
            }
        }

        /// <summary>
        /// Initialize the prefab
        /// </summary>
        /// <param name="rootElement"></param>
        public void Init(VisualElement rootElement = null)
        {
            AchievementBaseElement.Icons = m_Icons.ToList();
            AchievementsNotification = new AchievementNotificationElement();
            var parent = rootElement ?? m_UiDocument.rootVisualElement;
            parent.Add(AchievementsNotification);
        }
    }
}
