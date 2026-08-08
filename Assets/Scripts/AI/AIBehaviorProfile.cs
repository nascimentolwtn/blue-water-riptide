namespace BlueWaterRiptide.AI
{
    /// <summary>Basic-attack archetype a Sailor's behavior parameters key off — not the Sailor's own id, so a
    /// future 3rd Sailor reuses one of these presets instead of needing new AI code (Plan 01 §3).</summary>
    public enum AttackArchetype
    {
        /// <summary>Ranged basic attack, wants distance and line-of-sight (Ace's style).</summary>
        Serve,

        /// <summary>Melee basic attack, wants to close distance and commit (Anchor's style).</summary>
        Spike
    }

    /// <summary>
    /// Data-only tuning knobs for one attack archetype's utility-state scoring and firing conditions.
    /// Two named presets (Serve/Spike) cover today's 2-Sailor roster; a 3rd Sailor just needs a 3rd
    /// preset here, never new AIInputDriver/UtilityState code (Plan 01 §3).
    /// </summary>
    public sealed class AIBehaviorProfile
    {
        public AttackArchetype Archetype;

        /// <summary>Fraction of Definition.AttackRange this archetype tries to hold distance at while Engaging.</summary>
        public float PreferredRangeFraction;

        /// <summary>Multiple of Definition.AttackRange within which this archetype commits to melee/close-range pressure.</summary>
        public float CommitRangeMultiplier;

        /// <summary>HP fraction below which this archetype starts scoring Retreat highly.</summary>
        public float RetreatHpThreshold;

        /// <summary>Minimum ammo needed before firing is considered (Serve gates on ammo >= this; Spike is melee and ignores it functionally but the field stays for a consistent shape).</summary>
        public int MinAmmoToFire;

        /// <summary>Serve: radius around the aim target used to count "clustered enemies" for its Super condition.
        /// Spike: radius used to look for a nearby low-HP fleeing target to close on with its Super.</summary>
        public float SuperTriggerRadius;

        /// <summary>Serve: minimum clustered-enemy count that justifies using Super. Spike: unused (its Super
        /// conditions are ally-HP/target-HP driven, not a count) but kept for a consistent shape across archetypes.</summary>
        public int SuperTriggerEnemyCount;

        /// <summary>Spike only: ally HP fraction below which it will pop its ally-shield Super defensively.</summary>
        public float AllyShieldHpThreshold;

        /// <summary>Sideways strafe speed fraction (of MoveSpeed) used while holding range in Engage — Serve
        /// strafes perpendicular to threats per Plan 01 §3; Spike advances straight in so this stays near 0.</summary>
        public float StrafeSpeedFraction;

        public static AIBehaviorProfile ServePreset => new AIBehaviorProfile
        {
            Archetype = AttackArchetype.Serve,
            PreferredRangeFraction = 0.7f,
            CommitRangeMultiplier = 1.0f,
            RetreatHpThreshold = 0.4f,
            MinAmmoToFire = 2,
            SuperTriggerRadius = 6f,
            SuperTriggerEnemyCount = 2,
            AllyShieldHpThreshold = 0f, // not applicable to Serve
            StrafeSpeedFraction = 0.6f
        };

        public static AIBehaviorProfile SpikePreset => new AIBehaviorProfile
        {
            Archetype = AttackArchetype.Spike,
            PreferredRangeFraction = 0.2f,
            CommitRangeMultiplier = 1.5f,
            RetreatHpThreshold = 0.3f,
            MinAmmoToFire = 1,
            SuperTriggerRadius = 8f,
            SuperTriggerEnemyCount = 0, // not applicable to Spike
            AllyShieldHpThreshold = 0.5f,
            StrafeSpeedFraction = 0.1f
        };
    }
}
