using UnityEngine;

namespace BlueWaterRiptide.Core
{
    /// <summary>
    /// Transport-agnostic input snapshot. Touch, keyboard/mouse, AI, and network
    /// drivers all produce this same shape so the simulation never knows the source.
    /// </summary>
    public readonly struct InputCommand
    {
        public readonly Vector2 Move;
        public readonly Vector2 Aim;
        public readonly bool FirePressed;
        public readonly bool FireHeld;
        public readonly bool SuperPressed;
        public readonly double Timestamp;

        public InputCommand(Vector2 move, Vector2 aim, bool firePressed, bool fireHeld, bool superPressed, double timestamp)
        {
            Move = move;
            Aim = aim;
            FirePressed = firePressed;
            FireHeld = fireHeld;
            SuperPressed = superPressed;
            Timestamp = timestamp;
        }

        public static InputCommand None(double timestamp) => new InputCommand(Vector2.zero, Vector2.zero, false, false, false, timestamp);
    }
}
