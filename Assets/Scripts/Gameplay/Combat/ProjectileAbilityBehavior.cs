using UnityEngine;
using BlueWaterRiptide.Characters;

namespace BlueWaterRiptide.Gameplay.Combat
{
    /// <summary>Ensign Ace's Jump Serve — straight-line ammo-based projectile. First reusable ability behavior.</summary>
    public sealed class ProjectileAbilityBehavior : IAbilityBehavior
    {
        readonly float _damage;
        readonly float _speed;
        readonly float _range;

        public ProjectileAbilityBehavior(float damage, float speed, float range)
        {
            _damage = damage;
            _speed = speed;
            _range = range;
        }

        public void Execute(SailorPawn owner)
        {
            GameObject projectileObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObj.name = "Projectile";
            projectileObj.transform.position = owner.transform.position + owner.transform.forward * 1.2f + Vector3.up * 0.5f;
            projectileObj.transform.localScale = Vector3.one * 0.4f;

            var renderer = projectileObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = SailorPawn.CreateUrpMaterial(owner.Team == Core.Team.A ? Color.cyan : Color.magenta);
            }

            var sphereCollider = projectileObj.GetComponent<SphereCollider>();
            if (sphereCollider != null) sphereCollider.isTrigger = true;

            var rigidbody = projectileObj.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            var projectile = projectileObj.AddComponent<Projectile>();
            projectile.Initialize(owner, owner.transform.forward, _speed, _damage, _range);
        }
    }
}
