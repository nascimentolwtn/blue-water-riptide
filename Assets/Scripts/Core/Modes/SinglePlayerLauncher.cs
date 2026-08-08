using System.Collections.Generic;
using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.Core.Modes
{
    /// <summary>
    /// Builds the Session for a Single Player match (Plan 01 §2). Only constructs the Session
    /// data — it does not spawn pawns or wire concrete IInputDriver instances to participants;
    /// that's the scene/bootstrap integration layer's job, kept separate so this class stays a
    /// pure, testable function of (available Sailors, config) -> Session.
    /// </summary>
    public static class SinglePlayerLauncher
    {
        /// <summary>The "Sink or Swim" (Plan 00) ruleset id single-player matches always use in v1 —
        /// matches the literal M1PrototypeBootstrap already uses, so both launchers agree on the id.</summary>
        public const string SinkOrSwimRulesetId = "sink-or-swim";

        /// <summary>
        /// Builds a Session: participant 0 is the human on Team A, followed by aiTeammateCount AI
        /// teammates on Team A and aiOpponentCount AI opponents on Team B. Team sizes are parameters
        /// (not hardcoded 3v3) so v1's default config can change without touching this method.
        /// </summary>
        public static Session BuildSession(
            IReadOnlyList<SailorDefinitionData> availableSailors,
            string humanSailorId,
            int aiTeammateCount = 2,
            int aiOpponentCount = 3,
            int maxDuplicatesPerSailor = 2,
            int maxTanksPerTeam = 1)
        {
            var participants = new List<ParticipantInfo>(1 + aiTeammateCount + aiOpponentCount);
            int nextId = 0;

            participants.Add(new ParticipantInfo(new ParticipantId(nextId++), Team.A, DriverType.Human, humanSailorId));

            var teammateConstraints = new AITeamBuildConstraints
            {
                TeamSize = aiTeammateCount,
                MaxDuplicatesPerSailor = maxDuplicatesPerSailor,
                MaxTanksPerTeam = maxTanksPerTeam
            };
            IReadOnlyList<string> teammateSailorIds = AITeamBuilder.Build(availableSailors, teammateConstraints);
            for (int i = 0; i < teammateSailorIds.Count; i++)
            {
                participants.Add(new ParticipantInfo(new ParticipantId(nextId++), Team.A, DriverType.AI, teammateSailorIds[i]));
            }

            var opponentConstraints = new AITeamBuildConstraints
            {
                TeamSize = aiOpponentCount,
                MaxDuplicatesPerSailor = maxDuplicatesPerSailor,
                MaxTanksPerTeam = maxTanksPerTeam
            };
            IReadOnlyList<string> opponentSailorIds = AITeamBuilder.Build(availableSailors, opponentConstraints);
            for (int i = 0; i < opponentSailorIds.Count; i++)
            {
                participants.Add(new ParticipantInfo(new ParticipantId(nextId++), Team.B, DriverType.AI, opponentSailorIds[i]));
            }

            return new Session(SinkOrSwimRulesetId, ArenaCatalog.Default.Id, participants);
        }
    }
}
