using CrawfisSoftware.TempleRun.GameConfig;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Spawns obstacles on track segments based on difficulty settings.
    /// Listens for new spline segments and randomly places full-width or lane-specific barriers.
    ///    Dependencies: Blackboard, DifficultyConfig, LaneConfig
    ///    Subscribes: TempleRunEvents.SplineSegmentCreated (spawn obstacles on new segments)
    ///    Subscribes: TempleRunEvents.TeleportEnded (clean up old obstacles)
    ///    Subscribes: TempleRunEvents.TempleRunStarted (reset state)
    /// </summary>
    internal class ObstacleSpawner : MonoBehaviour
    {
        [Header("Obstacle Prefabs")]
        [Tooltip("Obstacle that spans the full track width — player must jump to clear it.")]
        [SerializeField] private GameObject _fullWidthObstaclePrefab;

        [Tooltip("Obstacle that blocks a single lane — player can jump or lane-change to avoid.")]
        [SerializeField] private GameObject _laneObstaclePrefab;

        [Header("Spawn Settings")]
        [Tooltip("Minimum distance from segment start before an obstacle can spawn (prevents spawning at turn points).")]
        [SerializeField] private float _minDistanceFromSegmentStart = 2f;

        [Tooltip("Minimum distance from segment end before an obstacle can spawn.")]
        [SerializeField] private float _minDistanceFromSegmentEnd = 2f;

        [Tooltip("Probability (0-1) that a spawned obstacle is full-width rather than lane-specific.")]
        [SerializeField] [Range(0f, 1f)] private float _fullWidthProbability = 0.3f;

        [Header("Obstacle Dimensions")]
        [Tooltip("Height of obstacle colliders (Y-axis). Should be less than jump clearance height.")]
        [SerializeField] private float _obstacleHeight = 0.5f;

        [Tooltip("Depth of obstacle colliders (Z-axis along track).")]
        [SerializeField] private float _obstacleDepth = 0.5f;

        [Tooltip("Initial height the obstacle should be placed.")]
        [SerializeField] private float _platformHeight = 1.5f;

        private Transform _parentTransform;
        private readonly Dictionary<int, List<GameObject>> _obstaclesBySegment = new();
        private int _currentSegmentID = -1;
        private int _segmentNumber = 0;
        private System.Random _random;

        private void Awake()
        {
            EventsPublisherTempleRun.Instance.SubscribeToEvent(
                TempleRunEvents.SplineSegmentCreated, OnSplineSegmentCreated);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(
                TempleRunEvents.TeleportEnded, OnTeleportEnded);

            _parentTransform = new GameObject("Generated Obstacles").transform;
            _random = new System.Random(Blackboard.Instance.MasterRandom.Next());
        }

        private void OnDestroy()
        {
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(
                TempleRunEvents.SplineSegmentCreated, OnSplineSegmentCreated);
            EventsPublisherTempleRun.Instance.UnsubscribeToEvent(
                TempleRunEvents.TeleportEnded, OnTeleportEnded);
        }

        private void OnSplineSegmentCreated(string eventName, object sender, object data)
        {
            if (_random == null) return;

            var (point1, point2, turnDirection) = ((Vector3, Vector3, Direction))data;

            float spawnRate = Blackboard.Instance.GameConfig.ObstacleSpawnRate;
            float roll = (float)_random.NextDouble();

            // Skip spawning if the roll exceeds the spawn rate
            if (roll > spawnRate)
            {
                _segmentNumber++;
                return;
            }

            Vector3 segmentDirection = (point2 - point1);
            float segmentLength = segmentDirection.magnitude;
            Vector3 unitDirection = segmentDirection.normalized;

            // Only spawn if the segment is long enough to have safe margins
            float usableLength = segmentLength - _minDistanceFromSegmentStart - _minDistanceFromSegmentEnd;
            if (usableLength <= 0f)
            {
                _segmentNumber++;
                return;
            }

            // Pick a random position along the usable portion of the segment
            float spawnT = (float)_random.NextDouble();
            float distanceAlongSegment = _minDistanceFromSegmentStart + spawnT * usableLength;
            Vector3 spawnPosition = point1 + unitDirection * distanceAlongSegment + _platformHeight * Vector3.up;

            // Determine obstacle type
            bool isFullWidth = (float)_random.NextDouble() < _fullWidthProbability;

            GameObject obstacle;
            if (isFullWidth)
            {
                obstacle = SpawnFullWidthObstacle(spawnPosition, unitDirection);
            }
            else
            {
                obstacle = SpawnLaneObstacle(spawnPosition, unitDirection);
            }

            if (obstacle != null)
            {
                if (!_obstaclesBySegment.ContainsKey(_segmentNumber))
                    _obstaclesBySegment[_segmentNumber] = new List<GameObject>();
                _obstaclesBySegment[_segmentNumber].Add(obstacle);
            }

            _segmentNumber++;
        }

        private GameObject SpawnFullWidthObstacle(Vector3 position, Vector3 forward)
        {
            GameObject prefab = _fullWidthObstaclePrefab;
            if (prefab == null)
            {
                // Create a primitive if no prefab assigned
                prefab = CreateDefaultObstaclePrefab(GetFullTrackWidth(), _obstacleHeight, _obstacleDepth);
            }

            Quaternion rotation = Quaternion.LookRotation(forward);
            GameObject obstacle = Instantiate(prefab, position, rotation, _parentTransform);
            obstacle.name = $"FullWidthBarrier_{_segmentNumber}";

            // Scale to span the full track width
            if (_fullWidthObstaclePrefab == null)
            {
                // Only set scale for default primitives; user prefabs handle their own scale
                Destroy(prefab);
            }

            return obstacle;
        }

        private GameObject SpawnLaneObstacle(Vector3 position, Vector3 forward)
        {
            GameObject prefab = _laneObstaclePrefab;
            LaneConfig laneConfig = Blackboard.Instance.LaneConfig;
            float laneWidth = laneConfig != null ? laneConfig.LaneWidth : 2f;
            int laneCount = laneConfig != null ? laneConfig.LaneCount : 3;
            int halfLanes = (laneCount - 1) / 2;

            // Pick a random lane
            int lane = _random.Next(-halfLanes, halfLanes + 1);

            // Offset position to the chosen lane
            Vector3 perpendicular = Vector3.Cross(forward, Vector3.up).normalized;
            Vector3 lanePosition = position - perpendicular * (lane * laneWidth);

            if (prefab == null)
            {
                prefab = CreateDefaultObstaclePrefab(laneWidth * 0.8f, _obstacleHeight, _obstacleDepth);
            }

            Quaternion rotation = Quaternion.LookRotation(forward);
            GameObject obstacle = Instantiate(prefab, lanePosition, rotation, _parentTransform);
            obstacle.name = $"LaneBarrier_{_segmentNumber}_Lane{lane}";

            if (_laneObstaclePrefab == null)
            {
                Destroy(prefab);
            }

            return obstacle;
        }

        private float GetFullTrackWidth()
        {
            LaneConfig laneConfig = Blackboard.Instance.LaneConfig;
            if (laneConfig != null)
                return laneConfig.LaneWidth * laneConfig.LaneCount;
            return 6f; // Default: 3 lanes × 2 units
        }

        /// <summary>
        /// Creates a default obstacle prefab (cube with trigger collider) when no custom prefab is assigned.
        /// </summary>
        private GameObject CreateDefaultObstaclePrefab(float width, float height, float depth)
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.transform.localScale = new Vector3(width, height, depth);

            // Replace the default collider with a trigger collider
            Collider defaultCollider = obstacle.GetComponent<Collider>();
            if (defaultCollider != null)
                defaultCollider.isTrigger = true;

            // Tag it so the collision detector can identify it
            obstacle.tag = "Obstacle";

            // Tint it red for visibility
            Renderer renderer = obstacle.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }

            return obstacle;
        }

        private void OnTeleportEnded(string eventName, object sender, object data)
        {
            // Clean up obstacles from the previous segment
            if (_currentSegmentID >= 0 && _obstaclesBySegment.TryGetValue(_currentSegmentID, out var obstacles))
            {
                foreach (var obstacle in obstacles)
                {
                    if (obstacle != null)
                        Destroy(obstacle);
                }
                _obstaclesBySegment.Remove(_currentSegmentID);
            }
            _currentSegmentID++;
        }
    }
}
