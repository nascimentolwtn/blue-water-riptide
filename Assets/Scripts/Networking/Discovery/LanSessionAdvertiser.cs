using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BlueWaterRiptide.Core;
using BlueWaterRiptide.Networking;

namespace BlueWaterRiptide.Networking.Discovery
{
    /// <summary>
    /// Host-side UDP broadcaster (Plan 02 §2): fires a JSON <see cref="SessionAdvertisement"/> at
    /// 255.255.255.255:47777 once a second while a lobby is open. No NGO dependency — this only
    /// needs a socket and a clock tick, so it stays usable/testable without a live NetworkManager.
    /// Externally ticked (same pattern as RisingTideHazard.Tick) rather than owning a coroutine, so
    /// the caller controls exactly when network I/O happens and can stop it deterministically.
    ///
    /// Android note (no code needed here, left as a signpost per Plan 02 §2/§7): broadcast *send*
    /// doesn't need the multicast lock that broadcast *receive* does — that requirement lands on
    /// LanSessionScanner, not here.
    /// </summary>
    public sealed class LanSessionAdvertiser
    {
        public const int BroadcastPort = 47777;
        const float BroadcastIntervalSeconds = 1f;

        UdpClient _socket;
        IPEndPoint _broadcastEndPoint;
        SessionAdvertisement _advertisement;
        float _timeSinceLastBroadcast;

        public bool IsAdvertising { get; private set; }

        /// <summary>
        /// Starts advertising a freshly created session. <paramref name="ip"/>/<paramref name="port"/>
        /// are the game's own listen endpoint (see NetworkBootstrap.DefaultPort), not the discovery
        /// port — <paramref name="ip"/> is typically <see cref="LanAddressUtil.GetLocalIPv4"/>'s result,
        /// resolved by the caller (LanSessionLauncher) so this class stays free of any "how do I find
        /// my own address" concern beyond broadcasting one it's handed.
        /// </summary>
        public void StartAdvertising(SessionCreateOptions options, string ip, ushort port, int playersCurrent)
        {
            StopAdvertising();

            _socket = new UdpClient();
            _socket.EnableBroadcast = true;
            _broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, BroadcastPort);

            _advertisement = new SessionAdvertisement
            {
                gameId = SessionAdvertisement.GameId,
                protocolVersion = NetworkProtocol.CurrentVersion,
                hostName = options.HostName,
                ip = ip,
                port = port,
                mode = options.Mode == SessionMode.Coop ? SessionAdvertisement.CoopMode : SessionAdvertisement.PvpMode,
                playersCurrent = playersCurrent,
                playersMax = options.PlayersMax,
                lobbyState = SessionAdvertisement.OpenLobbyState,
            };

            _timeSinceLastBroadcast = BroadcastIntervalSeconds; // send immediately on the first Tick
            IsAdvertising = true;
        }

        /// <summary>Updates the player-count field broadcast on the next tick (e.g. someone joined/left the lobby).</summary>
        public void SetPlayersCurrent(int playersCurrent)
        {
            if (_advertisement != null)
            {
                _advertisement.playersCurrent = playersCurrent;
            }
        }

        public void StopAdvertising()
        {
            IsAdvertising = false;
            _advertisement = null;

            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }
        }

        /// <summary>Call once per frame (or at least once a second) from a MonoBehaviour's Update while hosting a lobby.</summary>
        public void Tick(float deltaSeconds)
        {
            if (!IsAdvertising) return;

            _timeSinceLastBroadcast += deltaSeconds;
            if (_timeSinceLastBroadcast < BroadcastIntervalSeconds) return;

            _timeSinceLastBroadcast = 0f;
            Broadcast();
        }

        void Broadcast()
        {
            try
            {
                string json = UnityEngine.JsonUtility.ToJson(_advertisement);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                _socket.Send(bytes, bytes.Length, _broadcastEndPoint);
            }
            catch (SocketException ex)
            {
                UnityEngine.Debug.LogWarning($"LanSessionAdvertiser: broadcast failed ({ex.Message}); will retry next tick.");
            }
        }
    }
}
