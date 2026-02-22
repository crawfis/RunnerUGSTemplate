using CrawfisSoftware.TempleRun.GameConfig;

using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Spawns lines of coins on track segments based on difficulty settings.
    /// Listens for new spline segments and places coin lines at random positions and lanes.
    ///    Dependencies: Blackboard, DifficultyConfig, CoinConfig, LaneConfig
    ///    Subscribes: TempleRunEvents.SplineSegmentCreated (spawn coins on new segments)
    ///    Subscribes: TempleRunEvents.TeleportEnded (clean up old coins)
    /// </summary>
    internal class CoinSpawner : MonoBehaviour
    {
        [Header("Coin Config")]
        [SerializeField] private CoinConfig _coinConfig;

        private Transform _parentTransform;
        private readonly Dictionary<int, List<GameObject>> _coinsBySegment = new();
        private int _currentSegmentID = -1;
        private int _segmentNumber = 0;
        private System.Random _random;

        private void Awake()
        {
            EventsPublisherTempleRun.Instance.SubscribeToEvent(
                TempleRunEvents.SplineSegmentCreated, OnSplineSegmentCreated);
            EventsPublisherTempleRun.Instance.SubscribeToEvent(
                TempleRunEvents.TeleportEnded, OnTeleportEnded);

            _parentTransform = new GameObject("Generated Coins").transform;
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
            if (_random == null || _coinConfig == null) return;

            var (point1, point2, turnDirection) = ((Vector3, Vector3, Direction))data;

            float spawnRate = Blackboard.Instance.GameConfig.CoinSpawnRate;
            float roll = (float)_random.NextDouble();

            if (roll > spawnRate)
            {
                _segmentNumber++;
                return;
            }

            Vector3 segmentDirection = point2 - point1;
            float segmentLength = segmentDirection.magnitude;
            Vector3 unitDirection = segmentDirection.normalized;

            float minStart = _coinConfig.MinDistanceFromSegmentStart;
            float minEnd = _coinConfig.MinDistanceFromSegmentEnd;

            // Determine how many coins to spawn in this line
            int coinCount = _random.Next(_coinConfig.MinCoinsPerLine, _coinConfig.MaxCoinsPerLine + 1);
            float totalLineLength = (coinCount - 1) * _coinConfig.SpacingBetweenCoins;

            // Ensure the line fits within the usable segment length
            float usableLength = segmentLength - minStart - minEnd;
            if (usableLength <= 0f || totalLineLength > usableLength)
            {
                // Segment too short for a coin line — reduce coin count to fit
                coinCount = Mathf.Max(1, Mathf.FloorToInt(usableLength / _coinConfig.SpacingBetweenCoins) + 1);
                totalLineLength = (coinCount - 1) * _coinConfig.SpacingBetweenCoins;
            }

            if (usableLength <= 0f)
            {
                _segmentNumber++;
                return;
            }

            // Pick a random start position along the usable portion
            float maxStartDistance = usableLength - totalLineLength;
            float startDistance = minStart + (float)_random.NextDouble() * Mathf.Max(0f, maxStartDistance);

            // Pick a random lane for the entire coin line
            LaneConfig laneConfig = Blackboard.Instance.LaneConfig;
            float laneWidth = laneConfig != null ? laneConfig.LaneWidth : 2f;
            int laneCount = laneConfig != null ? laneConfig.LaneCount : 3;
            int halfLanes = (laneCount - 1) / 2;
            int lane = _random.Next(-halfLanes, halfLanes + 1);

            Vector3 perpendicular = Vector3.Cross(unitDirection, Vector3.up).normalized;
            Vector3 laneOffset = -perpendicular * (lane * laneWidth);

            var coinsInSegment = new List<GameObject>(coinCount);

            for (int i = 0; i < coinCount; i++)
            {
                float distanceAlongSegment = startDistance + i * _coinConfig.SpacingBetweenCoins;
                Vector3 spawnPosition = point1
                    + unitDirection * distanceAlongSegment
                    + _coinConfig.PlatformHeight * Vector3.up
                    + laneOffset;

                GameObject coin = SpawnCoin(spawnPosition, unitDirection);
                if (coin != null)
                {
                    coinsInSegment.Add(coin);
                }
            }

            if (coinsInSegment.Count > 0)
            {
                _coinsBySegment[_segmentNumber] = coinsInSegment;
            }

            _segmentNumber++;
        }

        private GameObject SpawnCoin(Vector3 position, Vector3 forward)
        {
            GameObject prefab = _coinConfig.CoinPrefab;
            GameObject coin;

            if (prefab != null)
            {
                Quaternion rotation = Quaternion.LookRotation(forward);
                coin = Instantiate(prefab, position, rotation, _parentTransform);
            }
            else
            {
                coin = CreateDefaultCoinPrefab();
                coin.transform.SetParent(_parentTransform, false);
                coin.transform.localPosition = position;
            }

            coin.name = $"Coin_{_segmentNumber}";
            coin.tag = "Coin";
            return coin;
        }

        /// <summary>
        /// Creates a default coin (yellow sphere with trigger collider) when no custom prefab is assigned.
        /// </summary>
        private GameObject CreateDefaultCoinPrefab()
        {
            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coin.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            Collider defaultCollider = coin.GetComponent<Collider>();
            if (defaultCollider != null)
                defaultCollider.isTrigger = true;

            Renderer renderer = coin.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.yellow;
            }

            return coin;
        }

        private void OnTeleportEnded(string eventName, object sender, object data)
        {
            if (_currentSegmentID >= 0 && _coinsBySegment.TryGetValue(_currentSegmentID, out var coins))
            {
                foreach (var coin in coins)
                {
                    if (coin != null)
                        Destroy(coin);
                }
                _coinsBySegment.Remove(_currentSegmentID);
            }
            _currentSegmentID++;
        }
    }
}
