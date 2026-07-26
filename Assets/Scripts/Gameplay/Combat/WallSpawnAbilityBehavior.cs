using UnityEngine;
using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.Gameplay.Combat
{
    /// <summary>
    /// Block-archetype primitive (Plan 00 §2) — spawns a line of temporary wall segments
    /// perpendicular to the caster's aim, each carrying a SpawnedWall marker, auto-destroyed
    /// after Duration seconds. No v1 Sailor is assigned this yet (Block-archetype Sailors are
    /// post-v1); ships dormant, validated only by code review until an on-device pass exercises
    /// it. Segments block Projectile (see Projectile.OnTriggerEnter) but nothing yet makes
    /// SailorPawn movement path around one — that needs real arena geometry (item 4, Editor/Asset)
    /// to be worth building, so today a Sailor can walk straight through their own wall even
    /// though a projectile can't pass through it.
    /// </summary>
    public sealed class WallSpawnAbilityBehavior : IAbilityBehavior
    {
        readonly int _segmentCount;
        readonly float _segmentWidth;
        readonly float _segmentHeight;
        readonly float _spawnDistance;
        readonly float _duration;

        public WallSpawnAbilityBehavior(int segmentCount, float segmentWidth, float segmentHeight, float spawnDistance, float duration)
        {
            _segmentCount = segmentCount;
            _segmentWidth = segmentWidth;
            _segmentHeight = segmentHeight;
            _spawnDistance = spawnDistance;
            _duration = duration;
        }

        public void Execute(SailorPawn owner)
        {
            if (_segmentCount <= 0) return;

            Vector3 flatForward = FlattenAndNormalize(owner.transform.forward);
            Vector3 perpendicular = new Vector3(-flatForward.z, 0f, flatForward.x);
            Vector3 center = owner.transform.position + flatForward * _spawnDistance;

            float totalWidth = _segmentCount * _segmentWidth;
            float startOffset = -(totalWidth - _segmentWidth) / 2f;

            for (int i = 0; i < _segmentCount; i++)
            {
                Vector3 segmentPos = center + perpendicular * (startOffset + i * _segmentWidth);
                segmentPos.y = _segmentHeight / 2f;
                SpawnSegment(segmentPos, perpendicular, owner);
            }
        }

        void SpawnSegment(Vector3 position, Vector3 perpendicular, SailorPawn owner)
        {
            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.name = "SpawnedWallSegment";
            segment.transform.position = position;
            segment.transform.rotation = Quaternion.LookRotation(perpendicular, Vector3.up);
            segment.transform.localScale = new Vector3(_segmentWidth, _segmentHeight, 0.5f);

            var renderer = segment.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = SailorPawn.CreateUrpMaterial(owner.Team == Team.A ? Color.cyan : Color.magenta);
            }

            segment.AddComponent<SpawnedWall>();
            Object.Destroy(segment, _duration);
        }

        static Vector3 FlattenAndNormalize(Vector3 direction)
        {
            Vector3 flat = new Vector3(direction.x, 0f, direction.z);
            return flat.sqrMagnitude > 0.0001f ? flat.normalized : Vector3.forward;
        }
    }
}
