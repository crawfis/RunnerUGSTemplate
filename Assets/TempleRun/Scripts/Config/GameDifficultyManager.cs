using CrawfisSoftware.Config;
using CrawfisSoftware.TempleRun.Events;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using TempleRunBus = CrawfisSoftware.Events.EventsFor<CrawfisSoftware.TempleRun.TempleRunEvents>;

namespace CrawfisSoftware.TempleRun.GameConfig
{
    /// <summary>
    /// Manages difficulty configurations for TempleRun gameplay.
    /// Subscribes to TempleRunEvents (via bridge from the GameFlow domain).
    ///    Dependencies: DifficultyConfig (shared, from the common package)
    ///    Subscribes: TempleRunEvents.TempleRunDifficultyChangeRequested,
    ///                TempleRunEvents.TempleRunDifficultySettingsApplied (level, or the fallback),
    ///                TempleRunEvents.DifficultySettingsApplied (the REMOTE table, bridged from GameFlow)
    ///    Publishes: TempleRunEvents.TempleRunDifficultyChanging, TempleRunEvents.DifficultyChangeFailed
    /// </summary>
    /// <remarks>
    /// <para><b>Three sources, ranked: remote &gt; level &gt; built-in fallback.</b> Each replaces
    /// the table wholesale, so without a rank the winner would be whichever published last -
    /// which, before this rank existed, meant the built-in fallback running in a gameplay scene's
    /// <c>Start</c> silently overwrote the level the player had just chosen.</para>
    /// <para>The top rank latches: once a remote table has been applied, nothing displaces it for
    /// the life of this component. The bottom rank stands itself down - <c>LoadDefaultGameConfigs</c>
    /// publishes only when no table has been retained yet - so the middle rank needs no guard here.
    /// Both table events are Sticky, so arriving before this component existed is not the same as
    /// never arriving: the retained table is delivered on subscribe.</para>
    /// </remarks>
    public class GameDifficultyManager : MonoBehaviour
    {
        public string CurrentDifficulty { get; private set; } = "Easy";
        public DifficultyConfig CurrentDifficultyConfig
        {
            get
            {
                if (_difficultyConfigs.ContainsKey(CurrentDifficulty))
                {
                    return _difficultyConfigs[CurrentDifficulty];
                }
                else
                {
                    Debug.LogWarning($"Current difficulty '{CurrentDifficulty}' not found. Returning null.");
                    return null;
                }
            }
        }
        public IEnumerable<string> AvailableDifficulties => _difficultyConfigs.Keys;
        public IEnumerable<DifficultyConfig> AvailableDifficultyConfigs => _difficultyConfigs.Values;

        private readonly Dictionary<string, DifficultyConfig> _difficultyConfigs = new Dictionary<string, DifficultyConfig>();

        private bool _remoteSettingsApplied;

        public void Awake()
        {
            TempleRunBus.Subscribe(TempleRunEvents.TempleRunDifficultyChangeRequested, OnDifficultyChangeRequested);
            TempleRunBus.Subscribe(TempleRunEvents.TempleRunDifficultySettingsApplied, OnDifficultySettingsChanged);
            TempleRunBus.Subscribe(TempleRunEvents.DifficultySettingsApplied, OnRemoteDifficultySettingsApplied);
        }

        private void OnDestroy()
        {
            TempleRunBus.Unsubscribe(TempleRunEvents.TempleRunDifficultyChangeRequested, OnDifficultyChangeRequested);
            TempleRunBus.Unsubscribe(TempleRunEvents.TempleRunDifficultySettingsApplied, OnDifficultySettingsChanged);
            TempleRunBus.Unsubscribe(TempleRunEvents.DifficultySettingsApplied, OnRemoteDifficultySettingsApplied);
        }

        // The table is the selected level's difficulty variants, so a level decides which
        // difficulties it offers. A preference the level does not offer resolves to the level's
        // first variant rather than leaving GameConfig unset - the player asked for a level, and
        // playing it at its own difficulty beats not playing it. That fallback is load-bearing
        // now that levels carry their own variant sets: a level that offers only "Medium" would
        // otherwise ignore the "Easy" that SetGameDifficulty reads out of PlayerPrefs.
        public void SetDifficulty(string difficultyName)
        {
            Debug.Log($"Attempting to set game difficulty from {CurrentDifficulty} to {difficultyName}");
            if (!_difficultyConfigs.ContainsKey(difficultyName))
            {
                if (_difficultyConfigs.Count == 0)
                {
                    Debug.LogWarning($"SetDifficulty failed: no difficulty configurations have been applied.");
                    return;
                }
                string fallback = _difficultyConfigs.Keys.First();
                Debug.LogWarning($"This level does not offer difficulty '{difficultyName}'; using '{fallback}'.");
                difficultyName = fallback;
            }
            CurrentDifficulty = difficultyName;
            TempleRunBus.Publish(TempleRunEvents.TempleRunDifficultyChanging, this, _difficultyConfigs[CurrentDifficulty]);
        }

        public void PopulateDifficulties(IList<DifficultyConfig> difficulties)
        {
            Clear();
            foreach (var config in difficulties)
            {
                AddConfig(config);
            }
        }

        public void Clear()
        {
            _difficultyConfigs?.Clear();
        }

        public void AddConfig(DifficultyConfig difficultyConfig)
        {
            _difficultyConfigs[difficultyConfig.DifficultyName] = difficultyConfig;
        }

        // An empty name is still worth reporting: a payload of the wrong type throws on the cast,
        // but a caller can legitimately publish "" and there is no difficulty by that name.
        public void OnDifficultyChangeRequested(string eventName, object sender, object data)
        {
            var newDifficulty = (string)data;
            if (string.IsNullOrEmpty(newDifficulty))
            {
                TempleRunBus.Publish(TempleRunEvents.DifficultyChangeFailed, this, CurrentDifficultyConfig);
                return;
            }
            SetDifficulty(newDifficulty);
        }

        // The level's table, or the built-in fallback when no level supplied one - they share this
        // event, and LoadDefaultGameConfigs decides between them by standing down when a table has
        // already been retained. Either way a remote table outranks both.
        public void OnDifficultySettingsChanged(string eventName, object sender, object data)
        {
            if (_remoteSettingsApplied)
            {
                // Applying a local table over one Remote Config supplied would undo the remote one
                // for the rest of the session, and would do it invisibly.
                return;
            }

            var difficultyConfigs = (IList<DifficultyConfig>)data;
            PopulateDifficulties(difficultyConfigs);
        }

        public void OnRemoteDifficultySettingsApplied(string eventName, object sender, object data)
        {
            var difficultyConfigs = (IList<DifficultyConfig>)data;
            PopulateDifficulties(difficultyConfigs);
            _remoteSettingsApplied = true;
        }
    }
}
