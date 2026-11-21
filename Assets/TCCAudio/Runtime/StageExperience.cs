using UnityEngine;
using UnityEngine.SceneManagement;
using CrossingTheWorld.TCCEgypt;
using CrossingTheWorld.TCCMenu;
using CrossingTheWorld.TCCBrazil;
using Viaggio.Italia;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CrossingTheWorld.TCCAudio
{
    // Interface central de pausa, luz e trilha original para todos os países.
    [DefaultExecutionOrder(10000)]
    public sealed class StageExperience : MonoBehaviour
    {
        public static StageExperience Instance { get; private set; }
        public static bool IsPaused => Instance != null && Instance.paused;
        public static bool IsCountry => Instance != null && Instance.country >= 0;
        private AudioSource current, fadingOut;
        private int country=-1, track=-1;
        private bool paused, goingBack;
        private float fade=1f, brightness=1f, originalAmbient=1f, nextMusicCheck;
        private Light[] sceneLights;
        private float[] baseLightIntensities;
        private Color accent;
        private string countryName;
        private GUIStyle heading, text, small, button;
        private readonly string[] music = {
            "Menu_Viagem", "Egito_Deserto", "Egito_Piramide",
            "Brasil_Praia", "Brasil_Embaixadinhas",
            "Italia_Cidade", "Italia_Restaurante", "Italia_Pizzaria_Desafio",
            "Japao_Jardim", "Japao_Templo"
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            if (Instance != null) return;
            var obj=new GameObject("TCC_Musica_Pausa");
            DontDestroyOnLoad(obj);
            Instance=obj.AddComponent<StageExperience>();
            SceneManager.sceneLoaded += Instance.SceneLoaded;
        }
        private void Awake()
        {
            if (Instance != null && Instance!=this) { Destroy(gameObject); return; }
            Instance=this;
            current=gameObject.AddComponent<AudioSource>();
            fadingOut=gameObject.AddComponent<AudioSource>();
            foreach(var a in new[]{current,fadingOut})
            {
                a.spatialBlend=0f; a.loop=true; a.playOnAwake=false;
                a.volume=0f; a.ignoreListenerPause=true;
            }
            brightness=Mathf.Clamp(PlayerPrefs.GetFloat("TCC.Fase.Luz",1f),.55f,1.35f);
            AudioListener.volume=Mathf.Clamp01(PlayerPrefs.GetFloat("TCC.Menu.Volume",1f));
        }
        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Time.timeScale=1f;
            paused=false; goingBack=false;
            string p=scene.path.Replace('\\','/');
            country=p.Contains("/TCCEgito/")?0:p.Contains("/TCCBrasil/")?1:
                p.Contains("/TCCItalia/")?2:p.Contains("/TCCJapao/")?3:-1;
            if(country==0) {countryName="EGITO"; accent=new Color(.85f,.68f,.39f);}
            else if(country==1) {countryName="BRASIL";accent=new Color(.36f,.81f,.62f);}
            else if(country==2) {countryName="ITÁLIA";accent=new Color(.91f,.70f,.41f);}
            else if(country==3) {countryName="JAPÃO";accent=new Color(.89f,.50f,.52f);}
            else {countryName="MENU";accent=new Color(.95f,.74f,.39f);}
            originalAmbient=RenderSettings.ambientIntensity;
            sceneLights=FindObjectsByType<Light>(FindObjectsInactive.Exclude);
            baseLightIntensities=new float[sceneLights.Length];
            for(int i=0;i<sceneLights.Length;i++) baseLightIntensities[i]=sceneLights[i].intensity;
            ApplyLight();
            UpdateSound();
        }
        private void Update()
        {
            if(country>=0 && !EgyptSceneTransition.IsBusy && Esc()) Pause(!paused);
            if (Time.unscaledTime>=nextMusicCheck)
            {
                nextMusicCheck=Time.unscaledTime+.25f;
                UpdateSound();
            }
            if(fade<1f)
            {
                fade=Mathf.Min(1f,fade+Time.unscaledDeltaTime/1.4f);
                fadingOut.volume=.45f*(1f-fade);
                current.volume=.45f*fade;
                if(fade>=1f) fadingOut.Stop();
            }
            if(paused) {Cursor.visible=true; Cursor.lockState=CursorLockMode.None;}
        }
        private void UpdateSound()
        {
            string p=SceneManager.GetActiveScene().path;
            int wanted=0;
            if(country==0) wanted=p.Contains("Interior")?2:1;
            else if(country==1)
            {
                var b=FindAnyObjectByType<BrazilStage>();
                wanted=b!=null && (b.State==BrazilState.Playing || b.State==BrazilState.Countdown ||
                    b.State==BrazilState.Ready || b.State==BrazilState.Failed)?4:3;
            }
            else if(country==2)
            {
                var it=FindAnyObjectByType<ItaliaFase>();
                wanted=!p.Contains("RESTAURANTE")?5:it!=null && it.InPizzaMinigame?7:6;
            }
            else if(country==3) wanted=p.Contains("INTERIOR")?9:8;
            if(wanted==track) return;
            AudioClip clip=Resources.Load<AudioClip>("Music/"+music[wanted]);
            if(clip==null) {Debug.LogWarning("Não foi encontrada a música: "+music[wanted]);return;}
            track=wanted;
            var old=current;
            current=fadingOut;
            fadingOut=old;
            current.Stop();current.clip=clip;current.volume=0f;
            current.Play();
            fade=0f;
        }
        private static bool Esc()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Escape);
#else
            return false;
#endif
        }
        private void Pause(bool state)
        {
            paused=state;
            Time.timeScale=paused?0f:1f;
            if(paused) {Cursor.visible=true; Cursor.lockState=CursorLockMode.None;}
        }
        private void ApplyLight()
        {
            float factor=Mathf.Lerp(.58f,1.38f,Mathf.InverseLerp(.55f,1.35f,brightness));
            RenderSettings.ambientIntensity=originalAmbient*factor;
            if(sceneLights!=null && baseLightIntensities!=null)
            {
                for(int i=0;i<sceneLights.Length;i++)
                    if(sceneLights[i]!=null) sceneLights[i].intensity=baseLightIntensities[i]*factor;
            }
        }
        private void AdjustLight(float value)
        {
            brightness=Mathf.Clamp(value,.55f,1.35f);
            PlayerPrefs.SetFloat("TCC.Fase.Luz",brightness);PlayerPrefs.Save();
            ApplyLight();
        }
        private static void AdjustVolume(float value)
        {
            AudioListener.volume=Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("TCC.Menu.Volume",AudioListener.volume);PlayerPrefs.Save();
        }
        private void BackToMenu()
        {
            if(goingBack || EgyptSceneTransition.IsBusy) return;
            goingBack=true;
            Pause(false);
            EgyptSceneTransition.Load(TravelMenu.ScenePath,null,"Voltando ao menu...");
        }
        private void Style()
        {
            if(heading!=null)return;
            heading=new GUIStyle(GUI.skin.label){fontSize=29,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};
            text=new GUIStyle(GUI.skin.label){fontSize=19,alignment=TextAnchor.MiddleCenter};
            small=new GUIStyle(GUI.skin.label){fontSize=15,alignment=TextAnchor.MiddleRight};
            button=new GUIStyle(GUI.skin.button){fontSize=20,fontStyle=FontStyle.Bold};
            heading.normal.textColor=text.normal.textColor=small.normal.textColor=Color.white;
        }
        private void OnGUI()
        {
            if(country<0 || EgyptSceneTransition.IsBusy)return;
            Style();
            var oldMatrix=GUI.matrix;
            var oldColor=GUI.color;
            float scale=Mathf.Min(Screen.width/1200f,Screen.height/700f);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1200f*scale)/2,(Screen.height-700f*scale)/2,0),
                Quaternion.identity,Vector3.one*scale);
            if(brightness<1f)
            {
                GUI.color=new Color(0,0,0,(1f-brightness)*.62f);
                GUI.DrawTexture(new Rect(0,0,1200,700),Texture2D.whiteTexture);
            }
            // A dica Esc agora faz parte das instruções inferiores de cada fase.
            // Fora da pausa não desenhamos nenhum painel sobre o cenário.
            if(paused)
            {
                GUI.color=new Color(0,0,0,.83f);
                GUI.DrawTexture(new Rect(0,0,1200,700),Texture2D.whiteTexture);
                GUI.color=new Color(.055f,.085f,.105f,1f);
                GUI.DrawTexture(new Rect(302,90,596,527),Texture2D.whiteTexture);
                GUI.color=accent;
                GUI.DrawTexture(new Rect(302,90,596,5),Texture2D.whiteTexture);
                GUI.color=Color.white;
                GUI.Label(new Rect(326,112,548,55),countryName+"  /  MENU DA FASE",heading);
                GUI.Label(new Rect(344,185,512,31),"VOLUME",text);
                float value=GUI.HorizontalSlider(new Rect(388,231,424,24),AudioListener.volume,0f,1f);
                if(Mathf.Abs(value-AudioListener.volume)>.002f)AdjustVolume(value);
                GUI.Label(new Rect(388,261,424,24),Mathf.RoundToInt(AudioListener.volume*100f)+"%",text);
                GUI.Label(new Rect(344,291,512,31),"LUMINOSIDADE",text);
                float light=GUI.HorizontalSlider(new Rect(388,339,424,24),brightness,.55f,1.35f);
                if(Mathf.Abs(light-brightness)>.002f)AdjustLight(light);
                GUI.Label(new Rect(388,365,424,25),Mathf.RoundToInt(brightness*100f)+"%",text);
                if(GUI.Button(new Rect(374,422,452,55),"CONTINUAR JOGANDO",button))Pause(false);
                if(GUI.Button(new Rect(374,496,452,55),"VOLTAR AO MENU PRINCIPAL",button))BackToMenu();
                GUI.Label(new Rect(353,571,494,25),"Pressione ESC para continuar",text);
            }
            GUI.matrix=oldMatrix;
            GUI.color=oldColor;
        }
        private void OnDestroy()
        {
            if(Instance==this)
            {
                SceneManager.sceneLoaded-=SceneLoaded;
                Time.timeScale=1f;
                Instance=null;
            }
        }
    }
}

namespace CrossingTheWorld.TCCAudio
{
    /// <summary>
    /// HUD de instruções unificada para as quatro fases (IMGUI).
    /// Recebe a cor do país e é desenhada dentro da matriz de escala da fase.
    /// </summary>
    public static class StageControlsHUD
    {
        private static GUIStyle headingStyle, keyStyle, descriptionStyle, detailStyle;
        private static readonly string[] fourKeys = { "A / D", "W / S", "E / Enter", "Esc" };
        private static readonly string[] fourLabels = { "Esquerda / direita", "Cima / baixo", "Interagir", "Menu da fase" };
        private static readonly string[] egyptKeys = { "A / D", "S / W", "Espaço", "E / Enter", "Esc" };
        private static readonly string[] egyptLabels = { "Esquerda / direita", "S: cima · W: baixo", "Pular", "Interagir", "Menu da fase" };

        private static void Init()
        {
            if (headingStyle != null) return;
            headingStyle = NewStyle(15, FontStyle.Bold, TextAnchor.MiddleLeft);
            keyStyle = NewStyle(15, FontStyle.Bold, TextAnchor.MiddleCenter);
            descriptionStyle = NewStyle(16, FontStyle.Normal, TextAnchor.MiddleLeft);
            detailStyle = NewStyle(11, FontStyle.Normal, TextAnchor.MiddleRight);
        }

        private static GUIStyle NewStyle(int size, FontStyle fontStyle, TextAnchor alignment)
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = size,
                fontStyle = fontStyle,
                alignment = alignment,
                wordWrap = false,
                richText = false,
                clipping = TextClipping.Clip,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };
        }

        private static void Fill(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
        }

        public static void Draw(Rect area, Color accent, bool egyptInvertedWS, bool includesJump)
        {
            Init();
            Color oldColor = GUI.color;
            Color oldBackground = GUI.backgroundColor;
            try
            {
                string[] keys = egyptInvertedWS ? egyptKeys : fourKeys;
                string[] descriptions = egyptInvertedWS ? egyptLabels : fourLabels;
                int count = includesJump ? 5 : 4;
                if (count == 5 && !egyptInvertedWS)
                {
                    // No jogo atual, somente o Egito usa pulo na HUD de exploração.
                    keys = egyptKeys;
                    descriptions = egyptLabels;
                }

                bool narrow = area.width < 300f;
                headingStyle.fontSize = narrow ? 14 : 15;
                keyStyle.fontSize = narrow ? 13 : 15;
                descriptionStyle.fontSize = narrow ? 14 : 16;
                detailStyle.fontSize = narrow ? 10 : 11;
                headingStyle.normal.textColor = accent;
                keyStyle.normal.textColor = new Color(.98f, .97f, .92f);
                descriptionStyle.normal.textColor = new Color(.94f, .96f, .95f);
                detailStyle.normal.textColor = new Color(.68f, .73f, .73f);

                // Fundo escuro + borda delicada na cor temática do país.
                Fill(new Rect(area.x + 3, area.y + 4, area.width, area.height),
                    new Color(0f, 0f, 0f, .29f));
                Fill(area, new Color(accent.r, accent.g, accent.b, .75f));
                Fill(new Rect(area.x + 1.5f, area.y + 1.5f,
                    area.width - 3f, area.height - 3f),
                    new Color(.053f, .074f, .084f, .97f));
                Fill(new Rect(area.x + 1.5f, area.y + 1.5f, 4f,
                    area.height - 3f), accent);

                float left = area.x + 16f;
                float right = area.xMax - 12f;
                GUI.color = Color.white;
                GUI.Label(new Rect(left, area.y + 7, area.width - 98f, 23f),
                    "CONTROLES", headingStyle);
                GUI.Label(new Rect(right - 83f, area.y + 7, 83f, 23f),
                    "TECLADO", detailStyle);
                Fill(new Rect(left, area.y + 34f, area.width - 28f, 1f),
                    new Color(accent.r, accent.g, accent.b, .43f));

                float rowTop = area.y + 41f;
                float rowPitch = (area.height - 53f) / count;
                float keyWidth = narrow ? 77f : 91f;
                for (int i = 0; i < count; i++)
                {
                    float y = rowTop + i * rowPitch;
                    float h = Mathf.Min(25f, rowPitch - 2f);
                    Fill(new Rect(left, y, keyWidth, h),
                        new Color(accent.r * .27f, accent.g * .27f, accent.b * .27f, .95f));
                    Fill(new Rect(left, y, 2.5f, h), accent);
                    GUI.color = Color.white;
                    GUI.Label(new Rect(left + 3f, y, keyWidth - 6f, h), keys[i], keyStyle);
                    float xText = left + keyWidth + 11f;
                    GUI.Label(new Rect(xText, y, Mathf.Max(45f, right - xText), h),
                        descriptions[i], descriptionStyle);
                }
            }
            finally
            {
                GUI.color = oldColor;
                GUI.backgroundColor = oldBackground;
            }
        }
    }
}
