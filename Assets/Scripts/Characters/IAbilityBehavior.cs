namespace BlueWaterRiptide.Characters
{
    /// <summary>
    /// A composable basic-attack or Super effect. SailorPawn only ever calls Execute — it never
    /// knows which Sailor or archetype it's running, so adding a Sailor's kit never touches SailorPawn.
    /// </summary>
    public interface IAbilityBehavior
    {
        void Execute(SailorPawn owner);
    }
}
