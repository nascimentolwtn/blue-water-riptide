namespace BlueWaterRiptide.Networking
{
    /// <summary>
    /// Wire-format version shared by every piece of the LAN transport (advertisements today,
    /// connection-approval payloads once that lands). Bump this constant any time a networked
    /// payload shape changes so mismatched builds show "version mismatch" instead of a silent
    /// desync or a serialization exception on a device that can't be patched mid-session.
    /// </summary>
    public static class NetworkProtocol
    {
        public const int CurrentVersion = 1;
    }
}
