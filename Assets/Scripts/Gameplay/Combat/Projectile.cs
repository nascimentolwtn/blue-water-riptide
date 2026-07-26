using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Core;
using UnityEngine;

namespace BlueWaterRiptide.Gameplay.Combat
{
    /// <summary>
    /// Straight-line basic-attack projectile (Ensign Ace's Jump Serve). Created procedurally by
    /// SailorPawn.Fire — this component just drives movement, range, and hit resolution.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        const float LifetimeSafetyLimit = 5f;

        SailorPawn _owner;
        Vector3 _direction;
        float _speed;
        float _damage;
        float _maxRange;
        float _knockbackForce;

        float _distanceTraveled;
        float _age;

        /// <summary>Builds and launches a projectile identically to how ProjectileAbilityBehavior
        /// does for Ace's basic attack — shared here so other projectile-based abilities (e.g. a
        /// widening-fan Super) don't duplicate the spawn/material/collider/rigidbody setup.</summary>
        public static Projectile Spawn(SailorPawn owner, Vector3 direction, float damage, float speed, float maxRange, float knockbackForce = 0f)
        {
            GameObject projectileObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObj.name = "Projectile";
            projectileObj.transform.position = owner.transform.position + direction.normalized * 1.2f + Vector3.up * 0.5f;
            projectileObj.transform.localScale = Vector3.one * 0.4f;

            var renderer = projectileObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = SailorPawn.CreateUrpMaterial(owner.Team == Team.A ? Color.cyan : Color.magenta);
            }

            var sphereCollider = projectileObj.GetComponent<SphereCollider>();
            if (sphereCollider != null) sphereCollider.isTrigger = true;

            var rigidbody = projectileObj.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            var projectile = projectileObj.AddComponent<Projectile>();
            projectile.Initialize(owner, direction, speed, damage, maxRange, knockbackForce);
            return projectile;
        }

        public void Initialize(SailorPawn owner, Vector3 direction, float speed, float damage, float maxRange, float knockbackForce = 0f)
        {
            _owner = owner;
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _maxRange = maxRange;
            _knockbackForce = knockbackForce;

            _distanceTraveled = 0f;
            _age = 0f;
        }

        void Update()
        {
            float step = _speed * Time.deltaTime;
            transform.position += _direction * step;
            _distanceTraveled += step;
            _age += Time.deltaTime;

            if (_distanceTraveled >= _maxRange || _age >= LifetimeSafetyLimit)
            {
                Destroy(gameObject);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<SpawnedWall>() != null)
            {
                Destroy(gameObject);
                return;
            }

            var pawn = other.GetComponent<SailorPawn>();
            if (pawn == null) pawn = other.GetComponentInParent<SailorPawn>();
            if (pawn == null) return;

            if (_owner == null) return;
            if (pawn.Id == _owner.Id) return;
            if (pawn.Team == _owner.Team) return;
            if (pawn.IsKnockedOut) return;

            CombatResolver.ApplyDamage(pawn, _damage, _owner);
            if (_knockbackForce > 0f) pawn.ApplyKnockback(_direction, _knockbackForce);
            Destroy(gameObject);
        }
    }
}
