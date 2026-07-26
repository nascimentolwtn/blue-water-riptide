using System.Collections.Generic;

namespace BlueWaterRiptide.AI
{
    public enum AIDifficulty { Deckhand, Bosun, Skipper }

    /// <summary>Difficulty is a parameter set layered on top of behavior profiles — never a fork
    /// of behavior code (Plan 01 §3).</summary>
    public sealed class AIDifficultyParams
    {
        public float ReactionDelaySeconds;
        public float AimErrorDegrees;
        /// <summary>0 = never leads a moving target, 1 = full target leading.</summary>
        public float TargetLeadingFraction;
        public bool PlaysTimeoutTactics;
        /// <summary>False = "uses Super late"/"decent timing", true = "coordinated"/timely.</summary>
        public bool UsesSuperPromptly;

        static readonly Dictionary<AIDifficulty, AIDifficultyParams> All = new Dictionary<AIDifficulty, AIDifficultyParams>
        {
            {
                AIDifficulty.Deckhand, new AIDifficultyParams
                {
                    ReactionDelaySeconds = 0.4f,
                    AimErrorDegrees = 12f,
                    TargetLeadingFraction = 0f,
                    PlaysTimeoutTactics = false,
                    UsesSuperPromptly = false,
                }
            },
            {
                AIDifficulty.Bosun, new AIDifficultyParams
                {
                    ReactionDelaySeconds = 0.25f,
                    AimErrorDegrees = 6f,
                    TargetLeadingFraction = 0.6f,
                    PlaysTimeoutTactics = false,
                    UsesSuperPromptly = true,
                }
            },
            {
                AIDifficulty.Skipper, new AIDifficultyParams
                {
                    ReactionDelaySeconds = 0.15f,
                    AimErrorDegrees = 2f,
                    TargetLeadingFraction = 1f,
                    PlaysTimeoutTactics = true,
                    UsesSuperPromptly = true,
                }
            },
        };

        public static AIDifficultyParams For(AIDifficulty difficulty) => All[difficulty];

        /// <summary>Parses SettingsRecord.aiDifficulty ("deckhand"/"bosun"/"skipper", case-insensitive);
        /// unrecognized/empty falls back to Bosun rather than throwing on a stale/malformed save.</summary>
        public static AIDifficulty ParseOrDefault(string value)
        {
            if (string.Equals(value, "deckhand", System.StringComparison.OrdinalIgnoreCase)) return AIDifficulty.Deckhand;
            if (string.Equals(value, "skipper", System.StringComparison.OrdinalIgnoreCase)) return AIDifficulty.Skipper;
            return AIDifficulty.Bosun;
        }
    }
}
