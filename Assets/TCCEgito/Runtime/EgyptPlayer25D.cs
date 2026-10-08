using UnityEngine;

namespace CrossingTheWorld.TCCEgypt
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class EgyptPlayer25D : MonoBehaviour
    {
        public EgyptStage stage;
        public Transform visual;
        public Animator animator;
        [Tooltip("Correção de orientação do modelo, em graus, se Amira olhar para o lado oposto ao movimento.")]
        public float visualYawOffset;
        [Tooltip("Direção no mundo correspondente a D/seta direita.")]
        public Vector3 movementAxis = Vector3.left;
        [Tooltip("Direção correspondente a W/seta para cima: afasta-se da câmera.")]
        public Vector3 depthAxis = Vector3.back;
        [Range(0.1f, 1f)] public float depthSpeedMultiplier = 0.65f;
        [Min(0.1f)] public float speed = 4f;
        [Min(0f)] public float jumpHeight = 1.2f;
        public float gravity = -22f;
        public float respawnBelowY = -15f;

        private CharacterController controller;
        private Vector3 spawn;
        private float verticalSpeed;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            spawn = transform.position;
            movementAxis.y = 0;
            movementAxis = movementAxis.sqrMagnitude > 0.001f ? movementAxis.normalized : Vector3.left;
            depthAxis.y = 0;
            depthAxis = Vector3.ProjectOnPlane(depthAxis, movementAxis);
            depthAxis = depthAxis.sqrMagnitude > 0.001f ? depthAxis.normalized
                : Vector3.Cross(movementAxis, Vector3.up).normalized;
            if (animator != null) animator.applyRootMotion = false;
        }

        private void Update()
        {
            TickMovement(new Vector2(EgyptInput.Horizontal, EgyptInput.Depth), EgyptInput.JumpPressed, Time.deltaTime);
        }

        // Também permite conduzir a personagem em sequências e nos testes físicos do Editor.
        public void TickMovement(Vector2 input, bool jump, float deltaTime)
        {
            if (controller == null) Awake();
            if (deltaTime <= 0) return;
            if (stage != null && stage.BlocksMovement)
            {
                if (animator != null) animator.SetFloat("Speed", 0);
                return;
            }
            input = Vector2.ClampMagnitude(input, 1f);
            Vector3 planarVelocity = (movementAxis * input.x + depthAxis * input.y * depthSpeedMultiplier) * speed;
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2f;
            if (controller.isGrounded && jump && jumpHeight > 0)
                verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            verticalSpeed += gravity * deltaTime;
            controller.Move((planarVelocity + Vector3.up * verticalSpeed) * deltaTime);

            if (visual != null && planarVelocity.sqrMagnitude > 0.001f)
                visual.rotation = Quaternion.LookRotation(planarVelocity, Vector3.up)
                    * Quaternion.Euler(0, visualYawOffset, 0);
            if (animator != null) animator.SetFloat("Speed", planarVelocity.magnitude / Mathf.Max(0.01f, speed));
            if (transform.position.y < respawnBelowY) Respawn();
        }

        public void Respawn()
        {
            controller.enabled = false;
            transform.position = spawn;
            verticalSpeed = 0;
            controller.enabled = true;
        }
    }
}
