using CrawfisSoftware.TempleRun.GameConfig;

using UnityEngine;

namespace CrawfisSoftware.TempleRun
{
    /// <summary>
    /// Detects collisions between the player and obstacles using Unity trigger colliders.
    /// If the player is jumping high enough, the collision is ignored (obstacle cleared).
    /// Otherwise publishes ObstacleHit which auto-chains to PlayerFailing.
    ///    Dependencies: Blackboard, JumpConfig
    ///    Publishes: TempleRunEvents.ObstacleHit
    /// </summary>
    [RequireComponent(typeof(Collider))]
    internal class ObstacleCollisionDetector : MonoBehaviour
    {
        [Tooltip("Tag used to identify obstacle GameObjects. Must match the tag set on obstacle prefabs.")]
        [SerializeField] private string _obstacleTag = "Obstacle";

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(_obstacleTag)) return;

            // Check if the player is jumping high enough to clear the obstacle
            float currentJumpHeight = Blackboard.Instance.JumpHeightOffset;
            JumpConfig jumpConfig = Blackboard.Instance.JumpConfig;
            float clearanceHeight = jumpConfig != null ? jumpConfig.ObstacleClearanceHeight : 1f;

            if (currentJumpHeight >= clearanceHeight)
            {
                // Player cleared the obstacle — no collision
                return;
            }

            // Player hit the obstacle
            EventsPublisherTempleRun.Instance.PublishEvent(
                TempleRunEvents.ObstacleHit, this, other.gameObject);
        }
    }
}
