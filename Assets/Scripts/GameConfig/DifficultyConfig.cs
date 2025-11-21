using System;

using UnityEngine;

namespace CrawfisSoftware.GameConfig
{
    [Serializable]
    public class DifficultyConfig
    {
        public string DifficultyName = "Medium";

        [Header("Movement Settings")]
        public float InitialSpeed = 5f;
        public float MaxSpeed = 80f;
        public float Acceleration = 0.2f;

        [Header("Level Generation")]
        public int StartRunway = 8;
        public int MinTrackLength = 4;
        public int MaxTrackLength = 16;
        //public int TileSetId = TileSet.Default.Id;

        [Header("Game Mechanics")]
        public float SafePreTurnDistance = 5f; // Maximum distance bfore a turn target that a turn is valid
        public float SafePostTurnDistance = 0.5f; // Maximum distance after a turn target that a turn is valid
        public float InputCoolDownForTurns = 1f; // Minimum time between turn requests. Hopefully less than MinTrackLength / MaxSpeed
        public int NumberOfLives = 2;

        [Header("Additional Difficulty Modifiers")]
        public float ObstacleSpawnRate = 1f;
        public float PowerUpSpawnRate = 1f;
        public int ScoreMultiplier = 1;
        public bool EnableAdvancedObstacles = false;
    }
}