using UnityEngine;
using CrossingTheWorld.TCCEgypt;

namespace CrossingTheWorld.TCCJapan
{
    // Enquadramento do templo: Kenji em destaque, com o monge e a arquitetura
    // visíveis ao fundo. A câmera não atravessa as laterais nem a porta.
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(450)]
    public sealed class JapanInteriorCamera25D : MonoBehaviour
    {
        [Header("Seguir Kenji")]
        public Transform target;
        public Vector3 offset = new Vector3(.45f, 3.8f, 4.8f);
        public Vector3 lookOffset = new Vector3(0f, 1.2f, -.8f);
        [Range(40f, 75f)] public float fieldOfView = 58f;
        [Range(.08f, .7f)] public float smoothTime = .19f;
        [Range(2f, 25f)] public float turnSharpness = 9f;

        [Header("Limites internos do salão")]
        public Vector2 horizontalBounds = new Vector2(-4.3f, 4.3f);
        public Vector2 depthBounds = new Vector2(-4.8f, 8.15f);
        [Range(0f, 1.5f)] public float wallSafety = .32f;
        public bool avoidWalls = true;
        [Range(.05f, .5f)] public float collisionRadius = .16f;

        private Camera actualCamera;
        private Vector3 velocity;
        private bool configured;

        public void Configure(Transform kenji)
        {
            target = kenji;
            actualCamera = GetComponent<Camera>();
            var old = GetComponent<EgyptCamera25D>();
            if (old != null) old.enabled = false;
            if (target == null || actualCamera == null)
            {
                Debug.LogWarning("Japão interno: câmera não encontrou Kenji ou Camera.");
                return;
            }

            actualCamera.orthographic = false;
            actualCamera.fieldOfView = fieldOfView;
            actualCamera.nearClipPlane = .09f;
            actualCamera.farClipPlane = 150f;
            velocity = Vector3.zero;
            configured = true;
            transform.position = CalculatePosition();
            AimAtPlayer(true);
        }

        private Vector3 CalculatePosition()
        {
            var candidate = target.position + offset;
            float minX = Mathf.Min(horizontalBounds.x, horizontalBounds.y) + wallSafety;
            float maxX = Mathf.Max(horizontalBounds.x, horizontalBounds.y) - wallSafety;
            float minZ = Mathf.Min(depthBounds.x, depthBounds.y) + wallSafety;
            float maxZ = Mathf.Max(depthBounds.x, depthBounds.y) - wallSafety;
            candidate.x = Mathf.Clamp(candidate.x, minX, maxX);
            candidate.z = Mathf.Clamp(candidate.z, minZ, maxZ);

            if (!avoidWalls) return candidate;

            // Impede que o ponto de vista atravesse paredes e pilares do templo,
            // sem deixar chão, teto e o próprio Kenji obstruírem a lente.
            Vector3 focus = target.position + Vector3.up * 1.2f;
            Vector3 towardCamera = candidate - focus;
            float length = towardCamera.magnitude;
            if (length <= 1.4f) return candidate;

            float safeDistance = length;
            foreach (var hit in Physics.SphereCastAll(focus, collisionRadius,
                towardCamera / length, length, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == null || hit.collider.transform.IsChildOf(target)) continue;
                if (hit.collider.transform.IsChildOf(transform)) continue;
                if (hit.normal.y > .66f || hit.normal.y < -.70f) continue;
                if (hit.distance < .35f) continue;
                safeDistance = Mathf.Min(safeDistance, hit.distance - .25f);
            }
            if (safeDistance < length)
            {
                candidate = focus + towardCamera.normalized * Mathf.Max(1.3f, safeDistance);
                candidate.x = Mathf.Clamp(candidate.x, minX, maxX);
                candidate.z = Mathf.Clamp(candidate.z, minZ, maxZ);
            }
            return candidate;
        }

        private void AimAtPlayer(bool instant)
        {
            Vector3 focus = target.position + lookOffset;
            Vector3 direction = focus - transform.position;
            if (direction.sqrMagnitude < .01f) return;
            Quaternion desired = Quaternion.LookRotation(direction.normalized, Vector3.up);
            if (instant) transform.rotation = desired;
            else transform.rotation = Quaternion.Slerp(transform.rotation, desired,
                1f - Mathf.Exp(-turnSharpness * Time.deltaTime));
        }

        private void LateUpdate()
        {
            if (!configured || target == null || Time.deltaTime <= 0f) return;
            actualCamera.fieldOfView = fieldOfView;
            Vector3 destination = CalculatePosition();
            transform.position = Vector3.SmoothDamp(transform.position, destination,
                ref velocity, smoothTime, Mathf.Infinity, Time.deltaTime);
            AimAtPlayer(false);
        }
    }
}
