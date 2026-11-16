# Acceptance criteria — Running should make noise

All checks start from the main menu with DevTools enabled. Use DevTools › Gameplay to respawn; keep invisibility **off** unless stated.

| # | Setup | Input sequence | Expected visual outcome | Verifiable state |
|---|---|---|---|---|
| 1 | *Respawn at checkpoint 1*, stand behind the wall closest to the pump-room guard (out of his cone) | Hold `Shift` + `W` along the wall for 1.5 s | HUD noise indicator visible while sprinting; the guard stops, turns towards the player's last position and walks there | Guard `….alertState` leaves `Patrolling` (investigating), his target is within 1 m of the player's position at the end of the sprint |
| 2 | Same as 1 | Walk (no `Shift`) the same path | No HUD indicator; the guard keeps patrolling | `….alertState: Patrolling` |
| 3 | Same as 1, then crouch | Hold `C` + `W` (crouched walk) | Silent, guard keeps patrolling | `….alertState: Patrolling` |
| 4 | After check 1, move away and wait 10 s | No input | The guard looks around at the noise point and returns to his route | `….alertState: Patrolling` again |
| 5 | Gameplay › *Invisible to enemies* **on**, same as 1 | Sprint as in check 1 | The guard still investigates (hearing is not sight) but never captures | `IsPlayerDetectable: false`, guard investigating, `PendingOutcome: None` |
| 6 | Take down a guard, then sprint next to him | Sprint 2 s within 3 m | The taken-down guard does not react | `….takenDown: true` |
| 7 | Sprint more than 8 m away from every guard | Sprint 3 s | Nobody reacts | All guards `Patrolling` |
| R1 | — | EditMode + PlayMode tests | All green; golden master unchanged or justified | Test Runner summary |

## Evidence

| File | What it shows |
|---|---|
| `evidence/002-noise.inputtrace` | Check 1 from checkpoint 1. |
| `evidence/002-heard.png` + `.json` | Frame where the guard starts investigating. |
| `evidence/002-hud.png` | HUD noise indicator while sprinting. |

## Automated checks

- EditMode: noise emission only while sprinting and moving; hearing radius on the floor plane; taken-down guards ignore noise; newest noise wins.
- PlayMode: sprinting next to a guard in the demo scene changes his state within 1 s.
