using System;
using System.Collections.Generic;
using UnityEngine;
using BlueWaterRiptide.Core;
using BlueWaterRiptide.Characters;

namespace BlueWaterRiptide.AI
{
    /// <summary>
    /// Real utility-AI opponent driver (Plan 01 §3), superseding DummyAIInputDriver once the
    /// integration layer wires it up. Never touches gameplay APIs directly — it only ever reads a
    /// caller-supplied AIPerception snapshot and emits InputCommands, exactly like a human driver
    /// would, so the simulation can't tell AI from human/remote input.
    ///
    /// Perception is pulled via a delegate rather than a stored reference: this keeps the class
    /// constructible and testable standalone (no SailorPawn/MonoBehaviour dependency), with the
    /// actual "build a fresh AIPerception from live SailorPawns" wiring left to a future integration
    /// pass (out of scope here per the task brief).
    /// </summary>
    public sealed class AIInputDriver : IInputDriver
    {
        /// <summary>Minimum seconds a utility state must hold before a higher-scoring state can replace it,
        /// preventing rapid state flip-flopping (Plan 01 §3).</summary>
        const float MinStateDwellSeconds = 0.7f;

        readonly Func<AIPerception> _perceptionProvider;
        readonly AIBehaviorProfile _profile;
        readonly AIDifficulty _difficulty;
        readonly AISquadBrain _squadBrain;
        readonly Lane _assignedLane;
        readonly float _strafeSign;

        readonly Dictionary<ParticipantId, (Vector3 pos, double time)> _targetTrackers = new Dictionary<ParticipantId, (Vector3, double)>();

        UtilityStateKind _currentState = UtilityStateKind.Engage;
        double _stateEnteredAt = double.NegativeInfinity;

        double _lastReactionTime = double.NegativeInfinity;
        InputCommand _cachedCommand;

        public AIInputDriver(Func<AIPerception> perceptionProvider, AIBehaviorProfile profile, AIDifficulty difficulty, AISquadBrain squadBrain = null, Lane assignedLane = Lane.Center)
        {
            _perceptionProvider = perceptionProvider ?? throw new ArgumentNullException(nameof(perceptionProvider));
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _difficulty = difficulty ?? throw new ArgumentNullException(nameof(difficulty));
            _squadBrain = squadBrain;
            _assignedLane = assignedLane;
            _strafeSign = UnityEngine.Random.value < 0.5f ? -1f : 1f;
        }

        public InputCommand Sample(double time)
        {
            AIPerception perception = _perceptionProvider();
            if (perception == null || perception.Self.Definition == null || perception.Self.IsKnockedOut)
            {
                return InputCommand.None(time);
            }

            // Reaction delay: keep re-issuing the last decision (with a fresh timestamp) until the
            // tier's reaction window elapses, then compute a new one.
            if (time - _lastReactionTime < _difficulty.ReactionDelaySeconds)
            {
                return RestampTimestamp(_cachedCommand, time);
            }
            _lastReactionTime = time;

            bool playSafe = false;
            if (_squadBrain != null && _difficulty.RespectsPlaySafe)
            {
                var teamMembers = new List<AIUnitSnapshot>(perception.Allies.Count + 1) { perception.Self };
                teamMembers.AddRange(perception.Allies);
                playSafe = _squadBrain.ShouldPlaySafe(teamMembers, perception.Enemies, perception.MatchTimeRemaining);
            }

            UtilityStateKind desiredState = UtilityStateScorer.SelectBestState(perception, _profile, playSafe);
            if (double.IsNegativeInfinity(_stateEnteredAt))
            {
                _currentState = desiredState;
                _stateEnteredAt = time;
            }
            else if (desiredState != _currentState && time - _stateEnteredAt >= MinStateDwellSeconds)
            {
                _currentState = desiredState;
                _stateEnteredAt = time;
            }

            AIUnitSnapshot? target = null;
            if (_difficulty.RespectsFocusFireSuggestion && _squadBrain != null)
            {
                target = _squadBrain.SuggestFocusFireTarget(perception.Enemies);
            }
            if (!target.HasValue)
            {
                target = perception.LowestHpEnemy();
            }

            InputCommand command = BuildCommand(perception, target, _currentState, time);
            _cachedCommand = command;
            return command;
        }

        static InputCommand RestampTimestamp(InputCommand source, double time)
        {
            return new InputCommand(source.Move, source.Aim, false, source.FireHeld, source.SuperPressed, time);
        }

        InputCommand BuildCommand(AIPerception perception, AIUnitSnapshot? targetOpt, UtilityStateKind state, double time)
        {
            var self = perception.Self;
            var definition = self.Definition;
            Vector3 selfPos = self.Position;

            Vector2 aim = Vector2.zero;
            if (targetOpt.HasValue)
            {
                Vector3 leadPoint = PredictTargetPosition(targetOpt.Value, time);
                Vector3 aimPoint = Vector3.Lerp(targetOpt.Value.Position, leadPoint, _difficulty.TargetLeadFraction);
                Vector3 aimDir = aimPoint - selfPos;
                aimDir.y = 0f;
                if (aimDir.sqrMagnitude > 0.0001f)
                {
                    aimDir = ApplyAimError(aimDir.normalized);
                    aim = new Vector2(aimDir.x, aimDir.z);
                }
            }

            Vector2 move;
            switch (state)
            {
                case UtilityStateKind.Retreat:
                    move = ComputeRetreatMove(perception, targetOpt);
                    break;
                case UtilityStateKind.Regroup:
                    move = ComputeRegroupMove(perception);
                    break;
                case UtilityStateKind.Flank:
                    move = ComputeFlankMove(perception, targetOpt);
                    break;
                case UtilityStateKind.HoldChoke:
                    move = ComputeHoldChokeMove(perception);
                    break;
                case UtilityStateKind.SuddenDeathPush:
                    move = ComputeSuddenDeathPushMove(perception, targetOpt);
                    break;
                case UtilityStateKind.Engage:
                default:
                    move = ComputeEngageMove(perception, targetOpt, definition);
                    break;
            }

            bool fireHeld = false;
            if (targetOpt.HasValue && state != UtilityStateKind.Retreat && state != UtilityStateKind.Regroup)
            {
                float dist = Vector3.Distance(selfPos, targetOpt.Value.Position);

                if (_profile.Archetype == AttackArchetype.Serve)
                {
                    bool hasAmmo = self.CurrentAmmo >= _profile.MinAmmoToFire;
                    bool inRange = definition != null && dist <= definition.AttackRange;
                    fireHeld = hasAmmo && inRange;
                }
                else // Spike
                {
                    float commitRange = definition != null ? definition.AttackRange * _profile.CommitRangeMultiplier : 0f;
                    fireHeld = self.CurrentAmmo >= _profile.MinAmmoToFire && dist <= commitRange;
                }
            }

            bool superPressed = ShouldUseSuper(perception, targetOpt, state);

            return new InputCommand(move, aim, false, fireHeld, superPressed, time);
        }

        Vector3 PredictTargetPosition(AIUnitSnapshot target, double time)
        {
            if (_targetTrackers.TryGetValue(target.Id, out var prev))
            {
                double dt = time - prev.time;
                _targetTrackers[target.Id] = (target.Position, time);

                if (dt > 0.0001 && dt < 2.0)
                {
                    Vector3 velocity = (target.Position - prev.pos) / (float)dt;
                    float leadTime = _difficulty.ReactionDelaySeconds + 0.15f;
                    return target.Position + velocity * leadTime;
                }

                return target.Position;
            }

            _targetTrackers[target.Id] = (target.Position, time);
            return target.Position;
        }

        Vector3 ApplyAimError(Vector3 dir)
        {
            if (_difficulty.AimErrorDegrees <= 0f) return dir;
            float errorDeg = UnityEngine.Random.Range(-_difficulty.AimErrorDegrees, _difficulty.AimErrorDegrees);
            return Quaternion.AngleAxis(errorDeg, Vector3.up) * dir;
        }

        bool ShouldUseSuper(AIPerception perception, AIUnitSnapshot? targetOpt, UtilityStateKind state)
        {
            var self = perception.Self;
            if (self.SuperCharge01 < 1f || self.Definition == null) return false;

            bool conditionMet;
            if (_profile.Archetype == AttackArchetype.Serve)
            {
                int clustered = targetOpt.HasValue ? perception.CountEnemiesNear(targetOpt.Value.Position, _profile.SuperTriggerRadius) : 0;
                conditionMet = clustered >= _profile.SuperTriggerEnemyCount;
            }
            else // Spike
            {
                bool closingOnFleeingLowHpTarget = targetOpt.HasValue
                    && targetOpt.Value.HpFraction < 0.3f
                    && Vector3.Distance(perception.Self.Position, targetOpt.Value.Position) <= _profile.SuperTriggerRadius;
                bool allyNeedsShield = perception.CountAlliesBelowHp(_profile.AllyShieldHpThreshold) >= 1;
                conditionMet = closingOnFleeingLowHpTarget || allyNeedsShield;
            }

            if (!conditionMet) return false;

            // Deckhand (easy) holds its Super until there's nothing more pressing to do, approximated
            // as "currently holding a choke" since that's the closest analog to idle among these states.
            if (_difficulty.DelaySuperUntilIdle)
            {
                return state == UtilityStateKind.HoldChoke;
            }

            return true;
        }

        Vector2 ComputeEngageMove(AIPerception perception, AIUnitSnapshot? targetOpt, SailorDefinitionData definition)
        {
            if (!targetOpt.HasValue)
            {
                return MoveTowardLaneWaypoint(perception);
            }

            Vector3 selfPos = perception.Self.Position;
            Vector3 toTarget = targetOpt.Value.Position - selfPos;
            toTarget.y = 0f;
            float dist = toTarget.magnitude;
            Vector3 dirToTarget = dist > 0.0001f ? toTarget / dist : Vector3.forward;

            float preferredRange = definition != null ? _profile.PreferredRangeFraction * definition.AttackRange : dist;
            float rangeError = dist - preferredRange;

            Vector3 radial = Vector3.zero;
            if (Mathf.Abs(rangeError) > 0.5f)
            {
                radial = dirToTarget * Mathf.Sign(rangeError);
            }

            Vector3 perp = new Vector3(-dirToTarget.z, 0f, dirToTarget.x);
            Vector3 combined = radial + perp * _profile.StrafeSpeedFraction * _strafeSign;
            if (combined.sqrMagnitude > 1f) combined.Normalize();

            return new Vector2(combined.x, combined.z);
        }

        Vector2 ComputeRetreatMove(AIPerception perception, AIUnitSnapshot? targetOpt)
        {
            Vector3 selfPos = perception.Self.Position;
            Vector3 awayDir = Vector3.zero;

            if (targetOpt.HasValue)
            {
                Vector3 delta = selfPos - targetOpt.Value.Position;
                delta.y = 0f;
                if (delta.sqrMagnitude > 0.0001f) awayDir = delta.normalized;
            }

            Vector3 towardAlly = Vector3.zero;
            var nearestAlly = NearestAlly(perception);
            if (nearestAlly.HasValue)
            {
                Vector3 d = nearestAlly.Value.Position - selfPos;
                d.y = 0f;
                if (d.sqrMagnitude > 0.0001f) towardAlly = d.normalized * 0.3f;
            }

            Vector3 combined = awayDir + towardAlly;
            if (combined.sqrMagnitude > 1f) combined.Normalize();

            return new Vector2(combined.x, combined.z);
        }

        Vector2 ComputeRegroupMove(AIPerception perception)
        {
            var nearestAlly = NearestAlly(perception);
            if (!nearestAlly.HasValue) return Vector2.zero;

            Vector3 d = nearestAlly.Value.Position - perception.Self.Position;
            d.y = 0f;
            if (d.sqrMagnitude < 0.0001f) return Vector2.zero;

            Vector3 dir = d.normalized;
            return new Vector2(dir.x, dir.z);
        }

        Vector2 ComputeFlankMove(AIPerception perception, AIUnitSnapshot? targetOpt)
        {
            if (!targetOpt.HasValue) return MoveTowardLaneWaypoint(perception);

            Vector3 selfPos = perception.Self.Position;
            Vector3 toTarget = targetOpt.Value.Position - selfPos;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f) return Vector2.zero;

            Vector3 dirToTarget = toTarget.normalized;
            Vector3 perp = new Vector3(-dirToTarget.z, 0f, dirToTarget.x) * _strafeSign;
            Vector3 combined = dirToTarget * 0.5f + perp * 0.9f;
            if (combined.sqrMagnitude > 1f) combined.Normalize();

            return new Vector2(combined.x, combined.z);
        }

        Vector2 ComputeHoldChokeMove(AIPerception perception)
        {
            return MoveTowardLaneWaypoint(perception) * 0.5f; // measured approach, not a full sprint — this state is about holding position
        }

        Vector2 ComputeSuddenDeathPushMove(AIPerception perception, AIUnitSnapshot? targetOpt)
        {
            if (!targetOpt.HasValue) return MoveTowardLaneWaypoint(perception);

            Vector3 d = targetOpt.Value.Position - perception.Self.Position;
            d.y = 0f;
            if (d.sqrMagnitude < 0.0001f) return Vector2.zero;

            Vector3 dir = d.normalized;
            return new Vector2(dir.x, dir.z);
        }

        Vector2 MoveTowardLaneWaypoint(AIPerception perception)
        {
            Vector3 waypoint = LaneWaypoint(_assignedLane, perception.Self.Team, perception.ArenaHalfExtents);
            Vector3 d = waypoint - perception.Self.Position;
            d.y = 0f;
            if (d.magnitude < 1f) return Vector2.zero;

            Vector3 dir = d.normalized;
            return new Vector2(dir.x, dir.z);
        }

        /// <summary>Simplified round-start spread point for this Sailor's assigned lane — the left/center/right
        /// partition AISquadBrain hands out, biased toward this team's half of the arena.</summary>
        static Vector3 LaneWaypoint(Lane lane, Team team, Vector2 arenaHalfExtents)
        {
            float x = team == Team.A ? -arenaHalfExtents.x * 0.4f : arenaHalfExtents.x * 0.4f;
            float z = lane switch
            {
                Lane.Left => -arenaHalfExtents.y * 0.5f,
                Lane.Right => arenaHalfExtents.y * 0.5f,
                _ => 0f
            };
            return new Vector3(x, 0f, z);
        }

        static AIUnitSnapshot? NearestAlly(AIPerception perception)
        {
            AIUnitSnapshot? best = null;
            float bestSqrDist = float.MaxValue;

            for (int i = 0; i < perception.Allies.Count; i++)
            {
                var ally = perception.Allies[i];
                if (ally.IsKnockedOut) continue;

                float sqrDist = (ally.Position - perception.Self.Position).sqrMagnitude;
                if (sqrDist < bestSqrDist)
                {
                    bestSqrDist = sqrDist;
                    best = ally;
                }
            }

            return best;
        }
    }
}
