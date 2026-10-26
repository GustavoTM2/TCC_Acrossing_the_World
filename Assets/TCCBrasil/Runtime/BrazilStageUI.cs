using UnityEngine;
using CrossingTheWorld.TCCEgypt;

namespace CrossingTheWorld.TCCBrazil
{
    [RequireComponent(typeof(BrazilStage))]
    public sealed class BrazilStageUI : MonoBehaviour
    {
        private BrazilStage stage;
        private readonly EgyptStageUI interactionUI = new EgyptStageUI();
        private GUIStyle body, small, title, large, button, panel, phaseTitle, objective, artifactLabel, artifactName;
        private Texture2D rounded;
        private readonly Color ink = new Color(.07f, .11f, .14f, .97f);
        private readonly Color gold = new Color(.86f, .7f, .4f);
        private readonly Color green = new Color(.16f, .72f, .51f);
        private void Awake() { stage = GetComponent<BrazilStage>(); }

        private void Prepare()
        {
            if (body != null) return;
            rounded = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            rounded.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
            {
                Vector2 d = new Vector2(Mathf.Max(Mathf.Abs(x - 15.5f) - 9.5f, 0), Mathf.Max(Mathf.Abs(y - 15.5f) - 9.5f, 0));
                rounded.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(7 - d.magnitude)));
            }
            rounded.Apply();
            panel = new GUIStyle { border = new RectOffset(8,8,8,8) };
            panel.normal.background = rounded;
            body = new GUIStyle(GUI.skin.label) { fontSize = 22, wordWrap = true, richText = false, alignment = TextAnchor.UpperLeft };
            body.normal.textColor = Color.white;
            small = new GUIStyle(body) { fontSize = 16 };
            phaseTitle = new GUIStyle(body) { fontSize = 23, fontStyle = FontStyle.Bold, padding = new RectOffset() };
            objective = new GUIStyle(body) { fontSize = 18, padding = new RectOffset() };
            artifactLabel = new GUIStyle(body) { fontSize = 14, padding = new RectOffset() };
            artifactName = new GUIStyle(phaseTitle) { fontSize = 19 };
            title = new GUIStyle(body) { fontSize = 29, fontStyle = FontStyle.Bold };
            large = new GUIStyle(title) { fontSize = 64, alignment = TextAnchor.MiddleCenter };
            button = new GUIStyle(GUI.skin.button) { fontSize = 19, fontStyle = FontStyle.Bold, padding = new RectOffset(16,16,8,8), border = new RectOffset(8,8,8,8) };
            button.normal.background = rounded; button.hover.background = rounded; button.active.background = rounded;
            button.normal.textColor = ink; button.hover.textColor = ink; button.active.textColor = ink;
        }
        private void Panel(Rect area)
        {
            GUI.backgroundColor = new Color(0,0,0,.32f);
            GUI.Box(new Rect(area.x+3,area.y+5,area.width,area.height),GUIContent.none,panel);
            GUI.backgroundColor = new Color(gold.r,gold.g,gold.b,.65f);
            GUI.Box(area,GUIContent.none,panel);
            GUI.backgroundColor = ink;
            GUI.Box(new Rect(area.x+1,area.y+1,area.width-2,area.height-2),GUIContent.none,panel);
            GUI.backgroundColor = Color.white;
        }
        private void Text(Rect area, string value, GUIStyle style, Color color) { var old = GUI.color; GUI.color = color; GUI.Label(area, value, style); GUI.color = old; }
        private bool Button(Rect area, string label) { GUI.backgroundColor = gold; bool clicked = GUI.Button(area,label,button); GUI.backgroundColor = Color.white; return clicked; }

        private void OnGUI()
        {
            if (stage == null) return;
            Prepare();
            Matrix4x4 previous = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2, (Screen.height - 720 * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);
            Panel(new Rect(24, 22, 640, 100));
            Text(new Rect(46, 35, 590, 28), "BRASIL  /  RIO DE JANEIRO", phaseTitle, gold);
            Text(new Rect(46, 72, 590, 42), stage.Objective, objective, Color.white);
            if (stage.HasPrize && stage.State != BrazilState.Reward) DrawSmallPrize();
            switch (stage.State)
            {
                case BrazilState.Exploring:
                    interactionUI.DrawExplorationControls("A/D · lados     W/S · fundo/frente     E/Enter · interagir");
                    interactionUI.DrawInteractionPrompt(stage.InteractionPrompt);
                    break;
                case BrazilState.Dialogue:
                    Panel(new Rect(94, 420, 1092, 270));
                    Text(new Rect(124, 443, 870, 38), stage.Speaker, title, gold);
                    Text(new Rect(1080, 450, 85, 25), (stage.LineIndex + 1) + " / " + stage.LineCount, small, Color.white);
                    Text(new Rect(124, 498, 1020, 120), stage.DialogueText, body, Color.white);
                    Text(new Rect(124, 643, 430, 28), "E ou Enter para continuar · Esc para sair", small, Color.white);
                    if (Button(new Rect(908, 630, 245, 43), "Continuar  →")) stage.AdvanceDialogue();
                    break;
                case BrazilState.Ready:
                    Panel(new Rect(280, 190, 720, 430));
                    Text(new Rect(316, 222, 650, 50), "DESAFIO DE EMBAIXADINHAS", title, gold);
                    Text(new Rect(316, 290, 650, 190), "Complete 10 embaixadinhas seguidas.\n\nAperte Espaço na faixa verde e solte entre os toques. Ela muda de lugar e de tamanho a cada acerto. Se errar ou demorar, a bola cai e a sequência recomeça.", body, Color.white);
                    if (Button(new Rect(316, 495, 648, 52), "Começar · Espaço")) stage.StartChallenge();
                    if (Button(new Rect(316, 560, 648, 42), "Voltar à praia")) stage.ContinueExploring();
                    break;
                case BrazilState.Countdown:
                    Panel(new Rect(470, 235, 340, 230));
                    Text(new Rect(500, 265, 280, 38), "PREPARE-SE", title, gold);
                    Text(new Rect(500, 310, 280, 105), Mathf.CeilToInt(stage.Countdown).ToString(), large, Color.white);
                    break;
                case BrazilState.Playing:
                    Panel(new Rect(985, 24, 266, 125));
                    Text(new Rect(1007, 35, 225, 32), "EMBAIXADINHAS", small, gold);
                    Text(new Rect(1003, 63, 225, 75), stage.Challenge.Score + " / 10", large, Color.white);
                    Panel(new Rect(250, 540, 780, 154));
                    Text(new Rect(282, 556, 710, 35), stage.Challenge.InWindow ? "AGORA! APERTE ESPAÇO" : stage.Challenge.IsPreparing ? "Observe a nova faixa verde..." : "Espere o marcador chegar à faixa verde", body, stage.Challenge.InWindow ? green : Color.white);
                    Rect bar = new Rect(282, 610, 716, 26);
                    GUI.color = new Color(.25f,.33f,.36f); GUI.DrawTexture(bar, Texture2D.whiteTexture);
                    GUI.color = green; GUI.DrawTexture(new Rect(bar.x + bar.width * stage.Challenge.WindowStart, bar.y, bar.width * (stage.Challenge.WindowEnd - stage.Challenge.WindowStart), bar.height), Texture2D.whiteTexture);
                    GUI.color = gold; GUI.DrawTexture(new Rect(bar.x + bar.width * Mathf.Clamp01(stage.Challenge.Phase) - 4, bar.y - 7, 8, 40), Texture2D.whiteTexture); GUI.color = Color.white;
                    Text(new Rect(282, 654, 710, 26), "Um toque por embaixadinha   •   Esc: sair do desafio", small, Color.white);
                    break;
                case BrazilState.Failed:
                    Panel(new Rect(300, 215, 680, 370));
                    Text(new Rect(338, 248, 610, 44), "QUASE LÁ! TENTE DE NOVO", title, gold);
                    Text(new Rect(338, 310, 600, 95), stage.Challenge.Failure + "\nSequência: " + stage.Challenge.Score + "/10  ·  Melhor: " + stage.Best + "/10", body, Color.white);
                    if (Button(new Rect(338, 434, 604, 52), "Tentar novamente · Espaço")) stage.StartChallenge();
                    if (Button(new Rect(338, 506, 604, 44), "Voltar à praia")) stage.ContinueExploring();
                    break;
                case BrazilState.Reward: DrawReward(); break;
            }
            GUI.matrix = previous;
        }

        private void DrawReward()
        {
            Panel(new Rect(180, 155, 920, 530));
            Text(new Rect(220, 186, 850, 36), "DESAFIO CONCLUÍDO · 10/10", title, gold);
            if (stage.rewardDisplay != null && stage.rewardDisplay.PreviewTexture != null)
                GUI.DrawTexture(new Rect(220, 245, 250, 355), stage.rewardDisplay.PreviewTexture, ScaleMode.ScaleToFit, true);
            Text(new Rect(514, 260, 530, 45), "Berimbau antigo", title, Color.white);
            Text(new Rect(514, 322, 520, 160), "O berimbau que animava as rodas de capoeira do avô do morador agora acompanha sua viagem.\n\nMais uma história para levar pelo mundo!", body, Color.white);
            if (Button(new Rect(514, 520, 520, 52), "Continuar na praia")) stage.ContinueExploring();
            GUI.enabled = false;
            Button(new Rect(514, 589, 520, 42), "Voltar ao menu · em breve");
            GUI.enabled = true;
        }
        private void DrawSmallPrize()
        {
            Panel(new Rect(944, 24, 312, 124));
            if (stage.rewardDisplay != null && stage.rewardDisplay.PreviewTexture != null)
                GUI.DrawTexture(new Rect(952, 28, 112, 112), stage.rewardDisplay.PreviewTexture, ScaleMode.ScaleToFit, true);
            Text(new Rect(1074, 48, 170, 28), "ARTEFATO", artifactLabel, new Color(.75f,.78f,.79f));
            Text(new Rect(1074, 75, 170, 55), "Berimbau antigo", artifactName, gold);
        }
        private void OnDestroy()
        {
            interactionUI.Dispose();
            if (rounded != null) Destroy(rounded);
        }
    }
}
