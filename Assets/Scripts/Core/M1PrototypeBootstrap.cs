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

                var arena = ArenaCatalog.Default;

                BuildLighting();
                BuildGround(root.transform);
                var cam = BuildCamera();

                var playerDefinition = SailorDefinitionData.AdmiralAnchor;
                var enemyDefinition = SailorDefinitionData.EnsignAce;

                var playerGo = BuildPawnObject(root.transform, "Player_Anchor", arena.SpawnPositionsTeamA[0], Color.blue);
                var enemyGo = BuildPawnObject(root.transform, "Enemy_Ace", arena.SpawnPositionsTeamB[0], Color.red);

                var playerPawn = playerGo.AddComponent<SailorPawn>();
                var enemyPawn = enemyGo.AddComponent<SailorPawn>();

                var participants = new List<ParticipantInfo>
                {
                    new ParticipantInfo(new ParticipantId(0), Team.A, DriverType.Human, playerDefinition.Id),
                    new ParticipantInfo(new ParticipantId(1), Team.B, DriverType.AI, enemyDefinition.Id),
                };
                var session = new Session("sink-or-swim", arena.Id, participants);
                var matchController = new MatchController(session, MatchRules.SinkOrSwimDefaults);

                // Touch is this project's real control scheme (Plan 01 §1) and what actually
                // works on Android (no reliable mouse/hardware-keyboard device there — see
                // napkin Execution & Validation). Keyboard/mouse stays available for quick
                // iteration when testing in the Editor.
                IInputDriver playerDriver = Application.isEditor
                    ? new KeyboardMouseInputDriver(playerGo.transform, cam)
                    : new TouchInputDriver();
                var enemyDriver = new DummyAIInputDriver(enemyGo.transform, playerGo.transform, enemyDefinition.AttackRange);

                playerPawn.Initialize(new ParticipantId(0), Team.A, playerDefinition, playerDriver, matchController);
                enemyPawn.Initialize(new ParticipantId(1), Team.B, enemyDefinition, enemyDriver, matchController);

                playerPawn.ArenaHalfExtents = arena.HalfExtents;
                enemyPawn.ArenaHalfExtents = arena.HalfExtents;

                // MatchLoopHost must subscribe to OnRoundCountdownStart before StartMatch() fires
                // round 1's countdown, so pawns get their spawn-immunity window from round 1 onward too.
                var loopHost = root.AddComponent<MatchLoopHost>();
                loopHost.Initialize(matchController, playerPawn, enemyPawn, arena, matchController.Rules);

                matchController.StartMatch();

                var hud = root.AddComponent<M1Hud>();
                hud.Initialize(playerPawn, enemyPawn, matchController);
                if (!Application.isEditor) root.AddComponent<TouchHud>();

                var profile = SaveService.Load();
                Debug.Log($"BWR_SAVE: profile '{profile.profileId}' ready — displayName={profile.displayName}, doubloons={profile.doubloons}, sailors={profile.sailors.Count}");
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

        static void BuildGround(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(parent);
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(3.4f, 1f, 2f); // Unity Plane default 10x10 -> ~34x20 (Tideline Cove's footprint; ArenaCatalog's 16.5x9.5 half-extents are inset from this)
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
