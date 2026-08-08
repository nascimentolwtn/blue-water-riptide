using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.Networking.Discovery
{
    /// <summary>
    /// Client-side UDP listener (Plan 02 §2): aggregates <see cref="SessionAdvertisement"/>
    /// broadcasts into a live session list, expiring entries not re-seen for 3s.
    ///
    /// Threading: the socket read loop runs on a background thread (UdpClient isn't the problem —
    /// Unity/NGO APIs downstream of the discovery events are, since they're not thread-safe outside
    /// the main thread). Received datagrams are only ever parsed and queued on the background
    /// thread; <see cref="Poll"/> must be called every frame from a MonoBehaviour's Update to drain
    /// the queue and raise <see cref="SessionDiscovered"/>/<see cref="SessionLost"/> on the main
    /// thread, matching the externally-ticked pattern used elsewhere in this project (e.g.
    /// RisingTideHazard.Tick) rather than the class reaching for Unity APIs itself off-thread.
    ///
    /// Android note (comment only, no code needed — Plan 02 §2/§7): broadcast *receive* is filtered
    /// on many devices unless a WifiManager.MulticastLock is held while scanning, and AP/client
    /// isolation on some routers silently breaks discovery entirely regardless of the lock — that's
    /// why ISessionProvider.Join(sessionId, "ip:port") always needs a manual "Join by IP" UI path as
    /// a fallback that never depends on this class. Acquiring the lock is Editor/platform-integration
    /// glue (Android plugin call), out of scope here.
    /// </summary>
    public sealed class LanSessionScanner
    {
        public const int ListenPort = LanSessionAdvertiser.BroadcastPort;
        const float ExpirySeconds = 3f;

        public event Action<SessionInfo> SessionDiscovered;
        public event Action<string> SessionLost;

        UdpClient _socket;
        Thread _receiveThread;
        volatile bool _stopRequested;

        readonly ConcurrentQueue<SessionAdvertisement> _incoming = new ConcurrentQueue<SessionAdvertisement>();
        readonly Dictionary<string, float> _timeSinceLastSeen = new Dictionary<string, float>();

        public bool IsBrowsing { get; private set; }

        public void StartBrowsing()
        {
            if (IsBrowsing) return;

            try
            {
                _socket = new UdpClient();
                _socket.ExclusiveAddressUse = false;
                _socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _socket.Client.Bind(new IPEndPoint(IPAddress.Any, ListenPort));
            }
            catch (SocketException ex)
            {
                UnityEngine.Debug.LogWarning($"LanSessionScanner: failed to bind discovery port {ListenPort} ({ex.Message}); browsing disabled, Join by IP still works.");
                _socket = null;
                return;
            }

            _stopRequested = false;
            _receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
            _receiveThread.Start();
            IsBrowsing = true;
        }

        public void StopBrowsing()
        {
            if (!IsBrowsing) return;

            IsBrowsing = false;
            _stopRequested = true;

            // Receive() only unblocks once the socket it's blocked on is closed — closing here
            // (rather than only in the thread) is what actually wakes the background thread up.
            _socket?.Close();
            _socket = null;

            _receiveThread = null; // background thread; let it exit on its own once Receive() throws
            _timeSinceLastSeen.Clear();
            while (_incoming.TryDequeue(out _)) { }
        }

        /// <summary>Drains queued advertisements and ages known sessions. Call every frame from a MonoBehaviour's Update while browsing.</summary>
        public void Poll(float deltaSeconds)
        {
            // De-dupe: keep only the newest advertisement per session id seen since the last Poll.
            var freshest = new Dictionary<string, SessionAdvertisement>();
            while (_incoming.TryDequeue(out var advertisement))
            {
                string sessionId = MakeSessionId(advertisement);
                freshest[sessionId] = advertisement;
            }

            foreach (var kvp in freshest)
            {
                _timeSinceLastSeen[kvp.Key] = 0f;
                SessionDiscovered?.Invoke(ToSessionInfo(kvp.Key, kvp.Value));
            }

            if (_timeSinceLastSeen.Count == 0) return;

            List<string> expired = null;
            foreach (var kvp in _timeSinceLastSeen)
            {
                float updated = kvp.Value + deltaSeconds;
                if (updated >= ExpirySeconds)
                {
                    (expired ??= new List<string>()).Add(kvp.Key);
                }
                else
                {
                    _timeSinceLastSeen[kvp.Key] = updated;
                }
            }

            if (expired == null) return;
            foreach (var sessionId in expired)
            {
                _timeSinceLastSeen.Remove(sessionId);
                SessionLost?.Invoke(sessionId);
            }
        }

        static string MakeSessionId(SessionAdvertisement advertisement) => $"{advertisement.ip}:{advertisement.port}";

        static SessionInfo ToSessionInfo(string sessionId, SessionAdvertisement advertisement)
        {
            SessionMode mode = advertisement.mode == SessionAdvertisement.PvpMode ? SessionMode.Pvp : SessionMode.Coop;
            string connectionData = $"{advertisement.ip}:{advertisement.port}";
            return new SessionInfo(sessionId, advertisement.hostName, mode, advertisement.playersCurrent, advertisement.playersMax, advertisement.protocolVersion, connectionData);
        }

        void ReceiveLoop()
        {
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (!_stopRequested)
            {
                try
                {
                    byte[] data = _socket.Receive(ref remoteEndPoint);
                    string json = Encoding.UTF8.GetString(data);
                    var advertisement = UnityEngine.JsonUtility.FromJson<SessionAdvertisement>(json);

                    if (advertisement == null || advertisement.gameId != SessionAdvertisement.GameId) continue;

                    _incoming.Enqueue(advertisement);
                }
                catch (ObjectDisposedException)
                {
                    break; // socket closed by StopBrowsing — expected exit path
                }
                catch (SocketException)
                {
                    break; // socket closed mid-Receive — same expected exit path on some platforms
                }
                catch (Exception ex)
                {
                    if (_stopRequested) break; // socket torn down mid-Receive by StopBrowsing — expected, not an error

                    // Malformed datagram (foreign broadcaster on 47777, truncated packet, etc.) —
                    // never let one bad packet kill discovery for the rest of the session.
                    UnityEngine.Debug.LogWarning($"LanSessionScanner: dropped malformed advertisement ({ex.Message}).");
                }
            }
        }
    }
}
