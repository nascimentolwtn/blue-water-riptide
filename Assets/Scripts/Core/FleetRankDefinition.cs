using System.Collections.Generic;
using UnityEngine;

namespace BlueWaterRiptide.Core
{
    /// <summary>One cosmetic Fleet Rank tier (Plan 05 §2) — a pure function of account trophy total, no per-Sailor gating.</summary>
    public sealed class FleetRankTier
    {
        public string Id;
        public string DisplayName;
        public int TrophyThreshold;

        /// <summary>One-time Doubloon grant when a profile first crosses into this tier (ProgressionService).</summary>
        public int RankUpDoubloonReward;
    }

    /// <summary>
    /// Single-entry id-keyed Fleet Rank tier table (Plan 05 §2), same Dictionary + lookup pattern as
    /// ArenaCatalog. Ordered ascending by TrophyThreshold — extend this table, not callers, to add tiers.
    /// </summary>
    public static class FleetRankDefinition
    {
        public const string DinghyId = "rank.dinghy";
        public const string SloopId = "rank.sloop";
        public const string CutterId = "rank.cutter";
        public const string FrigateId = "rank.frigate";
        public const string CruiserId = "rank.cruiser";
        public const string BattleshipId = "rank.battleship";
        public const string FlagshipId = "rank.flagship";

        /// <summary>Ordered ascending by TrophyThreshold — GetTierForTrophyTotal relies on this order.</summary>
        static readonly List<FleetRankTier> _tiersAscending = new List<FleetRankTier>
        {
            new FleetRankTier { Id = DinghyId, DisplayName = "Dinghy", TrophyThreshold = 0, RankUpDoubloonReward = 0 },
            new FleetRankTier { Id = SloopId, DisplayName = "Sloop", TrophyThreshold = 40, RankUpDoubloonReward = 10 },
            new FleetRankTier { Id = CutterId, DisplayName = "Cutter", TrophyThreshold = 100, RankUpDoubloonReward = 10 },
            new FleetRankTier { Id = FrigateId, DisplayName = "Frigate", TrophyThreshold = 200, RankUpDoubloonReward = 10 },
            new FleetRankTier { Id = CruiserId, DisplayName = "Cruiser", TrophyThreshold = 350, RankUpDoubloonReward = 10 },
            new FleetRankTier { Id = BattleshipId, DisplayName = "Battleship", TrophyThreshold = 550, RankUpDoubloonReward = 10 },
            new FleetRankTier { Id = FlagshipId, DisplayName = "Flagship", TrophyThreshold = 800, RankUpDoubloonReward = 10 },
        };

        static readonly Dictionary<string, FleetRankTier> _tiersById = BuildLookup();

        static Dictionary<string, FleetRankTier> BuildLookup()
        {
            var lookup = new Dictionary<string, FleetRankTier>();
            foreach (var tier in _tiersAscending) lookup[tier.Id] = tier;
            return lookup;
        }

        /// <summary>Ascending by TrophyThreshold; Dinghy first, Flagship last.</summary>
        public static IReadOnlyList<FleetRankTier> TiersAscending => _tiersAscending;

        public static FleetRankTier Get(string tierId)
        {
            if (tierId != null && _tiersById.TryGetValue(tierId, out var tier)) return tier;

            Debug.LogWarning($"FleetRankDefinition: unknown tier id '{tierId}', falling back to '{DinghyId}'.");
            return _tiersById[DinghyId];
        }

        /// <summary>Highest tier whose TrophyThreshold is &lt;= total (Plan 05 §2 — rank is a pure function of account trophy total).</summary>
        public static FleetRankTier GetTierForTrophyTotal(int trophyTotal)
        {
            var highestMatch = _tiersAscending[0];
            foreach (var tier in _tiersAscending)
            {
                if (tier.TrophyThreshold > trophyTotal) break;
                highestMatch = tier;
            }

            return highestMatch;
        }
    }
}
