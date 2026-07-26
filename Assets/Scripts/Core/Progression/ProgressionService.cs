using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.Core.Progression
{
    /// <summary>
    /// Owns the loaded SaveProfile and every progression rule from Plan 05 — trophy rates,
    /// derived account total/Fleet Rank, Voyage Road claims, first-win-of-day, level-ups.
    /// Nothing here talks to MatchController/Session directly; callers build a MatchResult and
    /// hand it to ApplyMatchResult, so this service works the same from any connection mode.
    /// </summary>
    public sealed class ProgressionService
    {
        // Plan 05 §1: (winDelta, lossDelta) per connection mode. Loss deltas are already negative.
        static readonly Dictionary<ConnectionMode, (int win, int loss)> TrophyRates = new Dictionary<ConnectionMode, (int, int)>
        {
            { ConnectionMode.SinglePlayer, (2, 0) },
            { ConnectionMode.LanCoop, (2, 0) },
            { ConnectionMode.LanPvp, (2, 0) },
            { ConnectionMode.OnlineSolo, (4, -2) },
            { ConnectionMode.OnlineFriendLobby, (2, 0) },
        };

        static readonly Dictionary<ConnectionMode, string> ModeStatsKey = new Dictionary<ConnectionMode, string>
        {
            { ConnectionMode.SinglePlayer, "singlePlayer" },
            { ConnectionMode.LanCoop, "lanCoop" },
            { ConnectionMode.LanPvp, "lanPvp" },
            { ConnectionMode.OnlineSolo, "onlineSolo" },
            { ConnectionMode.OnlineFriendLobby, "onlineFriendLobby" },
        };

        const int FirstWinOfDayDoubloonBonus = 20;
        const int MaxSailorLevel = 5;
        // Cost to go from level N to N+1 — index 0 is the 1->2 cost (Plan 00 §6).
        static readonly int[] LevelUpCosts = { 100, 200, 400, 800 };

        readonly SaveService _saveService;

        public SaveProfile Profile { get; private set; }

        public event Action OnProfileChanged;

        public ProgressionService(SaveService saveService)
        {
            _saveService = saveService;
            Profile = _saveService.Load();
        }

        public int GetAccountTrophyTotal() => Profile.sailors.Sum(s => s.trophies);

        public FleetRankTier GetCurrentFleetRank() => FleetRankCatalog.GetTierForTrophyTotal(GetAccountTrophyTotal());

        /// <summary>Ace is the starter; every other Sailor is unlocked by claiming the Voyage Road
        /// node that names them (derived from claims, never stored as its own flag).</summary>
        public bool IsSailorUnlocked(string sailorId)
        {
            if (sailorId == SaveService.StarterSailorId) return true;

            return VoyageRoadCatalog.AllAscending
                .Where(n => n.UnlocksSailorId == sailorId)
                .Any(n => Profile.voyageRoad.claimedNodeIds.Contains(n.Id));
        }

        public static float LevelStatMultiplier(int level) => 1f + 0.05f * (level - 1);

        /// <summary>Applies a finished match's trophy/Doubloon/stat deltas, then saves once.
        /// No-ops entirely for an incomplete match (disconnect/host-loss — Plan 05 §1).</summary>
        public void ApplyMatchResult(MatchResult result, DateTime nowUtc)
        {
            if (!result.Completed) return;

            var sailor = GetOrCreateSailorRecord(result.LocalSailorId);
            var (winDelta, lossDelta) = TrophyRates[result.Mode];
            int delta = result.LocalWon ? winDelta : lossDelta;

            sailor.trophies = Math.Max(0, sailor.trophies + delta);
            sailor.highestTrophies = Math.Max(sailor.highestTrophies, sailor.trophies);
            sailor.knockouts += result.LocalKnockouts;

            if (result.LocalWon) sailor.wins++;
            else sailor.losses++;

            IncrementMatchesByMode(result.Mode);

            if (result.LocalWon)
            {
                Profile.stats.winStreakCurrent++;
                Profile.stats.winStreakBest = Math.Max(Profile.stats.winStreakBest, Profile.stats.winStreakCurrent);
                if (result.WonViaSuddenDeath) Profile.stats.suddenDeathWins++;
                TryGrantFirstWinOfDay(nowUtc);
            }
            else
            {
                Profile.stats.winStreakCurrent = 0;
            }

            ClaimReachedFleetRankTiers();
            _saveService.Save(Profile);
            OnProfileChanged?.Invoke();
        }

        /// <summary>Grants a Doubloon bonus once per UTC calendar day, regardless of caller —
        /// callers decide when a "win" happened (Plan 05 §4.1).</summary>
        public bool TryGrantFirstWinOfDay(DateTime nowUtc)
        {
            if (DateTime.TryParse(Profile.daily.lastFirstWinUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var last)
                && last.Date == nowUtc.Date)
            {
                return false;
            }

            Profile.daily.lastFirstWinUtc = nowUtc.ToString("o");
            AddDoubloons(FirstWinOfDayDoubloonBonus);
            return true;
        }

        public void AddDoubloons(int amount)
        {
            if (amount <= 0) return;
            Profile.doubloons += amount;
        }

        public bool TrySpendDoubloons(int amount)
        {
            if (amount <= 0 || Profile.doubloons < amount) return false;
            Profile.doubloons -= amount;
            return true;
        }

        public bool TryClaimVoyageNode(string nodeId)
        {
            if (!VoyageRoadCatalog.TryGet(nodeId, out var node)) return false;
            if (Profile.voyageRoad.claimedNodeIds.Contains(nodeId)) return false;
            if (node.TrophyThreshold > GetAccountTrophyTotal()) return false;

            Profile.voyageRoad.claimedNodeIds.Add(nodeId);
            AddDoubloons(node.DoubloonReward);
            _saveService.Save(Profile);
            OnProfileChanged?.Invoke();
            return true;
        }

        /// <summary>Grants every Fleet Rank tier newly reached since the last claim, ascending.
        /// Called automatically from ApplyMatchResult (the only thing that changes trophies); the
        /// return value is what a rank-up celebration UI would consume.</summary>
        public IReadOnlyList<FleetRankTier> ClaimReachedFleetRankTiers()
        {
            var reached = FleetRankCatalog.GetUnclaimedReachedTiers(GetAccountTrophyTotal(), Profile.fleetRank.claimedTierIds).ToList();
            foreach (var tier in reached)
            {
                Profile.fleetRank.claimedTierIds.Add(tier.Id);
                AddDoubloons(tier.DoubloonReward);
            }
            return reached;
        }

        public bool TryLevelUpSailor(string sailorId)
        {
            var sailor = GetOrCreateSailorRecord(sailorId);
            if (sailor.level >= MaxSailorLevel) return false;

            int cost = LevelUpCosts[sailor.level - 1];
            if (!TrySpendDoubloons(cost)) return false;

            sailor.level++;
            _saveService.Save(Profile);
            OnProfileChanged?.Invoke();
            return true;
        }

        SailorProgressionRecord GetOrCreateSailorRecord(string sailorId)
        {
            var existing = Profile.sailors.FirstOrDefault(s => s.sailorId == sailorId);
            if (existing != null) return existing;

            var created = new SailorProgressionRecord { sailorId = sailorId };
            Profile.sailors.Add(created);
            return created;
        }

        void IncrementMatchesByMode(ConnectionMode mode)
        {
            string key = ModeStatsKey[mode];
            var entry = Profile.stats.matchesByMode.FirstOrDefault(e => e.mode == key);
            if (entry == null)
            {
                entry = new MatchesByModeEntry { mode = key };
                Profile.stats.matchesByMode.Add(entry);
            }
            entry.count++;
        }
    }
}
