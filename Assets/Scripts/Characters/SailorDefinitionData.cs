using BlueWaterRiptide.Gameplay.Combat;

namespace BlueWaterRiptide.Characters
{
    /// <summary>
    /// Plain C# stat block for a Sailor. Not a ScriptableObject — this prototype creates
    /// definitions purely from code, no Unity asset involved. BasicAttack/Super are composed
    /// from reusable IAbilityBehavior instances (00 §7.2) — adding a Sailor never touches SailorPawn.
    /// </summary>
    public class SailorDefinitionData
    {
        public string Id;
        public string DisplayName;
        public AttackArchetype Archetype;
        public float MaxHP;
        public float MoveSpeed;

        /// <summary>Generic "how close/far this Sailor wants to be" used by AI/bootstrap; not read by the behaviors themselves.</summary>
        public float AttackRange;

        public int MaxAmmo;
        public float ReloadTime;

        public bool KnockbackImmune;
        public float SuperChargeThreshold = 500f;

        public IAbilityBehavior BasicAttack;
        public IAbilityBehavior Super;

        public static SailorDefinitionData EnsignAce => new SailorDefinitionData
        {
            Id = "sailor.ace",
            DisplayName = "Ensign Ace",
            Archetype = AttackArchetype.Serve,
            MaxHP = 1300f,
            MoveSpeed = 6f,
            AttackRange = 14f,
            MaxAmmo = 3,
            ReloadTime = 1.7f,
            KnockbackImmune = false,
            BasicAttack = new ProjectileAbilityBehavior(damage: 140f, speed: 25f, range: 14f),
            // Super — Ace Barrage: five fast serves in a widening fan, each dealing basic-attack
            // damage plus moderate knockback (00 §3).
            Super = new FanProjectileAbilityBehavior(projectileCount: 5, spreadDegrees: 40f, damage: 140f, speed: 25f, range: 14f, knockbackForce: 5f)
        };

        public static SailorDefinitionData AdmiralAnchor => new SailorDefinitionData
        {
            Id = "sailor.anchor",
            DisplayName = "Admiral Anchor",
            Archetype = AttackArchetype.Spike,
            MaxHP = 1900f,
            MoveSpeed = 3.5f,
            AttackRange = 4f,
            MaxAmmo = 3,
            ReloadTime = 2.4f,
            KnockbackImmune = true, // Trait — Ballast
            SuperChargeThreshold = 550f,
            BasicAttack = new MeleeArcAbilityBehavior(damage: 190f, radius: 4f, coneAngleDegrees: 100f, knockbackForce: 8f),
            Super = new LeapSlamAuraAbilityBehavior(leapRange: 10f, impactRadius: 3f, impactDamage: 200f, auraRadius: 5f, auraDuration: 6f, auraDamageReductionFraction: 0.4f)
        };
    }
}
