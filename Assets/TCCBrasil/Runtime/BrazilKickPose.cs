using UnityEngine;

namespace CrossingTheWorld.TCCBrazil
{
    // Pé e bola usam o mesmo tempo do desafio: preparo, contato e subida.
    [DefaultExecutionOrder(100)]
    public sealed class BrazilKickPose : MonoBehaviour
    {
        public BrazilStage stage;
        public Animator animator;
        public Transform thigh, shin, visual;
        public Transform foot;
        private Quaternion thighRest, shinRest;
        private bool posing, launchPending;
        private Vector3 launchPoint;
        private float release;
        private const float BallRadius = .13f;
        public Vector3 ContactPoint => foot != null
            ? foot.position + visual.forward * .10f + Vector3.up * BallRadius
            : visual.position + visual.forward * .45f + visual.right * .12f + Vector3.up * .28f;

        public void Kick() { launchPending = true; }
        private void LateUpdate() { UpdatePresentation(Time.deltaTime); }

        // Atualizo depois do Animator para posicionar a bola sobre o pé já movimentado.
        public void UpdatePresentation(float deltaTime)
        {
            if (stage == null || stage.ball == null || visual == null || thigh == null || shin == null || animator == null) return;
            bool active = stage.State == BrazilState.Countdown || stage.State == BrazilState.Playing;
            if (active && !posing)
            {
                animator.SetFloat("Speed", 0);
                animator.Play("Parado", 0, 0); animator.Update(0);
                thighRest = thigh.localRotation; shinRest = shin.localRotation;
                if (foot == null)
                    foreach (var bone in shin.GetComponentsInChildren<Transform>())
                        if (bone.name.ToLowerInvariant() == "pe.r" || bone.name.ToLowerInvariant() == "pé.r") { foot = bone; break; }
                animator.enabled = false; posing = true; release = 0;
                launchPoint = ContactPoint;
            }
            if (!active)
            {
                launchPending = false;
                if (posing)
                {
                    release += Mathf.Max(0, deltaTime);
                    float amount = 1 - Mathf.SmoothStep(0, 1, release / .18f);
                    thigh.localRotation = Quaternion.Slerp(thighRest, thigh.localRotation, amount);
                    shin.localRotation = Quaternion.Slerp(shinRest, shin.localRotation, amount);
                    if (release >= .18f) Restore();
                }
                Vector3 ground = stage.BallRestPosition;
                stage.ball.position = Vector3.Lerp(stage.ball.position, ground, 1 - Mathf.Exp(-12 * Mathf.Max(0, deltaTime)));
                return;
            }
            float elapsed = BrazilTimingChallenge.PreparationDuration - stage.Challenge.PreparationRemaining
                + stage.Challenge.Phase * BrazilTimingChallenge.Duration;
            float contactTime = BrazilTimingChallenge.PreparationDuration + stage.Challenge.WindowStart * BrazilTimingChallenge.Duration;
            float lift = stage.State == BrazilState.Countdown
                ? Mathf.SmoothStep(0, 1, 1 - stage.Countdown / .3f)
                : Mathf.Max(1 - Mathf.SmoothStep(0, 1, elapsed / .22f),
                    Mathf.SmoothStep(0, 1, (elapsed - contactTime + .18f) / .18f));
            thigh.localRotation = thighRest; shin.localRotation = shinRest;
            thigh.Rotate(visual.right, -28 * lift, Space.World);
            shin.Rotate(visual.right, 18 * lift, Space.World);
            if (stage.State == BrazilState.Countdown)
            {
                Vector3 ground = stage.BallRestPosition;
                stage.ball.position = Vector3.Lerp(ground, ContactPoint, lift);
                return;
            }
            if (launchPending) { launchPoint = ContactPoint; launchPending = false; }
            float progress = Mathf.Clamp01(elapsed / Mathf.Max(.01f, contactTime));
            // A bola volta ao pé na abertura da faixa verde e só sobe novamente após um acerto.
            stage.ball.position = Vector3.Lerp(launchPoint, ContactPoint, Mathf.SmoothStep(0, 1, progress))
                + Vector3.up * (4 * progress * (1 - progress) * 1.15f);
            stage.ball.Rotate(110 * deltaTime, 35 * deltaTime, 0, Space.World);
        }
        private void Restore()
        {
            if (!posing) return;
            thigh.localRotation = thighRest; shin.localRotation = shinRest;
            animator.enabled = true; posing = false;
        }
        private void OnDisable() { Restore(); launchPending = false; }
    }
}
