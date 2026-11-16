using System.Collections.Generic;
using NUnit.Framework;
using StealthController.Core;
using UnityEngine;

public class GuardLogicTests
{
    private const float FIXED_DELTA = 0.02f;
    private const float WALK_DURATION = 6f;
    private const float CHASE_DURATION = 3f;
    private const float ARRIVAL_TOLERANCE = 0.05f;
    private const float SEARCH_MARGIN = 0.2f;
    private const float NO_WAIT = 0f;

    private static readonly Vector3 FarAway = new Vector3(0f, 0f, -100f);

    private GuardConfig config;
    private PlayerMovementConfig playerConfig;

    [SetUp]
    public void SetUp()
    {
        config = new GuardConfig();
        playerConfig = new PlayerMovementConfig();
    }

    [Test]
    public void Patrol_WalksToTheNextWaypoint()
    {
        GuardLogic guard = CreateGuard(new StraightLinePathfinderFake(), new Vector3(5f, 0f, 0f));

        Simulate(guard, Standing(FarAway), WALK_DURATION);

        Assert.AreEqual(GuardAlertState.Patrol, guard.AlertState);
        Assert.AreEqual(5f, guard.Position.x, ARRIVAL_TOLERANCE);
    }

    [Test]
    public void SeeingThePlayer_StartsChase()
    {
        GuardLogic guard = CreateGuard(new StraightLinePathfinderFake(), Vector3.zero);

        Simulate(guard, Standing(new Vector3(0f, 0f, 6f)), config.Vision.ConfirmDuration + SEARCH_MARGIN);

        Assert.AreEqual(GuardAlertState.Chase, guard.AlertState);
    }

    [Test]
    public void ChasingAVisiblePlayer_CapturesWithinCaptureDistance()
    {
        GuardLogic guard = CreateGuard(new StraightLinePathfinderFake(), Vector3.zero);

        Simulate(guard, Standing(new Vector3(0f, 0f, 6f)), CHASE_DURATION);

        Assert.IsTrue(guard.HasCapturedPlayer);
    }

    [Test]
    public void LosingThePlayer_SearchesThenReturnsToPatrol()
    {
        GuardLogic guard = CreateGuard(new StraightLinePathfinderFake(), Vector3.zero);
        Vector3 seenAt = new Vector3(0f, 0f, 6f);

        Simulate(guard, Standing(seenAt), config.Vision.ConfirmDuration + SEARCH_MARGIN);
        Simulate(guard, Standing(FarAway), CHASE_DURATION);

        Assert.AreEqual(GuardAlertState.Chase, guard.AlertState);
        Assert.IsTrue(guard.IsSearching);
        Assert.AreEqual(seenAt.z, guard.Position.z, ARRIVAL_TOLERANCE);

        Simulate(guard, Standing(FarAway), config.SearchDuration);

        Assert.AreEqual(GuardAlertState.Patrol, guard.AlertState);
        Assert.IsFalse(guard.HasCapturedPlayer);
    }

    [Test]
    public void UnreachableTarget_SearchesInPlace()
    {
        GuardLogic guard = CreateGuard(new StraightLinePathfinderFake().Blocked(), Vector3.zero);

        guard.Pursue(new Vector3(0f, 0f, 6f));
        guard.Tick(FIXED_DELTA, Standing(FarAway));

        Assert.IsTrue(guard.IsSearching);
        Assert.AreEqual(Vector3.zero, guard.Position);
    }

    [Test]
    public void Alert_SendsGuardTowardsKnownPosition()
    {
        GuardLogic guard = CreateGuard(new StraightLinePathfinderFake(), Vector3.zero);
        Vector3 alertPosition = new Vector3(4f, 0f, -4f);

        guard.Pursue(alertPosition);
        Simulate(guard, Standing(FarAway), CHASE_DURATION);

        Assert.AreEqual(GuardAlertState.Chase, guard.AlertState);
        Assert.AreEqual(alertPosition.x, guard.Position.x, ARRIVAL_TOLERANCE);
        Assert.AreEqual(alertPosition.z, guard.Position.z, ARRIVAL_TOLERANCE);
    }

    private GuardLogic CreateGuard(IPathfinder pathfinder, params Vector3[] routePoints)
    {
        List<GuardWaypoint> route = new List<GuardWaypoint>();

        foreach (Vector3 point in routePoints)
            route.Add(new GuardWaypoint(point, NO_WAIT, false, 0f));

        return new GuardLogic(config, route, Vector3.zero, 0f, new PlaneCollisionWorldFake(), pathfinder);
    }

    private void Simulate(GuardLogic guard, PlayerPose player, float seconds)
    {
        int steps = Mathf.CeilToInt(seconds / FIXED_DELTA);

        for (int i = 0; i < steps && !guard.HasCapturedPlayer; i++)
            guard.Tick(FIXED_DELTA, player);
    }

    private PlayerPose Standing(Vector3 position)
    {
        return new PlayerPose(position, Quaternion.identity, PlayerPoseState.Standing, 0f, 1f, playerConfig.StandingRadius, playerConfig.StandingHeight, Vector3.zero);
    }
}
