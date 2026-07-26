using UnityEngine;
using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.Gameplay.Combat
{
    /// <summary>
    /// Admiral Anchor's Super — Drop Anchor: leaps to an aimed spot (clears walls), deals impact
    /// damage on landing, and grants nearby allies (including the caster) a temporary damage-reduction aura.
    /// </summary>
    public sealed class LeapSlamAuraAbilityBehavior : IAbilityBehavior
    {
        readonly float _leapRange;
        readonly float _impactRadius;
        readonly float _impactDamage;
        readonly float _auraRadius;
        readonly float _auraDuration;
        readonly float _auraDamageReductionFraction;

        public LeapSlamAuraAbilityBehavior(float leapRange, float impactRadius, float impactDamage, float auraRadius, float auraDuration, float auraDamageReductionFraction)
        {
            _leapRange = leapRange;
            _impactRadius = impactRadius;
            _impactDamage = impactDamage;
            _auraRadius = auraRadius;
            _auraDuration = auraDuration;
            _auraDamageReductionFraction = auraDamageReductionFraction;
        }

        public void Execute(SailorPawn owner)
        {
            AbilityMotion.TranslateAlongAim(owner, _leapRange);
            Vector3 destination = owner.transform.position;

            // Impact damage — enemies only.
            Collider[] impactHits = Physics.OverlapSphere(destination, _impactRadius);
            foreach (var hit in impactHits)
            {
                SailorPawn pawn = ResolvePawn(hit);
                if (pawn == null || pawn.Id == owner.Id || pawn.IsKnockedOut) continue;
                if (pawn.Team == owner.Team) continue;

                CombatResolver.ApplyDamage(pawn, _impactDamage, owner);
            }

            // Damage-reduction aura — allies only (owner included, since it's standing at the landing point).
            Collider[] auraHits = Physics.OverlapSphere(destination, _auraRadius);
            foreach (var hit in auraHits)
            {
                SailorPawn pawn = ResolvePawn(hit);
                if (pawn == null || pawn.IsKnockedOut) continue;
                if (pawn.Team != owner.Team) continue;

                pawn.ApplyDamageReductionBuff(_auraDamageReductionFraction, _auraDuration);
            }

            SpawnAuraVisual(owner, destination);
        }

        static SailorPawn ResolvePawn(Collider collider)
        {
            SailorPawn pawn = collider.GetComponent<SailorPawn>();
            if (pawn == null) pawn = collider.GetComponentInParent<SailorPawn>();
            return pawn;
        }

        void SpawnAuraVisual(SailorPawn owner, Vector3 destination)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "DropAnchorAura";
            visual.transform.position = destination + Vector3.up * 0.02f;
            visual.transform.localScale = new Vector3(_auraRadius * 2f, 0.02f, _auraRadius * 2f);

            var visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null) Object.Destroy(visualCollider);

            var renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color tint = owner.Team == Team.A ? Color.cyan : Color.magenta;
                tint.a = 0.35f;
                renderer.material = SailorPawn.CreateUrpMaterial(tint);
            }

            Object.Destroy(visual, _auraDuration);
        }
    }
}
