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
            CloseFloorSeams(environment);
            foreach (Light lamp in environment.GetComponentsInChildren<Light>(true))
            {
                if (lamp.type == LightType.Point) lamp.intensity = 2f;
            }
            Physics.SyncTransforms();
            EgyptInteractor interactor = player.GetComponent<EgyptInteractor>();
            EgyptInteractionPoint mummy = interactor.points.Single(p => p.kind == EgyptPointKind.Mummy);
            EgyptInteractionPoint exit = interactor.points.Single(p => p.kind == EgyptPointKind.Exit);
            EgyptCamera25D follow = Camera.main.GetComponent<EgyptCamera25D>();
            // O percurso agora segue o comprimento da sala e termina no alto da escada.
            // Reaplico o mesmo layout ao atualizar os modelos, sem voltar à posição antiga.
            {
                player.transform.position = FloorAt(environment, new Vector3(0, 0, 16.5f)) + Vector3.up * 0.08f;
                player.respawnBelowY = player.transform.position.y - 10;
                player.movementAxis = Vector3.back;
                player.depthAxis = Vector3.right;
                if (player.visual != null) player.visual.rotation = Quaternion.LookRotation(player.movementAxis)
                    * Quaternion.Euler(0, player.visualYawOffset, 0);
                // Cada degrau mede aproximadamente 24 cm; a cápsula sobe sem precisar pular.
                player.GetComponent<CharacterController>().stepOffset = 0.35f;
                // Parto da altura do patamar para o raycast não pegar o teto nem o piso inferior.
                mummy.transform.position = FloorAt(environment, new Vector3(0, 2.4f, -6.25f));
                mummy.transform.rotation = Quaternion.identity;
                mummy.radius = 1.5f;
                mummy.maxHeightDifference = 0.3f;
                exit.transform.position = FloorAt(environment, new Vector3(0, 0, 17.2f));
                exit.radius = 1.5f;
                Transform marker = exit.transform.Find("Visual_Provisorio_Substituir");
                if (marker != null) marker.gameObject.SetActive(false);
                // Trilho paralelo à parede lateral: profundidade, altura e ângulo ficam fixos.
                follow.useWallRail = true;
                // O trilho fica dentro da sala, à frente da parede, para não mostrar o vazio externo.
                float railHeight = mummy.transform.position.y + 1.6f;
                follow.railStart = new Vector3(-7.5f, railHeight, -3.8f);
                follow.railEnd = new Vector3(-7.5f, railHeight, 14.5f);
                // Na altura da múmia, inclino um pouco o olhar para ainda enxergar o piso inferior.
                follow.railLookDirection = new Vector3(1f, -0.2f, 0);
                follow.offset = new Vector3(-8f, 2.2f, 3f);
                follow.lookOffset = new Vector3(0, 1.1f, -1.8f);
                follow.depthAxis = player.depthAxis;
                follow.depthFollow = 0f;
                follow.fixedHorizontalFraming = false;
                follow.horizontalFramePosition = 0;
                follow.clampFramingZ = true;
                follow.minFramingZ = -2.8f;
                follow.maxFramingZ = 12.3f;
                Camera.main.fieldOfView = 58;
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = new Color(0.025f, 0.022f, 0.018f);
                Vector3 railPosition = follow.railStart;
                railPosition.z = Mathf.Clamp(player.transform.position.z, follow.railStart.z, follow.railEnd.z);
                Camera.main.transform.position = railPosition;
                Camera.main.transform.rotation = Quaternion.LookRotation(follow.railLookDirection, Vector3.up);
                var bounds = player.stage.GetComponentInChildren<EgyptMovementBounds>();
                bounds.transform.position = Vector3.zero;
                bounds.transform.rotation = Quaternion.LookRotation(player.depthAxis, Vector3.up);
                bounds.horizontalMin = -17.7f;
                bounds.horizontalMax = 7.2f;
                // Esta parede invisível impede Amira de passar embaixo ou atrás da câmera.
                bounds.frontDistance = 2f;
                bounds.backDistance = 6.7f;
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
                idle.speaker = EgyptDialogueContent.MummySpeaker;
                idle.headNodDegrees = 1.8f;
                idle.armSwayDegrees = 2.5f;
            }
            player.stage.player = player;
            Camera.main.fieldOfView = 58;
            EgyptCameraObstruction obstruction = Camera.main.GetComponent<EgyptCameraObstruction>();
            if (obstruction == null) obstruction = Camera.main.gameObject.AddComponent<EgyptCameraObstruction>();
            obstruction.environment = environment.transform;
            obstruction.target = player.transform;
            // As paredes e o piso permanecem opacos; só pilares e corrimãos podem esconder Amira.
            obstruction.cutawaySurfaces = Array.Empty<Renderer>();
            obstruction.fadeOnlyInteriorProps = true;
            Debug.Log("TCC_INTERIOR_REPAIRED: trilho dentro da sala, barreira frontal, fechamento das frestas do piso e múmia no patamar.");
        }

        private static void CloseFloorSeams(GameObject environment)
        {
            const string name = "Fechamento_Frestas_Piso";
            Transform backing = environment.transform.Find(name);
            if (backing == null)
            {
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = name;
                backing = floor.transform;
                backing.SetParent(environment.transform, false);
            }
            // Uma camada abaixo dos módulos fecha pequenas frestas sem alterar o chão onde Amira anda.
            backing.position = new Vector3(-0.8f, -0.12f, 5.2f);
            backing.rotation = Quaternion.identity;
            Vector3 scale = environment.transform.lossyScale;
            backing.localScale = new Vector3(16.4f / scale.x, 0.1f / scale.y, 26.3f / scale.z);
            foreach (Collider collider in backing.GetComponents<Collider>()) UnityEngine.Object.DestroyImmediate(collider);
            backing.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Generated + "Interior_Tile.mat");
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
