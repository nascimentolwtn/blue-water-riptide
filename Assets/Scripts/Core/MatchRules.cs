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
        public bool RespawnsEnabled = false;

        /// <summary>M1 vertical slice: single round, no timer/sudden-death yet (Plan 01 M1 scope).</summary>
        public static MatchRules M1Defaults => new MatchRules
        {
            RoundsToWin = 1,
            RoundTimeLimit = 90f,
            RespawnsEnabled = false
        };
    }
}
