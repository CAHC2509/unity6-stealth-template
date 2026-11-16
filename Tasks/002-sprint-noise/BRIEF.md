# Running should make noise

**Type:** Feature
**Reported by:** Game designer
**Build / branch:** `task/002-start`

## What we want

Right now sprinting has no downside: players run everywhere because guards only react to what they see. We want sprinting to be loud. When the player sprints close to a guard who cannot see them, the guard should turn and walk to where the noise came from, look around, and go back to his route if he finds nothing. Walking, crouching and crawling stay silent.

The player must be able to read this: show a small noise indicator on the HUD while sprinting, and make it obvious when a guard heard something (he stops, turns towards the sound and moves there).

## Design values

- Noise radius while sprinting: **8 m**, measured on the floor plane, blocked by nothing (sound goes through walls).
- A guard that hears a noise investigates the **last** noise position; new noises update the target.
- Taken-down guards hear nothing. Surveillance cameras hear nothing.
- Hearing never captures the player on its own: capture still requires being seen.

## Notes

- Keyboard and gamepad. Text in English and Spanish.
- Keep the core logic engine-agnostic like the rest of the stealth module (no `MonoBehaviour` in `Runtime/Core`) and covered by EditMode tests.
- The golden master test must keep passing: the recorded session never sprints near a guard. If it does, explain why the trace changed.
