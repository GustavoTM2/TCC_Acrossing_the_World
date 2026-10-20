using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CrossingTheWorld.TCCEgypt.Editor
{
    public static class EgyptExplorationSetup
    {
        private const string Generated = "Assets/TCCEgito/Generated/";

        [MenuItem("TCC/Egito/Atualizar exploracao e vegetacao")]
        public static void FromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ApplyAndSave();
        }

        public static void ApplyAndSave()
        {
            if (Application.unityVersion != "6000.5.7f1") throw new InvalidOperationException("Use o Unity 6000.5.7f1.");
            EgyptInteriorRepair.RepairAndSave();
            var scene = EditorSceneManager.OpenScene(EgyptPresentationSetup.Exterior);
            var player = UnityEngine.Object.FindAnyObjectByType<EgyptPlayer25D>();
            var terrain = UnityEngine.Object.FindAnyObjectByType<Terrain>();
            // Aproximo Amira 1,2 m da câmera, mantendo o mesmo ponto de vista sobre a porta.
            Vector3 spawn = new Vector3(438.89f, 0, 956.03f);
            spawn.y = terrain.SampleHeight(spawn) + terrain.transform.position.y + 0.08f;
            player.transform.position = spawn;
            player.respawnBelowY = spawn.y - 15;
            var bounds = player.stage.GetComponentInChildren<EgyptMovementBounds>();
            bounds.transform.position = spawn;
            bounds.UpdateColliders();
            var camera = Camera.main.GetComponent<EgyptCamera25D>();
            camera.offset = new Vector3(0, 3, 7.8f);
            camera.transform.position = spawn + camera.offset;
            camera.transform.LookAt(spawn + camera.lookOffset);

            int count = 0;
            foreach (Renderer renderer in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Renderer>(true)))
            {
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material original = materials[i];
                    if (original == null) continue;
                    string path = AssetDatabase.GetAssetPath(original);
                    if (!path.StartsWith("Assets/TerrainSampleAssets/Materials/") && !path.StartsWith(Generated + "Vegetacao_")) continue;
                    string family = original.name.Replace("Vegetacao_", "");
                    materials[i] = FoliageMaterial(family);
                    changed = true;
                }
                if (!changed) continue;
                renderer.sharedMaterials = materials;
                count++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Não consegui salvar o exterior.");
            AssetDatabase.SaveAssets();
            Debug.Log("EGYPT_EXPLORATION_SETUP_OK: Amira próxima da câmera, sala completa e " + count + " plantas com materiais locais.");
        }

        private static Material FoliageMaterial(string family)
        {
            string path = Generated + "Vegetacao_" + family + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "Vegetacao_" + family };
                AssetDatabase.CreateAsset(material, path);
            }
            // As imagens de cor não vieram nos arquivos recebidos. Uso materiais próprios,
            // sem referências ausentes, preservando os modelos e a posição de cada planta.
            Color color = family.Contains("Dry") ? new Color(0.48f, 0.34f, 0.16f)
                : family == "Fern" ? new Color(0.31f, 0.39f, 0.18f)
                : family == "Plant" ? new Color(0.38f, 0.43f, 0.23f)
                : family == "Heather" ? new Color(0.46f, 0.37f, 0.25f)
                : new Color(0.44f, 0.43f, 0.23f);
            material.SetColor("_BaseColor", color);
            material.SetTexture("_BaseMap", null);
            material.SetFloat("_Metallic", 0);
            material.SetFloat("_Smoothness", 0.08f);
            material.SetFloat("_Cull", 0);
            material.doubleSidedGI = true;
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
