using System;
using System.Collections.Generic;

namespace BlueWaterRiptide.Core.Modes
{
    public sealed class SinglePlayerConfig
    {
        public string HumanSailorId;
        public int TeamSize = 3;
    }

    /// <summary>
    /// Builds the Session for Single Player (Plan 01 §2): one human + AI teammates on Team A,
    /// full AI Team B. AITeamBuilder fills every AI slot from the roster the caller supplies —
    /// never a hard-coded Sailor id here. Building the Session is all this does; spawning
    /// pawns/drivers for it (a Session-driven scene builder, replacing M1PrototypeBootstrap's
    /// hardcoded 1v1) is a separate, not-yet-implemented step — nothing currently consumes the
    /// Session this returns.
    /// </summary>
    public static class SinglePlayerLauncher
    {
        public static Session BuildSession(SinglePlayerConfig config, IReadOnlyList<SailorRosterEntry> availablePool, Random rng)
        {
            var participants = new List<ParticipantInfo>();
            int nextId = 0;

            participants.Add(new ParticipantInfo(new ParticipantId(nextId++), Team.A, DriverType.Human, config.HumanSailorId));

            var teamAAiConstraints = new TeamBuildConstraints { TeamSize = Math.Max(0, config.TeamSize - 1) };
            foreach (var sailorId in AITeamBuilder.Build(availablePool, teamAAiConstraints, rng))
            {
                participants.Add(new ParticipantInfo(new ParticipantId(nextId++), Team.A, DriverType.AI, sailorId));
            }

            var teamBConstraints = new TeamBuildConstraints { TeamSize = config.TeamSize };
            foreach (var sailorId in AITeamBuilder.Build(availablePool, teamBConstraints, rng))
            {
                participants.Add(new ParticipantInfo(new ParticipantId(nextId++), Team.B, DriverType.AI, sailorId));
            }

            return new Session(ConnectionMode.SinglePlayer, "sink-or-swim", ArenaCatalog.Default.Id, participants);
        }
    }
}
