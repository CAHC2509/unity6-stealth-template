using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using StealthController.Core;
using StealthController.Unity;
using UnityEngine;

public class StealthGoldenMasterTests
{
    private const float FIXED_DELTA = 0.02f;
    private const int ENVIRONMENT_LAYER = 31;
    private const int PROBE_LAYER = 2;
    private const int RECORD_EVERY_FRAMES = 5;
    private const float VALUE_TOLERANCE = 0.001f;
    private const float ALERT_RADIUS = 30f;
    private const float WAYPOINT_WAIT = 1f;
    private const float WAYPOINT_LOOK_YAW = 180f;
    private const float GUARD_SPAWN_YAW = 270f;
    private const float CAMERA_BASE_YAW = -90f;
    private const float SETTLE_DURATION = 0.5f;
    private const float SHORT_PAUSE_DURATION = 0.3f;
    private const float POSTURE_SETTLE_DURATION = 0.4f;
    private const float STAND_UP_DURATION = 0.6f;
    private const float HOLD_MARGIN = 0.1f;
    private const float WALL_APPROACH_DURATION = 3f;
    private const float WALL_SHUFFLE_DURATION = 2.5f;
    private const float CORNER_PUSH_DURATION = 0.5f;
    private const float CORNER_RETURN_DURATION = 1f;
    private const float WALL_LEAVE_DURATION = 0.6f;
    private const float FIRST_SPRINT_DURATION = 1.2f;
    private const float CROUCHED_WALK_DURATION = 1f;
    private const float COVER_APPROACH_DURATION = 1.2f;
    private const float STEP_APPROACH_DURATION = 1.5f;
    private const float FIRST_CRAWL_DURATION = 2.5f;
    private const float SPRINT_FROM_PRONE_DURATION = 0.6f;
    private const float CHECKPOINT_APPROACH_DURATION = 3f;
    private const float BEAM_APPROACH_DURATION = 5f;
    private const float BEAM_CRAWL_DURATION = 6f;
    private const float EXIT_SPRINT_DURATION = 3f;
    private const float RETURN_DURATION = 4f;
    private const float CAPTURE_APPROACH_DURATION = 6f;
    private const float FINAL_IDLE_DURATION = 14f;
    private const string CAPTURED_LABEL = "Captured";
    private const string EXIT_LABEL = "Exit";
    private const string NUMBER_FORMAT = "F4";
    private const string TRACE_RELATIVE_PATH = "_ProjectName/Tests/Editor/Stealth/GoldenMaster/StealthGoldenMasterTrace.txt";

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private readonly List<PlayerInputFrame> inputs = new List<PlayerInputFrame>();
    private readonly StringBuilder trace = new StringBuilder();

    private UnityCollisionWorldAdapter playerWorld;
    private UnityCollisionWorldAdapter visionWorld;
    private PlayerMovementConfig playerConfig;

    [SetUp]
    public void SetUp()
    {
        playerConfig = new PlayerMovementConfig();

        SpawnBox(new Vector3(0f, -0.1f, 0f), new Vector3(60f, 0.2f, 60f));
        SpawnBox(new Vector3(0.5f, 1f, 3.25f), new Vector3(3f, 2f, 0.5f));
        SpawnBox(new Vector3(-8f, 0.75f, 0f), new Vector3(2f, 0.3f, 30f));
        SpawnBox(new Vector3(6f, 0.5f, -3f), new Vector3(3f, 1f, 0.6f));
        SpawnBox(new Vector3(4f, 0.125f, -8f), new Vector3(2f, 0.25f, 2f));

        GameObject probeObject = new GameObject("GoldenMasterCollisionProbe");
        probeObject.layer = PROBE_LAYER;
        CapsuleCollider probe = probeObject.AddComponent<CapsuleCollider>();
        probe.isTrigger = true;
        spawnedObjects.Add(probeObject);

        playerWorld = new UnityCollisionWorldAdapter(1 << ENVIRONMENT_LAYER, probe);
        visionWorld = new UnityCollisionWorldAdapter(1 << ENVIRONMENT_LAYER, null);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject spawned in spawnedObjects)
            Object.DestroyImmediate(spawned);

        spawnedObjects.Clear();
    }

    [Test]
    public void FullSession_MatchesRecordedTrace()
    {
        BuildInputScript();
        RunSession();

        string tracePath = Path.Combine(Application.dataPath, TRACE_RELATIVE_PATH);

        if (!File.Exists(tracePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(tracePath));
            File.WriteAllText(tracePath, trace.ToString());
            Assert.Inconclusive("Golden master trace recorded at " + tracePath);
        }

        string[] expected = File.ReadAllLines(tracePath);
        string[] actual = trace.ToString().TrimEnd('\n').Split('\n');

        Assert.AreEqual(expected.Length, actual.Length, "Trace length changed.");

        for (int i = 0; i < expected.Length; i++)
            AssertLineMatches(expected[i], actual[i], i);
    }

    private void RunSession()
    {
        PlayerLogic player = new PlayerLogic(playerConfig, playerWorld);
        CameraRigLogic cameraRig = new CameraRigLogic(new CameraRigConfig());
        MissionLogic mission = BuildMission();
        EnemyCoordinatorLogic enemies = BuildEnemies(out GuardLogic guard, out SurveillanceCameraLogic surveillanceCamera);
        StealthSessionLogic session = new StealthSessionLogic(player, cameraRig, enemies, mission, new TakedownLogic(new TakedownConfig(), playerWorld));

        session.Start();

        for (int frame = 0; frame < inputs.Count; frame++)
        {
            StealthSessionOutcome outcome = session.Tick(FIXED_DELTA, inputs[frame]);
            string outcomeLabel = outcome == StealthSessionOutcome.Captured ? CAPTURED_LABEL : outcome == StealthSessionOutcome.MissionCompleted ? EXIT_LABEL : string.Empty;

            if (frame % RECORD_EVERY_FRAMES == 0 || outcomeLabel.Length > 0)
                Record(frame, player.Pose, guard.Pose, surveillanceCamera, mission, cameraRig.Framing, outcomeLabel);

            if (outcome != StealthSessionOutcome.None)
                session.ResolvePendingOutcome();
        }
    }

    private MissionLogic BuildMission()
    {
        MissionLogic mission = new MissionLogic();
        mission.SetCheckpoints(new List<CheckpointData>
        {
            new CheckpointData(0, new Bounds(new Vector3(0f, 1f, -1f), new Vector3(3f, 2f, 3f)), new Vector3(0f, 0f, -1f), 0f),
            new CheckpointData(1, new Bounds(new Vector3(-4f, 1f, 2f), new Vector3(2f, 2f, 4f)), new Vector3(-4f, 0f, 2f), 270f)
        });
        mission.SetExit(new Bounds(new Vector3(-14f, 1f, 2f), new Vector3(4f, 2f, 8f)));
        return mission;
    }

    private EnemyCoordinatorLogic BuildEnemies(out GuardLogic guard, out SurveillanceCameraLogic surveillanceCamera)
    {
        List<GuardWaypoint> route = new List<GuardWaypoint>
        {
            new GuardWaypoint(new Vector3(8f, 0f, 10f), WAYPOINT_WAIT, true, WAYPOINT_LOOK_YAW),
            new GuardWaypoint(new Vector3(-6f, 0f, 10f), WAYPOINT_WAIT, true, WAYPOINT_LOOK_YAW)
        };

        guard = new GuardLogic(new GuardConfig(), route, new Vector3(8f, 0f, 10f), GUARD_SPAWN_YAW, visionWorld, new StraightLinePathfinderFake());
        surveillanceCamera = new SurveillanceCameraLogic(new SurveillanceCameraConfig(), new Vector3(12f, 2.7f, -6f), new Vector3(12f, 0f, -6f), CAMERA_BASE_YAW, visionWorld);

        EnemyCoordinatorLogic enemies = new EnemyCoordinatorLogic(ALERT_RADIUS);
        enemies.AddGuard(guard);
        enemies.AddSurveillanceCamera(surveillanceCamera);
        return enemies;
    }

    private void BuildInputScript()
    {
        inputs.Clear();

        Hold(Idle(), SETTLE_DURATION);
        Hold(Move(Vector2.up), WALL_APPROACH_DURATION);
        Hold(Idle(), SETTLE_DURATION);
        Hold(Move(Vector2.right), WALL_SHUFFLE_DURATION);
        Hold(Move(Vector2.up), CORNER_PUSH_DURATION);
        Hold(Move(Vector2.left), CORNER_RETURN_DURATION);
        Hold(Move(Vector2.down), WALL_LEAVE_DURATION);
        Hold(Sprint(Vector2.left), FIRST_SPRINT_DURATION);
        Hold(Idle(), SHORT_PAUSE_DURATION);
        CrouchTap();
        Hold(Idle(), POSTURE_SETTLE_DURATION);
        Hold(Move(Vector2.down), CROUCHED_WALK_DURATION);
        Hold(Move(new Vector2(0.4f, 0.4f), true), CROUCHED_WALK_DURATION);
        Stand();
        Hold(Move(Vector2.right), COVER_APPROACH_DURATION);
        Hold(Move(Vector2.down), STEP_APPROACH_DURATION);
        CrouchToProne();
        Hold(Move(Vector2.left), FIRST_CRAWL_DURATION);
        Hold(Sprint(Vector2.left), SPRINT_FROM_PRONE_DURATION);
        Hold(Idle(), STAND_UP_DURATION);
        Hold(Move(Vector2.up), CHECKPOINT_APPROACH_DURATION);
        Hold(Move(Vector2.left), BEAM_APPROACH_DURATION);
        CrouchToProne();
        Hold(Move(Vector2.left), BEAM_CRAWL_DURATION);
        Hold(Sprint(Vector2.left), EXIT_SPRINT_DURATION);
        Hold(Move(Vector2.right), RETURN_DURATION);
        Hold(Move(Vector2.up), CAPTURE_APPROACH_DURATION);
        Hold(Idle(), FINAL_IDLE_DURATION);
    }

    private void Hold(PlayerInputFrame input, float seconds)
    {
        int frames = Mathf.RoundToInt(seconds / FIXED_DELTA);

        for (int i = 0; i < frames; i++)
            inputs.Add(input);
    }

    private void CrouchTap()
    {
        inputs.Add(new PlayerInputFrame(Vector2.zero, false, false, true, true, false));
        inputs.Add(new PlayerInputFrame(Vector2.zero, false, false, false, false, true));
    }

    private void CrouchToProne()
    {
        inputs.Add(new PlayerInputFrame(Vector2.zero, false, false, true, true, false));
        Hold(new PlayerInputFrame(Vector2.zero, false, false, false, true, false), playerConfig.CrouchHoldToProneDuration + HOLD_MARGIN);
        inputs.Add(new PlayerInputFrame(Vector2.zero, false, false, false, false, true));
        Hold(Idle(), STAND_UP_DURATION);
    }

    private void Stand()
    {
        inputs.Add(new PlayerInputFrame(Vector2.zero, false, false, false, false, false, true));
        Hold(Idle(), STAND_UP_DURATION);
    }

    private PlayerInputFrame Idle() => new PlayerInputFrame(Vector2.zero, false, false, false);
    private PlayerInputFrame Move(Vector2 move) => new PlayerInputFrame(move, false, false, false);
    private PlayerInputFrame Move(Vector2 move, bool isAnalog) => new PlayerInputFrame(move, isAnalog, false, false);
    private PlayerInputFrame Sprint(Vector2 move) => new PlayerInputFrame(move, false, true, false);

    private void Record(int frame, PlayerPose player, GuardPose guard, SurveillanceCameraLogic surveillanceCamera, MissionLogic mission, CameraFraming framing, string outcome)
    {
        trace.Append(frame).Append(' ')
            .Append(player.PoseState).Append(' ');
        AppendVector(player.Position);
        AppendNumber(player.Rotation.eulerAngles.y);
        AppendNumber(player.NormalizedSpeed);
        AppendNumber(player.PostureTransitionProgress);
        AppendNumber(player.CapsuleRadius);
        AppendNumber(player.CapsuleHeight);
        AppendVector(player.WallNormal);

        trace.Append(guard.AlertState).Append(' ')
            .Append(guard.IsSearching).Append(' ')
            .Append(guard.CanSeePlayer).Append(' ');
        AppendVector(guard.Position);
        AppendNumber(guard.Yaw);
        AppendNumber(guard.Speed);

        AppendNumber(surveillanceCamera.Yaw);
        trace.Append(surveillanceCamera.IsAlerting).Append(' ');

        trace.Append(mission.ActiveCheckpoint.Order).Append(' ');
        AppendVector(framing.Pivot);
        trace.Append(outcome).Append('\n');
    }

    private void AppendVector(Vector3 value)
    {
        AppendNumber(value.x);
        AppendNumber(value.y);
        AppendNumber(value.z);
    }

    private void AppendNumber(float value)
    {
        trace.Append(value.ToString(NUMBER_FORMAT, CultureInfo.InvariantCulture)).Append(' ');
    }

    private static void AssertLineMatches(string expectedLine, string actualLine, int lineIndex)
    {
        string[] expectedTokens = expectedLine.Trim().Split(' ');
        string[] actualTokens = actualLine.Trim().Split(' ');

        Assert.AreEqual(expectedTokens.Length, actualTokens.Length, "Token count changed at line " + lineIndex + "\nexpected: " + expectedLine + "\nactual:   " + actualLine);

        for (int i = 0; i < expectedTokens.Length; i++)
        {
            bool isNumber = float.TryParse(expectedTokens[i], NumberStyles.Float, CultureInfo.InvariantCulture, out float expectedValue);

            if (isNumber && float.TryParse(actualTokens[i], NumberStyles.Float, CultureInfo.InvariantCulture, out float actualValue))
            {
                if (Mathf.Abs(expectedValue - actualValue) <= VALUE_TOLERANCE) continue;
            }
            else if (expectedTokens[i] == actualTokens[i])
            {
                continue;
            }

            Assert.Fail("Trace diverged at line " + lineIndex + ", token " + i + "\nexpected: " + expectedLine + "\nactual:   " + actualLine);
        }
    }

    private void SpawnBox(Vector3 center, Vector3 size)
    {
        GameObject box = new GameObject("GoldenMasterBox");
        box.layer = ENVIRONMENT_LAYER;
        box.transform.position = center;
        box.AddComponent<BoxCollider>().size = size;
        spawnedObjects.Add(box);
        Physics.SyncTransforms();
    }
}
