# Coding Style

This document describes the patterns, syntax conventions and code structure used in this project. All new code must follow this style exactly.

---

## 1. Class Structure and Member Order (`MonoBehaviour` / MVC)

Members inside a class are always ordered from top to bottom as follows:

1. **Serialized fields:** declared as `[SerializeField] private`, optionally grouped with `[Header("...")]`.
2. **Non-serialized private / protected fields:** state variables or cached references.
3. **Unity methods and base lifecycle:** `Awake()`, `Initialize(params object[] parameters)`, `Conclude()`, `AddListeners()`, `RemoveListeners()`.
4. **Public methods (UI callbacks or controller API):** methods called by buttons (`On...Clicked`) or by other systems.
5. **Private methods (event handlers and internal logic):** methods that handle events, DOTween callbacks, MEC coroutines, etc.

---

## 2. Syntax and Formatting

- **Braces:** Allman style (the opening brace goes on its own line, below the class, method or conditional declaration). Single-statement `if`s may omit braces, and guard clauses go on one line (`if (x == null) return;`).
- **Indentation:** 4 spaces (no tabs).
- **Expression-bodied members:** use `=>` for one-line methods, read-only properties or simple transition factories:
  ```csharp
  protected override UITransitionBase CreateTransition() => new UIFadeTransition(fadeTransitionData);
  public ViewBase View => view;
  ```
- **No magic numbers:** no meaningful numeric literal appears in code; it becomes a named constant in `Constants.cs` (this includes default values of serialized fields and `[Range]`/`[Min]` limits). In the stealth module, constants go in `StealthConstants` (core) or `StealthUnityConstants` (adapters). `0`, `1`, `0.5f` and `2f` are allowed only as an obvious identity, counter, half or double:
  ```csharp
  [SerializeField, Min(0f)] private float runSpeed = StealthConstants.PLAYER_RUN_SPEED;
  bodyCollider.center = Vector3.up * (height * 0.5f);
  ```
- **Component-based portable core (`Assets/StealthController/Runtime/Core/`):** a `…Component` encapsulates one behaviour and its state. Its constructor receives only the components or contracts it needs and its config values, and it exposes `Reset()` and, if it advances over time, `Tick(float deltaTime, ...)`. An actor (`…Logic`) only creates its components, wires them together and runs the state machine; its states call the components (`player.Posture.TryChange(...)`), not methods on the actor.
- **Explicit types:** avoid the `var` keyword for local variables; always use the concrete type (`MainController mainController = ...`).
- **Access modifiers:** always explicit (`private`, `protected`, `public`). Serialized fields are always `[SerializeField] private`.

---

## 3. Parameters and Casting (`Initialize`)

- `Initialize(params object[] parameters)` receives external dependencies.
- Values are cast from the parameter array with the `as` operator and concrete types:
  ```csharp
  mainState = parameters[0] as MainState;
  settingsManager = parameters[1] as SettingsManager;
  ```

---

## 4. Events and UI (`AddListeners` / `RemoveListeners`)

- Strict symmetry: every event or listener added in `AddListeners()` must be removed in exactly the same way in `RemoveListeners()`.
- In UI, buttons call controller actions directly:
  ```csharp
  playButton.onClick.AddListener(mainController.OnPlayClicked);
  ```

---

## 5. Zero Comments
- **Comments (`//`, `/* */`, `///`) are not allowed.** Code is 100% self-documenting through descriptive, expressive names for classes, methods and variables.
