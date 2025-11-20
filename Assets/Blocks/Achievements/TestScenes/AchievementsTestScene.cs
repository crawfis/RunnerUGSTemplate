using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Achievements.TestScene
{
    /// <summary>
    /// Test scene for the achievements Starter Kit
    /// </summary>
    public class AchievementsTestScene : MonoBehaviour
    {
        [SerializeField]
        UIDocument m_UiDocument;
        [SerializeField]
        bool m_UseTrustedClient;
        [SerializeField]
        AchievementsPrefab m_AchievementsPrefab;

        Button m_ResetButtonUI;

        const string k_ContentContainer = "ContentContainer";
        const string k_ResetButtonId = "Reset";

        // loading animation
        int m_MaxDotCount = 6;
        int m_CurrentDotCount = 0;
        bool m_Loading = true;
        float m_LastTextUpdate = -1;
        const float k_TextTransitionInterval = 0.5f;
        const string k_BaseLoadingText = "Loading";
        const string k_LoadingLabelId = "Loading";
        Label m_LoadingLabelUI;


        void Start()
        {
            InitUI();
            InitLoading();
            InitReset();
        }

        void InitLoading()
        {
            m_LastTextUpdate = Time.time;
            m_LoadingLabelUI = m_UiDocument.rootVisualElement.Q<Label>(k_LoadingLabelId);
        }

        void InitReset()
        {
            m_ResetButtonUI = m_UiDocument.rootVisualElement.Q<Button>(k_ResetButtonId);
            m_ResetButtonUI.clicked += async () => await OnReset();
            m_ResetButtonUI.style.display = DisplayStyle.None;
        }

        void InitUI()
        {
            var contentContainer = m_UiDocument.rootVisualElement.Q<VisualElement>(k_ContentContainer);
            m_AchievementsPrefab.Initialize(m_UseTrustedClient, contentContainer);
            AchievementsObserver.Instance.RuntimeAchievementData.propertyChanged += OnAchievementsPropertyChanged;
            m_AchievementsPrefab.AchievementsContainer.style.display = DisplayStyle.None;
        }

        void OnAchievementsPropertyChanged(object sender, BindablePropertyChangedEventArgs args)
        {
            if (m_Loading
                && args.propertyName == nameof(AchievementsObserver.RuntimeAchievementData.Achievements)
                && AchievementsObserver.Instance.RuntimeAchievementData.Achievements.Count > 0)
            {
                AchievementsObserver.Instance.RuntimeAchievementData.propertyChanged -= OnAchievementsPropertyChanged;
                m_LoadingLabelUI.style.display = DisplayStyle.None;
                m_AchievementsPrefab.AchievementsContainer.style.display = DisplayStyle.Flex;
                m_ResetButtonUI.style.display = DisplayStyle.Flex;
                m_Loading = false;
            }
        }

        async Task OnReset()
        {
            m_ResetButtonUI.SetEnabled(false);
            try
            {
                await AchievementsObserver.Instance.ResetAchievementsAsync();
            }
            finally
            {
                m_ResetButtonUI.SetEnabled(true);
            }
        }

        void Update()
        {
            if (!m_Loading) return;
            if (Time.time > m_LastTextUpdate + k_TextTransitionInterval)
            {
                m_LastTextUpdate = Time.time;
                m_CurrentDotCount = (m_CurrentDotCount + 1) % m_MaxDotCount;
                m_LoadingLabelUI.text = $"{k_BaseLoadingText}{new string('.', m_CurrentDotCount)}";
            }
        }
    }
}
