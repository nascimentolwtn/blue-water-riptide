using UnityEngine;
using UnityEngine.InputSystem;
using BlueWaterRiptide.Core;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace BlueWaterRiptide.Characters
{
    /// <summary>
    /// Real touch driver (Plan 01 §1 controls): left virtual joystick for movement, right
    /// attack button (tap/drag to aim, hold to fire), Super button above the attack button.
    /// Replaces KeyboardMouseInputDriver on Android, where there's no mouse device and
    /// keyboard forwarding is unreliable (Mouse.current is null on-device; Keyboard.current
    /// resolves to the on-screen IME keyboard, not a hardware-forwarded one).
    /// </summary>
    public sealed class TouchInputDriver : IInputDriver
    {
        const float JoystickRadius = 100f;
        const float AimDeadzone = 10f;

        int? _moveTouchId;
        Vector2 _moveOrigin;

        int? _attackTouchId;
        Vector2 _attackOrigin;

        Vector2 _lastAimDirection = Vector2.up;

        public InputCommand Sample(double time)
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
                return InputCommand.None(time);

            bool superPressed = false;
            Vector2? moveCurrentPos = null;
            Vector2? attackCurrentPos = null;

            var touches = touchscreen.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                int touchId = touch.touchId.ReadValue();
                TouchPhase phase = touch.phase.ReadValue();
                Vector2 position = touch.position.ReadValue();

                switch (phase)
                {
                    case TouchPhase.Began:
                        if (_moveTouchId == null && TouchZones.MoveZone.Contains(position))
                        {
                            _moveTouchId = touchId;
                            _moveOrigin = position;
                        }
                        else if (_attackTouchId == null && TouchZones.AttackZone.Contains(position))
                        {
                            _attackTouchId = touchId;
                            _attackOrigin = position;
                        }
                        else if (TouchZones.SuperZone.Contains(position))
                        {
                            superPressed = true;
                        }
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (_moveTouchId == touchId) moveCurrentPos = position;
                        else if (_attackTouchId == touchId) attackCurrentPos = position;
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (_moveTouchId == touchId) _moveTouchId = null;
                        else if (_attackTouchId == touchId) _attackTouchId = null;
                        break;
                }

                // A touch that just Began is also its current position for this frame.
                if (phase == TouchPhase.Began)
                {
                    if (_moveTouchId == touchId) moveCurrentPos = position;
                    else if (_attackTouchId == touchId) attackCurrentPos = position;
                }
            }

            Vector2 move = Vector2.zero;
            if (_moveTouchId != null && moveCurrentPos.HasValue)
            {
                Vector2 delta = moveCurrentPos.Value - _moveOrigin;
                if (delta.magnitude > JoystickRadius) delta = delta.normalized * JoystickRadius;
                move = delta / JoystickRadius;
            }

            if (_attackTouchId != null && attackCurrentPos.HasValue)
            {
                Vector2 delta = attackCurrentPos.Value - _attackOrigin;
                if (delta.magnitude > AimDeadzone)
                    _lastAimDirection = delta.normalized;
            }

            bool fireHeld = _attackTouchId != null;

            return new InputCommand(move, _lastAimDirection, false, fireHeld, superPressed, time);
        }
    }
}
