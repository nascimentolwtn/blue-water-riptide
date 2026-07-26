using BlueWaterRiptide.Characters;

namespace BlueWaterRiptide.AI
{
    /// <summary>
    /// Behavior parameters keyed by attack archetype (Plan 01 §3), consumed by the utility
    /// states/AIInputDriver. New archetype = new profile here, no AI decision-code changes.
    /// </summary>
    public sealed class AIBehaviorProfile
    {
        /// <summary>Fraction of the Sailor's AttackRange this archetype tries to hold as its
        /// engagement distance (Serve keeps a range band; Spike wants to close to melee).</summary>
        public float PreferredRangeFraction = 1f;
        public bool StrafePerpendicular;
        public int MinAmmoToEngage = 1;
        public float RetreatHpFraction = 0.3f;

        // Serve-specific: "use Super on >= N clustered enemies within radius."
        public float SuperClusterRadius;
        public int SuperClusterMinCount;

        // Spike-specific: "commit within Nx slam range" / "use Super's aura when an ally is below this HP fraction."
        public float CommitRangeMultiplier = 1f;
        public float AllyLowHpFractionForAura;

        public static AIBehaviorProfile ForArchetype(AttackArchetype archetype)
        {
            switch (archetype)
            {
                case AttackArchetype.Spike:
                    return Spike;
                case AttackArchetype.Serve:
                default:
                    // Lob/Set/Block profiles ship with those Sailors — Serve is the closest
                    // existing shape (ranged poke) until then, not a design statement.
                    return Serve;
            }
        }

        public static AIBehaviorProfile Serve => new AIBehaviorProfile
        {
            PreferredRangeFraction = 0.7f,
            StrafePerpendicular = true,
            MinAmmoToEngage = 2,
            RetreatHpFraction = 0.4f,
            SuperClusterRadius = 6f,
            SuperClusterMinCount = 2,
        };

        public static AIBehaviorProfile Spike => new AIBehaviorProfile
        {
            PreferredRangeFraction = 1f,
            StrafePerpendicular = false,
            MinAmmoToEngage = 1,
            RetreatHpFraction = 0.25f,
            CommitRangeMultiplier = 1.5f,
            AllyLowHpFractionForAura = 0.5f,
        };
    }
}
