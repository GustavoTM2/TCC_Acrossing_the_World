using UnityEngine;

namespace CrossingTheWorld.TCCEgypt
{
    // Apresentação em IMGUI com estilos reutilizados e escala proporcional à janela.
    internal sealed class EgyptStageUI
    {
        private readonly Color gold = new Color(0.86f, 0.7f, 0.4f);
        private readonly Color ink = new Color(0.07f, 0.11f, 0.14f, 0.97f);
        private Texture2D rounded;
        private GUIStyle panel, small, body, speaker, heading, button;

        private void Prepare()
        {
            if (rounded != null) return;
            rounded = new Texture2D(32, 32, TextureFormat.RGBA32, false) { hideFlags = HideFlags.DontSave };
            var pixels = new Color[32 * 32];
            for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
            {
                float dx = Mathf.Max(9 - x, Mathf.Max(x - 22, 0));
                float dy = Mathf.Max(9 - y, Mathf.Max(y - 22, 0));
                pixels[y * 32 + x] = new Color(1, 1, 1, Mathf.Clamp01(9.5f - Mathf.Sqrt(dx * dx + dy * dy)));
            }
            rounded.SetPixels(pixels); rounded.Apply(false, true);
            panel = new GUIStyle { border = new RectOffset(10, 10, 10, 10) };
            panel.normal.background = rounded;
            small = Text(14, new Color(0.75f, 0.78f, 0.79f));
            body = Text(22, new Color(0.95f, 0.94f, 0.91f));
            speaker = Text(23, gold, true);
            heading = Text(30, Color.white, true);
            button = new GUIStyle(GUI.skin.button) { fontSize = 17, fontStyle = FontStyle.Bold,
                border = new RectOffset(10, 10, 10, 10), alignment = TextAnchor.MiddleCenter };
            foreach (GUIStyleState state in new[] { button.normal, button.hover, button.active, button.focused })
            { state.background = rounded; state.textColor = new Color(0.08f, 0.12f, 0.14f); }
        }

        private static GUIStyle Text(int size, Color color, bool bold = false)
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = size, wordWrap = true,
                fontStyle = bold ? FontStyle.Bold : FontStyle.Normal, padding = new RectOffset() };
            style.normal.textColor = color;
            return style;
        }

        private void Fill(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.Box(rect, GUIContent.none, panel);
            GUI.color = Color.white;
        }

        private void Panel(Rect rect)
        {
            Fill(new Rect(rect.x + 3, rect.y + 5, rect.width, rect.height), new Color(0, 0, 0, 0.32f));
            Fill(rect, new Color(gold.r, gold.g, gold.b, 0.65f));
            Fill(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2), ink);
        }

        private void Rule(Rect rect)
        {
            GUI.color = new Color(gold.r, gold.g, gold.b, 0.42f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private bool Button(Rect rect, string label)
        {
            GUI.backgroundColor = gold;
            bool pressed = GUI.Button(rect, label, button);
            GUI.backgroundColor = Color.white;
            return pressed;
        }

        public void Draw(EgyptStage stage, string objective, string status, EgyptLine line, int index, int count)
        {
            Prepare();
            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color, oldBackground = GUI.backgroundColor;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) * 0.5f,
                (Screen.height - 720 * scale) * 0.5f, 0), Quaternion.identity, Vector3.one * scale);
            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            try
            {
                Panel(new Rect(24, 22, 640, 100));
                GUI.Label(new Rect(46, 35, 590, 24), "EGITO  /  AREIAS DO TEMPO", speaker);
                GUIStyle objectiveStyle = new GUIStyle(body) { fontSize = 18 };
                GUI.Label(new Rect(46, 72, 590, 42), objective, objectiveStyle);
                if (EgyptStage.Progress.HasScarab)
                {
                    Panel(new Rect(928, 24, 328, 52));
                    GUI.Label(new Rect(948, 40, 294, 25), "Escaravelho conquistado", new GUIStyle(speaker) { fontSize = 18 });
                }

                if (stage.DialogueIsOpen && line != null)
                {
                    Panel(new Rect(88, 456, 1104, 240));
                    GUI.Label(new Rect(126, 477, 880, 36), line.speaker, speaker);
                    GUI.Label(new Rect(1060, 484, 90, 25), (index + 1) + " / " + count,
                        new GUIStyle(small) { alignment = TextAnchor.MiddleRight });
                    Rule(new Rect(126, 514, 1028, 1));
                    GUIStyle dialogueBody = new GUIStyle(body);
                    while (dialogueBody.fontSize > 18 && dialogueBody.CalcHeight(new GUIContent(line.text), 1028) > 102)
                        dialogueBody.fontSize--;
                    GUI.Label(new Rect(126, 532, 1028, 104), line.text, dialogueBody);
                    GUI.Label(new Rect(126, 658, 620, 24), "E ou Enter para continuar a conversa", small);
                    if (Button(new Rect(946, 646, 208, 34), index + 1 == count ? "Concluir conversa" : "Continuar")) stage.AdvanceFromInput();
                }
                else if (stage.IsDeliveringScarab)
                {
                    Panel(new Rect(390, 632, 500, 58));
                    GUI.Label(new Rect(414, 650, 452, 32), "Recebendo o Velho Escaravelho...", new GUIStyle(speaker) { fontSize = 20 });
                }
                else if (stage.CompletionIsOpen)
                {
                    Panel(new Rect(264, 206, 752, 322));
                    GUI.Label(new Rect(306, 236, 668, 25), "ARTEFATO CONQUISTADO", speaker);
                    GUI.Label(new Rect(306, 278, 668, 48), "Velho Escaravelho", heading);
                    Rule(new Rect(306, 334, 668, 1));
                    GUI.Label(new Rect(306, 355, 668, 94), "Amira aprendeu que ouvir, respeitar e confiar nas pessoas torna a viagem mais rica.", body);
                    if (Button(new Rect(477, 468, 326, 38), "Continuar explorando")) stage.CloseCompletion();
                }
                else
                {
                    if (!string.IsNullOrEmpty(status))
                    {
                        Panel(new Rect(220, 592, 840, 63));
                        GUI.Label(new Rect(244, 609, 792, 38), status, new GUIStyle(body) { fontSize = 19, alignment = TextAnchor.MiddleCenter });
                    }
                    Fill(new Rect(24, 672, 800, 30), new Color(ink.r, ink.g, ink.b, 0.8f));
                    GUI.Label(new Rect(38, 679, 774, 22), "A/D · lados     W/S · fundo/frente     Espaço · pular     E/Enter · interagir", small);
                }
            }
            finally { GUI.matrix = oldMatrix; GUI.color = oldColor; GUI.backgroundColor = oldBackground; }
        }

        public void Dispose()
        {
            if (rounded != null) Object.Destroy(rounded);
        }
    }
}
