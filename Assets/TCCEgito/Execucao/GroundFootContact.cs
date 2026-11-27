using UnityEngine;

namespace CrossingTheWorld.TCCEgypt
{
    // Os clipes são in-place. Em rampas e curvas, a cápsula pode girar ou descer
    // durante o apoio. Corrige as duas articulações sem deslocar o jogador.
    [DefaultExecutionOrder(200)]
    internal sealed class GroundFootContact : MonoBehaviour
    {
        private Animator animator;
        private CharacterController controller;
        private readonly Transform[] feet = new Transform[2];
        private readonly Vector3[] anchors = new Vector3[2];
        private readonly int[] cycles = { int.MinValue, int.MinValue };
        private static readonly int Speed = Animator.StringToHash("Speed");

        public void Bind(Animator rig, Transform left)
        {
            animator = rig;
            controller = rig.GetComponentInParent<CharacterController>();
            feet[0] = left;
            foreach (var bone in rig.GetComponentsInChildren<Transform>(true))
                if (bone.name == "pe.r" || bone.name == "feet.r") { feet[1] = bone; break; }
        }

        private void LateUpdate()
        {
            if (!animator || !animator.isActiveAndEnabled || !controller || !controller.isGrounded ||
                !feet[0] || !feet[1] || animator.GetFloat(Speed) < .05f || animator.IsInTransition(0))
            {
                Release();
                return;
            }
            var state = animator.GetCurrentAnimatorStateInfo(0);
            if (!state.IsName("Caminhada") && !state.IsName("Caminhando")) { Release(); return; }
            for (int i = 0; i < feet.Length; i++)
            {
                float phase = state.normalizedTime + i * .5f;
                int cycle = Mathf.FloorToInt(phase);
                phase -= cycle;
                var foot = feet[i];
                if (phase > .60f) continue;
                if (cycles[i] != cycle || (foot.position - anchors[i]).sqrMagnitude > controller.bounds.size.y * controller.bounds.size.y)
                {
                    anchors[i] = foot.position;
                    cycles[i] = cycle;
                }
                float weight = 1f - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.47f, .60f, phase));
                Stabilize(foot, Vector3.Lerp(foot.position, anchors[i], weight));
            }
        }

        private void OnDisable() { Release(); }
        private void Release() { cycles[0] = cycles[1] = int.MinValue; }

        private static void Stabilize(Transform foot, Vector3 target)
        {
            var knee = foot.parent;
            var hip = knee.parent;
            var a = knee.position - hip.position;
            var b = foot.position - knee.position;
            float upper = a.magnitude, lower = b.magnitude;
            if ((target - foot.position).sqrMagnitude < .0000001f || upper < .0001f || lower < .0001f) return;
            var delta = target - hip.position;
            var dir = delta.normalized;
            var pole = Vector3.ProjectOnPlane(a, dir);
            if (pole.sqrMagnitude < .0000001f) return;
            float reach = Mathf.Clamp(delta.magnitude, Mathf.Abs(upper - lower) + .00001f, (upper + lower) * .9999f);
            float along = (upper * upper - lower * lower + reach * reach) / (2 * reach);
            var bend = dir * along + pole.normalized * Mathf.Sqrt(Mathf.Max(0, upper * upper - along * along));
            var sole = foot.rotation;
            hip.rotation = Quaternion.FromToRotation(a, bend) * hip.rotation;
            knee.rotation = Quaternion.FromToRotation(foot.position - knee.position,
                hip.position + dir * reach - knee.position) * knee.rotation;
            foot.rotation = sole;
        }
    }
}

