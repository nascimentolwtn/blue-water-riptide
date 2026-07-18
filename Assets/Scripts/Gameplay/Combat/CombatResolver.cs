using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.Gameplay.Combat
{
    /// <summary>
    /// Thin seam for damage resolution — knockback application lives on the ability behaviors
    /// that cause it (they know the direction/force); this just applies damage and charges the
    /// attacker's Super, matching "Super charges by dealing damage" (00 §2).
    /// </summary>
    public static class CombatResolver
    {
        public static void ApplyDamage(SailorPawn target, float amount, SailorPawn source)
        {
            target.ApplyDamage(amount, source.Id);
            source.AddSuperCharge(amount);
        }
    }
}
