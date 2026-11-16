# Architecture and Development Rules

Architecture guide, conventions and working rules for this repository (`UnityTemplate`). Applies to anyone working on the code.

---

## Project

**UnityTemplate** — Modular, decoupled base template for game development in Unity 6. It contains the foundational systems, ready to extend: per-scene state architecture, additive loading through Addressables, multi-purpose save system, centralized Input System management with rebinding, settings system (Audio, Graphics, Controls, Language, Sensitivity) and a UI framework with transitions and assisted navigation.

- **Engine:** Unity 6 (`6000.4.1f1`)
- **Render Pipeline:** Universal Render Pipeline (URP 17.4.0, 2D Feature)
- **Core Systems:**
  - Input System (`1.19.0`)
  - Localization (`1.5.11`)
  - Addressables (`2.9.1`)
  - AI Navigation (`2.0.12`)
  - uGUI & TextMesh Pro (`2.0.0`)
- **Third-Party Plugins (`Assets/Plugins/`):**
  - **DOTween** (Demigiant, free version) — Animations, UI transitions and sequences.
  - **MEC Free / More Effective Coroutines** (Trinary Software) — High-performance coroutines.
  - **IngameDebugConsole** — Runtime debug console.

### Repository Organization
- The project's code and assets live **exclusively** inside `Assets/_ProjectName/`. Exception: the stealth module is a standalone package in `Assets/StealthController/` (it is exported as a `.unitypackage` and works in an empty project); the template uses it, never the other way around.
- `Assets/Editor/` contains global editor tools (`SceneBootstrapper`, `SelectableNavigationBuilderEditor`).
- `Assets/Plugins/` for third-party libraries and utilities.
- The `Assets/` root hosts engine configuration folders (`URPSettings/`, `AudioSettings/`, `AddressableAssetsData/`, `TextMesh Pro/`, `Resources/`).
- **Namespaces:** Gameplay scripts (`Assets/_ProjectName/Scripts/`) do not use `namespace`. They all live in the global namespace. Keep it that way unless explicitly decided otherwise. Decided exception: the `Assets/StealthController/` package uses `StealthController.Core`, `StealthController.Unity` and `StealthController.Unity.UI` so it does not clash with the classes of the project it is imported into.

---

## Architecture

### 1. Per-Scene State Machine & GameManager

`GameManager` is the entry point and the only persistent object (`Assets/_ProjectName/Scenes/GameManager.unity`). It contains no gameplay logic; it coordinates scene loading and dependency injection into the active state.

```
GameManager  ──LoadScene(GameStates.ToString())──▶  SceneLoadingManager (Addressables, Additive)
     ▲                                                         │
     │ GameManager.SetNewGameStateRequest?.Invoke(this)       ▼
GameStateBase (Start) ◀───────────── scene loaded ─────── MainState / GameplayState
     │
     └── StateFinished(GameStates next) ──▶ GameManager.OnCurrentStateFinished ──▶ SwitchScenes
```

- **Scene-Enum Mapping:** The scene name and its address in the `Scenes` Addressables group match `GameStates.ToString()`. Flow scenes live in `Assets/_ProjectName/Scenes/GameFlow/`.
- **Implemented states:** `Main` and `Gameplay`. The `GameStates` enum uses explicit values (they are serialized as integers): a new value gets a new number, and an existing one is never reused or renumbered. A value is only added once its scene and its state exist.
- **Validated scene switch:** `SceneLoadingManager.SwitchScenes` checks that the target scene exists in Addressables before unloading the current one; if it does not exist, it emits `SwitchScenesFailed` and `GameManager` re-enters the current state.
- **Dependency Injection:** Done explicitly in `GameManager.OnSetNewGameStateRequest` through *pattern matching* on the concrete `GameStateBase` instance received (calling `state.Dependencies(...)`). Currently only `MainState` receives dependencies.
- **Bootstrap Tool:** `SceneBootstrapper` (in `Assets/Editor/`) ensures that pressing Play in the editor always starts from the `GameManager.unity` scene and restores the active scene on exit.

#### Adding a new GameState
1. Add (or reuse) the value in the `GameStates` enum (`Scripts/General/Enums.cs`).
2. Create `<Name>State : GameStateBase` in `Scripts/GameStates/GameFlow/` and its `Controller`/`View` in `Scripts/GameFlow/<Name>/`.
3. Create the scene `Scenes/GameFlow/<Name>.unity` with the state at its root.
4. Register the scene in the `Scenes` Addressables group with an address exactly equal to `GameStates.<Name>.ToString()`.
5. If it needs dependencies, add a `Dependencies(...)` to the state and its `case` in the `switch` of `GameManager.OnSetNewGameStateRequest`.

---

### 2. Base Lifecycle & MVC

All controllers, views and managers derive from the hierarchy in `Assets/_ProjectName/Scripts/MVCBase/` (except `GameStateBase`, which lives in `Scripts/GameStates/Base/`):

```
MonoBehaviour
 └── LifecycleBehaviourBase  ──▶ Initialize(params object[]) / Conclude / AddListeners / RemoveListeners / RequireDependency
      ├── ManagerBase
      │    └── GameStateBase           ──▶ + EnterState / ExitState / StateFinished
      ├── ControllerBase
      │    └── UIControllerBase<TView> ──▶ + View serialized in the base / OnViewOpened / OnViewClosed (every UI controller)
      └── ViewBase
           └── UIViewBase              ──▶ + ShowView / HideView / ShowInstant / HideInstant / SubView and Focus Handling / StartsHidden / RequiresSelection
                └── CancelableUIViewBase ──▶ + Close with the UI/Cancel action → OnCancelRequested (ignored if subviews are open)
```

`MVCBase/` also contains `UITransitionHandle` (plain class, not a `MonoBehaviour`) that exposes the state and callbacks of a view transition.

- **Dependency validation:** `RequireDependency(dependency, nameof(dependency))` logs `Debug.LogError(message, this)` and returns `false` if it is missing. It is the standard way to comply with rule 9. For services that are not a `UnityEngine.Object` (interfaces received through `Initialize`, such as `ICollisionWorld` or `IPathfinder`), use `RequireService(service, nameof(service))`.
- **Views that close with Cancel** (menus, submenus, confirmation windows) inherit from `CancelableUIViewBase` and implement `OnCancelRequested()`; they do not create their own `InputRequest`.

- `Initialize(params object[] parameters)` and `Conclude()` are the project's explicit lifecycle, **not** `OnEnable`/`OnDestroy`. Exception: components that extend Unity classes (e.g. `ObservableDropdown`) or standalone utilities (e.g. `ScrollRectSelectionFollower`) may use Unity callbacks when they do not take part in the `Initialize`/`Conclude` flow.
- `AddListeners()` and `RemoveListeners()` are the only methods responsible for connecting and disconnecting events.

---

### 3. Layer Separation by Feature

| Folder / Layer | Suffix | Type | Responsibility |
|---|---|---|---|
| `Data/` | `Data`, `Config`, `Entity` | `[Serializable]` or `ScriptableObject` | Pure state and operations on its own fields. Does not reference views or controllers. |
| `Controllers/` | `Controller` | `UIControllerBase<TView>` if it governs a UI view; `ControllerBase` otherwise | Orchestrates business logic, listens to input/events and commands the view. |
| `Views/` / `View/` | `View` | `MonoBehaviour : ViewBase` or `UIViewBase` | Visual presentation, UI binding and capture of direct component interaction. |
| `Manager/` / `Managers/` | `Manager` | `MonoBehaviour : ManagerBase` | Cross-cutting scene systems or core subsystems. |

---

### 4. Main Subsystems

- **Input System (`InputManager`):**
  - Centralizes input subscriptions through `InputRequest` (`MapId`, `ActionId`, `Phase`, `OnInput`).
  - Caches actions by map (`InputActionMapId`) and action (`InputActionId`).
  - Automatically handles gamepad/device joins and disconnections (`PlayerInputManager`) and supports global or per-player input.
- **Save System (`SaveSystem`):**
  - Static facade with an in-memory cache, delegating to an `ISaveSystem` implementation (`JSONSaveSystem`, `PlayerPrefsSaveSystem`, `CSVSaveSystem`).
  - Methods: `Save<T>(key, data)`, `Load<T>(key)`, `HasKey(key)`, `Delete(key)`, `ClearCache()`.
  - Implementations derive from `SaveSystemBase` (null validation, error handling, default values and JSON deserialization); file-based ones derive from `FileSaveSystemBase` (paths and atomic writes). A new implementation only defines `HasKey`, `Write`, `Read` and `Remove`.
- **Settings System (`SettingsManager`):**
  - Orchestrates the settings submenus: `AudioSettingsController`, `GraphicsSettingsController`, `ControlsSettingsController` (with interactive rebinding; the Input System logic lives in `ControlRebindSession` and `RebindOperationUtility`, and the controller only orchestrates the overlay and saving), `LanguageSettingsController` and `SensitivitySettingsController`.
  - Submenus derive from `SettingsSubMenuControllerBase<TView>`, which already handles opening/closing (saving and returning to the menu via `SettingsMenuController.ShowMenu()`); each submenu only implements `SaveSettings`, `LoadSettings` and `RestoreDefaultSettings`.
- **UI Framework (`UIViewBase` & Transitions):**
  - Animated open/close control with `UITransitionBase` (`UIFadeTransition`, `UIZoomFadeTransition`) through DOTween.
  - `ShowView`/`HideView` never discard requests: if the view is transitioning, the request is queued and the last one wins (the replaced one cancels its `UITransitionHandle`); if the view is already in the requested state, the handle completes immediately. Navigation between the settings menu and submenus also applies "last request wins" in `SettingsMenuController`.
  - Automatic keyboard/gamepad navigation through `SelectableNavigationBuilderBase` builders (`Horizontal`, `Vertical`, `Grid`).
  - Confirmation windows (`ConfirmationWindowController`) with 1- and 2-button support.

### 4b. Web Build (itch.io)

- `Assets/Editor/WebBuild/WebBuildEditor.cs`: *Tools > Web Build > Apply Release Settings* applies the web publishing configuration (Gzip with Decompression Fallback for itch.io, Strip Engine Code, Managed Stripping High, IL2CPP Optimize Size, Explicitly Thrown Only exceptions, WebAssembly 2023, geometric memory growth up to 2048 MB, 1280×720 canvas and `codeOptimization = DiskSizeLTO`, which is stored per machine in `Library/` and does not travel with the project). *Tools > Web Build > Build Release* applies it, cleans and builds Addressables and generates the player in `Builds/Web` (outside `Assets`). The `*_BurstDebugInformation_DoNotShip` folder is not uploaded.
- Default quality on Web: `Medium`. On Web there is no quit button in the main menu (`PlatformCapabilityUtility.SupportsApplicationQuit`).
- On Web, Localization initializes asynchronously: nothing may read `LocalizationSettings.AvailableLocales` before `LocalizationSettings.InitializationOperation` finishes (`LanguageSettingsController` waits for it with MEC).

### 4b2. UI Theme (`UIThemeConfig`)

- **Single source of truth:** `Assets/_ProjectName/UI/Theme/UITheme.asset` (`Scripts/UI/Theme/UIThemeConfig`) defines fonts (Lexend Bold for titles, Medium for buttons and headers, Regular for text), colors (background, surface, alternate surface, veil, control, accent, primary and secondary text), font sizes, row and button heights, padding, spacing and corner radii.
- **Application:** *Tools > UI > Apply Theme* (`Assets/Editor/UITheme/UIThemeApplierEditor`) styles the prefabs in `Prefabs/UI/BaseElements/`, the stealth package UI (pause, result banner, canvas) with its own assets, and **reverts style overrides** (font, size, color, sprite, selection colors, layout padding and spacing, CanvasScaler) in all scenes and prefabs. Run it again after changing the theme or adding UI.
- **Scaling:** `ScreenCanvas` uses *Scale With Screen Size* 1920×1080 in **Expand** mode: the 1920×1080 design area always fits entirely, at any aspect ratio, with no cropping or overlap.
- **Composition:** menus are a title (`NormalTitle`, centered at the top) and a `Card` (base prefab with a child background that ignores layout, `VerticalLayoutGroup` and `ContentSizeFitter`) containing rows (`ToggleItem`, `SliderItem`, `DropdownItem`, `InputRebinder_`) and `NormalButton`, all with a `LayoutElement` using the theme height. Secondary texts use `SmallText`, subtitles use `Subtitle`. No absolute positions inside a menu.

### 4c. DevTools (`Scripts/DevTools/`)

- **Activation:** *Settings > Developer* submenu (`DeveloperSettingsController`, key `DeveloperSettings`), which publishes `DevToolsManager.SetDevToolsEnabledRequest`. It only exists in the editor and in development builds (`PlatformCapabilityUtility.SupportsDevTools`); in release the button is hidden and `DevToolsManager` is disabled. The panel is shown or hidden with the `Debug/ToggleDevTools` action (F1 or L3 + R3). `Debug` is a persistent map: `InputManager` always enables it and does not filter it by the active map.
- **Structure:** prefab `Prefabs/DevTools/DevTools` inside `GameManager.unity`. `DevToolsManager` initializes the logic controllers (`PerformanceMonitorController`, `TimeControlController`, `InputRecorderController`, `InputDisplayController`, `ScreenCaptureController`, `SessionLogController`, `DevToolsStateController`, `DevToolsConsoleController`) and the UI ones (`DevToolsPanelController`, `DevToolsOverlayController` and one tab per `DevToolsTab` in `Tabs/<Name>/`, built on `DevToolsTabControllerBase<TView>` and `DevToolsTabViewBase`). Texts in the `DevTools` table (keys `devTools.*`); preferences in `DevToolsPreferencesData`.
- **Providers:** each game implements `IDevToolsProvider` (status section, overlay summary, `DevToolsCommandData` commands and panel visibility notification to suspend its input) and registers with `DevToolsManager.RegisterProviderRequest` / `UnregisterProviderRequest`. Reference: `GameFlow/Gameplay/StealthDevToolsProviderController`, which uses the stealth package's debug API (`StealthSessionManager.SetPlayerDetectable`, `RespawnAtCheckpoint`, `TeleportPlayerToExit`, `SetInputSuspended`).
- **Files:** CSV benchmarks, TXT logs, `.inputtrace` recordings and PNG + JSON captures in `persistentDataPath/DevTools/`, written with `FileWriteUtility` (atomic writing shared with the save system).
- **Render counters:** Unity 6.4 no longer exposes `Draw Calls Count` or `Batches Count`; draw calls are the sum of `Constants.PROFILER_DRAW_CALL_COUNTERS`.

### 5. Stealth Module (package `Assets/StealthController/`)

Tactical stealth controller demo with a portable logic core (plain C#) separated from the Unity layer. The package's usage guide (in Spanish) is [`Documentation/GUIDE.md`](Assets/StealthController/Documentation/GUIDE.md).

- **Standalone package:** everything stealth-related lives in `Assets/StealthController/` and depends on nothing from `_ProjectName` (no MVC, `InputManager`, `Globals`, DOTween, MEC or Localization). Folders: `Runtime/Core/` (assembly `StealthController.Core`), `Runtime/Unity/` (assembly `StealthController.Unity`, which only compiles if Input System, AI Navigation and uGUI are present, via `versionDefines` + `defineConstraints`), `Editor/` (assembly `StealthController.Editor` with no dependencies: `StealthProjectSetupEditor` installs those packages, imports the TMP Essentials and names layers 8/9 on import; menu *Tools > Stealth Controller > Run Project Setup*; and `Editor/Inspectors/`, assembly `StealthController.Inspectors` with the same constraints as `StealthController.Unity`: `BasicSettingsInspectorEditorBase` and its two concrete editors show first the fields marked with `[BasicSetting]` (attribute in `Core/Shared/`) grouped by their `[Header]`, and the rest in the *Advanced* foldout; a component with no basic fields uses the default inspector), `Art/`, `Prefabs/`, `Input/StealthInputActions`, `Texts/StealthTexts`, `Scenes/StealthDemo`, `Documentation/`, `README.md` and `THIRD_PARTY_NOTICES.txt`. It is exported with *Tools > Stealth Controller > Export Package* (`Assets/Editor/PackageExport/StealthPackageExportEditor`, part of the template, not the package) to `Exports/StealthController.unitypackage` (outside `Assets`); it refuses to export if there are compilation errors. The only external dependency is the `TMP_SDF` shader from the TMP Essentials, which the installer imports with its standard GUID. The package does not include tests: they remain in `Assets/_ProjectName/Tests/Editor/Stealth/`.

- **`Runtime/Core/`:** the portable core, in its own assembly `StealthController.Core` (it cannot reference the template or the adapters). Plain C# classes with no `MonoBehaviour`, `Physics`, `Time`, coroutines or DOTween; only Unity's `Vector3`, `Quaternion`, `Mathf` and `Bounds`. They advance with `Tick(float deltaTime, ...)` and see the world through `Core/Contracts/` (`ICollisionWorld`, `IPlayerInputSource`, `IPathfinder`). Serialization attributes only appear in the `…Config` classes. `StealthCoreArchitectureTests` enforces these rules. Its own constants and enums live in `Core/Shared/` (`StealthConstants`, `StealthEnums`).
  - `Components/`: one `…Component` per behavior (its config holds the parameters and its dependencies arrive through the constructor). Shared: `FacingComponent` (smoothed turning) and `VisionSensorComponent` (guards and cameras). Player: `CapsuleMotorComponent`, `PostureComponent`, `LocomotionComponent`, `MoveInputComponent`, `CrouchButtonComponent` and `WallCoverComponent` (with `WallSensorLogic` and `WallCornerTraversalLogic`). Guard: `PathFollowerComponent`, `PatrolRouteComponent` and `PlayerCaptureComponent`. Surveillance camera: `SweepComponent` and `AlertEmitterComponent`. Takedown: `TakedownAttackerComponent` (player) and `TakedownVictimComponent` (guard), with their `TakedownConfig`. `Vision/` also contains `VisionConeShapeLogic`, the cone geometry.
  - `Actors/`: `PlayerLogic`, `GuardLogic` and `SurveillanceCameraLogic` only compose components and carry their state machine (`States/`), which talks to the components, not to the actor.
  - `Camera/` (`CameraRigLogic` and `CameraOcclusionLogic`), `Mission/` (`MissionLogic`) and `Session/` (`EnemyCoordinatorLogic`, `StealthSessionLogic` and `TakedownLogic`).
- **`Runtime/Unity/`:** the only part that knows about Unity. Its behaviours derive from `Lifecycle/StealthBehaviourBase` (same `Initialize`/`Conclude`/`AddListeners`/`RemoveListeners`/`RequireDependency` lifecycle as the template, copied to stay standalone). `Services/` implements the collision and navigation contracts (`UnityCollisionWorldAdapter`, `NavMeshPathfinderAdapter`); `Input/` reads its own actions asset (`InputActionsAdapter`, `PlayerInputAdapter` and `InputBindingDisplayAdapter`, which provides the short key label based on the last device used). `Actors/` creates the logic with its serialized configs and applies the poses (`PlayerAdapter.ApplyPose`/`SnapPose`, `GuardAdapter`, `SurveillanceCameraAdapter`, `CameraRigAdapter.ApplyFraming`). `Presentation/` renders: `PlayerAnimationAdapter` and `GuardAnimationAdapter` (built on `CharacterAnimationAdapterBase`) translate the poses into parameters (`Speed`, `WallLateral` and a per-transition progress) and into `CrossFade`; `VisionConeAdapter` uploads the cone mesh; `CameraOcclusionAdapter` publishes the shader globals. `Authoring/` contains checkpoints, exit and waypoints. Unity-only constants (animator, shader, cone alpha, gizmos, input, text keys and UI) live in `Runtime/Unity/StealthUnityConstants.cs` and its enums in `StealthUnityEnums.cs`. `Game/StealthGameController` boots the game (session + UI; `Initialize On Start`, or a host calls it), `UI/` contains the views with their own fade (`FadeViewBase`, no DOTween) and `Texts/` the keyed texts (`TextTableData`, `LocalizationAdapter` and `LocalizedTextView`; English and Spanish, system language or the one set by `SetLanguage`).
- **Characters:** a single Mixamo Humanoid model in `Art/Characters/Mixamo/Model/`, clips in `Mixamo/Animations/`, controllers `AC_Player` and `AC_Guard` in `Art/Characters/Animators/` and per-role materials in `Art/Characters/Materials/` (paths inside the package). The Animator never applies root motion: movement is driven by the core. `AC_Player` uses a single layer with one state per posture (`Standing`, `Crouched`, `Prone`, `WallStanding`) and the transitions `StandToCrouch`, `CrouchToStand`, `CrouchToProne` and `ProneToCrouch`, each with its own progress parameter (`StandToCrouchProgress`, etc.) as clip time, so they finish at the same time as the logic and so that a fading-out transition does not jump when the next one restarts (when chaining prone → crouched → standing). The blend trees scale each clip's time so its natural speed matches the core's (no foot sliding).
- **Game rules:** walls are only used while standing and with corners; crouching against a wall detaches you. The camera is a fixed top-down framing with follow. Checkpoints activate in order. `EnemyCoordinatorLogic` distributes camera alerts to nearby guards.
- **Session:** `StealthSessionLogic` (core) orders each frame: player → enemies → mission → camera. A capture or reaching the exit freezes the session until `ResolvePendingOutcome()`, which resets enemies and player at the active checkpoint (or the whole mission when reaching the exit). `RestartMission()` goes back to the first checkpoint with the enemies reset. `StealthSessionManager` builds everything, enables the `Player` map of its copy of `StealthInputActions` for the duration of the session (and `UI` while paused), applies the poses after each `Tick` and emits `OutcomeReached`; `StealthGameController` shows the banner ("Captured" / "Zone cleared") and calls `ResolvePendingOutcome()`. In the template, `GameplayState` calls `StealthGameController.Initialize()`/`Conclude()` (with `Initialize On Start` turned off on the instance), passes it the language from `LocalizationSettings` and listens to `ExitRequested` to return to `Main`.
- **Camera occlusion:** wall, cover, obstacle and duct materials use `Art/Shaders/StealthOcclusionCutoutLit.shader` (a copy of URP Lit without comments, with cutout in ForwardLit, GBuffer, DepthOnly and DepthNormals; shadows are not cut out). `CameraOcclusionLogic` decides by raycast whether something blocks the player and how much the cylindrical tunnel between camera and player opens; `CameraOcclusionAdapter` publishes it in the `_StealthCutout*` globals (names in `StealthUnityConstants`). The floor uses the regular Lit so the tunnel does not cut through it.
- **Vision cones:** `VisionConeShapeLogic` computes the visibility polygon (horizontal rays from the eye origin, with binary search at corners) as a ring mesh projected onto the floor, which `VisionConeAdapter` uploads to its `Mesh`, with the floor plane (point and normal) cached per cell so each vertex gets its exact height on slopes and there is no z-fighting; a sharp rise between rings is treated as the top of an obstacle, not as floor. Cameras look from the floor beneath them plus `sightHeight` (1.6 m, like a guard), so walls and cover block equally in the logic and in the cone, and they have a blind zone (`VisionConfig.minRange`, 1.8 m) that the logic respects and the cone leaves visible.
- **Stealth input:** the package uses its own `Input/StealthInputActions` (`Player` map: `Move`, `Sprint`, `Crouch`, `Stand` with Space / south button, `Takedown` with E / left click / right trigger and `Pause` with Escape / Start; `UI` map for the pause menu). The template's `.inputactions` keeps the same actions (`Stand` and `Takedown` are the former `Jump` and `Attack`), but the stealth module no longer uses it. Standing up is triggered by `Stand` or by starting a sprint (Sprint + movement) from crouched or prone.
- **Map:** `Waterworks` is the package prefab `Prefabs/Maps/Waterworks` (offset +200 on X, its own NavMesh in `Prefabs/Maps/NavMesh-Waterworks.asset`), a linear route of corridors at three heights (0, 1.5 and 3 m) joined by ramps: entrance corridor with nooks and a fallen beam that can only be crossed prone, pipe gallery with a wall of windows onto the pump room (the guard looks through them: crouched next to the low wall he cannot see you), ramp to the container warehouse, warehouse exit under a pipe that can only be crossed crawling, ramp to the elevated walkway, control building with another pipe to crawl under and a ramp down to the exit. The crawl passages cut the NavMesh, so guards do not cross them. It has 8 guards with back-and-forth routes or loops and 4 cameras with the sweep configured in `cameraConfig`. The checkpoints are the start, the top of ramp 1, the foot of ramp 2 (on leaving the warehouse pipe, ~16 m from the ramp camera; the walkway guard patrols between x 216 and 243) and the control building door. The whole game is the prefab `Prefabs/Game/StealthGame` (root with `StealthGameController`; children `StealthSessionManager`, `Player`, `StealthCameraRig`, `MainCamera`, `Directional Light`, `Waterworks` with `Enemies/Guards`, `Enemies/SurveillanceCameras` and `Enemies/Routes`, and the `UI` canvas). `Gameplay.unity` hierarchy: `GameplayState` in `--- Managers ---`, the `StealthGame` instance in `--- Controllers ---` and the `StealthTestRoom` test room in `--- LevelDev ---`. The package scene `Scenes/StealthDemo` only has an `EventSystem` and `StealthGame`. The former maps `RailDepot` and `StealthMap` are not part of the package: they are template prefabs in `Prefabs/StealthLegacyMaps/`. To go back to one, instantiate it and reassign checkpoints, exit, guards and cameras in `StealthSessionManager`.
- **Takedown from behind:** `TakedownLogic` (in the session) looks each frame for a guard on patrol, not seeing the player, within 2 m and within ±60° of its back, with the player standing or crouched, no wall between the two and room for the animation (the start point 1.85 m behind the guard and the exit point). On pressing `Takedown`, the player switches to `PlayerPoseState.Takedown` and the guard to `GuardAlertState.TakenDown`, and the two paired Mixamo clips (`StealthTakedownAttacker` / `StealthTakedownVictim` in `Mixamo/Animations/`, with the translation baked into the pose) advance with the same time through the `TakedownProgress` parameter of `AC_Player` (state `Takedown`) and `AC_Guard` (state `TakenDown`). The approach part the player has already covered is skipped. At 5.5 s the player is standing again at the exit point and the guard stays down, with no cone, unable to see or capture, until respawn. The prompt is a keycap with just the key above the guard's head (`TakedownPromptController` / `TakedownPromptView`, package prefab `Prefabs/UI/TakedownPrompt`, which nests `ActionPrompt` without a label) and follows the device in use.
- **Posture indicator:** `PostureHudController` / `PostureHudView` (in `Runtime/Unity/UI/`, package prefabs `Prefabs/UI/PostureHud` and `ActionPrompt`) shows in the bottom-left corner, over a gradient (`Art/UI/UI_HudCornerGradient.png`), the silhouette and name of the current posture and a list of actions with keycaps: always Sprint and the two postures that can be switched to (Stand, Crouch, and Crouch with the "Hold" pill for prone). Each `ActionPromptView` is a row of the `ActionPrompt` prefab. The controller listens to `StealthSessionManager.PlayerPoseStateChanged` and reads the keys of the device in use with `InputBindingDisplayAdapter.ShortLabelFor` (short names such as Shift or LS). Icons in `Art/UI/Posture/` (MingCute, Apache 2.0, and a custom one) and the Lexend font (OFL) copied into `Art/Fonts/Lexend/`; see the package's `THIRD_PARTY_NOTICES.txt`.
- **Pause:** the `Player/Pause` action (Escape / Start) is listened to by `StealthGameController`; `SetPaused(true)` freezes the session and switches to the `UI` map, where `PauseMenuView` (Resume / Restart mission / exit) closes with `UI/Cancel`. The exit button ("Main menu") only appears if something listens to `ExitRequested`, as the template does.
- **Tests:** the core is tested in `Tests/Editor/Stealth/` with fake collision worlds and pathfinders (`PlaneCollisionWorldFake`, `StraightLinePathfinderFake`). `StealthGoldenMasterTests` replays a scripted run with real physics through `StealthSessionLogic` and compares it with the trace recorded in `GoldenMaster/StealthGoldenMasterTrace.txt`. Any core refactor must keep it intact; it is only re-recorded by deleting the file when a behavior change is intentional.

---

## Coding Rules

### Naming and Structure
1. **Mandatory suffixes by role:** `…Controller`, `…Manager`, `…View`, `…Data`, `…Config`, `…Base` (abstract), `…Editor` (editor tools), `…Utility` (stateless static classes), `…Tests` (EditMode tests in `Assets/_ProjectName/Tests/Editor/`), `…Logic` (stateful systems of the portable core: actors, session, mission, camera and probes), `…Component` (a reusable behavior of the portable core; only in `Stealth/Core/Components/`), `…State` (states of a core state machine), `…Adapter` (bridge between the portable core and Unity), `…Fake` (test doubles, only in `Tests/Editor/`).
2. **Interfaces:** Prefixed with `I` + role (`ISaveSystem`, `IInputController`).
3. **Visibility:** Private fields in `camelCase`; public properties in `PascalCase`, preferably expression-bodied (`public ViewBase View => view;`).
4. **Constants:** `SCREAMING_SNAKE` in `Constants.cs`. Exception: a portable module (currently the `StealthController` package) has its own constants and enums to stay standalone. `Runtime/Core/Shared/` holds `StealthConstants` and `StealthEnums`, everything the core uses. `Runtime/Unity/StealthUnityConstants.cs` and `StealthUnityEnums.cs` hold what only the Unity layer uses, and `Editor/StealthEditorConstants.cs` what the installer uses. Their names do not carry the `STEALTH_` prefix.
4b. **Magic numbers are forbidden:** every meaningful numeric literal (thresholds, speeds, durations, distances, angles, factors, `[Range]`/`[Min]` limits, buffer sizes, layer or axis indices) is declared as a named constant in `Constants.cs`, grouped by `#region`. Default values of serialized fields are also taken from `Constants.cs`. Only exceptions: `0` and `1` as identity, counter or increment; `0.5f` and `2f` as an obvious geometric half or double (center of a capsule, diameter from radius). In tests, times and tolerances use named constants and only scenario position data may be inlined.
5. **Folder = Layer:** The file location reflects its role in the architecture.

### MVC and Responsibility
6. **The View knows its Controller:** In UI, the view receives its controller through `Initialize(params object[] parameters)` and reports actions directly to it.
7. **`…Data` operates only on its own data:** It computes and encapsulates its internal state; it does not access the scene or manipulate the view.
8. **Dependency caching:** Local components are cached in `Awake()`. External dependencies are received in `Initialize(...)`.
9. **Null handling:** Every mandatory dependency (injected via `Initialize` or `[SerializeField]`) is validated on initialization and, if null, immediately reports `Debug.LogError(message, this)`, preventing silent error propagation.
10. **UI controllers separated from non-UI ones:** Every controller that manages a UI view derives from `UIControllerBase<TView>`, which holds the view (`[SerializeField] view` field in the base, exposed as `View`) and fires `OnViewOpened()` / `OnViewClosed()` when it opens or closes. The concrete controller does not declare its own view field. `ControllerBase` is reserved for controllers without a view (logic, input, gameplay). A UI component that only captures interaction and does not govern any view is not a controller: it is a `…View` that derives from `ViewBase`.

### Lifecycle and Events
11. **`Awake()`:** Only internal initialization and caching of components on the same GameObject. No complex cross-references or game logic.
12. **`Initialize(...)` and `Conclude()`:** Flow initialization, calls to `AddListeners()` / `RemoveListeners()`, data instantiation and coroutine cleanup (`Timing.KillCoroutines`).
13. **Strict symmetry:** Every event subscribed in `AddListeners()` must be unsubscribed in `RemoveListeners()`.
14. **Event buses (`static Action`):** Whoever declares the bus manages its lifecycle; whoever raises it always does so with safe invocation (`?.Invoke`).

### Unity and Inspector
15. **Serialization:** Always `[SerializeField] private`, never public fields. Group with `[Header]`.
16. **Validation:** `[Min]`, `[Range]` or `[SerializeField, Tooltip]` attributes where applicable.
17. **Safe renaming:** When renaming an existing serialized field, add `[FormerlySerializedAs("oldName")]`.
18. **Coroutines:** Exclusively with **MEC** (`Timing.RunCoroutine(Routine().CancelWith(gameObject))`). Store the `CoroutineHandle` if it needs to be stopped.
19. **Code-driven animations:** With **DOTween**, returning `Tween` or `Sequence` if the caller must chain or wait for completion.
20. **Debug and Gizmos:** Drawing or test blocks must be wrapped in `#if UNITY_EDITOR` and depend on explicit flags.

### Performance and Memory
21. **Zero unnecessary allocations in Updates:** Do not `new` lists, dictionaries or collections in recurring methods; cache and clear (`Clear()`).
22. **`GetComponent` at runtime:** Only during initialization (`Awake`/`Initialize`) or when evaluating dynamic collision/raycast results.

### Localization and Text
23. **UI always localized:** Do not write fixed translatable strings to `TMP_Text.text`. Use `LocalizeStringEvent` with Localization tables in `Assets/_ProjectName/Localization/`.
24. **Texts with variables:** Update variables through `StringReference.TryGetValue(...)` and call `RefreshString()`.
25. **Purely numeric texts:** Counters or direct math values (`"10/100"`) may be written directly to `TMP_Text`.

### Code Style
26. **Guard Clauses:** Early `return` and validations at the start of methods. Avoid deep `if` nesting.
27. **Comments are forbidden:** NEVER write comments (`//`, `/* */`, `///`) in `Assets/_ProjectName/` or in `Assets/Editor/`. Code must be 100% self-documenting; intent is expressed exclusively through clear names of classes, methods, properties and variables. Use `#region` to group members. Does not apply to `Assets/Plugins/` (third-party code, not modified).
28. **Language:** All code (types, variables, methods, logs and headers) in **English**. User-facing Spanish and English live in the Localization tables.
29. **Explicit types:** Use concrete types instead of `var`, including `foreach (KeyValuePair<K, V> pair in ...)` and `out Type name`.
30. **Mutable Singleton pattern is forbidden:** Use dependency injection or decoupled static event buses.

### Editor and Version Control
31. **Editor scripts:** General tools in `Assets/Editor/`; specific drawers in an `Editor/` subfolder next to the feature.
32. **Saving asset changes:** In editor tools, mark `EditorUtility.SetDirty` and persist with `AssetDatabase.SaveAssets()` or `EditorSceneManager.SaveScene`.
33. **Git:** Small, focused commits. Generated folders (`Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Builds/`) are never committed.
34. **Prior search and anti-duplication (DRY & SOLID):** Before implementing a new feature, helper or logic, you **MUST search the existing codebase** to check whether it is already solved. If that logic exists or duplication is detected, it must be refactored and moved to a centralized class, base component or utility accessible to all the parts that need it, strictly following SOLID principles.
35. **Verification after changes:** After modifying scripts, check that they compile without errors in the Unity console and that the EditMode tests in `Assets/_ProjectName/Tests/Editor/` pass before considering the task done. After creating or moving flow scenes, verify their entry in the `Scenes` Addressables group.
36. **UI Focus Management:** When opening or closing UI views (`UIViewBase`), always ensure the correct interactive element is selected (`SelectDefaultItem()` or `SelectCurrentItem()`) to keep keyboard and gamepad navigation smooth.
37. **Serialized assets through the Editor:** Scenes, prefabs, GameObjects, components and serialized assets are modified in the Unity Editor or through editor scripts, never by hand-editing the YAML files (`.unity`, `.prefab`, `.asset`, `.meta`).

### Prefabs and UI
38. **Prefabs mandatory for reuse and runtime:** Every element used more than once, and especially everything instantiated at runtime, must be a prefab in `Assets/_ProjectName/Prefabs/`. Before creating a new one, search for an existing prefab and reuse it. Assembling loose duplicates in the scene is forbidden.
39. **UI is composed from base prefabs and nested hierarchy:** Every button, text, toggle, slider, dropdown and window must instantiate the prefabs in `Prefabs/UI/BaseElements/` and use prefab variants to specialize, never disconnected copies. Prefab nesting is encouraged so a change to the base propagates to every use. Scenes only contain positioned instances, not definitions.
40. **Localization Organization and Conventions:** Unity Localization tables are organized under `Assets/_ProjectName/Localization/`, split into thematic or per-screen folders (e.g. `MainMenu/`, `SettingsMenu/AudioSettings/`, `CommonActions/`). Each table strictly follows the naming pattern `<TableName>.asset` (String Table Collection), together with `<TableName> Shared Data.asset`, `<TableName>_en.asset` and `<TableName>_es.asset`. Every translatable UI text must belong to its corresponding table; creating loose tables at the root or hardcoded texts is forbidden.
41. **Style only from the theme:** font, size, color, sprite and selection colors are not overridden on instances of the base prefabs; any visual change goes in `UITheme.asset` or in the base prefab and is applied with *Tools > UI > Apply Theme*. Menus are composed with `Card` and layout groups, never with absolute positions.

---

> **Note on writing style:** For the exact details of syntax, member ordering in classes and code formatting conventions in this repository, see the dedicated document [`CODING_STYLE.md`](CODING_STYLE.md).

---

## Quick Code Map

| Path | Description |
|---|---|
| `Assets/_ProjectName/Scripts/GameManager/` | Persistent bootstrap and state management |
| `Assets/_ProjectName/Scripts/GameStates/` | `Base/GameStateBase` and flow states in `GameFlow/` (`MainState`, `GameplayState`) |
| `Assets/_ProjectName/Scripts/GameFlow/` | Controllers and views per state (`Main/`, `Gameplay/`) |
| `Assets/_ProjectName/Scripts/MVCBase/` | Architecture base classes (`LifecycleBehaviourBase`, `ManagerBase`, `ControllerBase`, `UIControllerBase`, `ViewBase`, `UIViewBase`, `CancelableUIViewBase`, `UITransitionHandle`) |
| `Assets/_ProjectName/Scripts/InputSystem/` | `InputManager`, input subscriptions, data and requests; `Controllers/ClickButtonOnSubmitController` (auxiliary submit); `Utilities/DeviceTypeUtility` (classify devices and bindings), `Utilities/InputBindingDisplayUtility` (visible binding and its text based on the device) and `Utilities/RebindOperationUtility` (rebind configuration and validation) |
| `Assets/_ProjectName/Scripts/SaveSystem/` | Persistence system (`SaveSystem`, `JSONSaveSystem`, etc.) |
| `Assets/_ProjectName/Scripts/Settings/` | Manager and settings submenus (Audio, Graphics, Controls, Language, Sensitivity) |
| `Assets/_ProjectName/Scripts/UI/` | Transitions, assisted navigation, confirmations, loading effects, `Extensions/` (e.g. `ObservableDropdown`), and `ScrollFocus/` |
| `Assets/_ProjectName/Scripts/SceneLoading/` | `SceneLoadingManager` (additive loading with Addressables) |
| `Assets/_ProjectName/Scripts/UserData/` | Serializable user data model |
| `Assets/_ProjectName/Scripts/General/` | `Enums.cs`, `Constants.cs`, `Globals.cs` |
| `Assets/_ProjectName/Scripts/Utilities/` | Debug utilities (`DebugReport`), text formatting (`Formatting/TextFormatUtility`) and per-platform capabilities (`Platform/PlatformCapabilityUtility`) |
| `Assets/_ProjectName/Localization/` | Localization tables (`MainMenu`, `SettingsMenu`, `CommonActions`, `Miscellaneous`, `DevicesNames`, `InputActionNames` and `Gameplay`, the latter unused since the stealth UI lives in the package) and configuration in `_Settings/` |
| `Assets/_ProjectName/Scenes/` | `GameManager.unity` scene and flow in `GameFlow/` (`Main.unity`, `Gameplay.unity`) |
| `Assets/_ProjectName/Prefabs/UI/BaseElements/` | Base UI library with nesting: see [`ASSETS_MAP.md`](ASSETS_MAP.md) |
| `Assets/StealthController/` | Standalone stealth package: `Runtime/Core/` (`Shared/`, `Contracts/`, `Components/`, `Actors/`, `Camera/`, `Mission/`, `Session/`), `Runtime/Unity/` (`Lifecycle/`, `Services/`, `Input/`, `Actors/`, `Presentation/`, `Authoring/`, `Manager/`, `Game/`, `UI/`, `Texts/`), `Editor/`, `Art/`, `Prefabs/` (`Game/`, `Characters/`, `Gameplay/`, `Blockout/`, `Maps/`, `UI/`), `Input/`, `Texts/`, `Scenes/` and `Documentation/` ([`GUIDE.md`](Assets/StealthController/Documentation/GUIDE.md)) |
| `Assets/_ProjectName/Prefabs/StealthLegacyMaps/` | Former maps `RailDepot` and `StealthMap` (outside the package) |
| `Assets/_ProjectName/Scripts/DevTools/` | Development tools: `Manager/`, `Controllers/` (logic, panel and overlay), `Tabs/` (one folder per tab), `Views/`, `Data/`, `Contracts/` (`IDevToolsProvider`) and `Utilities/` |
| `Assets/_ProjectName/Tests/Editor/` | EditMode tests (NUnit, assembly `Assembly-CSharp-Editor`): save system, rebind and platform utilities, settings data, `InputData`, `UITransitionHandle`, DevTools (`DevToolsTests`) and the stealth module in `Stealth/` (components, actors, reference trace and core architecture) |
| `Assets/Editor/` | `SceneBootstrapper`, `SelectableNavigationBuilderEditor`, `WebBuild/WebBuildEditor` (web build) and `PackageExport/StealthPackageExportEditor` (stealth package export) |
