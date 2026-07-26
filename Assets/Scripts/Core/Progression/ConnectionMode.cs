namespace BlueWaterRiptide.Core.Progression
{
    /// <summary>
    /// Trust tier a match result was earned under (Plan 05 §1). Online modes are deferred
    /// (Plan 03) but included now so ProgressionService's rate table never needs reshaping later.
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
