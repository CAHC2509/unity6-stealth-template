using System.Collections.Generic;
using System.Globalization;
using System.Text;
using StealthController.Core;
using StealthController.Unity;
using UnityEngine;

public class StealthDevToolsProviderController : ControllerBase, IDevToolsProvider
{
    private StealthSessionManager session;
    private bool isRegistered;

    public string DevToolsSectionName => Constants.STEALTH_DEVTOOLS_SECTION;

    public override void Initialize(params object[] parameters)
    {
        session = parameters[0] as StealthSessionManager;

        if (!RequireDependency(session, nameof(session))) return;

        base.Initialize(parameters);

        DevToolsManager.RegisterProviderRequest?.Invoke(this);
        isRegistered = true;
    }

    public override void Conclude()
    {
        if (isRegistered)
        {
            DevToolsManager.UnregisterProviderRequest?.Invoke(this);
            isRegistered = false;
        }

        base.Conclude();
    }

    public void WriteDevToolsState(DevToolsStateSectionData section)
    {
        StealthSessionLogic logic = session.Logic;

        if (logic == null)
        {
            section.Add(nameof(session.IsRunning), false);
            return;
        }

        PlayerPose pose = logic.Player.Pose;
        MissionLogic mission = logic.Mission;

        section.Add(nameof(session.IsRunning), true);
        section.Add(nameof(logic.IsPaused), logic.IsPaused);
        section.Add(nameof(logic.PendingOutcome), logic.PendingOutcome.ToString());
        section.Add(nameof(logic.Enemies.IsPlayerDetectable), logic.Enemies.IsPlayerDetectable);
        section.Add(nameof(session.IsInputSuspended), session.IsInputSuspended);
        section.Add("player.position", pose.Position);
        section.Add("player.yaw", pose.Rotation.eulerAngles.y);
        section.Add("player.poseState", pose.PoseState.ToString());
        section.Add("player.normalizedSpeed", pose.NormalizedSpeed);
        section.Add("player.onWall", pose.IsOnWall);
        section.Add("mission.activeCheckpoint", mission.ActiveCheckpointRank);
        section.Add("mission.checkpointCount", mission.CheckpointCount);
        section.Add("mission.reachedExit", mission.HasReachedExit);
        section.Add("takedown.available", session.HasTakedownTarget);

        WriteGuards(section);
        WriteSurveillanceCameras(section);
    }

    public void WriteDevToolsOverlaySummary(StringBuilder builder)
    {
        StealthSessionLogic logic = session.Logic;

        if (logic == null)
        {
            builder.Append(Constants.DEVTOOLS_NONE_TEXT);
            return;
        }

        builder.Append(logic.Player.PoseState.ToString())
            .Append(Constants.DEVTOOLS_OVERLAY_SEPARATOR)
            .Append(Constants.STEALTH_DEVTOOLS_COMMAND_CHECKPOINT).Append(' ')
            .Append((logic.Mission.ActiveCheckpointRank + 1).ToString(CultureInfo.InvariantCulture))
            .Append('/').Append(logic.Mission.CheckpointCount.ToString(CultureInfo.InvariantCulture));

        if (!logic.Enemies.IsPlayerDetectable)
            builder.Append(Constants.DEVTOOLS_OVERLAY_SEPARATOR).Append(Constants.STEALTH_DEVTOOLS_COMMAND_INVISIBLE);
    }

    public void CollectDevToolsCommands(List<DevToolsCommandData> commands)
    {
        StealthSessionLogic logic = session.Logic;
        if (logic == null) return;

        commands.Add(DevToolsCommandData.CreateToggle(Constants.STEALTH_DEVTOOLS_COMMAND_INVISIBLE, Constants.STEALTH_DEVTOOLS_LABEL_INVISIBLE, IsPlayerInvisible, SetPlayerInvisible));
        commands.Add(DevToolsCommandData.CreateAction(Constants.STEALTH_DEVTOOLS_COMMAND_RESTART, Constants.STEALTH_DEVTOOLS_LABEL_RESTART, session.RestartMission));
        commands.Add(DevToolsCommandData.CreateAction(Constants.STEALTH_DEVTOOLS_COMMAND_EXIT, Constants.STEALTH_DEVTOOLS_LABEL_EXIT, TeleportToExit));

        for (int rank = 0; rank < logic.Mission.CheckpointCount; rank++)
        {
            int capturedRank = rank;
            int checkpointNumber = rank + 1;

            commands.Add(DevToolsCommandData.CreateAction(
                $"{Constants.STEALTH_DEVTOOLS_COMMAND_CHECKPOINT}{checkpointNumber}",
                Constants.STEALTH_DEVTOOLS_LABEL_CHECKPOINT,
                () => session.RespawnAtCheckpoint(capturedRank),
                checkpointNumber));
        }
    }

    public void OnDevToolsPanelVisibilityChanged(bool isVisible)
    {
        session.SetInputSuspended(isVisible);
    }

    private bool IsPlayerInvisible() => session.Logic != null && !session.Logic.Enemies.IsPlayerDetectable;

    private void SetPlayerInvisible(bool invisible) => session.SetPlayerDetectable(!invisible);

    private void TeleportToExit() => session.TeleportPlayerToExit();

    private void WriteGuards(DevToolsStateSectionData section)
    {
        IReadOnlyList<GuardAdapter> guards = session.Guards;

        for (int i = 0; i < guards.Count; i++)
        {
            GuardAdapter guard = guards[i];
            if (guard == null || !guard.IsReady) continue;

            GuardLogic logic = guard.Logic;
            string prefix = $"guard[{i}] {guard.name}";

            section.Add(prefix + ".alertState", logic.AlertState.ToString());
            section.Add(prefix + ".searching", logic.IsSearching);
            section.Add(prefix + ".canSeePlayer", logic.Vision.CanSeePlayer);
            section.Add(prefix + ".takenDown", logic.IsTakenDown);
            section.Add(prefix + ".position", logic.Position);
        }
    }

    private void WriteSurveillanceCameras(DevToolsStateSectionData section)
    {
        IReadOnlyList<SurveillanceCameraAdapter> cameras = session.SurveillanceCameras;

        for (int i = 0; i < cameras.Count; i++)
        {
            SurveillanceCameraAdapter surveillanceCamera = cameras[i];
            if (surveillanceCamera == null || !surveillanceCamera.IsReady) continue;

            SurveillanceCameraLogic logic = surveillanceCamera.Logic;
            string prefix = $"camera[{i}] {surveillanceCamera.name}";

            section.Add(prefix + ".alerting", logic.IsAlerting);
            section.Add(prefix + ".canSeePlayer", logic.Vision.CanSeePlayer);
            section.Add(prefix + ".yaw", logic.Yaw);
        }
    }
}
