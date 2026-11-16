using System.Collections;
using NUnit.Framework;
using StealthController.Core;
using StealthController.Unity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public class StealthInputReplayTests : InputTestFixture
{
    private const string DEMO_SCENE_PATH = "Assets/StealthController/Scenes/StealthDemo.unity";
    private const float SESSION_START_TIMEOUT = 10f;
    private const float WALK_DURATION = 1f;
    private const float MIN_WALK_DISTANCE = 1f;
    private const float CROUCH_SETTLE_DURATION = 1f;
    private const float CROUCH_TAP_DURATION = 0.1f;
    private const float REPLAY_TIMEOUT = 10f;
    private const float SETTLE_DURATION = 0.5f;
    private const float REPLAY_POSITION_TOLERANCE = 0.75f;
    private const int TRACE_BUFFER_BYTES = 1024 * 1024;

    private Keyboard keyboard;
    private StealthSessionManager session;

    private IEnumerator LoadDemoScene()
    {
        session = null;
        keyboard = InputSystem.AddDevice<Keyboard>();

#if UNITY_EDITOR
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode(DEMO_SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
#else
        Assert.Ignore("The stealth demo scene is loaded by path, which only works in the Editor.");
#endif

        float elapsed = 0f;

        while ((session == null || !session.IsRunning) && elapsed < SESSION_START_TIMEOUT)
        {
            session = Object.FindAnyObjectByType<StealthSessionManager>();
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Assert.IsNotNull(session, "The demo scene has no StealthSessionManager.");
        Assert.IsTrue(session.IsRunning, "The stealth session did not start.");
    }

    [UnityTest]
    public IEnumerator HoldingForward_MovesThePlayer()
    {
        yield return LoadDemoScene();

        Vector3 start = PlayerPosition;

        yield return HoldKey(keyboard.wKey, WALK_DURATION);

        Assert.Greater(HorizontalDistance(start, PlayerPosition), MIN_WALK_DISTANCE);
        Assert.AreEqual(StealthSessionOutcome.None, session.Logic.PendingOutcome);
    }

    [UnityTest]
    public IEnumerator TappingCrouch_EndsCrouched()
    {
        yield return LoadDemoScene();

        Assert.AreEqual(PlayerPoseState.Standing, session.PlayerPoseState);

        yield return HoldKey(keyboard.cKey, CROUCH_TAP_DURATION);
        yield return WaitSeconds(CROUCH_SETTLE_DURATION);

        Assert.AreEqual(PlayerPoseState.Crouched, session.PlayerPoseState);
    }

    [UnityTest]
    public IEnumerator RecordedInput_ReplaysToTheSameState()
    {
        yield return LoadDemoScene();

        Vector3 start = PlayerPosition;

        using (InputEventTrace trace = new InputEventTrace(TRACE_BUFFER_BYTES))
        {
            trace.Enable();

            yield return HoldKey(keyboard.wKey, WALK_DURATION);
            yield return HoldKey(keyboard.cKey, CROUCH_TAP_DURATION);
            yield return WaitSeconds(CROUCH_SETTLE_DURATION);

            trace.Disable();

            Vector3 recordedEnd = PlayerPosition;
            PlayerPoseState recordedPose = session.PlayerPoseState;

            Assert.Greater(HorizontalDistance(start, recordedEnd), MIN_WALK_DISTANCE, "The recording did not move the player.");

            session.RestartMission();
            yield return null;

            Assert.Less(HorizontalDistance(start, PlayerPosition), REPLAY_POSITION_TOLERANCE, "Restart did not return the player to the start.");

            using (InputEventTrace.ReplayController replay = trace.Replay())
            {
                replay.PlayAllEventsAccordingToTimestamps();

                float elapsed = 0f;

                while (!replay.finished && elapsed < REPLAY_TIMEOUT)
                {
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }

                Assert.IsTrue(replay.finished, "The replay did not finish.");
            }

            yield return WaitSeconds(SETTLE_DURATION);

            Assert.AreEqual(recordedPose, session.PlayerPoseState);
            Assert.Less(HorizontalDistance(recordedEnd, PlayerPosition), REPLAY_POSITION_TOLERANCE);
        }
    }

    private Vector3 PlayerPosition => session.Logic.Player.Pose.Position;

    private IEnumerator HoldKey(UnityEngine.InputSystem.Controls.KeyControl key, float duration)
    {
        Press(key);
        yield return WaitSeconds(duration);
        Release(key);
        yield return null;
    }

    private static IEnumerator WaitSeconds(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }
}
