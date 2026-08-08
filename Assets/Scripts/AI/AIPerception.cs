using System.Collections.Generic;
using UnityEngine;
using BlueWaterRiptide.Core;
using BlueWaterRiptide.Characters;

namespace BlueWaterRiptide.AI
{
    /// <summary>Read-only snapshot of one Sailor as seen by an AI driver — never a live SailorPawn reference,
    /// so behavior scoring can't accidentally call into gameplay APIs (Plan 01 §3: "AI never calls gameplay
    /// APIs directly").</summary>
    public readonly struct AIUnitSnapshot
    {
        public readonly ParticipantId Id;
        public readonly Team Team;
        public readonly SailorDefinitionData Definition;
        public readonly Vector3 Position;
        public readonly float HpFraction;
        public readonly int CurrentAmmo;
        public readonly bool IsKnockedOut;
        public readonly float SuperCharge01;

        public AIUnitSnapshot(ParticipantId id, Team team, SailorDefinitionData definition, Vector3 position, float hpFraction, int currentAmmo, bool isKnockedOut, float superCharge01)
        {
            Id = id;
            Team = team;
            Definition = definition;
            Position = position;
            HpFraction = hpFraction;
            CurrentAmmo = currentAmmo;
            IsKnockedOut = isKnockedOut;
            SuperCharge01 = superCharge01;
        }
    }

    /// <summary>
    /// Read-only world view an AIInputDriver scores decisions against. Built fresh each tick by
    /// whatever integration layer owns the live SailorPawns (out of scope here) — this type itself
    /// has no dependency on MonoBehaviours or the simulation, only plain snapshots.
    /// </summary>
    public sealed class AIPerception
    {
        public AIUnitSnapshot Self { get; }
        public IReadOnlyList<AIUnitSnapshot> Allies { get; }
        public IReadOnlyList<AIUnitSnapshot> Enemies { get; }
        public Vector2 ArenaHalfExtents { get; }
        public float MatchTimeRemaining { get; }

        /// <summary>True while MatchController.CurrentCombatPhase == SuddenDeath — drives the SuddenDeathPush utility state.</summary>
        public bool IsSuddenDeath { get; }

        /// <summary>Hook for a future arena feature (tall-grass-style concealment) that doesn't exist yet
        /// in this flat-plane prototype. Always false today — see Plan 01 §3.</summary>
        public bool IsConcealed { get; }

        public AIPerception(AIUnitSnapshot self, IReadOnlyList<AIUnitSnapshot> allies, IReadOnlyList<AIUnitSnapshot> enemies, Vector2 arenaHalfExtents, float matchTimeRemaining, bool isSuddenDeath = false, bool isConcealed = false)
        {
            Self = self;
            Allies = allies;
            Enemies = enemies;
            ArenaHalfExtents = arenaHalfExtents;
            MatchTimeRemaining = matchTimeRemaining;
            IsSuddenDeath = isSuddenDeath;
            IsConcealed = isConcealed;
        }

        /// <summary>Nearest living enemy to Self, or null if none remain.</summary>
        public AIUnitSnapshot? NearestEnemy()
        {
            AIUnitSnapshot? best = null;
            float bestSqrDist = float.MaxValue;
            for (int i = 0; i < Enemies.Count; i++)
            {
                var enemy = Enemies[i];
                if (enemy.IsKnockedOut) continue;

                float sqrDist = (enemy.Position - Self.Position).sqrMagnitude;
                if (sqrDist < bestSqrDist)
                {
                    bestSqrDist = sqrDist;
                    best = enemy;
                }
            }
            return best;
        }

        /// <summary>Lowest-HP living enemy — the default focus-fire target absent squad coordination.</summary>
        public AIUnitSnapshot? LowestHpEnemy()
        {
            AIUnitSnapshot? best = null;
            float bestHp = float.MaxValue;
            for (int i = 0; i < Enemies.Count; i++)
            {
                var enemy = Enemies[i];
                if (enemy.IsKnockedOut) continue;

                if (enemy.HpFraction < bestHp)
                {
                    bestHp = enemy.HpFraction;
                    best = enemy;
                }
            }
            return best;
        }

        /// <summary>Count of living enemies within radius of a point — the simplified stand-in for
        /// "clustered enemies" the Serve archetype's Super condition needs (Plan 01 §3).</summary>
        public int CountEnemiesNear(Vector3 point, float radius)
        {
            int count = 0;
            float sqrRadius = radius * radius;
            for (int i = 0; i < Enemies.Count; i++)
            {
                var enemy = Enemies[i];
                if (enemy.IsKnockedOut) continue;
                if ((enemy.Position - point).sqrMagnitude <= sqrRadius) count++;
            }
            return count;
        }

        /// <summary>Living allies (excluding Self) below the given HP fraction — used by Spike's ally-shield Super condition.</summary>
        public int CountAlliesBelowHp(float hpFraction)
        {
            int count = 0;
            for (int i = 0; i < Allies.Count; i++)
            {
                var ally = Allies[i];
                if (ally.IsKnockedOut) continue;
                if (ally.HpFraction < hpFraction) count++;
            }
            return count;
        }
    }
}
