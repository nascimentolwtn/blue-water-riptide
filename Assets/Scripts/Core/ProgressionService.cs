using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace BlueWaterRiptide.Core
{
    /// <summary>Connection-mode ids, matching ProfileData.stats.matchesByModeKeys (Plan 05 §1/§6). Online modes (Plan 03) don't exist yet — not included here.</summary>
    public static class ConnectionModeIds
    {
        public const string SinglePlayer = "singlePlayer";
        public const string LanCoop = "lanCoop";
        public const string LanPvp = "lanPvp";
    }

    /// <summary>What ApplyMatchResult changed, for a future Results screen to display.</summary>
    public sealed class MatchResultOutcome
    {
        public int TrophyDelta;
        public int DoubloonsEarned;
        public bool RankedUp;

        /// <summary>Highest tier newly claimed this call, if RankedUp is true.</summary>
        public FleetRankTier NewRankTier;

        public bool FirstWinOfDayBonus;
        public List<CommendationDefinition> NewlyUnlockedCommendations = new List<CommendationDefinition>();
    }

    /// <summary>
    /// Plan 05 gamification math: trophies, Fleet Rank, Doubloon sinks/sources, Commendations. Pure —
    /// takes a ProfileData and plain args, returns results; never reaches into MatchController/Session.
    /// Wiring this into the live match flow (calling ApplyMatchResult from a Results screen) is a follow-up.
    /// </summary>
    public static class ProgressionService
    {
        const int FirstWinOfDayDoubloonBonus = 20;

        /// <summary>100/200/400/800 per 00 §6 — index 0 is the cost of levelling 1->2, so 4 entries cap Sailors at level 5.</summary>
        static readonly int[] LevelUpCosts = { 100, 200, 400, 800 };

        public static int MaxSailorLevel => 1 + LevelUpCosts.Length;

        public static int GetAccountTrophyTotal(ProfileData profile)
        {
            int total = 0;
            if (profile?.sailors == null) return total;

            foreach (var entry in profile.sailors) total += entry.trophies;
            return total;
        }

        public static string GetFleetRankId(int trophyTotal) => FleetRankDefinition.GetTierForTrophyTotal(trophyTotal).Id;

        public static FleetRankTier GetFleetRankTier(ProfileData profile) => FleetRankDefinition.GetTierForTrophyTotal(GetAccountTrophyTotal(profile));

        static (int win, int loss) GetTrophyRates(string connectionMode)
        {
            switch (connectionMode)
            {
                case ConnectionModeIds.SinglePlayer:
                case ConnectionModeIds.LanCoop:
                case ConnectionModeIds.LanPvp:
                    return (2, 0); // Plan 05 §1: all three current modes share the SP-tier rate; only the (unbuilt) Online modes differ.
                default:
                    Debug.LogWarning($"ProgressionService: unknown connection mode '{connectionMode}', defaulting to Single Player trophy rates.");
                    return (2, 0);
            }
        }

        static SailorSaveEntry GetOrCreateSailorEntry(ProfileData profile, string sailorId)
        {
            foreach (var entry in profile.sailors)
            {
                if (entry.sailorId == sailorId) return entry;
            }

            var fresh = new SailorSaveEntry { sailorId = sailorId, level = 1 };
            profile.sailors.Add(fresh);
            return fresh;
        }

        static SailorSaveEntry FindSailorEntry(ProfileData profile, string sailorId)
        {
            foreach (var entry in profile.sailors)
            {
                if (entry.sailorId == sailorId) return entry;
            }

            return null;
        }

        static int GetOrAddStatsModeIndex(ProfileData profile, string connectionMode)
        {
            for (int i = 0; i < profile.stats.matchesByModeKeys.Count; i++)
            {
                if (profile.stats.matchesByModeKeys[i] == connectionMode) return i;
            }

            profile.stats.matchesByModeKeys.Add(connectionMode);
            profile.stats.matchesByModeValues.Add(0);
            return profile.stats.matchesByModeKeys.Count - 1;
        }

        /// <summary>
        /// Applies one match's worth of progression for the Sailor played: trophy delta (Plan 05 §1 rate
        /// table), win/loss/knockout counters, win streak, first-win-of-day bonus, Fleet Rank rank-ups,
        /// and any Commendations newly satisfied. Call once per match at Results — not idempotent,
        /// calling it twice for the same match double-counts. Pass a null/absent result (don't call this
        /// at all) for a disconnect/host-loss per Plan 05 §1's "no result = no delta".
        /// </summary>
        public static MatchResultOutcome ApplyMatchResult(ProfileData profile, string sailorId, string connectionMode, bool won, bool suddenDeathWin, int knockouts)
        {
            var outcome = new MatchResultOutcome();
            if (profile == null || string.IsNullOrEmpty(sailorId)) return outcome;

            var sailorEntry = GetOrCreateSailorEntry(profile, sailorId);

            var (winTrophies, lossTrophies) = GetTrophyRates(connectionMode);
            int delta = won ? winTrophies : lossTrophies;
            sailorEntry.trophies = Mathf.Max(0, sailorEntry.trophies + delta);
            sailorEntry.highestTrophies = Mathf.Max(sailorEntry.highestTrophies, sailorEntry.trophies);
            outcome.TrophyDelta = delta;

            if (won) sailorEntry.wins++;
            else sailorEntry.losses++;
            sailorEntry.knockouts += Mathf.Max(0, knockouts);

            profile.stats.matchesPlayed++;
            if (won) profile.stats.wins++;
            int modeIndex = GetOrAddStatsModeIndex(profile, connectionMode);
            profile.stats.matchesByModeValues[modeIndex]++;

            if (won && suddenDeathWin) profile.stats.suddenDeathWins++;

            if (won)
            {
                profile.stats.winStreakCurrent++;
                profile.stats.winStreakBest = Mathf.Max(profile.stats.winStreakBest, profile.stats.winStreakCurrent);
            }
            else
            {
                profile.stats.winStreakCurrent = 0;
            }

            if (won && IsFirstWinOfDay(profile))
            {
                profile.doubloons += FirstWinOfDayDoubloonBonus;
                profile.daily.lastFirstWinUtc = DateTime.UtcNow.ToString("o");
                outcome.FirstWinOfDayBonus = true;
                outcome.DoubloonsEarned += FirstWinOfDayDoubloonBonus;
            }

            ApplyRankUps(profile, outcome);

            outcome.NewlyUnlockedCommendations = CheckCommendations(profile, connectionMode, won, suddenDeathWin);
            foreach (var commendation in outcome.NewlyUnlockedCommendations)
            {
                profile.doubloons += commendation.DoubloonReward;
                outcome.DoubloonsEarned += commendation.DoubloonReward;
            }

            return outcome;
        }

        static bool IsFirstWinOfDay(ProfileData profile)
        {
            string lastUtcRaw = profile.daily.lastFirstWinUtc;
            if (string.IsNullOrEmpty(lastUtcRaw)) return true;

            if (!DateTime.TryParse(lastUtcRaw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind | DateTimeStyles.AdjustToUniversal, out var lastUtc))
            {
                return true; // unparseable stamp shouldn't block the bonus forever
            }

            return lastUtc.Date != DateTime.UtcNow.Date;
        }

        /// <summary>Claims every Fleet Rank tier now reachable that hasn't already been claimed (handles a rare multi-tier jump in one call, not just the tier immediately above the previous one).</summary>
        static void ApplyRankUps(ProfileData profile, MatchResultOutcome outcome)
        {
            int trophyTotal = GetAccountTrophyTotal(profile);

            foreach (var tier in FleetRankDefinition.TiersAscending)
            {
                if (tier.TrophyThreshold > trophyTotal) break;
                if (profile.fleetRank.claimedTierIds.Contains(tier.Id)) continue;

                profile.fleetRank.claimedTierIds.Add(tier.Id);
                profile.doubloons += tier.RankUpDoubloonReward;
                outcome.DoubloonsEarned += tier.RankUpDoubloonReward;
                outcome.RankedUp = true;
                outcome.NewRankTier = tier;
            }
        }

        /// <summary>
        /// Unlocks any CommendationCatalog entries now satisfied. lastMatch* args carry the current call's
        /// transient context (mode/won/sudden-death) for match-scoped Commendations that aren't worth a
        /// dedicated persisted counter (e.g. "play a LAN match") — everything else reads persisted stats.
        /// </summary>
        public static List<CommendationDefinition> CheckCommendations(ProfileData profile, string lastMatchConnectionMode = null, bool lastMatchWon = false, bool lastMatchSuddenDeathWin = false)
        {
            var newlyUnlocked = new List<CommendationDefinition>();
            if (profile == null) return newlyUnlocked;

            void TryUnlock(string commendationId, bool conditionMet)
            {
                if (!conditionMet) return;
                if (profile.commendations.unlockedIds.Contains(commendationId)) return;

                var def = CommendationCatalog.Get(commendationId);
                if (def == null) return;

                profile.commendations.unlockedIds.Add(commendationId);
                profile.commendations.unlockedAtUtc.Add(DateTime.UtcNow.ToString("o"));
                newlyUnlocked.Add(def);
            }

            int totalKnockouts = 0;
            foreach (var entry in profile.sailors) totalKnockouts += entry.knockouts;

            var aceEntry = FindSailorEntry(profile, "sailor.ace");
            var anchorEntry = FindSailorEntry(profile, "sailor.anchor");

            TryUnlock(CommendationCatalog.FirstKnockoutId, totalKnockouts > 0);
            TryUnlock(CommendationCatalog.WinAMatchId, profile.stats.wins > 0);
            TryUnlock(CommendationCatalog.UnlockAnchorId, anchorEntry != null);
            TryUnlock(CommendationCatalog.WinSuddenDeathId, profile.stats.suddenDeathWins > 0);
            TryUnlock(CommendationCatalog.WinTenWithAceId, aceEntry != null && aceEntry.wins >= 10);
            TryUnlock(CommendationCatalog.WinTenWithAnchorId, anchorEntry != null && anchorEntry.wins >= 10);
            TryUnlock(CommendationCatalog.ReachFrigateId, GetAccountTrophyTotal(profile) >= FleetRankDefinition.Get(FleetRankDefinition.FrigateId).TrophyThreshold);
            TryUnlock(CommendationCatalog.PlayLanMatchId, lastMatchConnectionMode == ConnectionModeIds.LanCoop || lastMatchConnectionMode == ConnectionModeIds.LanPvp);
            TryUnlock(CommendationCatalog.WinLanCoopMatchId, lastMatchWon && lastMatchConnectionMode == ConnectionModeIds.LanCoop);
            TryUnlock(CommendationCatalog.WinLanPvpMatchId, lastMatchWon && lastMatchConnectionMode == ConnectionModeIds.LanPvp);

            return newlyUnlocked;
        }

        /// <summary>Doubloon sink: 100/200/400/800 per level (00 §6). Returns false without side effects if the Sailor is already at MaxSailorLevel or the profile can't afford it.</summary>
        public static bool LevelUpSailor(ProfileData profile, string sailorId)
        {
            if (profile == null || string.IsNullOrEmpty(sailorId)) return false;

            var entry = GetOrCreateSailorEntry(profile, sailorId);
            int currentLevel = Mathf.Max(1, entry.level); // pre-Plan-05 saves may have level 0 (never written); floor at the starting level 1
            int levelIndex = currentLevel - 1; // cost of going from currentLevel to currentLevel + 1
            if (levelIndex < 0 || levelIndex >= LevelUpCosts.Length) return false;

            int cost = LevelUpCosts[levelIndex];
            if (profile.doubloons < cost) return false;

            profile.doubloons -= cost;
            entry.level = currentLevel + 1;
            return true;
        }
    }
}
