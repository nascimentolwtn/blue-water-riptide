using UnityEngine;

namespace BlueWaterRiptide.Gameplay.Court
{
    /// <summary>
    /// Marker for a temporary blocking obstacle spawned by an ability (e.g. WallSpawnAbilityBehavior).
    /// Projectiles check for this component to know they've hit a wall rather than a Sailor.
    /// </summary>
    public sealed class TemporaryObstacle : MonoBehaviour
    {
    }
}
