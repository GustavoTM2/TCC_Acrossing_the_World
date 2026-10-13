using UnityEngine;

namespace CrossingTheWorld.TCCBrazil
{
    public sealed class BrazilExplorationHUD : MonoBehaviour
    {
        private GUIStyle title;
        private GUIStyle controls;

        // Aqui desenho o título da fase e as instruções dos controles na tela.
        private void OnGUI()
        {
            // Preparo os estilos uma vez e reutilizo nas próximas chamadas.
            if (title == null)
            {
                title = new GUIStyle(GUI.skin.label) { fontSize = 21, fontStyle = FontStyle.Bold };
                controls = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
                title.normal.textColor = Color.white;
                controls.normal.textColor = new Color(0.94f, 0.96f, 1f);
            }
            Matrix4x4 previous = GUI.matrix;
            Color previousColor = GUI.color;
            // Ajusto o tamanho do painel conforme a resolução da janela.
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);
            GUI.color = new Color(0.04f, 0.13f, 0.18f, 0.85f);
            GUI.DrawTexture(new Rect(24, 24, 594, 104), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(42, 34, 555, 30), "BRASIL — RAÍZES TROPICAIS", title);
            GUI.Label(new Rect(42, 73, 555, 48), "Explore a praia. A/D: lados • W/S: fundo/frente\nVocê também pode usar as setas do teclado.", controls);
            // Restauro as configurações para não alterar outras partes da interface.
            GUI.matrix = previous;
            GUI.color = previousColor;
        }
    }
}
