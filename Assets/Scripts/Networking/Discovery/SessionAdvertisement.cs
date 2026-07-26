using System;
using UnityEngine;

namespace BlueWaterRiptide.Networking.Discovery
{
    public enum LanMode { Coop, Pvp }

    /// <summary>
    /// Wire format broadcast by a host to LanSessionAdvertiser.BroadcastPort every ~1s while its
    /// lobby is open (Plan 02 §2). Deliberately has no Netcode for GameObjects dependency —
    /// discovery must work before any NGO session exists. Bump ProtocolVersion on any field
    /// change; scanners grey out sessions whose protocolVersion doesn't match theirs.
    /// </summary>
    [Serializable]
    public sealed class SessionAdvertisement
    {
        public const int ProtocolVersion = 1;

        public string gameId;
        public int protocolVersion = ProtocolVersion;
        public string hostName;
        public string ip;
        public int port;
        public string mode;
        public int playersCurrent;
        public int playersMax;
        public string lobbyState;

        public static SessionAdvertisement Create(string hostName, string ip, int port, LanMode mode, int playersCurrent, int playersMax, string lobbyState)
        {
            return new SessionAdvertisement
            {
                gameId = "blue-water-riptide",
                protocolVersion = ProtocolVersion,
                hostName = hostName,
                ip = ip,
                port = port,
                mode = mode == LanMode.Coop ? "coop" : "pvp",
                playersCurrent = playersCurrent,
                playersMax = playersMax,
                lobbyState = lobbyState
            };
        }

        public bool IsProtocolCompatible => protocolVersion == ProtocolVersion;

        public string ToJson() => JsonUtility.ToJson(this);

        public static SessionAdvertisement FromJson(string json) => JsonUtility.FromJson<SessionAdvertisement>(json);
    }
}
