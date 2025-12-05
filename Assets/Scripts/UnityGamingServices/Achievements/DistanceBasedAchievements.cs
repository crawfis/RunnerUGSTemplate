//using Blocks.Achievements;

using Blocks.Achievements;

using CrawfisSoftware.TempleRun;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class DistanceBasedAchievements : MonoBehaviour
{
    [SerializeField] private List<float> _distances;
    private void Awake()
    {
        // Ensure the AchievementsObserver is initialized
        var observer = AchievementsObserver.Instance;
        AchievementsObserver.Instance.AchievementUnlocked += (achievement) =>
        {
            Debug.Log($"Achievement Unlocked: {achievement.Id}");
        };
        //_ = observer.ResetAchievementsAsync();
    }
    private void Start()
    {
        StartCoroutine(ClaimAchievements());
    }

    private IEnumerator ClaimAchievements()
    {
        DistanceTracker _distanceTracker = Blackboard.Instance.DistanceTracker;
        int index = 0;
        //float distance = _distances[index];
        while (index < _distances.Count)
        {
            if (Blackboard.Instance.DistanceTracker.DistanceTravelled > _distances[index])
            {
                Debug.Log($"Distance Achievement reached at {_distances[index]}");
                index++;
                //distance = _distances[index];
                var ach = AchievementsObserver.Instance.RuntimeAchievementData.Achievements
                    .Find(a => a.Id == "first_achievement");
                yield return AchievementsObserver.Instance.UnlockAchievementAsync(ach);
            }
            yield return null;
        }
    }
}