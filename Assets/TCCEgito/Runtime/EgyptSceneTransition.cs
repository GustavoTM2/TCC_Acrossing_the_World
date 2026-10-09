using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrossingTheWorld.TCCEgypt
{
    public sealed class EgyptSceneTransition : MonoBehaviour
    {
        private static EgyptSceneTransition instance;
        public static bool IsBusy => instance != null && instance.busy;
        private bool busy;
        private float opacity;
        private string caption;
        private GUIStyle label;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() { instance = null; }

        public static void Load(string path, EgyptStage owner)
        {
            if (IsBusy) return;
            if (instance == null)
            {
                instance = FindFirstObjectByType<EgyptSceneTransition>();
                if (instance == null) instance = new GameObject("Transicao_Egito").AddComponent<EgyptSceneTransition>();
                DontDestroyOnLoad(instance.gameObject);
            }
            instance.busy = true;
            instance.caption = path.Contains("Interior") ? "Entrando na pirâmide..." : "Voltando ao deserto...";
            instance.StartCoroutine(instance.ChangeScene(path, owner));
        }

        private IEnumerator ChangeScene(string path, EgyptStage owner)
        {
            yield return Fade(0, 1, 0.45f);
            AsyncOperation loading = BeginLoad(path);
            if (loading == null)
            {
                if (owner != null) owner.Notify("Não foi possível carregar a cena. Confira o Console.");
                yield return Fade(1, 0, 0.35f);
                busy = false;
                yield break;
            }
            while (!loading.isDone) yield return null;
            // Aguarda Awake/Start, inclusive o enquadramento da câmera, mantendo a tela preta.
            yield return null;
            yield return null;
            yield return new WaitForSecondsRealtime(0.15f);
            yield return Fade(1, 0, 0.45f);
            busy = false;
        }

        private static AsyncOperation BeginLoad(string path)
        {
            try { return SceneManager.LoadSceneAsync(path); }
            catch (Exception exception) { Debug.LogException(exception); return null; }
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            opacity = from;
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                opacity = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }
            opacity = to;
        }

        private void OnGUI()
        {
            if (!busy && opacity <= 0) return;
            Matrix4x4 matrix = GUI.matrix;
            Color color = GUI.color;
            int depth = GUI.depth;
            GUI.depth = -10000;
            GUI.matrix = Matrix4x4.identity;
            GUI.color = new Color(1, 1, 1, opacity);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.blackTexture);
            if (opacity > 0.95f)
            {
                if (label == null) label = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,
                    fontSize = 20, normal = { textColor = new Color(0.9f, 0.79f, 0.58f) } };
                GUI.Label(new Rect(0, Screen.height * 0.73f, Screen.width, 50), caption, label);
            }
            GUI.depth = depth;
            GUI.color = color;
            GUI.matrix = matrix;
        }

        private void OnDestroy() { if (instance == this) instance = null; }
    }
}
