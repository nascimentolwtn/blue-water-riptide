using System.Collections.Generic;
using System.Linq;

namespace BlueWaterRiptide.Core.Progression
{
    public sealed class FleetRankTier
    {
        public string Id { get; }
        public string DisplayName { get; }
        public int TrophyThreshold { get; }

        public FleetRankTier(string id, string displayName, int trophyThreshold)
        {
            Id = id;
            DisplayName = displayName;
            TrophyThreshold = trophyThreshold;
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
            new FleetRankTier("rank.dinghy", "Dinghy", 0),
            new FleetRankTier("rank.sloop", "Sloop", 40),
            new FleetRankTier("rank.cutter", "Cutter", 100),
            new FleetRankTier("rank.frigate", "Frigate", 200),
            new FleetRankTier("rank.cruiser", "Cruiser", 350),
            new FleetRankTier("rank.battleship", "Battleship", 550),
            new FleetRankTier("rank.flagship", "Flagship", 800),
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
