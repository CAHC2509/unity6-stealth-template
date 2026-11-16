# Reference solution — Running should make noise

> Not for the solver. Lives on `task/002-solution`.

## Design

The feature crosses four systems: player locomotion (who makes noise), the session (who hears it), guard AI (what hearing does) and the HUD (how the player reads it).

1. **Emission (player).** A new `NoiseEmitterComponent` in `Runtime/Core/Components/Noise/` reads the player's locomotion each tick and reports a noise when the player is sprinting and actually moving (normalized speed above the run threshold). Radius comes from a serializable `NoiseConfig` (8 m default, constant in `StealthConstants`).
2. **Propagation (session).** `EnemyCoordinatorLogic.Tick` receives the player's noise for the frame and, for every guard that is not taken down and is within the radius on the XZ plane, calls a new `GuardLogic.Hear(Vector3 position)`. Sound is not occluded, so no collision query is needed.
3. **Reaction (guard).** `Hear` reuses the existing chase/search flow without sight: it sets the search target and enters the chase state in its *searching* phase, so the guard walks to the position, looks around and returns to patrol through the existing timeouts. Capture still depends on `PlayerCaptureComponent`, which only triggers from sight.
4. **Feedback (HUD).** `StealthSessionManager` publishes `NoiseChanged(bool)`; a `NoiseIndicatorView` in `Runtime/Unity/UI/` fades in a small icon next to the posture HUD. Texts in `Texts/StealthTexts` (en/es).

## Changes

| File | Change |
|---|---|
| `Runtime/Core/Components/Noise/NoiseEmitterComponent.cs`, `NoiseConfig.cs` | New component and config. |
| `Runtime/Core/Actors/Player/PlayerLogic.cs` | Compose the emitter; expose `Noise`. |
| `Runtime/Core/Session/EnemyCoordinatorLogic.cs`, `StealthSessionLogic.cs` | Pass the noise to guards within radius. |
| `Runtime/Core/Actors/Guard/GuardLogic.cs`, `States/GuardChaseState.cs` | `Hear(position)` enters searching without sight. |
| `Runtime/Unity/Manager/StealthSessionManager.cs`, `Runtime/Unity/UI/NoiseIndicatorView.cs`, `Prefabs/UI/` | Event and HUD indicator. |
| `Assets/_ProjectName/Tests/Editor/Stealth/NoiseTests.cs` | EditMode coverage of the rules in `ACCEPTANCE.md`. |
| `Assets/_ProjectName/Scripts/GameFlow/Gameplay/StealthDevToolsProviderController.cs` | Add `player.makingNoise` and each guard's investigation target to the DevTools state. |

## Verification

- [ ] All checks in `ACCEPTANCE.md` pass; checks 1, 4, 5 and 6 fail on `start`.
- [ ] Golden master unchanged.
- [ ] Gamepad: sprint is the left stick press; the indicator follows the device in use.

## Acceptable alternatives

- A separate *investigating* guard state instead of reusing the searching phase of chase, as long as checks 1 and 4 hold.
- Emitting noise from the Unity layer is **not** acceptable: it breaks the engine-agnostic core rule from the brief.
