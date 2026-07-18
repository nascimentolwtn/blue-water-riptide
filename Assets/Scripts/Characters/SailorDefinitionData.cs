namespace BlueWaterRiptide.Characters
{
    /// <summary>
    /// Plain C# stat block for a Sailor. Not a ScriptableObject — this prototype creates
    /// definitions purely from code, no Unity asset involved.
    /// </summary>
    public class SailorDefinitionData
    {
        public string Id;
        public string DisplayName;
        public float MaxHP;
        public float MoveSpeed;
        public float AttackDamage;
        public float AttackRange;
        public float ProjectileSpeed;
        public int MaxAmmo;
        public float ReloadTime;

        public static SailorDefinitionData EnsignAce => new SailorDefinitionData
        {
            Id = "sailor.ace",
            DisplayName = "Ensign Ace",
            MaxHP = 1300f,
            MoveSpeed = 6f,
            AttackDamage = 140f,
            AttackRange = 14f,
            ProjectileSpeed = 25f,
            MaxAmmo = 3,
            ReloadTime = 1.7f
        };
    }
}
