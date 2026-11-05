using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace CrossingTheWorld.TCCMenu.Editor
{
    [InitializeOnLoad]
    public static class TravelMenuPreview
    {
        private const string Pending = "TCC.Menu.Preview";
        static TravelMenuPreview()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Pending, false)) return;
                SessionState.SetBool(Pending, false);
                FocusGame();
                Debug.Log("TRAVEL_MENU_USER_PLAY_READY: menu aberto para teste manual.");
            };
        }
        [MenuItem("TCC/Menu/Jogar pelo menu")]
        public static void OpenForUser()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { FocusGame(); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(TravelMenu.ScenePath);
            SessionState.SetBool(Pending, true);
            FocusGame(); EditorApplication.EnterPlaymode();
        }
        private static void FocusGame()
        {
            var game = EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView"));
            game.Show(); game.Focus();
        }
    }
}
