using UnityEngine;
using UnityEngine.Rendering;

namespace CrossingTheWorld.TCCEgypt
{
    // Aplica o plano somente durante o render da câmera do jogador. Os controles
    // continuam usando os eixos e a posição da câmera de exploração.
    public sealed class DialogueShot : MonoBehaviour
    {
        private Camera view;
        private Transform actor, hero, partner;
        private Vector3 aim, position, axis, side;
        private float portraitFov, frameHeight, blend, savedFov, savedOrthoSize;
        private int requested = -1, lastRender = -1;
        private Vector3 savedPosition;
        private Quaternion savedRotation;
        private bool applied;
        private readonly RaycastHit[] hits = new RaycastHit[32];

        private Renderer[] listenerMeshes;
        private readonly System.Collections.Generic.List<Renderer> hidden = new System.Collections.Generic.List<Renderer>();

        private Volume portraitVolume;
        private VolumeProfile portraitProfile;

        public void SoftenCounterLighting()
        {
            portraitVolume = gameObject.AddComponent<Volume>();
            portraitVolume.isGlobal = true;
            portraitVolume.priority = 100;
            portraitVolume.weight = 0;
            portraitProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            portraitVolume.sharedProfile = portraitProfile;
            var color = portraitProfile.Add<UnityEngine.Rendering.Universal.ColorAdjustments>();
            color.postExposure.Override(-1.7f);
            portraitProfile.Add<UnityEngine.Rendering.Universal.Bloom>().intensity.Override(.12f);
        }

        private void LateUpdate()
        {
            if (portraitVolume) portraitVolume.weight = blend;
        }

        private void OnDestroy()
        {
            if (portraitProfile) Destroy(portraitProfile);
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += BeginRender;
            RenderPipelineManager.endCameraRendering += EndRender;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= BeginRender;
            RenderPipelineManager.endCameraRendering -= EndRender;
            Restore();
        }

        public void Frame(Transform player, Transform npc, bool playerSpeaks, Transform playerVisual, float yawOffset, Vector3 npcForward = default, float elevation = .15f)
        {
            if (!player || !npc) return;
            if (!view) view = Camera.main;
            if (!view) return;
            bool opening = requested < Time.frameCount - 1 || npc != partner;
            requested = Time.frameCount;
            if (opening)
            {
                hero = player;
                partner = npc;
                axis = Vector3.ProjectOnPlane(npc.position - player.position, Vector3.up).normalized;
                if (axis.sqrMagnitude < .01f) axis = Vector3.forward;
                side = Vector3.Cross(Vector3.up, axis);
                if (Vector3.Dot(side, view.transform.position - player.position) < 0) side = -side;
                if (playerVisual)
                    playerVisual.rotation = Quaternion.LookRotation(axis) * Quaternion.Euler(0, yawOffset, 0);
            }
            var next = playerSpeaks ? player : npc;
            if (!opening && next == actor) return;
            actor = next;
            listenerMeshes = (playerSpeaks ? npc : player).GetComponentsInChildren<Renderer>(true);
            var body = BoundsOf(actor);
            float height = Mathf.Max(.3f, body.size.y);
            Vector3 towardListener = playerSpeaks ? axis : -axis;
            Vector3 focus = body.center + Vector3.up * (.18f * height);
            Vector3 front = playerSpeaks ? axis : Vector3.ProjectOnPlane(
                npcForward.sqrMagnitude > .01f ? npcForward : npc.forward, Vector3.up).normalized;
            if (front.sqrMagnitude < .01f) front = towardListener;
            var shoulder = Vector3.Cross(Vector3.up, front);
            if (Vector3.Dot(shoulder, side) < 0) shoulder = -shoulder;
            Vector3 direction = front + shoulder * .32f;
            direction = (direction + Vector3.up * elevation).normalized;
            float distance = height * Mathf.Max(1.40f, 1.20f / view.aspect);
            float fullDistance = distance;
            int count = Physics.SphereCastNonAlloc(focus, height * .055f, direction, hits, distance, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var hit = hits[i];
                if (hit.transform.IsChildOf(hero) || hit.transform.IsChildOf(partner)) continue;
                distance = Mathf.Min(distance, Mathf.Max(height * .35f, hit.distance - .07f * height));
            }
            position = focus + direction * distance;
            aim = focus + shoulder * (.035f * height);
            frameHeight = .51f * height;
            // Uma parede atrás da câmera pode encurtar a distância. Abre a lente
            // para conservar o espaço acima da cabeça mesmo no enquadramento curto.
            portraitFov = Mathf.Min(79f, 2f * Mathf.Atan(Mathf.Tan(19.5f * Mathf.Deg2Rad) * fullDistance / distance) * Mathf.Rad2Deg);
        }

        private static Bounds BoundsOf(Transform subject)
        {
            Bounds box = default;
            bool found = false;
            foreach (var mesh in subject.GetComponentsInChildren<Renderer>())
            {
                if (!mesh.enabled || !(mesh is MeshRenderer || mesh is SkinnedMeshRenderer)) continue;
                if (found) box.Encapsulate(mesh.bounds);
                else { box = mesh.bounds; found = true; }
            }
            return found ? box : new Bounds(subject.position + Vector3.up * .85f, new Vector3(.5f, 1.7f, .4f));
        }

        private void BeginRender(ScriptableRenderContext context, Camera cam)
        {
            if (cam != view || applied) return;
            if (lastRender != Time.frameCount)
            {
                blend = Mathf.MoveTowards(blend, requested == Time.frameCount ? 1f : 0f, Time.unscaledDeltaTime * 3.5f);
                lastRender = Time.frameCount;
            }
            if (blend <= 0) return;
            float t = Mathf.SmoothStep(0, 1, blend);
            savedPosition = cam.transform.position;
            savedRotation = cam.transform.rotation;
            savedFov = cam.fieldOfView;
            savedOrthoSize = cam.orthographicSize;
            if (requested == Time.frameCount)
            {
                // Começa já do lado do rosto. Uma pequena aproximação suaviza o corte.
                var close = position + (position - aim) * (.09f * (1f - t));
                cam.transform.SetPositionAndRotation(close, Quaternion.LookRotation(aim - close));
                if (cam.orthographic) cam.orthographicSize = frameHeight * (1f + .09f * (1f - t));
                else cam.fieldOfView = portraitFov;
            }
            else
            {
                cam.transform.SetPositionAndRotation(Vector3.Lerp(savedPosition, position, t),
                    Quaternion.Slerp(savedRotation, Quaternion.LookRotation(aim - position), t));
                if (cam.orthographic) cam.orthographicSize = Mathf.Lerp(savedOrthoSize, frameHeight, t);
                else cam.fieldOfView = Mathf.Lerp(savedFov, portraitFov, t);
            }
            foreach (var mesh in listenerMeshes)
            {
                if (!mesh || mesh.forceRenderingOff) continue;
                mesh.forceRenderingOff = true;
                hidden.Add(mesh);
            }
            applied = true;
        }

        private void EndRender(ScriptableRenderContext context, Camera cam)
        {
            if (cam == view) Restore();
        }

        private void Restore()
        {
            if (applied && view)
            {
                view.transform.SetPositionAndRotation(savedPosition, savedRotation);
                if (view.orthographic) view.orthographicSize = savedOrthoSize;
                else view.fieldOfView = savedFov;
            }
            foreach (var mesh in hidden)
                if (mesh) mesh.forceRenderingOff = false;
            hidden.Clear();
            applied = false;
        }
    }
}
