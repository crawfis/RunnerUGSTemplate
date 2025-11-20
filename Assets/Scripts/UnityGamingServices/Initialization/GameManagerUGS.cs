using CrawfisSoftware.Utilities;

using System;
using System.Threading.Tasks;

using Unity.Services.Authentication;

using UnityEngine;
using UnityEngine.SceneManagement;

using Logger = CrawfisSoftware.Utilities.Logger;
using Random = UnityEngine.Random;
namespace CrawfisSoftware.UGS
{
    /// <summary>
    /// Core game manager that handles scene navigation, authentication flow, and gameplay session management.
    /// Serves as a bridge between the meta-game systems (UGS features) and the core Match3 gameplay.
    /// 
    /// Key responsibilities:
    /// - Scene management (Init, MainMenu, PlayerHub, Gameplay)
    /// - Authentication state handling
    /// - Gameplay session lifecycle
    /// </summary>
    public class GameManagerUGS : MonoBehaviour
    {
        private PlayerAuthenticationManager m_AuthenticationManager;

        public event Action GameplayLevelWon;
        public event Action GameplayReplayLevelLost;
        
        public void Initialize(PlayerAuthenticationManager authenticationManager)
        {
            m_AuthenticationManager = authenticationManager;
            
            HandleStartupAuthentication();
        }
        
        private async void HandleStartupAuthentication()
        {
            await m_AuthenticationManager.SignInCachedPlayerAsync();

            if (m_AuthenticationManager.IsSignedIn)
            {
                EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.PlayerSignedIn, this, (AuthenticationService.Instance.PlayerName, AuthenticationService.Instance.PlayerId));
            }
        }
    }
}
