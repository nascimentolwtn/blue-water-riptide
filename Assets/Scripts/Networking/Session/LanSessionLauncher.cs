using System.Collections.Generic;
using BlueWaterRiptide.Core;
using BlueWaterRiptide.Networking;
using BlueWaterRiptide.Networking.Discovery;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace BlueWaterRiptide.Networking.Session
{
    /// <summary>
    /// LAN backing for <see cref="ISessionProvider"/> (Plan 02/03 §3.2): wraps
    /// <see cref="LanSessionAdvertiser"/> while hosting and <see cref="LanSessionScanner"/> while
    /// browsing, and separately turns a lobby snapshot into the <see cref="BlueWaterRiptide.Core.Session"/> MatchController
    /// actually runs on via <see cref="BuildSession"/>.
    ///
    /// Like <see cref="NetworkBootstrap"/>, this reaches for <see cref="NetworkManager.Singleton"/>
    /// only inside Create/Join, guarded null-safely — the in-scene NetworkManager is Editor-bound
    /// and won't exist while this assembly compiles standalone or in an editor-less test. If it's
    /// absent, Create still advertises (so discovery/lobby UI can be built and tested before the
    /// scene wiring lands) and Join fails fast via JoinCompleted(false).
    /// </summary>
    public sealed class LanSessionLauncher : ISessionProvider
    {
        public const string SinkOrSwimRulesetId = "sink-or-swim";

        public event System.Action<SessionInfo> SessionDiscovered;
        public event System.Action<string> SessionLost;
        public event System.Action<bool> JoinCompleted;

        readonly LanSessionAdvertiser _advertiser = new LanSessionAdvertiser();
        readonly LanSessionScanner _scanner = new LanSessionScanner();

        SessionInfo? _hostedSession;
        bool _joinPending;

        public LanSessionLauncher()
        {
            _scanner.SessionDiscovered += info => SessionDiscovered?.Invoke(info);
            _scanner.SessionLost += id => SessionLost?.Invoke(id);
        }

        /// <summary>Call once per frame from a MonoBehaviour's Update while this provider is in use (hosting and/or browsing) — drives the discovery socket poll and the advertise-broadcast timer.</summary>
        public void Poll(float deltaSeconds)
        {
            _scanner.Poll(deltaSeconds);
            _advertiser.Tick(deltaSeconds);
        }

        public SessionInfo Create(SessionCreateOptions options)
        {
            string ip = LanAddressUtil.GetLocalIPv4() ?? "127.0.0.1";
            ushort port = NetworkBootstrap.DefaultPort;

            var networkManager = NetworkManager.Singleton;
            if (networkManager != null)
            {
                if (networkManager.NetworkConfig.NetworkTransport is UnityTransport transport)
                {
                    NetworkBootstrap.ConfigureDirect(transport, ip, port);
                }

                if (!networkManager.IsListening)
                {
                    networkManager.StartHost();
                }
            }
            else
            {
                Debug.LogWarning("LanSessionLauncher.Create: no live NetworkManager.Singleton — advertising only until Editor scene wiring provides one.");
            }

            string sessionId = $"{ip}:{port}";
            _advertiser.StartAdvertising(options, ip, port, playersCurrent: 1);

            var info = new SessionInfo(sessionId, options.HostName, options.Mode, 1, options.PlayersMax, NetworkProtocol.CurrentVersion, sessionId);
            _hostedSession = info;
            return info;
        }

        /// <summary>Updates the advertised player count (e.g. lobby roster changed) without restarting advertising.</summary>
        public void SetPlayersCurrent(int playersCurrent) => _advertiser.SetPlayersCurrent(playersCurrent);

        public void StopAdvertising()
        {
            _advertiser.StopAdvertising();
            _hostedSession = null;
        }

        public void StartBrowsing() => _scanner.StartBrowsing();

        public void StopBrowsing() => _scanner.StopBrowsing();

        public void Join(string sessionId, string connectionData)
        {
            if (!TryParseIpPort(connectionData, out var ip, out var port))
            {
                Debug.LogWarning($"LanSessionLauncher.Join: malformed connectionData '{connectionData}', expected \"ip:port\".");
                JoinCompleted?.Invoke(false);
                return;
            }

            var networkManager = NetworkManager.Singleton;
            if (networkManager == null || !(networkManager.NetworkConfig.NetworkTransport is UnityTransport transport))
            {
                Debug.LogWarning("LanSessionLauncher.Join: no live NetworkManager.Singleton with a UnityTransport — cannot connect yet.");
                JoinCompleted?.Invoke(false);
                return;
            }

            NetworkBootstrap.ConfigureDirect(transport, ip, port);

            _joinPending = true;
            networkManager.OnClientConnectedCallback += OnClientConnected;
            networkManager.OnClientDisconnectCallback += OnClientDisconnectedWhilePending;
            networkManager.StartClient();
        }

        public void Leave()
        {
            StopAdvertising();
            StopBrowsing();

            var networkManager = NetworkManager.Singleton;
            if (networkManager != null && (networkManager.IsHost || networkManager.IsClient))
            {
                networkManager.Shutdown();
            }
        }

        /// <summary>
        /// Builds the transport-agnostic <see cref="BlueWaterRiptide.Core.Session"/> MatchController runs on (host-only —
        /// clients never construct their own Session, they just render replicated state). Mirrors
        /// what a SinglePlayerLauncher would do for local participants, but resolves Human vs Remote
        /// by comparing each slot's <see cref="LobbyPlayerEntry.ParticipantIdOrToken"/> against the
        /// host's own id/token, since that's the only thing distinguishing "this is me" from
        /// "this is a connected client" once AI slots are filtered out.
        /// </summary>
        public BlueWaterRiptide.Core.Session BuildSession(LobbyState lobbyState, string localParticipantId)
        {
            var participants = new List<ParticipantInfo>(lobbyState.Players.Count);
            for (int i = 0; i < lobbyState.Players.Count; i++)
            {
                var entry = lobbyState.Players[i];
                DriverType driverType;
                if (entry.IsAI)
                {
                    driverType = DriverType.AI;
                }
                else if (entry.ParticipantIdOrToken == localParticipantId)
                {
                    driverType = DriverType.Human;
                }
                else
                {
                    driverType = DriverType.Remote;
                }

                participants.Add(new ParticipantInfo(new ParticipantId(i), entry.Team, driverType, entry.SailorId));
            }

            return new BlueWaterRiptide.Core.Session(SinkOrSwimRulesetId, ArenaCatalog.Default.Id, participants);
        }

        void OnClientConnected(ulong clientId)
        {
            if (!_joinPending) return;

            _joinPending = false;
            UnsubscribeJoinCallbacks();
            JoinCompleted?.Invoke(true);
        }

        void OnClientDisconnectedWhilePending(ulong clientId)
        {
            if (!_joinPending) return;

            _joinPending = false;
            UnsubscribeJoinCallbacks();
            JoinCompleted?.Invoke(false);
        }

        void UnsubscribeJoinCallbacks()
        {
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null) return;

            networkManager.OnClientConnectedCallback -= OnClientConnected;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnectedWhilePending;
        }

        static bool TryParseIpPort(string connectionData, out string ip, out ushort port)
        {
            ip = null;
            port = 0;

            if (string.IsNullOrEmpty(connectionData)) return false;

            int separatorIndex = connectionData.LastIndexOf(':');
            if (separatorIndex <= 0 || separatorIndex == connectionData.Length - 1) return false;

            ip = connectionData.Substring(0, separatorIndex);
            return ushort.TryParse(connectionData.Substring(separatorIndex + 1), out port);
        }
    }
}
