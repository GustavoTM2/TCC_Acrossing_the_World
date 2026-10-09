using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace CrossingTheWorld.TCCEgypt.Editor
{
    // Ajusta somente a cópia jogável; cenas, prefabs e materiais originais permanecem preservados.
    public static class EgyptInteriorRepair
    {
        private const string EnvironmentName = "Cenario_Interior_Original";
        private const string Generated = "Assets/TCCEgito/Generated/";

        [MenuItem("TCC/Corrigir interior da piramide")]
        public static void RepairFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { Debug.LogWarning("Saia do Play antes de corrigir o interior."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            try { RepairAndSave(); }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        public static void RepairAndSave()
        {
            Scene scene = EditorSceneManager.OpenScene(EgyptFreeDepthSetup.Interior);
            ApplyToScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Não consegui salvar o interior.");
            AssetDatabase.SaveAssets();
        }

        public static void ApplyToScene(Scene scene)
        {
            EgyptPlayer25D player = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<EgyptPlayer25D>(true)).Single();
            if (!player.stage.interior) return;
            GameObject environment = scene.GetRootGameObjects().FirstOrDefault(r => r.name == EnvironmentName);
            bool firstRepair = environment == null;
            if (firstRepair)
            {
                GameObject[] originalRoots = scene.GetRootGameObjects();
                environment = new GameObject(EnvironmentName);
                environment.transform.position = new Vector3(368.6f, 0, 1030.5f);
                foreach (GameObject root in originalRoots)
                {
                    if (root == player.stage.gameObject || root.GetComponent<Camera>() != null) continue;
                    Light light = root.GetComponent<Light>();
                    if (light != null && light.type == LightType.Directional) continue;
                    root.transform.SetParent(environment.transform, true);
                    foreach (Light lamp in root.GetComponentsInChildren<Light>(true)) lamp.range *= 0.1f;
                }
                environment.transform.localScale = Vector3.one * 0.1f;
                environment.transform.position = Vector3.zero;
            }
            foreach (MeshFilter filter in environment.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null || filter.GetComponent<Collider>() != null) continue;
                filter.gameObject.AddComponent<MeshCollider>().sharedMesh = filter.sharedMesh;
            }
            RepairMaterials(environment);
            foreach (Light lamp in environment.GetComponentsInChildren<Light>(true))
            {
                if (lamp.type == LightType.Point) lamp.intensity = 2f;
            }
            Physics.SyncTransforms();
            EgyptInteractor interactor = player.GetComponent<EgyptInteractor>();
            EgyptInteractionPoint mummy = interactor.points.Single(p => p.kind == EgyptPointKind.Mummy);
            EgyptInteractionPoint exit = interactor.points.Single(p => p.kind == EgyptPointKind.Exit);
            EgyptCamera25D follow = Camera.main.GetComponent<EgyptCamera25D>();
            if (firstRepair || !follow.fixedHorizontalFraming)
            {
                // Raycast começa abaixo do teto. O Tile_B superior não é o piso da sala.
                player.transform.position = FloorAt(environment, new Vector3(6, 0, 6)) + Vector3.up * 0.08f;
                player.respawnBelowY = player.transform.position.y - 10;
                mummy.transform.position = FloorAt(environment, new Vector3(-6, 0, 6));
                exit.transform.position = FloorAt(environment, new Vector3(7.4f, 0, 6));
                Transform marker = exit.transform.Find("Visual_Provisorio_Substituir");
                if (marker != null) marker.gameObject.SetActive(false);
                follow.offset = new Vector3(0, 2.6f, 8f);
                follow.lookOffset = new Vector3(0, 1.1f, 0);
                follow.depthFollow = 0.2f;
                follow.fixedHorizontalFraming = true;
                follow.horizontalFramePosition = 0;
                Camera.main.fieldOfView = 65;
                Vector3 frame = player.transform.position;
                frame.x = follow.horizontalFramePosition;
                Camera.main.transform.position = frame + follow.offset;
                Camera.main.transform.LookAt(frame + follow.lookOffset);
                var bounds = player.stage.GetComponentInChildren<EgyptMovementBounds>();
                bounds.transform.position = player.transform.position;
                bounds.horizontalMin = -1.8f;
                bounds.horizontalMax = 13.8f;
                bounds.frontDistance = 1.2f;
                bounds.backDistance = 2.5f;
                bounds.UpdateColliders();
                if (player.stage.scarabVisual != null)
                {
                    player.stage.scarabVisual.transform.position = mummy.transform.position + new Vector3(0, 1.25f, 0.3f);
                    player.stage.scarabVisual.SetActive(false);
                }
            }
            foreach (EgyptNpcIdle idle in mummy.GetComponentsInChildren<EgyptNpcIdle>(true))
            {
                idle.stage = player.stage;
                idle.speaker = "Múmia";
                idle.headNodDegrees = 1.8f;
                idle.armSwayDegrees = 2.5f;
            }
            player.stage.player = player;
            Camera.main.fieldOfView = 65;
            EgyptCameraObstruction obstruction = Camera.main.GetComponent<EgyptCameraObstruction>();
            if (obstruction == null) obstruction = Camera.main.gameObject.AddComponent<EgyptCameraObstruction>();
            obstruction.environment = environment.transform;
            obstruction.target = player.transform;
            Debug.Log("TCC_INTERIOR_REPAIRED: estrutura original preservada, escala 0.1, jogador no piso interno; " + environment.GetComponentsInChildren<Renderer>(true).Length + " módulos visuais.");
        }

        public static Vector3 FloorAt(GameObject environment, Vector3 point)
        {
            foreach (RaycastHit hit in Physics.RaycastAll(point + Vector3.up * 2, Vector3.down, 5, ~0, QueryTriggerInteraction.Ignore).OrderBy(h => h.distance))
            {
                if (!hit.collider.transform.IsChildOf(environment.transform) || hit.normal.y < 0.65f) continue;
                for (Transform parent = hit.collider.transform; parent != environment.transform; parent = parent.parent)
                    if (parent.name.StartsWith("Tile_") || parent.name.StartsWith("Step_")) return new Vector3(point.x, hit.point.y, point.z);
            }
            throw new InvalidOperationException("Piso interno ausente em " + point + ". Confira os módulos originais.");
        }

        private static void RepairMaterials(GameObject environment)
        {
            if (!(GraphicsSettings.defaultRenderPipeline is UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)) return;
            foreach (Renderer renderer in environment.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material original = materials[i];
                    if (original == null || !AssetDatabase.GetAssetPath(original).StartsWith("Assets/DungeonModularPack/")) continue;
                    string textureId = original.name.Replace("M_", "").Replace("_A", "");
                    string path = Generated + "Interior_" + textureId + ".mat";
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (material == null)
                    {
                        material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "Interior_" + textureId };
                        AssetDatabase.CreateAsset(material, path);
                    }
                    Texture2D color = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/DungeonModularPack/Textures/T_" + textureId + "_C.tga");
                    Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/DungeonModularPack/Textures/T_" + textureId + "_N.tga");
                    if (color == null) throw new InvalidOperationException("Textura original ausente: " + textureId);
                    material.SetTexture("_BaseMap", color);
                    material.SetColor("_BaseColor", new Color(1f, 0.89f, 0.72f));
                    material.SetFloat("_Smoothness", 0.15f);
                    material.SetTexture("_BumpMap", normal);
                    if (normal != null) material.EnableKeyword("_NORMALMAP");
                    EditorUtility.SetDirty(material);
                    materials[i] = material;
                }
                renderer.sharedMaterials = materials;
            }
        }
    }
}
