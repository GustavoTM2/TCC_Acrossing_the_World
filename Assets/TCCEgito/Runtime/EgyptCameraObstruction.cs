using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CrossingTheWorld.TCCEgypt
{
    // Torna translúcido somente o módulo que estiver entre a câmera e Amira.
    // Os colliders continuam ativos e os materiais originais nunca são alterados.
    [DefaultExecutionOrder(100)]
    public sealed class EgyptCameraObstruction : MonoBehaviour
    {
        public Transform environment;
        public Transform target;
        [Range(0, 1)] public float obscuredAlpha = 0.12f;
        [Min(0.1f)] public float fadeSpeed = 6f;
        private sealed class Surface
        {
            public Renderer renderer;
            public Material[] originals;
            public Material[] transparent;
            public Color[] colors;
            public float alpha = 1;
        }
        private readonly Dictionary<Renderer, Surface> surfaces = new Dictionary<Renderer, Surface>();
        private readonly HashSet<Renderer> blocked = new HashSet<Renderer>();

        private void LateUpdate() { UpdateOcclusion(Time.unscaledDeltaTime); }

        public void UpdateOcclusion(float deltaTime)
        {
            if (environment == null || target == null) return;
            blocked.Clear();
            Vector3 direction = target.position + Vector3.up - transform.position;
            float distance = direction.magnitude;
            if (distance > 0.5f)
                foreach (RaycastHit hit in Physics.SphereCastAll(transform.position, 0.28f,
                    direction.normalized, distance - 0.35f, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (!hit.collider.transform.IsChildOf(environment)) continue;
                    Renderer renderer = hit.collider.GetComponent<Renderer>();
                    if (renderer == null || !renderer.enabled) continue;
                    blocked.Add(renderer);
                    if (!surfaces.ContainsKey(renderer)) surfaces.Add(renderer, CreateSurface(renderer));
                }
            foreach (Surface surface in surfaces.Values)
            {
                if (surface.renderer == null) continue;
                surface.alpha = Mathf.MoveTowards(surface.alpha, blocked.Contains(surface.renderer) ? obscuredAlpha : 1f,
                    deltaTime * fadeSpeed);
                if (surface.alpha >= 0.999f) { surface.renderer.sharedMaterials = surface.originals; continue; }
                for (int i = 0; i < surface.transparent.Length; i++)
                {
                    Color color = surface.colors[i];
                    color.a *= surface.alpha;
                    surface.transparent[i].SetColor("_BaseColor", color);
                }
                surface.renderer.sharedMaterials = surface.transparent;
            }
        }

        private static Surface CreateSurface(Renderer renderer)
        {
            Material[] originals = renderer.sharedMaterials;
            var surface = new Surface { renderer = renderer, originals = originals,
                transparent = new Material[originals.Length], colors = new Color[originals.Length] };
            for (int i = 0; i < originals.Length; i++)
            {
                var material = new Material(originals[i]) { hideFlags = HideFlags.DontSave };
                surface.transparent[i] = material;
                surface.colors[i] = material.GetColor("_BaseColor");
                material.SetFloat("_Surface", 1);
                material.SetFloat("_Blend", 0);
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt("_SrcBlendAlpha", (int)BlendMode.One);
                material.SetInt("_DstBlendAlpha", (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.SetOverrideTag("RenderType", "Transparent");
                material.renderQueue = (int)RenderQueue.Transparent;
                material.SetShaderPassEnabled("ShadowCaster", false);
                material.SetShaderPassEnabled("DepthOnly", false);
            }
            return surface;
        }

        private void OnDisable()
        {
            foreach (Surface surface in surfaces.Values)
                if (surface.renderer != null) { surface.renderer.sharedMaterials = surface.originals; surface.alpha = 1; }
        }
        private void OnDestroy()
        {
            OnDisable();
            foreach (Surface surface in surfaces.Values)
                foreach (Material material in surface.transparent)
                    if (Application.isPlaying) Destroy(material); else DestroyImmediate(material);
            surfaces.Clear();
        }
    }
}
