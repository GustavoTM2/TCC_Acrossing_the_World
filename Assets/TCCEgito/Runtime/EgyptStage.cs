using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CrossingTheWorld.TCCEgypt
{
    [Serializable]
    public sealed class EgyptLine
    {
        public string speaker;
        [TextArea(2, 7)] public string text;
        public EgyptLine(string speaker, string text) { this.speaker = speaker; this.text = text; }
    }

    public sealed class EgyptStage : MonoBehaviour
    {
        public bool interior;
        public string exteriorScene = "Assets/TCCEgito/Scenes/Egito_Exterior_Jogavel.unity";
        public string interiorScene = "Assets/TCCEgito/Scenes/Egito_Interior_Jogavel.unity";
        public GameObject scarabVisual;
        public UnityEvent onGuideFinished = new UnityEvent();
        public UnityEvent onScarabReceived = new UnityEvent();
        public UnityEvent onStageCompleted = new UnityEvent();

        [Header("Textos adaptados do roteiro do TCC")]
        public EgyptLine[] guideLines =
        {
            new EgyptLine("Guia turístico", "Bem-vinda ao Egito, uma terra onde cada monumento guarda milhares de anos de história."),
            new EgyptLine("Guia turístico", "As pirâmides e a Esfinge fazem parte do patrimônio histórico deste destino. Nossa agência apresenta esses lugares em seus passeios culturais."),
            new EgyptLine("Guia turístico", "Dentro desta pirâmide vive uma antiga múmia, guardiã de um artefato conhecido como Velho Escaravelho."),
            new EgyptLine("Amira", "Então a lenda é verdadeira?"),
            new EgyptLine("Guia turístico", "Ela guarda memórias importantes sobre este lugar. Ouça sua história com atenção.")
        };
        public EgyptLine[] mummyLines =
        {
            new EgyptLine("Múmia", "Há milhares de anos, eu vivi nesta terra e acompanhei acontecimentos que hoje se transformaram em histórias."),
            new EgyptLine("Múmia", "Meu corpo foi preparado de acordo com os rituais funerários do antigo Egito, para que minha memória fosse preservada."),
            new EgyptLine("Múmia", "As pirâmides foram erguidas com planejamento, conhecimento de engenharia e o trabalho organizado de muitas pessoas."),
            new EgyptLine("Múmia", "Os blocos eram transportados e posicionados com técnicas desenvolvidas ao longo de muitos anos."),
            new EgyptLine("Amira", "Quero conhecer este lugar com respeito e aprender com suas histórias."),
            new EgyptLine("Múmia", "Você demonstrou respeito e curiosidade. Por isso, pode levar o Velho Escaravelho como lembrança desta jornada."),
            new EgyptLine("Amira", "Coragem não significa enfrentar tudo sozinha. Às vezes, confiar nas pessoas é o que nos leva mais longe.")
        };

        public static EgyptQuestProgress Progress { get; private set; } = new EgyptQuestProgress();
        public bool DialogueIsOpen => lines != null;
        public bool CompletionIsOpen { get; private set; }
        public bool IsLoading { get; private set; }
        public bool BlocksMovement => DialogueIsOpen || CompletionIsOpen || IsLoading;
        public string Hint { get; set; }
        private EgyptLine[] lines;
        private int lineIndex;
        private Action dialogueFinished;
        private string message;
        private float messageUntil;
        private int lastAdvanceFrame = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() { Progress = new EgyptQuestProgress(); }

        private void Awake()
        {
            // Permite testar o interior diretamente no Editor sem precisar percorrer o exterior.
            if (interior && Progress.Step == EgyptQuestStep.MeetGuide)
            {
                Progress.FinishGuide();
                Progress.EnterPyramid();
            }
            else if (interior && Progress.Step == EgyptQuestStep.EnterPyramid) Progress.EnterPyramid();
            if (scarabVisual != null) scarabVisual.SetActive(Progress.HasScarab);
        }

        public void Interact(EgyptPointKind kind)
        {
            if (BlocksMovement) return;
            switch (kind)
            {
                case EgyptPointKind.Guide:
                    if (Progress.Step == EgyptQuestStep.MeetGuide)
                        Open(guideLines, () => { if (Progress.FinishGuide()) onGuideFinished.Invoke(); });
                    else Open(new[] { new EgyptLine("Guia turístico", Progress.HasScarab
                        ? "Você voltou com novos aprendizados. Boa viagem, Amira!"
                        : "Entre na pirâmide e converse com a múmia.") }, null);
                    break;
                case EgyptPointKind.Entrance:
                    if (Progress.Step == EgyptQuestStep.MeetGuide) Notify("Converse com o guia antes de entrar na pirâmide.");
                    else LoadDestination(interiorScene);
                    break;
                case EgyptPointKind.Mummy:
                    if (Progress.Step == EgyptQuestStep.ListenToMummy)
                        Open(mummyLines, ReceiveScarab);
                    else if (Progress.HasScarab)
                        Open(new[] { new EgyptLine("Múmia", "Leve consigo o respeito pelas histórias deste lugar.") }, null);
                    else Notify("Converse com o guia e entre na pirâmide para continuar.");
                    break;
                case EgyptPointKind.Exit:
                    LoadDestination(exteriorScene);
                    break;
            }
        }

        private void Open(EgyptLine[] newLines, Action finished)
        {
            if (newLines == null || newLines.Length == 0)
            {
                Notify("Preencha o diálogo no Inspector para continuar.");
                return;
            }
            foreach (EgyptLine line in newLines)
                if (line == null || string.IsNullOrWhiteSpace(line.text))
                {
                    Notify("Há uma fala vazia no Inspector. Corrija o diálogo.");
                    return;
                }
            lines = newLines;
            lineIndex = 0;
            dialogueFinished = finished;
        }

        public void AdvanceDialogue()
        {
            if (!DialogueIsOpen) return;
            if (++lineIndex < lines.Length) return;
            Action finished = dialogueFinished;
            lines = null;
            dialogueFinished = null;
            finished?.Invoke();
        }

        public void AdvanceFromInput()
        {
            // Um Enter não deve avançar duas falas se também ativar o botão do IMGUI.
            if (lastAdvanceFrame == Time.frameCount) return;
            lastAdvanceFrame = Time.frameCount;
            AdvanceDialogue();
        }

        private void ReceiveScarab()
        {
            if (!Progress.FinishMummy()) return;
            CompletionIsOpen = true;
            if (scarabVisual != null) scarabVisual.SetActive(true);
            onScarabReceived.Invoke();
            onStageCompleted.Invoke();
        }

        public void CloseCompletion() { CompletionIsOpen = false; }
        public void Notify(string text) { message = text; messageUntil = Time.unscaledTime + 4f; }

        private void LoadDestination(string path)
        {
            if (IsLoading) return;
            if (string.IsNullOrWhiteSpace(path) || !Application.CanStreamedLevelBeLoaded(path))
            {
                Notify("A cena de destino não está disponível. Adicione as cenas jogáveis à lista do Build Profile.");
                return;
            }
            // A missão permanece em memória; a nova cena não perde a apresentação do guia.
            try
            {
                IsLoading = true;
                AsyncOperation operation = SceneManager.LoadSceneAsync(path);
                if (operation == null) { IsLoading = false; Notify("Não foi possível carregar a cena."); }
            }
            catch (Exception exception)
            {
                IsLoading = false;
                Debug.LogException(exception, this);
                Notify("Não foi possível carregar a cena. Confira o Console.");
            }
        }

        private void OnDisable()
        {
            // Interromper uma conversa nunca concede o item ou completa a missão.
            lines = null;
            dialogueFinished = null;
        }

        private string Objective
        {
            get
            {
                switch (Progress.Step)
                {
                    case EgyptQuestStep.MeetGuide: return "Converse com o guia turístico.";
                    case EgyptQuestStep.EnterPyramid: return "Vá até a entrada da pirâmide e aperte E.";
                    case EgyptQuestStep.ListenToMummy: return "Encontre a múmia e ouça suas histórias.";
                    default: return "Velho Escaravelho conquistado — fase concluída.";
                }
            }
        }

        private void OnGUI()
        {
            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2,
                (Screen.height - 720 * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);
            GUIStyle body = new GUIStyle(GUI.skin.label) { fontSize = 21, wordWrap = true };
            GUIStyle title = new GUIStyle(body) { fontStyle = FontStyle.Bold };

            GUI.Box(new Rect(20, 20, 900, 100), "");
            GUI.Label(new Rect(40, 28, 860, 35), "EGITO — AREIAS DO TEMPO", title);
            GUI.Label(new Rect(40, 65, 860, 50), Objective, body);
            GUI.Label(new Rect(30, 130, 1100, 30), "A/D: lados    W/S: fundo/frente    Espaço: pular    E/Enter: interagir", body);

            if (IsLoading)
            {
                GUI.Box(new Rect(340, 300, 600, 100), "");
                GUI.Label(new Rect(365, 330, 550, 40), "Carregando o destino...", title);
            }
            else if (DialogueIsOpen)
            {
                EgyptLine line = lines[lineIndex];
                GUI.Box(new Rect(130, 425, 1020, 275), "");
                GUI.Label(new Rect(155, 442, 970, 36), line.speaker, title);
                GUI.Label(new Rect(155, 490, 970, 130), line.text, body);
                GUI.Label(new Rect(155, 649, 650, 30), "E ou Enter para continuar", body);
                if (GUI.Button(new Rect(920, 644, 200, 40), "Continuar")) AdvanceFromInput();
            }
            else if (CompletionIsOpen)
            {
                GUI.Box(new Rect(250, 240, 780, 250), "");
                GUI.Label(new Rect(280, 265, 720, 40), "VELHO ESCARAVELHO CONQUISTADO", title);
                GUI.Label(new Rect(280, 315, 720, 110), "Amira aprendeu que ouvir, respeitar e confiar nas pessoas torna a viagem mais rica.", body);
                if (GUI.Button(new Rect(525, 435, 230, 35), "Continuar explorando")) CloseCompletion();
            }
            else
            {
                string status = Time.unscaledTime < messageUntil ? message : Hint;
                if (!string.IsNullOrEmpty(status))
                {
                    GUI.Box(new Rect(170, 610, 940, 85), "");
                    GUI.Label(new Rect(195, 626, 890, 65), status, body);
                }
            }
            GUI.color = oldColor;
            GUI.matrix = oldMatrix;
        }
    }
}
