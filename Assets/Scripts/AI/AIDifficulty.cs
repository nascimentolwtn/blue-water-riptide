namespace BlueWaterRiptide.AI
{
    /// <summary>
    /// A difficulty tier is a parameter set layered on top of an AIBehaviorProfile — AIInputDriver
    /// blends both rather than branching on tier by name (Plan 01 §3: "never separate behavior code
    /// per difficulty").
    /// </summary>
    public sealed class AIDifficulty
    {
        public string Name;

        /// <summary>Seconds of delay before this AI reacts to a fresh perception snapshot (stale aim/decisions
        /// until the delay elapses).</summary>
        public float ReactionDelaySeconds;

        /// <summary>Max random aim error, degrees, applied symmetrically around the true aim direction.</summary>
        public float AimErrorDegrees;

        /// <summary>0 = always aims directly at the target's current position; 1 = always aims at the fully
        /// leaded (velocity-predicted) point. Deckhand never leads (0), Skipper always leads (1).</summary>
        public float TargetLeadFraction;

        /// <summary>If true, this tier only fires its Super when idle/no better utility state is available
        /// (Deckhand's "uses Super late" behavior) instead of on the archetype's normal trigger condition.</summary>
        public bool DelaySuperUntilIdle;

        /// <summary>If true, this tier respects AISquadBrain's focus-fire suggestion when picking a target.</summary>
        public bool RespectsFocusFireSuggestion;

        /// <summary>If true, this tier respects AISquadBrain's "play safe" flag (holds a lead conservatively
        /// near match/round timeout) instead of ignoring timeout tactics.</summary>
        public bool RespectsPlaySafe;

        public static AIDifficulty DeckhandPreset => new AIDifficulty
        {
            Name = "Deckhand",
            ReactionDelaySeconds = 0.4f,
            AimErrorDegrees = 12f,
            TargetLeadFraction = 0f,
            DelaySuperUntilIdle = true,
            RespectsFocusFireSuggestion = false,
            RespectsPlaySafe = false
        };

        public static AIDifficulty BosunPreset => new AIDifficulty
        {
            Name = "Bosun",
            ReactionDelaySeconds = 0.25f,
            AimErrorDegrees = 6f,
            TargetLeadFraction = 0.6f,
            DelaySuperUntilIdle = false,
            RespectsFocusFireSuggestion = false,
            RespectsPlaySafe = false
        };

        public static AIDifficulty SkipperPreset => new AIDifficulty
        {
            Name = "Skipper",
            ReactionDelaySeconds = 0.15f,
            AimErrorDegrees = 2f,
            TargetLeadFraction = 1f,
            DelaySuperUntilIdle = false,
            RespectsFocusFireSuggestion = true,
            RespectsPlaySafe = true
        };
    }
}
