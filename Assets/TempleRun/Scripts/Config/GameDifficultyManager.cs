using CrawfisSoftware.Config;
using CrawfisSoftware.TempleRun.Events;

using System;
using System.Collections.Generic;

using UnityEngine;

using TempleRunBus = CrawfisSoftware.Events.EventsFor<CrawfisSoftware.TempleRun.TempleRunEvents>;

namespace CrawfisSoftware.TempleRun.GameConfig
{
    /// <summary>
    /// Manages difficulty configurations for TempleRun gameplay.
    ///    Dependencies: DifficultyConfig (shared, from the common package)
    ///    Subscribes: TempleRunEvents.TempleRunDifficultyChangeRequested,
    ///                TempleRunEvents.TempleRunDifficultySettingsApplied (the LOCAL table),
    ///                TempleRunEvents.DifficultySettingsApplied (the REMOTE table, bridged from GameFlow)
    ///    Publishes: TempleRunEvents.TempleRunDifficultyChanging, TempleRunEvents.DifficultyChangeFailed
    /// </summary>
    /// <remarks>
    /// <para><b>Remote wins, whichever arrives first.</b> Two tables can reach this component: the
    /// local one that <c>LoadDefaultGameConfigs</c> publishes from a ScriptableObject in
    /// <c>Start</c>, and the remote one Remote Config supplies during boot. Each replaces the table
    /// wholesale, so without a rule the winner would be decided by scene load order - and the local
    /// publish, running in a gameplay scene's <c>Start</c>, would usually land last and silently
    /// discard the remote table.</para>
    /// <para>So a remote table latches. Once one has been applied the local publish is ignored for
    /// the life of this component. The remote event is Sticky, so arriving before this component
    /// existed is not the same as never arriving: the retained table is delivered on subscribe.</para>
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
            TempleRunBus.Subscribe(TempleRunEvents.TempleRunDifficultyChangeRequested, OnDifficultyChanging);
            TempleRunBus.Subscribe(TempleRunEvents.TempleRunDifficultySettingsApplied, OnDifficultySettingsChanged);
            TempleRunBus.Subscribe(TempleRunEvents.DifficultySettingsApplied, OnRemoteDifficultySettingsApplied);
        }

        private void OnDestroy()
        {
            TempleRunBus.Unsubscribe(TempleRunEvents.TempleRunDifficultyChangeRequested, OnDifficultyChanging);
            TempleRunBus.Unsubscribe(TempleRunEvents.TempleRunDifficultySettingsApplied, OnDifficultySettingsChanged);
            TempleRunBus.Unsubscribe(TempleRunEvents.DifficultySettingsApplied, OnRemoteDifficultySettingsApplied);
        }

        public void SetDifficulty(string difficultyName)
        {
            Debug.Log($"Attempting to set game difficulty from {CurrentDifficulty} to {difficultyName}");
            if (_difficultyConfigs.ContainsKey(difficultyName))
            {
                CurrentDifficulty = difficultyName;
                TempleRunBus.Publish(TempleRunEvents.TempleRunDifficultyChanging, this, _difficultyConfigs[CurrentDifficulty]);
            }
            else
            {
                Debug.LogWarning($"SetDifficulty failed: difficulty '{difficultyName}' not found in available configurations.");
            }
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

        public void OnDifficultyChanging(string eventName, object sender, object data)
        {
            string newDifficulty = data as string;
            if (string.IsNullOrEmpty(newDifficulty))
            {
                TempleRunBus.Publish(TempleRunEvents.DifficultyChangeFailed, this, CurrentDifficultyConfig);
                return;
            }
            SetDifficulty(newDifficulty);
        }

        public void OnDifficultySettingsChanged(string eventName, object sender, object data)
        {
            if (_remoteSettingsApplied)
            {
                // The local ScriptableObject table is the fallback, not an override. Applying it
                // over a table Remote Config supplied would undo the remote one for the rest of
                // the session, and would do it invisibly.
                return;
            }

            PopulateDifficulties(RequireConfigs(data, nameof(OnDifficultySettingsChanged)));
        }

        public void OnRemoteDifficultySettingsApplied(string eventName, object sender, object data)
        {
            PopulateDifficulties(RequireConfigs(data, nameof(OnRemoteDifficultySettingsApplied)));
            _remoteSettingsApplied = true;
        }

        private static IList<DifficultyConfig> RequireConfigs(object data, string handler)
        {
            var difficultyConfigs = data as IList<DifficultyConfig>;
            if (difficultyConfigs == null)
            {
                throw new ArgumentException($"{handler} event data must be of type IList<DifficultyConfig>");
            }
            return difficultyConfigs;
        }
    }
}
