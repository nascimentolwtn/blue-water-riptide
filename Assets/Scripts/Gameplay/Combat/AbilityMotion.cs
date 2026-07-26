using UnityEngine;
using BlueWaterRiptide.Characters;

namespace BlueWaterRiptide.Gameplay.Combat
{
    /// <summary>
    /// Shared aim-directed translation primitive for dash/leap-style abilities — arena-clamped so
    /// a dash/leap can never carry a Sailor off the playable area. Extracted from
    /// LeapSlamAuraAbilityBehavior so DashAbilityBehavior can reuse the same motion (Plan 01's
    /// "standalone dash behavior" note). Does not path around obstacles — there is no arena
    /// geometry to path around yet (item 4 is Editor/Asset-bound); a leap/dash currently clears
    /// everything, matching Drop Anchor's existing "clears walls" design intent.
    /// </summary>
    public static class AbilityMotion
    {
        public static void TranslateAlongAim(SailorPawn owner, float distance)
        {
            Vector3 destination = owner.transform.position + owner.transform.forward * distance;
            destination.x = Mathf.Clamp(destination.x, -owner.ArenaHalfExtents.x, owner.ArenaHalfExtents.x);
            destination.z = Mathf.Clamp(destination.z, -owner.ArenaHalfExtents.y, owner.ArenaHalfExtents.y);
            owner.transform.position = destination;
        }
    }
}
