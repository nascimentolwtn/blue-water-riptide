using UnityEngine;
using BlueWaterRiptide.Characters;

namespace BlueWaterRiptide.Gameplay.Combat
{
    /// <summary>Ensign Ace's Super — Ace Barrage: a widening fan of projectiles, each dealing
    /// damage and moderate knockback (Plan 00 §3).</summary>
    public sealed class FanProjectileAbilityBehavior : IAbilityBehavior
    {
        readonly int _projectileCount;
        readonly float _spreadDegrees;
        readonly float _damage;
        readonly float _speed;
        readonly float _range;
        readonly float _knockbackForce;

        public FanProjectileAbilityBehavior(int projectileCount, float spreadDegrees, float damage, float speed, float range, float knockbackForce)
        {
            _projectileCount = projectileCount;
            _spreadDegrees = spreadDegrees;
            _damage = damage;
            _speed = speed;
            _range = range;
            _knockbackForce = knockbackForce;
        }

        public void Execute(SailorPawn owner)
        {
            if (_projectileCount <= 0) return;

            float startAngle = _projectileCount > 1 ? -_spreadDegrees / 2f : 0f;
            float step = _projectileCount > 1 ? _spreadDegrees / (_projectileCount - 1) : 0f;

            for (int i = 0; i < _projectileCount; i++)
            {
                float angle = startAngle + step * i;
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * owner.transform.forward;
                Projectile.Spawn(owner, direction, _damage, _speed, _range, _knockbackForce);
            }
        }
    }
}
