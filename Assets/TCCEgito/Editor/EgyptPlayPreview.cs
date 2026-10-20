using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CrossingTheWorld.TCCEgypt.Editor
{
    // Abre a fase para testar manualmente e mantém o Play ligado.
    [InitializeOnLoad]
    public static class EgyptPlayPreview
    {
        private const string Pending = "TCC.Egypt.UserPreview";

        static EgyptPlayPreview()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Pending, false)) return;
                SessionState.SetBool(Pending, false);
                FocusGame();
                Debug.Log("EGYPT_USER_PLAY_READY: fase aberta para teste manual; Play permanece ligado.");
            };
        }

        [MenuItem("TCC/Egito/Jogar para testar")]
        public static void OpenForUser()
        {
            OpenSceneForUser(EgyptPresentationSetup.Exterior);
        }

        [MenuItem("TCC/Egito/Testar interior da piramide")]
        public static void OpenInteriorForUser()
        {
            OpenSceneForUser(EgyptPresentationSetup.Interior);
        }

        private static void OpenSceneForUser(string scene)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { FocusGame(); return; }
            if (Application.unityVersion != "6000.5.7f1") throw new InvalidOperationException("Use o Unity 6000.5.7f1.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(scene);
            SessionState.SetBool(Pending, true);
            FocusGame();
            EditorApplication.EnterPlaymode();
        }

        private static void FocusGame()
        {
            EditorWindow game = EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView"));
            game.Show();
            game.Focus();
        }
    }
}
