namespace CrawfisSoftware.TempleRun
{
    public enum TempleRunEvents
    {
        LeftTurnSucceeded, RightTurnSucceeded,
        PlayerFailing, PlayerFailed, PlayerDying, PlayerDied,
        SplineSegmentCreated, CurrentSplineChanging, CurrentSplineChanged,
        ActiveTrackChanging, TrackSegmentCreated,
        TeleportStarted, TeleportEnded,

        // Currently not used
        PlayerCollectedCoin, PlayerCollectedPowerUp,
        PowerUpActivating, PowerUpActivated, PowerUpDeactivating, PowerUpDeactivated,
        PlayerJumped, PlayerLanded,
        ObstacleHit,
        SlideStarted, SlideEnded,
        PlayerPause, PlayerResume,
    }
}