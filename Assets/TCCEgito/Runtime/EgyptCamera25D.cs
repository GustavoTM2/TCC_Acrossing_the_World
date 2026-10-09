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
        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 framingTarget = target.position
                - depthAxis * Vector3.Dot(target.position - initialTarget, depthAxis) * (1f - depthFollow);
            if (fixedHorizontalFraming) framingTarget.x = horizontalFramePosition;
            float amount = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, framingTarget + offset, amount);
            transform.LookAt(framingTarget + lookOffset);
        }
    }
}
