using UnityEngine;
using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Gameplay.Court;

namespace BlueWaterRiptide.Core
{
    /// <summary>
    /// Thin glue (Plan 08 step 6) between the transport-agnostic MatchController and the M1
    /// prototype's two hardcoded pawns: ticks the match clock every frame and reacts to round-
    /// lifecycle events (reset pawns at RoundCountdown, spin up/apply the Rising Tide hazard during
    /// Sudden Death). No game logic of its own — matches the M1Hud/TouchHud pattern of a small
    /// MonoBehaviour wrapper the bootstrap adds and Initializes.
    /// </summary>
    public class MatchLoopHost : MonoBehaviour
    {
        MatchController _matchController;
        SailorPawn _pawnA;
        SailorPawn _pawnB;
        ArenaDefinition _arena;
        MatchRules _rules;

        RisingTideHazard _hazard;

        public void Initialize(MatchController matchController, SailorPawn pawnA, SailorPawn pawnB, ArenaDefinition arena, MatchRules rules)
        {
            _matchController = matchController;
            _pawnA = pawnA;
            _pawnB = pawnB;
            _arena = arena;
            _rules = rules;

            _pawnA.SpawnImmunityDuration = arena.SpawnImmunityDuration;
            _pawnB.SpawnImmunityDuration = arena.SpawnImmunityDuration;

            matchController.OnRoundCountdownStart += HandleRoundCountdownStart;
            matchController.OnSuddenDeathStart += HandleSuddenDeathStart;
        }

        void HandleRoundCountdownStart(int roundNumber)
        {
            // 00 §1's round-reset rule: no Super carry into round 1, 50% carry into round 2+.
            float superRetain = roundNumber <= 1 ? 0f : 0.5f;

            _pawnA.ResetForRound(_arena.SpawnPositionsTeamA[0], superRetain);
            _pawnB.ResetForRound(_arena.SpawnPositionsTeamB[0], superRetain);

            // Rising Tide is per-round; drop the previous round's shrunk hazard so the next Sudden
            // Death (if any) starts fresh at the arena's full half-extents.
            _hazard = null;
        }

        void HandleSuddenDeathStart()
        {
            _hazard = new RisingTideHazard(_arena.HalfExtents, _rules.SuddenDeathRingInterval, _arena.SuddenDeathRingShrinkStep, _arena.SuddenDeathMinHalfExtent);
            Debug.Log("BWR_SUDDEN_DEATH: Rising Tide active, safe zone will now shrink.");
        }

        void Update()
        {
            if (_matchController == null) return;

            _matchController.Tick(new UnityMatchClock());

            bool suddenDeathActive = _matchController.State == MatchState.Combat
                && _matchController.CurrentCombatPhase == CombatPhase.SuddenDeath
                && _hazard != null;

            if (!suddenDeathActive) return;

            _hazard.Tick(_matchController.SuddenDeathElapsed);
            ApplyRisingTideDamage(_pawnA);
            ApplyRisingTideDamage(_pawnB);
        }

        void ApplyRisingTideDamage(SailorPawn pawn)
        {
            if (pawn == null || pawn.IsKnockedOut) return;
            if (_hazard.IsPositionSafe(pawn.transform.position)) return;

            pawn.ApplyDamage(_rules.SuddenDeathDamagePerSecond * Time.deltaTime, pawn.Id);
        }
    }
}
