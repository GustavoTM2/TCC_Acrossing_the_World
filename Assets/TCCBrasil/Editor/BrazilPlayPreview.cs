using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CrossingTheWorld.TCCBrazil.Editor
{
    // Abre a praia e mantém o jogo rodando para o teste manual.
    [InitializeOnLoad]
    public static class BrazilPlayPreview
    {
        private const string Pending = "TCC.Brazil.UserPreview";
        static BrazilPlayPreview()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Pending,false)) return;
                SessionState.SetBool(Pending,false);
                FocusGame();
                Debug.Log("BRAZIL_USER_PLAY_READY: praia aberta para teste manual; Play permanece ligado.");
            };
        }
        public static void OpenForUser()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { FocusGame(); return; }
            if (Application.unityVersion != "6000.5.7f1") throw new InvalidOperationException("Use Unity 6000.5.7f1.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(BrazilSceneSetup.ScenePath);
            SessionState.SetBool(Pending,true); FocusGame(); EditorApplication.EnterPlaymode();
        }
        private static void FocusGame()
        {
            var game = EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView"));
            game.Show(); game.Focus();
        }
    }
}
