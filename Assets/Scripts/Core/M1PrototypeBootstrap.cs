using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BlueWaterRiptide.AI;
using BlueWaterRiptide.Characters;

namespace BlueWaterRiptide.Core
{
    /// <summary>
    /// M1 vertical-slice bootstrap (Plan 01 M1: greybox, Ace only, one dummy AI opponent,
    /// single-round Sink or Swim). Builds the entire prototype procedurally at runtime —
    /// arena, camera, light, both Sailors, Session/MatchController — so testing needs nothing
    /// but pressing Play. Prototype-only: replaced by real scenes/prefabs per Plan 01/04 later.
    /// </summary>
    public static class M1PrototypeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnGameStart()
        {
            Build();
            SceneManager.sceneLoaded += (_, _) => Build();
        }

        static void Build()
        {
            try
            {
                var root = new GameObject("M1Prototype");

                BuildLighting();
                BuildGround(root.transform, out Vector2 halfExtents);
                var cam = BuildCamera();

                var playerDefinition = SailorDefinitionData.AdmiralAnchor;
                var enemyDefinition = SailorDefinitionData.EnsignAce;

                var playerGo = BuildPawnObject(root.transform, "Player_Anchor", new Vector3(-8f, 1f, 0f), Color.blue);
                var enemyGo = BuildPawnObject(root.transform, "Enemy_Ace", new Vector3(8f, 1f, 0f), Color.red);

                var playerPawn = playerGo.AddComponent<SailorPawn>();
                var enemyPawn = enemyGo.AddComponent<SailorPawn>();

                var participants = new List<ParticipantInfo>
                {
                    new ParticipantInfo(new ParticipantId(0), Team.A, DriverType.Human, playerDefinition.Id),
                    new ParticipantInfo(new ParticipantId(1), Team.B, DriverType.AI, enemyDefinition.Id),
                };
                var session = new Session("sink-or-swim", "prototype", participants);
                var matchController = new MatchController(session, MatchRules.M1Defaults);

                var playerDriver = new KeyboardMouseInputDriver(playerGo.transform, cam);
                var enemyDriver = new DummyAIInputDriver(enemyGo.transform, playerGo.transform, enemyDefinition.AttackRange);

                playerPawn.Initialize(new ParticipantId(0), Team.A, playerDefinition, playerDriver, matchController);
                enemyPawn.Initialize(new ParticipantId(1), Team.B, enemyDefinition, enemyDriver, matchController);

                playerPawn.ArenaHalfExtents = halfExtents;
                enemyPawn.ArenaHalfExtents = halfExtents;

                matchController.StartMatch();

                var hud = root.AddComponent<M1Hud>();
                hud.Initialize(playerPawn, enemyPawn, matchController);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"BWR_BOOT_FAILED: {e.GetType().Name}: {e.Message}\n{e.StackTrace}");
            }
        }

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

        static void BuildGround(Transform parent, out Vector2 halfExtents)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(parent);
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(3.4f, 1f, 2f); // Unity Plane default 10x10 -> ~34x20 (Tideline Cove's footprint)
            ground.GetComponent<Renderer>().material = SailorPawn.CreateUrpMaterial(new Color(0.2f, 0.45f, 0.65f));

            halfExtents = new Vector2(16.5f, 9.5f); // inset from the plane edges so pawns can't walk off
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

        static GameObject BuildPawnObject(Transform parent, string name, Vector3 position, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.GetComponent<Renderer>().material = SailorPawn.CreateUrpMaterial(color);
            return go;
        }
    }
}
