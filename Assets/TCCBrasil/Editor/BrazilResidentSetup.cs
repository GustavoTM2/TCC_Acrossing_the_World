using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CrossingTheWorld.TCCEgypt;
using Object = UnityEngine.Object;

namespace CrossingTheWorld.TCCBrazil.Editor
{
    public static class BrazilResidentSetup
    {
        public const string ModelPath = "Assets/TCCBrasil/Art/Morador/Morador_Brasileiro.fbx";
        public const string TexturePath = "Assets/TCCBrasil/Art/Morador/Morador_Brasileiro.png";
        private const string MaterialPath = "Assets/TCCBrasil/Generated/Morador_Texturizado.mat";

        // Atualizo só o visual do morador, mantendo o ponto de conversa e o desafio.
        [MenuItem("TCC/Brasil/Atualizar modelo do morador")]
        public static void UpdateScene()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Saia do Play antes de atualizar o modelo.");
            if (Application.unityVersion != "6000.5.7f1") throw new InvalidOperationException("Use Unity 6000.5.7f1.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(BrazilSceneSetup.ScenePath);
            var stage = Object.FindAnyObjectByType<BrazilStage>();
            if (stage == null || stage.resident == null) throw new InvalidOperationException("O ponto do morador não está configurado.");
            ApplyVisual(stage.resident);
            Validate(stage);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("BRAZIL_RESIDENT_READY: modelo, textura, escala, articulações e ponto de interação verificados.");
        }

        public static Transform Create(Transform parent, Vector3 position, Quaternion rotation)
        {
            var root = new GameObject("Morador_Brasileiro").transform;
            root.SetParent(parent,false); root.SetPositionAndRotation(position,rotation);
            ApplyVisual(root);
            return root;
        }

        private static void ApplyVisual(Transform root)
        {
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            if (importer == null) throw new InvalidOperationException("Falta o FBX do morador.");
            if (!importer.isReadable || importer.animationType != ModelImporterAnimationType.Generic || importer.importAnimation)
            {
                importer.isReadable = true; importer.animationType = ModelImporterAnimationType.Generic;
                importer.importAnimation = false; importer.SaveAndReimport();
            }
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            if (model == null || texture == null) throw new InvalidOperationException("Falta o modelo ou a textura do morador.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "Morador_Texturizado" };
                AssetDatabase.CreateAsset(material,MaterialPath);
            }
            material.SetTexture("_BaseMap",texture); material.SetColor("_BaseColor",Color.white);
            material.SetFloat("_Smoothness",.12f); material.SetFloat("_Metallic",0);
            EditorUtility.SetDirty(material);

            // Preparo a substituição antes de retirar o visual anterior.
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
            visual.name = "Visual_Morador"; visual.transform.SetParent(root,false);
            foreach (var animator in visual.GetComponentsInChildren<Animator>(true)) animator.enabled = false;
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterials = Enumerable.Repeat(material,renderer.sharedMaterials.Length).ToArray();
            var bones = visual.GetComponentsInChildren<Transform>(true);
            Transform head = bones.Single(t => t.name == "Bone.003");
            Transform left = bones.Single(t => t.name == "braco.l");
            Transform right = bones.Single(t => t.name == "braco.r");
            var bounds = BrazilSceneSetup.BoundsOf(visual);
            visual.transform.localScale *= 1.8f / bounds.size.y;
            bounds = BrazilSceneSetup.BoundsOf(visual);
            visual.transform.position += new Vector3(root.position.x - bounds.center.x,
                root.position.y - bounds.min.y, root.position.z - bounds.center.z);

            foreach (var idle in root.GetComponents<EgyptNpcIdle>()) Object.DestroyImmediate(idle);
            foreach (Transform child in root.Cast<Transform>().ToArray())
                if (child != visual.transform) Object.DestroyImmediate(child.gameObject);
            root.name = "Morador_Brasileiro";
            var idleMotion = root.gameObject.AddComponent<EgyptNpcIdle>();
            idleMotion.coordinateSpace = root; idleMotion.head = head; idleMotion.leftArm = left; idleMotion.rightArm = right;
            idleMotion.relaxArmDegrees = 2; idleMotion.armSwayDegrees = 1; idleMotion.headTurnDegrees = 2;
            idleMotion.headNodDegrees = .7f; idleMotion.phaseOffset = 2; idleMotion.CaptureBasePose();
            var collider = root.GetComponent<CapsuleCollider>() ?? root.gameObject.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0,.9f,0); collider.height = 1.8f; collider.radius = .24f;
        }

        public static void Validate(BrazilStage stage)
        {
            var root = stage.resident;
            var skin = root.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skin == null || skin.bones.Length == 0) throw new InvalidOperationException("O modelo precisa manter seu esqueleto.");
            if (skin.sharedMaterials.Any(m => m == null || m.GetTexture("_BaseMap") != AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath)))
                throw new InvalidOperationException("Textura do morador incorreta.");
            var bounds = BrazilSceneSetup.BoundsOf(root.gameObject);
            if (Mathf.Abs(bounds.size.y - 1.8f) > .03f || Mathf.Abs(bounds.min.y - root.position.y) > .03f)
                throw new InvalidOperationException("Altura ou apoio do morador incorretos.");
            if (Vector3.Distance(root.position,stage.challengeAnchor.position) > 5)
                throw new InvalidOperationException("Morador longe da área do desafio.");
        }

        public static void UpdateAndValidateInPlay()
        {
            UpdateScene();
            BrazilAdventureValidation.Run();
        }
    }
}
