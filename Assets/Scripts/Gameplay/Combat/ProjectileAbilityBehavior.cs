using BlueWaterRiptide.Characters;

namespace BlueWaterRiptide.Gameplay.Combat
{
    /// <summary>Ensign Ace's Jump Serve — straight-line ammo-based projectile. First reusable ability behavior.</summary>
    public sealed class ProjectileAbilityBehavior : IAbilityBehavior
    {
        readonly float _damage;
        readonly float _speed;
        readonly float _range;

        public ProjectileAbilityBehavior(float damage, float speed, float range)
        {
            _damage = damage;
            _speed = speed;
            _range = range;
        }

        public void Execute(SailorPawn owner)
        {
            Projectile.Spawn(owner, owner.transform.forward, _damage, _speed, _range);
        }
    }
}
