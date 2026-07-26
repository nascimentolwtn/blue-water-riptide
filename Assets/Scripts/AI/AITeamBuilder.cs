using System;
using System.Collections.Generic;

namespace BlueWaterRiptide.AI
{
    /// <summary>One entry in the pool AITeamBuilder samples from. Deliberately just an id + a
    /// coarse role flag — AI code never hard-codes which specific Sailor is which (napkin
    /// guardrail); the caller (a launcher) is what knows the roster's actual content.</summary>
    public readonly struct SailorRosterEntry
    {
        public readonly string SailorId;
        public readonly bool IsTank;

        public SailorRosterEntry(string sailorId, bool isTank)
        {
            SailorId = sailorId;
            IsTank = isTank;
        }
    }

    public sealed class TeamBuildConstraints
    {
        public int TeamSize = 3;
        public int MaxDuplicatesPerTeam = 2;
        public int MaxTanksPerTeam = 1;
    }

    /// <summary>
    /// Samples a team's worth of Sailor ids from the unlocked pool (Plan 01 §2). With v1's
    /// 2-Sailor pool the constraints mostly no-op (e.g. {Anchor, Ace, Ace} is the natural result
    /// with a 1-tank cap and a 2-Sailor pool) and scale automatically once the roster grows —
    /// never hard-code Ace/Anchor here.
    /// </summary>
    public static class AITeamBuilder
    {
        public static List<string> Build(IReadOnlyList<SailorRosterEntry> availablePool, TeamBuildConstraints constraints, Random rng)
        {
            var result = new List<string>(constraints.TeamSize);
            if (availablePool == null || availablePool.Count == 0) return result;

            var perSailorCount = new Dictionary<string, int>();
            int tankCount = 0;

            for (int slot = 0; slot < constraints.TeamSize; slot++)
            {
                var candidates = new List<SailorRosterEntry>();
                foreach (var entry in availablePool)
                {
                    int count = perSailorCount.TryGetValue(entry.SailorId, out int c) ? c : 0;
                    if (count >= constraints.MaxDuplicatesPerTeam) continue;
                    if (entry.IsTank && tankCount >= constraints.MaxTanksPerTeam) continue;

                    candidates.Add(entry);
                }

                // Every remaining option violates a constraint (a small pool exhausts them
                // quickly) — relax by falling back to the full pool rather than under-filling
                // the team.
                if (candidates.Count == 0) candidates.AddRange(availablePool);

                var chosen = candidates[rng.Next(candidates.Count)];
                result.Add(chosen.SailorId);
                perSailorCount[chosen.SailorId] = (perSailorCount.TryGetValue(chosen.SailorId, out int existing) ? existing : 0) + 1;
                if (chosen.IsTank) tankCount++;
            }

            return result;
        }
    }
}
