using System.IO;
using UnityEditor;
using UnityEngine;
using BlueWaterRiptide.Core;

namespace BlueWaterRiptide.EditorTools
{
    /// <summary>
    /// Bakes the "Tideline Cove" greybox that M1PrototypeBootstrap currently rebuilds from
    /// scratch every Play (see BuildGround/BuildPawnObject there) into a real, inspectable
    /// prefab asset under Resources, so it can eventually be Resources.Load'ed instead of
    /// re-generated at runtime. All geometry is derived from ArenaCatalog's data so the
    /// prefab can't drift out of sync with the numbers the match loop and Rising Tide hazard
    /// actually use. Idempotent — safe to re-run; overwrites the prefab with fresh geometry
    /// from the current catalog values each time.
    /// </summary>
    public static class ArenaAuthoring
    {
        const string OutputFolder = "Assets/Resources/Environment";
        const string PrefabPath = OutputFolder + "/TidelineCove.prefab";

        // BuildGround's runtime placeholder scales Unity's default 10x10 Plane to ~34x20 around
        // ArenaCatalog's 16.5x9.5 half-extents (33x19 full) — i.e. it deliberately extends ~1 unit
        // past the enforced play area on each edge rather than clipping exactly to it. This keeps
        // that same "floor reads slightly larger than the play area" relationship, but derives it
        // from HalfExtents directly (via a margin constant) instead of hardcoding an absolute scale,
        // so it stays correct if HalfExtents ever changes.
        const float GroundMargin = 2f;

        const float WallThickness = 0.3f;
        const float WallHeight = 1f;

        const float SpawnPadDiameter = 2f;
        const float SpawnPadHeight = 0.1f;

        [MenuItem("Tools/Blue Water Riptide/Build Tideline Cove Arena Prefab")]
        public static void BuildTidelineCoveArenaPrefab()
        {
            Directory.CreateDirectory(OutputFolder);

            ArenaDefinition arena = ArenaCatalog.Get(ArenaCatalog.TidelineCoveId);

            var root = new GameObject("TidelineCoveArena");
            try
            {
                BuildGround(root.transform, arena);
                BuildSpawnPad(root.transform, "SpawnPad_TeamA", arena.SpawnPositionsTeamA[0], Color.cyan);
                BuildSpawnPad(root.transform, "SpawnPad_TeamB", arena.SpawnPositionsTeamB[0], Color.magenta);
                BuildBoundary(root.transform, arena);

                bool prefabExisted = File.Exists(PrefabPath);
                var saved = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out bool success);

                if (!success || saved == null)
                {
                    Debug.LogError($"BWR_SETUP_FAILED: could not save Tideline Cove arena prefab to {PrefabPath}");
                    return;
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"BWR_SETUP_OK: Tideline Cove arena prefab {(prefabExisted ? "re-saved" : "created")} at {PrefabPath}.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static void BuildGround(Transform parent, ArenaDefinition arena)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(parent);
            ground.transform.position = Vector3.zero;

            // Unity's default Plane mesh is 10x10 world units at scale 1, so dividing the desired
            // full-size footprint (half-extents * 2, plus the margin above) by 10 gives the scale
            // that reproduces it, kept in lockstep with ArenaCatalog instead of a hardcoded value.
            float fullX = arena.HalfExtents.x * 2f + GroundMargin;
            float fullZ = arena.HalfExtents.y * 2f + GroundMargin;
            ground.transform.localScale = new Vector3(fullX / 10f, 1f, fullZ / 10f);

            ground.GetComponent<Renderer>().material = CreateUrpMaterial(new Color(0.2f, 0.45f, 0.65f));
        }

        static void BuildSpawnPad(Transform parent, string name, Vector3 spawnPosition, Color tint)
        {
            // Spawn positions carry a pawn pivot height (y=1 for the capsule prototype) that has
            // nothing to do with a flat ground marker's thickness — only the XZ position is shared
            // with the pawn spawn point, the pad's own height is its own small constant.
            var pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pad.name = name;
            pad.transform.SetParent(parent);
            pad.transform.position = new Vector3(spawnPosition.x, SpawnPadHeight * 0.5f, spawnPosition.z);
            pad.transform.localScale = new Vector3(SpawnPadDiameter, SpawnPadHeight, SpawnPadDiameter);
            pad.GetComponent<Renderer>().material = CreateUrpMaterial(tint);
        }

        static void BuildBoundary(Transform parent, ArenaDefinition arena)
        {
            var boundaryMaterial = CreateUrpMaterial(Color.gray);
            float hx = arena.HalfExtents.x;
            float hz = arena.HalfExtents.y;

            // North/south walls span the full width (plus corner overlap via WallThickness);
            // east/west walls span only the depth between them, so the four thin cubes read as a
            // closed rectangle outline at the HalfExtents edge without double-covering corners.
            BuildWall(parent, "Boundary_North", new Vector3(0f, WallHeight * 0.5f, hz), new Vector3(hx * 2f + WallThickness, WallHeight, WallThickness), boundaryMaterial);
            BuildWall(parent, "Boundary_South", new Vector3(0f, WallHeight * 0.5f, -hz), new Vector3(hx * 2f + WallThickness, WallHeight, WallThickness), boundaryMaterial);
            BuildWall(parent, "Boundary_East", new Vector3(hx, WallHeight * 0.5f, 0f), new Vector3(WallThickness, WallHeight, hz * 2f), boundaryMaterial);
            BuildWall(parent, "Boundary_West", new Vector3(-hx, WallHeight * 0.5f, 0f), new Vector3(WallThickness, WallHeight, hz * 2f), boundaryMaterial);
        }

        static void BuildWall(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().material = material;
        }

        /// <summary>
        /// SailorPawn.CreateUrpMaterial (Assets/Scripts/Characters/SailorPawn.cs) is declared
        /// `internal`, which only grants access within its own compiled assembly — this project has
        /// no asmdef/InternalsVisibleTo bridging the default runtime assembly to the Editor assembly
        /// this file compiles into, so that helper isn't reachable from here. Duplicated with the
        /// same shader-lookup logic rather than widening SailorPawn's public surface for one caller.
        /// </summary>
        static Material CreateUrpMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            return new Material(shader) { color = color };
        }
    }
}
