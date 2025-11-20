using CrawfisSoftware.GameConfig;

using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Loads a configuration ScriptableObject and puts in on the Blackboard
    /// Dependencies: Blackboard, TempleRunGameConfig
    /// </summary>
    public class LoadGameConfig : MonoBehaviour
    {
        [SerializeField] private DifficultyConfig _gameConfig;

        private void Start()
        {
            Blackboard.Instance.GameConfig = _gameConfig;
        }
    }
}