using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Leaderboards.TestScene
{
    public class ViewToggle : MonoBehaviour
    {
        const string k_Show = "Show Leaderboard";
        const string k_Hide = "Hide Leaderboard";

        [SerializeField]
        UIDocument m_DocumentToggle;

        [SerializeField]
        UIDocument m_DocumentLeaderboard;

        [SerializeField]
        UIDocument m_DocumentGame;

        Button m_ToggleButton;
        bool m_IsLeaderboardActive;

        void Awake()
        {
            m_ToggleButton = m_DocumentToggle.rootVisualElement.Q<Button>();
            m_ToggleButton.clickable.clicked += OnToggleButtonClicked;

            m_IsLeaderboardActive = false;

            ToggleUI(m_IsLeaderboardActive);
        }

        void OnToggleButtonClicked()
        {
            m_IsLeaderboardActive = !m_IsLeaderboardActive;
            ToggleUI(m_IsLeaderboardActive);
        }

        void ToggleUI(bool isLeaderboardActive)
        {
            m_ToggleButton.text = isLeaderboardActive ? k_Hide : k_Show;
            m_DocumentLeaderboard.rootVisualElement.style.display =
                isLeaderboardActive
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

            m_DocumentGame.rootVisualElement.style.display =
                isLeaderboardActive
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;

        }
    }
}
