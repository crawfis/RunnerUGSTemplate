namespace CrawfisSoftware.TempleRun
{
    public enum TempleRunEvents
    {
        // ---------- Player lifecycle ----------
        PlayerFailRequested = 0,
        PlayerFailing = 1,
        PlayerFailed = 2,
        PlayerDeathRequested = 3,
        PlayerDying = 4,
        PlayerDied = 5,
        PlayerReviveRequested = 6,
        PlayerReviving = 7,
        PlayerRevived = 8,

        // ---------- Player pause / resume ----------
        PlayerPauseRequested = 20,
        PlayerPausing = 21,
        PlayerPaused = 22,
        PlayerResumeRequested = 23,
        PlayerResuming = 24,
        PlayerResumed = 25,
        //PlayerPause = PlayerPaused, // Legacy naming
        //PlayerResume = PlayerResumed, // Legacy naming

        // ---------- Countdown ----------
        CountdownStartRequested = 30,
        CountdownStarting = 31,
        CountdownStarted = 32,
        CountdownTick = 33,
        CountdownEnding = 34,
        CountdownEnded = 35,
        CountdownCancelled = 36,

        // ---------- Game start lifecycle (TempleRun domain) ----------
        TempleRunStartRequested = 38,
        TempleRunStarting = 39,
        TempleRunStarted = 40,

        // ---------- Player movement: turning ----------
        TurnLeftRequested = 40,
        TurnLeftStarting = 41,
        TurnLeftCompleted = 42,
        TurnRightRequested = 43,
        TurnRightStarting = 44,
        TurnRightCompleted = 45,
        //LeftTurnSucceeded = TurnLeftCompleted, // Legacy naming
        //RightTurnSucceeded = TurnRightCompleted, // Legacy naming

        // ---------- Player movement: slide ----------
        SlideRequested = 60,
        SlideStarting = 61,
        SlideStarted = 62,
        SlideEndRequested = 63,
        SlideEnding = 64,
        SlideEnded = 65,

        // ---------- Player movement: jump ----------
        JumpRequested = 80,
        JumpStarting = 81,
        JumpStarted = 82,
        JumpEndRequested = 83,
        JumpEnding = 84,
        JumpLanded = 85,

        // ---------- Player movement: lane change ----------
        LaneChangeLeftRequested = 100,
        LaneChangingLeft = 101,
        LaneChangedLeft = 102,
        LaneChangeRightRequested = 103,
        LaneChangingRight = 104,
        LaneChangedRight = 105,

        // ---------- Player hazards / collisions ----------
        ObstacleHit = 120,
        ObstacleRecoveryRequested = 121,
        ObstacleRecovering = 122,
        ObstacleRecovered = 123,

        // ---------- Player interaction: coins / power-ups ----------
        CoinCollectRequested = 140,
        CoinCollecting = 141,
        CoinCollected = 142,

        PowerUpCollectRequested = 160,
        PowerUpCollecting = 161,
        PowerUpCollected = 162,

        PowerUpActivateRequested = 180,
        PowerUpActivating = 181,
        PowerUpActivated = 182,
        PowerUpDeactivateRequested = 183,
        PowerUpDeactivating = 184,
        PowerUpDeactivated = 185,

        // ---------- Abstract track generation (splines) ----------
        SplineSegmentCreateRequested = 200,
        SplineSegmentCreating = 201,
        SplineSegmentCreated = 202,
        SplineSegmentReleaseRequested = 203,
        SplineSegmentReleasing = 204,
        SplineSegmentReleased = 205,

        CurrentSplineChangeRequested = 220,
        CurrentSplineChanging = 221,
        CurrentSplineChanged = 222,

        // ---------- Track generation (segments/tiles) ----------
        TrackSegmentCreateRequested = 240,
        TrackSegmentCreating = 241,
        TrackSegmentCreated = 242,
        TrackSegmentRecycleRequested = 243,
        TrackSegmentRecycling = 244,
        TrackSegmentRecycled = 245,

        ActiveTrackChangeRequested = 260,
        ActiveTrackChanging = 261,
        ActiveTrackChanged = 262,

        // ---------- Teleportation ----------
        TeleportRequested = 280,
        TeleportStarting = 281,
        TeleportStarted = 282,
        TeleportEndRequested = 283,
        TeleportEnding = 284,
        TeleportEnded = 285,
    }
}