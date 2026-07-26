using System.Collections.Generic;
using System.Linq;

namespace BlueWaterRiptide.Core.Progression
{
    public sealed class FleetRankTier
    {
        public string Id { get; }
        public string DisplayName { get; }
        public int TrophyThreshold { get; }
        /// <summary>One-time Doubloon grant on first reaching this tier (Plan 05 §2). Placeholder
        /// tuning default, not a balanced number — Dinghy (the starting tier) grants nothing.</summary>
        public int DoubloonReward { get; }

        public FleetRankTier(string id, string displayName, int trophyThreshold, int doubloonReward)
        {
            Id = id;
            DisplayName = displayName;
            TrophyThreshold = trophyThreshold;
            DoubloonReward = doubloonReward;
        }
    }

    /// <summary>
    /// v1 Fleet Rank tiers (Plan 05 §2) — cosmetic, trophy-derived, non-seasonal. Ordered
    /// ascending by threshold; a tier's badge applies as soon as the account trophy total
    /// reaches its threshold, derived fresh on every read (never stored as its own field).
    /// </summary>
    public static class FleetRankCatalog
    {
        static readonly List<FleetRankTier> TiersAscending = new List<FleetRankTier>
        {
            new FleetRankTier("rank.dinghy", "Dinghy", 0, doubloonReward: 0),
            new FleetRankTier("rank.sloop", "Sloop", 40, doubloonReward: 20),
            new FleetRankTier("rank.cutter", "Cutter", 100, doubloonReward: 30),
            new FleetRankTier("rank.frigate", "Frigate", 200, doubloonReward: 40),
            new FleetRankTier("rank.cruiser", "Cruiser", 350, doubloonReward: 50),
            new FleetRankTier("rank.battleship", "Battleship", 550, doubloonReward: 75),
            new FleetRankTier("rank.flagship", "Flagship", 800, doubloonReward: 150),
        };

        public static IReadOnlyList<FleetRankTier> AllAscending => TiersAscending;

        /// <summary>Highest tier whose threshold the given account trophy total has reached.</summary>
        public static FleetRankTier GetTierForTrophyTotal(int accountTrophyTotal)
        {
            FleetRankTier result = TiersAscending[0];
            foreach (var tier in TiersAscending)
            {
                if (tier.TrophyThreshold > accountTrophyTotal) break;
                result = tier;
            }
            return result;
        }

        /// <summary>Tiers reached but not yet present in a profile's claimedTierIds, ascending.</summary>
        public static IEnumerable<FleetRankTier> GetUnclaimedReachedTiers(int accountTrophyTotal, IReadOnlyCollection<string> claimedTierIds)
        {
            return TiersAscending.Where(t => t.TrophyThreshold <= accountTrophyTotal && !claimedTierIds.Contains(t.Id));
        }
    }
}
