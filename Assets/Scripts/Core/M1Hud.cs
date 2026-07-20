using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using BlueWaterRiptide.Characters;

namespace BlueWaterRiptide.Core
{
    /// <summary>
    /// IMGUI debug HUD for the M1 prototype — HP/ammo readout and a restart key.
    /// No Canvas/UI assets involved; replaced by the real HUD per Plan 04 later.
    /// </summary>
    public class M1Hud : MonoBehaviour
    {
        SailorPawn _player;
        SailorPawn _enemy;
        string _resultText = "";

        public void Initialize(SailorPawn player, SailorPawn enemy, MatchController matchController)
        {
            _player = player;
            _enemy = enemy;

            matchController.OnMatchEnd += winner =>
            {
                _resultText = winner == Team.A ? "YOU WIN" : "YOU LOSE";
            };
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                Restart();
            }
        }

        static void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void OnGUI()
        {
            if (_player == null || _enemy == null) return;

            GUI.Box(new Rect(10, 10, 280, 100), "");
            GUI.Label(new Rect(20, 15, 260, 20), $"You:   {Mathf.CeilToInt(_player.CurrentHP)} HP   Ammo {_player.CurrentAmmo}   Super {Mathf.RoundToInt(_player.SuperCharge01 * 100f)}%");
            GUI.Label(new Rect(20, 35, 260, 20), $"Enemy: {Mathf.CeilToInt(_enemy.CurrentHP)} HP   Ammo {_enemy.CurrentAmmo}");
            string controlsHint = Application.isEditor
                ? "WASD move · Mouse aim · Hold LMB fire"
                : "Left: move · Right: drag to aim, hold to fire";
            string superHint = Application.isEditor
                ? "RMB: Super (Drop Anchor) when charged"
                : "Tap SUPER zone (top-right) when charged";
            GUI.Label(new Rect(20, 60, 260, 20), controlsHint);
            GUI.Label(new Rect(20, 78, 260, 20), superHint);

            if (!string.IsNullOrEmpty(_resultText))
            {
                var bannerStyle = new GUIStyle(GUI.skin.label) { fontSize = 40, alignment = TextAnchor.MiddleCenter };
                var buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 20 };

                GUI.Label(new Rect(Screen.width / 2f - 200, Screen.height / 2f - 60, 400, 80), _resultText, bannerStyle);

                var restartRect = new Rect(Screen.width / 2f - 100, Screen.height / 2f + 30, 200, 50);
                if (GUI.Button(restartRect, "Restart", buttonStyle))
                {
                    Restart();
                }
            }
        }
    }
}
