using System;

namespace BlueWaterRiptide.Core
{
    /// <summary>Lobby mode, shared by every connection mode that can back-fill empty slots with AI (Plan 02 §3, Plan 03 §1).</summary>
    public enum SessionMode { Coop, Pvp }

    /// <summary>
    /// One discoverable/joinable session as seen by a browser, transport-agnostic (Plan 03 §3.2/3.3).
    /// LAN fills <see cref="ConnectionData"/> with "ip:port" from UDP broadcast; a future online
    /// provider fills it with a Relay join code — callers never parse it, only pass it back to Join.
    /// </summary>
    public readonly struct SessionInfo
    {
        public readonly string SessionId;
        public readonly string HostName;
        public readonly SessionMode Mode;
        public readonly int PlayersCurrent;
        public readonly int PlayersMax;
        public readonly int ProtocolVersion;
        public readonly string ConnectionData;

        public SessionInfo(string sessionId, string hostName, SessionMode mode, int playersCurrent, int playersMax, int protocolVersion, string connectionData)
        {
            SessionId = sessionId;
            HostName = hostName;
            Mode = mode;
            PlayersCurrent = playersCurrent;
            PlayersMax = playersMax;
            ProtocolVersion = protocolVersion;
            ConnectionData = connectionData;
        }
    }

    /// <summary>Options for standing up a new session as its host.</summary>
    public readonly struct SessionCreateOptions
    {
        public readonly string HostName;
        public readonly SessionMode Mode;
        public readonly int PlayersMax;

        public SessionCreateOptions(string hostName, SessionMode mode, int playersMax)
        {
            HostName = hostName;
            Mode = mode;
            PlayersMax = playersMax;
        }
    }

    /// <summary>
    /// Abstracts session create/advertise/browse/join/leave behind one seam (Plan 03 §3.2) so
    /// Host/Join UI (Plan 04, once built) binds to this interface rather than to LAN-specific
    /// classes. <see cref="BlueWaterRiptide.Networking"/>'s LAN implementation backs this with UDP
    /// broadcast + direct connect; a future online implementation backs it with Unity Lobby + Relay
    /// — neither the UI nor Core code above this interface needs to change when that lands.
    /// </summary>
    public interface ISessionProvider
    {
        /// <summary>Raised as each session is (re-)seen while browsing. Implementations debounce repeated advertisements themselves.</summary>
        event Action<SessionInfo> SessionDiscovered;

        /// <summary>Raised when a previously discovered session stops being seen (e.g. LAN advertisement expiry).</summary>
        event Action<string> SessionLost;

        /// <summary>Raised once a Join call resolves, success or failure.</summary>
        event Action<bool> JoinCompleted;

        /// <summary>Stands up a new session and starts advertising it. The caller is the host.</summary>
        SessionInfo Create(SessionCreateOptions options);

        /// <summary>Stops advertising the session created by <see cref="Create"/>. No-op if not hosting.</summary>
        void StopAdvertising();

        /// <summary>Starts surfacing <see cref="SessionDiscovered"/>/<see cref="SessionLost"/> events for joinable sessions.</summary>
        void StartBrowsing();

        /// <summary>Stops browsing. No-op if not currently browsing.</summary>
        void StopBrowsing();

        /// <summary>Attempts to join a session by id (as reported by <see cref="SessionDiscovered"/>, or entered manually e.g. "Join by IP"/join code).</summary>
        void Join(string sessionId, string connectionData);

        /// <summary>Leaves the current session, whether hosting or joined as a client.</summary>
        void Leave();
    }
}
