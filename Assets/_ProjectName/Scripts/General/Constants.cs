public static class Constants
{
    #region Settings

    public const string AUDIO_SETTINGS_SAVE_KEY = "AudioSettings";
    public const string AUDIO_GENERAL_VOL_SAVE_KEY = "GeneralVolume";
    public const string AUDIO_MUSIC_VOL_SAVE_KEY = "MusicVolume";
    public const string AUDIO_EFFECTS_VOL_SAVE_KEY = "EffectsVolume";
    public const string AUDIO_UI_VOL_SAVE_KEY = "UIVolume";

    public const float AUDIO_DEFAULT_GENERAL_VOL = 1f;
    public const float AUDIO_DEFAULT_MUSIC_VOL = 1f;
    public const float AUDIO_DEFAULT_EFFECTS_VOL = 1f;
    public const float AUDIO_DEFAULT_UI_VOL = 1f;

    public const float AUDIO_MIN_LINEAR_VOLUME = 0.0001f;
    public const float AUDIO_MUTED_DECIBELS = -80f;
    public const float AUDIO_DECIBELS_PER_LOG_UNIT = 20f;

    public const string GRAPHICS_SETTINGS_SAVE_KEY = "GraphicsSettings";
    public const int GRAPHICS_VSYNC_ENABLED_COUNT = 1;
    public const int GRAPHICS_VSYNC_DISABLED_COUNT = 0;
    public const float GRAPHICS_TARGET_ASPECT_RATIO = 16f / 9f;
    public const float GRAPHICS_ASPECT_RATIO_TOLERANCE = 0.01f;

    public const string CONTROLS_SETTINGS_SAVE_KEY = "ControlsSettings";

    public const string LANGUAGE_SETTINGS_SAVE_KEY = "LanguageSettings";

    public const string SENSITIVITY_SETTINGS_SAVE_KEY = "SensibilitySettings";
    public const float SENSITIVITY_DEFAULT_GENERAL = 0.5f;

    #endregion

    #region UI

    public const float PERCENT_MULTIPLIER = 100f;
    public const float FULL_ROTATION_DEGREES = 360f;

    public const float UI_THEME_MIN_AUTO_SIZE_RATIO = 0.1f;
    public const string UI_THEME_ASSET_PATH = "Assets/_ProjectName/UI/Theme/UITheme.asset";
    public const string UI_BASE_ELEMENTS_FOLDER = "Assets/_ProjectName/Prefabs/UI/BaseElements/";

    #endregion

    #region Input

    public const string INPUT_REBIND_CANCEL_CONTROL = "<Keyboard>/escape";
    public const string INPUT_REBIND_COMPOSITE_PART_CONTROL_TYPE = "Key";
    public const string INPUT_GROUP_KEYBOARD = "Keyboard";
    public const string INPUT_GROUP_MOUSE = "Mouse";
    public const string INPUT_GROUP_GAMEPAD = "Gamepad";
    public const string INPUT_GAMEPAD_PATH_PREFIX = "<Gamepad>";
    public const string INPUT_CONTROL_LEFT_SHIFT = "leftShift";
    public const string INPUT_CONTROL_RIGHT_SHIFT = "rightShift";
    public const string INPUT_CONTROL_LEFT_CTRL = "leftCtrl";
    public const string INPUT_CONTROL_RIGHT_CTRL = "rightCtrl";
    public const string INPUT_CONTROL_LEFT_ALT = "leftAlt";
    public const string INPUT_CONTROL_LEFT_STICK_PRESS = "leftStickPress";
    public const string INPUT_CONTROL_RIGHT_STICK_PRESS = "rightStickPress";
    public const string INPUT_CONTROL_LEFT_TRIGGER = "leftTrigger";
    public const string INPUT_CONTROL_RIGHT_TRIGGER = "rightTrigger";
    public const string INPUT_KEY_LABEL_SHIFT = "Shift";
    public const string INPUT_KEY_LABEL_CTRL = "Ctrl";
    public const string INPUT_KEY_LABEL_ALT = "Alt";
    public const string INPUT_KEY_LABEL_LEFT_STICK = "LS";
    public const string INPUT_KEY_LABEL_RIGHT_STICK = "RS";
    public const string INPUT_KEY_LABEL_LEFT_TRIGGER = "LT";
    public const string INPUT_KEY_LABEL_RIGHT_TRIGGER = "RT";

    #endregion

    #region SaveSystem

    public const int CSV_MIN_RECORDS_WITH_DATA = 2;

    #endregion

    #region Web Build

    public const string WEB_BUILD_OUTPUT_FOLDER = "Builds/Web";
    public const int WEB_BUILD_CANVAS_WIDTH = 1280;
    public const int WEB_BUILD_CANVAS_HEIGHT = 720;
    public const int WEB_BUILD_MAXIMUM_MEMORY_MB = 2048;
    public const int BYTES_PER_MEGABYTE = 1024 * 1024;

    #endregion

    #region Files

    public const string TEMPORARY_FILE_SUFFIX = ".tmp";
    public const string FILE_TIMESTAMP_FORMAT = "yyyyMMdd_HHmmss";
    public const int BYTES_PER_KILOBYTE = 1024;
    public const float MILLISECONDS_PER_SECOND = 1000f;
    public const float NANOSECONDS_PER_MILLISECOND = 1000000f;

    #endregion

    #region DevTools

    public const string DEVELOPER_SETTINGS_SAVE_KEY = "DeveloperSettings";
    public const string DEVTOOLS_PREFERENCES_SAVE_KEY = "DevToolsPreferences";
    public const string DEVTOOLS_TABLE = "DevTools";

    public const int DEVTOOLS_STATISTICS_WINDOW_FRAMES = 240;
    public const float DEVTOOLS_REFRESH_INTERVAL = 0.5f;
    public const float DEVTOOLS_MIN_REFRESH_INTERVAL = 0.05f;
    public const float DEVTOOLS_INPUT_REFRESH_INTERVAL = 0.05f;
    public const float DEVTOOLS_MIN_INPUT_REFRESH_INTERVAL = 0.02f;
    public const float DEVTOOLS_ONE_PERCENT_RATIO = 0.01f;
    public const float DEVTOOLS_BENCHMARK_DURATION = 10f;
    public const int DEVTOOLS_BENCHMARK_MAX_SAMPLES = 20000;

    public const float DEVTOOLS_MIN_TIME_SCALE = 0.05f;
    public const float DEVTOOLS_MAX_TIME_SCALE = 4f;
    public const float DEVTOOLS_DEFAULT_TIME_SCALE = 1f;

    public const int DEVTOOLS_MAX_LOG_ENTRIES = 5000;
    public const int DEVTOOLS_INPUT_BUFFER_KILOBYTES = 512;
    public const int DEVTOOLS_INPUT_MAX_BUFFER_KILOBYTES = 16384;
    public const int DEVTOOLS_MAX_DISPLAYED_CONTROLS = 8;
    public const float DEVTOOLS_STICK_DISPLAY_DEADZONE = 0.2f;
    public const int DEVTOOLS_CAPTURE_SETTLE_FRAMES = 2;

    public const string DEVTOOLS_BENCHMARKS_FOLDER = "DevTools/Benchmarks";
    public const string DEVTOOLS_LOGS_FOLDER = "DevTools/Logs";
    public const string DEVTOOLS_RECORDINGS_FOLDER = "DevTools/Recordings";
    public const string DEVTOOLS_CAPTURES_FOLDER = "DevTools/Captures";
    public const string DEVTOOLS_BENCHMARK_FILE_PREFIX = "benchmark";
    public const string DEVTOOLS_LOG_FILE_PREFIX = "session";
    public const string DEVTOOLS_RECORDING_FILE_PREFIX = "input";
    public const string DEVTOOLS_CAPTURE_FILE_PREFIX = "capture";
    public const string DEVTOOLS_CSV_EXTENSION = "csv";
    public const string DEVTOOLS_TEXT_EXTENSION = "txt";
    public const string DEVTOOLS_JSON_EXTENSION = "json";
    public const string DEVTOOLS_PNG_EXTENSION = "png";
    public const string DEVTOOLS_RECORDING_EXTENSION = "inputtrace";
    public const string DEVTOOLS_FILE_URL_PREFIX = "file:///";

    public const string DEVTOOLS_ISO_TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss";
    public const string DEVTOOLS_DECIMAL_FORMAT = "0.00";
    public const string DEVTOOLS_VECTOR_FORMAT = "({0:0.00}, {1:0.00}, {2:0.00})";
    public const string DEVTOOLS_FPS_SUMMARY_FORMAT = "{0:0} / {1:0} / {2:0} / {3:0}";
    public const string DEVTOOLS_FRAME_TIME_FORMAT = "{0:0.00} / {1:0.00}";
    public const string DEVTOOLS_TRUE_TEXT = "true";
    public const string DEVTOOLS_FALSE_TEXT = "false";
    public const string DEVTOOLS_NONE_TEXT = "none";
    public const string DEVTOOLS_NOT_AVAILABLE_TEXT = "-";
    public const string DEVTOOLS_SECTION_PREFIX = "[";
    public const string DEVTOOLS_SECTION_SUFFIX = "]";
    public const string DEVTOOLS_KEY_VALUE_SEPARATOR = ": ";
    public const string DEVTOOLS_CONTROLS_SEPARATOR = ", ";
    public const string DEVTOOLS_OVERLAY_SEPARATOR = "  |  ";
    public const string DEVTOOLS_CSV_SEPARATOR = ",";
    public const string DEVTOOLS_FPS_UNIT = " FPS";
    public const string DEVTOOLS_MILLISECONDS_UNIT = " ms";
    public const string DEVTOOLS_ONE_PERCENT_LOW_LABEL = "1% ";
    public const string DEVTOOLS_TIME_SCALE_PREFIX = "x";
    public const string DEVTOOLS_PAUSED_LABEL = "PAUSED";
    public const string DEVTOOLS_RECORDING_LABEL = "REC";
    public const string DEVTOOLS_REPLAYING_LABEL = "REPLAY";

    public const string PROFILER_MAIN_THREAD = "Main Thread";
    public const string PROFILER_RENDER_THREAD = "Render Thread";
    public const string PROFILER_GC_ALLOCATED_IN_FRAME = "GC Allocated In Frame";
    public const string PROFILER_TOTAL_USED_MEMORY = "Total Used Memory";
    public const string PROFILER_GC_RESERVED_MEMORY = "GC Reserved Memory";
    public static readonly string[] PROFILER_DRAW_CALL_COUNTERS =
    {
        "Standard Draw Calls Count",
        "Standard Instanced Draw Calls Count",
        "Standard Indirect Draw Calls Count",
        "SRP Batcher Draw Calls Count",
        "BRG Draw Calls Count",
        "BRG Indirect Draw Calls Count"
    };
    public const string PROFILER_VERTICES = "Vertices Count";
    public const string PROFILER_SET_PASS_CALLS = "SetPass Calls Count";
    public const string PROFILER_TRIANGLES = "Triangles Count";

    public const string DEVTOOLS_BENCHMARK_CSV_HEADER = "time_s,frame_ms,main_thread_ms,render_thread_ms,gc_alloc_bytes,total_used_bytes,gc_reserved_bytes,draw_calls,setpass_calls,triangles,vertices";
    public const string DEVTOOLS_BENCHMARK_SUMMARY_HEADER = "avg_fps,min_fps,max_fps,one_percent_low_fps,avg_frame_ms";
    public const string DEVTOOLS_LOG_DROPPED_FORMAT = "Oldest {0} entries were dropped to respect the log limit.";
    public const string DEVTOOLS_LOG_HEADER_PRODUCT = "Product: ";
    public const string DEVTOOLS_LOG_HEADER_ENGINE = "Unity: ";
    public const string DEVTOOLS_LOG_HEADER_PLATFORM = "Platform: ";
    public const string DEVTOOLS_LOG_HEADER_DEVICE = "Device: ";
    public const string DEVTOOLS_LOG_HEADER_SESSION_START = "Session start: ";
    public const string DEVTOOLS_LOG_HEADER_EXPORTED = "Exported: ";
    public const string DEVTOOLS_LOG_TIME_SUFFIX = "s ";
    public const string DEVTOOLS_LOG_FRAME_PREFIX = "f";

    public const string DEVTOOLS_GAME_SECTION = "Game";
    public const string DEVTOOLS_DISPLAY_SECTION = "Display";
    public const string DEVTOOLS_KEY_GAME_STATE = "gameState";
    public const string DEVTOOLS_KEY_LOADED_SCENES = "loadedScenes";
    public const string DEVTOOLS_KEY_TIME_SCALE = "timeScale";
    public const string DEVTOOLS_KEY_PAUSED = "paused";
    public const string DEVTOOLS_KEY_FRAME = "frame";
    public const string DEVTOOLS_KEY_PLAY_TIME = "realtimeSinceStartup";
    public const string DEVTOOLS_KEY_DEVICE = "deviceType";
    public const string DEVTOOLS_KEY_LAST_DEVICE = "device";
    public const string DEVTOOLS_KEY_LOCALE = "locale";
    public const string DEVTOOLS_KEY_RESOLUTION = "resolution";
    public const string DEVTOOLS_KEY_FULLSCREEN = "fullscreenMode";
    public const string DEVTOOLS_KEY_QUALITY = "quality";
    public const string DEVTOOLS_KEY_VSYNC = "vSyncCount";
    public const string DEVTOOLS_KEY_TARGET_FRAME_RATE = "targetFrameRate";

    public const string DEVTOOLS_COMMAND_TOGGLE = "devtools";
    public const string DEVTOOLS_COMMAND_TOGGLE_DESCRIPTION = "Shows or hides the DevTools panel";
    public const string DEVTOOLS_COMMAND_TIME_SCALE = "devtools.timescale";
    public const string DEVTOOLS_COMMAND_TIME_SCALE_DESCRIPTION = "Sets Time.timeScale (0.05 - 4)";
    public const string DEVTOOLS_COMMAND_PAUSE = "devtools.pause";
    public const string DEVTOOLS_COMMAND_PAUSE_DESCRIPTION = "Pauses or resumes game time";
    public const string DEVTOOLS_COMMAND_STEP = "devtools.step";
    public const string DEVTOOLS_COMMAND_STEP_DESCRIPTION = "Advances one frame while paused";
    public const string DEVTOOLS_COMMAND_STATE = "devtools.state";
    public const string DEVTOOLS_COMMAND_STATE_DESCRIPTION = "Logs the current game state";
    public const string DEVTOOLS_COMMAND_SCREENSHOT = "devtools.screenshot";
    public const string DEVTOOLS_COMMAND_SCREENSHOT_DESCRIPTION = "Saves a screenshot and a state snapshot";
    public const string DEVTOOLS_COMMAND_EXPORT_LOGS = "devtools.exportlogs";
    public const string DEVTOOLS_COMMAND_EXPORT_LOGS_DESCRIPTION = "Exports this session's logs to a txt file";
    public const string DEVTOOLS_COMMAND_BENCHMARK = "devtools.benchmark";
    public const string DEVTOOLS_COMMAND_BENCHMARK_DESCRIPTION = "Records a performance benchmark to a csv file";
    public const string DEVTOOLS_COMMAND_RECORD = "devtools.record";
    public const string DEVTOOLS_COMMAND_RECORD_DESCRIPTION = "Starts or stops recording input";
    public const string DEVTOOLS_COMMAND_REPLAY = "devtools.replay";
    public const string DEVTOOLS_COMMAND_REPLAY_DESCRIPTION = "Replays the last input recording";
    public const string DEVTOOLS_COMMAND_LOAD_RECORDING = "devtools.loadrecording";
    public const string DEVTOOLS_COMMAND_LOAD_RECORDING_DESCRIPTION = "Loads an input recording from a file path";
    public const string DEVTOOLS_COMMAND_LIST = "devtools.commands";
    public const string DEVTOOLS_COMMAND_LIST_DESCRIPTION = "Lists the gameplay commands of the current game";
    public const string DEVTOOLS_COMMAND_RUN = "devtools.run";
    public const string DEVTOOLS_COMMAND_RUN_DESCRIPTION = "Runs a gameplay command by index";

    public const string STEALTH_DEVTOOLS_SECTION = "Stealth";
    public const string STEALTH_DEVTOOLS_COMMAND_INVISIBLE = "invisible";
    public const string STEALTH_DEVTOOLS_COMMAND_RESTART = "restart";
    public const string STEALTH_DEVTOOLS_COMMAND_EXIT = "teleportExit";
    public const string STEALTH_DEVTOOLS_COMMAND_CHECKPOINT = "checkpoint";
    public const string STEALTH_DEVTOOLS_LABEL_INVISIBLE = "devTools.commandInvisible";
    public const string STEALTH_DEVTOOLS_LABEL_RESTART = "devTools.commandRestartMission";
    public const string STEALTH_DEVTOOLS_LABEL_EXIT = "devTools.commandTeleportExit";
    public const string STEALTH_DEVTOOLS_LABEL_CHECKPOINT = "devTools.commandRespawnCheckpoint";

    #endregion

    #region Package Export

    public const string STEALTH_PACKAGE_ROOT = "Assets/StealthController";
    public const string STEALTH_PACKAGE_EXPORT_FOLDER = "Exports";
    public const string STEALTH_PACKAGE_FILE_NAME = "StealthController.unitypackage";

    #endregion
}
