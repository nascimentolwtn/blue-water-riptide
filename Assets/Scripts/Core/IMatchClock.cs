namespace BlueWaterRiptide.Core
{
    public interface IMatchClock
    {
        double Time { get; }
        float DeltaTime { get; }
    }

    /// <summary>Default clock backed by UnityEngine.Time. Single Player/LAN both tick from this.</summary>
    public sealed class UnityMatchClock : IMatchClock
    {
        public double Time => UnityEngine.Time.timeAsDouble;
        public float DeltaTime => UnityEngine.Time.deltaTime;
    }
}
