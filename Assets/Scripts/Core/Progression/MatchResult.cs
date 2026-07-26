using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.Core.Progression
{
    /// <summary>
    /// Self-contained summary of one finished match, from the local participant's point of view.
    /// Built by whichever launcher/bootstrap observed MatchController.OnMatchEnd (plus its
    /// KnockoutsByAttacker tally) — ProgressionService never reaches into Session/MatchController
    /// itself, so any connection mode can feed it results the same way.
    /// </summary>
    public sealed class MatchResult
    {
        public ConnectionMode Mode { get; }
        public string LocalSailorId { get; }
        public bool LocalWon { get; }
        public int LocalKnockouts { get; }
        public bool WonViaSuddenDeath { get; }
        /// <summary>False for a disconnect/host-loss — no trophy/Doubloon delta applies (Plan 05 §1).</summary>
        public bool Completed { get; }

        public MatchResult(ConnectionMode mode, string localSailorId, bool localWon, int localKnockouts, bool wonViaSuddenDeath, bool completed)
        {
            Mode = mode;
            LocalSailorId = localSailorId;
            LocalWon = localWon;
            LocalKnockouts = localKnockouts;
            WonViaSuddenDeath = wonViaSuddenDeath;
            Completed = completed;
        }
    }
}
