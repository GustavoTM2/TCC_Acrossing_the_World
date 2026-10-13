using UnityEngine;

namespace CrossingTheWorld.TCCEgypt
{
    public sealed class EgyptCamera25D : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 3f, 9f);
        public Vector3 lookOffset = new Vector3(0, 1f, 0);
        [Min(0.01f)] public float followSharpness = 8f;
        [Range(0f, 1f)] public float depthFollow = 0.2f;
        public Vector3 depthAxis = Vector3.back;
        public bool fixedHorizontalFraming;
        public float horizontalFramePosition;
        private Vector3 initialTarget;

        // Posiciono a câmera em relação ao personagem quando a fase começa.
        private void Start()
        {
            if (target == null) return;
            initialTarget = target.position;
            depthAxis.y = 0;
            depthAxis = depthAxis.sqrMagnitude > 0.001f ? depthAxis.normalized : Vector3.back;
            Vector3 frame = target.position;
            if (fixedHorizontalFraming) frame.x = horizontalFramePosition;
            transform.position = frame + offset;
            transform.LookAt(frame + lookOffset);
        }
        // Atualizo a câmera depois do movimento do personagem para acompanhar a posição final do quadro.
        private void LateUpdate()
        {
            if (target == null) return;
            // Acompanho só parte do movimento em profundidade para manter a visão 2,5D.
            Vector3 framingTarget = target.position
                - depthAxis * Vector3.Dot(target.position - initialTarget, depthAxis) * (1f - depthFollow);
            if (fixedHorizontalFraming) framingTarget.x = horizontalFramePosition;
            float amount = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            // O Lerp faz a câmera seguir com suavidade, sem mudar de posição de uma vez.
            transform.position = Vector3.Lerp(transform.position, framingTarget + offset, amount);
            // Mantenho a câmera olhando para o personagem com um ajuste de altura.
            transform.LookAt(framingTarget + lookOffset);
        }
    }
}
