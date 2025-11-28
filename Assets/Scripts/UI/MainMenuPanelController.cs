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
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameScenesLoading, (_, _, _) => HidePanel());
            EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.MainMenuShowing, (_, _, _) => ShowPanel());
        }
        private void OnDestroy()
        {
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameScenesLoading, (_, _, _) => HidePanel());
            EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.MainMenuShowing, (_, _, _) => ShowPanel());
        }

        private void ShowPanel()
        {
            menuUI.rootVisualElement.visible = true;
            EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.MainMenuShown, this, null);
        }

        private void HidePanel()
        {
            menuUI.rootVisualElement.visible = false;
            EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.MainMenuHidden, this, null);
        }
    }
}