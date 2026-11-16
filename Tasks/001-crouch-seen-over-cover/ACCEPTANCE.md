# Acceptance criteria — Guards see me through the low wall even when I'm crouched

All checks start from the main menu with DevTools enabled (Settings › Developer). Time scale 1. State keys come from DevTools › State (section `[Stealth]`); guard indices are the ones listed there.

| # | Setup | Input sequence | Expected visual outcome | Verifiable state |
|---|---|---|---|---|
| 1 | Gameplay › *Respawn at checkpoint 1*, close the panel | Walk to the low wall with windows, tap `C`, hold still 6 s while the pump-room guard faces the windows | The guard's cone reaches the wall but stays white; he keeps patrolling; no capture banner | `player.poseState: Crouched`, pump-room `guard[n] ….canSeePlayer: false`, `….alertState: Patrolling`, `PendingOutcome: None` |
| 2 | Same as 1 | Same position, press `Space` to stand up and wait | The cone turns red and the guard reacts | `player.poseState: Standing`, `guard[n] ….canSeePlayer: true` |
| 3 | Gameplay › *Respawn at checkpoint 2* | Walk into an open area in front of any patrolling guard, crouched | A crouched player in the open **is** seen (crouching is not invisibility) | `player.poseState: Crouched`, `guard[n] ….canSeePlayer: true` |
| R1 | Any checkpoint | Hide standing behind a tall wall | Guards do not see through tall walls | `….canSeePlayer: false` |
| R2 | — | Run all EditMode and PlayMode tests | All green, golden master unchanged | Test Runner summary |

## Evidence

| File | What it shows |
|---|---|
| `evidence/001-repro.inputtrace` | Check 1 recorded from checkpoint 1. |
| `evidence/001-before.png` + `.json` | Capture on `start`: crouched, cone red, `canSeePlayer: true`. |
| `evidence/001-after.png` + `.json` | Same frame on `solution`: cone white, `canSeePlayer: false`. |

## Automated checks

- EditMode: `VisionSensorComponentTests` — a crouched target behind a wall lower than standing eye level is not visible; a standing one is.
- PlayMode: `StealthInputReplayTests` — recorded input replays to the same player state.
