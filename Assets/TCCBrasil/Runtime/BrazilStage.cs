using UnityEngine;
using CrossingTheWorld.TCCEgypt;
using CrossingTheWorld.TCCMenu;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CrossingTheWorld.TCCBrazil
{
    public enum BrazilState { Exploring, Dialogue, Ready, Countdown, Playing, Failed, Reward }

    public sealed class BrazilStage : MonoBehaviour
    {
        public BrazilPlayer25D player;
        public Transform guide, resident, challengeAnchor, ball;
        public BrazilKickPose kickPose;
        public BrazilRewardDisplay rewardDisplay;
        public EgyptCamera25D followCamera;
        public BrazilState State { get; private set; }
        public bool GuideFinished { get; private set; }
        public bool HasPrize { get; private set; }
        public int AwardCount { get; private set; }
        public int Best { get; private set; }
        public float Countdown { get; private set; }
        public int LineIndex { get; private set; }
        public bool TalkingToGuide { get; private set; }
        public bool BlocksMovement => State != BrazilState.Exploring || EgyptSceneTransition.IsBusy;
        public BrazilTimingChallenge Challenge { get; } = new BrazilTimingChallenge();
        public int LineCount => dialogueLines == null ? 0 : dialogueLines.GetLength(0);
        public string Speaker => dialogueLines == null ? "" : dialogueLines[LineIndex, 0];
        public string DialogueText => dialogueLines == null ? "" : dialogueLines[LineIndex, 1];
        public string Objective => HasPrize ? "Berimbau conquistado — fase concluída." :
            State == BrazilState.Playing || State == BrazilState.Countdown ? "Complete 10 embaixadinhas. Aperte Espaço na faixa verde." :
            !GuideFinished ? "Converse com o guia da Horizon Travel." : "Siga pela praia e converse com o morador sobre o desafio.";
        // A bola em repouso acompanha a orientação da área de embaixadinhas.
        public Vector3 BallRestPosition => challengeAnchor.position + player.horizontalAxis * .55f - player.depthAxis * .12f + Vector3.up * .13f;
        private Vector3 normalOffset, normalLook;
        private float normalFov, failureAge;
        private bool cameraPrepared;
        private string[,] dialogueLines;

        // Falas fornecidas para o projeto Horizon Travel.
        private static readonly string[,] GuideLines = {
            { "Agente da Horizon Travel", "E aí, Lucas! Bem-vindo ao Rio! Sou da Horizon Travel e vou te guiar por aqui." },
            { "Lucas", "Valeu! Qual é a primeira parada?" },
            { "Agente da Horizon Travel", "Tá rolando um desafio de embaixadinhas ali na frente. Bora tentar?" },
            { "Lucas", "Bora! Quero ver quantas eu consigo fazer!" },
            { "Agente da Horizon Travel", "Então chega lá! O desafio é manter a bola no ar sem deixar cair. Manda ver!" }
        };
        private static readonly string[,] ResidentLines = {
            { "Morador brasileiro", "E aí, Lucas! Bora encarar umas embaixadinhas? Se vencer o desafio, esse berimbau antigo é seu!" },
            { "Lucas", "Um berimbau? Como ele veio parar aqui?" },
            { "Morador brasileiro", "Era do meu avô. Ele tocava nas rodas de capoeira do bairro. Quando o berimbau soava, a roda ganhava vida!" },
            { "Morador brasileiro", "Ele dizia que esse berimbau merecia conhecer o mundo. Então prometi passar ele pra alguém disposto a viver uma aventura." },
            { "Lucas", "Então vou caprichar! Como funciona o desafio?" },
            { "Morador brasileiro", "Faça o máximo de embaixadinhas sem deixar a bola cair e alcance a meta. Bora ver se você leva esse pedacinho da nossa história!" }
        };

        // Depois da primeira conversa, uso um lembrete ou uma despedida conforme o progresso.
        private static readonly string[,] GuideReminder = {
            { "Agente da Horizon Travel", "As embaixadinhas estão te esperando, Lucas! Vá até a roda e converse com o morador. Vou ficar por aqui torcendo por você!" }
        };
        private static readonly string[,] GuideFarewell = {
            { "Agente da Horizon Travel", "Parabéns pelo desafio, Lucas! Leve o berimbau e as boas lembranças do Rio com você. Até a próxima viagem!" }
        };
        private static readonly string[,] ResidentFarewell = {
            { "Morador brasileiro", "Parabéns pela vitória, Lucas! O berimbau do meu avô agora segue viagem com você. Obrigado pela companhia e até a próxima!" }
        };

        private void Start()
        {
            GuideFinished = TravelProgress.BrazilGuide || TravelProgress.BrazilPrize;
            HasPrize = TravelProgress.BrazilPrize;
            if (followCamera != null)
            {
                normalOffset = followCamera.offset; normalLook = followCamera.lookOffset;
                normalFov = followCamera.GetComponent<Camera>().fieldOfView;
                cameraPrepared = true;
            }
            UpdateBall();
        }

        private bool Near(Transform target) => target != null && Vector3.Distance(player.transform.position, target.position) < 2.8f;

        public string InteractionPrompt
        {
            get
            {
                if (State != BrazilState.Exploring || player == null) return "";
                if (Near(guide)) return "E — Conversar com o guia";
                if (Near(resident)) return GuideFinished
                    ? "E — Conversar com o morador" : "Converse com o guia primeiro";
                return "";
            }
        }

        private void Update()
        {
            if (EgyptSceneTransition.IsBusy) return;
            bool escape = false;
#if ENABLE_INPUT_SYSTEM
            escape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            escape = Input.GetKeyDown(KeyCode.Escape);
#endif
            if (escape && State != BrazilState.Exploring && State != BrazilState.Reward) { ContinueExploring(); return; }
            if (State == BrazilState.Exploring && EgyptInput.InteractPressed)
            {
                if (Near(guide)) OpenDialogue(true);
                else if (Near(resident) && GuideFinished) OpenDialogue(false);
            }
            else if (State == BrazilState.Dialogue && EgyptInput.InteractPressed) AdvanceDialogue();
            else if (State == BrazilState.Ready && EgyptInput.JumpPressed) StartChallenge();
            else if (State == BrazilState.Countdown || State == BrazilState.Playing)
                TickChallenge(Time.deltaTime, EgyptInput.JumpPressed);
            else if (State == BrazilState.Failed)
            {
                failureAge += Time.deltaTime;
                if (failureAge > .35f && EgyptInput.JumpPressed) StartChallenge();
            }
            UpdateBall();
        }

        public void OpenDialogue(bool isGuide)
        {
            if (State != BrazilState.Exploring || (!isGuide && !GuideFinished)) return;
            dialogueLines = isGuide
                ? (HasPrize ? GuideFarewell : GuideFinished ? GuideReminder : GuideLines)
                : (HasPrize ? ResidentFarewell : ResidentLines);
            TalkingToGuide = isGuide; LineIndex = 0; State = BrazilState.Dialogue;
        }
        public void AdvanceDialogue()
        {
            if (State != BrazilState.Dialogue) return;
            if (LineIndex + 1 < LineCount) { LineIndex++; return; }
            if (TalkingToGuide)
            {
                GuideFinished = true; State = BrazilState.Exploring;
                TravelProgress.SaveBrazil(GuideFinished, HasPrize);
            }
            // Se já ganhou, a conversa só termina; não abro o prêmio ou o desafio de novo.
            else State = HasPrize ? BrazilState.Exploring : BrazilState.Ready;
            dialogueLines = null;
        }

        public void StartChallenge()
        {
            if (State != BrazilState.Ready && State != BrazilState.Failed) return;
            Challenge.Begin(); Countdown = 3; State = BrazilState.Countdown;
            player.PlaceAt(challengeAnchor.position);
            player.visual.rotation = Quaternion.LookRotation(player.horizontalAxis) * Quaternion.Euler(0, player.visualYawOffset, 0);
            if (followCamera != null)
            {
                followCamera.offset = new Vector3(6, 2.7f, -6.42f);
                followCamera.lookOffset = Vector3.up * 1.1f + player.horizontalAxis * .45f;
                followCamera.GetComponent<Camera>().fieldOfView = 55;
            }
        }

        // A contagem prepara o jogador antes de começar a verificar os acertos.
        public void TickChallenge(float deltaTime, bool pressed)
        {
            if (State == BrazilState.Countdown)
            {
                Countdown = Mathf.Max(0, Countdown - Mathf.Max(0, deltaTime));
                if (Countdown <= 0) { State = BrazilState.Playing; if (kickPose != null) kickPose.Kick(); }
                return;
            }
            if (State != BrazilState.Playing) return;
            if (Challenge.Tick(deltaTime, pressed) && kickPose != null) kickPose.Kick();
            Best = Mathf.Max(Best, Challenge.Score);
            if (!Challenge.Finished) return;
            if (Challenge.Won)
            {
                if (!HasPrize) { HasPrize = true; AwardCount++; }
                TravelProgress.SaveBrazil(GuideFinished, HasPrize);
                State = BrazilState.Reward;
            }
            else { State = BrazilState.Failed; failureAge = 0; }
        }

        public void ContinueExploring()
        {
            dialogueLines = null;
            State = BrazilState.Exploring;
            if (cameraPrepared && followCamera != null)
            {
                followCamera.offset = normalOffset; followCamera.lookOffset = normalLook;
                followCamera.GetComponent<Camera>().fieldOfView = normalFov;
            }
            UpdateBall();
        }

        public void ReturnToMenu()
        {
            if (EgyptSceneTransition.IsBusy) return;
            TravelProgress.SaveBrazil(GuideFinished, HasPrize);
            EgyptSceneTransition.Load(TravelMenu.ScenePath, null, "Voltando ao menu...");
        }

        private void UpdateBall()
        {
            if (ball == null || challengeAnchor == null) return;
            // A apresentação acontece no LateUpdate, depois de posicionar os ossos.
            if (kickPose != null) return;
            ball.position = BallRestPosition;
        }
    }
}
