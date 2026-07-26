using UnityEngine;

namespace BlueWaterRiptide.Gameplay.Combat
{
    /// <summary>Marker on a temporary wall segment spawned by WallSpawnAbilityBehavior — lets other
    /// systems (e.g. Projectile) recognize a spawned obstacle without a tag string.</summary>
    public sealed class SpawnedWall : MonoBehaviour
    {
    }
}
