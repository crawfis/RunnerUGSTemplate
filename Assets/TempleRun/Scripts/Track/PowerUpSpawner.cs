using CrawfisSoftware.TempleRun.GameConfig;

using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Spawns power-ups on track segments based on difficulty settings.
    /// Uses weighted random selection from a list of PowerUpDefinition ScriptableObjects.
    ///    Dependencies: Blackboard, DifficultyConfig, PowerUpDefinition[]
    ///    Subscribes: TempleRunEvents.SplineSegmentCreated (spawn power-ups on new segments)
    ///    Subscribes: TempleRunEvents.TeleportEnded (clean up old power-ups)
    /// </summary>
    internal class PowerUpSpawner : MonoBehaviour
    {
        [Header("Power-Up Definitions")]
        [Tooltip("Available power-up types. Weighted random selection chooses from this list.")]
        [SerializeField] private PowerUpDefinition[] _powerUpDefinitions;

        [Header("Spawn Settings")]
        [Tooltip("Minimum distance from segment start before a power-up can spawn.")]
        [SerializeField] private float _minDistanceFromSegmentStart = 2f;

        [Tooltip("Minimum distance from segment end before a power-up can spawn.")]
        [SerializeField] private float _minDistanceFromSegmentEnd = 2f;

        [Tooltip("Height above the track surface to place power-ups.")]
        [SerializeField] private float _platformHeight = 2.0f;

        private Transform _parentTransform;
        private readonly Dictionary<int, List<GameObject>> _powerUpsBySegment = new();
        private int _currentSegmentID = -1;
        private int _segmentNumber = 0;
        private System.Random _random;

        private void Awake()
        {
            EventsPublisherTempleRun.Instance.SubscribeToEvent(
                TempleRunEvents.SplineSegmentCreated, OnSplineSegmentCreated);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(
                TempleRunEvents.TeleportEnded, OnTeleportEnded);

            _parentTransform = new GameObject("Generated PowerUps").transform;
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
            if (_random == null || _powerUpDefinitions == null || _powerUpDefinitions.Length == 0) return;

            var (point1, point2, turnDirection) = ((Vector3, Vector3, Direction))data;

            float spawnRate = Blackboard.Instance.GameConfig.PowerUpSpawnRate;
            float roll = (float)_random.NextDouble();

            if (roll > spawnRate)
            {
                _segmentNumber++;
                return;
            }

            Vector3 segmentDirection = point2 - point1;
            float segmentLength = segmentDirection.magnitude;
            Vector3 unitDirection = segmentDirection.normalized;

            float usableLength = segmentLength - _minDistanceFromSegmentStart - _minDistanceFromSegmentEnd;
            if (usableLength <= 0f)
            {
                _segmentNumber++;
                return;
            }

            // Pick a random position along the usable portion
            float spawnT = (float)_random.NextDouble();
            float distanceAlongSegment = _minDistanceFromSegmentStart + spawnT * usableLength;
            Vector3 spawnPosition = point1 + unitDirection * distanceAlongSegment + _platformHeight * Vector3.up;

            // Pick a random lane
            LaneConfig laneConfig = Blackboard.Instance.LaneConfig;
            float laneWidth = laneConfig != null ? laneConfig.LaneWidth : 2f;
            int laneCount = laneConfig != null ? laneConfig.LaneCount : 3;
            int halfLanes = (laneCount - 1) / 2;
            int lane = _random.Next(-halfLanes, halfLanes + 1);

            Vector3 perpendicular = Vector3.Cross(unitDirection, Vector3.up).normalized;
            spawnPosition -= perpendicular * (lane * laneWidth);

            // Weighted random selection of power-up type
            PowerUpDefinition definition = SelectWeightedRandom();
            GameObject powerUp = SpawnPowerUp(spawnPosition, unitDirection, definition);

            if (powerUp != null)
            {
                if (!_powerUpsBySegment.ContainsKey(_segmentNumber))
                    _powerUpsBySegment[_segmentNumber] = new List<GameObject>();
                _powerUpsBySegment[_segmentNumber].Add(powerUp);
            }

            _segmentNumber++;
        }

        private PowerUpDefinition SelectWeightedRandom()
        {
            float totalWeight = 0f;
            foreach (var def in _powerUpDefinitions)
                totalWeight += def.SpawnWeight;

            float roll = (float)_random.NextDouble() * totalWeight;
            float cumulative = 0f;
            foreach (var def in _powerUpDefinitions)
            {
                cumulative += def.SpawnWeight;
                if (roll <= cumulative) return def;
            }
            return _powerUpDefinitions[^1];
        }

        private GameObject SpawnPowerUp(Vector3 position, Vector3 forward, PowerUpDefinition definition)
        {
            GameObject prefab = definition.Prefab;
            GameObject powerUp;

            Quaternion rotation = Quaternion.LookRotation(forward);

            if (prefab != null)
            {
                powerUp = Instantiate(prefab, position, rotation, _parentTransform);
            }
            else
            {
                powerUp = CreateDefaultPowerUpPrefab(definition.TintColor);
                powerUp.transform.SetParent(_parentTransform, false);
                powerUp.transform.localPosition = position;
                powerUp.transform.localRotation = rotation;
            }

            powerUp.name = $"PowerUp_{definition.PowerUpId}_{_segmentNumber}";
            powerUp.tag = "PowerUp";

            // Ensure the power-up has an identifier component
            var identifier = powerUp.GetComponent<PowerUpIdentifier>();
            if (identifier == null)
            {
                identifier = powerUp.AddComponent<PowerUpIdentifier>();
            }
            identifier.Definition = definition;

            return powerUp;
        }

        /// <summary>
        /// Creates a default power-up (tinted capsule with trigger collider) when no custom prefab is assigned.
        /// </summary>
        private GameObject CreateDefaultPowerUpPrefab(Color tintColor)
        {
            GameObject powerUp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            powerUp.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

            Collider defaultCollider = powerUp.GetComponent<Collider>();
            if (defaultCollider != null)
                defaultCollider.isTrigger = true;

            Renderer renderer = powerUp.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = tintColor;
            }

            return powerUp;
        }

        private void OnTeleportEnded(string eventName, object sender, object data)
        {
            if (_currentSegmentID >= 0 && _powerUpsBySegment.TryGetValue(_currentSegmentID, out var powerUps))
            {
                foreach (var powerUp in powerUps)
                {
                    if (powerUp != null)
                        Destroy(powerUp);
                }
                _powerUpsBySegment.Remove(_currentSegmentID);
            }
            _currentSegmentID++;
        }
    }
}
