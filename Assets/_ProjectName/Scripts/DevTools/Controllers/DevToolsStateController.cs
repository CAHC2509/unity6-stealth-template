using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public class DevToolsStateController : ControllerBase
{
    private readonly DevToolsStateSnapshotData snapshot = new();
    private readonly StringBuilder textBuilder = new();
    private readonly StringBuilder sceneBuilder = new();
    private readonly List<DevToolsCommandData> commands = new();

    private GameManager gameManager;
    private TimeControlController timeControl;
    private IReadOnlyList<IDevToolsProvider> providers;

    public DevToolsStateSnapshotData Snapshot => snapshot;
    public IReadOnlyList<DevToolsCommandData> Commands => commands;

    public override void Initialize(params object[] parameters)
    {
        gameManager = parameters[0] as GameManager;
        timeControl = parameters[1] as TimeControlController;
        providers = parameters[2] as IReadOnlyList<IDevToolsProvider>;

        bool hasDependencies = RequireDependency(gameManager, nameof(gameManager))
            & RequireDependency(timeControl, nameof(timeControl))
            & RequireService(providers, nameof(providers));

        if (!hasDependencies) return;

        base.Initialize(parameters);
    }

    public DevToolsStateSnapshotData Capture()
    {
        snapshot.Begin(DateTime.Now, Time.frameCount, Time.realtimeSinceStartup);

        WriteGameSection(snapshot.BeginSection(Constants.DEVTOOLS_GAME_SECTION));
        WriteDisplaySection(snapshot.BeginSection(Constants.DEVTOOLS_DISPLAY_SECTION));

        for (int i = 0; i < providers.Count; i++)
            providers[i].WriteDevToolsState(snapshot.BeginSection(providers[i].DevToolsSectionName));

        return snapshot;
    }

    public string CaptureText()
    {
        Capture().WriteText(textBuilder);
        return textBuilder.ToString();
    }

    public string CaptureJson() => Capture().ToJson();

    public IReadOnlyList<DevToolsCommandData> CollectCommands()
    {
        commands.Clear();

        for (int i = 0; i < providers.Count; i++)
            providers[i].CollectDevToolsCommands(commands);

        return commands;
    }

    public void WriteOverlaySummary(StringBuilder builder)
    {
        builder.Append(gameManager.CurrentState.ToString());

        for (int i = 0; i < providers.Count; i++)
        {
            builder.Append(Constants.DEVTOOLS_OVERLAY_SEPARATOR);
            providers[i].WriteDevToolsOverlaySummary(builder);
        }
    }

    private void WriteGameSection(DevToolsStateSectionData section)
    {
        section.Add(Constants.DEVTOOLS_KEY_GAME_STATE, gameManager.CurrentState.ToString());
        section.Add(Constants.DEVTOOLS_KEY_LOADED_SCENES, BuildLoadedScenes());
        section.Add(Constants.DEVTOOLS_KEY_TIME_SCALE, Time.timeScale);
        section.Add(Constants.DEVTOOLS_KEY_PAUSED, timeControl.IsPaused);
        section.Add(Constants.DEVTOOLS_KEY_FRAME, Time.frameCount);
        section.Add(Constants.DEVTOOLS_KEY_PLAY_TIME, Time.realtimeSinceStartup);
        section.Add(Constants.DEVTOOLS_KEY_DEVICE, Globals.GlobalDeviceType.ToString());
        section.Add(Constants.DEVTOOLS_KEY_LAST_DEVICE, BuildLastUsedDevice());

        Locale locale = LocalizationSettings.SelectedLocale;
        section.Add(Constants.DEVTOOLS_KEY_LOCALE, locale != null ? locale.Identifier.Code : Constants.DEVTOOLS_NONE_TEXT);
    }

    private void WriteDisplaySection(DevToolsStateSectionData section)
    {
        section.Add(Constants.DEVTOOLS_KEY_RESOLUTION, $"{Screen.width}x{Screen.height}");
        section.Add(Constants.DEVTOOLS_KEY_FULLSCREEN, Screen.fullScreenMode.ToString());
        section.Add(Constants.DEVTOOLS_KEY_QUALITY, QualitySettings.names[QualitySettings.GetQualityLevel()]);
        section.Add(Constants.DEVTOOLS_KEY_VSYNC, QualitySettings.vSyncCount);
        section.Add(Constants.DEVTOOLS_KEY_TARGET_FRAME_RATE, Application.targetFrameRate);
    }

    private string BuildLoadedScenes()
    {
        sceneBuilder.Clear();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (i > 0)
                sceneBuilder.Append(Constants.DEVTOOLS_CONTROLS_SEPARATOR);

            sceneBuilder.Append(SceneManager.GetSceneAt(i).name);
        }

        return sceneBuilder.ToString();
    }

    private static string BuildLastUsedDevice()
    {
        PlayerInput playerInput = Globals.GlobalPlayerInput;

        if (playerInput == null || playerInput.devices.Count == 0)
            return Constants.DEVTOOLS_NONE_TEXT;

        return playerInput.devices[0].displayName;
    }
}
