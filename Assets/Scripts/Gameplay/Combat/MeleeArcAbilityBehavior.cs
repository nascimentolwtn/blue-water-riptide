using UnityEngine;
using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.Gameplay.Combat
{
    /// <summary>
    /// Admiral Anchor's basic attack — Anchor Slam: close-range AOE ground pound in a frontal
    /// arc with strong knockback (00-game-design-overview.md §3).
    /// </summary>
    public sealed class MeleeArcAbilityBehavior : IAbilityBehavior
    {
        readonly float _damage;
        readonly float _radius;
        readonly float _coneAngleDegrees;
        readonly float _knockbackForce;

        public MeleeArcAbilityBehavior(float damage, float radius, float coneAngleDegrees, float knockbackForce)
        {
            _damage = damage;
            _radius = radius;
            _coneAngleDegrees = coneAngleDegrees;
            _knockbackForce = knockbackForce;
        }

        public void Execute(SailorPawn owner)
        {
            Vector3 originPos = owner.transform.position;
            Vector3 flatForward = FlattenAndNormalize(owner.transform.forward, Vector3.forward);

            Collider[] candidates = Physics.OverlapSphere(originPos, _radius);
            for (int i = 0; i < candidates.Length; i++)
            {
                SailorPawn pawn = candidates[i].GetComponent<SailorPawn>();
                if (pawn == null) pawn = candidates[i].GetComponentInParent<SailorPawn>();
                if (pawn == null) continue;

                if (pawn.Id == owner.Id) continue;
                if (pawn.Team == owner.Team) continue;
                if (pawn.IsKnockedOut) continue;

                Vector3 toTarget = pawn.transform.position - originPos;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude < 0.0001f) continue; // pawn is effectively on top of us, no meaningful direction

                Vector3 flatToTarget = toTarget.normalized;

                float angle = Vector3.Angle(flatForward, flatToTarget);
                if (angle > _coneAngleDegrees / 2f) continue;

                CombatResolver.ApplyDamage(pawn, _damage, owner);
                pawn.ApplyKnockback(flatToTarget, _knockbackForce);
            }

            SpawnTelegraphVisual(owner, originPos);
        }

        static Vector3 FlattenAndNormalize(Vector3 direction, Vector3 fallback)
        {
            Vector3 flat = new Vector3(direction.x, 0f, direction.z);
            return flat.sqrMagnitude > 0.0001f ? flat.normalized : fallback;
        }

        void SpawnTelegraphVisual(SailorPawn owner, Vector3 originPos)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "AnchorSlamTelegraph";
            go.transform.position = originPos + Vector3.up * 0.05f;
            go.transform.localScale = new Vector3(_radius * 2f, 0.05f, _radius * 2f);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = SailorPawn.CreateUrpMaterial(owner.Team == Team.A ? Color.cyan : Color.magenta);
            }

            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            Object.Destroy(go, 0.15f);
        }
    }
}
