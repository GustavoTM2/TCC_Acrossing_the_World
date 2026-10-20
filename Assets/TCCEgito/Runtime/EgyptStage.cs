using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CrossingTheWorld.TCCEgypt
{
    [Serializable]
    // Cada fala guarda o nome do personagem e o texto da conversa.
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

        [Header("Falas do roteiro Horizon Travel")]
        public EgyptLine[] guideLines = EgyptDialogueContent.CreateGuide();
        public EgyptLine[] mummyLines = EgyptDialogueContent.CreateMummy();

        public static EgyptQuestProgress Progress { get; private set; } = new EgyptQuestProgress();
        public bool DialogueIsOpen => lines != null;
        public bool CompletionIsOpen { get; private set; }
        public bool IsLoading => EgyptSceneTransition.IsBusy;
        public bool IsDeliveringScarab { get; private set; }
        public string CurrentSpeaker => DialogueIsOpen ? lines[lineIndex].speaker : "";
        // Estas situações pausam a movimentação para o jogador acompanhar o que está acontecendo.
        public bool BlocksMovement => DialogueIsOpen || CompletionIsOpen || IsLoading || IsDeliveringScarab;
        public string Hint { get; set; }
        private EgyptLine[] lines;
        private int lineIndex;
        private Action dialogueFinished;
        private string message;
        private float messageUntil;
        private int lastAdvanceFrame = -1;
        private EgyptStageUI ui;
        private EgyptScarabDisplay scarabDisplay;
        public Texture ScarabPreview => scarabDisplay != null ? scarabDisplay.PreviewTexture : null;
        private Vector3 scarabScale;
        private Quaternion scarabRotation;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() { Progress = new EgyptQuestProgress(); }

        private void Awake()
        {
            scarabDisplay = GetComponent<EgyptScarabDisplay>();
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

        // Escolho a ação do guia, da entrada, da múmia ou da saída conforme o progresso da fase.
        public void Interact(EgyptPointKind kind)
        {
            if (BlocksMovement) return;
            switch (kind)
            {
                case EgyptPointKind.Guide:
                    if (Progress.Step == EgyptQuestStep.MeetGuide)
                        Open(guideLines, () => { if (Progress.FinishGuide()) onGuideFinished.Invoke(); });
                    else Open(new[] { new EgyptLine(EgyptDialogueContent.GuideSpeaker, Progress.HasScarab
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
                        Open(new[] { new EgyptLine(EgyptDialogueContent.MummySpeaker, "Leve consigo o respeito pelas histórias deste lugar.") }, null);
                    else Notify("Converse com o guia e entre na pirâmide para continuar.");
                    break;
                case EgyptPointKind.Exit:
                    LoadDestination(exteriorScene);
                    break;
            }
        }

        // Abro a conversa na primeira fala e guardo a ação que deve acontecer quando ela terminar.
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

        // Passo para a próxima fala. Ao terminar, fecho o diálogo e executo a ação guardada.
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

        // Animo o escaravelho até Amira ao longo de vários quadros, sem travar o jogo.
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

        // Registro a conquista do escaravelho e abro o aviso de conclusão.
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

        // O objetivo mostrado na tela muda de acordo com o progresso do jogador.
        private string Objective
        {
            get
            {
                switch (Progress.Step)
                {
                    case EgyptQuestStep.MeetGuide: return "Converse com o guia turístico.";
                    case EgyptQuestStep.EnterPyramid: return "Vá até a entrada da pirâmide e aperte E.";
                    case EgyptQuestStep.ListenToMummy: return "Suba a escadaria e converse com Rami, a múmia junto ao portal.";
                    default: return "Velho Escaravelho conquistado — fase concluída.";
                }
            }
        }

        // Envio os textos e o estado da fase para a classe que desenha a interface.
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
