using UnityEngine;

namespace StealthController.Core
{
    public readonly struct PlayerInputFrame
    {
        public Vector2 Move { get; }
        public bool IsMoveAnalog { get; }
        public bool SprintHeld { get; }
        public bool CrouchPressed { get; }
        public bool CrouchHeld { get; }
        public bool CrouchReleased { get; }
        public bool StandPressed { get; }
        public bool TakedownPressed { get; }

        public PlayerInputFrame(Vector2 move, bool isMoveAnalog, bool sprintHeld, bool crouchPressed)
            : this(move, isMoveAnalog, sprintHeld, crouchPressed, crouchPressed, false) { }

        public PlayerInputFrame(Vector2 move, bool isMoveAnalog, bool sprintHeld, bool crouchPressed, bool crouchHeld, bool crouchReleased)
            : this(move, isMoveAnalog, sprintHeld, crouchPressed, crouchHeld, crouchReleased, false) { }

        public PlayerInputFrame(Vector2 move, bool isMoveAnalog, bool sprintHeld, bool crouchPressed, bool crouchHeld, bool crouchReleased, bool standPressed)
            : this(move, isMoveAnalog, sprintHeld, crouchPressed, crouchHeld, crouchReleased, standPressed, false) { }

        public PlayerInputFrame(Vector2 move, bool isMoveAnalog, bool sprintHeld, bool crouchPressed, bool crouchHeld, bool crouchReleased, bool standPressed, bool takedownPressed)
        {
            Move = move;
            IsMoveAnalog = isMoveAnalog;
            SprintHeld = sprintHeld;
            CrouchPressed = crouchPressed;
            CrouchHeld = crouchHeld;
            CrouchReleased = crouchReleased;
            StandPressed = standPressed;
            TakedownPressed = takedownPressed;
        }
    }
}
