using CrawfisSoftware.TempleRun;
using CrawfisSoftware.GameConfig;
using CrawfisSoftware.UGS;

using System;
using System.Collections;

using Unity.Services.Core;

using UnityEngine;
using UnityEngine.UIElements;

public enum UIState { None, Menu, Countdown, Gameplay, GameOverOverlay, Feedback, Loading }

public class UIPanelController : MonoBehaviour
{
    [Header("UIDocuments (drag from scene)")]
    public UIDocument menuUI;
    public UIDocument hudUI;
    public UIDocument countDownUI;
    public UIDocument gameOverUI;
    public UIDocument loadingUI;

    // Todo: Remove login logic from here later
    private bool _isSignedIn = true;

    void Awake()
    {
        // Optional: warm root VE references
        if (menuUI) menuUI.rootVisualElement.visible = false;
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

        EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.GameStarting, OnGameStarting);
        EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.GameStarted, OnGameStarted);
        EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.GameEnding, OnGameEnding);
        EventsPublisherTempleRun.Instance.SubscribeToEvent(GamePlayEvents.GameEnding, OnGameEnded);
        // Todo: Wait for an event that the menu is ready (e.g., remote config loaded, addressables, etc.) before showing it
        StartCoroutine(ShowLoadingRoutine(GameConstants.DefaultLoadingDisplayTime));

    }

    private void OnFeedbackClosed(string arg1, object arg2, object arg3)
    {
        Go(UIState.Menu);
    }

    private void OnPlayerSignOut(string arg1, object arg2, object arg3)
    {
        _isSignedIn = false;
        Go(UIState.None);
    }

    private void OnDestroy()
    {
        //EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.UnityServicesInitialized, OnUnityInitialized);
        //EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.PlayerAuthenticated, OnPlayerAuthenticated);
        //EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.PlayerSignedOut, OnPlayerSignOut);
        //EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.AchievementsClosed, OnFeedbackClosed);

        EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.GameStarting, OnGameStarting);
        EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.GameStarted, OnGameStarted);
        EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.GameEnding, OnGameEnding);
        EventsPublisherTempleRun.Instance.UnsubscribeToEvent(GamePlayEvents.GameEnded, OnGameEnded);

    }

    private void OnPlayerAuthenticated(string eventName, object sender, object data)
    {
        _isSignedIn = true;
        StartCoroutine(ShowMenuDelayed(1f));
    }

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
        SetActive(countDownUI, true);
        ShowCountdown(GameConstants.CountdownSeconds);
    }

    private void OnGameStarted(string eventName, object sender, object data)
    {
        Go(UIState.Gameplay);
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
        EventsPublisherTempleRun.Instance.PublishEvent(GamePlayEvents.LoadingScreenShowing, this, null);
        EventsPublisherTempleRun.Instance.PublishEvent(GamePlayEvents.LoadingScreenShown, this, null);
        yield return new WaitForSecondsRealtime(seconds);
        if(_isSignedIn)
            Go(UIState.Menu);
        else 
            Go(UIState.None);
        EventsPublisherTempleRun.Instance.PublishEvent(GamePlayEvents.LoadingScreenHidden, this, null);
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
        SetActive(menuUI, s == UIState.Menu);
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