using UnityEngine;
using CrossingTheWorld.TCCEgypt;

namespace CrossingTheWorld.TCCBrazil
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class BrazilPlayer25D : MonoBehaviour
    {
        public Transform visual;
        public Animator animator;
        public Vector3 horizontalAxis = Vector3.right;
        public Vector3 depthAxis = Vector3.forward;
        public float visualYawOffset;
        [Min(0.1f)] public float speed = 4f;
        [Range(0.1f, 1f)] public float depthSpeedMultiplier = 0.65f;
        public float gravity = -22f;
        public float respawnBelowY = -15f;
        private CharacterController controller;
        private Vector3 spawn;
        private float verticalSpeed;

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

        private void Update()
        {
            TickMovement(new Vector2(EgyptInput.Horizontal, EgyptInput.Depth), Time.deltaTime);
        }

        // Usa as mesmas entradas de teclado do Egito; nesta etapa Lucas apenas caminha.
        public void TickMovement(Vector2 input, float deltaTime)
        {
            if (controller == null) Awake();
            if (deltaTime <= 0) return;
            input = Vector2.ClampMagnitude(input, 1);
            Vector3 velocity = (horizontalAxis * input.x + depthAxis * input.y * depthSpeedMultiplier) * speed;
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2f;
            verticalSpeed += gravity * deltaTime;
            controller.Move((velocity + Vector3.up * verticalSpeed) * deltaTime);
            if (visual != null && velocity.sqrMagnitude > 0.001f)
                visual.rotation = Quaternion.LookRotation(velocity, Vector3.up) * Quaternion.Euler(0, visualYawOffset, 0);
            if (animator != null) animator.SetFloat("Speed", velocity.magnitude / speed);
            if (transform.position.y < respawnBelowY) Respawn();
        }

        public void Respawn()
        {
            if (controller == null) Awake();
            controller.enabled = false;
            transform.position = spawn;
            verticalSpeed = 0;
            controller.enabled = true;
        }
    }
}
