using UnityEngine;

namespace CrossingTheWorld.TCCEgypt
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class EgyptPlayer25D : MonoBehaviour
    {
        public EgyptStage stage;
        public Transform visual;
        public Animator animator;
        [Tooltip("Ajuste da altura do modelo em metros. Valores negativos aproximam os pés da areia.")]
        [Range(-0.2f, 0.2f)] public float visualHeightOffset;
        [Tooltip("Correção de orientação do modelo, em graus, se Amira olhar para o lado oposto ao movimento.")]
        public float visualYawOffset;
        [Tooltip("Direção no mundo correspondente a D/seta direita.")]
        public Vector3 movementAxis = Vector3.left;
        [Tooltip("Direção da profundidade no mundo. No Egito, S avança para o fundo e W volta; setas mantêm o sentido normal.")]
        public Vector3 depthAxis = Vector3.back;
        [Range(0.1f, 1f)] public float depthSpeedMultiplier = 0.65f;
        [Min(0.1f)] public float speed = 4f;
        [Min(0f)] public float jumpHeight = 1.2f;
        public float gravity = -22f;
        public float respawnBelowY = -15f;

        private CharacterController controller;
        private GroundWalk walk;
        private Vector3 spawn;
        private float verticalSpeed;

        // Preparo o CharacterController e guardo a posição inicial da Amira.
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            spawn = transform.position;
            ApplyVisualHeightOffset();
            movementAxis.y = 0;
            movementAxis = movementAxis.sqrMagnitude > 0.001f ? movementAxis.normalized : Vector3.left;
            depthAxis.y = 0;
            depthAxis = Vector3.ProjectOnPlane(depthAxis, movementAxis);
            depthAxis = depthAxis.sqrMagnitude > 0.001f ? depthAxis.normalized
                : Vector3.Cross(movementAxis, Vector3.up).normalized;
            if (animator != null) animator.applyRootMotion = false;
        }

        // Abaixo só o modelo. A cápsula continua na mesma altura para manter as colisões e o pulo.
        public void ApplyVisualHeightOffset()
        {
            if (visual == null || visual == transform) return;
            Transform heightRoot = transform.Find("Altura_Visual_Amira");
            if (heightRoot == null)
            {
                if (Mathf.Approximately(visualHeightOffset, 0)) return;
                heightRoot = new GameObject("Altura_Visual_Amira").transform;
                heightRoot.SetParent(transform, false);
            }
            // O ajuste fica fora do Animator, para a caminhada não desfazer a altura escolhida.
            if (visual.parent != heightRoot) visual.SetParent(heightRoot, false);
            heightRoot.localPosition = Vector3.up * visualHeightOffset;
        }

        // Leio o teclado a cada quadro e envio os comandos para a movimentação.
        private void Update()
        {
            TickMovement(new Vector2(EgyptInput.Horizontal, EgyptInput.EgyptDepth), EgyptInput.JumpPressed, Time.deltaTime);
        }

        // Também permite conduzir a personagem em sequências e nos testes físicos do Editor.
        public void TickMovement(Vector2 input, bool jump, float deltaTime)
        {
            if (controller == null) Awake();
            if (deltaTime <= 0) return;
            // Durante diálogos, transições ou entrega do item, Amira fica parada.
            if (stage != null && stage.BlocksMovement)
            {
                // Continua sujeito à gravidade; só a movimentação horizontal e o pulo são bloqueados.
                input = Vector2.zero;
                jump = false;
            }
            // Evito que andar na diagonal aumente a velocidade.
            input = Vector2.ClampMagnitude(input, 1f);
            walk ??= new GroundWalk(animator, controller.height);
            Vector3 planarVelocity = walk.Approach(EgyptInput.GroundDirection(input, movementAxis, depthAxis, depthSpeedMultiplier),
                speed, visual, visualYawOffset, deltaTime);
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2f;
            // Amira só pode iniciar um pulo quando está no chão.
            if (controller.isGrounded && jump && jumpHeight > 0)
                verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            // Aplico a gravidade e depois movimento Amira respeitando as colisões.
            verticalSpeed += gravity * deltaTime;
            var previousPosition = transform.position;
            controller.Move((planarVelocity + Vector3.up * verticalSpeed) * deltaTime);

            walk.Animate(animator, input.sqrMagnitude > 0 ? (transform.position - previousPosition) / deltaTime : Vector3.zero);
            if (transform.position.y < respawnBelowY) Respawn();
        }

        // Se cair fora da fase, Amira volta para a posição inicial.
        public void Respawn()
        {
            controller.enabled = false;
            transform.position = spawn;
            verticalSpeed = 0;
            controller.enabled = true;
        }
    }
}
