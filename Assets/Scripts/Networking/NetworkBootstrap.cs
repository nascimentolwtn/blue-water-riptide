using System;
using Unity.Netcode.Transports.UTP;

namespace BlueWaterRiptide.Networking
{
    /// <summary>
    /// Isolates UTP endpoint setup behind a small factory (Plan 03 §3.3: "direct(ip,port) today,
    /// relay(allocation) tomorrow — never scatter transport configuration"). Pure config methods,
    /// deliberately not tied to <c>NetworkManager.Singleton</c> at call time: the live
    /// NetworkManager GameObject is Editor-bound and doesn't exist while this assembly compiles
    /// standalone, so callers (future Editor-side glue) pass their own <see cref="UnityTransport"/>
    /// reference in rather than this class reaching out to find one.
    /// </summary>
    public static class NetworkBootstrap
    {
        public const ushort DefaultPort = 7777;

        /// <summary>
        /// Configures a UTP transport for LAN direct-connect (host and client both call this —
        /// the host binds a listen address, a joining client points at the host's ip:port from a
        /// discovered/ manually entered <c>SessionInfo.ConnectionData</c>).
        /// </summary>
        public static void ConfigureDirect(UnityTransport transport, string ip, ushort port)
        {
            if (transport == null)
            {
                throw new ArgumentNullException(nameof(transport));
            }

            if (string.IsNullOrEmpty(ip))
            {
                throw new ArgumentException("ip must not be null/empty.", nameof(ip));
            }

            transport.SetConnectionData(ip, port);
        }

        /// <summary>
        /// Placeholder for Plan 03's future online mode (Unity Relay). Not implemented here —
        /// this stub exists so the direct/relay split is visible in the API shape from day one
        /// instead of being bolted on later, per Plan 03 §3.3's "never scatter transport
        /// configuration" guidance.
        /// </summary>
        public static void ConfigureRelay(UnityTransport transport, object relayAllocation)
        {
            throw new NotImplementedException(
                "ConfigureRelay is a Plan 03 (online mode) placeholder — Relay allocation flow isn't implemented yet.");
        }
    }
}
