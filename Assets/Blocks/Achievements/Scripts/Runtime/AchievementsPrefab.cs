using System.Linq;
using Blocks.Achievements.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Achievements
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
