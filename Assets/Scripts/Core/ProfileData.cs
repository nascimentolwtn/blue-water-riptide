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
    }

    /// <summary>Placeholder — Voyage Road progression lands in a later milestone (00 §"Progression").</summary>
    [Serializable]
    public class VoyageRoadData
    {
        public int currentNodeIndex;
    }

    /// <summary>Placeholder — cosmetic Fleet Rank tiering (Plan 05).</summary>
    [Serializable]
    public class FleetRankData
    {
        public int trophies;
    }

    /// <summary>Placeholder — commendations/achievements (future milestone).</summary>
    [Serializable]
    public class CommendationsData
    {
    }

    /// <summary>Placeholder — lifetime match stats (future milestone).</summary>
    [Serializable]
    public class StatsData
    {
        public int matchesPlayed;
        public int wins;
    }

    /// <summary>
    /// Local save schema (mirrors Plan 05), M1-scoped basics only. Empty placeholder sections are
    /// included up front specifically so schemaVersion doesn't need bumping when voyageRoad/
    /// fleetRank/commendations/stats gain real fields in M2/M3 — JsonUtility just leaves unknown-at-
    /// the-time fields at their default until this class grows to match.
    /// </summary>
    [Serializable]
    public class ProfileData
    {
        public int schemaVersion = 1;
        public string profileId;
        public string displayName;
        public string createdAtUtc;
        public int doubloons;
        public List<SailorSaveEntry> sailors = new List<SailorSaveEntry>();

        public VoyageRoadData voyageRoad = new VoyageRoadData();
        public FleetRankData fleetRank = new FleetRankData();
        public CommendationsData commendations = new CommendationsData();
        public StatsData stats = new StatsData();
    }
}
