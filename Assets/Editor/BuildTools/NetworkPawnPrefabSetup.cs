using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Networking.Runtime;

namespace BlueWaterRiptide.EditorTools
{
    /// <summary>
    /// Bakes the two runtime network prefabs LanMatchBootstrap needs as real prefab assets under
    /// Resources (Plan 10 M1). NGO can only resolve a NetworkObject spawn on a client if the
    /// spawning prefab was registered with a GlobalObjectIdHash both peers agree on, and that hash
    /// (NetworkObject.OnValidate) is only ever generated for an on-disk prefab asset — a purely
    /// runtime `new GameObject()` + `AddComponent&lt;NetworkObject&gt;()` (this project's usual
    /// M1PrototypeBootstrap pattern) has no such hash and can't be resolved on a second peer. Mirrors
    /// ArenaAuthoring's build-then-SaveAsPrefabAsset pattern; idempotent, safe to re-run.
    /// </summary>
    public static class NetworkPawnPrefabSetup
    {
        const string OutputFolder = "Assets/Resources/Networking";
        const string PawnPrefabPath = OutputFolder + "/NetworkedPawn.prefab";
        const string MatchStatePrefabPath = OutputFolder + "/NetworkMatchState.prefab";

        [MenuItem("Tools/Blue Water Riptide/Build LAN Network Prefabs")]
        public static void BuildLanNetworkPrefabs()
        {
            Directory.CreateDirectory(OutputFolder);

            BuildPawnPrefab();
            BuildMatchStatePrefab();

            AssetDatabase.Refresh();
        }

        static void BuildPawnPrefab()
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "NetworkedPawn";
            try
            {
                root.GetComponent<Renderer>().material = CreateUrpMaterial(new Color(0.8f, 0.8f, 0.8f));

                // Order matters only for RequireComponent auto-add avoidance: SailorPawn (and its
                // own [RequireComponent(typeof(Collider))], already satisfied by CreatePrimitive)
                // and NetworkInputDriver both need to exist before NetworkPawnPresenter is added, so
                // its own [RequireComponent] attributes find them already present instead of
                // triggering a second, ordering-order-dependent auto-add.
                root.AddComponent<NetworkObject>();
                root.AddComponent<NetworkTransform>();
                root.AddComponent<SailorPawn>();
                root.AddComponent<NetworkSailorState>();
                root.AddComponent<NetworkInputDriver>();
                root.AddComponent<NetworkPawnPresenter>();

                Save(root, PawnPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static void BuildMatchStatePrefab()
        {
            var root = new GameObject("NetworkMatchState");
            try
            {
                root.AddComponent<NetworkObject>();
                root.AddComponent<NetworkMatchState>();

                Save(root, MatchStatePrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static void Save(GameObject root, string path)
        {
            bool existed = File.Exists(path);
            var saved = PrefabUtility.SaveAsPrefabAsset(root, path, out bool success);

            if (!success || saved == null)
            {
                Debug.LogError($"BWR_SETUP_FAILED: could not save network prefab to {path}");
                return;
            }

            AssetDatabase.SaveAssets();

            var networkObject = saved.GetComponent<NetworkObject>();
            Debug.Log($"BWR_SETUP_OK: {(existed ? "re-saved" : "created")} {path} (PrefabIdHash={networkObject.PrefabIdHash}).");
        }

        /// <summary>
        /// SailorPawn.CreateUrpMaterial (Assets/Scripts/Characters/SailorPawn.cs) is declared
        /// `internal`, which only grants access within its own compiled assembly — this project has
        /// no asmdef/InternalsVisibleTo bridging the default runtime assembly to the Editor assembly
        /// this file compiles into, so that helper isn't reachable from here (same duplication
        /// ArenaAuthoring.cs already made for the same reason).
        /// </summary>
        static Material CreateUrpMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            return new Material(shader) { color = color };
        }
    }
}
