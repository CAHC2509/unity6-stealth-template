# Reference solution — Guards see me through the low wall even when I'm crouched

> Not for the solver. Lives on `task/001-solution`.

## Root cause

The vision sensor tests three points of the player's body (head, chest and feet) against the guard's cone and line of sight. The sample heights are ratios of the **current** capsule height, so a crouched player (1.1 m capsule) is sampled lower than a standing one (1.8 m).

The injected bug in `task/001-start` replaces the current capsule height with the standing height constant:

```csharp
// VisionSensorComponent.HasLineOfSight (bugged)
Vector3 samplePoint = player.Position + Vector3.up * (StealthConstants.PLAYER_STANDING_HEIGHT * BodySampleHeightRatios[i]);
```

The head sample therefore stays at ~1.6 m whatever the posture, which clears the low wall and makes the crouched player visible. Prone still "works" by accident only where the crawl obstacles fully occlude the cone.

## Changes

| File | Change |
|---|---|
| `Assets/StealthController/Runtime/Core/Components/Vision/VisionSensorComponent.cs` | Sample with `player.CapsuleHeight` again: `player.Position + Vector3.up * (player.CapsuleHeight * BodySampleHeightRatios[i])`. |
| `Assets/_ProjectName/Tests/Editor/Stealth/VisionSensorComponentTests.cs` | Add a test with a wall lower than standing eye level: crouched target not visible, standing target visible. |

## Verification

- [ ] Checks 1–3 and R1–R2 of `ACCEPTANCE.md` fail on `start` (1) and pass on `solution` (all).
- [ ] `evidence/001-repro.inputtrace` replays on `solution` with no capture.
- [ ] Golden master unchanged (the recorded session never crouches near cover).

## Acceptable alternatives

- Reading the height from `PlayerPose.PoseState` and the movement config instead of `CapsuleHeight` is acceptable if it handles posture transitions (the capsule height is already interpolated, which is why it is the preferred source).
- Lowering the guard's eye height or raising the wall is **not** acceptable: it changes level tuning and breaks check 3.
