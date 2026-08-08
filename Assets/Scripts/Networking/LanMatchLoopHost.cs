using UnityEngine;
using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Core;
using BlueWaterRiptide.Gameplay.Court;
using BlueWaterRiptide.Networking.Runtime;

namespace BlueWaterRiptide.Networking
{
    /// <summary>
    /// Host-only analog of Core/MatchLoopHost (Plan 08 step 6) for the LAN path: ticks
    /// MatchController and reacts to round-lifecycle events exactly like the Single Player version
    /// (round reset, Rising Tide during Sudden Death), but additionally mirrors round/pawn state
    /// into NetworkMatchState/NetworkSailorState every frame so clients can render the match without
    /// ever running their own MatchController (Plan 02 §1: host-authoritative). Lives in Networking
    /// rather than reusing Core.MatchLoopHost because pushing into NetworkVariables needs NGO types,
    /// and Core/Gameplay must stay transport-agnostic (CLAUDE.md architecture section) — this
    /// duplicates MatchLoopHost's small tick/reset logic rather than adding a Netcode reference there.
    /// </summary>
    public class LanMatchLoopHost : MonoBehaviour
    {
        MatchController _matchController;
        SailorPawn _pawnA;
        SailorPawn _pawnB;
        NetworkSailorState _stateA;
        NetworkSailorState _stateB;
        NetworkMatchState _networkMatchState;
        ArenaDefinition _arena;
        MatchRules _rules;

        RisingTideHazard _hazard;

        // MatchController doesn't expose per-team round-win counts publicly (only the transitions
        // that imply them) — mirrored locally here from the same OnRoundEnd winner MatchController
        // itself uses internally, rather than widening MatchController's public surface for one caller.
        int _roundWinsA;
        int _roundWinsB;

        public void Initialize(MatchController matchController, SailorPawn pawnA, SailorPawn pawnB,
            NetworkSailorState stateA, NetworkSailorState stateB, NetworkMatchState networkMatchState,
            ArenaDefinition arena, MatchRules rules)
        {
            _matchController = matchController;
            _pawnA = pawnA;
            _pawnB = pawnB;
            _stateA = stateA;
            _stateB = stateB;
            _networkMatchState = networkMatchState;
            _arena = arena;
            _rules = rules;

            _pawnA.SpawnImmunityDuration = arena.SpawnImmunityDuration;
            _pawnB.SpawnImmunityDuration = arena.SpawnImmunityDuration;

            matchController.OnRoundCountdownStart += HandleRoundCountdownStart;
            matchController.OnCombatStart += HandleCombatStart;
            matchController.OnSuddenDeathStart += HandleSuddenDeathStart;
            matchController.OnRoundEnd += HandleRoundEnd;
            matchController.OnMatchEnd += HandleMatchEnd;
        }

        void HandleRoundCountdownStart(int roundNumber)
        {
            float superRetain = roundNumber <= 1 ? 0f : 0.5f;

            _pawnA.ResetForRound(_arena.SpawnPositionsTeamA[0], superRetain);
            _pawnB.ResetForRound(_arena.SpawnPositionsTeamB[0], superRetain);
            _hazard = null;

            _networkMatchState.SetRoundState(roundNumber, _roundWinsA, _roundWinsB, _rules.RoundCountdownDuration, MatchPhase.RoundCountdown);
        }

        void HandleCombatStart()
        {
            _networkMatchState.SetRoundState(_matchController.CurrentRound, _roundWinsA, _roundWinsB, _matchController.RoundTimeRemaining, MatchPhase.Combat);
        }

        void HandleSuddenDeathStart()
        {
            _hazard = new RisingTideHazard(_arena.HalfExtents, _rules.SuddenDeathRingInterval, _arena.SuddenDeathRingShrinkStep, _arena.SuddenDeathMinHalfExtent);
            Debug.Log("BWR_LAN_SUDDEN_DEATH: Rising Tide active, safe zone will now shrink.");

            _networkMatchState.SetRoundState(_matchController.CurrentRound, _roundWinsA, _roundWinsB, 0f, MatchPhase.SuddenDeath);
        }

        void HandleRoundEnd(Team? winner)
        {
            if (winner == Team.A) _roundWinsA++;
            else if (winner == Team.B) _roundWinsB++;

            _networkMatchState.SetRoundState(_matchController.CurrentRound, _roundWinsA, _roundWinsB, _rules.RoundEndDuration, MatchPhase.RoundEnd);
        }

        void HandleMatchEnd(Team winner)
        {
            _networkMatchState.SetRoundState(_matchController.CurrentRound, _roundWinsA, _roundWinsB, 0f, MatchPhase.MatchEnd);
        }

        void Update()
        {
            if (_matchController == null) return;

            _matchController.Tick(new UnityMatchClock());

            MirrorPawnState(_pawnA, _stateA);
            MirrorPawnState(_pawnB, _stateB);

            bool suddenDeathActive = _matchController.State == MatchState.Combat
                && _matchController.CurrentCombatPhase == CombatPhase.SuddenDeath
                && _hazard != null;

            if (!suddenDeathActive) return;

            _hazard.Tick(_matchController.SuddenDeathElapsed);
            int ringIndex = _rules.SuddenDeathRingInterval > 0f ? Mathf.FloorToInt(_matchController.SuddenDeathElapsed / _rules.SuddenDeathRingInterval) : 0;
            _networkMatchState.SetTideRingIndex(ringIndex);

            ApplyRisingTideDamage(_pawnA);
            ApplyRisingTideDamage(_pawnB);
        }

        static void MirrorPawnState(SailorPawn pawn, NetworkSailorState state)
        {
            if (pawn == null || state == null) return;

            state.SetHp(pawn.CurrentHP);
            state.SetAmmo(pawn.CurrentAmmo);
            state.SetSuperCharge(pawn.SuperCharge01);
            state.SetKnockedOut(pawn.IsKnockedOut);
        }

        void ApplyRisingTideDamage(SailorPawn pawn)
        {
            if (pawn == null || pawn.IsKnockedOut) return;
            if (_hazard.IsPositionSafe(pawn.transform.position)) return;

            pawn.ApplyDamage(_rules.SuddenDeathDamagePerSecond * Time.deltaTime, pawn.Id);
        }
    }
}
