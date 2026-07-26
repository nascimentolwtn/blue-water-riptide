using System;
using System.Collections.Generic;

namespace BlueWaterRiptide.Core.Sessions
{
    public enum SessionProviderState { Idle, Advertising, Joining, Joined, Failed }

    /// <summary>Config for the host path (CreateAndAdvertise). Deliberately minimal and
    /// mode-agnostic — Mode is a provider-defined string (e.g. LAN's "coop"/"pvp") rather than an
    /// enum here, so this interface never needs to know every connection mode's vocabulary.</summary>
    public sealed class SessionHostConfig
    {
        public string HostDisplayName;
        public string Mode;
        public int MaxPlayers;
    }

    /// <summary>One session as seen by a browsing client — a plain DTO a Host/Join UI screen binds
    /// to, never the transport-specific advertisement type underneath.</summary>
    public sealed class DiscoveredSessionInfo
    {
        public string HostDisplayName;
        public string Mode;
        public int PlayersCurrent;
        public int PlayersMax;
        public bool ProtocolCompatible;
        /// <summary>Opaque to callers — pass back into Join() unmodified via SessionJoinTarget.FromDiscovered.</summary>
        public object JoinToken;
    }

    /// <summary>What to join: a previously-discovered session, or a manual "Join by IP" endpoint
    /// (Plan 02 §2's fallback for when discovery itself fails, e.g. AP/client isolation).</summary>
    public sealed class SessionJoinTarget
    {
        public DiscoveredSessionInfo Discovered { get; private set; }
        public string ManualAddress { get; private set; }
        public int ManualPort { get; private set; }

        public static SessionJoinTarget FromDiscovered(DiscoveredSessionInfo info) => new SessionJoinTarget { Discovered = info };
        public static SessionJoinTarget FromAddress(string address, int port) => new SessionJoinTarget { ManualAddress = address, ManualPort = port };
    }

    /// <summary>
    /// Seam a Host/Join UI screen binds to instead of any transport-specific type (Plan 03 §3.2's
    /// "do this now" checklist — keep Session/UI transport-agnostic so Online can slot in later
    /// without touching callers). LAN implements this today; a future Online provider implements
    /// it the same way. Poll-style (Tick), matching MatchController's convention, rather than
    /// async/Task, so it composes the same way in a single-threaded Unity Update loop.
    /// </summary>
    public interface ISessionProvider
    {
        SessionProviderState State { get; }

        /// <summary>Host path: start advertising/listening for joiners.</summary>
        void CreateAndAdvertise(SessionHostConfig config);

        /// <summary>Client path: sessions discovered since Join/browsing started.</summary>
        IReadOnlyList<DiscoveredSessionInfo> GetDiscovered();

        /// <summary>Client path: attempt to join a specific target.</summary>
        void Join(SessionJoinTarget target);

        /// <summary>Leaves/stops hosting and returns to Idle.</summary>
        void Leave();

        event Action<SessionProviderState> OnStateChanged;

        /// <summary>Call every frame/tick while this provider is in use.</summary>
        void Tick(float deltaTime);
    }
}
