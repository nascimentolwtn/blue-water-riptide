using System;
using System.Collections.Generic;

namespace BlueWaterRiptide.Core
{
    /// <summary>One Sailor's save-relevant progress. List instead of a dictionary — JsonUtility can't serialize Dictionary&lt;TKey,TValue&gt;.</summary>
    [Serializable]
    public class SailorSaveEntry
    {
        public string sailorId;
        public int level;
        public int upgrades;

        public int trophies;

        /// <summary>Needed now so a future seasonal reset (Plan 05 §2 "Flagship League") has a basis to reset against.</summary>
        public int highestTrophies;

        public int wins;
        public int losses;
        public int knockouts;
    }

    /// <summary>Voyage Road progression track (00 §6). Nodes are referenced by id, never index, so reordering content can't corrupt a save.</summary>
    [Serializable]
    public class VoyageRoadData
    {
        public int currentNodeIndex;
        public List<string> claimedNodeIds = new List<string>();
    }

    /// <summary>Cosmetic Fleet Rank tiering (Plan 05 §2). The trophy total itself is derived (ProgressionService) — only one-time rank-up grants are stored.</summary>
    [Serializable]
    public class FleetRankData
    {
        public List<string> claimedTierIds = new List<string>();
    }

    /// <summary>Achievement unlocks (Plan 05 §4 item 2). Parallel lists instead of a dictionary — JsonUtility can't serialize Dictionary&lt;TKey,TValue&gt;.</summary>
    [Serializable]
    public class CommendationsData
    {
        public List<string> unlockedIds = new List<string>();

        /// <summary>Same index as unlockedIds; UTC ISO-8601 (DateTime.UtcNow.ToString("o")).</summary>
        public List<string> unlockedAtUtc = new List<string>();
    }

    /// <summary>Lifetime match stats (Plan 05 §6) — monotonic counters, feeds Commendations/records.</summary>
    [Serializable]
    public class StatsData
    {
        public int matchesPlayed;
        public int wins;

        /// <summary>Connection-mode keys, e.g. "singlePlayer"/"lanCoop"/"lanPvp" — parallel to matchesByModeValues; JsonUtility can't serialize Dictionary.</summary>
        public List<string> matchesByModeKeys = new List<string>();
        public List<int> matchesByModeValues = new List<int>();

        public int suddenDeathWins;
        public int winStreakCurrent;
        public int winStreakBest;
    }

    /// <summary>Daily-cadence bonuses (Plan 05 §4 item 1).</summary>
    [Serializable]
    public class DailyData
    {
        /// <summary>UTC ISO-8601 timestamp of the last match that granted the first-win-of-the-day bonus.</summary>
        public string lastFirstWinUtc;
    }

    /// <summary>Local device settings, persisted so they survive an app reinstall via the same profile.</summary>
    [Serializable]
    public class SettingsData
    {
        public string aiDifficulty = "bosun";
        public string joystickSide = "left";
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
    }

    /// <summary>
    /// Local save schema (Plan 05 §6). schemaVersion 2 adds Trophies/Fleet Rank/Commendations/daily/settings
    /// on top of the v1 placeholders — see SaveService.Load()'s migrator.
    /// </summary>
    [Serializable]
    public class ProfileData
    {
        public int schemaVersion = 2;
        public string profileId;
        public string displayName;
        public string createdAtUtc;
        public int doubloons;
        public List<SailorSaveEntry> sailors = new List<SailorSaveEntry>();

        public VoyageRoadData voyageRoad = new VoyageRoadData();
        public FleetRankData fleetRank = new FleetRankData();
        public CommendationsData commendations = new CommendationsData();
        public StatsData stats = new StatsData();
        public DailyData daily = new DailyData();
        public SettingsData settings = new SettingsData();
    }
}
