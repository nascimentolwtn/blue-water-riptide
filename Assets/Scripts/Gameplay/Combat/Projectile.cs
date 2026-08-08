using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Core;
using BlueWaterRiptide.Gameplay.Court;
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

        float _distanceTraveled;
        float _age;

        public void Initialize(SailorPawn owner, Vector3 direction, float speed, float damage, float maxRange)
        {
            _owner = owner;
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _maxRange = maxRange;

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
            if (other.GetComponent<TemporaryObstacle>() != null)
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
            Destroy(gameObject);
        }
    }
}
