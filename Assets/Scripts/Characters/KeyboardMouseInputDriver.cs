using UnityEngine;
using UnityEngine.InputSystem;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.Characters
{
    /// <summary>
    /// Desktop-testing stand-in for the eventual touch driver (Plan 01's TouchInputDriver).
    /// Lets the M1 prototype be played with keyboard/mouse in the Editor before touch UI exists.
    /// </summary>
    public sealed class KeyboardMouseInputDriver : IInputDriver
    {
        readonly Transform _self;
        Camera _cam;

        public KeyboardMouseInputDriver(Transform self, Camera cam = null)
        {
            _self = self;
            _cam = cam;
        }

        public InputCommand Sample(double time)
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (keyboard == null || mouse == null)
                return InputCommand.None(time);

            Vector2 move = Vector2.zero;
            if (keyboard.dKey.isPressed) move.x += 1f;
            if (keyboard.aKey.isPressed) move.x -= 1f;
            if (keyboard.wKey.isPressed) move.y += 1f;
            if (keyboard.sKey.isPressed) move.y -= 1f;
            if (move.sqrMagnitude > 1f) move.Normalize();

            Vector2 aim = Vector2.zero;

            if (_cam == null)
                _cam = Camera.main;

            if (_cam != null && _self != null)
            {
                Vector2 screenPos = mouse.position.ReadValue();
                Ray ray = _cam.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0f));
                Plane groundPlane = new Plane(Vector3.up, _self.position);

                if (groundPlane.Raycast(ray, out float enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    Vector3 toHit = hitPoint - _self.position;
                    Vector2 flat = new Vector2(toHit.x, toHit.z);
                    if (flat.sqrMagnitude > 0.0001f)
                        aim = flat.normalized;
                }
            }

            bool fireHeld = mouse.leftButton.isPressed;
            bool firePressed = mouse.leftButton.wasPressedThisFrame;
            bool superPressed = mouse.rightButton.wasPressedThisFrame;

            return new InputCommand(move, aim, firePressed, fireHeld, superPressed, time);
        }
    }
}
