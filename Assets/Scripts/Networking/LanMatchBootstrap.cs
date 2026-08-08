using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Core;
using BlueWaterRiptide.Networking.Runtime;
using BlueWaterRiptide.Networking.Session;

namespace BlueWaterRiptide.Networking
{
    /// <summary>
    /// Plan 10 M1 "wire-up spike" bootstrap: once the lobby's host presses Start Match, this builds
    /// the arena/camera/lighting the same way M1PrototypeBootstrap does (duplicated rather than
    /// reusing its private/static helpers — see that class's doc comment; this is Networking-owned,
    /// not a Core/Gameplay change), spawns one real NetworkObject pawn per participant, and drives a
    /// host-only MatchController whose events LanMatchLoopHost mirrors into NetworkMatchState /
    /// NetworkSailorState. Non-host clients never call HostStartMatch — they only prepare local
    /// presentation (arena/camera) via ClientPreparePresentation and receive pawns/state via NGO
    /// replication (see NetworkPawnPresenter).
    ///
    /// NGO can only resolve a NetworkObject spawn on a client if the spawning prefab was registered
    /// (NetworkManager.AddNetworkPrefab) with a GlobalObjectIdHash both peers agree on — that hash is
    /// only ever generated for on-disk prefab assets (NetworkObject.OnValidate, editor-only), which is
    /// why the two prefabs this class instantiates are baked ahead of time by the headless
    /// Tools > Blue Water Riptide > Build LAN Network Prefabs editor script rather than built from
    /// `new GameObject()` at runtime the way the rest of this prototype works.
    /// </summary>
    public static class LanMatchBootstrap
    {
        public const string PawnPrefabResourcePath = "Networking/NetworkedPawn";
        public const string MatchStatePrefabResourcePath = "Networking/NetworkMatchState";

        static bool _clientPresentationBuilt;

        /// <summary>
        /// Registers this spike's two network prefabs with the live NetworkManager. Must be called
        /// before StartHost()/StartClient() (NetworkConfig.ForceSamePrefabs defaults true and NGO
        /// throws if prefabs are added after IsListening) — call from the lobby screen's Host/Join
        /// handlers, ahead of LanSessionLauncher.Create/Join.
        /// </summary>
        public static void EnsureNetworkPrefabsRegistered()
        {
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null)
            {
                Debug.LogError("BWR_LAN: no live NetworkManager.Singleton — cannot register network prefabs.");
                return;
            }

            RegisterIfMissing(networkManager, PawnPrefabResourcePath);
            RegisterIfMissing(networkManager, MatchStatePrefabResourcePath);
        }

        static void RegisterIfMissing(NetworkManager networkManager, string resourcePath)
        {
            var prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                Debug.LogError($"BWR_LAN: missing network prefab at Resources/{resourcePath} — run Tools > Blue Water Riptide > Build LAN Network Prefabs first.");
                return;
            }

            if (networkManager.NetworkConfig.Prefabs.Contains(prefab)) return;
            networkManager.AddNetworkPrefab(prefab);
        }

        /// <summary>
        /// Host-only: builds the Session for a fixed 1v1 (Plan 10 M1 scope — no Sailor picks/team
        /// assignment yet, that's M2/M3), spawns both pawns as owned NetworkObjects, and starts
        /// MatchController. hostClientId/clientClientId are NGO client ids (host is always Team A /
        /// Ensign Ace, the joining guest is always Team B / Admiral Anchor — matches
        /// NetworkPawnPresenter's ownership-based Team inference on the client side).
        /// </summary>
        public static void HostStartMatch(LanSessionLauncher launcher, ulong hostClientId, ulong clientClientId)
        {
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null || !networkManager.IsServer)
            {
                Debug.LogError("BWR_LAN: HostStartMatch called without an active server — aborting.");
                return;
            }

            var pawnPrefab = Resources.Load<GameObject>(PawnPrefabResourcePath);
            var matchStatePrefab = Resources.Load<GameObject>(MatchStatePrefabResourcePath);
            if (pawnPrefab == null || matchStatePrefab == null)
            {
                Debug.LogError("BWR_LAN: network prefabs missing — call EnsureNetworkPrefabsRegistered (and build them via the editor tool) before HostStartMatch.");
                return;
            }

            var arena = ArenaCatalog.Default;
            var root = new GameObject("LanMatchHost");

            BuildLighting();
            BuildGround(root.transform, arena);
            Camera cam = BuildCamera();

            var lobbyState = new LobbyState
            {
                Mode = SessionMode.Pvp,
                Started = true,
                Players = new List<LobbyPlayerEntry>
                {
                    new LobbyPlayerEntry { ParticipantIdOrToken = hostClientId.ToString(), Team = Team.A, SailorId = SailorDefinitionData.EnsignAce.Id, IsReady = true, IsAI = false, DisplayName = "Host" },
                    new LobbyPlayerEntry { ParticipantIdOrToken = clientClientId.ToString(), Team = Team.B, SailorId = SailorDefinitionData.AdmiralAnchor.Id, IsReady = true, IsAI = false, DisplayName = "Guest" },
                }
            };

            // Fully qualified: this file sits in BlueWaterRiptide.Networking, a sibling of the
            // BlueWaterRiptide.Networking.Session namespace it also imports below — bare "Session"
            // resolves to that namespace segment instead of Core.Session (napkin Execution & Validation #7).
            BlueWaterRiptide.Core.Session session = launcher.BuildSession(lobbyState, hostClientId.ToString());
            var matchController = new MatchController(session, MatchRules.SinkOrSwimDefaults);

            var pawnAGo = Object.Instantiate(pawnPrefab, arena.SpawnPositionsTeamA[0], Quaternion.identity, root.transform);
            var pawnBGo = Object.Instantiate(pawnPrefab, arena.SpawnPositionsTeamB[0], Quaternion.identity, root.transform);
            pawnAGo.name = "Pawn_Host_EnsignAce";
            pawnBGo.name = "Pawn_Guest_AdmiralAnchor";

            // Cheap visual distinction only — both pawns share one prefab/material for this spike.
            pawnAGo.GetComponent<Renderer>().material.color = new Color(0.3f, 0.55f, 0.9f);
            pawnBGo.GetComponent<Renderer>().material.color = new Color(0.85f, 0.3f, 0.3f);

            var pawnA = pawnAGo.GetComponent<SailorPawn>();
            var pawnB = pawnBGo.GetComponent<SailorPawn>();
            var driverA = pawnAGo.GetComponent<NetworkInputDriver>();
            var driverB = pawnBGo.GetComponent<NetworkInputDriver>();

            // The guest's driver (driverB) intentionally gets no local source here — on the host
            // process IsOwner is false for a client-owned NetworkObject, so NetworkInputDriver.Sample
            // already falls through to the buffered-from-ServerRpc branch for it.
            IInputDriver hostLocalSource = Application.isEditor
                ? new KeyboardMouseInputDriver(pawnAGo.transform, cam)
                : new TouchInputDriver();
            driverA.InitializeLocalSource(hostLocalSource);

            pawnA.Initialize(new ParticipantId(0), Team.A, SailorDefinitionData.EnsignAce, driverA, matchController);
            pawnB.Initialize(new ParticipantId(1), Team.B, SailorDefinitionData.AdmiralAnchor, driverB, matchController);
            pawnA.ArenaHalfExtents = arena.HalfExtents;
            pawnB.ArenaHalfExtents = arena.HalfExtents;

            pawnAGo.GetComponent<NetworkObject>().SpawnWithOwnership(hostClientId);
            pawnBGo.GetComponent<NetworkObject>().SpawnWithOwnership(clientClientId);

            var matchStateGo = Object.Instantiate(matchStatePrefab, Vector3.zero, Quaternion.identity, root.transform);
            matchStateGo.GetComponent<NetworkObject>().Spawn();
            var networkMatchState = matchStateGo.GetComponent<NetworkMatchState>();

            var loopHost = root.AddComponent<LanMatchLoopHost>();
            loopHost.Initialize(matchController, pawnA, pawnB,
                pawnAGo.GetComponent<NetworkSailorState>(), pawnBGo.GetComponent<NetworkSailorState>(),
                networkMatchState, arena, matchController.Rules);

            matchController.StartMatch();

            Debug.Log("BWR_LAN: host match started — 2 pawns spawned as NetworkObjects, MatchController running.");
        }

        /// <summary>Non-host client: builds local arena/camera/lighting only, no Session/MatchController — presentation-only per Plan 02 §1's host-authoritative model. Safe to call more than once (idempotent).</summary>
        public static void ClientPreparePresentation()
        {
            if (_clientPresentationBuilt) return;
            _clientPresentationBuilt = true;

            var arena = ArenaCatalog.Default;
            var root = new GameObject("LanMatchClient");

            BuildLighting();
            BuildGround(root.transform, arena);
            BuildCamera();

            Debug.Log("BWR_LAN: client presentation (arena/camera/lighting) ready — waiting for host-spawned pawns.");
        }

        // Duplicated from M1PrototypeBootstrap rather than reusing its private/static helpers (task
        // guidance: don't refactor M1PrototypeBootstrap, treat it as a read-only reference pattern).
        static void BuildLighting()
        {
            var existing = Object.FindFirstObjectByType<Light>();
            if (existing != null && existing.type == LightType.Directional)
            {
                existing.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                existing.intensity = 1.2f;
                return;
            }

            var lightGo = new GameObject("Directional Light");
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
        }

        static void BuildGround(Transform parent, ArenaDefinition arena)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(parent);
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(3.4f, 1f, 2f);
            ground.GetComponent<Renderer>().material = SailorPawn.CreateUrpMaterial(new Color(0.2f, 0.45f, 0.65f));
        }

        static Camera BuildCamera()
        {
            Camera cam = Camera.main;
            if (cam == null) cam = Object.FindFirstObjectByType<Camera>();

            GameObject camGo;
            if (cam != null)
            {
                camGo = cam.gameObject;
            }
            else
            {
                camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 20f, -14f);
            camGo.transform.LookAt(Vector3.zero, Vector3.up);
            return cam;
        }
    }
}
