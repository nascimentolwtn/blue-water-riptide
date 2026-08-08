using System.Collections.Generic;
using UnityEngine;

namespace BlueWaterRiptide.Core
{
    /// <summary>
    /// Static, data-only description of one arena: geometry the round loop and Rising Tide hazard
    /// need (00 §5 "Tideline Cove"). Values below are exactly what M1PrototypeBootstrap used to
    /// hardcode inline (Plan 08 step 3) — moved here so a second arena is data, not new bootstrap code.
    /// </summary>
    public sealed class ArenaDefinition
    {
        public string Id;
        public string DisplayName;

        /// <summary>Playable-area half-extents on the XZ plane (matches SailorPawn.ArenaHalfExtents).</summary>
        public Vector2 HalfExtents;

        public Vector3[] SpawnPositionsTeamA;
        public Vector3[] SpawnPositionsTeamB;

        /// <summary>Seconds of no-damage grace a Sailor gets right after ResetForRound places them at a spawn pad.</summary>
        public float SpawnImmunityDuration = 1.5f;

        /// <summary>World units each edge of the safe zone recedes per Sudden Death ring tick (MatchRules.SuddenDeathRingInterval).</summary>
        public float SuddenDeathRingShrinkStep = 2f;

        /// <summary>Floor on the shrinking safe zone's half-extents so it can never collapse to zero/negative.</summary>
        public float SuddenDeathMinHalfExtent = 3f;

        /// <summary>Resources-loadable path for the arena's greybox prefab, e.g. via `Resources.Load&lt;GameObject&gt;(PrefabResourcePath)`.</summary>
        public string PrefabResourcePath;
    }

    /// <summary>
    /// Single-entry id-keyed arena registry (Plan 08 step 3). Only "arena.tideline_cove" exists for
    /// M1 — expand this dictionary, not the callers, when a second arena is added.
    /// </summary>
    public static class ArenaCatalog
    {
        public const string TidelineCoveId = "arena.tideline_cove";

        static readonly Dictionary<string, ArenaDefinition> _arenas = new Dictionary<string, ArenaDefinition>
        {
            [TidelineCoveId] = new ArenaDefinition
            {
                Id = TidelineCoveId,
                DisplayName = "Tideline Cove",
                HalfExtents = new Vector2(16.5f, 9.5f),
                SpawnPositionsTeamA = new[] { new Vector3(-8f, 1f, 0f) },
                SpawnPositionsTeamB = new[] { new Vector3(8f, 1f, 0f) },
                SpawnImmunityDuration = 1.5f,
                SuddenDeathRingShrinkStep = 2f,
                SuddenDeathMinHalfExtent = 3f,
                PrefabResourcePath = "Environment/TidelineCove",
            }
        };

        /// <summary>M1's only arena. Bootstrap uses this until a mode/lobby can pick by id (Plan 04).</summary>
        public static ArenaDefinition Default => _arenas[TidelineCoveId];

        public static ArenaDefinition Get(string arenaId)
        {
            if (arenaId != null && _arenas.TryGetValue(arenaId, out var def)) return def;

            Debug.LogWarning($"ArenaCatalog: unknown arena id '{arenaId}', falling back to '{TidelineCoveId}'.");
            return Default;
        }
    }
}
