namespace BlueWaterRiptide.Core
{
    /// <summary>
    /// Plain data for now (M1 prototype scope) — becomes a ScriptableObject once the Editor-side
    /// authoring flow is worth it. Shape is deliberately asset-instance-ready: fields only, no logic.
    /// </summary>
    public sealed class MatchRules
    {
        public int RoundsToWin = 2;
        public float RoundTimeLimit = 90f;
        public float CountdownDuration = 3f;
        public float RoundResetDelay = 5f;
        public float SuddenDeathRingInterval = 5f;
        public float SuperChargeCarryoverFraction = 0.5f;
        public bool RespawnsEnabled = false;

        /// <summary>
        /// Plan 00 §1 default ruleset: best-of-3 Sink or Swim, 3s round countdown, 90s round timer,
        /// 5s between-round reset, Rising Tide sudden death (5s per ring) on a survivor-count tie,
        /// Super charge carries over at 50% into the next round.
        /// </summary>
        public static MatchRules Default => new MatchRules
        {
            RoundsToWin = 2,
            RoundTimeLimit = 90f,
            CountdownDuration = 3f,
            RoundResetDelay = 5f,
            SuddenDeathRingInterval = 5f,
            SuperChargeCarryoverFraction = 0.5f,
            RespawnsEnabled = false
        };

        /// <summary>M1 vertical slice: single round, no countdown/timer/sudden-death yet (Plan 01 M1 scope).</summary>
        public static MatchRules M1Defaults => new MatchRules
        {
            RoundsToWin = 1,
            RoundTimeLimit = 0f,
            CountdownDuration = 0f,
            RoundResetDelay = 0f,
            RespawnsEnabled = false
        };
    }
}
