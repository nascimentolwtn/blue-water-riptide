using UnityEngine;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.AI
{
    /// <summary>Moves toward the nearest ally when no enemy is visible — a 1v1 match (today's M1
    /// prototype) never has an ally, so this is a no-op there; it starts mattering once 3v3
    /// Single Player (Plan 01 M2) spawns real teammates.</summary>
    public sealed class RegroupState : IAIState
    {
        public string Name => "Regroup";

        public float Score(in AIContext ctx) => ctx.Perception.GetNearestEnemy() == null ? 0.4f : 0f;

        public InputCommand Produce(in AIContext ctx, double time)
        {
            var nearestAlly = ctx.Perception.GetNearestAlly();
            if (nearestAlly == null) return InputCommand.None(time);

            Vector3 toAlly = nearestAlly.Value.Pawn.transform.position - ctx.Self.transform.position;
            toAlly.y = 0f;
            if (toAlly.sqrMagnitude < 1f) return InputCommand.None(time); // close enough — stop closing in

            Vector3 dir = toAlly.normalized;
            return new InputCommand(new Vector2(dir.x, dir.z), Vector2.zero, false, false, false, time);
        }
    }
}
