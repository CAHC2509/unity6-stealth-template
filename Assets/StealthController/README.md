# Stealth Controller

Top-down tactical stealth controller for Unity 6 (URP): player with postures (stand, crouch, prone), wall cover with corners, sprint, back takedown; guards with patrol, chase and search; surveillance cameras with sweep and alerts; checkpoints, mission exit, HUD, pause menu and an example map (Waterworks).

The game logic is plain C# with no engine calls, kept apart from the Unity layer that shows it. See `Documentation/GUIDE.md` for how everything works, how it is organised, the inspector settings and how to build a new level.

## Requirements

- Unity 6 (6000.4 or newer), project created from the **Universal 3D (URP)** template.
- Packages: Input System, AI Navigation and uGUI (TextMesh Pro). The Universal 3D template already includes them.
- Player setting **Active Input Handling**: "Input System Package (New)" or "Both".

## Install

1. Import `StealthController.unitypackage` (Assets > Import Package > Custom Package).
2. On import, the setup script (`Editor/StealthProjectSetupEditor.cs`) runs once:
   - installs Input System, AI Navigation or uGUI if any is missing;
   - imports the TextMesh Pro Essential Resources if they are missing;
   - names layers 8 and 9 `Player` and `Enemy` if they are free;
   - warns in the console if URP or the new Input System is not active.
   If it needs to run again: **Tools > Stealth Controller > Run Project Setup**.
3. Open `Scenes/StealthDemo.unity` and press Play.

## Controls

| Action | Keyboard / mouse | Gamepad |
|---|---|---|
| Move | WASD | Left stick |
| Sprint | Left Shift | Left stick press |
| Crouch (hold: go prone) | C | East button |
| Stand up | Space | South button |
| Takedown (from behind) | E / left click | Right trigger |
| Pause | Escape | Start |

## Folders

| Folder | Content |
|---|---|
| `Runtime/Core/` | Portable game logic, assembly `StealthController.Core`, namespace `StealthController.Core`. No Unity engine calls besides math types. |
| `Runtime/Unity/` | Unity layer, assembly `StealthController.Unity`: adapters (`Services/`, `Actors/`, `Presentation/`, `Authoring/`, `Input/`), session (`Manager/`), game bootstrap (`Game/`), UI (`UI/`) and texts (`Texts/`). |
| `Editor/` | Project setup script, assembly `StealthController.Editor` (no dependencies, so it compiles before the packages exist), and `Inspectors/` (assembly `StealthController.Inspectors`): simplified inspectors that show the basic settings first and the rest under **Advanced**. |
| `Art/` | Characters (Mixamo model, clips, animator controllers, materials), blockout materials, occlusion shader, UI sprites, Lexend font. |
| `Prefabs/` | `Game/StealthGame` (whole game: session, player, camera, light, map and UI), `Characters/`, `Gameplay/` (checkpoint, exit, surveillance camera), `Blockout/`, `Maps/Waterworks` (with its NavMesh) and `UI/`. |
| `Input/` | `StealthInputActions` (maps `Player` and `UI`). |
| `Texts/` | `StealthTexts`: UI texts by key in English and Spanish. |
| `Scenes/` | `StealthDemo`: EventSystem + `StealthGame`. |
| `Documentation/` | `GUIDE.md`: user guide. |

## Inspector

Each game object shows only its basic settings (speeds, turning, vision, sweep, camera framing, route, mission pieces), each with a tooltip. Everything else is under the collapsed **Advanced** section. A field shows up among the basic settings when it has the `[BasicSetting]` attribute.

## Using it in your own game

- Drop `Prefabs/Game/StealthGame` into a scene that has an EventSystem with an `InputSystemUIInputModule`. It starts on its own.
- To start it from your own code, turn off **Initialize On Start** in `StealthGameController` and call `Initialize()` / `Conclude()`.
- `StealthGameController.SetLanguage("en" | "es")` changes the UI language; by default it follows the system language.
- Subscribe to `StealthGameController.ExitRequested` to show an extra pause button (for example "Main menu"); the button stays hidden while nobody listens.
- Text: add keys and languages in `Texts/StealthTexts`.
- New level: build it with the blockout prefabs, bake a NavMeshSurface, place `Checkpoint`, `MissionExit`, guards (with waypoints) and surveillance cameras, and assign them in `StealthSessionManager`.
