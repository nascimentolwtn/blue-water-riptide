using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.AI
{
    /// <summary>
    /// Real utility-state AI driver (Plan 01 §3) — replaces DummyAIInputDriver's "stand and
    /// shoot" for anything past the M1 slice. Re-scores states at ~10Hz with a minimum dwell time
    /// so it doesn't flip-flop, and only emits InputCommand — like every other driver, it never
    /// calls gameplay APIs directly.
    /// </summary>
    public sealed class AIInputDriver : IInputDriver
    {
        const double DecisionInterval = 0.1;
        const double MinStateDwellSeconds = 0.7;

        readonly SailorPawn _self;
        readonly AIPerception _perception;
        readonly AIBehaviorProfile _profile;
        readonly AIDifficultyParams _difficulty;
        readonly IAIState[] _states;

        IAIState _currentState;
        double _lastDecisionTime = double.NegativeInfinity;
        double _stateEnteredTime;

        ParticipantId? _lastTargetId;
        double _targetAcquiredTime;

        public AIInputDriver(SailorPawn self, AIPerception perception, AIBehaviorProfile profile, AIDifficultyParams difficulty)
        {
            _self = self;
            _perception = perception;
            _profile = profile;
            _difficulty = difficulty;
            _states = new IAIState[] { new EngageState(), new RetreatState(), new RegroupState() };
            _currentState = _states[0];
        }

        public InputCommand Sample(double time)
        {
            if (_self == null || _self.IsKnockedOut) return InputCommand.None(time);

            var ctx = new AIContext(_self, _perception, _profile, _difficulty);

            if (time - _lastDecisionTime >= DecisionInterval)
            {
                _lastDecisionTime = time;
                ReconsiderState(ctx, time);
            }

            TrackTargetAcquisition(ctx, time);

            var cmd = _currentState.Produce(ctx, time);

            // Reaction delay only gates attack actions, not movement — the AI still visibly
            // repositions the instant a target appears, it just doesn't fire/Super on it until
            // the difficulty's reaction delay has elapsed since that target was first noticed.
            bool reactionElapsed = time - _targetAcquiredTime >= _difficulty.ReactionDelaySeconds;
            if (!reactionElapsed && (cmd.FireHeld || cmd.SuperPressed))
            {
                cmd = new InputCommand(cmd.Move, cmd.Aim, false, false, false, time);
            }

            return cmd;
        }

        void ReconsiderState(in AIContext ctx, double time)
        {
            if (time - _stateEnteredTime < MinStateDwellSeconds) return;

            IAIState best = _currentState;
            float bestScore = _currentState.Score(ctx);

            foreach (var state in _states)
            {
                float score = state.Score(ctx);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = state;
                }
            }

            if (!ReferenceEquals(best, _currentState))
            {
                _currentState = best;
                _stateEnteredTime = time;
            }
        }

        void TrackTargetAcquisition(in AIContext ctx, double time)
        {
            var nearest = ctx.Perception.GetNearestEnemy();
            ParticipantId? currentTargetId = nearest?.Pawn.Id;

            bool isNewAcquisition = currentTargetId.HasValue && currentTargetId != _lastTargetId;
            bool targetLost = !currentTargetId.HasValue && _lastTargetId.HasValue;

            if (isNewAcquisition || targetLost)
            {
                _targetAcquiredTime = time;
            }

            _lastTargetId = currentTargetId;
        }
    }
}
