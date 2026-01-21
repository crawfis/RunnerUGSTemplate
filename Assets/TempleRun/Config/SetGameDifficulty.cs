using CrawfisSoftware.Events;

using UnityEngine;

namespace CrawfisSoftware.TempleRun.GameConfig
{
    /// <summary>
    /// Sends a message to set the game difficulty at the start of the game.
    /// </summary>
    public class SetGameDifficulty : MonoBehaviour
    {
        // Debug option to override player prefs for testing.
        [SerializeField] private bool _overridePlayerPrefs;
        [SerializeField] private string _overrideGameDifficultyName;

        private void Start()
        {
            string currentDifficulty;
            if (_overridePlayerPrefs)
            {
                PlayerPrefs.SetString(PlayerPrefKeys.GameDifficultyKey, _overrideGameDifficultyName);
                currentDifficulty = _overrideGameDifficultyName;
            }
            else
            {
                currentDifficulty = PlayerPrefs.GetString(PlayerPrefKeys.GameDifficultyKey, "Easy");
            }
            EventsPublisherGameFlow.Instance?.PublishEvent(GameFlowEvents.DifficultyChanging, this, currentDifficulty);
        }
    }
}