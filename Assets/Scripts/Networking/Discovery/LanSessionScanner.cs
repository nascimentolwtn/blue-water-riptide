using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace BlueWaterRiptide.Networking.Discovery
{
    /// <summary>One session seen on the network, plus when it was last re-advertised.</summary>
    public readonly struct DiscoveredSession
    {
        public readonly SessionAdvertisement Advertisement;
        public readonly DateTime LastSeenUtc;

        public DiscoveredSession(SessionAdvertisement advertisement, DateTime lastSeenUtc)
        {
            Advertisement = advertisement;
            LastSeenUtc = lastSeenUtc;
        }
    }

    /// <summary>
    /// Client-side UDP listener (Plan 02 §2). Listens on LanSessionAdvertiser.BroadcastPort,
    /// aggregates advertisements keyed by ip:port, and expires entries not re-seen within
    /// ExpireAfterSeconds. No Netcode for GameObjects dependency — plain UDP.
    /// Receives arrive on a background IO thread (BeginReceive callback), so all shared state is
    /// guarded by a lock and nothing here touches UnityEngine.Time (main-thread-only) off the
    /// main thread — DateTime.UtcNow is used instead. Android callers must hold a
    /// WifiManager.MulticastLock (Editor/Asset-bound, done where the scanner is started) or
    /// broadcasts won't arrive on many devices.
    /// </summary>
    public sealed class LanSessionScanner : IDisposable
    {
        public const float ExpireAfterSeconds = 3f;

        readonly UdpClient _client;
        readonly object _lock = new object();
        readonly Dictionary<string, DiscoveredSession> _sessions = new Dictionary<string, DiscoveredSession>();

        public bool IsActive { get; private set; }

        public LanSessionScanner()
        {
            _client = new UdpClient(AddressFamily.InterNetwork);
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.Client.Bind(new IPEndPoint(IPAddress.Any, LanSessionAdvertiser.BroadcastPort));
        }

        public void Start()
        {
            if (IsActive) return;

            IsActive = true;
            lock (_lock) _sessions.Clear();
            BeginReceive();
        }

        public void Stop()
        {
            IsActive = false;
            lock (_lock) _sessions.Clear();
        }

        /// <summary>Snapshot of currently-live sessions. Safe to call from the main thread at any time.</summary>
        public IReadOnlyList<DiscoveredSession> GetSessions()
        {
            lock (_lock) return _sessions.Values.ToList();
        }

        /// <summary>Call every frame/tick to expire sessions not re-advertised recently.</summary>
        public void Tick()
        {
            if (!IsActive) return;

            var now = DateTime.UtcNow;
            lock (_lock)
            {
                List<string> stale = null;
                foreach (var kvp in _sessions)
                {
                    if ((now - kvp.Value.LastSeenUtc).TotalSeconds > ExpireAfterSeconds)
                    {
                        (stale ??= new List<string>()).Add(kvp.Key);
                    }
                }

                if (stale != null)
                {
                    foreach (var key in stale) _sessions.Remove(key);
                }
            }
        }

        void BeginReceive()
        {
            try
            {
                _client.BeginReceive(OnReceive, null);
            }
            catch (ObjectDisposedException)
            {
                // Disposed while a receive was in flight — nothing left to do.
            }
        }

        void OnReceive(IAsyncResult result)
        {
            IPEndPoint remoteEndPoint = null;
            byte[] bytes;
            try
            {
                bytes = _client.EndReceive(result, ref remoteEndPoint);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"LanSessionScanner: receive failed: {e.Message}");
                if (IsActive) BeginReceive();
                return;
            }

            if (IsActive)
            {
                TryStore(bytes);
                BeginReceive();
            }
        }

        void TryStore(byte[] bytes)
        {
            try
            {
                var json = Encoding.UTF8.GetString(bytes);
                var advertisement = SessionAdvertisement.FromJson(json);
                if (advertisement == null || string.IsNullOrEmpty(advertisement.ip)) return;

                string key = $"{advertisement.ip}:{advertisement.port}";
                lock (_lock) _sessions[key] = new DiscoveredSession(advertisement, DateTime.UtcNow);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"LanSessionScanner: malformed advertisement: {e.Message}");
            }
        }

        public void Dispose() => _client.Dispose();
    }
}
