using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
namespace CrossingTheWorld.TCCBrazil.Editor
{
    public static class BrazilSceneryIntegration
    {
        // Troca somente o cenário; os objetos da aventura continuam com suas referências.
        [MenuItem("TCC/Brasil/Integrar cenario da equipe")]
        public static void Integrate()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var target = EditorSceneManager.OpenScene(BrazilSceneSetup.ScenePath);
            var source = EditorSceneManager.OpenScene("Assets/TCCBrasil/Source/Brasil_Cenario_Equipe.unity", OpenSceneMode.Additive);
            foreach (var go in target.GetRootGameObjects())
                if (go.name != "TCC_Brasil_Exploracao" && go.name != "TCC_Brasil_Aventura") Object.DestroyImmediate(go);
            foreach (var go in source.GetRootGameObjects())
                if (go.name != "TCC_Brasil_Exploracao") SceneManager.MoveGameObjectToScene(go, target);
            EditorSceneManager.CloseScene(source, true);
            SceneManager.SetActiveScene(target);
            var terrain = Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None)
                .Single(t => AssetDatabase.GetAssetPath(t.terrainData) == "Assets/TCCBrasil/Generated/Praia_Terreno.asset");
            var data = terrain.terrainData;
            int resolution = data.heightmapResolution;
            var heights = data.GetHeights(0, 0, resolution, resolution);
            // Mantenho o relevo novo ao redor e nivelo apenas o caminho do desafio.
            for (int z = 0; z < resolution; z++) for (int x = 0; x < resolution; x++)
            {
                float wx = terrain.transform.position.x + x * data.size.x / (resolution - 1);
                float wz = terrain.transform.position.z + z * data.size.z / (resolution - 1);
                float distance = Mathf.Max(Mathf.Max(356 - wx, wx - 382), Mathf.Max(516 - wz, wz - 584));
                float blend = 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01(distance / 16));
                heights[z,x] = Mathf.Lerp(heights[z,x], (77.13f - terrain.transform.position.y) / data.size.y, blend);
            }
            data.SetHeights(0, 0, heights);
            terrain.materialTemplate = AssetDatabase.LoadAssetAtPath<Material>("Assets/TCCBrasil/Generated/Areia_Terreno_URP.mat");
            terrain.GetComponent<TerrainCollider>().terrainData = data;
            BrazilAdventureSetup.ResizeProps(terrain);
            EditorUtility.SetDirty(data);
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(target);
            EditorSceneManager.SaveScene(target);
            AssetDatabase.SaveAssets();
            BrazilAdventureSetup.ValidateScene();
            Debug.Log("BRAZIL_SCENERY_INTEGRATION_OK");
        }
    }
}
