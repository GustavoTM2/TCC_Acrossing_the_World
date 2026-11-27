using UnityEngine;

namespace CrossingTheWorld.TCCEgypt
{
    // Os clipes percorrem 1,55 comprimentos de perna por ciclo de um segundo.
    internal sealed class GroundWalk
    {
        private readonly float stride;
        private float pace;
        private static readonly int Speed = Animator.StringToHash("Speed");

        public GroundWalk(Animator animator, float fallbackHeight)
        {
            stride = fallbackHeight * .55f;
            if (!animator) return;
            foreach (var bone in animator.GetComponentsInChildren<Transform>(true))
            {
                if (bone.name != "pe.l" && bone.name != "feet.l") continue;
                var knee = bone.parent;
                float leg = Vector3.Distance(bone.position, knee.position)
                          + Vector3.Distance(knee.position, knee.parent.position);
                if (leg > .001f)
                {
                    stride = leg * 1.55f;
                    var contact = animator.GetComponent<GroundFootContact>();
                    if (!contact) contact = animator.gameObject.AddComponent<GroundFootContact>();
                    contact.Bind(animator, bone);
                }
                break;
            }
        }

        public Vector3 Approach(Vector3 input, float speedLimit, Transform visual, float yawOffset, float dt)
        {
            if (input.sqrMagnitude < .0001f) { pace = 0; return Vector3.zero; }
            var direction = input.normalized;
            float alignment = 1f;
            if (visual)
            {
                var correction = Quaternion.Euler(0, yawOffset, 0);
                var target = Quaternion.LookRotation(direction) * correction;
                visual.rotation = Quaternion.RotateTowards(visual.rotation, target, 720f * dt);
                alignment = Mathf.Clamp01(Vector3.Dot(visual.rotation * Quaternion.Inverse(correction) * Vector3.forward, direction));
            }
            var limit = Mathf.Min(speedLimit, stride * 1.65f);
            pace = Mathf.MoveTowards(pace, input.magnitude * limit, limit * 8f * dt);
            return direction * (pace * alignment);
        }

        public void Animate(Animator animator, Vector3 velocity)
        {
            if (animator) animator.SetFloat(Speed, new Vector2(velocity.x, velocity.z).magnitude / Mathf.Max(.001f, stride));
        }
    }
}

