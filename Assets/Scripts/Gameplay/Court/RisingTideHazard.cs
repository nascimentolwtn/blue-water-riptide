using UnityEngine;

namespace BlueWaterRiptide.Gameplay.Court
{
    /// <summary>
    /// Sudden Death "Rising Tide" hazard geometry: a rectangular safe zone (centered on the arena
    /// origin) that steps inward every ring interval once fed elapsed Sudden Death time via Tick.
    /// Pure C# — no MonoBehaviour/SailorPawn references, so Gameplay/Court stays decoupled from
    /// Characters; damage application (DOT) is wired at the host layer (MatchLoopHost).
    ///
    /// This is a deliberately simplified concentric-rect inset stand-in for 00 §5's "floods inward
    /// from the south inlet and outer boundary" — acceptable for a greybox arena with no real
    /// geometry yet (Plan 08 risk notes); revisit once the real Tideline Cove exists.
    /// </summary>
    public sealed class RisingTideHazard
    {
        readonly Vector2 _startHalfExtents;
        readonly float _ringInterval;
        readonly float _shrinkStep;
        readonly float _minHalfExtent;

        /// <summary>Current safe-zone half-extents on the XZ plane, recomputed by Tick.</summary>
        public Vector2 CurrentHalfExtents { get; private set; }

        public RisingTideHazard(Vector2 arenaHalfExtents, float ringInterval, float shrinkStep, float minHalfExtent = 2f)
        {
            _startHalfExtents = arenaHalfExtents;
            _ringInterval = ringInterval > 0f ? ringInterval : 5f;
            _shrinkStep = shrinkStep;
            _minHalfExtent = Mathf.Max(0.01f, minHalfExtent);

            CurrentHalfExtents = arenaHalfExtents;
        }

        /// <summary>Recomputes the safe-zone rect for the given elapsed-since-Sudden-Death-began time (seconds).</summary>
        public void Tick(float elapsedSeconds)
        {
            int ringsElapsed = Mathf.Max(0, Mathf.FloorToInt(elapsedSeconds / _ringInterval));
            float shrinkAmount = ringsElapsed * _shrinkStep;

            float halfX = Mathf.Max(_minHalfExtent, _startHalfExtents.x - shrinkAmount);
            float halfZ = Mathf.Max(_minHalfExtent, _startHalfExtents.y - shrinkAmount);
            CurrentHalfExtents = new Vector2(halfX, halfZ);
        }

        /// <summary>Current safe-zone rect on the XZ plane, expressed as a Rect (x/y here map to world X/Z).</summary>
        public Rect GetSafeZoneRect()
        {
            return new Rect(-CurrentHalfExtents.x, -CurrentHalfExtents.y, CurrentHalfExtents.x * 2f, CurrentHalfExtents.y * 2f);
        }

        public bool IsPositionSafe(Vector3 worldPosition)
        {
            return Mathf.Abs(worldPosition.x) <= CurrentHalfExtents.x && Mathf.Abs(worldPosition.z) <= CurrentHalfExtents.y;
        }
    }
}
