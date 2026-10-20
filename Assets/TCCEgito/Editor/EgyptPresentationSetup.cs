using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrossingTheWorld.TCCEgypt.Editor
{
    public static class EgyptPresentationSetup
    {
        public const string Exterior = "Assets/TCCEgito/Scenes/Egito_Exterior_Jogavel.unity";
        public const string Interior = "Assets/TCCEgito/Scenes/Egito_Interior_Jogavel.unity";
        private const string PrefabPath = "Assets/TCCEgito/Generated/Escaravelho_Interface.prefab";
        private const string MaterialPath = "Assets/TCCEgito/Generated/Escaravelho_Interface.mat";

        [MenuItem("TCC/Egito/Atualizar altura e escaravelho na tela")]
        public static void ApplyFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ApplyAndSave();
        }

        public static void ApplyAndSave()
        {
            if (Application.unityVersion != "6000.5.7f1")
                throw new InvalidOperationException("Abra o projeto no Unity 6000.5.7f1.");
            GameObject prefab = PreparePreviewPrefab();
            foreach (string path in new[] { Exterior, Interior })
            {
                Scene scene = EditorSceneManager.OpenScene(path);
                EgyptPlayer25D player = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<EgyptPlayer25D>(true)).Single();
                player.stage.player = player;
                if (!player.stage.interior)
                {
                    player.visualHeightOffset = -0.08f;
                    player.ApplyVisualHeightOffset();
                }
                EgyptScarabDisplay display = player.stage.GetComponent<EgyptScarabDisplay>();
                if (display == null) display = player.stage.gameObject.AddComponent<EgyptScarabDisplay>();
                display.previewPrefab = prefab;
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Não consegui salvar " + path);
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(Exterior);
            Debug.Log("EGYPT_PRESENTATION_SETUP_OK: Amira -8 cm no exterior e escaravelho nas duas cenas.");
        }

        private static GameObject PreparePreviewPrefab()
        {
            // Uso a textura original em um material sem depender das luzes do deserto ou da pirâmide.
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(EgyptModelIntegration.TexturePath("Escaravelho")));
            material.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(material);
            GameObject root = new GameObject("Escaravelho_Interface");
            try
            {
                GameObject model = EgyptModelIntegration.MakeVisual("Escaravelho", root.transform, 1.45f,
                    true, Quaternion.Euler(35, 0, 0), Vector3.zero);
                foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
                    renderer.sharedMaterials = Enumerable.Repeat(material, renderer.sharedMaterials.Length).ToArray();
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                if (prefab == null) throw new InvalidOperationException("Não consegui preparar o escaravelho da interface.");
                return prefab;
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
    }
}
