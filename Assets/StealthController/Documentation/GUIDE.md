# Stealth Controller: user guide

Top-down tactical stealth controller for Unity 6 (URP). This guide explains what the package includes, how to get it running, how it is organized internally, and how to tune it or build new levels without programming.

---

## 1. What's included

- **Player** with three stances (standing, crouched and prone), sprinting, wall cover (with corner turning) and takedowns from behind.
- **Guards** that patrol a route, chase the player when they see them and search when they lose track.
- **Surveillance cameras** that sweep from side to side and alert nearby guards.
- **Mission** with checkpoints (respawn points) and an exit that completes the zone.
- **Top-down game camera** that follows the player and opens a "tunnel" through walls that block the view.
- **UI**: stance indicator with the available keys, takedown prompt, result banner ("Captured" / "Zone cleared") and pause menu.
- **Sample map** (Waterworks) with 8 guards and 4 cameras.
- **Texts** in English and Spanish.

---

## 2. Getting started

1. Create a project with Unity 6's **Universal 3D (URP)** template.
2. Import `StealthController.unitypackage` (*Assets > Import Package > Custom Package*).
3. On import, a setup wizard runs automatically that:
   - installs Input System, AI Navigation and uGUI if missing;
   - imports the TextMesh Pro essential resources;
   - names layers 8 and 9 as `Player` and `Enemy` if they are free;
   - warns in the console if URP or the new Input System is not active.

   To run it again: *Tools > Stealth Controller > Run Project Setup*.
4. Open `Scenes/StealthDemo` and press Play.

### Controls

| Action | Keyboard / mouse | Gamepad |
|---|---|---|
| Move | WASD | Left stick |
| Sprint | Left Shift | Left stick press |
| Crouch (hold: prone) | C | East button |
| Stand up | Space | South button |
| Takedown (from behind) | E / left click | Right trigger |
| Pause | Escape | Start |

---

## 3. How it works

The package is split into two layers that never mix:

| Layer | Folder | What it does |
|---|---|---|
| **Logic** | `Runtime/Core/` | The game rules: how the player moves, what a guard sees, when a capture happens. It is pure code: it knows nothing about scenes, physics, animations or screens. |
| **Presentation** | `Runtime/Unity/` | Connects the logic to Unity: reads input, answers collision and navigation queries, moves scene objects, plays animations and draws the UI. |

Thanks to this separation, the logic can be tested, reused or ported to another platform without touching the presentation, and vice versa.

### The per-frame cycle

```
 Input (keyboard / gamepad)
        │
        ▼
 Session ──▶ 1. Player ──▶ 2. Enemies ──▶ 3. Mission ──▶ 4. Camera
        │      (stances,       (guards and       (checkpoints     (framing and
        │       wall,           surveillance      and exit)        occlusion)
        │       takedown)       cameras)
        ▼
 Resulting poses ──▶ Presentation (position, animation, vision cones, UI)
```

1. The **presentation** collects the frame's input.
2. The **session** advances all the logic in a fixed order: player, enemies, mission and camera.
3. Each part of the logic produces a **pose** (position, rotation, stance, alert state…).
4. The **presentation** applies those poses to the scene objects and their animations.

The logic queries the world through three very small "contracts", which the presentation implements with Unity:

- **Collisions**: capsule sweeps, raycasts and overlaps against the level.
- **Navigation**: paths between two points for the guards.
- **Input**: a summary of the frame (movement direction and buttons pressed or released).

### Components

Each behavior is an independent piece (a *component*) with its own parameters and the pieces it needs to work. For example, vision is a component used by both guards and surveillance cameras, and smoothed turning is shared by the player and the guards.

| Component | Used by | Does |
|---|---|---|
| Capsule motor | Player | Moves the body against the level: collisions, steps, slopes and gravity. |
| Stance | Player | Switches between standing, crouched and prone, and the body size in each. |
| Locomotion | Player | Accelerates and decelerates toward the desired speed. |
| Movement input | Player | Converts input into velocity based on stance and sprinting. |
| Crouch button | Player | Distinguishes a press (crouch) from a hold (prone). |
| Wall cover | Player | Detects walls, presses the back against them and turns corners. |
| Turning | Player, guards | Smoothly turns the body toward a direction. |
| Vision sensor | Guards, cameras | Checks whether the player is inside the cone and unobstructed for the confirmation time. |
| Path follower | Guards | Walks along the path provided by navigation. |
| Patrol route | Guards | Goes through the route points and waits at each one. |
| Capture | Guards | Catches the player on reaching them. |
| Sweep | Cameras | Rotates from side to side with pauses at the ends. |
| Alert emitter | Cameras | Alerts nearby guards on seeing the player. |
| Takedown attacker / victim | Player / guards | Synchronize the takedown animation of both characters. |

The **actors** (player, guard and surveillance camera) only combine components and decide which state they are in. For example, the player goes through the states *Standing*, *Crouched*, *Prone*, *On wall* and *Takedown*, and the guard through *Patrol*, *Chase* (which includes searching) and *Taken down*.

### Game rules

- The wall can only be used while standing; crouching against the wall detaches you from it.
- Checkpoints activate in order: a new one only counts if its number is not lower than the active one.
- When captured, the player and the enemies return to the last checkpoint. On reaching the exit, the mission restarts from the beginning.
- A takedown is only possible from behind a guard who is patrolling and does not see the player, while standing or crouched, close by, with no wall between the two and with room for the animation.
- A guard who has been taken down stays down, unable to see or capture, until the next respawn.
- Surveillance cameras do not chase: they alert the guards within their alert radius.

---

## 4. Folder structure

```
Assets/StealthController/
├── Runtime/
│   ├── Core/             Game logic (StealthController.Core assembly)
│   │   ├── Shared/       Constants, enums and the BasicSetting attribute
│   │   ├── Contracts/    Collision, navigation and input contracts, and the poses
│   │   ├── Components/   One component per behavior, with its configuration
│   │   ├── Actors/       Player, guard and surveillance camera, with their states
│   │   ├── Camera/       Game camera framing and occlusion
│   │   ├── Mission/      Checkpoints and exit
│   │   └── Session/      Frame order, enemy coordination and takedown
│   └── Unity/            Presentation (StealthController.Unity assembly)
│       ├── Lifecycle/    Base class with the Initialize / Conclude cycle
│       ├── Services/     Collision and navigation implementation with Unity
│       ├── Input/        Input reading and key names per device
│       ├── Actors/       Player, guard, surveillance camera and game camera in the scene
│       ├── Presentation/ Animations, vision cone mesh and occlusion
│       ├── Authoring/    Checkpoints, exit and route points
│       ├── Manager/      The session: builds everything and advances it every frame
│       ├── Game/         Game startup, pause and banners
│       ├── UI/           UI views
│       └── Texts/        Text table and language
├── Editor/               Setup wizard and simplified inspectors
├── Art/                  Characters, materials, occlusion shader, sprites and font
├── Prefabs/              Full game, characters, level pieces, map and UI
├── Input/                Input actions (Player and UI maps)
├── Texts/                Texts in English and Spanish
├── Scenes/               Demo scene
└── Documentation/        This guide
```

### Main prefabs

| Prefab | Purpose |
|---|---|
| `Prefabs/Game/StealthGame` | The full game: session, player, camera, light, map and UI. Just drop it into a scene. |
| `Prefabs/Characters/` | Player and guard. |
| `Prefabs/Gameplay/` | Checkpoint, mission exit and surveillance camera. |
| `Prefabs/Blockout/` | Walls, floors, ramps, cover and obstacles for building levels. |
| `Prefabs/Maps/Waterworks` | Sample map with its navigation mesh. |
| `Prefabs/UI/` | Stance indicator, takedown prompt, banner and pause menu. |

---

## 5. Tuning the game from the inspector

Each game object shows only its **basic settings** at the top, and each setting explains what it does when you hover over its name. The remaining options (collision, sensors, internal references, colors, etc.) are in the collapsible **Advanced** section, closed by default. You don't need to touch it to tune the game.

| Object | Basic settings |
|---|---|
| **Player** (`PlayerAdapter`) | Walking, sprinting, crouched and prone speed; acceleration and deceleration; turn speed and smoothing (standing and prone); crouch/stand-up and go-prone duration; how long the button must be held to go prone; wall movement speed. |
| **Guard** (`GuardAdapter`) | Patrol and chase speed; turn speed; vision range, angle and confirmation time; capture distance; search duration; route points. |
| **Surveillance camera** (`SurveillanceCameraAdapter`) | Sweep angle, speed and pause; time between alerts; vision range, angle and confirmation time. |
| **Game camera** (`CameraRigAdapter`) | Pitch, yaw, distance and field of view; how far it leads the player; follow smoothing; occlusion tunnel on or off. |
| **Session** (`StealthSessionManager`) | Checkpoints, exit, guards and cameras that make up the mission; camera alert radius; takedown distance and angle. |
| **Checkpoint** and **exit** | Area size; checkpoint order. |
| **Game** (`StealthGameController`) | Start automatically when the scene begins; how long the result banner is shown. |

For a setting to appear among the basic ones, its field has the `[BasicSetting]` attribute in code. Removing it moves the setting to **Advanced**.

---

## 6. Building a new level

1. Build the geometry with the prefabs in `Prefabs/Blockout/`. Everything that collides, blocks sight or serves as cover goes on the `Default` layer.
2. Add a `NavMeshSurface` and generate the navigation mesh (*Bake*) so the guards know where to walk.
3. Place the **player** at the starting point.
4. Place **checkpoints** (the one with the lowest number is the mission start) and an **exit**.
5. Place **guards** and create their route: add empty objects as points and assign them in *Route*. For each point you can set how long the guard waits and whether it faces forward.
6. Place **surveillance cameras** facing the center of their sweep.
7. In the **session** (`StealthSessionManager`), assign the checkpoints, the exit, the guards and the cameras.
8. Press Play.

The fastest way is to duplicate the `StealthGame` prefab and replace the Waterworks map with your own.

---

## 7. Using it inside another game

- Put `Prefabs/Game/StealthGame` in a scene with an `EventSystem` that uses `InputSystemUIInputModule`. It starts on its own.
- To start it from your own code, disable **Initialize On Start** in `StealthGameController` and call `Initialize()` / `Conclude()`.
- `StealthGameController.SetLanguage("en" | "es")` changes the language; by default it follows the system language.
- If you subscribe to `StealthGameController.ExitRequested`, the pause menu shows an extra button to exit (for example "Main menu").

### Texts and languages

All UI texts are in `Texts/StealthTexts`, by key and by language. To change a text, edit its entry. To add a language, add its code (for example `fr`) to the language list and its translation to each entry.

### Controls

The actions are in `Input/StealthInputActions`. Keys can be rebound without touching code; the stance indicator always shows the key bound on the device currently in use.

---

## 8. Common problems

| Symptom | Likely cause |
|---|---|
| The package scripts don't compile or don't show up | Input System, AI Navigation or uGUI is missing. Run *Tools > Stealth Controller > Run Project Setup*. |
| Texts show as boxes or don't show at all | The TextMesh Pro essential resources are missing. Run the setup wizard. |
| Pink materials | The project doesn't use URP. Create the project with the Universal 3D template. |
| The player doesn't respond to the keyboard | *Active Input Handling* (Project Settings > Player) doesn't include the new Input System. |
| Guards don't move | No navigation mesh has been generated in the level, or their route points are outside it. |
| A guard or camera does nothing | It isn't assigned in the session (`StealthSessionManager`). |
| The console shows an error on start | A required reference is missing. The message indicates which field and on which object; select it from the console and check it under **Advanced**. |
