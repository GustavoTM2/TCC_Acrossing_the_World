using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace CrossingTheWorld.TCCBrazil.Editor
{
    // Confiro os materiais do mapa recebido antes de testar a aventura.
    public static class BrazilMapReview
    {
        public static void ReviewAndPlay()
        {
            EditorSceneManager.OpenScene(BrazilSceneSetup.ScenePath);
            var report = new StringBuilder();
            foreach (var terrain in Terrain.activeTerrains)
            {
                report.AppendLine("Terrain: " + AssetDatabase.GetAssetPath(terrain.terrainData) + " posição=" + terrain.transform.position);
                report.AppendLine("Material: " + (terrain.materialTemplate != null ? terrain.materialTemplate.name + " / " + terrain.materialTemplate.shader.name : "ausente"));
                foreach (var layer in terrain.terrainData.terrainLayers)
                    report.AppendLine("Camada: " + (layer != null ? layer.name + " / " + (layer.diffuseTexture != null ? layer.diffuseTexture.name : "sem textura") : "ausente"));
            }
            foreach (var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                foreach (var material in renderer.sharedMaterials)
                    if (material == null || material.shader == null || !material.shader.isSupported)
                        report.AppendLine("Material ausente ou incompatível: " + renderer.name);
            string output = Environment.GetEnvironmentVariable("TCC_BRAZIL_VALIDATION_OUTPUT") ?? Path.GetFullPath("Temp/MapImport");
            Directory.CreateDirectory(output);
            File.WriteAllText(Path.Combine(output, "materiais.txt"), report.ToString());
            BrazilAdventureValidation.RunAndLeaveOpen();
        }
    }
}
