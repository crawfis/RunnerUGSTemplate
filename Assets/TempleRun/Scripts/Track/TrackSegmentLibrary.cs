using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    [Serializable]
    public class TrackSegmentDefinition
    {
        public string Id;
        public string Direction;
        public float Length = 5f;
        public float Weight = 1f;
        public int MaxRepeat = 0;
    }

    [Serializable]
    public class TrackSegmentConnection
    {
        public string FromId;
        public string ToId;
    }

    [Serializable]
    public class TrackSegmentLibraryDefinition
    {
        public string Version;
        public string StartSegmentId;
        public List<TrackSegmentDefinition> Segments = new();
        public List<TrackSegmentConnection> Connections = new();
    }

    public class TrackSegmentLibrary
    {
        private readonly TrackSegmentLibraryDefinition _definition;
        private readonly Dictionary<string, TrackSegmentDefinition> _segmentsById = new();
        private readonly Dictionary<string, List<string>> _connectionsByFromId = new();

        public TrackSegmentLibrary(TrackSegmentLibraryDefinition definition)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            foreach (var segment in _definition.Segments)
            {
                if (!string.IsNullOrWhiteSpace(segment.Id))
                {
                    _segmentsById[segment.Id] = segment;
                }
            }

            foreach (var connection in _definition.Connections)
            {
                if (string.IsNullOrWhiteSpace(connection.FromId) || string.IsNullOrWhiteSpace(connection.ToId))
                {
                    continue;
                }

                if (!_connectionsByFromId.TryGetValue(connection.FromId, out var list))
                {
                    list = new List<string>();
                    _connectionsByFromId[connection.FromId] = list;
                }

                list.Add(connection.ToId);
            }
        }

        public static TrackSegmentLibrary LoadFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var definition = JsonUtility.FromJson<TrackSegmentLibraryDefinition>(json);
            if (definition == null)
            {
                return null;
            }

            return new TrackSegmentLibrary(definition);
        }

        public TrackSegmentDefinition GetStartSegment(System.Random random)
        {
            if (!string.IsNullOrWhiteSpace(_definition.StartSegmentId) && _segmentsById.TryGetValue(_definition.StartSegmentId, out var segment))
            {
                return segment;
            }

            return SelectNext(null, 0, random);
        }

        public TrackSegmentDefinition SelectNext(string previousSegmentId, int previousRepeatCount, System.Random random)
        {
            var candidates = new List<TrackSegmentDefinition>();

            if (!string.IsNullOrWhiteSpace(previousSegmentId) && _connectionsByFromId.TryGetValue(previousSegmentId, out var allowedIds))
            {
                foreach (var id in allowedIds)
                {
                    if (_segmentsById.TryGetValue(id, out var segment) && IsAllowed(segment, previousSegmentId, previousRepeatCount))
                    {
                        candidates.Add(segment);
                    }
                }
            }
            else
            {
                foreach (var segment in _definition.Segments)
                {
                    if (IsAllowed(segment, previousSegmentId, previousRepeatCount))
                    {
                        candidates.Add(segment);
                    }
                }
            }

            if (candidates.Count == 0)
            {
                candidates.AddRange(_definition.Segments);
            }

            return SelectWeighted(candidates, random);
        }

        private static bool IsAllowed(TrackSegmentDefinition segment, string previousSegmentId, int previousRepeatCount)
        {
            if (segment == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(previousSegmentId) && segment.Id == previousSegmentId && segment.MaxRepeat > 0)
            {
                return previousRepeatCount < segment.MaxRepeat;
            }

            return true;
        }

        private static TrackSegmentDefinition SelectWeighted(List<TrackSegmentDefinition> candidates, System.Random random)
        {
            if (candidates.Count == 0)
            {
                return null;
            }

            float totalWeight = 0f;
            foreach (var candidate in candidates)
            {
                totalWeight += Mathf.Max(0f, candidate.Weight);
            }

            if (totalWeight <= 0f)
            {
                return candidates[random.Next(candidates.Count)];
            }

            float pick = (float)random.NextDouble() * totalWeight;
            float cumulative = 0f;
            foreach (var candidate in candidates)
            {
                cumulative += Mathf.Max(0f, candidate.Weight);
                if (pick <= cumulative)
                {
                    return candidate;
                }
            }

            return candidates[candidates.Count - 1];
        }
    }
}
