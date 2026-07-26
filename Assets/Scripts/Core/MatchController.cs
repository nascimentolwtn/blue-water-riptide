using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueWaterRiptide.Core
{
    /// <summary>
    /// Setup -> RoundCountdown -> Combat (with a SuddenDeath sub-state) -> RoundEnd -> (next
    /// round's RoundCountdown | MatchEnd) -> Results, per Plan 00 §7.1. Pure C#, no UnityEngine
    /// references — Single Player runs it locally, LAN will run it host-authoritative.
    /// Callers must call Tick(deltaTime) once per frame/tick to advance countdowns/timers; with
    /// MatchRules.M1Defaults (CountdownDuration/RoundTimeLimit both 0) every state resolves
    /// synchronously and Tick is a no-op, which is how the M1 prototype's single-round,
    /// no-timer scope stays intact without needing to call it at all.
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

        readonly Dictionary<Team, int> _roundWins;
        readonly HashSet<ParticipantId> _knockedOut = new HashSet<ParticipantId>();

        float _roundResetRemaining;
        float _suddenDeathRingRemaining;
        int _suddenDeathRingIndex;

        public event Action<ParticipantId> OnKnockout;
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

        /// <summary>Advances countdowns/timers. Safe to call every frame regardless of State.</summary>
        public void Tick(float deltaTime)
        {
            switch (State)
            {
                case MatchState.RoundCountdown:
                    CountdownRemaining -= deltaTime;
                    if (CountdownRemaining <= 0f) BeginCombat();
                    break;

                case MatchState.Combat:
                    TickCombat(deltaTime);
                    break;

                case MatchState.RoundEnd:
                    _roundResetRemaining -= deltaTime;
                    if (_roundResetRemaining <= 0f) BeginRound();
                    break;
            }
        }

        public void ReportKnockout(ParticipantId participant)
        {
            if (State != MatchState.Combat) return;
            if (!_knockedOut.Add(participant)) return;

            OnKnockout?.Invoke(participant);
            CheckSurvivors();
        }

        void BeginRound()
        {
            _knockedOut.Clear();
            InSuddenDeath = false;
            _suddenDeathRingIndex = 0;

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

            EndRound(aliveA > 0 ? Team.A : Team.B);
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
