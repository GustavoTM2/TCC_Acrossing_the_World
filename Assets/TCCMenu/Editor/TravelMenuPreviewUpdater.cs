using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CrossingTheWorld.TCCMenu.Editor
{
    /// <summary>
    /// Atualiza a cena real do menu com as quatro capturas das fases (1:1).
    /// Não recria a cena nem altera os botões, as cenas carregadas ou o progresso.
    /// </summary>
    public static class TravelMenuPreviewUpdater
    {
        private static readonly string[] PreviewPaths =
        {
            "Assets/TCCMenu/Art/Egito.png",
            "Assets/TCCMenu/Art/Brasil.png",
            "Assets/TCCExploracao/Generated/Italia_Previa.png",
            "Assets/TCCExploracao/Generated/Japao_Previa.png"
        };

        [MenuItem("TCC/Menu/Atualizar imagens das fases (1x1)")]
        public static void UpdateManually()
        {
            Apply(true);
        }

        private static void Apply(bool showDialog)
        {
            AssetDatabase.Refresh();
            var textures = new Texture2D[4];

            for (int i = 0; i < 4; i++)
            {
                textures[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(PreviewPaths[i]);
                if (textures[i] == null)
                {
                    Debug.LogError("Imagem não encontrada: " + PreviewPaths[i]);
                    return;
                }
            }

            Scene scene = SceneManager.GetSceneByPath(TravelMenu.ScenePath);
            bool openedScene = !scene.isLoaded;
            if (openedScene)
                scene = EditorSceneManager.OpenScene(TravelMenu.ScenePath, OpenSceneMode.Additive);

            try
            {
                TravelMenu menu = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    menu = root.GetComponentInChildren<TravelMenu>(true);
                    if (menu != null) break;
                }

                if (menu == null || menu.cards == null || menu.cards.Length < 4)
                    throw new InvalidOperationException("Quatro cards não encontrados na cena " + TravelMenu.ScenePath);

                for (int i = 0; i < 4; i++)
                {
                    RectTransform card = menu.cards[i];
                    if (card == null) throw new InvalidOperationException("Card " + i + " está vazio.");

                    RectTransform preview = card.Find("Previa") as RectTransform;
                    if (preview == null)
                        throw new InvalidOperationException("Objeto Previa ausente no card " + i);

                    RawImage image = preview.GetComponent<RawImage>();
                    if (image == null)
                        throw new InvalidOperationException("Componente RawImage ausente no card " + i);

                    image.texture = textures[i];
                    image.uvRect = new Rect(0, 0, 1, 1);
                    preview.sizeDelta = new Vector2(320, 320);

                    // Preserva o topo do card (y = 310 no layout original).
                    float oldHeight = card.sizeDelta.y;
                    float top = -card.anchoredPosition.y - oldHeight * (1f - card.pivot.y);
                    card.sizeDelta = new Vector2(340, 480);
                    card.anchoredPosition = new Vector2(
                        card.anchoredPosition.x,
                        -(top + 480f * (1f - card.pivot.y)));

                    SetSize(card, "Borda", 340, 480);
                    SetSize(card, "Fundo", 334, 474);
                    SetPosition(card, "Pais", 338, 42);
                    SetPosition(card, "Local", 384, 30);
                    SetPosition(card, "Estado", 436, 24);
                    EditorUtility.SetDirty(image);
                }

                EditorUtility.SetDirty(menu);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("TCC_MENU_1X1_OK: Egito, Brasil, Itália e Japão atualizados.");
                if (showDialog)
                    EditorUtility.DisplayDialog("Prévia das fases", "Menu atualizado! As quatro imagens estão quadradas (1:1).", "OK");
            }
            finally
            {
                if (openedScene && scene.isLoaded)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void SetSize(RectTransform parent, string name, float w, float h)
        {
            var target = parent.Find(name) as RectTransform;
            if (target != null) target.sizeDelta = new Vector2(w, h);
        }

        private static void SetPosition(RectTransform parent, string name, float y, float height)
        {
            var target = parent.Find(name) as RectTransform;
            if (target == null) return;
            target.anchoredPosition = new Vector2(target.anchoredPosition.x, -y);
            target.sizeDelta = new Vector2(target.sizeDelta.x, height);
        }
    }
}
