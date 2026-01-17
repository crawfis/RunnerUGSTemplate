using CrawfisSoftware.UGS;
using CrawfisSoftware.UGS.Events;

using System;

using UnityEngine;

namespace CrawfisSoftware.UGS.Authentication
{
    internal class UnityEventsToEventsPublisher : MonoBehaviour
    {
        public  void UnityServicesInitialized()
        {
            EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.UnityServicesInitialized, this, null);
        }
        public void UnityServicesInitializationFailed(Exception ex)
        {
            EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.UnityServicesInitializationFailed, this, ex);
        }
        public void SignedIn()
        {
            EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerSignedIn, this, (Unity.Services.Authentication.AuthenticationService.Instance.PlayerName, Unity.Services.Authentication.AuthenticationService.Instance.PlayerId));
        }
        public void SignInFailed(Exception ex)
        {
            EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerSignInFailed, this, ex);
        }
        public void SignOut()
        {
            EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerSignedOut, this, null);
        }
        public void SessionExpired()
        {
            EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerSessionExpired, this, null);
        }
    }
}