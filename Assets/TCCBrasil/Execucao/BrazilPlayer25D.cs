using UnityEngine;
using CrossingTheWorld.TCCEgypt;

namespace CrossingTheWorld.TCCBrazil
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class BrazilPlayer25D : MonoBehaviour
    {
        // Referências do modelo e da animação, preenchidas no Inspector
        public Transform visual;
        public Animator animator;
        public BrazilStage stage;
        public Vector3 horizontalAxis = Vector3.right;
        public Vector3 depthAxis = Vector3.forward;
        public float visualYawOffset;
        [Min(0.1f)] public float speed = 4f;
        [Range(0.1f, 1f)] public float depthSpeedMultiplier = 0.65f;
        public float gravity = -22f;
        public float respawnBelowY = -15f;
        private CharacterController controller;
        private GroundWalk walk;
        private Vector3 spawn;
        private float verticalSpeed;

        // No início, preparo o controle e guardo a posição para voltar caso Lucas caia
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            spawn = transform.position;
            horizontalAxis.y = 0;
            horizontalAxis = horizontalAxis.sqrMagnitude > 0.001f ? horizontalAxis.normalized : Vector3.right;
            depthAxis.y = 0;
            depthAxis = Vector3.ProjectOnPlane(depthAxis, horizontalAxis);
            depthAxis = depthAxis.sqrMagnitude > 0.001f ? depthAxis.normalized : Vector3.Cross(horizontalAxis, Vector3.up);
            if (animator != null) animator.applyRootMotion = false;
        }

        // A cada quadro, leio as teclas. O deltaTime ajusta o movimento ao tempo entre os quadros
        private void Update()
        {
            // Mesma inversão para o Brasil inteiro e para Kenji (ambas cenas).
            TickMovement(new Vector2(EgyptInput.Horizontal, EgyptInput.BrazilDepth), Time.deltaTime);
        }

        // Reaproveito as entradas do Egito e respeito o bloqueio das conversas e do desafio.
        public void TickMovement(Vector2 input, float deltaTime)
        {
            if (controller == null) Awake();
            if (deltaTime <= 0) return;
            // Durante a conversa e o desafio, mantenho Lucas no lugar, mas continuo aplicando a gravidade.
            if (stage != null && stage.BlocksMovement) input = Vector2.zero;
            // Limito a entrada para Lucas não andar mais rápido na diagonal
            input = Vector2.ClampMagnitude(input, 1);
            // Calculo o movimento para os lados e para frente/fundo. Na profundidade, a velocidade é menor
            walk ??= new GroundWalk(animator, controller.height);
            Vector3 velocity = walk.Approach(EgyptInput.GroundDirection(input, horizontalAxis, depthAxis, depthSpeedMultiplier),
                speed, visual, visualYawOffset, deltaTime);
            // Verifico o chão e aplico a gravidade para manter Lucas apoiado no cenário
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2f;
            verticalSpeed += gravity * deltaTime;
            // O CharacterController movimenta Lucas e considera as colisões com o cenário
            var previousPosition = transform.position;
            controller.Move((velocity + Vector3.up * verticalSpeed) * deltaTime);
            walk.Animate(animator, input.sqrMagnitude > 0 ? (transform.position - previousPosition) / deltaTime : Vector3.zero);
            if (transform.position.y < respawnBelowY) Respawn();
        }

        // Se Lucas cair abaixo do limite, devolvo ele à posição inicial
        public void Respawn()
        {
            PlaceAt(spawn);
        }

        public void PlaceAt(Vector3 position)
        {
            if (controller == null) Awake();
            controller.enabled = false;
            transform.position = position;
            verticalSpeed = 0;
            controller.enabled = true;
        }
    }
}
