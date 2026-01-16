////using CrawfisSoftware.AreaUpgradables;
//using CrawfisSoftware.DependencyInjection;
//using CrawfisSoftware.PlayerDataManagement;
//using CrawfisSoftware.PlayerEconomyManagement;

//using Unity.Services.CloudCode;
//using Unity.Services.CloudCode.GeneratedBindings;
//using Unity.Services.Core;

//using UnityEngine;

//namespace CrawfisSoftware.UGS
//{
//    /// <summary>
//    /// Core initialization class responsible for bootstrapping the game's systems and services.
//    /// Handles Unity Gaming Services setup, core game systems initialization, and service registration.
//    /// This class follows a modular architecture pattern where each system is initialized independently
//    /// and registered with a central locator for global access.
//    /// 
//    /// Key responsibilities:
//    /// - Unity Gaming Services initialization
//    /// - Core game systems creation and setup
//    /// - Service registration via GameSystemLocator
//    /// - Development utilities (account deletion)
//    /// </summary>
//    public class GameInitializer : MonoBehaviour
//    {
//        [SerializeField] private GameManagerUGS m_GameManagerUGSPrefab;
//        [SerializeField] private string _remoteConfigDifficultyLevel = "Hard";

//        //const string k_Environment = "development";
//        PlayerAuthenticationManager _authenticationManager;
//        GameManagerUGS _gameManager;
//        RemoteConfigManager _remoteConfigManager;
//        bool _isInitialized = false;

//        private async void Awake()
//        {
//            EventsPublisherUGS.Instance.SubscribeToEvent(UGS_EventsEnum.PlayerSignedIn, OnPlayerSignedIn);
//            if (UnityServices.Instance != null && UnityServices.State == ServicesInitializationState.Initialized)
//            {
//                InitializeAuthentication();
//            }
//            else
//            {
//                UnityServices.Initialized += InitializeAuthentication;
//            }
//        }
//        private void OnDestroy()
//        {
//            _remoteConfigManager?.Dispose();
//            _remoteConfigManager = null;
//        }

//        private void InitializeAuthentication()
//        {
//            _authenticationManager = new PlayerAuthenticationManager();
//            _gameManager = Instantiate(m_GameManagerUGSPrefab);
//            DontDestroyOnLoad(_gameManager.gameObject);
//            EventsPublisherUGS.Instance.PublishEvent(UGS_EventsEnum.GameManagerInstantiated, this, null);
//            _gameManager.Initialize(_authenticationManager);
//        }

//        private void OnPlayerSignedIn(string eventName, object sender, object data)
//        {
//            if (_isInitialized) return;
//            InitializeCoreGameSystems();
//            _isInitialized = true;
//        }

//        /// <summary>
//        /// Initializes all core game systems and establishes their dependencies:
//        /// 1. Scene/UI systems for loading and transitions
//        /// 2. Network/Authentication for online services
//        /// 3. Player systems (data, economy, areas) with local and cloud components
//        ///
//        /// The architecture uses a Client/Manager pattern where:
//        /// - Managers handle local game state and logic
//        /// - "-Client" classes interface with Cloud Code via CloudBindingsProvider
//        /// - Systems are registered with GameSystemLocator for global access
//        /// </summary>
//        private void InitializeCoreGameSystems()
//        {
//            // Network & Authentication
//            var networkConnectivityHandler = _gameManager.gameObject.GetComponent<NetworkConnectivityHandler>();

//            // Player Data
//            var playerDataServiceProvider = new PlayerDataServiceBindings(CloudCodeService.Instance);
//            var localStorageSystem = new LocalStorageSystem();
//            var dataManager = new PlayerDataManager(_gameManager, localStorageSystem);
//            var dataManagerClient = new PlayerDataManagerClient(_gameManager, _authenticationManager, playerDataServiceProvider, networkConnectivityHandler);

//            // Economy
//            var playerEconomyServiceProvider = new PlayerEconomyServiceBindings(CloudCodeService.Instance);
//            var economyManager = new PlayerEconomyManager(localStorageSystem);
//            var economyManagerClient = new PlayerEconomyManagerClient(dataManagerClient, playerEconomyServiceProvider);

//            // Areas
//            //var areaManager = new AreaManager(dataManager, economyManager);
//            //var areaManagerClient = new AreaManagerClient(dataManagerClient, bindingsProvider);
//            //var commandBatchSystem = new CommandBatchSystem(dataManager,areaManager, bindingsProvider, localStorageSystem);

//            // Remote Config
//            RemoteConfigManager _remoteConfigManager = new RemoteConfigManager();


//            RegisterCoreGameSystems
//            (
//                _authenticationManager,
//                playerDataServiceProvider,
//                playerEconomyServiceProvider,
//                _gameManager,
//                dataManager,
//                dataManagerClient,
//                economyManager,
//                economyManagerClient,
//                //areaManager,
//                //areaManagerClient,
//                //commandBatchSystem,
//                networkConnectivityHandler
//            );

//            dataManager.Initialize(dataManagerClient, economyManager);
//            economyManager.Initialize(economyManagerClient);
//            _remoteConfigManager.Initialize();
//        }

//        /// <summary>
//        /// Registers all core game systems with the GameSystemLocator.
//        /// This provides centralized access to these systems throughout the game.
//        /// </summary>
//        private void RegisterCoreGameSystems
//        (
//            PlayerAuthenticationManager authenticationManager,
//            PlayerDataServiceBindings playerDataBindingsProvider,
//            PlayerEconomyServiceBindings economyBindingsProvider,
//            GameManagerUGS gameManagerUGS,
//            PlayerDataManager playerDataManager,
//            PlayerDataManagerClient playerDataManagerClient,
//            PlayerEconomyManager economyManager,
//            PlayerEconomyManagerClient economyClient,
//            //AreaManager areaManager,
//            //AreaManagerClient areaManagerClient,
//            //CommandBatchSystem commandBatchSystem,
//            NetworkConnectivityHandler networkConnectivityHandler
//        )
//        {
//            //GameSystemLocator.Register<PlayerAuthenticationManager>(authenticationManager);
//            GameSystemLocator.Register<PlayerDataServiceBindings>(playerDataBindingsProvider);
//            GameSystemLocator.Register<PlayerEconomyServiceBindings>(economyBindingsProvider);
//            GameSystemLocator.Register<GameManagerUGS>(gameManagerUGS);
//            GameSystemLocator.Register<PlayerDataManager>(playerDataManager);
//            GameSystemLocator.Register<PlayerDataManagerClient>(playerDataManagerClient);
//            GameSystemLocator.Register<PlayerEconomyManager>(economyManager);
//            GameSystemLocator.Register<PlayerEconomyManagerClient>(economyClient);
//            //GameSystemLocator.Register<AreaManager>(areaManager);
//            //GameSystemLocator.Register<AreaManagerClient>(areaManagerClient);
//            //GameSystemLocator.Register<CommandBatchSystem>(commandBatchSystem);
//            GameSystemLocator.Register<NetworkConnectivityHandler>(networkConnectivityHandler);
//        }
//    }
//}