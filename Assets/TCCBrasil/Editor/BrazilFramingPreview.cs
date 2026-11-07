using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CrossingTheWorld.TCCBrazil.Editor
{
    // Registra a visão inicial, a chegada ao morador e o enquadramento do desafio.
    [InitializeOnLoad]
    public static class BrazilFramingPreview
    {
        private const string Key = "TCC.Brazil.FramingPreview";
        private static int frames, step, lastFrame;
        static BrazilFramingPreview() { EditorApplication.update += Tick; }
        public static void ApplyAndPlay()
        {
            BrazilBeachFraming.Apply();
            RunAndLeaveOpen();
        }
        public static void RunAndLeaveOpen()
        {
            EditorSceneManager.OpenScene(BrazilSceneSetup.ScenePath);
            CrossingTheWorld.TCCMenu.TravelProgress.Reset();
            SessionState.SetBool(Key, true);
            FocusGame();
            EditorApplication.EnterPlaymode();
        }
        private static void Tick()
        {
            if (!SessionState.GetBool(Key, false) || !EditorApplication.isPlaying) return;
            Application.runInBackground = true;
            EditorApplication.QueuePlayerLoopUpdate();
            if (lastFrame == Time.frameCount) return;
            lastFrame = Time.frameCount;
            if (++frames < 120) return;
            frames = 0;
            var stage = Object.FindAnyObjectByType<BrazilStage>();
            stage.enabled = false; stage.player.enabled = false;
            Capture(stage.followCamera.GetComponent<Camera>(), new[] { "01-chegada", "02-morador", "03-desafio" }[step]);
            if (step == 0)
            {
                stage.OpenDialogue(true);
                while (stage.State == BrazilState.Dialogue) stage.AdvanceDialogue();
                stage.player.PlaceAt(stage.challengeAnchor.position);
            }
            else if (step == 1)
            {
                stage.OpenDialogue(false);
                while (stage.State == BrazilState.Dialogue) stage.AdvanceDialogue();
                stage.StartChallenge();
            }
            else
            {
                SessionState.SetBool(Key, false);
                CrossingTheWorld.TCCMenu.TravelProgress.Reset();
                SceneManager.LoadScene(BrazilSceneSetup.ScenePath);
                FocusGame();
                Debug.Log("BRAZIL_FRAMING_PREVIEW_READY: capturas em Temp/BrazilFraming; fase aberta para jogar.");
            }
            step++;
        }
        private static void Capture(Camera camera, string name)
        {
            string output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/BrazilFraming"));
            Directory.CreateDirectory(output);
            var previous = RenderTexture.active;
            var target = camera.targetTexture;
            var texture = new RenderTexture(1280, 720, 24);
            var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            camera.targetTexture = texture;
            camera.Render();
            RenderTexture.active = texture;
            image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); image.Apply();
            File.WriteAllBytes(Path.Combine(output, name + ".png"), image.EncodeToPNG());
            camera.targetTexture = target;
            RenderTexture.active = previous;
            Object.DestroyImmediate(image); Object.DestroyImmediate(texture);
        }
        private static void FocusGame()
        {
            var game = EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView"));
            game.Show(); game.Focus();
        }
    }
}
