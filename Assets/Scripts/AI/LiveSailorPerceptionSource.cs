using System.Collections.Generic;
using UnityEngine;
using BlueWaterRiptide.Core;
using BlueWaterRiptide.Characters;

namespace BlueWaterRiptide.AI
{
    /// <summary>
    /// Integration adapter AIPerception's own doc comment calls "out of scope here": builds a fresh
    /// AIPerception snapshot from the live 1v1 SailorPawns each call. No teammates exist in this
    /// matchup, so Allies is always empty — a future team mode would need a multi-pawn variant of this.
    /// </summary>
    public sealed class LiveSailorPerceptionSource
    {
        readonly SailorPawn _self;
        readonly SailorPawn _enemy;
        readonly MatchController _matchController;
        readonly ArenaDefinition _arena;

        static readonly IReadOnlyList<AIUnitSnapshot> NoAllies = new AIUnitSnapshot[0];

        public LiveSailorPerceptionSource(SailorPawn self, SailorPawn enemy, MatchController matchController, ArenaDefinition arena)
        {
            _self = self;
            _enemy = enemy;
            _matchController = matchController;
            _arena = arena;
        }

        public AIPerception BuildSnapshot()
        {
            var selfSnapshot = ToSnapshot(_self);
            var enemies = new[] { ToSnapshot(_enemy) };

            return new AIPerception(
                selfSnapshot,
                NoAllies,
                enemies,
                _arena.HalfExtents,
                _matchController.RoundTimeRemaining,
                isSuddenDeath: _matchController.CurrentCombatPhase == CombatPhase.SuddenDeath,
                isConcealed: false);
        }

        static AIUnitSnapshot ToSnapshot(SailorPawn pawn)
        {
            return new AIUnitSnapshot(
                pawn.Id,
                pawn.Team,
                pawn.Definition,
                pawn.transform.position,
                pawn.HpFraction,
                pawn.CurrentAmmo,
                pawn.IsKnockedOut,
                pawn.SuperCharge01);
        }
    }
}
