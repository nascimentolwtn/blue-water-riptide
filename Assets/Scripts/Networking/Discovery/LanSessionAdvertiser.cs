using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace BlueWaterRiptide.Networking.Discovery
{
    /// <summary>
    /// Host-side UDP broadcaster (Plan 02 §2). Sends the current SessionAdvertisement to
    /// 255.255.255.255:BroadcastPort every AdvertiseInterval seconds while active. No Netcode
    /// for GameObjects dependency — plain UDP, driven by an external Tick() call so it stays
    /// usable without a MonoBehaviour of its own.
    /// </summary>
    public sealed class LanSessionAdvertiser : IDisposable
    {
        public const int BroadcastPort = 47777;
        public const float AdvertiseInterval = 1f;

        readonly UdpClient _client;
        readonly IPEndPoint _broadcastEndPoint;
        float _timeSinceLastSend;

        public bool IsActive { get; private set; }
        public SessionAdvertisement Advertisement { get; set; }

        public LanSessionAdvertiser()
        {
            _client = new UdpClient { EnableBroadcast = true };
            _broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, BroadcastPort);
        }

        /// <summary>Begins advertising. Call Tick(deltaTime) every frame afterward to actually send.</summary>
        public void Start(SessionAdvertisement advertisement)
        {
            Advertisement = advertisement;
            IsActive = true;
            _timeSinceLastSend = AdvertiseInterval;
        }

        /// <summary>Stops advertising. Call when the lobby closes or the match starts.</summary>
        public void Stop() => IsActive = false;

        public void Tick(float deltaTime)
        {
            if (!IsActive || Advertisement == null) return;

            _timeSinceLastSend += deltaTime;
            if (_timeSinceLastSend < AdvertiseInterval) return;

            _timeSinceLastSend = 0f;
            Send();
        }

        void Send()
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(Advertisement.ToJson());
                _client.Send(bytes, bytes.Length, _broadcastEndPoint);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"LanSessionAdvertiser: broadcast failed: {e.Message}");
            }
        }

        public void Dispose() => _client.Dispose();
    }
}
