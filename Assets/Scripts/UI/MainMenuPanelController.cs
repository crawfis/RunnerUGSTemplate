using CrawfisSoftware.Events;

using UnityEngine;
using UnityEngine.UIElements;

namespace CrawfisSoftware.UI
{
    class MainMenuPanelController : MonoBehaviour
    {
        public UIDocument menuUI;
        private void Awake()
        {
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameStarting, (_, _, _) => HidePanel());
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameplayReady, (_, _, _) => ShowPanel());
        }
        private void OnDestroy()
        {
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameStarting, (_, _, _) => HidePanel());
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameplayReady, (_, _, _) => ShowPanel());
        }

        private void ShowPanel()
        {
            menuUI.rootVisualElement.visible = true;
        }

        private void HidePanel()
        {
            menuUI.rootVisualElement.visible = false;
        }
    }
}