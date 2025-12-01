using System.Threading.Tasks;

using Unity.Services.Authentication;
using Unity.Services.Core;

using UnityEngine;

using Logger = CrawfisSoftware.Utilities.Logger;
namespace CrawfisSoftware.UGS
{
    /// <summary>
    /// Manages player authentication lifecycle with Unity Gaming Services.
    /// Handles initial sign-in with cached credentials, access token expiry recovery,
    /// and authentication state changes through events. 
    /// </summary>
    public class PlayerAuthenticationManager : MonoBehaviour
    {
        public bool IsSignedIn => AuthenticationService.Instance.IsSignedIn;
        private bool m_IsResumingFromExpiredToken = false;
        
        private const string k_KeyEmoji = "🔑";
        
        public void Awake()
        {
            AuthenticationService.Instance.SwitchProfile("initial-development"); 
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.PlayerAuthenticating, HandleSuccessfulSignIn);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.PlayerSignedOut, HandleSignedOut);
            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.PlayerSessionExpired, HandleSessionExpired);
            //AuthenticationService.Instance.SignedIn += HandleSuccessfulSignIn;
            //AuthenticationService.Instance.SignedOut += HandleSignedOut;
            //AuthenticationService.Instance.Expired += HandleSessionExpired;
        }

        public void OnDestroy()
        {
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.PlayerAuthenticating, HandleSuccessfulSignIn);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.PlayerSignedOut, HandleSignedOut);
            EventsPublisherUGS.Instance.UnsubscribeToEvent(UGS_EventsEnum.PlayerSessionExpired, HandleSessionExpired);
            //AuthenticationService.Instance.SignedIn -= HandleSuccessfulSignIn;
            //AuthenticationService.Instance.SignedOut -= HandleSignedOut;
            //AuthenticationService.Instance.Expired -= HandleSessionExpired;
        }

        private async void Start()
        {
            // Sign in here automatically from cached session on game start
            await SignInCachedPlayerAsync();
        }

        /// <summary>
        /// Note: Assumes the player is already signed in!
        /// Attempts to sign in the player using a cached authentication session, if available.
        /// </summary>
        /// <remarks>If no cached session exists, the sign-in attempt fails and the SignInFailed event is
        /// invoked. If the cached session is invalid or the sign-in fails due to authentication or network errors, the
        /// SignInFailed event is also invoked. This method does not prompt the user for credentials and only succeeds
        /// if a valid cached session is present.</remarks>
        /// <returns>A task that represents the asynchronous sign-in operation.</returns>
        public async Task SignInCachedPlayerAsync()
        {
            if (!AuthenticationService.Instance.SessionTokenExists)
            {
                Logger.LogDemo($"{k_KeyEmoji} No cached session found");
                EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerSignInFailed, this, null);
                return;
            }
            
            try 
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Logger.LogDemo($"{k_KeyEmoji} Existing player returned");

                // Sign in returning player using the Session Token stored after sign in.
                // Players don't need to sign in with Unity Player Account again; using 
                // this method will re-authorise them (they will keep the same Player ID).
                if (AuthenticationService.Instance.IsAuthorized)
                {
                    //EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerSignedIn, this, (AuthenticationService.Instance.PlayerName, AuthenticationService.Instance.PlayerId));
                    Debug.Log($"Returning Player ID: {AuthenticationService.Instance.PlayerId}");
                    Debug.Log($"Returning Player is Authorized: {AuthenticationService.Instance.IsAuthorized}");
                    EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerAuthenticated, this, (AuthenticationService.Instance.PlayerName, AuthenticationService.Instance.PlayerId));
                }
            }
            catch (AuthenticationException ex) 
            {
                Logger.LogWarning($"💡 Authentication failed - if testing, try enabling 'Delete Account On Start' in GameInitializer to reset state {ex.Message}");
                EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerSignInFailed, this, null);
            }
            catch (RequestFailedException ex) 
            {
                Logger.LogWarning($"Network error during sign-in: {ex.Message}");
                EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerSignInFailed, this, null);
            }
        }
        
        /// <summary>
        /// Unity Authentication's access tokens are valid for 1 hour and refreshed when necessary.
        /// If the token can't be refreshed (e.g. the player is offline), the token expires.
        /// In this case, when the player goes back online, they need to be signed in again to obtain authorization to call Unity services
        /// </summary>
        public async Task SignInResumeFromExpiredAccessTokenAsync()
        {
            if (!AuthenticationService.Instance.IsExpired)
            {
                Logger.LogWarning("Sign in not required, access token has not expired");
                return;
            }
            
            try
            {
                Logger.Log($"{k_KeyEmoji} Signing in again due to expired access token");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                m_IsResumingFromExpiredToken = true;
            }
            catch (RequestFailedException ex) 
            {
                Logger.LogWarning($"Network error during sign-in: {ex.Message}");
                EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerSignInFailed, this, null);
            }
        }
        
        private void HandleSuccessfulSignIn(string eventName, object sender, object data)
        {
            // For simplicity, requires being online
            if (m_IsResumingFromExpiredToken)
            {
                // An event for handling coming online after being offline for a while (e.g. player progress is validated in and saved to cloud)
                EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerResumedFromExpiredToken, this, UnityEngine.Time.time);
                m_IsResumingFromExpiredToken = false;
                //return;
            }
            _ = SignInCachedPlayerAsync();
            //EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerSignedIn, this, null);
            // PlayerDataManagerClient handles sign in by fetching cloud data, this flows to overwriting local data
            LogPlayerInfo();
            EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerAuthenticated, this, (AuthenticationService.Instance.PlayerId, AuthenticationService.Instance.AccessToken));
        }

        private void HandleSignedOut(string eventName, object sender, object data)
        {
            Logger.LogDemo($"{k_KeyEmoji} Player signed out");
        }

        private void HandleSessionExpired(string eventName, object sender, object data)
        {
            Logger.LogDemo($"{k_KeyEmoji} Session expired! You'll need to sign in again when possible");
        }
        
        private void LogPlayerInfo()
        {
            var playerId = AuthenticationService.Instance.PlayerId;
            var accessToken = AuthenticationService.Instance.AccessToken;
            Logger.Log($"{k_KeyEmoji} Authentication successful!" +
                $"\n{k_KeyEmoji} PlayerID: {playerId}" +
                $"\n{k_KeyEmoji} Token: {accessToken}");
        }
    }
}
