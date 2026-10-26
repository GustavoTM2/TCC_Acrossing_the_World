using UnityEngine;

namespace CrossingTheWorld.TCCBrazil
{
    // Movimento simples feito nos ossos enquanto ainda não temos uma animação de embaixadinha.
    [DefaultExecutionOrder(100)]
    public sealed class BrazilKickPose : MonoBehaviour
    {
        public BrazilStage stage;
        public Animator animator;
        public Transform thigh, shin, visual;
        private Quaternion thighRest, shinRest;
        private bool posing;
        private float kickAge = 1;
        public void Kick() { kickAge = 0; }
        private void LateUpdate()
        {
            bool active = stage != null && (stage.State == BrazilState.Countdown || stage.State == BrazilState.Playing);
            if (active && !posing)
            {
                thighRest = thigh.localRotation; shinRest = shin.localRotation;
                animator.enabled = false; posing = true;
            }
            if (!active) { Restore(); return; }
            kickAge += Time.deltaTime;
            float amount = Mathf.Sin(Mathf.Clamp01(kickAge / .36f) * Mathf.PI);
            thigh.localRotation = thighRest; shin.localRotation = shinRest;
            thigh.Rotate(visual.right, -28 * amount, Space.World);
            shin.Rotate(visual.right, 18 * amount, Space.World);
        }
        private void Restore()
        {
            if (!posing) return;
            thigh.localRotation = thighRest; shin.localRotation = shinRest;
            animator.enabled = true; posing = false;
        }
        private void OnDisable() { Restore(); }
    }
}
