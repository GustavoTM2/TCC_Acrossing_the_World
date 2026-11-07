using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace CrossingTheWorld.TCCMenu.Editor
{
    // Ferramenta de preparação: monta objetos comuns de UI e salva a cena para editar no Inspector.
    public static class TravelMenuSetup
    {
        private const string Root = "Assets/TCCMenu/";
        private static Font serif, body;
        private static readonly Color Gold = new Color(1, .76f, .37f);
        private static readonly Color Cream = new Color(.96f, .89f, .74f);

        [MenuItem("TCC/Menu/Preparar menu principal")]
        public static void Build()
        {
            if (Application.unityVersion != "6000.5.7f1") throw new InvalidOperationException("Use a Unity 6000.5.7f1.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            AssetDatabase.Refresh();
            ConfigureTexture("Menu_Fundo.png", false, 2048);
            ConfigureTexture("Bussola.png", true, 512);
            ConfigureTexture("Egito.png", false, 2048);
            ConfigureTexture("Brasil.png", false, 2048);
            serif = AssetDatabase.LoadAssetAtPath<Font>(Root + "Fonts/Cinzel.ttf");
            body = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
            if (serif == null || body == null) throw new InvalidOperationException("Fontes do menu não encontradas.");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = new GameObject("Camera_Menu").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.025f, .045f, .06f);
            camera.cullingMask = 0;
            camera.gameObject.AddComponent<AudioListener>();

            var canvas = new GameObject("Canvas_Menu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1672, 941);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            RectTransform frame = Rect("Area_16_por_9", canvas.transform, 0, 0, 1672, 941);
            frame.anchorMin = frame.anchorMax = frame.pivot = new Vector2(.5f, .5f);
            frame.anchoredPosition = Vector2.zero;
            var menu = canvas.AddComponent<TravelMenu>();

            var events = new GameObject("EventSystem", typeof(EventSystem));
            // A navegação por teclado fica no TravelMenu; o EventSystem cuida dos cliques.
            events.GetComponent<EventSystem>().sendNavigationEvents = false;
#if ENABLE_INPUT_SYSTEM
            events.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
#else
            events.AddComponent<StandaloneInputModule>();
#endif
            var main = Rect("Menu_Principal", frame, 0, 0, 1672, 941);
            var destinations = Rect("Selecao_de_Paises", frame, 0, 0, 1672, 941);
            menu.mainPage = main.gameObject; menu.destinationsPage = destinations.gameObject;
            Texture2D background = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Art/Menu_Fundo.png");
            Picture("Mapa", main, 0, 0, 1672, 941, background);
            menu.mainLabels = new Text[4];
            string[] options = { "JOGAR", "OPÇÕES", "CRÉDITOS", "SAIR" };
            for (int i = 0; i < 4; i++)
            {
                RectTransform row = Rect(options[i], main, 50, 375 + 85 * i, 325, 75);
                MakeButton(row, menu, i, false);
                menu.mainLabels[i] = Label("Texto", row, 60, 0, 250, 75, options[i], serif, i == 0 ? 44 : 34, i == 0 ? Gold : Cream);
                menu.mainLabels[i].fontStyle = i == 0 ? FontStyle.Bold : FontStyle.Normal;
                if (i < 3)
                {
                    Box("Separador", main, 75, 452 + i * 85, 273, 2, new Color(.83f, .65f, .35f, .7f));
                    var diamond = Box("Losango", main, 211, 449 + i * 85, 6, 6, Gold);
                    diamond.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
                }
            }
            var compass = Box("Bussola_Selecao", main, 56, 379, 64, 64, Color.white);
            compass.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Art/Bussola.png");
            compass.preserveAspect = true;
            menu.compass = compass.rectTransform;
            Label("Controles", main, 30, 885, 360, 28, "↑ ↓ / W S · selecionar     Enter · confirmar", body, 15, Cream);

            Picture("Mapa", destinations, 0, 0, 1672, 941, background);
            Box("Fundo_Escuro", destinations, 0, 0, 1672, 941, new Color(.025f, .06f, .08f, .94f));
            Label("Titulo", destinations, 100, 130, 1472, 65, "ESCOLHA SEU DESTINO", serif, 42, Gold);
            Label("Subtitulo", destinations, 100, 205, 1472, 32, "Escolha o país que deseja conhecer.", body, 22, Cream);
            menu.cards = new RectTransform[4]; menu.cardBorders = new Image[4];
            menu.countryLabels = new Text[4]; menu.countryStatus = new Text[4];
            string[] names = { "EGITO", "BRASIL", "ITÁLIA", "JAPÃO" };
            string[] locations = { "Areias do Tempo", "Rio de Janeiro", "Pontes", "Templo" };
            for (int i = 0; i < 4; i++)
            {
                RectTransform card = Rect(names[i], destinations, 100 + i * 370, 310, 340, 420);
                // O pivô central permite aumentar a janela sem empurrar as vizinhas.
                card.pivot = new Vector2(.5f, .5f);
                card.anchoredPosition += new Vector2(170, -210);
                menu.cards[i] = card;
                menu.cardBorders[i] = Box("Borda", card, 0, 0, 340, 420, Gold);
                Box("Fundo", card, 3, 3, 334, 414, new Color(.035f, .065f, .08f));
                Texture2D preview = i < 2 ? AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Art/" + (i == 0 ? "Egito.png" : "Brasil.png")) : AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TCCExploracao/Generated/" + (i == 2 ? "Italia" : "Japao") + "_Previa.png");
                RawImage picture = Picture("Previa", card, 10, 10, 320, 255, preview);
                picture.uvRect = Crop(preview, 320f / 255f);
                menu.countryLabels[i] = Label("Pais", card, 10, 285, 320, 48, names[i], serif, 33, Cream);
                Label("Local", card, 10, 335, 320, 30, locations[i], body, 20, Cream);
                menu.countryStatus[i] = Label("Estado", card, 10, 380, 320, 24, i < 2 ? "" : "EXPLORAR", body, 16, i < 2 ? Gold : new Color(.65f, .67f, .66f));
                MakeButton(card, menu, i, true);
            }
            var back = Rect("Voltar", destinations, 100, 818, 180, 48);
            Image backImage = back.gameObject.AddComponent<Image>(); backImage.color = new Color(.08f, .14f, .17f);
            Button backButton = back.gameObject.AddComponent<Button>(); backButton.targetGraphic = backImage;
            backButton.navigation = new Navigation { mode = Navigation.Mode.None };
            Label("Texto", back, 0, 0, 180, 48, "← Voltar", body, 22, Cream);
            UnityEventTools.AddPersistentListener(backButton.onClick, menu.ShowMain);
            Label("Controles", destinations, 300, 823, 1250, 38, "← → / A D · escolher país     Enter · viajar     Esc · voltar", body, 22, Cream);
            menu.notice = Label("Aviso", frame, 460, 860, 1150, 45, "", body, 23, Gold);
            destinations.gameObject.SetActive(false);
            EditorSceneManager.SaveScene(scene, TravelMenu.ScenePath);
            // O menu é a primeira cena; preservo as demais entradas que já existiam no projeto.
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != TravelMenu.ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(TravelMenu.ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("TRAVEL_MENU_SETUP_OK");
        }

        private static void ConfigureTexture(string file, bool sprite, int size)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(Root + "Art/" + file);
            importer.textureType = sprite ? TextureImporterType.Sprite : TextureImporterType.Default;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = sprite; importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp; importer.maxTextureSize = size;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height);
            return rect;
        }
        private static Text Label(string name, Transform parent, float x, float y, float w, float h, string value, Font font, int size, Color color)
        {
            var text = Rect(name, parent, x, y, w, h).gameObject.AddComponent<Text>();
            text.text = value; text.font = font; text.fontSize = size; text.color = color;
            text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            return text;
        }
        private static Image Box(string name, Transform parent, float x, float y, float w, float h, Color color)
        {
            var image = Rect(name, parent, x, y, w, h).gameObject.AddComponent<Image>();
            image.color = color; image.raycastTarget = false; return image;
        }
        private static RawImage Picture(string name, Transform parent, float x, float y, float w, float h, Texture texture)
        {
            var image = Rect(name, parent, x, y, w, h).gameObject.AddComponent<RawImage>();
            image.texture = texture; image.raycastTarget = false; return image;
        }
        private static Rect Crop(Texture image, float targetAspect)
        {
            float width = Mathf.Min(1, targetAspect / ((float)image.width / image.height));
            return new Rect((1 - width) / 2, 0, width, 1);
        }
        private static void MakeButton(RectTransform rect, TravelMenu menu, int index, bool country)
        {
            var image = rect.gameObject.AddComponent<Image>(); image.color = Color.clear;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            var pointer = rect.gameObject.AddComponent<MenuPointer>();
            pointer.menu = menu; pointer.index = index; pointer.country = country;
            UnityEventTools.AddPersistentListener(button.onClick, pointer.Click);
        }
    }
}
