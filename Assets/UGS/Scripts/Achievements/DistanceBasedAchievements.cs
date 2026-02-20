//using Blocks.Achievements;

using Blocks.Achievements;

using CrawfisSoftware.UGS.Events;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace CrawfisSoftware.UGS.Achievements
{
    /// <summary>
    /// Unlocks achievements based on distance travelled.
    /// Receives distance updates via UGS events (bridged from TempleRun domain).
    ///    Dependencies: EventsPublisherUGS, AchievementsObserver
    ///    Subscribes: UGS_EventsEnum.DistanceUpdated
    ///    Publishes: (via AchievementsObserver)
    /// </summary>
    public class DistanceBasedAchievements : MonoBehaviour
    {
        [SerializeField] private List<float> _distances;
        private float _currentDistance = 0f;
        private int _nextAchievementIndex = 0;

        private void Awake()
        {
            // Ensure the AchievementsObserver is initialized
            var observer = AchievementsObserver.Instance;
            AchievementsObserver.Instance.AchievementUnlocked += (achievement) =>
            {
                Debug.Log($"Achievement Unlocked: {achievement.Id}");
            };

            // Subscribe to distance updates from the bridge
            EventsPublisherUGS.Instance.SubscribeToEvent(
                UGS_EventsEnum.DistanceUpdated,
                OnDistanceUpdated);
        }

        private void OnDestroy()
        {
            EventsPublisherUGS.Instance.UnsubscribeToEvent(
                UGS_EventsEnum.DistanceUpdated,
                OnDistanceUpdated);
        }

        private void OnDistanceUpdated(string eventName, object sender, object data)
        {
            if (data is float distance)
            {
                _currentDistance = distance;
                CheckAndUnlockAchievements();
            }
        }

        private void CheckAndUnlockAchievements()
        {
            while (_nextAchievementIndex < _distances.Count &&
                   _currentDistance > _distances[_nextAchievementIndex])
            {
                Debug.Log($"Distance Achievement reached at {_distances[_nextAchievementIndex]}");
                var ach = AchievementsObserver.Instance.RuntimeAchievementData.Achievements
                    .Find(a => a.Id == "first_achievement");
                StartCoroutine(UnlockAchievementAsync(ach.Id));
                _nextAchievementIndex++;
            }
        }

        private IEnumerator UnlockAchievementAsync(string achievement)
        {
            if (achievement != null)
            {
                yield return AchievementsObserver.Instance.UnlockAchievementAsync(achievement);
            }
        }
    }
}