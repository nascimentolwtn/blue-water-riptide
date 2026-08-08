using BlueWaterRiptide.Networking;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlueWaterRiptide.EditorTools
{
    /// <summary>
    /// Ensures Boot.unity carries a single, properly-wired NetworkManager + UnityTransport pair —
    /// the one piece of hand-authored scene wiring Local Network mode needs before
    /// LanSessionLauncher can run at all (see .claude/plans/02-local-network-mode.md). NGO expects
    /// exactly one persistent NetworkManager singleton; Boot is this project's only scene, and
    /// M1PrototypeBootstrap already spawns everything else procedurally at runtime, so this is
    /// simply where that singleton belongs. Never touches an already-present NetworkManager —
    /// someone may have hand-tuned it. Idempotent — safe to re-run.
    /// </summary>
    public static class NetworkSceneSetup
    {
        const string BootScenePath = "Assets/Scenes/Boot.unity";
        const string NetworkManagerObjectName = "NetworkManager";

        [MenuItem("Tools/Blue Water Riptide/Ensure NetworkManager In Scene")]
        public static void EnsureNetworkManagerInScene()
        {
            Scene scene;
            try
            {
                scene = EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Single);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"BWR_SETUP_FAILED: could not open {BootScenePath} ({e.Message}).");
                return;
            }

            if (!scene.IsValid())
            {
                Debug.LogError($"BWR_SETUP_FAILED: {BootScenePath} did not open to a valid scene.");
                return;
            }

            // Don't touch an existing NetworkManager — a hand-tuned config left in the scene by
            // someone else takes priority over whatever placeholder defaults we'd create here.
            var existing = Object.FindFirstObjectByType<NetworkManager>();
            if (existing != null)
            {
                Debug.Log($"BWR_SETUP_OK: NetworkManager already present in {BootScenePath} ('{existing.gameObject.name}') — left untouched.");
                return;
            }

            var networkManagerObject = new GameObject(NetworkManagerObjectName);
            var networkManager = networkManagerObject.AddComponent<NetworkManager>();
            var transport = networkManagerObject.AddComponent<UnityTransport>();

            // Placeholder only — a real host IP gets set later at runtime by LanSessionLauncher.
            NetworkBootstrap.ConfigureDirect(transport, "127.0.0.1", NetworkBootstrap.DefaultPort);

            AssignTransportToConfig(networkManager, transport);

            // NGO's NetworkManager self-manages DontDestroyOnLoad from its own Awake()/OnEnable()
            // once the game is actually running — calling Object.DontDestroyOnLoad here would be a
            // no-op at best (that API only applies in Play mode, not while authoring a scene asset).

            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);

            if (saved)
            {
                Debug.Log($"BWR_SETUP_OK: created '{NetworkManagerObjectName}' with NetworkManager + UnityTransport in {BootScenePath} and saved the scene.");
            }
            else
            {
                Debug.LogError($"BWR_SETUP_FAILED: created '{NetworkManagerObjectName}' but failed to save {BootScenePath}.");
            }
        }

        /// <summary>
        /// Wires NetworkConfig.NetworkTransport via SerializedObject rather than a direct field
        /// reference. This script is authored without a live Editor available to confirm NGO
        /// 2.13.0's exact serialized field names, so a missing/renamed property degrades to a
        /// logged warning (leaving the transport to be assigned by hand in the Inspector) instead
        /// of a compile-time guess that could fail to build, or a runtime exception.
        /// </summary>
        static void AssignTransportToConfig(NetworkManager networkManager, UnityTransport transport)
        {
            var serializedManager = new SerializedObject(networkManager);

            var configProperty = serializedManager.FindProperty("NetworkConfig");
            if (configProperty == null)
            {
                Debug.LogWarning("BWR_SETUP: NetworkManager.NetworkConfig serialized property not found — created the UnityTransport component but could not auto-assign it; set it by hand on the NetworkManager's Network Transport field in the Inspector.");
                return;
            }

            var transportProperty = configProperty.FindPropertyRelative("NetworkTransport");
            if (transportProperty == null)
            {
                Debug.LogWarning("BWR_SETUP: NetworkConfig.NetworkTransport serialized property not found — created the UnityTransport component but could not auto-assign it; set it by hand on the NetworkManager's Network Transport field in the Inspector.");
                return;
            }

            transportProperty.objectReferenceValue = transport;
            serializedManager.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
