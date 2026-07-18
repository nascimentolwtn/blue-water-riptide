using System.Collections.Generic;

namespace BlueWaterRiptide.Core
{
    public enum Team { A, B }

    public enum DriverType { Human, AI, Remote }

    /// <summary>Describes one match participant. Built by whichever connection mode launcher constructs the Session.</summary>
    public sealed class ParticipantInfo
    {
        public ParticipantId Id { get; }
        public Team Team { get; }
        public DriverType DriverType { get; }
        public string SailorId { get; }

        public ParticipantInfo(ParticipantId id, Team team, DriverType driverType, string sailorId)
        {
            Id = id;
            Team = team;
            DriverType = driverType;
            SailorId = sailorId;
        }
    }

    /// <summary>
    /// Describes a match-to-be. Single Player, LAN, and (future) Online all construct one of these
    /// and hand it to MatchController — this is the seam that keeps all connection modes symmetric.
    /// </summary>
    public sealed class Session
    {
        public string RulesetId { get; }
        public string ArenaId { get; }
        public IReadOnlyList<ParticipantInfo> Participants { get; }

        public Session(string rulesetId, string arenaId, IReadOnlyList<ParticipantInfo> participants)
        {
            RulesetId = rulesetId;
            ArenaId = arenaId;
            Participants = participants;
        }
    }
}
