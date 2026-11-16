# Unity 6 Game Template + Tactical Stealth Demo

A production-style Unity 6 (URP) project template with a fully playable top-down tactical stealth game built on top of it.

**▶ [Play the web build on Unity Play](https://play.unity.com/en/games/0107d333-572f-4ed9-aafe-116155470668/stealth-mission-controller)**

The template provides the systems every game needs (scene-based state machine, additive Addressables loading, save system, centralized Input System with rebinding, settings menus, localized UI framework with gamepad navigation). The stealth game shows those systems running a real game: postures, wall cover, back takedowns, patrolling guards, surveillance cameras, checkpoints and a full mission map.

- **Engine:** Unity `6000.4.1f1`, Universal Render Pipeline 17.4
- **Packages:** Input System, Addressables, Localization, AI Navigation, uGUI / TextMesh Pro, Test Framework
- **Languages:** English and Spanish (Unity Localization)
- **Platforms:** Desktop and WebGL (keyboard & mouse or gamepad)

---

## Getting started

1. Clone the repository and open the folder with **Unity 6000.4.1f1** (Unity Hub > Add project from disk).
2. Let the first import finish (Addressables and Localization tables are generated on import).
3. Press **Play** from any scene. `SceneBootstrapper` always starts from `Assets/_ProjectName/Scenes/GameManager.unity` and restores your open scene when you stop.
4. In the main menu choose **Play** to start the stealth mission, or open **Settings**.

### Controls

| Action | Keyboard / mouse | Gamepad |
|---|---|---|
| Move | WASD | Left stick |
| Sprint | Left Shift | Left stick press |
| Crouch (hold: go prone) | C | East button |
| Stand up | Space | South button |
| Takedown (from behind) | E / left click | Right trigger |
| Pause | Escape | Start |

### Running the tests

**Window > General > Test Runner > EditMode > Run All**, or from the command line:

```bash
Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results.xml
Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults playmode.xml
```

PlayMode tests (`Assets/_ProjectName/Tests/PlayMode/`) drive a virtual keyboard with `InputTestFixture` in the stealth demo scene: walking moves the player, tapping crouch ends crouched, and a recorded input sequence replays to the same position and posture after a restart.

---

## The stealth game

`Assets/StealthController/` is a self-contained module (it can be exported as a `.unitypackage` and works in an empty project). It is split in two assemblies:

| Assembly | Content |
|---|---|
| `StealthController.Core` | Pure C# game logic. No `MonoBehaviour`, physics, time or coroutines: every system advances with `Tick(deltaTime)` and sees the world through contracts (`ICollisionWorld`, `IPathfinder`, `IPlayerInputSource`). An architecture test enforces these rules. |
| `StealthController.Unity` | The only layer that talks to Unity: adapters for physics and NavMesh, input, animation, vision-cone meshes, camera occlusion, HUD and menus. |

**Gameplay systems**

- **Player:** standing, crouched and prone postures with timed transitions, sprint, wall cover with corner traversal, back takedown with paired animations.
- **Guards:** patrol routes, vision cones that respect cover and posture, chase and search, capture.
- **Surveillance cameras:** sweep patterns, blind zone, alerts relayed to nearby guards by an enemy coordinator.
- **Mission:** ordered checkpoints, respawn on capture, mission exit and restart.
- **Presentation:** occlusion "tunnel" shader so walls never hide the player, posture HUD with device-aware key prompts, pause menu.
- **Map:** *Waterworks*, a three-level linear mission with 8 guards and 4 cameras.

**Deterministic testing.** Because the core is engine-agnostic, it is covered by EditMode tests with fake collision worlds and pathfinders. `StealthGoldenMasterTests` replays a scripted play session through the full session logic with real physics and compares every frame against a recorded trace, so any behavioural change is caught immediately.

See [`Assets/StealthController/README.md`](Assets/StealthController/README.md) for using the module in another project and [`Assets/StealthController/Documentation/GUIDE.md`](Assets/StealthController/Documentation/GUIDE.md) for a deep dive and level-building guide.

---

## Developer tools

An in-game DevTools suite for playtesting, bug hunting and writing verifiable acceptance criteria. Enable it in **Settings > Developer**, then press **F1** (or **L3 + R3** on a gamepad) to show or hide the panel. It only exists in the Editor and development builds.

| Tab | What it does |
|---|---|
| **Performance** | Average / min / max / 1% low FPS, frame time, main-thread CPU, GC allocations, memory, draw calls, SetPass calls, triangles and vertices (`ProfilerRecorder`). A 10-second **benchmark** exports every frame to CSV with a summary row. |
| **Time** | Time scale (0.05x to 4x), pause and single-frame stepping. |
| **State** | Live snapshot of the game state (game flow, scenes, device, display and every gameplay system that registers a provider). Copy it to the clipboard or export it as JSON. |
| **Gameplay** | Commands published by the current game. The stealth demo adds invisibility, restart, teleport to the exit and respawn at any checkpoint. |
| **Input** | Records input with `InputEventTrace` to a `.inputtrace` file and replays it. Input pressed while the panel is open is not recorded, so a recording contains only gameplay. An optional overlay shows the pressed controls. |
| **Capture** | Screenshot plus a JSON state snapshot of the same frame, as evidence of an expected visual outcome. |
| **Console** | Turns the In-game Debug Console on or off, counts errors and warnings, and exports the whole session log to a `.txt` file. |

A compact overlay (FPS, frame time, 1% low, game state, time scale, recording status) stays on screen without blocking input. Every action is also available as an In-game Debug Console command (`devtools`, `devtools.timescale 0.5`, `devtools.run 3`, `devtools.exportlogs`, …).

**Adding a game:** implement `IDevToolsProvider` (state section, overlay summary, commands and a panel-visibility hook) and register it with `DevToolsManager.RegisterProviderRequest`. `StealthDevToolsProviderController` is the reference implementation.

---

## Template architecture

### Scene-based state machine

`GameManager` is the only persistent object. Each game state (`Main`, `Gameplay`) lives in its own scene, loaded additively through Addressables. The scene address matches `GameStates.ToString()`.

```
GameManager ──LoadScene(state)──▶ SceneLoadingManager (Addressables, additive)
     ▲                                        │
     │ SetNewGameStateRequest                 ▼
GameStateBase ◀──────── scene loaded ──── MainState / GameplayState
     │
     └── StateFinished(next) ──▶ GameManager ──▶ SwitchScenes
```

Scene switches are validated: if the target scene is not in Addressables, the switch fails safely and the current state is re-entered. Dependencies are injected explicitly by `GameManager` into the state that requests them.

### Explicit lifecycle and MVC

```
MonoBehaviour
 └── LifecycleBehaviourBase   Initialize / Conclude / AddListeners / RemoveListeners / RequireDependency
      ├── ManagerBase
      │    └── GameStateBase            EnterState / ExitState / StateFinished
      ├── ControllerBase
      │    └── UIControllerBase<TView>  owns its view, OnViewOpened / OnViewClosed
      └── ViewBase
           └── UIViewBase               animated Show/Hide, sub-views, focus handling
                └── CancelableUIViewBase  closes with UI/Cancel
```

Objects are driven by an explicit `Initialize` / `Conclude` lifecycle instead of `OnEnable` / `OnDestroy`, every event subscription is symmetric, and missing dependencies are reported immediately with `Debug.LogError(message, context)`.

### Core systems

| System | Highlights |
|---|---|
| **Input** (`InputManager`) | Centralized subscriptions through `InputRequest` (map, action, phase, callback), cached actions, automatic device join/leave, interactive rebinding with persistence. |
| **Save** (`SaveSystem`) | Static facade with in-memory cache over interchangeable back-ends: JSON (atomic file writes), PlayerPrefs and CSV. |
| **Settings** | Audio (mixer), graphics, controls (rebinding), sensitivity and language sub-menus on a shared base class that handles open/close/save. |
| **UI framework** | DOTween transitions (fade, zoom-fade) with queued "last request wins" semantics, automatic keyboard/gamepad navigation builders (horizontal, vertical, grid), 1- and 2-button confirmation windows, scroll-follow for focused items. |
| **Localization** | Every UI string comes from String Tables (English/Spanish); language switches live. |
| **Web build** | *Tools > Web Build > Build Release* applies itch.io-ready settings, builds Addressables and the player. |
| **DevTools** | Performance, time, state, gameplay commands, input recording, capture and log export (see [Developer tools](#developer-tools)). |
| **UI theme** | One `UIThemeConfig` asset (fonts, colors, sizes, spacing, corner radii) applied by *Tools > UI > Apply Theme* to every base prefab, scene and the stealth UI; style overrides on instances are reverted so all menus stay consistent. Canvases scale in *Expand* mode, so the 1920×1080 layout fits any aspect ratio without overlaps. |
| **Tasks** | [`Tasks/`](Tasks/README.md): workflow, templates and two worked examples (a playtest-only bug and a multi-system feature) with briefs, acceptance criteria and reference solutions. |

### Project layout

| Path | Content |
|---|---|
| `Assets/_ProjectName/Scripts/` | Template code: `GameManager/`, `GameStates/`, `GameFlow/`, `MVCBase/`, `InputSystem/`, `SaveSystem/`, `Settings/`, `UI/`, `SceneLoading/`, `Utilities/` |
| `Assets/_ProjectName/Scenes/` | `GameManager.unity` and the flow scenes in `GameFlow/` |
| `Assets/_ProjectName/Prefabs/UI/BaseElements/` | Nested prefab library for buttons, toggles, sliders, dropdowns, windows (see [`ASSETS_MAP.md`](ASSETS_MAP.md)) |
| `Assets/_ProjectName/Localization/` | String tables grouped by screen |
| `Assets/_ProjectName/Tests/Editor/` | EditMode tests (template systems and stealth core) |
| `Assets/StealthController/` | Self-contained stealth module |
| `Assets/Editor/` | Editor tooling: scene bootstrapper, navigation builder inspector, package export, web build |
| `Assets/Plugins/` | Third-party libraries |

Coding conventions are documented in [`CODING_STYLE.md`](CODING_STYLE.md). [`ARCHITECTURE.md`](ARCHITECTURE.md) contains the full architecture and development rules.

---

## Third-party content

DOTween (free), MEC Free, IngameDebugConsole, the Lexend font, MingCute icons and Mixamo characters/animations belong to their respective authors. See [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md).

## License

The original code and assets of this project are released under the [MIT License](LICENSE). Third-party content keeps its own license.
