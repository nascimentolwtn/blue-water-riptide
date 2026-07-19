using UnityEngine;
using BlueWaterRiptide.Characters;

namespace BlueWaterRiptide.Core
{
    /// <summary>
    /// IMGUI visual overlay for the M1 prototype's touch control zones (move/attack/Super), so a
    /// person can see where to touch. Pure visualization — draws no interactive GUI controls, so
    /// it never intercepts touches meant for TouchInputDriver. No Canvas/UI assets involved;
    /// replaced by the real HUD per Plan 04 later.
    /// </summary>
    public class TouchHud : MonoBehaviour
    {
        Texture2D _fillTex;

        void Awake()
        {
            _fillTex = new Texture2D(1, 1);
            _fillTex.SetPixel(0, 0, Color.white);
            _fillTex.Apply();
        }

        void OnGUI()
        {
            DrawZone(TouchZones.MoveZone, "MOVE");
            DrawZone(TouchZones.AttackZone, "FIRE");
            DrawZone(TouchZones.SuperZone, "SUPER");
        }

        void DrawZone(Rect zone, string label)
        {
            var guiRect = new Rect(zone.x, Screen.height - zone.y - zone.height, zone.width, zone.height);

            var prevColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.08f);
            GUI.DrawTexture(guiRect, _fillTex);
            GUI.color = prevColor;

            var borderStyle = new GUIStyle(GUI.skin.box);
            GUI.Box(guiRect, GUIContent.none, borderStyle);

            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 1f, 1f, 0.5f) }
            };
            GUI.Label(guiRect, label, labelStyle);
        }
    }
}
