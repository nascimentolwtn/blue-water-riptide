using UnityEngine;

namespace BlueWaterRiptide.Characters
{
    /// <summary>
    /// Shared screen-space zone definitions for the M1 prototype's touch controls (Plan 01 §1:
    /// left joystick, right attack button, Super button above it). Rects are in Input System
    /// screen space — origin bottom-left, Y increases upward — matching Touchscreen positions
    /// directly. IMGUI callers (top-left origin) must flip Y: `Screen.height - rect.y - rect.height`.
    /// Single source of truth so TouchInputDriver and any HUD overlay never drift apart.
    /// </summary>
    public static class TouchZones
    {
        public static Rect MoveZone => new Rect(0f, 0f, Screen.width * 0.5f, Screen.height);

        public static Rect AttackZone => new Rect(Screen.width * 0.5f, 0f, Screen.width * 0.5f, Screen.height * 0.65f);

        public static Rect SuperZone => new Rect(Screen.width * 0.5f, Screen.height * 0.65f, Screen.width * 0.5f, Screen.height * 0.35f);
    }
}
