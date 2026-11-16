using System;

public enum SaveSystemType
{
    None,
    PlayerPrefs,
    JSON,
    CSV
}

public enum GameStates
{
    None = 0,
    Main = 1,
    Gameplay = 3
}

public enum DeviceType
{
    Any,
    KeyboardMouse,
    Gamepad
}

public enum InputActionMapId
{
    UI,
    Player,
    Debug
}

public enum InputActionId
{
    Navigate,
    Submit,
    AuxiliarySubmit,
    Cancel,

    Move,
    Look,
    Takedown,
    Interact,
    Crouch,
    Stand,
    Previous,
    Next,
    Sprint,
    Pause,

    ToggleDevTools
}

public enum InputListenPhase
{
    Started,
    Performed,
    Canceled
}

public enum RebindErrorType
{
    InvalidDevice,
    KeyAlreadyUsed
}

public enum InputValueType
{
    Button,
    Float,
    Vector2
}

public enum SelectableNavigationLoopMode
{
    Linear,
    ClosedLoop
}

[Flags]
public enum NavigationDirections
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 4,
    Right = 8,
    All = Up | Down | Left | Right
}

public enum DevToolsTab
{
    Performance,
    Time,
    State,
    Gameplay,
    Input,
    Capture,
    Console
}

public enum InputRecorderState
{
    Idle,
    Recording,
    Replaying
}
