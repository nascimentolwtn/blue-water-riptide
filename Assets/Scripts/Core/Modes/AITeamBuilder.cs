using System.Collections.Generic;
using UnityEngine;
using BlueWaterRiptide.Characters;

namespace BlueWaterRiptide.Core.Modes
{
    /// <summary>Roster-sampling limits for one AI team. Written generically so a pool of 2 (today's
    /// full v1 roster) mostly no-ops while still holding once more Sailors ship (Plan 01 §2).</summary>
    public sealed class AITeamBuildConstraints
    {
        public int TeamSize = 3;

        /// <summary>Max copies of the same Sailor id allowed on one team.</summary>
        public int MaxDuplicatesPerSailor = 2;

        /// <summary>Max Sailors with Definition.KnockbackImmune == true allowed on one team.</summary>
        public int MaxTanksPerTeam = 1;
    }

    /// <summary>
    /// Samples a roster of Sailor ids for one AI team from an available pool, respecting duplicate
    /// and tank-count constraints. Deliberately keyed off SailorDefinitionData fields (KnockbackImmune)
    /// rather than any hardcoded id — Plan 01 §2's "do not hard-code Ace/Anchor anywhere in AI code."
    /// </summary>
    public static class AITeamBuilder
    {
        public static IReadOnlyList<string> Build(IReadOnlyList<SailorDefinitionData> availableSailors, AITeamBuildConstraints constraints)
        {
            var result = new List<string>();
            if (availableSailors == null || availableSailors.Count == 0 || constraints == null || constraints.TeamSize <= 0)
            {
                return result;
            }

            var pool = new List<SailorDefinitionData>(availableSailors);
            Shuffle(pool);

            var usageCount = new Dictionary<string, int>();
            int tankCount = 0;

            while (result.Count < constraints.TeamSize)
            {
                SailorDefinitionData pick = FindEligibleCandidate(pool, usageCount, tankCount, constraints);

                // Pool too small to satisfy constraints at this team size (e.g. more slots than the
                // duplicate/tank caps allow) — fill the remaining slots anyway rather than under-filling
                // the team, relaxing the duplicate cap as a last resort.
                if (pick == null)
                {
                    pick = pool[result.Count % pool.Count];
                }

                result.Add(pick.Id);
                usageCount[pick.Id] = (usageCount.TryGetValue(pick.Id, out var current) ? current : 0) + 1;
                if (pick.KnockbackImmune) tankCount++;
            }

            return result;
        }

        static SailorDefinitionData FindEligibleCandidate(List<SailorDefinitionData> pool, Dictionary<string, int> usageCount, int tankCount, AITeamBuildConstraints constraints)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                var candidate = pool[i];
                int used = usageCount.TryGetValue(candidate.Id, out var c) ? c : 0;
                if (used >= constraints.MaxDuplicatesPerSailor) continue;
                if (candidate.KnockbackImmune && tankCount >= constraints.MaxTanksPerTeam) continue;

                return candidate;
            }

            return null;
        }

        static void Shuffle(List<SailorDefinitionData> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
