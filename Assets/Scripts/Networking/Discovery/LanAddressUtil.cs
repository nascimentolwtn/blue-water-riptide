using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace BlueWaterRiptide.Networking.Discovery
{
    /// <summary>
    /// Shared IPv4-on-this-LAN lookup. Small and standalone because both the advertiser (fills the
    /// "ip" wire field) and any future "show my address for Join by IP" UI need the same answer,
    /// and there's no single obviously-correct Unity API for it (Application/Device APIs don't
    /// expose LAN IP; this walks NetworkInterface like a plain .NET app would).
    /// </summary>
    public static class LanAddressUtil
    {
        /// <summary>Best-effort local IPv4 address on an active, non-loopback network interface. Returns
        /// null if none is found (e.g. airplane mode) — callers should treat that as "can't host yet".</summary>
        public static string GetLocalIPv4()
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up) continue;
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                var props = nic.GetIPProperties();
                foreach (var addr in props.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return addr.Address.ToString();
                    }
                }
            }

            return null;
        }
    }
}
