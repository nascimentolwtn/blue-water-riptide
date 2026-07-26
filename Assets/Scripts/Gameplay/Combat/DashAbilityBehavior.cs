using BlueWaterRiptide.Characters;

namespace BlueWaterRiptide.Gameplay.Combat
{
    /// <summary>
    /// Standalone forward dash — the reusable motion primitive Drop Anchor's leap currently
    /// fuses together with its impact/aura effects (see LeapSlamAuraAbilityBehavior, which now
    /// shares AbilityMotion with this class). No v1 Sailor is assigned this directly yet; ships
    /// as a validated, ready-to-compose primitive for a future dash-kit Sailor.
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
            AbilityMotion.TranslateAlongAim(owner, _distance);
        }
    }
}
