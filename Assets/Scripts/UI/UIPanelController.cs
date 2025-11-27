using CrawfisSoftware.Events;
using CrawfisSoftware.GameConfig;
using CrawfisSoftware.TempleRun;

using System.Collections;

using UnityEngine;
using UnityEngine.UIElements;

public enum UIState { None, Menu, Countdown, Gameplay, GameOverOverlay, Feedback, Loading }

public class UIPanelController : MonoBehaviour
{
    [Header("UIDocuments (drag from scene)")]
    //public UIDocument menuUI;
    public UIDocument hudUI;
    public UIDocument countDownUI;
    public UIDocument gameOverUI;
    public UIDocument loadingUI;

    // Todo: Remove login logic from here later
    private bool _isSignedIn = true;

    void Awake()
    {
        // Optional: warm root VE references
        //if (menuUI) menuUI.rootVisualElement.visible = false;
        if (hudUI) hudUI.rootVisualElement.visible = false;
        if (countDownUI) countDownUI.rootVisualElement.visible = false;
        if (gameOverUI) gameOverUI.rootVisualElement.visible = false;

        Go(UIState.Loading);

        //if (UnityServices.State == ServicesInitializationState.Initialized)
        //{
        //    StartCoroutine(ShowLoadingRoutine(GameConstants.DefaultLoadingDisplayTime));
        //}
        //else
        //{
        //    StartCoroutine(ShowLoadingRoutine(GameConstants.DefaultLoadingDisplayTime));
        //    EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.UnityServicesInitialized, OnUnityInitialized);
        //}
        //EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.PlayerAuthenticated, OnPlayerAuthenticated);
        //EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.PlayerSignedOut, OnPlayerSignOut);
        //EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.AchievementsClosed, OnFeedbackClosed);

        EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameStarting, OnGameStarting);
        EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameStarted, OnGameStarted);
        EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameEnding, OnGameEnding);
        EventsPublisherGameFlow.Instance.SubscribeToEvent(GameFlowEvents.GameEnding, OnGameEnded);
        // Todo: Wait for an event that the menu is ready (e.g., remote config loaded, addressables, etc.) before showing it
        StartCoroutine(ShowLoadingRoutine(GameConstants.DefaultLoadingDisplayTime));

    }

    //private void OnFeedbackClosed(string arg1, object arg2, object arg3)
    //{
    //    Go(UIState.Menu);
    //}

    //private void OnPlayerSignOut(string arg1, object arg2, object arg3)
    //{
    //    _isSignedIn = false;
    //    Go(UIState.None);
    //}

    private void OnDestroy()
    {
        //EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.UnityServicesInitialized, OnUnityInitialized);
        //EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.PlayerAuthenticated, OnPlayerAuthenticated);
        //EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.PlayerSignedOut, OnPlayerSignOut);
        //EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.AchievementsClosed, OnFeedbackClosed);

        EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameStarting, OnGameStarting);
        EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameStarted, OnGameStarted);
        EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameEnding, OnGameEnding);
        EventsPublisherGameFlow.Instance.UnsubscribeToEvent(GameFlowEvents.GameEnding, OnGameEnded);
    }

    //private void OnPlayerAuthenticated(string eventName, object sender, object data)
    //{
    //    _isSignedIn = true;
    //    StartCoroutine(ShowMenuDelayed(1f));
    //}

    private IEnumerator ShowMenuDelayed(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Go(UIState.Menu);
    }

    private void OnUnityInitialized(string eventName, object sender, object data)
    {
    }

    private void OnGameStarting(string eventName, object sender, object data)
    {
        Go(UIState.Gameplay);
        if (hudUI) hudUI.rootVisualElement.visible = true;
        SetActive(countDownUI, true);
        ShowCountdown(GameConstants.CountdownSeconds);
    }

    private void OnGameStarted(string eventName, object sender, object data)
    {
        Go(UIState.Gameplay);
        if (hudUI) hudUI.rootVisualElement.visible = true;
    }

    private void OnGameEnding(string eventName, object sender, object data)
    {
        Go(UIState.GameOverOverlay);
    }

    private void OnGameEnded(string eventName, object sender, object data)
    {
        //Go(UIState.Feedback);
        Go(UIState.Menu);
    }

    private IEnumerator ShowLoadingRoutine(float seconds)
    {
        //EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.LoadingScreenShowing, this, null);
        EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.LoadingScreenShown, this, null);
        yield return new WaitForSecondsRealtime(seconds);
        if(_isSignedIn)
            Go(UIState.Menu);
        else 
            Go(UIState.None);
        EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.LoadingScreenHidden, this, null);
    }

    void SetActive(UIDocument doc, bool on)
    {
        if (!doc) return;
        doc.gameObject.SetActive(on);
        if (doc.rootVisualElement != null)
            doc.rootVisualElement.visible = on;
    }

    public void Go(UIState s)
    {
        //SetActive(menuUI, s == UIState.Menu);
        SetActive(hudUI, s == UIState.Gameplay);
        SetActive(countDownUI, s == UIState.Countdown);
        SetActive(gameOverUI, s == UIState.GameOverOverlay);
        SetActive(loadingUI, s == UIState.Loading);
    }

    public void ShowCountdown(float seconds = 3f)
    {
        if (!countDownUI ) { Debug.LogWarning("Countdown UXML not set"); return; }

        SetActive(countDownUI, true);
        StopAllCoroutines();
        StartCoroutine(CountdownRoutine(seconds));
    }

    System.Collections.IEnumerator CountdownRoutine(float seconds)
    {
        float t = seconds;
        var label = countDownUI.rootVisualElement.Q<Label>("Countdown");
        while (t > 0f)
        {
            if (label != null) label.text = Mathf.CeilToInt(t).ToString();
            yield return null;
            t -= Time.deltaTime;
        }
        SetActive(countDownUI, false);
    }

    public void ShowGameOver()
    {
        if (!gameOverUI) { Debug.LogWarning("GameOver UXML not set"); return; }
        SetActive(gameOverUI, true);
    }
}