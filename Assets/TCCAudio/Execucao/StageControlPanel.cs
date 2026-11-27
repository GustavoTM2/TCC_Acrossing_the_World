using UnityEngine;

namespace CrossingTheWorld.TCCAudio
{
    /// <summary>
    /// Quadro de controles único para as fases: duas colunas reais, não uma
    /// string de espaços. Usa o GUI.matrix já escalado pela HUD de cada país.
    /// </summary>
    public static class StageControlPanel
    {
        public struct Control
        {
            public readonly string Key;
            public readonly string Action;

            public Control(string key, string action)
            {
                Key = key;
                Action = action;
            }
        }

        public static void Draw(Rect bounds, Color accent, params Control[] rows)
        {
            if (rows == null || rows.Length == 0) return;
            var oldColor = GUI.color;
            var oldBackground = GUI.backgroundColor;
            try
            {
                GUI.color = new Color(0f, 0f, 0f, .38f);
                GUI.DrawTexture(new Rect(bounds.x + 3f, bounds.y + 4f,
                    bounds.width, bounds.height), Texture2D.whiteTexture);

                GUI.color = new Color(.065f, .088f, .105f, .97f);
                GUI.DrawTexture(bounds, Texture2D.whiteTexture);

                GUI.color = accent;
                GUI.DrawTexture(new Rect(bounds.x, bounds.y, 4f, bounds.height), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(bounds.x + 4f, bounds.y, bounds.width - 4f, 2f),
                    Texture2D.whiteTexture);

                var titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(0, 0, 0, 0)
                };
                titleStyle.normal.textColor = accent;
                var keyStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleRight,
                    clipping = TextClipping.Clip,
                    padding = new RectOffset(0, 0, 0, 0)
                };
                keyStyle.normal.textColor = new Color(.96f, .94f, .88f);
                var actionStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = bounds.width < 300f ? 13 : 14,
                    alignment = TextAnchor.MiddleLeft,
                    clipping = TextClipping.Clip,
                    padding = new RectOffset(0, 0, 0, 0)
                };
                actionStyle.normal.textColor = new Color(.83f, .87f, .86f);

                GUI.color = Color.white;
                GUI.Label(new Rect(bounds.x + 12f, bounds.y + 9f,
                    bounds.width - 24f, 23f), "CONTROLES", titleStyle);

                GUI.color = new Color(accent.r, accent.g, accent.b, .40f);
                GUI.DrawTexture(new Rect(bounds.x + 17f, bounds.y + 37f,
                    bounds.width - 34f, 1f), Texture2D.whiteTexture);

                float rowsY = bounds.y + 44f;
                float rowHeight = (bounds.height - 53f) / rows.Length;
                float separatorX = bounds.x + 108f;
                GUI.color = new Color(accent.r, accent.g, accent.b, .20f);
                GUI.DrawTexture(new Rect(separatorX, rowsY, 1f, bounds.height - 53f),
                    Texture2D.whiteTexture);
                GUI.color = Color.white;

                for (int i = 0; i < rows.Length; ++i)
                {
                    float y = rowsY + i * rowHeight;
                    GUI.Label(new Rect(bounds.x + 12f, y, 86f, rowHeight), rows[i].Key, keyStyle);
                    GUI.Label(new Rect(bounds.x + 120f, y,
                        bounds.width - 133f, rowHeight), rows[i].Action, actionStyle);
                }
            }
            finally
            {
                GUI.color = oldColor;
                GUI.backgroundColor = oldBackground;
            }
        }
    }
}
