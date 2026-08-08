using System.Collections.Generic;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.AI
{
    /// <summary>Simplified stand-in for "one AI north, one center/south" (Plan 01 §3) since there's no real
    /// arena geometry yet beyond ArenaHalfExtents — a left/center/right partition of the AI team's own side.</summary>
    public enum Lane
    {
        Left,
        Center,
        Right
    }

    /// <summary>
    /// Lightweight per-team coordination: focus-fire suggestion, round-start lane assignment, and a
    /// timeout-aware "play safe" flag. Deliberately not a full squad-tactics system — Plan 01 §3 calls
    /// this out as "lightweight." AIInputDriver instances consult one shared AISquadBrain per team;
    /// only Skipper-tier difficulty is required to respect its suggestions.
    /// </summary>
    public sealed class AISquadBrain
    {
        public Team Team { get; }

        readonly Dictionary<ParticipantId, Lane> _laneAssignments = new Dictionary<ParticipantId, Lane>();

        public AISquadBrain(Team team)
        {
            Team = team;
        }

        /// <summary>Call once at round start with this team's participant ids (in any stable order) to
        /// hand out a left/center/right partition.</summary>
        public void AssignLanes(IReadOnlyList<ParticipantId> teamMembers)
        {
            _laneAssignments.Clear();
            for (int i = 0; i < teamMembers.Count; i++)
            {
                _laneAssignments[teamMembers[i]] = LaneForIndex(i, teamMembers.Count);
            }
        }

        public Lane GetLane(ParticipantId id)
        {
            return _laneAssignments.TryGetValue(id, out var lane) ? lane : Lane.Center;
        }

        static Lane LaneForIndex(int index, int teamSize)
        {
            if (teamSize <= 1) return Lane.Center;
            return (Lane)(index % 3);
        }

        /// <summary>Lowest-HP living enemy visible to the team — the shared focus-fire suggestion.
        /// Callers pass whichever enemy snapshots their own perception already resolved as visible.</summary>
        public AIUnitSnapshot? SuggestFocusFireTarget(IReadOnlyList<AIUnitSnapshot> visibleEnemies)
        {
            AIUnitSnapshot? best = null;
            float bestHp = float.MaxValue;

            for (int i = 0; i < visibleEnemies.Count; i++)
            {
                var enemy = visibleEnemies[i];
                if (enemy.IsKnockedOut) continue;

                if (enemy.HpFraction < bestHp)
                {
                    bestHp = enemy.HpFraction;
                    best = enemy;
                }
            }

            return best;
        }

        /// <summary>True when this team holds a strict survivor-count advantage with under
        /// timeoutThresholdSeconds left — Skipper-tier AI plays conservatively in that window instead
        /// of trading unnecessarily (Plan 01 §3 "timeout awareness").</summary>
        public bool ShouldPlaySafe(IReadOnlyList<AIUnitSnapshot> teamMembers, IReadOnlyList<AIUnitSnapshot> enemyMembers, float matchTimeRemaining, float timeoutThresholdSeconds = 20f)
        {
            if (matchTimeRemaining > timeoutThresholdSeconds) return false;

            int aliveTeam = CountAlive(teamMembers);
            int aliveEnemy = CountAlive(enemyMembers);

            return aliveTeam > aliveEnemy;
        }

        static int CountAlive(IReadOnlyList<AIUnitSnapshot> units)
        {
            int count = 0;
            for (int i = 0; i < units.Count; i++)
            {
                if (!units[i].IsKnockedOut) count++;
            }
            return count;
        }
    }
}
