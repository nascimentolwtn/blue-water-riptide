using System.Collections.Generic;
using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.AI
{
    /// <summary>Shared intent one AI-controlled team's AIInputDrivers all read (Plan 01 §3).</summary>
    public sealed class AISquadIntent
    {
        public ParticipantId? FocusFireTargetId;
        /// <summary>Holding a survivor-count lead under the timeout window — Plan 01 §3's
        /// "win-by-timeout awareness." Not yet consumed by any state (no state currently changes
        /// behavior on this flag); exposed for a future HoldChoke/defensive-posture pass.</summary>
        public bool PlaySafe;
    }

    /// <summary>
    /// Lightweight per-team coordination (Plan 01 §3): focus-fire suggestion (lowest-HP visible
    /// enemy) and a "play safe" flag when holding a survivor lead under 20s remaining. Lane
    /// assignment (north/center/south) is deliberately not implemented — there are no arena lanes
    /// to assign to yet (item 4's real geometry); add it once Tideline Cove's real layout exists.
    /// </summary>
    public sealed class AISquadBrain
    {
        const double RecomputeInterval = 1.0;
        const float PlaySafeTimeRemainingThreshold = 20f;

        readonly Team _team;
        readonly IReadOnlyList<SailorPawn> _allPawns;

        double _lastRecompute = double.NegativeInfinity;

        public AISquadIntent Intent { get; private set; } = new AISquadIntent();

        public AISquadBrain(Team team, IReadOnlyList<SailorPawn> allPawns)
        {
            _team = team;
            _allPawns = allPawns;
        }

        /// <summary>Call roughly once per frame/tick; internally throttles to RecomputeInterval.</summary>
        public void Tick(double time, float roundTimeRemaining, bool inSuddenDeath)
        {
            if (time - _lastRecompute < RecomputeInterval) return;
            _lastRecompute = time;

            Intent = Recompute(roundTimeRemaining, inSuddenDeath);
        }

        AISquadIntent Recompute(float roundTimeRemaining, bool inSuddenDeath)
        {
            int aliveAllies = 0;
            int aliveEnemies = 0;
            SailorPawn lowestHpEnemy = null;
            float lowestHpFraction = float.MaxValue;

            if (_allPawns != null)
            {
                foreach (var pawn in _allPawns)
                {
                    if (pawn == null || pawn.IsKnockedOut) continue;

                    if (pawn.Team == _team)
                    {
                        aliveAllies++;
                        continue;
                    }

                    aliveEnemies++;
                    if (pawn.HpFraction < lowestHpFraction)
                    {
                        lowestHpFraction = pawn.HpFraction;
                        lowestHpEnemy = pawn;
                    }
                }
            }

            return new AISquadIntent
            {
                FocusFireTargetId = lowestHpEnemy?.Id,
                PlaySafe = !inSuddenDeath && roundTimeRemaining > 0f && roundTimeRemaining <= PlaySafeTimeRemainingThreshold && aliveAllies > aliveEnemies,
            };
        }
    }
}
