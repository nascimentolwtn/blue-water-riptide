using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.Gameplay.Combat
{
    /// <summary>
    /// Thin seam for damage resolution — knockback and damage-modifier logic slot in here later
    /// per the design docs. For now it's a pass-through to the target pawn.
    /// </summary>
    public static class CombatResolver
    {
        public static void ApplyDamage(SailorPawn target, float amount, ParticipantId source)
        {
            target.ApplyDamage(amount, source);
        }
    }
}
