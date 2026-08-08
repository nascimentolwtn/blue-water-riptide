using UnityEngine;
using UnityEngine.SceneManagement;
using BlueWaterRiptide.UI;
using BlueWaterRiptide.UI.Navigation;

namespace BlueWaterRiptide.Networking
{
    /// <summary>
    /// Additive LAN entry point (Plan 10 M1 task 3): pushes the Host/Join lobby (LanLobbyScreen) as
    /// an always-on top-right overlay alongside whatever M1PrototypeBootstrap builds, rather than
    /// gating or otherwise touching that class — the safer of the two options its own task brief
    /// allows ("leave M1PrototypeBootstrap completely untouched... an always-on second UI layer is
    /// fine"). Single Player keeps working with zero required action; LAN is reachable by simply
    /// using the panel this creates. The two matches' 3D scenes can visually coexist if both are
    /// started in the same session (M1's own arena/camera, and LanMatchBootstrap's) — an accepted
    /// rough edge for this spike, not something this class attempts to resolve.
    /// </summary>
    public static class LanModeEntryPoint
    {
        const string LobbyScreenId = "lan-lobby";

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
                var controllerGo = new GameObject("LanLobbyUIStateController");
                var controller = controllerGo.AddComponent<UIStateController>();

                controller.RegisterScreen(LobbyScreenId, () =>
                {
                    var screenGo = new GameObject("LanLobbyScreen");
                    screenGo.AddComponent<LanLobbyScreen>();
                    return screenGo;
                });

                controller.Push(LobbyScreenId);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"BWR_LAN_BOOT_FAILED: {e.GetType().Name}: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}
