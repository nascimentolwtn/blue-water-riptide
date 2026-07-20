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

        int _currentRound = 0;
        bool _suddenDeathActive = false;
        MatchController _matchController;

        public void Initialize(SailorPawn player, SailorPawn enemy, MatchController matchController)
        {
            _player = player;
            _enemy = enemy;
            _matchController = matchController;

            matchController.OnMatchEnd += winner =>
            {
                _resultText = winner == Team.A ? "YOU WIN" : "YOU LOSE";
            };

            matchController.OnRoundCountdownStart += round => _currentRound = round;
            matchController.OnSuddenDeathStart += () => _suddenDeathActive = true;
            matchController.OnRoundEnd += winner => _suddenDeathActive = false;
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

            if (_currentRound > 0)
            {
                var roundStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                GUI.Label(new Rect(Screen.width / 2f - 60, 10, 120, 30), $"Round {_currentRound}", roundStyle);
            }

            if (_suddenDeathActive)
            {
                var warningStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 24,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.red }
                };
                var bannerRect = new Rect(Screen.width / 2f - 260, 46, 520, 34);
                GUI.Label(bannerRect, "⚠ SUDDEN DEATH - SAFE ZONE SHRINKING ⚠", warningStyle);

                if (_matchController != null)
                {
                    var elapsedStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.red } };
                    GUI.Label(new Rect(Screen.width / 2f - 100, 78, 200, 20), $"{_matchController.SuddenDeathElapsed:F0}s in the ring", elapsedStyle);
                }
            }

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
