using System;
using System.Collections;
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
        public EgyptPlayer25D player;
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
        public bool IsLoading => EgyptSceneTransition.IsBusy;
        public bool IsDeliveringScarab { get; private set; }
        public string CurrentSpeaker => DialogueIsOpen ? lines[lineIndex].speaker : "";
        public bool BlocksMovement => DialogueIsOpen || CompletionIsOpen || IsLoading || IsDeliveringScarab;
        public string Hint { get; set; }
        private EgyptLine[] lines;
        private int lineIndex;
        private Action dialogueFinished;
        private string message;
        private float messageUntil;
        private int lastAdvanceFrame = -1;
        private EgyptStageUI ui;
        private Vector3 scarabScale;
        private Quaternion scarabRotation;

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
            if (scarabVisual != null)
            {
                scarabScale = scarabVisual.transform.localScale;
                scarabRotation = scarabVisual.transform.localRotation;
                scarabVisual.SetActive(false);
            }
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
            if (Progress.Step != EgyptQuestStep.ListenToMummy || IsDeliveringScarab) return;
            // A validação no Editor confere a missão; a apresentação ocorre durante o Play.
            if (!Application.isPlaying || scarabVisual == null || player == null) { FinishReceipt(); return; }
            StartCoroutine(DeliverScarab());
        }

        private IEnumerator DeliverScarab()
        {
            IsDeliveringScarab = true;
            Hint = "";
            Transform item = scarabVisual.transform;
            Vector3 start = item.position;
            Vector3 destination = player.transform.position + new Vector3(0, 1.1f, 0.3f);
            scarabVisual.SetActive(true);
            float elapsed = 0;
            const float duration = 1.25f;
            while (elapsed < duration)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                item.position = Vector3.Lerp(start, destination, Mathf.SmoothStep(0, 1, t))
                    + Vector3.up * Mathf.Sin(t * Mathf.PI) * 0.45f;
                item.localRotation = scarabRotation * Quaternion.Euler(0, t * 270f, 0);
                item.localScale = scarabScale * Mathf.Lerp(1f, 0.1f, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.65f, 1f, t)));
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            scarabVisual.SetActive(false);
            item.localScale = scarabScale;
            item.localRotation = scarabRotation;
            IsDeliveringScarab = false;
            FinishReceipt();
        }

        private void FinishReceipt()
        {
            if (!Progress.FinishMummy()) return;
            CompletionIsOpen = true;
            if (scarabVisual != null) scarabVisual.SetActive(false);
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
            // O controlador persiste entre cenas para manter a tela preta durante a carga.
            EgyptSceneTransition.Load(path, this);
        }

        private void OnDisable()
        {
            // Interromper uma conversa ou a entrega nunca concede o item.
            StopAllCoroutines();
            IsDeliveringScarab = false;
            if (scarabVisual != null)
            {
                scarabVisual.SetActive(false);
                scarabVisual.transform.localScale = scarabScale;
                scarabVisual.transform.localRotation = scarabRotation;
            }
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
            if (IsLoading) return;
            if (ui == null) ui = new EgyptStageUI();
            string status = Time.unscaledTime < messageUntil ? message : Hint;
            ui.Draw(this, Objective, status, DialogueIsOpen ? lines[lineIndex] : null,
                lineIndex, DialogueIsOpen ? lines.Length : 0);
        }

        private void OnDestroy() { ui?.Dispose(); }
    }
}
