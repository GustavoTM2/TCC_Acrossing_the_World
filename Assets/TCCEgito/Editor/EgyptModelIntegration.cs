using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace CrossingTheWorld.TCCEgypt.Editor
{
    public static class EgyptModelIntegration
    {
        private const string Art = "Assets/TCCEgito/Art/";
        private const string Generated = "Assets/TCCEgito/Generated/";
        private static readonly string[] Ids = { "Amira", "Guia", "Mumia", "Escaravelho" };

        public static string ModelPath(string id)
        {
            return Art + id + "/" + (id == "Amira" ? "Amira_Caminhada" : id) + ".fbx";
        }
        public static string TexturePath(string id) { return Art + id + "/" + id + ".png"; }

        public static void ApplyToScene(Scene scene)
        {
            foreach (string id in Ids)
                if (AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath(id)) == null
                    || AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath(id)) == null)
                    throw new InvalidOperationException("Falta o modelo ou a textura de " + id + " em Assets/TCCEgito/Art.");

            EgyptPlayer25D player = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<EgyptPlayer25D>(true)).Single();
            RuntimeAnimatorController controller = player.animator != null ? player.animator.runtimeAnimatorController : null;
            if (controller == null)
                controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(Generated + "Amira_Movimento.controller");
            Quaternion rotation = player.visual != null ? player.visual.localRotation : Quaternion.identity;
            if (player.visual != null && player.visual != player.transform)
                UnityEngine.Object.DestroyImmediate(player.visual.gameObject);
            else RemoveVisualChildren(player.gameObject, "Visual_Amira", "Visual_Modelo");

            CharacterController capsule = player.GetComponent<CharacterController>();
            float height = capsule.height * Mathf.Abs(player.transform.lossyScale.y);
            Vector3 feet = player.transform.TransformPoint(capsule.center - Vector3.up * capsule.height * 0.5f);
            GameObject amira = MakeVisual("Amira", player.transform, height, false, rotation, Vector3.zero);
            amira.name = "Visual_Amira";
            Bounds amiraBounds = BoundsOf(amira);
            amira.transform.position += Vector3.up * (feet.y - amiraBounds.min.y);
            Animator animator = amira.GetComponentInChildren<Animator>();
            if (animator == null) animator = amira.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            player.visual = amira.transform;
            player.animator = animator;

            EgyptInteractionPoint[] points = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<EgyptInteractionPoint>(true)).ToArray();
            foreach (EgyptInteractionPoint point in points)
            {
                string id = point.kind == EgyptPointKind.Guide ? "Guia"
                    : point.kind == EgyptPointKind.Mummy ? "Mumia" : null;
                if (id == null) continue;
                RemoveVisualChildren(point.gameObject, "Visual_Provisorio_Substituir", "Visual_Modelo");
                MakeVisual(id, point.transform, 1.8f * Mathf.Abs(point.transform.lossyScale.y), false,
                    Quaternion.identity, new Vector3(0, 0, -0.5f));
                point.name = id == "Guia" ? "Guia_Turistico" : "Mumia_Guardia";
            }

            EgyptStage stage = player.stage;
            if (stage.interior && stage.scarabVisual != null)
            {
                GameObject reward = stage.scarabVisual;
                bool active = reward.activeSelf;
                reward.SetActive(true);
                reward.name = "Velho_Escaravelho";
                reward.transform.localScale = Vector3.one;
                RemoveVisualChildren(reward, "Visual_Modelo");
                foreach (Component component in new Component[] { reward.GetComponent<MeshRenderer>(),
                    reward.GetComponent<MeshFilter>(), reward.GetComponent<Collider>() })
                    if (component != null) UnityEngine.Object.DestroyImmediate(component);
                MakeVisual("Escaravelho", reward.transform, 0.55f, true,
                    Quaternion.Euler(35, 0, 0), Vector3.zero);
                reward.SetActive(active);
            }
        }

        private static void RemoveVisualChildren(GameObject root, params string[] names)
        {
            for (int i = root.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = root.transform.GetChild(i);
                if (names.Contains(child.name)) UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        public static GameObject MakeVisual(string id, Transform parent, float size, bool longestAxis,
            Quaternion rotation, Vector3 offset)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath(id));
            if (asset == null) throw new InvalidOperationException("Modelo não importado: " + ModelPath(id));
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            visual.name = "Visual_Modelo";
            if (parent != null) visual.transform.SetParent(parent, false);
            visual.transform.localPosition = offset;
            visual.transform.localRotation = rotation;
            visual.transform.localScale = Vector3.one;
            foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            foreach (Camera camera in visual.GetComponentsInChildren<Camera>(true)) camera.enabled = false;
            foreach (Light light in visual.GetComponentsInChildren<Light>(true)) light.enabled = false;

            Material material = TexturedMaterial(id);
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
                renderer.sharedMaterials = Enumerable.Repeat(material, Mathf.Max(1, renderer.sharedMaterials.Length)).ToArray();
            Bounds bounds = BoundsOf(visual);
            float extent = longestAxis ? Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z)) : bounds.size.y;
            if (extent <= 0.0001f) throw new InvalidOperationException("Modelo sem geometria visível: " + id);
            visual.transform.localScale *= size / extent;
            bounds = BoundsOf(visual);
            Vector3 anchor = parent != null ? parent.TransformPoint(offset) : offset;
            if (longestAxis) visual.transform.position += anchor - bounds.center;
            else visual.transform.position += Vector3.up * (anchor.y - bounds.min.y);
            return visual;
        }

        public static Material TexturedMaterial(string id)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath(id));
            if (texture == null) throw new InvalidOperationException("Textura não importada: " + TexturePath(id));
            string path = Generated + id + "_Texturizado.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                RenderPipelineAsset pipeline = GraphicsSettings.defaultRenderPipeline;
                string shaderName = pipeline == null ? "Standard" : pipeline.GetType().Name.Contains("HDRender")
                    ? "HDRP/Lit" : "Universal Render Pipeline/Lit";
                Shader shader = Shader.Find(shaderName);
                if (shader == null) throw new InvalidOperationException("Shader não encontrado: " + shaderName);
                material = new Material(shader) { name = id + "_Texturizado" };
                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
                if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
                AssetDatabase.CreateAsset(material, path);
            }
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
            if (material.HasProperty("_BaseColorMap")) material.SetTexture("_BaseColorMap", texture);
            EditorUtility.SetDirty(material);
            return material;
        }

        public static Bounds BoundsOf(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new InvalidOperationException("Sem Renderer em " + root.name);
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        public static void Validate(Scene scene)
        {
            EgyptPlayer25D player = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<EgyptPlayer25D>(true)).Single();
            ValidateVisual(player.visual.gameObject, "Amira");
            foreach (EgyptInteractionPoint point in player.GetComponent<EgyptInteractor>().points)
            {
                string id = point.kind == EgyptPointKind.Guide ? "Guia" : point.kind == EgyptPointKind.Mummy ? "Mumia" : null;
                if (id != null) ValidateVisual(point.transform.Find("Visual_Modelo").gameObject, id);
            }
            if (player.stage.interior) ValidateVisual(player.stage.scarabVisual.transform.Find("Visual_Modelo").gameObject, "Escaravelho");
            Debug.Log("TCC_EGITO_MODELS_OK: " + scene.path);
        }

        private static void ValidateVisual(GameObject visual, string id)
        {
            if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(visual) != ModelPath(id))
                throw new InvalidOperationException("Modelo incorreto na cena: " + id);
            Texture2D expected = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath(id));
            foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>(true))
                foreach (Material material in renderer.sharedMaterials)
                    if (material == null || !new[] { "_BaseMap", "_MainTex", "_BaseColorMap" }
                        .Any(property => material.HasProperty(property) && material.GetTexture(property) == expected))
                        throw new InvalidOperationException("Material sem a textura fornecida: " + id);
        }
    }
}
