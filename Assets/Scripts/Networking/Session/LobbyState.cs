using System;
using System.Collections.Generic;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.Networking.Session
{
    /// <summary>
    /// One lobby slot: a joined human, a reserved-for-rejoin human, or an AI back-fill. Plain data
    /// (Plan 02 §5: "LobbyState should be a data object, not logic embedded in NetworkBehaviours")
    /// so it can be built/inspected/tested without any NGO type in scope.
    /// </summary>
    [Serializable]
    public sealed class LobbyPlayerEntry
    {
        /// <summary>NGO client id once connected, or the pending rejoinToken while a slot is reserved
        /// for a disconnected human (RejoinTokenRegistry) — kept as a string here so this file has
        /// zero NGO/ulong-clientId coupling; the Runtime layer resolves which one it's looking at.</summary>
        public string ParticipantIdOrToken;

        public Team Team;
        public string SailorId;
        public bool IsReady;
        public bool IsAI;
        public string DisplayName;
    }

    /// <summary>
    /// Full lobby snapshot (Plan 02 §3): who's joined, what mode, whether the host has started the
    /// match. <see cref="LanSessionLauncher.BuildSession"/> turns this into the transport-agnostic
    /// <see cref="Session"/> that MatchController actually runs on.
    /// </summary>
    [Serializable]
    public sealed class LobbyState
    {
        public List<LobbyPlayerEntry> Players = new List<LobbyPlayerEntry>();
        public SessionMode Mode = SessionMode.Coop;
        public bool Started;
    }
}
