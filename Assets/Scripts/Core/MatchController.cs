using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueWaterRiptide.Core
{
    /// <summary>
    /// M1/Sink-or-Swim state machine: Setup -> RoundCountdown -> Combat -> RoundEnd -> (next round | MatchEnd).
    /// Combat has a sub-state (<see cref="CombatPhase"/>) for Normal vs SuddenDeath — kept as a sub-state
    /// under Combat rather than a new top-level MatchState, per Plan 00 §7's explicit wording.
    /// Pure C#, no UnityEngine references — Single Player runs it locally, LAN will run it host-authoritative.
    /// </summary>
    public enum MatchState { Setup, RoundCountdown, Combat, RoundEnd, MatchEnd }

    /// <summary>Sub-state of MatchState.Combat.</summary>
    public enum CombatPhase { Normal, SuddenDeath }

    public sealed class MatchController
    {
        public MatchState State { get; private set; } = MatchState.Setup;
        public Session Session { get; }
        public MatchRules Rules { get; }

        /// <summary>1-based index of the round currently running (or about to run, during RoundCountdown).</summary>
        public int CurrentRound { get; private set; } = 0;

        /// <summary>Normal vs SuddenDeath sub-state. Only meaningful while State == Combat.</summary>
        public CombatPhase CurrentCombatPhase => _currentCombatPhase;

        /// <summary>Elapsed seconds since SuddenDeath began. 0 while not in SuddenDeath.</summary>
        public float SuddenDeathElapsed => _suddenDeathActive ? _suddenDeathTimer : 0f;

        readonly Dictionary<Team, int> _roundWins;
        readonly HashSet<ParticipantId> _knockedOut = new HashSet<ParticipantId>();

        // Timing state.
        float _roundTimer;
        bool _suddenDeathActive;
        float _suddenDeathTimer;
        CombatPhase _currentCombatPhase = CombatPhase.Normal;

        public event Action<ParticipantId> OnKnockout;
        public event Action<Team?> OnRoundEnd;
        public event Action<Team> OnMatchEnd;
        public event Action<int> OnRoundCountdownStart;
        public event Action OnCombatStart;
        public event Action OnSuddenDeathStart;

        public MatchController(Session session, MatchRules rules)
        {
            Session = session;
            Rules = rules;
            _roundWins = new Dictionary<Team, int> { { Team.A, 0 }, { Team.B, 0 } };
        }

        public void StartMatch()
        {
            if (State != MatchState.Setup) return;
            StartNewRound();
        }

        /// <summary>
        /// Advances all active timers by clock.DeltaTime and drives state transitions that are
        /// time-based rather than event-based (countdown expiry, round-end dwell, round timeout).
        /// Safe to call every frame regardless of current state.
        /// </summary>
        public void Tick(IMatchClock clock)
        {
            float dt = clock.DeltaTime;

            switch (State)
            {
                case MatchState.RoundCountdown:
                    _roundTimer -= dt;
                    if (_roundTimer <= 0f)
                    {
                        EnterCombat();
                    }
                    break;

                case MatchState.Combat:
                    if (_currentCombatPhase == CombatPhase.Normal)
                    {
                        _roundTimer -= dt;
                        if (_roundTimer <= 0f)
                        {
                            ResolveRoundTimeout();
                        }
                    }
                    else // SuddenDeath
                    {
                        _suddenDeathTimer += dt;
                    }
                    break;

                case MatchState.RoundEnd:
                    _roundTimer -= dt;
                    if (_roundTimer <= 0f)
                    {
                        AdvanceRound();
                    }
                    break;

                // Setup / MatchEnd: nothing to tick.
                default:
                    break;
            }
        }

        public void ReportKnockout(ParticipantId participant)
        {
            if (State != MatchState.Combat) return;
            if (!_knockedOut.Add(participant)) return;

            OnKnockout?.Invoke(participant);
            CheckElimination();
        }

        /// <summary>Elimination check triggered by a knockout: does one team have zero survivors?</summary>
        void CheckElimination()
        {
            // Guard: a knockout landing the same tick as round-timer expiry could re-enter after
            // Tick() already moved state out of Combat. KO takes precedence, but only while still in Combat.
            if (State != MatchState.Combat) return;

            bool teamAAlive = Session.Participants.Any(p => p.Team == Team.A && !_knockedOut.Contains(p.Id));
            bool teamBAlive = Session.Participants.Any(p => p.Team == Team.B && !_knockedOut.Contains(p.Id));

            if (teamAAlive && teamBAlive) return;

            if (!teamAAlive && !teamBAlive)
            {
                // Simultaneous double-knockout: no survivors on either side, no winner this round.
                EnterRoundEnd(null);
                return;
            }

            Team winner = teamAAlive ? Team.A : Team.B;
            EnterRoundEnd(winner);
        }

        /// <summary>Round timer expired while still Combat/Normal: decide by survivor count or escalate to SuddenDeath.</summary>
        void ResolveRoundTimeout()
        {
            if (State != MatchState.Combat || _currentCombatPhase != CombatPhase.Normal) return;

            int aliveA = Session.Participants.Count(p => p.Team == Team.A && !_knockedOut.Contains(p.Id));
            int aliveB = Session.Participants.Count(p => p.Team == Team.B && !_knockedOut.Contains(p.Id));

            if (aliveA == aliveB)
            {
                EnterSuddenDeath();
                return;
            }

            Team winner = aliveA > aliveB ? Team.A : Team.B;
            EnterRoundEnd(winner);
        }

        void EnterCombat()
        {
            if (State != MatchState.RoundCountdown) return;

            State = MatchState.Combat;
            _currentCombatPhase = CombatPhase.Normal;
            _roundTimer = Rules.RoundTimeLimit;
            _suddenDeathActive = false;
            _suddenDeathTimer = 0f;

            OnCombatStart?.Invoke();
        }

        void EnterSuddenDeath()
        {
            if (State != MatchState.Combat || _currentCombatPhase != CombatPhase.Normal) return;

            _currentCombatPhase = CombatPhase.SuddenDeath;
            _suddenDeathActive = true;
            _suddenDeathTimer = 0f;

            OnSuddenDeathStart?.Invoke();
        }

        void EnterRoundEnd(Team? winner)
        {
            if (State != MatchState.Combat) return;

            if (winner.HasValue)
            {
                _roundWins[winner.Value]++;
            }

            State = MatchState.RoundEnd;
            _roundTimer = Rules.RoundEndDuration;

            OnRoundEnd?.Invoke(winner);
        }

        /// <summary>
        /// Shared advance-to-next-round-or-end-match logic. Called when RoundEnd's dwell timer
        /// expires (via Tick). Decides between looping back to RoundCountdown and terminal MatchEnd.
        /// </summary>
        public void AdvanceRound()
        {
            if (State != MatchState.RoundEnd) return;

            Team? matchWinner = null;
            if (_roundWins[Team.A] >= Rules.RoundsToWin) matchWinner = Team.A;
            else if (_roundWins[Team.B] >= Rules.RoundsToWin) matchWinner = Team.B;

            if (matchWinner.HasValue)
            {
                State = MatchState.MatchEnd;
                OnMatchEnd?.Invoke(matchWinner.Value);
                return;
            }

            StartNewRound();
        }

        /// <summary>
        /// Resets per-round state and enters RoundCountdown for a fresh round. Called both from
        /// StartMatch (round 1) and AdvanceRound (round 2+).
        /// </summary>
        void StartNewRound()
        {
            _knockedOut.Clear();
            CurrentRound++;

            State = MatchState.RoundCountdown;
            _currentCombatPhase = CombatPhase.Normal;
            _roundTimer = Rules.RoundCountdownDuration;
            _suddenDeathActive = false;
            _suddenDeathTimer = 0f;

            OnRoundCountdownStart?.Invoke(CurrentRound);
        }
    }
}
