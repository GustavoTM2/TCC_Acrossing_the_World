using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace CrossingTheWorld.TCCEgypt.Editor
{
    public static class EgyptTerrainRepair
    {
        private const string ScenePath = "Assets/TCCEgito/Scenes/Egito_Exterior_Jogavel.unity";
        private const string Generated = "Assets/TCCEgito/Generated";
        private const string SandPath = "Assets/TCCEgito/Art/Ambiente/Areia.png";

        [MenuItem("TCC/Corrigir terreno do Egito")]
        public static void RepairFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Saia do Play antes de corrigir o terreno.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            try { Repair(); }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        public static void Repair()
        {
            Texture2D sand = AssetDatabase.LoadAssetAtPath<Texture2D>(SandPath);
            if (sand == null) throw new InvalidOperationException("Falta a textura " + SandPath);
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
                throw new InvalidOperationException("Abra o projeto com a fase integrada: não encontrei " + ScenePath);
            if (!AssetDatabase.IsValidFolder(Generated)) AssetDatabase.CreateFolder("Assets/TCCEgito", "Generated");

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Terrain[] terrains = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Terrain>(true)).ToArray();
            if (terrains.Length == 0) throw new InvalidOperationException("A cena não contém Terrain.");
            TerrainLayer sandLayer = null;
            int repaired = 0;
            foreach (Terrain terrain in terrains)
            {
                TerrainData source = terrain.terrainData;
                if (source == null) throw new InvalidOperationException("Terrain sem TerrainData: " + terrain.name);
                TerrainLayer[] previous = source.terrainLayers;
                Debug.Log("TCC_TERRAIN_BEFORE: " + terrain.name + ", layers=" + previous.Length
                    + ", missingDiffuse=" + previous.Count(layer => layer == null || layer.diffuseTexture == null)
                    + ", data=" + AssetDatabase.GetAssetPath(source));
                if (previous.Length > 0 && previous.All(layer => layer != null && layer.diffuseTexture != null))
                {
                    Debug.Log("O terreno já possui texturas em todas as camadas; preservei sua pintura.");
                    continue;
                }

                string sourcePath = AssetDatabase.GetAssetPath(source);
                string guid = AssetDatabase.AssetPathToGUID(sourcePath);
                string dataPath = Generated + "/Egito_Terreno_" + guid + ".asset";
                TerrainData data = AssetDatabase.LoadAssetAtPath<TerrainData>(dataPath);
                if (data == null)
                {
                    if (string.IsNullOrEmpty(sourcePath) || !AssetDatabase.CopyAsset(sourcePath, dataPath))
                        throw new InvalidOperationException("Não consegui criar uma cópia do TerrainData.");
                    data = AssetDatabase.LoadAssetAtPath<TerrainData>(dataPath);
                }

                if (sandLayer == null)
                {
                    string layerPath = Generated + "/Egito_Areia.terrainlayer";
                    sandLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(layerPath);
                    if (sandLayer == null)
                    {
                        sandLayer = new TerrainLayer
                        {
                            name = "Egito_Areia", diffuseTexture = sand,
                            tileSize = new Vector2(12, 12), smoothness = 0, metallic = 0
                        };
                        AssetDatabase.CreateAsset(sandLayer, layerPath);
                    }
                }

                bool hadNoLayers = data.terrainLayers.Length == 0;
                TerrainLayer[] layers = data.terrainLayers;
                if (hadNoLayers) layers = new[] { sandLayer };
                else
                    for (int i = 0; i < layers.Length; i++)
                        if (layers[i] == null || layers[i].diffuseTexture == null) layers[i] = sandLayer;
                data.terrainLayers = layers;
                if (hadNoLayers)
                {
                    float[,,] paint = new float[data.alphamapHeight, data.alphamapWidth, 1];
                    for (int y = 0; y < data.alphamapHeight; y++)
                        for (int x = 0; x < data.alphamapWidth; x++) paint[y, x, 0] = 1;
                    data.SetAlphamaps(0, 0, paint);
                }

                terrain.terrainData = data;
                TerrainCollider collider = terrain.GetComponent<TerrainCollider>();
                if (collider != null) collider.terrainData = data;
                RenderPipelineAsset pipeline = GraphicsSettings.defaultRenderPipeline;
                string materialPath = Generated + "/Egito_Terreno.mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    if (pipeline != null && pipeline.defaultTerrainMaterial != null)
                        material = new Material(pipeline.defaultTerrainMaterial);
                    else
                    {
                        Shader shader = Shader.Find("Nature/Terrain/Standard");
                        if (shader == null) throw new InvalidOperationException("Shader de terreno indisponível.");
                        material = new Material(shader);
                    }
                    material.name = "Egito_Terreno";
                    AssetDatabase.CreateAsset(material, materialPath);
                }
                terrain.materialTemplate = material;
                terrain.Flush();
                EditorUtility.SetDirty(data);
                repaired++;
                Debug.Log("TCC_TERRAIN_REPAIRED: " + terrain.name + ", layers=" + data.terrainLayers.Length
                    + ", texture=" + AssetDatabase.GetAssetPath(sand) + ", shader=" + material.shader.name);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Não consegui salvar a cena corrigida.");
            AssetDatabase.SaveAssets();
            Debug.Log("Terreno do Egito corrigido: " + repaired + ". O relevo, as colisões e o cenário original foram preservados.");
        }

        public static void ValidateBatch()
        {
            try
            {
                Repair();
                Scene scene = SceneManager.GetActiveScene();
                Terrain terrain = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Terrain>(true)).Single();
                TerrainData original = AssetDatabase.LoadAssetAtPath<TerrainData>("Assets/New Terrain.asset");
                TerrainData corrected = terrain.terrainData;
                if (original == corrected || original.size != corrected.size
                    || original.heightmapResolution != corrected.heightmapResolution)
                    throw new InvalidOperationException("A correção não preservou a cópia do relevo.");
                for (int y = 0; y < original.heightmapResolution; y += 32)
                    for (int x = 0; x < original.heightmapResolution; x += 32)
                        if (!Mathf.Approximately(original.GetHeight(x, y), corrected.GetHeight(x, y)))
                            throw new InvalidOperationException("O relevo foi alterado.");
                if (corrected.terrainLayers.Length == 0
                    || corrected.terrainLayers.Any(layer => layer == null || layer.diffuseTexture == null)
                    || terrain.GetComponent<TerrainCollider>().terrainData != corrected)
                    throw new InvalidOperationException("Terreno sem textura ou collider correspondente.");
                float[,,] paint = corrected.GetAlphamaps(0, 0, 1, 1);
                if (paint[0, 0, 0] < 0.99f) throw new InvalidOperationException("A camada de areia não foi pintada.");
                Debug.Log("TCC_TERRAIN_VALIDATION_OK: areia, relevo preservado, cópia de TerrainData e collider.");
                EgyptSceneSetup.BuildForValidation();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }
    }
}
