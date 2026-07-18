using UnityEngine;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.AI
{
    /// <summary>
    /// M1 placeholder opponent: stands still, faces the target, fires whenever in range.
    /// Not a real utility-AI system — see Plan 01 Section 3 for the future behavior-state design.
    /// </summary>
    public sealed class DummyAIInputDriver : IInputDriver
    {
        readonly Transform _self;
        readonly Transform _target;
        readonly float _fireRange;

        public DummyAIInputDriver(Transform self, Transform target, float fireRange)
        {
            _self = self;
            _target = target;
            _fireRange = fireRange;
        }

        public InputCommand Sample(double time)
        {
            if (_self == null || _target == null)
            {
                return InputCommand.None(time);
            }

            Vector3 selfPos = _self.position;
            Vector3 targetPos = _target.position;
            Vector3 delta = new Vector3(targetPos.x - selfPos.x, 0f, targetPos.z - selfPos.z);

            Vector2 aim = Vector2.zero;
            float sqrDistance = delta.sqrMagnitude;
            if (sqrDistance > Mathf.Epsilon)
            {
                Vector3 dir = delta.normalized;
                aim = new Vector2(dir.x, dir.z);
            }

            bool inRange = sqrDistance <= _fireRange * _fireRange;

            return new InputCommand(Vector2.zero, aim, false, inRange, false, time);
        }
    }
}
