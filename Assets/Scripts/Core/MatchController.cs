using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueWaterRiptide.Core
{
    /// <summary>
    /// M1-scoped state machine: Setup -> Combat -> RoundEnd -> MatchEnd.
    /// RoundCountdown and SuddenDeath are not implemented yet (Plan 01 M1 explicitly skips rounds/timer).
    /// Pure C#, no UnityEngine references — Single Player runs it locally, LAN will run it host-authoritative.
    /// </summary>
    public enum MatchState { Setup, Combat, RoundEnd, MatchEnd }

    public sealed class MatchController
    {
        public MatchState State { get; private set; } = MatchState.Setup;
        public Session Session { get; }
        public MatchRules Rules { get; }

        readonly Dictionary<Team, int> _roundWins;
        readonly HashSet<ParticipantId> _knockedOut = new HashSet<ParticipantId>();

        public event Action<ParticipantId> OnKnockout;
        public event Action<Team> OnRoundEnd;
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
            State = MatchState.Combat;
        }

        public void ReportKnockout(ParticipantId participant)
        {
            if (State != MatchState.Combat) return;
            if (!_knockedOut.Add(participant)) return;

            OnKnockout?.Invoke(participant);
            CheckRoundEnd();
        }

        void CheckRoundEnd()
        {
            bool teamAAlive = Session.Participants.Any(p => p.Team == Team.A && !_knockedOut.Contains(p.Id));
            bool teamBAlive = Session.Participants.Any(p => p.Team == Team.B && !_knockedOut.Contains(p.Id));

            if (teamAAlive && teamBAlive) return;

            Team winner = teamAAlive ? Team.A : Team.B;
            _roundWins[winner]++;
            State = MatchState.RoundEnd;
            OnRoundEnd?.Invoke(winner);

            if (_roundWins[winner] >= Rules.RoundsToWin)
            {
                State = MatchState.MatchEnd;
                OnMatchEnd?.Invoke(winner);
            }
        }
    }
}
