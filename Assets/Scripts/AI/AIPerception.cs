using System;
using System.Collections.Generic;
using UnityEngine;
using BlueWaterRiptide.Characters;

namespace BlueWaterRiptide.AI
{
    public readonly struct PerceivedPawn
    {
        public readonly SailorPawn Pawn;
        public readonly float Distance;

        public PerceivedPawn(SailorPawn pawn, float distance)
        {
            Pawn = pawn;
            Distance = distance;
        }
    }

    /// <summary>
    /// Read-only world snapshot for one AI-controlled pawn, rebuilt on every query — AI never
    /// queries gameplay state directly (Plan 01 §3), everything funnels through here so a future
    /// concealment/fog system has exactly one seam to enforce "AI can't see through grass, no
    /// cheating" (not implemented yet — no grass/concealment exists until the real arena does,
    /// item 4; GetVisibleEnemies is where that filter goes once it can).
    /// </summary>
    public sealed class AIPerception
    {
        readonly SailorPawn _self;
        readonly IReadOnlyList<SailorPawn> _allPawns;

        public AIPerception(SailorPawn self, IReadOnlyList<SailorPawn> allPawns)
        {
            _self = self;
            _allPawns = allPawns;
        }

        public SailorPawn Self => _self;

        public List<PerceivedPawn> GetVisibleEnemies() => Collect(p => p.Team != _self.Team);

        public List<PerceivedPawn> GetAllies() => Collect(p => p.Team == _self.Team);

        public PerceivedPawn? GetNearestEnemy() => Nearest(GetVisibleEnemies());

        public PerceivedPawn? GetNearestAlly() => Nearest(GetAllies());

        public PerceivedPawn? GetLowestHpVisibleEnemy()
        {
            PerceivedPawn? lowest = null;
            foreach (var enemy in GetVisibleEnemies())
            {
                if (lowest == null || enemy.Pawn.HpFraction < lowest.Value.Pawn.HpFraction) lowest = enemy;
            }
            return lowest;
        }

        /// <summary>Visible enemies within radius of a point — feeds "use Super on N+ clustered enemies."</summary>
        public int CountEnemiesWithin(Vector3 point, float radius)
        {
            int count = 0;
            foreach (var enemy in GetVisibleEnemies())
            {
                if (Vector3.Distance(point, enemy.Pawn.transform.position) <= radius) count++;
            }
            return count;
        }

        static PerceivedPawn? Nearest(List<PerceivedPawn> pawns)
        {
            PerceivedPawn? nearest = null;
            foreach (var p in pawns)
            {
                if (nearest == null || p.Distance < nearest.Value.Distance) nearest = p;
            }
            return nearest;
        }

        List<PerceivedPawn> Collect(Func<SailorPawn, bool> predicate)
        {
            var result = new List<PerceivedPawn>();
            if (_allPawns == null) return result;

            foreach (var pawn in _allPawns)
            {
                if (pawn == null || pawn.IsKnockedOut) continue;
                if (pawn.Id == _self.Id) continue;
                if (!predicate(pawn)) continue;

                float distance = Vector3.Distance(_self.transform.position, pawn.transform.position);
                result.Add(new PerceivedPawn(pawn, distance));
            }
            return result;
        }
    }
}
