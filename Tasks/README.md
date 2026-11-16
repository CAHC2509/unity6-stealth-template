# Tasks

Playtest-driven tasks built on this project: **bug tasks** (a realistic gameplay bug is injected and must be found by playing) and **feature tasks** (a change that crosses several gameplay systems). Each task ships with a brief written for the person solving it, verifiable acceptance criteria and a reference solution.

```
Tasks/
├── README.md                  ← this workflow
├── _Template/                 ← copy this folder to start a task
│   ├── BRIEF.md               ← what the solver reads (player / designer / producer voice)
│   ├── ACCEPTANCE.md          ← input sequences, expected visuals, verifiable states
│   └── SOLUTION.md            ← reference fix or implementation (never shown to the solver)
└── <id>-<slug>/               ← one folder per task, same three files plus evidence/
```

## Branches

| Branch | Content |
|---|---|
| `task/<id>-start` | The starting point the solver receives. For a bug task, the bug is injected here. |
| `task/<id>-solution` | `task/<id>-start` plus the reference solution. Its diff is what `SOLUTION.md` explains. |

Keep the injected bug in a single, small commit on `task/<id>-start` so it can be reviewed, and never leave hints in code, comments or commit messages that the solver can see.

## Workflow

1. **Pick the systems.** A good bug only shows on screen after playing (it is not visible by reading one file); a good feature touches at least two systems (input, AI, mission, HUD, save…).
2. **Write the brief first** (`BRIEF.md`). Describe symptoms or goals the way a player, designer or producer would. No file names, no class names, no root cause.
3. **Inject the bug / prepare the start branch.** Play it until the symptom is reproducible with a short input sequence.
4. **Capture evidence with DevTools** (Settings › Developer, then F1):
   - *Gameplay* tab: jump to the right checkpoint, toggle invisibility while setting up.
   - *Input* tab: record the reproduction (`.inputtrace`). Input pressed while the panel is open is not recorded.
   - *Capture* tab: screenshot + JSON state of the failing frame (before) and of the fixed frame (after).
   - *State* tab: copy the exact state keys you will assert (`guard[2] Guard_Pumps.canSeePlayer: true`).
   - *Console* tab: export the session log if the bug prints errors.
   Store the files in `Tasks/<id>-<slug>/evidence/`.
5. **Write the acceptance criteria** (`ACCEPTANCE.md`): numbered checks, each with setup, input sequence, expected visual outcome and the state keys that prove it. Include at least one regression check (something that must keep working).
6. **Write and verify the reference solution** on `task/<id>-solution`. Run the EditMode and PlayMode tests and replay every recording from step 4.
7. **End-to-end playtest** from a clean start (main menu → task setup → acceptance checks) on both branches: every check must fail on `start` (for bugs) and pass on `solution`.

## Writing good acceptance criteria

- **Reproducible setup.** "Respawn at checkpoint 2 (DevTools › Gameplay)", not "go to the second area".
- **Exact input.** Keys or buttons, durations and order: "Hold `W` for 2 s, then tap `C`". Attach the `.inputtrace` when timing matters.
- **Observable result.** What the player sees ("the guard's cone stays white and the guard keeps patrolling") **and** the state that proves it (`guard[0] ….alertState: Patrolling`).
- **Binary.** Every check is pass/fail without interpretation.
- **Automatable where possible.** If a check can be a test (see `Assets/_ProjectName/Tests/PlayMode/`), write it: it is the strongest proof.

## DevTools console commands useful for tasks

| Command | Use |
|---|---|
| `devtools.commands` / `devtools.run <index>` | List and run gameplay commands (respawn at checkpoint, invisibility, …). |
| `devtools.state` | Print the full state snapshot to the log. |
| `devtools.timescale 0.25` | Slow motion to watch a fast bug. |
| `devtools.pause` / `devtools.step` | Freeze and advance frame by frame. |
| `devtools.record` / `devtools.replay` / `devtools.loadrecording <path>` | Record, replay and load input sequences. |
| `devtools.screenshot` | Screenshot plus JSON state. |
| `devtools.exportlogs` | Session log as `.txt`. |
