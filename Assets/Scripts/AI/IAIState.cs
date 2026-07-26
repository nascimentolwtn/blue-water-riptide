using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.AI
{
    /// <summary>Everything one utility-scored behavior state needs to score/produce a command
    /// (Plan 01 §3). A snapshot, not a service — AIInputDriver builds one fresh per decision.</summary>
    public readonly struct AIContext
    {
        public readonly SailorPawn Self;
        public readonly AIPerception Perception;
        public readonly AIBehaviorProfile Profile;
        public readonly AIDifficultyParams Difficulty;

        public AIContext(SailorPawn self, AIPerception perception, AIBehaviorProfile profile, AIDifficultyParams difficulty)
        {
            Self = self;
            Perception = perception;
            Profile = profile;
            Difficulty = difficulty;
        }
    }

    /// <summary>One utility-scored behavior state (Engage, Retreat, Regroup, ...). Highest Score
    /// wins, subject to AIInputDriver's minimum dwell time before switching (Plan 01 §3).</summary>
    public interface IAIState
    {
        string Name { get; }
        float Score(in AIContext ctx);
        InputCommand Produce(in AIContext ctx, double time);
    }
}
