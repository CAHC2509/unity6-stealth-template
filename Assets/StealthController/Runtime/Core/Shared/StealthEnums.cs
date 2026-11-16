namespace StealthController.Core
{
    public enum PlayerPoseState
    {
        Standing,
        Crouched,
        Crawling,
        WallStanding,
        Takedown
    }

    public enum GuardAlertState
    {
        Patrol,
        Chase,
        TakenDown
    }

    public enum StealthSessionOutcome
    {
        None,
        Captured,
        MissionCompleted
    }
}
