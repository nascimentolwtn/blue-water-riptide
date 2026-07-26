namespace BlueWaterRiptide.Core
{
    /// <summary>
    /// Which connection mode a Session was built under (Plan 00 §7.5) — also the trust tier a
    /// match result is earned under for Progression's trophy rate table (Plan 05 §1). Online
    /// modes are deferred (Plan 03) but included now so neither Session nor the rate table needs
    /// reshaping later.
    /// </summary>
    public enum ConnectionMode
    {
        SinglePlayer,
        LanCoop,
        LanPvp,
        OnlineSolo,
        OnlineFriendLobby
    }
}
