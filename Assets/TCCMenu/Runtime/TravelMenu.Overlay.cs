using UnityEngine;
using UnityEngine.UI;
using CrossingTheWorld.TCCEgypt;

namespace CrossingTheWorld.TCCMenu
{
    // Painéis do menu com mouse e teclado, sem trocar de cena.
    public sealed partial class TravelMenu
    {
        private enum MenuOverlay { None, Options, Credits, Quit }
        private MenuOverlay activeOverlay;
        private int overlaySelection;
        private float volume = 1f;
        private GUIStyle overlayTitle, overlayBody, overlayButton;
        private bool OverlayVisible => activeOverlay != MenuOverlay.None;

        private void RestorePreferences()
        {
            volume = Mathf.Clamp01(PlayerPrefs.GetFloat("TCC.Menu.Volume", 1f));
            AudioListener.volume = volume;
            if (PlayerPrefs.HasKey("TCC.Menu.Fullscreen")) Screen.fullScreen = PlayerPrefs.GetInt("TCC.Menu.Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
            if (PlayerPrefs.HasKey("TCC.Menu.Quality")) QualitySettings.SetQualityLevel(
                Mathf.Clamp(PlayerPrefs.GetInt("TCC.Menu.Quality", QualitySettings.GetQualityLevel()), 0, Mathf.Max(0, QualitySettings.names.Length - 1)), true);
        }

        private void SetOverlay(MenuOverlay next)
        {
            if (EgyptSceneTransition.IsBusy) return;
            activeOverlay = next;
            overlaySelection = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            var raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster != null) raycaster.enabled = !OverlayVisible;
        }

        public void OpenOptions() { SetOverlay(MenuOverlay.Options); }
        public void OpenCredits() { SetOverlay(MenuOverlay.Credits); }
        public void OpenQuitConfirmation() { SetOverlay(MenuOverlay.Quit); }

        private void MoveOverlaySelection(int direction)
        {
            int number = activeOverlay == MenuOverlay.Options ? 4 : activeOverlay == MenuOverlay.Quit ? 2 : 1;
            overlaySelection = (overlaySelection + direction + number) % number;
        }

        private void ConfirmOverlaySelection()
        {
            if (activeOverlay == MenuOverlay.Credits) { ShowMain(); return; }
            if (activeOverlay == MenuOverlay.Quit)
            {
                if (overlaySelection == 0) QuitGame();
                else ShowMain();
                return;
            }
            if (activeOverlay != MenuOverlay.Options) return;
            switch (overlaySelection)
            {
                case 0: SetVolume(Mathf.Clamp01(volume + .1f)); break;
                case 1: ToggleFullscreen(); break;
                case 2: CycleQuality(); break;
                case 3: ShowMain(); break;
            }
        }

        private void SetVolume(float value)
        {
            volume = Mathf.Clamp01(value);
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat("TCC.Menu.Volume", volume);
            PlayerPrefs.Save();
        }
        private static void ToggleFullscreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
            PlayerPrefs.SetInt("TCC.Menu.Fullscreen", Screen.fullScreen ? 1 : 0);
            PlayerPrefs.Save();
        }
        private static void CycleQuality()
        {
            var names = QualitySettings.names;
            if (names == null || names.Length == 0) return;
            int quality = (QualitySettings.GetQualityLevel() + 1) % names.Length;
            QualitySettings.SetQualityLevel(quality, true);
            PlayerPrefs.SetInt("TCC.Menu.Quality", quality);
            PlayerPrefs.Save();
        }
        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnGUI()
        {
            if (!OverlayVisible || EgyptSceneTransition.IsBusy) return;
            float scale = Mathf.Min(Screen.width / 1200f, Screen.height / 720f);
            var oldMatrix = GUI.matrix;
            var oldColor = GUI.color;
            GUI.matrix = Matrix4x4.TRS(
                new Vector3((Screen.width - 1200f * scale) / 2f, (Screen.height - 720f * scale) / 2f, 0),
                Quaternion.identity, Vector3.one * scale);
            if (overlayTitle == null)
            {
                overlayTitle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 31, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter
                };
                overlayTitle.normal.textColor = new Color(1f,.77f,.37f);
                overlayBody = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 19, wordWrap = true, alignment = TextAnchor.MiddleCenter
                };
                overlayBody.normal.textColor = Color.white;
                overlayButton = new GUIStyle(GUI.skin.button) { fontSize = 19, fontStyle = FontStyle.Bold };
            }
            GUI.color = new Color(0f,0f,0f,.91f);
            GUI.DrawTexture(new Rect(0,0,1200,720), Texture2D.whiteTexture);
            GUI.color = new Color(.055f,.09f,.105f,1f);
            GUI.DrawTexture(new Rect(264,128,672,462), Texture2D.whiteTexture);
            GUI.color = new Color(.93f,.72f,.31f,1f);
            GUI.DrawTexture(new Rect(264,128,672,4), Texture2D.whiteTexture);
            GUI.color = Color.white;
            string heading = activeOverlay == MenuOverlay.Options ? "OPÇÕES" :
                activeOverlay == MenuOverlay.Credits ? "CRÉDITOS" : "SAIR DO JOGO?";
            GUI.Label(new Rect(300,154,600,58), heading, overlayTitle);

            if (activeOverlay == MenuOverlay.Options)
            {
                GUI.Label(new Rect(302,228,596,30), "Volume do jogo", overlayBody);
                float next = GUI.HorizontalSlider(new Rect(386,270,427,24),volume,0f,1f);
                if (Mathf.Abs(next-volume)>.002f) SetVolume(next);
                GUI.Label(new Rect(770,295,106,32),Mathf.RoundToInt(volume*100f)+"%",overlayBody);
                if (GUI.Button(new Rect(335,336,530,48),
                    "Tela cheia: "+(Screen.fullScreen ? "SIM":"NÃO"),overlayButton)) ToggleFullscreen();
                var qualities = QualitySettings.names;
                string quality = qualities != null && qualities.Length > 0
                    ? qualities[Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, qualities.Length - 1)] : "Padrão";
                if (GUI.Button(new Rect(335,401,530,48),
                    "Qualidade: "+quality,overlayButton)) CycleQuality();
                if (GUI.Button(new Rect(335,471,530,50),"VOLTAR AO MENU",overlayButton)) ShowMain();
                GUI.Label(new Rect(335,537,530,30),"Esc: voltar  ·  Enter: confirmar",overlayBody);
            }
            else if (activeOverlay == MenuOverlay.Credits)
            {
                GUI.Label(new Rect(317,228,566,176),
                    "ACROSSING THE WORLD — TCC\n\nProjeto educacional de exploração cultural.\nDesenvolvido pela equipe do TCC.\n\nCenários: Egito, Brasil, Itália e Japão.",overlayBody);
                if (GUI.Button(new Rect(365,468,470,54),"VOLTAR AO MENU",overlayButton)) ShowMain();
            }
            else
            {
                GUI.Label(new Rect(330,238,540,110),
                    "Tem certeza de que deseja sair?\nNo Editor, o botão encerra o Play Mode.",overlayBody);
                if (GUI.Button(new Rect(330,382,540,50),"SIM, SAIR DO JOGO",overlayButton)) QuitGame();
                if (GUI.Button(new Rect(330,453,540,50),"CANCELAR / VOLTAR",overlayButton)) ShowMain();
            }
            GUI.matrix = oldMatrix;
            GUI.color = oldColor;
        }
    }
}
