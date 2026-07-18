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

        ParticipantId _ownerId;
        Team _ownerTeam;
        Vector3 _direction;
        float _speed;
        float _damage;
        float _maxRange;
        MatchController _matchController;

        float _distanceTraveled;
        float _age;

        public void Initialize(ParticipantId ownerId, Team ownerTeam, Vector3 direction, float speed, float damage, float maxRange, MatchController matchController)
        {
            _ownerId = ownerId;
            _ownerTeam = ownerTeam;
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _maxRange = maxRange;
            _matchController = matchController;

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
            var pawn = other.GetComponent<SailorPawn>();
            if (pawn == null) pawn = other.GetComponentInParent<SailorPawn>();
            if (pawn == null) return;

            if (pawn.Id == _ownerId) return;
            if (pawn.Team == _ownerTeam) return;
            if (pawn.IsKnockedOut) return;

            CombatResolver.ApplyDamage(pawn, _damage, _ownerId);
            Destroy(gameObject);
        }
    }
}
