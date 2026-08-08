using UnityEngine;
using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Core;
using BlueWaterRiptide.Gameplay.Court;

namespace BlueWaterRiptide.Gameplay.Combat
{
    /// <summary>
    /// Reusable wall/obstacle primitive (00 §7.2's "wall" ability shape): spawns a temporary solid
    /// obstacle a fixed distance in front of the caster that blocks projectiles for its lifetime,
    /// then despawns. Sailor movement doesn't yet resolve against arbitrary colliders (SailorPawn
    /// clamps to arena bounds only, no obstacle avoidance) — this primitive is projectile-blocking
    /// only until that lands; revisit once real arena geometry (backlog item 4) exists to drive it.
    /// </summary>
    public sealed class WallSpawnAbilityBehavior : IAbilityBehavior
    {
        readonly float _spawnDistance;
        readonly float _width;
        readonly float _height;
        readonly float _thickness;
        readonly float _duration;

        public WallSpawnAbilityBehavior(float spawnDistance, float width, float height, float thickness, float duration)
        {
            _spawnDistance = spawnDistance;
            _width = width;
            _height = height;
            _thickness = thickness;
            _duration = duration;
        }

        public void Execute(SailorPawn owner)
        {
            Vector3 flatForward = FlattenAndNormalize(owner.transform.forward);
            Vector3 center = owner.transform.position + flatForward * _spawnDistance;
            center.x = Mathf.Clamp(center.x, -owner.ArenaHalfExtents.x, owner.ArenaHalfExtents.x);
            center.z = Mathf.Clamp(center.z, -owner.ArenaHalfExtents.y, owner.ArenaHalfExtents.y);
            center.y = _height * 0.5f;

            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "TemporaryWall";
            wall.transform.position = center;
            wall.transform.rotation = Quaternion.LookRotation(flatForward, Vector3.up);
            wall.transform.localScale = new Vector3(_width, _height, _thickness);
            wall.AddComponent<TemporaryObstacle>();

            var wallCollider = wall.GetComponent<Collider>();
            if (wallCollider != null) wallCollider.isTrigger = true; // Projectile hit detection is trigger-based (see Projectile.OnTriggerEnter)

            var renderer = wall.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = SailorPawn.CreateUrpMaterial(owner.Team == Team.A ? Color.cyan : Color.magenta);
            }

            Object.Destroy(wall, _duration);
        }

        static Vector3 FlattenAndNormalize(Vector3 direction)
        {
            Vector3 flat = new Vector3(direction.x, 0f, direction.z);
            return flat.sqrMagnitude > 0.0001f ? flat.normalized : Vector3.forward;
        }
    }
}
