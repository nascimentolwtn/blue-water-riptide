using UnityEngine;
using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.AI
{
    /// <summary>Backs away from the nearest visible enemy when low on HP or out of ammo — never
    /// fires or Supers while retreating (Plan 01 §3's "retreat when HP &lt; threshold or ammo empty").</summary>
    public sealed class RetreatState : IAIState
    {
        public string Name => "Retreat";

        public float Score(in AIContext ctx)
        {
            bool lowHp = ctx.Self.HpFraction <= ctx.Profile.RetreatHpFraction;
            bool noAmmo = ctx.Self.CurrentAmmo <= 0;

            if (lowHp) return 1f;
            if (noAmmo) return 0.5f;
            return 0f;
        }

        public InputCommand Produce(in AIContext ctx, double time)
        {
            var nearest = ctx.Perception.GetNearestEnemy();
            if (nearest == null) return InputCommand.None(time);

            Vector3 away = ctx.Self.transform.position - nearest.Value.Pawn.transform.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.0001f) return InputCommand.None(time);

            Vector3 dir = away.normalized;
            Vector2 move = new Vector2(dir.x, dir.z);

            return new InputCommand(move, move, false, false, false, time);
        }
    }
}
