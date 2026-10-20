using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CrossingTheWorld.TCCEgypt
{
    public sealed class EgyptSceneTransition : MonoBehaviour
    {
        private static EgyptSceneTransition instance;
        public static bool IsBusy => instance != null && instance.busy;
        public float Opacity => overlay != null ? overlay.alpha : 0;
        private bool busy;
        private Canvas canvas;
        private CanvasGroup overlay;
        private Text caption;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() { instance = null; }

        // Inicio a transição e evito que duas trocas de cena aconteçam ao mesmo tempo.
        public static void Load(string path, EgyptStage owner)
        {
            if (IsBusy) return;
            if (string.IsNullOrWhiteSpace(path) || !Application.CanStreamedLevelBeLoaded(path))
            {
                if (owner != null) owner.Notify("A cena de destino não está disponível.");
                return;
            }
            if (instance == null)
            {
                instance = FindFirstObjectByType<EgyptSceneTransition>();
                if (instance == null) instance = new GameObject("Transicao_Egito").AddComponent<EgyptSceneTransition>();
                // Mantenho este objeto durante a troca de cena para a tela preta continuar aparecendo.
                DontDestroyOnLoad(instance.gameObject);
            }
            instance.PrepareOverlay();
            instance.busy = true;
            instance.canvas.enabled = true;
            instance.caption.text = path.Contains("Interior") ? "Entrando na pirâmide..." : "Voltando ao deserto...";
            instance.StartCoroutine(instance.ChangeScene(path, owner));
        }

        // Escureço a tela, carrego a cena e depois mostro o cenário novamente.
        private IEnumerator ChangeScene(string path, EgyptStage owner)
        {
            // Só começo a carregar depois que o cenário antigo estiver totalmente coberto.
            yield return Fade(0, 1, 0.65f);
            Canvas.ForceUpdateCanvases();
            yield return null;
            AsyncOperation loading = BeginLoad(path);
            if (loading == null)
            {
                if (owner != null) owner.Notify("Não foi possível carregar a cena. Confira o Console.");
                yield return Fade(1, 0, 0.35f);
                Finish();
                yield break;
            }
            while (!loading.isDone) yield return null;
            // Aguarda Awake/Start, inclusive o enquadramento da câmera, mantendo a tela preta.
            yield return null;
            yield return null;
            yield return new WaitForSecondsRealtime(0.3f);
            yield return Fade(1, 0, 0.65f);
            Finish();
        }

        private static AsyncOperation BeginLoad(string path)
        {
            // Carrego a próxima cena de forma assíncrona enquanto a transição fica na tela.
            try { return SceneManager.LoadSceneAsync(path, LoadSceneMode.Single); }
            catch (Exception exception) { Debug.LogException(exception); return null; }
        }

        // Mudo a opacidade aos poucos para fazer a tela escurecer ou clarear suavemente.
        private IEnumerator Fade(float from, float to, float duration)
        {
            overlay.alpha = from;
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                overlay.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }
            overlay.alpha = to;
        }

        // A tela preta ocupa a janela inteira e não depende da câmera ou do tamanho dos diálogos.
        private void PrepareOverlay()
        {
            if (canvas != null) return;
            GameObject root = new GameObject("Tela_Preta", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasGroup), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            overlay = root.GetComponent<CanvasGroup>();
            overlay.alpha = 0;
            overlay.blocksRaycasts = true;
            root.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            root.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1280, 720);

            GameObject black = new GameObject("Fundo_Preto", typeof(RectTransform), typeof(Image));
            black.transform.SetParent(root.transform, false);
            RectTransform rect = black.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            black.GetComponent<Image>().color = Color.black;

            GameObject label = new GameObject("Aviso", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(root.transform, false);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0.18f);
            labelRect.anchorMax = new Vector2(1, 0.3f);
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            caption = label.GetComponent<Text>();
            caption.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            caption.fontSize = 22;
            caption.alignment = TextAnchor.MiddleCenter;
            caption.color = new Color(0.9f, 0.79f, 0.58f);
            caption.raycastTarget = false;
        }

        private void Finish()
        {
            overlay.alpha = 0;
            canvas.enabled = false;
            busy = false;
        }

        private void OnDestroy() { if (instance == this) instance = null; }
    }
}
