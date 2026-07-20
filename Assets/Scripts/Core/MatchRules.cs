namespace BlueWaterRiptide.Core
{
    /// <summary>
    /// Plain data for now (M1 prototype scope) — becomes a ScriptableObject once the Editor-side
    /// authoring flow is worth it. Shape is deliberately asset-instance-ready: fields only, no logic.
    /// </summary>
    public sealed class MatchRules
    {
        public int RoundsToWin = 2;

        /// <summary>Seconds a round can run in Combat before it's still tied and Sudden Death triggers.</summary>
        public float RoundTimeLimit = 90f;
        public bool RespawnsEnabled = false;

        /// <summary>Seconds the pre-round countdown holds before Combat begins (state: RoundCountdown).</summary>
        public float RoundCountdownDuration = 3f;

        /// <summary>Seconds the reset/dwell period lasts between rounds (state: RoundEnd), before the next round or MatchEnd.</summary>
        public float RoundEndDuration = 5f;

        /// <summary>Seconds between each step the playable-area ring shrinks once Sudden Death is active.</summary>
        public float SuddenDeathRingInterval = 5f;

        /// <summary>Damage per second dealt to Sailors caught outside the shrinking safe zone during Sudden Death.</summary>
        public float SuddenDeathDamagePerSecond = 5f;

        /// <summary>M1 vertical slice: single round, no timer/sudden-death yet (Plan 01 M1 scope).</summary>
        public static MatchRules M1Defaults => new MatchRules
        {
            RoundsToWin = 1,
            RoundTimeLimit = 90f,
            RespawnsEnabled = false
        };

        /// <summary>"Sink or Swim" ruleset (Plan 00): best-of-3 rounds with countdown/reset dwell periods and a
        /// timed Sudden Death shrinking ring to force a decision once the round timer expires.</summary>
        public static MatchRules SinkOrSwimDefaults => new MatchRules
        {
            RoundsToWin = 2,
            RoundTimeLimit = 90f,
            RespawnsEnabled = false,
            RoundCountdownDuration = 3f,
            RoundEndDuration = 5f,
            SuddenDeathRingInterval = 5f,
            SuddenDeathDamagePerSecond = 5f
        };
    }
}
