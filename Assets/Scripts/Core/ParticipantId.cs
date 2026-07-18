namespace BlueWaterRiptide.Core
{
    public readonly struct ParticipantId
    {
        public readonly int Value;

        public ParticipantId(int value) => Value = value;

        public override bool Equals(object obj) => obj is ParticipantId other && other.Value == Value;
        public override int GetHashCode() => Value;
        public override string ToString() => $"P{Value}";

        public static bool operator ==(ParticipantId a, ParticipantId b) => a.Value == b.Value;
        public static bool operator !=(ParticipantId a, ParticipantId b) => a.Value != b.Value;
    }
}
