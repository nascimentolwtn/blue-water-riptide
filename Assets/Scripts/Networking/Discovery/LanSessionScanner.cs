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
    /// Receives arrive on a background IO thread (the BeginReceive/EndReceive callback chain),
    /// which may only touch plain BCL types — raw datagram bytes are queued there and every
    /// Unity-touching step (JsonUtility parsing included, since it is not safely callable off
    /// the main thread despite some docs implying otherwise) happens in Tick(), which callers
    /// must only invoke from the main thread. DateTime.UtcNow (not UnityEngine.Time, which
    /// throws off-main-thread) times the received-on-background-thread entries. Android callers
    /// must hold a WifiManager.MulticastLock (Editor/Asset-bound, done where the scanner is
    /// started) or broadcasts won't arrive on many devices.
    /// </summary>
    public sealed class LanSessionScanner : IDisposable
    {
        public const float ExpireAfterSeconds = 3f;

        readonly UdpClient _client;
        readonly object _lock = new object();
        readonly Dictionary<string, DiscoveredSession> _sessions = new Dictionary<string, DiscoveredSession>();
        readonly Queue<byte[]> _pendingDatagrams = new Queue<byte[]>();

        bool _receivePending;

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

        /// <summary>Call every frame/tick, from the main thread, to parse queued datagrams and expire stale sessions.</summary>
        public void Tick()
        {
            if (!IsActive) return;

            DrainPendingDatagrams();

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

        void DrainPendingDatagrams()
        {
            List<byte[]> batch;
            lock (_lock)
            {
                if (_pendingDatagrams.Count == 0) return;
                batch = new List<byte[]>(_pendingDatagrams);
                _pendingDatagrams.Clear();
            }

            foreach (var bytes in batch) TryStore(bytes);
        }

        void BeginReceive()
        {
            if (_receivePending) return;

            try
            {
                _receivePending = true;
                _client.BeginReceive(OnReceive, null);
            }
            catch (ObjectDisposedException)
            {
                _receivePending = false;
            }
        }

        // Runs on a background IO thread. Must not touch UnityEngine APIs (JsonUtility included).
        void OnReceive(IAsyncResult result)
        {
            _receivePending = false;

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
                lock (_lock) _pendingDatagrams.Enqueue(bytes);
                BeginReceive();
            }
        }

        // Runs on the main thread (called from Tick via DrainPendingDatagrams) — safe to touch JsonUtility here.
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
