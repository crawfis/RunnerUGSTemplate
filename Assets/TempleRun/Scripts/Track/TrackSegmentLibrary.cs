using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// A single track segment definition stored in the segment registry.
    ///   Id:               Unique identifier referenced by connection rules and level files.
    ///   Direction:        "Left", "Right", "Both", or "Straight".
    ///   Length:           Physical segment length in Unity units.
    ///   Weight:           Relative selection weight (higher = more frequent).
    ///   MaxRepeat:        Max consecutive appearances (0 = unlimited).
    ///   DifficultyRating: 0–10 continuous difficulty scale (shorter = harder).
    ///   Tags:             Category labels used by level files to filter segments.
    /// </summary>
    [Serializable]
    public class TrackSegmentDefinition
    {
        public string Id;
        public string Direction;
        public float Length = 5f;
        public float Weight = 1f;
        public int MaxRepeat = 0;
        public float DifficultyRating = 0f;
        public List<string> Tags = new List<string>();
    }

    [Serializable]
    public class TrackSegmentConnection
    {
        public string FromId;
        public string ToId;
    }

    /// <summary>
    /// Standalone registry of all available track segments.
    /// Loaded once and shared across multiple level rulesets.
    /// File lives in: Assets/TempleRun/Resources/TrackSegments_Registry.json
    /// </summary>
    [Serializable]
    public class TrackSegmentRegistryDefinition
    {
        public string Version;
        public List<TrackSegmentDefinition> Segments = new List<TrackSegmentDefinition>();
    }

    /// <summary>
    /// Level-specific ruleset: describes which segments are active and how they connect.
    ///
    /// Two-file usage (recommended):
    ///   Set SegmentRegistryFile to the registry resource name.
    ///   Set ActiveSegmentTags to filter which registry segments apply to this level.
    ///   Leave Segments and Connections empty — they are populated at runtime.
    ///
    /// Single-file usage (legacy / standalone):
    ///   Populate Segments and Connections directly, leave SegmentRegistryFile empty.
    ///
    /// Connection graph:
    ///   If Connections is empty, all active segments are candidates after every turn
    ///   (all-to-all fallback). Define explicit Connections only when you need to
    ///   restrict which segments may follow which.
    /// </summary>
    [Serializable]
    public class TrackSegmentLibraryDefinition
    {
        public string Version;
        public string LevelName;
        public int LevelNumber;
        public float DifficultyRating;
        public int LaneCount = 3;
        public float LaneWidth = 2f;
        public string StartSegmentId;

        // --- Two-file split ---
        public string SegmentRegistryFile;                            // Resources path of the shared registry
        public List<string> ActiveSegmentTags = new List<string>();  // Include segments with any of these tags
        public List<string> ActiveSegmentIds  = new List<string>();  // Include segments with exactly these IDs

        // --- Segment data (populated directly or merged from registry at load time) ---
        public List<TrackSegmentDefinition> Segments = new List<TrackSegmentDefinition>();
        public List<TrackSegmentConnection> Connections = new List<TrackSegmentConnection>();
    }

    /// <summary>
    /// Runtime library for selecting track segments during gameplay.
    ///
    ///   Dependencies: UnityEngine (Resources, JsonUtility, Mathf)
    ///   Usage: Call LoadFromResources() or LoadFromJson() once per level, then call
    ///          GetStartSegment() and SelectNext() on each turn.
    ///
    /// Selection algorithm:
    ///   1. If Connections are defined for the previous segment, limit candidates to those.
    ///   2. Otherwise all active segments are candidates (all-to-all fallback).
    ///   3. MaxRepeat filters out a segment if it has appeared consecutively too many times.
    ///   4. Weighted random selection picks from the remaining candidates.
    /// </summary>
    public class TrackSegmentLibrary
    {
        private readonly TrackSegmentLibraryDefinition _definition;
        private readonly Dictionary<string, TrackSegmentDefinition> _segmentsById
            = new Dictionary<string, TrackSegmentDefinition>();
        private readonly Dictionary<string, List<string>> _connectionsByFromId
            = new Dictionary<string, List<string>>();

        // Expose level metadata for UI and TrackManagerLibrary
        public string LevelName       => _definition.LevelName;
        public int    LevelNumber     => _definition.LevelNumber;
        public float  DifficultyRating => _definition.DifficultyRating;
        public int    LaneCount       => _definition.LaneCount > 0 ? _definition.LaneCount : 3;
        public float  LaneWidth       => _definition.LaneWidth > 0f ? _definition.LaneWidth : 2f;
        public int    SegmentCount    => _definition.Segments.Count;

        public TrackSegmentLibrary(TrackSegmentLibraryDefinition definition)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));

            foreach (var segment in _definition.Segments)
            {
                if (!string.IsNullOrWhiteSpace(segment.Id))
                    _segmentsById[segment.Id] = segment;
            }

            foreach (var connection in _definition.Connections)
            {
                if (string.IsNullOrWhiteSpace(connection.FromId) ||
                    string.IsNullOrWhiteSpace(connection.ToId))
                    continue;

                if (!_connectionsByFromId.TryGetValue(connection.FromId, out var list))
                {
                    list = new List<string>();
                    _connectionsByFromId[connection.FromId] = list;
                }
                list.Add(connection.ToId);
            }
        }

        // -------------------------------------------------------------------------
        // Factory methods
        // -------------------------------------------------------------------------

        /// <summary>Load a combined (standalone / legacy) library from a single JSON string.</summary>
        public static TrackSegmentLibrary LoadFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            var definition = JsonUtility.FromJson<TrackSegmentLibraryDefinition>(json);
            return definition == null ? null : new TrackSegmentLibrary(definition);
        }

        /// <summary>
        /// Load a level file and merge segments from a separate registry JSON string.
        /// Segments are filtered by ActiveSegmentTags or ActiveSegmentIds.
        /// The StartSegmentId is always included regardless of filters.
        /// If levelDef already has Segments, the registry is ignored.
        /// </summary>
        public static TrackSegmentLibrary LoadFromJson(string levelJson, string registryJson)
        {
            if (string.IsNullOrWhiteSpace(levelJson)) return null;
            var levelDef = JsonUtility.FromJson<TrackSegmentLibraryDefinition>(levelJson);
            if (levelDef == null) return null;

            if (!string.IsNullOrWhiteSpace(registryJson) && levelDef.Segments.Count == 0)
                MergeRegistrySegments(levelDef, registryJson);

            return new TrackSegmentLibrary(levelDef);
        }

        /// <summary>
        /// Build a runtime library from an already-deserialized level definition.
        /// If registryJson is provided and levelDef has no Segments, merges from registry.
        /// </summary>
        public static TrackSegmentLibrary LoadFromDefinition(
            TrackSegmentLibraryDefinition levelDef, string registryJson = null)
        {
            if (levelDef == null) return null;
            if (!string.IsNullOrWhiteSpace(registryJson) && levelDef.Segments.Count == 0)
                MergeRegistrySegments(levelDef, registryJson);
            return new TrackSegmentLibrary(levelDef);
        }

        /// <summary>
        /// Load a level from Unity Resources. If the level definition specifies a
        /// SegmentRegistryFile, that registry is automatically loaded and merged.
        /// Example: LoadFromResources("TrackLevel_01_Beginner")
        /// </summary>
        public static TrackSegmentLibrary LoadFromResources(string levelResourcePath)
        {
            var levelAsset = Resources.Load<TextAsset>(levelResourcePath);
            if (levelAsset == null)
            {
                Debug.LogError($"[TrackSegmentLibrary] Level resource not found: '{levelResourcePath}'");
                return null;
            }

            var levelDef = JsonUtility.FromJson<TrackSegmentLibraryDefinition>(levelAsset.text);
            if (levelDef == null)
            {
                Debug.LogError($"[TrackSegmentLibrary] Failed to parse JSON: '{levelResourcePath}'");
                return null;
            }

            if (!string.IsNullOrWhiteSpace(levelDef.SegmentRegistryFile))
            {
                var registryAsset = Resources.Load<TextAsset>(levelDef.SegmentRegistryFile);
                if (registryAsset != null)
                    return LoadFromJson(levelAsset.text, registryAsset.text);

                Debug.LogWarning($"[TrackSegmentLibrary] Registry '{levelDef.SegmentRegistryFile}' " +
                                 $"not found; loading level standalone.");
            }

            return LoadFromJson(levelAsset.text);
        }

        // -------------------------------------------------------------------------
        // Selection
        // -------------------------------------------------------------------------

        public TrackSegmentDefinition GetStartSegment(System.Random random)
        {
            if (!string.IsNullOrWhiteSpace(_definition.StartSegmentId) &&
                _segmentsById.TryGetValue(_definition.StartSegmentId, out var segment))
                return segment;

            return SelectNext(null, 0, random);
        }

        /// <summary>
        /// Select the next segment after previousSegmentId.
        /// previousRepeatCount is how many times that same segment has appeared consecutively.
        /// </summary>
        public TrackSegmentDefinition SelectNext(
            string previousSegmentId,
            int previousRepeatCount,
            System.Random random)
        {
            var candidates = new List<TrackSegmentDefinition>();

            if (!string.IsNullOrWhiteSpace(previousSegmentId) &&
                _connectionsByFromId.TryGetValue(previousSegmentId, out var allowedIds))
            {
                // Use explicit connection graph
                foreach (var id in allowedIds)
                {
                    if (_segmentsById.TryGetValue(id, out var seg) &&
                        IsAllowed(seg, previousSegmentId, previousRepeatCount))
                        candidates.Add(seg);
                }
            }
            else
            {
                // No explicit connections — all active segments are candidates
                foreach (var seg in _definition.Segments)
                {
                    if (IsAllowed(seg, previousSegmentId, previousRepeatCount))
                        candidates.Add(seg);
                }
            }

            if (candidates.Count == 0)
                candidates.AddRange(_definition.Segments);

            return SelectWeighted(candidates, random);
        }

        // -------------------------------------------------------------------------
        // Private helpers
        // -------------------------------------------------------------------------

        /// <summary>
        /// Filter registry segments into levelDef.Segments based on ActiveSegmentTags/Ids.
        /// The StartSegmentId is always included regardless of filters.
        /// </summary>
        private static void MergeRegistrySegments(
            TrackSegmentLibraryDefinition levelDef,
            string registryJson)
        {
            var registry = JsonUtility.FromJson<TrackSegmentRegistryDefinition>(registryJson);
            if (registry?.Segments == null || registry.Segments.Count == 0) return;

            bool filterByIds  = levelDef.ActiveSegmentIds  != null && levelDef.ActiveSegmentIds.Count  > 0;
            bool filterByTags = levelDef.ActiveSegmentTags != null && levelDef.ActiveSegmentTags.Count > 0;

            foreach (var seg in registry.Segments)
            {
                if (string.IsNullOrWhiteSpace(seg.Id)) continue;

                bool include;

                if (seg.Id == levelDef.StartSegmentId)
                {
                    include = true; // start segment is always included
                }
                else if (filterByIds)
                {
                    include = levelDef.ActiveSegmentIds.Contains(seg.Id);
                }
                else if (filterByTags)
                {
                    include = false;
                    if (seg.Tags != null)
                    {
                        foreach (var tag in seg.Tags)
                        {
                            if (levelDef.ActiveSegmentTags.Contains(tag))
                            {
                                include = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    include = true; // no filter: include every registry segment
                }

                if (include) levelDef.Segments.Add(seg);
            }
        }

        private static bool IsAllowed(
            TrackSegmentDefinition segment,
            string previousSegmentId,
            int previousRepeatCount)
        {
            if (segment == null) return false;

            if (!string.IsNullOrWhiteSpace(previousSegmentId) &&
                segment.Id == previousSegmentId &&
                segment.MaxRepeat > 0)
                return previousRepeatCount < segment.MaxRepeat;

            return true;
        }

        private static TrackSegmentDefinition SelectWeighted(
            List<TrackSegmentDefinition> candidates,
            System.Random random)
        {
            if (candidates.Count == 0) return null;

            float totalWeight = 0f;
            foreach (var c in candidates)
                totalWeight += Mathf.Max(0f, c.Weight);

            if (totalWeight <= 0f)
                return candidates[random.Next(candidates.Count)];

            float pick       = (float)random.NextDouble() * totalWeight;
            float cumulative = 0f;
            foreach (var c in candidates)
            {
                cumulative += Mathf.Max(0f, c.Weight);
                if (pick <= cumulative) return c;
            }

            return candidates[candidates.Count - 1];
        }
    }
}
