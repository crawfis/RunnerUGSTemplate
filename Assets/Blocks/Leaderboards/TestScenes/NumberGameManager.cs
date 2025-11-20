using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.Core;
using Unity.Services.Leaderboards.Exceptions;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace Blocks.Leaderboards.TestScene
{
    public class NumberGameManager : MonoBehaviour
    {
        const string k_FormatScore = "Your score: {0}";
        const string k_QuestionMarks = "??";

        enum GameClientType
        {
            Local,
            Trusted
        }

        [SerializeField]
        UIDocument m_GameUI;

        [SerializeField]
        GameClientType m_GameClientType = GameClientType.Local;

        ServiceObserver<IAuthenticationService> m_AuthenticationServiceObserver;
        [SerializeField]
        LeaderboardPrefab m_LeaderboardPrefab;

        Button m_ButtonGame;
        Button m_ButtonReset;
        Button m_ButtonPopulate;

        Label m_LabelGoal;
        Label m_LabelScore;

        IntegerField m_FieldGuess;

        INumberGameClient m_Client;

        void Awake()
        {
            InitializeLeaderboard();
            InitializeUI();
            InitializeAuthenticationObserver();
        }

        void InitializeLeaderboard()
        {
            m_LeaderboardPrefab.Initialize(m_GameClientType != GameClientType.Local);
        }

        void InitializeUI()
        {
            m_ButtonGame = m_GameUI.rootVisualElement.Q<Button>("ButtonGame");
            m_ButtonGame.clickable.clicked += OnButtonGameClicked;
            m_ButtonGame.style.display = DisplayStyle.Flex;

            m_ButtonReset = m_GameUI.rootVisualElement.Q<Button>("ButtonReset");
            m_ButtonReset.clickable.clicked += OnResetButtonClicked;
            m_ButtonReset.style.display = DisplayStyle.None;

            m_ButtonPopulate = m_GameUI.rootVisualElement.Q<Button>("ButtonPopulate");
            m_ButtonPopulate.clickable.clicked += OnPopulateButtonClicked;

            m_LabelGoal = m_GameUI.rootVisualElement.Q<Label>("LabelGoal");
            m_LabelScore = m_GameUI.rootVisualElement.Q<Label>("LabelScore");

            m_FieldGuess = m_GameUI.rootVisualElement.Q<IntegerField>();

            ToggleButtonsEnabledSelf(false);
            ResetLabels();
        }

        void InitializeAuthenticationObserver()
        {
            m_AuthenticationServiceObserver = new ServiceObserver<IAuthenticationService>();
            if (m_AuthenticationServiceObserver.Service == null)
            {
                m_AuthenticationServiceObserver.Initialized += InitializeGameClient;
            }
            else
            {
                InitializeGameClient(m_AuthenticationServiceObserver.Service);
            }
        }

        void InitializeGameClient(IAuthenticationService authenticationService)
        {
            m_AuthenticationServiceObserver.Initialized -= InitializeGameClient;

            switch (m_GameClientType)
            {
                case GameClientType.Local:
                    LeaderboardsObserver.Instance.UseTrustedClient = false;
                    m_Client = new LocalNumberGameClient();
                    break;
                case GameClientType.Trusted:
                    LeaderboardsObserver.Instance.UseTrustedClient = true;
                    m_Client = new TrustedNumberGameClient();
                    break;
            }

            ToggleButtonsEnabledSelf(true);
        }

        void ToggleButtonsEnabledSelf(bool toggle)
        {
            m_ButtonGame.enabledSelf = toggle;
            m_ButtonPopulate.enabledSelf = toggle;
            m_ButtonPopulate.enabledSelf = toggle;
        }

        async void OnButtonGameClicked()
        {
            if (string.IsNullOrEmpty(m_FieldGuess.text))
            {
                return;
            }

            m_ButtonGame.enabledSelf = false;

            try
            {
                UpdateGoalAndScore(await m_Client.RequestGoalAndScore(m_LeaderboardPrefab.LeaderboardId, m_FieldGuess.value));
                await m_LeaderboardPrefab.UpdateScoresAsync();
            }
            catch (LeaderboardsException e)
            {
                if (e.ErrorCode == 56
                    && e.Message.Contains("Access has been restricted"))
                {
                    Debug.LogWarning($"[Kits] Your game client type is set to `{GameClientType.Local}`. " +
                        $"Make sure your access control policy is not active for leaderboards when using the `{GameClientType.Local}` game client. Delete it, swap to the `{GameClientType.Trusted}` client, or mark the effect to `Allow`.");
                    Debug.LogException(e);
                }
                else
                {
                    throw;
                }
            }
            catch (CloudCodeException e)
            {
                if (e.Message.Contains("Message: Forbidden"))
                {
                    Debug.LogWarning($"[Kits] Your game client type is set to `{GameClientType.Trusted}`. " +
                        "Make sure the KitsGameModule is using the ServiceToken to call the Leaderboards Service.");
                    Debug.LogException(e);
                }
                else
                {
                    throw;
                }
            }

            m_ButtonGame.enabledSelf = true;
            m_ButtonGame.style.display = DisplayStyle.None;
            m_ButtonReset.style.display = DisplayStyle.Flex;
        }

        async void OnPopulateButtonClicked()
        {
            await SetupFakeEntriesForLeaderboard(m_LeaderboardPrefab.LeaderboardId);
            await m_LeaderboardPrefab.UpdateScoresAsync();
        }

        static async Task SetupFakeEntriesForLeaderboard(string lbName, int nbOfEntries = 50, int minScore = 1, int maxScore = 100)
        {
            var cloudCode = new LeaderboardAdminFunctionsBindings(CloudCodeService.Instance);
            // leaderboard may limit cloud-code past some number of calls per minute
            var res = await cloudCode.SetupFakeEntries(lbName, nbOfEntries, minScore, maxScore);
            Debug.Log(res);
        }

        void UpdateGoalAndScore(NumberGameResult result)
        {
            m_LabelGoal.text = result.Goal.ToString();
            m_LabelScore.text = string.Format(k_FormatScore, result.Score);
        }

        void OnResetButtonClicked()
        {
            m_ButtonReset.style.display = DisplayStyle.None;
            m_ButtonGame.style.display = DisplayStyle.Flex;

            ResetLabels();
        }

        void ResetLabels()
        {
            m_LabelGoal.text = k_QuestionMarks;
            m_LabelScore.text = string.Format(k_FormatScore, k_QuestionMarks);
        }
    }
}
