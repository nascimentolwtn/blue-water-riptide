using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueWaterRiptide.Core
{
    /// <summary>
    /// Setup -> RoundCountdown -> Combat (with a SuddenDeath sub-state) -> RoundEnd -> (next
    /// round's RoundCountdown | MatchEnd) -> Results, per Plan 00 §7.1. Pure C#, no UnityEngine
    /// references — Single Player runs it locally, LAN will run it host-authoritative.
    /// Callers must call Tick(deltaTime) once per frame/tick. Besides advancing
    /// countdowns/timers, Tick also resolves any round-ending knockout(s) reported via
    /// ReportKnockout since the previous Tick call — resolution is deliberately deferred rather
    /// than handled inline, so multiple knockouts landing in the same frame (e.g. one AOE hit
    /// wiping both teams' last member at once) are evaluated together instead of by whichever
    /// order ReportKnockout happened to be called in.
    /// </summary>
    public enum MatchState { Setup, RoundCountdown, Combat, RoundEnd, MatchEnd, Results }

    public sealed class MatchController
    {
        public MatchState State { get; private set; } = MatchState.Setup;
        public Session Session { get; }
        public MatchRules Rules { get; }

        public int RoundNumber { get; private set; } = 1;
        public bool InSuddenDeath { get; private set; }
        public float CountdownRemaining { get; private set; }
        public float RoundTimeRemaining { get; private set; }

        /// <summary>Knockouts credited per attacker, accumulated for the whole match (not reset
        /// between rounds) — feeds per-Sailor lifetime knockout stats at Results.</summary>
        public IReadOnlyDictionary<ParticipantId, int> KnockoutsByAttacker => _knockoutsByAttacker;

        readonly Dictionary<Team, int> _roundWins;
        readonly HashSet<ParticipantId> _knockedOut = new HashSet<ParticipantId>();
        readonly Dictionary<ParticipantId, int> _knockoutsByAttacker = new Dictionary<ParticipantId, int>();

        float _roundResetRemaining;
        float _suddenDeathRingRemaining;
        int _suddenDeathRingIndex;
        bool _survivorCheckPending;
        ParticipantId? _lastKnockoutAttacker;

        public event Action<ParticipantId> OnKnockout;
        /// <summary>Fired at the start of every round (including the first) before the countdown
        /// begins — subscribers restore each pawn's HP/ammo/collider and spawn position here.</summary>
        public event Action<int> OnRoundReset;
        public event Action<int> OnRoundStart;
        public event Action<Team> OnRoundEnd;
        public event Action OnSuddenDeathStart;
        public event Action<int> OnSuddenDeathRingAdvance;
        public event Action<Team> OnMatchEnd;

        public MatchController(Session session, MatchRules rules)
        {
            Session = session;
            Rules = rules;
            _roundWins = new Dictionary<Team, int> { { Team.A, 0 }, { Team.B, 0 } };
        }

        public void StartMatch()
        {
            if (State != MatchState.Setup) return;
            BeginRound();
        }

        /// <summary>Advances countdowns/timers and resolves any pending round-ending knockout.
        /// Safe to call every frame regardless of State.</summary>
        public void Tick(float deltaTime)
        {
            switch (State)
            {
                case MatchState.RoundCountdown:
                    CountdownRemaining -= deltaTime;
                    if (CountdownRemaining <= 0f) BeginCombat();
                    break;

                case MatchState.Combat:
                    if (_survivorCheckPending)
                    {
                        _survivorCheckPending = false;
                        CheckSurvivors();
                        if (State != MatchState.Combat) break;
                    }
                    TickCombat(deltaTime);
                    break;

                case MatchState.RoundEnd:
                    _roundResetRemaining -= deltaTime;
                    if (_roundResetRemaining <= 0f) BeginRound();
                    break;
            }
        }

        /// <summary>Reports that <paramref name="victim"/> was knocked out by <paramref name="attacker"/>.
        /// Whether this ends the round is resolved on the next Tick(), not inline — see the class
        /// doc comment.</summary>
        public void ReportKnockout(ParticipantId victim, ParticipantId attacker)
        {
            if (State != MatchState.Combat) return;
            if (!_knockedOut.Add(victim)) return;

            _knockoutsByAttacker[attacker] = _knockoutsByAttacker.TryGetValue(attacker, out int count) ? count + 1 : 1;
            _lastKnockoutAttacker = attacker;
            _survivorCheckPending = true;

            OnKnockout?.Invoke(victim);
        }

        void BeginRound()
        {
            _knockedOut.Clear();
            InSuddenDeath = false;
            _suddenDeathRingIndex = 0;
            _survivorCheckPending = false;
            _lastKnockoutAttacker = null;

            OnRoundReset?.Invoke(RoundNumber);

            if (Rules.CountdownDuration > 0f)
            {
                State = MatchState.RoundCountdown;
                CountdownRemaining = Rules.CountdownDuration;
            }
            else
            {
                BeginCombat();
            }
        }

        void BeginCombat()
        {
            State = MatchState.Combat;
            RoundTimeRemaining = Rules.RoundTimeLimit;
            OnRoundStart?.Invoke(RoundNumber);
        }

        void TickCombat(float deltaTime)
        {
            if (InSuddenDeath)
            {
                _suddenDeathRingRemaining -= deltaTime;
                if (_suddenDeathRingRemaining <= 0f)
                {
                    _suddenDeathRingIndex++;
                    _suddenDeathRingRemaining = Rules.SuddenDeathRingInterval;
                    OnSuddenDeathRingAdvance?.Invoke(_suddenDeathRingIndex);
                }
                return;
            }

            if (Rules.RoundTimeLimit <= 0f) return;

            RoundTimeRemaining -= deltaTime;
            if (RoundTimeRemaining > 0f) return;

            RoundTimeRemaining = 0f;
            HandleRoundTimeout();
        }

        void HandleRoundTimeout()
        {
            int aliveA = CountAlive(Team.A);
            int aliveB = CountAlive(Team.B);

            if (aliveA == aliveB)
            {
                StartSuddenDeath();
                return;
            }

            EndRound(aliveA > aliveB ? Team.A : Team.B);
        }

        void StartSuddenDeath()
        {
            InSuddenDeath = true;
            _suddenDeathRingIndex = 0;
            _suddenDeathRingRemaining = Rules.SuddenDeathRingInterval;
            OnSuddenDeathStart?.Invoke();
        }

        void CheckSurvivors()
        {
            int aliveA = CountAlive(Team.A);
            int aliveB = CountAlive(Team.B);

            if (aliveA > 0 && aliveB > 0) return;

            if (aliveA == 0 && aliveB == 0 && _lastKnockoutAttacker.HasValue)
            {
                // Simultaneous wipe (e.g. one AOE hit knocking out both teams' last member in the
                // same batch): award the round to whichever team scored that final knockout,
                // rather than the arbitrary "Team A alive? no -> Team B" fallback below.
                EndRound(FindTeam(_lastKnockoutAttacker.Value));
                return;
            }

            EndRound(aliveA > 0 ? Team.A : Team.B);
        }

        Team FindTeam(ParticipantId id)
        {
            foreach (var p in Session.Participants)
            {
                if (p.Id == id) return p.Team;
            }
            return Team.B; // Unreachable with valid input — Session.Participants is the only source of ids.
        }

        int CountAlive(Team team) =>
            Session.Participants.Count(p => p.Team == team && !_knockedOut.Contains(p.Id));

        void EndRound(Team winner)
        {
            _roundWins[winner]++;
            State = MatchState.RoundEnd;
            OnRoundEnd?.Invoke(winner);

            if (_roundWins[winner] >= Rules.RoundsToWin)
            {
                State = MatchState.MatchEnd;
                OnMatchEnd?.Invoke(winner);
                State = MatchState.Results;
                return;
            }

            RoundNumber++;
            // Deliberately never recurses into BeginRound() synchronously here, even when
            // RoundResetDelay <= 0 — Tick()'s RoundEnd branch picks it up on the next call
            // instead. Recursing in-line would let a second knockout reported later in the
            // same call stack (e.g. from an AOE hit processed in a loop) land on the round
            // that had just silently started, misattributing it and corrupting round-win
            // bookkeeping.
            _roundResetRemaining = Rules.RoundResetDelay;
        }
    }
}
