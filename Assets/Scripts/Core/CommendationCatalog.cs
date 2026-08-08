using System.Collections.Generic;
using UnityEngine;

namespace BlueWaterRiptide.Core
{
    /// <summary>One achievement definition (Plan 05 §4 item 2). Unlock conditions live in ProgressionService.CheckCommendations, not here — this is data only.</summary>
    public sealed class CommendationDefinition
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public int DoubloonReward;
    }

    /// <summary>
    /// Small id-keyed catalog of Commendations (Plan 05 §4 item 2 example list). Same Dictionary + lookup
    /// pattern as ArenaCatalog. Deliberately ~10 entries for the 2-Sailor/1-arena M1 slice — expand this
    /// table, not callers, as roster/content grows.
    /// </summary>
    public static class CommendationCatalog
    {
        public const string FirstKnockoutId = "comm.first-ko";
        public const string WinAMatchId = "comm.win-a-match";
        public const string UnlockAnchorId = "comm.unlock-anchor";
        public const string WinSuddenDeathId = "comm.win-sudden-death";
        public const string WinTenWithAceId = "comm.win-10-ace";
        public const string WinTenWithAnchorId = "comm.win-10-anchor";
        public const string ReachFrigateId = "comm.reach-frigate";
        public const string PlayLanMatchId = "comm.play-lan-match";
        public const string WinLanCoopMatchId = "comm.win-lan-coop-match";
        public const string WinLanPvpMatchId = "comm.win-lan-pvp-match";

        static readonly Dictionary<string, CommendationDefinition> _commendations = new Dictionary<string, CommendationDefinition>
        {
            [FirstKnockoutId] = new CommendationDefinition
            {
                Id = FirstKnockoutId,
                DisplayName = "First Blood",
                Description = "Score your first knockout.",
                DoubloonReward = 10
            },
            [WinAMatchId] = new CommendationDefinition
            {
                Id = WinAMatchId,
                DisplayName = "Taking the Cup",
                Description = "Win a match.",
                DoubloonReward = 10
            },
            [UnlockAnchorId] = new CommendationDefinition
            {
                Id = UnlockAnchorId,
                DisplayName = "Heavy Reinforcements",
                Description = "Unlock Admiral Anchor.",
                DoubloonReward = 20
            },
            [WinSuddenDeathId] = new CommendationDefinition
            {
                Id = WinSuddenDeathId,
                DisplayName = "Down to the Wire",
                Description = "Win a round in Sudden Death.",
                DoubloonReward = 20
            },
            [WinTenWithAceId] = new CommendationDefinition
            {
                Id = WinTenWithAceId,
                DisplayName = "Ace's Wingmate",
                Description = "Win 10 matches with Ensign Ace.",
                DoubloonReward = 30
            },
            [WinTenWithAnchorId] = new CommendationDefinition
            {
                Id = WinTenWithAnchorId,
                DisplayName = "Anchor's Watch",
                Description = "Win 10 matches with Admiral Anchor.",
                DoubloonReward = 30
            },
            [ReachFrigateId] = new CommendationDefinition
            {
                Id = ReachFrigateId,
                DisplayName = "Fleet Colors",
                Description = "Reach Frigate Fleet Rank.",
                DoubloonReward = 30
            },
            [PlayLanMatchId] = new CommendationDefinition
            {
                Id = PlayLanMatchId,
                DisplayName = "Shipmates",
                Description = "Play a LAN match.",
                DoubloonReward = 10
            },
            [WinLanCoopMatchId] = new CommendationDefinition
            {
                Id = WinLanCoopMatchId,
                DisplayName = "Squadron Tactics",
                Description = "Win a LAN co-op match against AI.",
                DoubloonReward = 15
            },
            [WinLanPvpMatchId] = new CommendationDefinition
            {
                Id = WinLanPvpMatchId,
                DisplayName = "Friendly Fire",
                Description = "Win a LAN PvP match.",
                DoubloonReward = 15
            },
        };

        public static CommendationDefinition Get(string commendationId)
        {
            if (commendationId != null && _commendations.TryGetValue(commendationId, out var def)) return def;

            Debug.LogWarning($"CommendationCatalog: unknown commendation id '{commendationId}'.");
            return null;
        }

        public static IReadOnlyCollection<CommendationDefinition> All => _commendations.Values;
    }
}
