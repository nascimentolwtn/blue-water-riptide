using System;
using System.Collections.Generic;

namespace BlueWaterRiptide.Core.Progression
{
    /// <summary>Per-Sailor progression record, id-keyed (Plan 05 §6).</summary>
    [Serializable]
    public sealed class SailorProgressionRecord
    {
        public string sailorId;
        public int trophies;
        public int highestTrophies;
        public int level = 1;
        public int wins;
        public int losses;
        public int knockouts;
    }

    [Serializable]
    public sealed class VoyageRoadRecord
    {
        public List<string> claimedNodeIds = new List<string>();
    }

    [Serializable]
    public sealed class FleetRankRecord
    {
        public List<string> claimedTierIds = new List<string>();
    }

    [Serializable]
    public sealed class CommendationEntry
    {
        public string commendationId;
        public string unlockedAtUtc;
    }

    [Serializable]
    public sealed class MatchesByModeEntry
    {
        public string mode;
        public int count;
    }

    [Serializable]
    public sealed class StatsRecord
    {
        public List<MatchesByModeEntry> matchesByMode = new List<MatchesByModeEntry>();
        public int suddenDeathWins;
        public int winStreakCurrent;
        public int winStreakBest;
    }

    [Serializable]
    public sealed class DailyRecord
    {
        public string lastFirstWinUtc = "";
    }

    [Serializable]
    public sealed class SettingsRecord
    {
        public string aiDifficulty = "bosun";
        public string joystickSide = "left";
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
    }

    /// <summary>
    /// Local JSON save schema (Plan 05 §6 / Plan 00 §6). Content matches the plan's schema field
    /// for field; the wire *shape* substitutes List-of-entry for the plan's illustrative
    /// dictionaries (`sailors`, `commendations.unlocked`, `stats.matchesByMode`) because
    /// UnityEngine.JsonUtility — the only JSON serializer available without adding a package —
    /// cannot serialize Dictionary at all. Ids stay the lookup key in every entry, so nothing
    /// about the "string content ids everywhere, ids not indices" contract is lost, only the
    /// on-disk representation of the id -> record mapping.
    /// </summary>
    [Serializable]
    public sealed class SaveProfile
    {
        public int schemaVersion = SaveService.CurrentSchemaVersion;
        public string profileId;
        public string displayName = "Player";
        public string createdAtUtc;
        public int doubloons;
        public List<SailorProgressionRecord> sailors = new List<SailorProgressionRecord>();
        public VoyageRoadRecord voyageRoad = new VoyageRoadRecord();
        public FleetRankRecord fleetRank = new FleetRankRecord();
        public List<CommendationEntry> commendations = new List<CommendationEntry>();
        public StatsRecord stats = new StatsRecord();
        public DailyRecord daily = new DailyRecord();
        public SettingsRecord settings = new SettingsRecord();
    }
}
