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

        // Preparo os estilos de texto, botões e o fundo arredondado das caixas.
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
            button.onNormal.background = rounded;
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

        // Desenho a caixa com sombra, borda dourada e fundo escuro.
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

        // Aqui desenho os objetivos, avisos, diálogos e a mensagem de conclusão.
        public void Draw(EgyptStage stage, string objective, string status, EgyptLine line, int index, int count)
        {
            Prepare();
            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color, oldBackground = GUI.backgroundColor;
            // Ajusto a interface à resolução da janela usando 1280 por 720 como referência.
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
                if (EgyptStage.Progress.HasScarab && !stage.CompletionIsOpen)
                {
                    Panel(new Rect(944, 24, 312, 124));
                    DrawScarab(new Rect(952, 28, 112, 112), stage.ScarabPreview);
                    GUI.Label(new Rect(1074, 48, 170, 28), "ARTEFATO", small);
                    GUI.Label(new Rect(1074, 75, 170, 55), "Velho Escaravelho", new GUIStyle(speaker) { fontSize = 19 });
                }

                // Na conversa, mostro quem está falando, o texto e o botão para continuar.
                if (stage.DialogueIsOpen && line != null)
                {
                    Panel(new Rect(88, 456, 1104, 240));
                    GUI.Label(new Rect(126, 477, 880, 36), line.speaker, speaker);
                    GUI.Label(new Rect(1060, 484, 90, 25), (index + 1) + " / " + count,
                        new GUIStyle(small) { alignment = TextAnchor.MiddleRight });
                    Rule(new Rect(126, 514, 1028, 1));
                    GUIStyle dialogueBody = new GUIStyle(body);
                    // Diminuo um pouco a letra se a fala for longa, para caber dentro da caixa.
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
                // Mostro o escaravelho e as duas opções ao concluir a fase.
                else if (stage.CompletionIsOpen)
                {
                    Panel(new Rect(240, 174, 800, 466));
                    GUI.Label(new Rect(278, 202, 724, 30), "FASE CONCLUÍDA", speaker);
                    Rule(new Rect(278, 244, 724, 1));
                    // Mostro o escaravelho girando em destaque antes de deixá-lo no canto da tela.
                    DrawScarab(new Rect(274, 260, 236, 236), stage.ScarabPreview);
                    GUI.Label(new Rect(534, 278, 458, 48), "Velho Escaravelho", heading);
                    GUI.Label(new Rect(534, 342, 450, 132), "Amira aprendeu que ouvir, respeitar e confiar nas pessoas torna a viagem mais rica.", body);
                    bool wasEnabled = GUI.enabled;
                    GUI.enabled = false;
                    Button(new Rect(278, 536, 342, 44), "Voltar para o menu");
                    GUI.enabled = wasEnabled;
                    if (Button(new Rect(660, 536, 342, 44), "Continuar a fase")) stage.CloseCompletion();
                    GUI.Label(new Rect(278, 592, 342, 28), "Menu indisponível nesta versão.", small);
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

        private static void DrawScarab(Rect rect, Texture preview)
        {
            if (preview != null) GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit, true);
        }

        public void Dispose()
        {
            if (rounded != null) Object.Destroy(rounded);
        }
    }
}
