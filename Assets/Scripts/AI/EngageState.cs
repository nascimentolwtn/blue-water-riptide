using UnityEngine;
using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.AI
{
    /// <summary>
    /// Holds/closes to the profile's preferred range band on the nearest visible enemy, fires
    /// when in range with enough ammo, and uses Super once charged (Plan 01 §3's Serve/Spike
    /// rules). Target leading is not implemented yet — aim points straight at the target's
    /// current position, only jittered by the difficulty's aim-error; AIDifficultyParams.
    /// TargetLeadingFraction is unused until a future pass adds target velocity tracking.
    /// </summary>
    public sealed class EngageState : IAIState
    {
        public string Name => "Engage";

        public float Score(in AIContext ctx)
        {
            if (SelectTarget(ctx) == null) return 0f;
            if (ctx.Self.CurrentAmmo < ctx.Profile.MinAmmoToEngage) return 0f;
            if (ctx.Self.HpFraction <= ctx.Profile.RetreatHpFraction) return 0.1f; // Retreat should outscore this
            return 0.6f;
        }

        public InputCommand Produce(in AIContext ctx, double time)
        {
            var selected = SelectTarget(ctx);
            if (selected == null) return InputCommand.None(time);

            SailorPawn self = ctx.Self;
            SailorPawn target = selected.Value.Pawn;
            float distance = selected.Value.Distance;

            Vector3 toTarget = target.transform.position - self.transform.position;
            toTarget.y = 0f;
            Vector3 dirToTarget = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : self.transform.forward;

            float preferredRange = self.Definition.AttackRange * ctx.Profile.PreferredRangeFraction;
            float rangeError = distance - preferredRange;

            Vector2 move;
            if (Mathf.Abs(rangeError) > 0.5f)
            {
                Vector3 radial = rangeError > 0f ? dirToTarget : -dirToTarget; // too far -> close in, too close -> back off
                move = new Vector2(radial.x, radial.z);
            }
            else if (ctx.Profile.StrafePerpendicular)
            {
                Vector3 perpendicular = new Vector3(-dirToTarget.z, 0f, dirToTarget.x);
                move = new Vector2(perpendicular.x, perpendicular.z);
            }
            else
            {
                move = Vector2.zero;
            }

            Vector2 aim = ApplyAimError(new Vector2(dirToTarget.x, dirToTarget.z), ctx, target, time);

            bool inRange = distance <= self.Definition.AttackRange;
            bool fireHeld = inRange && self.CurrentAmmo > 0;
            bool superPressed = ShouldUseSuper(ctx, target, distance, preferredRange);

            return new InputCommand(move, aim, false, fireHeld, superPressed, time);
        }

        static Vector2 ApplyAimError(Vector2 aim, in AIContext ctx, SailorPawn target, double time)
        {
            float errorDegrees = ctx.Difficulty.AimErrorDegrees;
            if (errorDegrees <= 0f || aim.sqrMagnitude < 0.0001f) return aim;

            // Deterministic per-target jitter that drifts smoothly over time rather than
            // snapping every frame — Perlin noise keyed by time and the target's id.
            float noise = Mathf.PerlinNoise((float)time * 2.5f, target.Id.Value * 7.31f);
            float jitterDegrees = (noise - 0.5f) * 2f * errorDegrees;
            float radians = jitterDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(aim.x * cos - aim.y * sin, aim.x * sin + aim.y * cos);
        }

        /// <summary>Prefers the squad's focus-fire suggestion when it's currently visible,
        /// otherwise falls back to the nearest visible enemy. Null SquadIntent (solo AI, e.g.
        /// today's M1 1v1) always falls back.</summary>
        static PerceivedPawn? SelectTarget(in AIContext ctx)
        {
            if (ctx.SquadIntent?.FocusFireTargetId is ParticipantId focusId)
            {
                foreach (var enemy in ctx.Perception.GetVisibleEnemies())
                {
                    if (enemy.Pawn.Id == focusId) return enemy;
                }
            }

            return ctx.Perception.GetNearestEnemy();
        }

        static bool ShouldUseSuper(in AIContext ctx, SailorPawn target, float distance, float preferredRange)
        {
            if (ctx.Self.SuperCharge01 < 1f) return false;

            // Deckhand "uses Super late": wait until committed close to the target rather than
            // popping it the instant it's available.
            if (!ctx.Difficulty.UsesSuperPromptly && distance > preferredRange * 0.5f) return false;

            if (ctx.Profile.SuperClusterMinCount > 0)
            {
                return ctx.Perception.CountEnemiesWithin(target.transform.position, ctx.Profile.SuperClusterRadius) >= ctx.Profile.SuperClusterMinCount;
            }

            return true;
        }
    }
}
