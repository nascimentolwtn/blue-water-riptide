using UnityEngine;
using BlueWaterRiptide.Characters;

namespace BlueWaterRiptide.Gameplay.Combat
{
    /// <summary>
    /// Reusable mobility primitive (00 §7.2's "dash" ability shape): instantly repositions the
    /// caster a fixed distance in their current facing direction, clamped to the arena bounds.
    /// Distinct from Admiral Anchor's Leap Slam (LeapSlamAuraAbilityBehavior), which fuses a leap
    /// with impact damage and an aura — this is the plain mobility-only building block for future kits.
    /// </summary>
    public sealed class DashAbilityBehavior : IAbilityBehavior
    {
        readonly float _distance;

        public DashAbilityBehavior(float distance)
        {
            _distance = distance;
        }

        public void Execute(SailorPawn owner)
        {
            Vector3 flatForward = FlattenAndNormalize(owner.transform.forward);
            Vector3 destination = owner.transform.position + flatForward * _distance;
            destination.x = Mathf.Clamp(destination.x, -owner.ArenaHalfExtents.x, owner.ArenaHalfExtents.x);
            destination.z = Mathf.Clamp(destination.z, -owner.ArenaHalfExtents.y, owner.ArenaHalfExtents.y);

            owner.transform.position = destination;
        }

        static Vector3 FlattenAndNormalize(Vector3 direction)
        {
            Vector3 flat = new Vector3(direction.x, 0f, direction.z);
            return flat.sqrMagnitude > 0.0001f ? flat.normalized : Vector3.forward;
        }
    }
}
