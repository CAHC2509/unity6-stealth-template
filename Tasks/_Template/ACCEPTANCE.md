# Acceptance criteria — <Task title>

All checks start from the main menu with DevTools enabled (Settings › Developer). Times are at time scale 1.

| # | Setup | Input sequence | Expected visual outcome | Verifiable state (DevTools › State) |
|---|---|---|---|---|
| 1 | <e.g. Gameplay › Respawn at checkpoint 2> | <e.g. Hold `W` 1.5 s, tap `C`> | <What is on screen> | <`key: value` lines that must match> |
| 2 | | | | |
| R1 | <Regression: something that must keep working> | | | |

## Evidence

| File | What it shows |
|---|---|
| `evidence/<id>-repro.inputtrace` | Input recording of check 1 (DevTools › Input, or `devtools.loadrecording <path>` then `devtools.replay`). |
| `evidence/<id>-before.png` + `.json` | Failing frame on `task/<id>-start`. |
| `evidence/<id>-after.png` + `.json` | Same frame on `task/<id>-solution`. |

## Automated checks

- EditMode: `<TestClass.TestName>` — <what it proves>.
- PlayMode: `<TestClass.TestName>` — <what it proves>.
