using System;

namespace BlueWaterRiptide.Networking.Discovery
{
    /// <summary>
    /// UDP broadcast datagram shape (Plan 02 §2). [Serializable] + public fields only, no
    /// properties: JsonUtility (used for wire (de)serialization, see LanSessionAdvertiser/Scanner)
    /// only ever sees public fields on a [Serializable] type.
    /// </summary>
    [Serializable]
    public sealed class SessionAdvertisement
    {
        public string gameId;
        public int protocolVersion;
        public string hostName;
        public string ip;
        public int port;

        /// <summary>"coop" or "pvp" — mirrors SessionMode, kept as a raw string on the wire so a
        /// future protocol version can add a mode without an enum-ordinal compatibility trap.</summary>
        public string mode;

        public int playersCurrent;
        public int playersMax;

        /// <summary>e.g. "open" while the lobby accepts joins, "started" once match play begins
        /// (advertising stops on match start per Plan 02 §2, so in practice this is always "open"
        /// today — kept as a string field so a future "spectate a live match" mode has somewhere
        /// to put it without a wire-format bump).</summary>
        public string lobbyState;

        public const string CoopMode = "coop";
        public const string PvpMode = "pvp";
        public const string OpenLobbyState = "open";

        /// <summary>Filters out other apps' broadcasts sharing the same LAN discovery port.</summary>
        public const string GameId = "blue-water-riptide";
    }
}
