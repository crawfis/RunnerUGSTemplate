using Blocks.Achievements.UI;

using CrawfisSoftware.UGS.Events;

using System.Linq;

using UnityEngine;
using UnityEngine.UIElements;

namespace CrawfisSoftware.UGS.Achievements
{
    /// <summary>
    /// Monobehaviour script allowing drag & drop of the AchievementsContainer in a scene
    /// </summary>
    public class AchievementsPrefab : MonoBehaviour
    {
        [SerializeField]
        bool InitOnAwake = true;
        [SerializeField]
        bool DevelopmentMode = true;
        [SerializeField]
        bool UseTrustedClient;
        [SerializeField]
        Texture2D[] m_Icons;
        [SerializeField]
        UIDocument m_UiDocument;

        public AchievementsContainer AchievementsContainer { get; private set; }

        void Awake()
        {
            if (InitOnAwake)
            {
                Initialize(UseTrustedClient);
            }
            m_UiDocument.rootVisualElement.style.display = DisplayStyle.None;
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.AchievementsOpening, OnAchievementsOpening);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.AchievementsClosing, OnAchievementsClosing);
        }
        private void OnDestroy()
        {
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.AchievementsOpening, OnAchievementsOpening);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.AchievementsClosing, OnAchievementsClosing);
        }
        private void OnAchievementsOpening(string eventName, object sender, object data)
        {
            m_UiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            //this.gameObject.SetActive(true);
        }

        private void OnAchievementsClosing(string eventName, object sender, object data)
        {
            m_UiDocument.rootVisualElement.style.display = DisplayStyle.None;
            //this.gameObject.SetActive(false);
            EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.AchievementsClosed, this, null);
        }

        /// <summary>
        /// Initialize the prefab using the information set on this prefab instance
        /// </summary>
        public void Initialize()
        {
            Initialize(UseTrustedClient);
        }

        /// <summary>
        /// Initialize the prefab with client choice and potential different root UI
        /// </summary>
        /// <param name="useTrustedClient">Use local client or cloud code module</param>
        /// <param name="rootElement">UI element to parent to</param>
        public void Initialize(bool useTrustedClient, VisualElement rootElement = null)
        {
            AchievementBaseElement.Icons = m_Icons.ToList();
            AchievementsContainer = new AchievementsContainer(useTrustedClient, DevelopmentMode);
            var parent = rootElement ?? m_UiDocument.rootVisualElement;
            parent.Add(AchievementsContainer);
        }
    }
}