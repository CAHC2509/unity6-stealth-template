# Guards see me through the low wall even when I'm crouched

**Type:** Bug
**Reported by:** Player (community feedback)
**Build / branch:** `task/001-start`

## What is happening

In the pipe gallery, right after the start of the Waterworks mission, there is a low wall with windows looking into the pump room. The tutorial hint says crouching next to the low wall keeps you hidden, but the guard in the pump room spots me every time, even when I'm fully crouched and pressed against the wall. Prone still works. It feels unfair because standing and crouching behave exactly the same.

## How to see it

1. Start a mission from the main menu (or use DevTools › Gameplay › *Respawn at checkpoint 1*).
2. Walk forward into the pipe gallery and crouch (`C` / east button) next to the low wall with windows.
3. Wait for the pump-room guard to look towards the windows.
4. The guard's vision cone turns red, he chases you and you get captured.

## Notes

- Happens every time, keyboard and gamepad.
- Standing behind tall walls still hides the player correctly. Prone players are not spotted.
- Do not change guard ranges, cone angles or level geometry: the level was tuned for the intended behaviour.
