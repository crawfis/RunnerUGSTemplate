using Unity.Services.Core;
using Unity.Services.Core.Components;

using UnityEngine;

namespace CrawfisSoftware.UGS
{
    internal class UGS_State : MonoBehaviour
    {
        [SerializeField] private ServicesInitialization _uGS_Services;
        public static UGS_State Instance { get; private set; }

        public static string UGS_Environment => Instance._uGS_Services.EnvironmentName;

        // Keep track of potentially missed events in scenes that load after UGS initialization
        public static bool IsUnityServicesInitialized { get; private set; } = false;
        public static bool IsRemoteConfigUpdated { get; private set; } = false;
        public static bool IsCheckForExistingSession { get; private set; } = false;
        public static bool IsPlayerSigningIn { get; private set; } = false;
        public static bool IsPlayerSignedIn { get; private set; } = false;
        public static bool IsPlayerAuthenticated { get; private set; } = false;

        private void Awake()
        {
            if(Instance != null)
            {
                DestroyImmediate(Instance);
            }
            Instance = this;

            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                IsUnityServicesInitialized = true;
            }
            else
            {
                EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.UnityServicesInitialized, OnUnityServicesInitialized);
            }
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.RemoteConfigUpdated, OnRemoteConfigUpdated);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.CheckForExistingSession, OnCheckingForExistingSession);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.PlayerSigningIn, OnPlayerSigningIn);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.PlayerSignedIn, OnPlayerSignedIn);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.PlayerAuthenticated, OnPlayerAuthenticated);
        }

        private void OnDestroy()
        {
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.UnityServicesInitialized, OnUnityServicesInitialized);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.CheckForExistingSession, OnCheckingForExistingSession);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.PlayerSigningIn, OnPlayerSigningIn);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.RemoteConfigUpdated, OnRemoteConfigUpdated);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.PlayerSignedIn, OnPlayerSignedIn);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.PlayerAuthenticated, OnPlayerAuthenticated);
        }

        private void OnUnityServicesInitialized(string eventName, object sender, object data)
        {
            IsUnityServicesInitialized = true;
        }

        private void OnRemoteConfigUpdated(string eventName, object sender, object data)
        {
            IsRemoteConfigUpdated = true;
        }

        private void OnCheckingForExistingSession(string eventName, object sender, object data)
        {
            IsCheckForExistingSession = true;
        }

        private void OnPlayerSigningIn(string eventName, object sender, object data)
        {
            IsPlayerSigningIn = true;
            IsPlayerSignedIn = false;
        }

        private void OnPlayerSignedIn(string eventName, object sender, object data)
        {
            IsPlayerSignedIn = true;
            IsPlayerSigningIn = false;
        }

        private void OnPlayerAuthenticated(string eventName, object sender, object data)
        {
            IsPlayerSignedIn = true;
            IsPlayerSigningIn = false;
            IsPlayerAuthenticated = true;
        }
    }
}