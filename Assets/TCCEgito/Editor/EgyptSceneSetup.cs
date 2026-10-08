using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace CrossingTheWorld.TCCEgypt.Editor
{
    public static class EgyptSceneSetup
    {
        private const string Root = "Assets/TCCEgito";
        private const string Generated = Root + "/Generated";
        private const string SceneFolder = Root + "/Scenes";
        private const string Exterior = SceneFolder + "/Egito_Exterior_Jogavel.unity";
        private const string Interior = SceneFolder + "/Egito_Interior_Jogavel.unity";
        private const string ExteriorSourceGuid = "99c9720ab356a0642a771bea13969a05";
        private const string InteriorSourceGuid = "cd148ddb9ea8fec41b9ed7f12d9c0eeb";
        private const string AmiraGuid = "e0de2ef07061db84b96e74331e71182f";
        private const string AmiraMaterialGuid = "006963aa4c4b9664ea35da119b21813d";

        [MenuItem("TCC/Criar fase jogavel do Egito")]
        public static void BuildFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Saia do Play antes de montar a fase.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            try { Build(); UpdateModelsInScenes(); }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        [MenuItem("TCC/Atualizar modelos do Egito")]
        public static void UpdateModelsFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Saia do Play antes de atualizar os modelos.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            try { Build(); UpdateModelsInScenes(); }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        private static void UpdateModelsInScenes()
        {
            EgyptFreeDepthSetup.PrepareAssets();
            foreach (string path in new[] { Exterior, Interior })
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                EgyptModelIntegration.ApplyToScene(scene);
                EgyptFreeDepthSetup.ApplyToScene(scene);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, path))
                    throw new InvalidOperationException("Não consegui salvar os modelos em " + path);
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(Exterior);
            Debug.Log("Modelos e texturas de Amira, guia, múmia e Escaravelho integrados às cenas jogáveis.");
        }

        // Entrada para validação por linha de comando, sem abrir diálogos.
        public static void BuildForValidation()
        {
            try
            {
                Build();
                UpdateModelsInScenes();
                ValidateScene(Exterior, EgyptPointKind.Guide, EgyptPointKind.Entrance);
                ValidateScene(Interior, EgyptPointKind.Mummy, EgyptPointKind.Exit);
                ValidateDialogueFlow();
                Debug.Log("TCC_EGITO_VALIDATION_OK: duas cenas, controles, câmera e referências válidas.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void Build()
        {
            string exteriorSource = AssetDatabase.GUIDToAssetPath(ExteriorSourceGuid);
            string interiorSource = AssetDatabase.GUIDToAssetPath(InteriorSourceGuid);
            string amiraPath = AssetDatabase.GUIDToAssetPath(AmiraGuid);
            if (string.IsNullOrEmpty(exteriorSource) || string.IsNullOrEmpty(interiorSource)
                || string.IsNullOrEmpty(amiraPath))
                throw new InvalidOperationException("Não encontrei as duas cenas e o modelo de Amira enviados. Instale este pacote naquele projeto.");

            EnsureFolder(Generated);
            EnsureFolder(SceneFolder);
            bool hasExterior = AssetDatabase.LoadAssetAtPath<SceneAsset>(Exterior) != null;
            bool hasInterior = AssetDatabase.LoadAssetAtPath<SceneAsset>(Interior) != null;
            if (hasExterior || hasInterior)
            {
                if (!hasExterior || !hasInterior)
                    throw new InvalidOperationException("Uma das cenas jogáveis já existe. A montagem foi interrompida para preservar seu trabalho. Confira Assets/TCCEgito/Scenes.");
                RegisterScenes();
                EditorSceneManager.OpenScene(Exterior);
                Debug.Log("As cenas jogáveis já existem. Abri o exterior e preservei suas alterações.");
                return;
            }

            AnimatorController animation = MakeAnimation(amiraPath);
            Material guideMaterial = MakeMaterial("Guia", new Color(0.9f, 0.65f, 0.2f));
            Material mummyMaterial = MakeMaterial("Mumia", new Color(0.75f, 0.68f, 0.51f));
            Material scarabMaterial = MakeMaterial("Escaravelho", new Color(0.95f, 0.75f, 0.12f));
            if (!AssetDatabase.CopyAsset(exteriorSource, Exterior)) throw new InvalidOperationException("Não consegui copiar a cena externa.");
            if (!AssetDatabase.CopyAsset(interiorSource, Interior)) throw new InvalidOperationException("Não consegui copiar a cena interna.");

            PrepareScene(Exterior, false, amiraPath, animation, guideMaterial, scarabMaterial);
            PrepareScene(Interior, true, amiraPath, animation, mummyMaterial, scarabMaterial);
            RegisterScenes();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(Exterior);
            Selection.activeGameObject = GameObject.Find("Amira_Jogadora");
            Debug.Log("Egito montado. Abra a janela Game e aperte Play. A/D movem, Espaço pula e E interage. Guia, múmia e Escaravelho têm visuais provisórios.");
        }

        private static void PrepareScene(string path, bool interior, string modelPath,
            AnimatorController animation, Material npcMaterial, Material scarabMaterial)
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            // Desativa somente a instância sem controle da cena copiada.
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == "amira_caminhada") root.SetActive(false);

            if (interior) AddInteriorColliders(scene);
            Physics.SyncTransforms();

            GameObject setup = new GameObject("TCC_Egito_Programacao");
            EgyptStage stage = setup.AddComponent<EgyptStage>();
            stage.interior = interior;
            stage.exteriorScene = Exterior;
            stage.interiorScene = Interior;

            // Posições iniciais escolhidas a partir das cenas fornecidas, todas na mesma faixa Z.
            Vector3 spawn = Ground(interior ? new Vector3(390, 0, 1030.5f)
                : new Vector3(438.89f, 0.78f, 954.83f), interior) + Vector3.up * 0.08f;
            GameObject player = MakePlayer(spawn, modelPath, animation, stage);
            player.transform.SetParent(setup.transform, true);
            EgyptInteractor interactor = player.GetComponent<EgyptInteractor>();

            Vector3 npcPosition = Ground(interior ? new Vector3(372, 0, 1030.5f)
                : new Vector3(433.5f, 0, 954.83f), interior);
            EgyptInteractionPoint npc = MakePoint(interior ? "Mumia_Provisoria" : "Guia_Provisorio",
                npcPosition, interior ? EgyptPointKind.Mummy : EgyptPointKind.Guide, npcMaterial, false);
            npc.transform.SetParent(setup.transform, true);

            Vector3 portalPosition = Ground(interior ? new Vector3(394, 0, 1030.5f)
                : new Vector3(414, 0, 954.83f), interior);
            EgyptInteractionPoint portal = MakePoint(interior ? "Saida_Piramide" : "Entrada_Piramide",
                portalPosition, interior ? EgyptPointKind.Exit : EgyptPointKind.Entrance, scarabMaterial, true);
            portal.transform.SetParent(setup.transform, true);
            interactor.points = new[] { npc, portal };

            if (interior)
            {
                GameObject scarab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                scarab.name = "Escaravelho_Visual_Provisorio";
                scarab.transform.SetParent(setup.transform, true);
                scarab.transform.position = npcPosition + new Vector3(0, 1.3f, 0.65f);
                scarab.transform.localScale = new Vector3(0.32f, 0.18f, 0.45f);
                scarab.GetComponent<Renderer>().sharedMaterial = scarabMaterial;
                UnityEngine.Object.DestroyImmediate(scarab.GetComponent<Collider>());
                stage.scarabVisual = scarab;
                scarab.SetActive(false);

                GameObject fill = new GameObject("Luz_Apoio_Amira");
                fill.transform.SetParent(player.transform, false);
                fill.transform.localPosition = new Vector3(0, 2, 0);
                Light light = fill.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 12;
                light.intensity = 2;
                light.color = new Color(1, 0.83f, 0.59f);
            }
            SetupCamera(player.transform);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, path)) throw new InvalidOperationException("Não consegui salvar " + path);
        }

        private static GameObject MakePlayer(Vector3 spawn, string modelPath, AnimatorController animation, EgyptStage stage)
        {
            GameObject player = new GameObject("Amira_Jogadora");
            player.tag = "Player";
            player.transform.position = spawn;
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0, 0.9f, 0);
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 50;
            controller.skinWidth = 0.03f;

            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));
            visual.name = "Visual_Amira";
            visual.transform.SetParent(player.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            Bounds bounds = VisualBounds(visual);
            if (bounds.size.y > 0.001f) visual.transform.localScale *= 1.8f / bounds.size.y;
            bounds = VisualBounds(visual);
            visual.transform.position += Vector3.up * (spawn.y - bounds.min.y);

            Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(AmiraMaterialGuid));
            if (material != null)
                foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>())
                {
                    Material[] materials = renderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; i++) materials[i] = material;
                    renderer.sharedMaterials = materials;
                }
            Animator animator = visual.GetComponentInChildren<Animator>();
            if (animator == null) animator = visual.AddComponent<Animator>();
            animator.runtimeAnimatorController = animation;
            animator.applyRootMotion = false;
            EgyptPlayer25D movement = player.AddComponent<EgyptPlayer25D>();
            movement.stage = stage;
            movement.visual = visual.transform;
            movement.animator = animator;
            movement.respawnBelowY = spawn.y - 15;
            EgyptInteractor interactor = player.AddComponent<EgyptInteractor>();
            interactor.stage = stage;
            return player;
        }

        private static Bounds VisualBounds(GameObject visual)
        {
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
            Bounds bounds = new Bounds(visual.transform.position, Vector3.zero);
            if (renderers.Length == 0) return bounds;
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private static EgyptInteractionPoint MakePoint(string name, Vector3 position,
            EgyptPointKind kind, Material material, bool portal)
        {
            GameObject root = new GameObject(name);
            root.transform.position = position;
            EgyptInteractionPoint point = root.AddComponent<EgyptInteractionPoint>();
            point.kind = kind;
            point.radius = 2.5f;
            GameObject visual = GameObject.CreatePrimitive(portal ? PrimitiveType.Cube : PrimitiveType.Capsule);
            visual.name = "Visual_Provisorio_Substituir";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0, portal ? 1.2f : 0.9f, -0.5f);
            visual.transform.localScale = portal ? new Vector3(0.2f, 2.4f, 0.2f) : new Vector3(0.65f, 0.9f, 0.65f);
            visual.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
            return point;
        }

        private static void SetupCamera(Transform player)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject root = new GameObject("Main Camera");
                root.tag = "MainCamera";
                camera = root.AddComponent<Camera>();
                root.AddComponent<AudioListener>();
            }
            camera.orthographic = false;
            camera.fieldOfView = 55;
            camera.nearClipPlane = 0.1f;
            EgyptCamera25D follow = camera.GetComponent<EgyptCamera25D>();
            if (follow == null) follow = camera.gameObject.AddComponent<EgyptCamera25D>();
            follow.target = player;
            camera.transform.position = player.position + follow.offset;
            camera.transform.LookAt(player.position + follow.lookOffset);
        }

        private static void AddInteriorColliders(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>())
                    if (filter.sharedMesh != null && filter.GetComponent<Collider>() == null)
                    {
                        MeshCollider collider = filter.gameObject.AddComponent<MeshCollider>();
                        collider.sharedMesh = filter.sharedMesh;
                    }
        }

        private static Vector3 Ground(Vector3 point, bool interior)
        {
            RaycastHit[] hits = Physics.RaycastAll(point + Vector3.up * 200, Vector3.down, 400,
                ~0, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits.OrderBy(hit => hit.distance))
            {
                if (Vector3.Dot(hit.normal, Vector3.up) < 0.65f) continue;
                bool floor = hit.collider is TerrainCollider;
                if (interior)
                    for (Transform ancestor = hit.collider.transform; ancestor != null; ancestor = ancestor.parent)
                        if (ancestor.name.StartsWith("Tile_") || ancestor.name.StartsWith("Step_")) floor = true;
                if (floor) { point.y = hit.point.y; return point; }
            }
            Debug.LogWarning("Não encontrei piso sob " + point + ". Ajuste esse marcador na cena jogável.");
            return point;
        }

        private static AnimatorController MakeAnimation(string modelPath)
        {
            string controllerPath = Generated + "/Amira_Movimento.controller";
            AnimatorController existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (existing != null) return existing;
            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>()
                .FirstOrDefault(item => !item.name.StartsWith("__preview"));
            if (clip == null) throw new InvalidOperationException("Não encontrei o clip de caminhada no FBX de Amira.");
            AnimationClip walk = UnityEngine.Object.Instantiate(clip);
            walk.name = "Amira_Caminhada_Loop";
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(walk);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(walk, settings);
            AssetDatabase.CreateAsset(walk, Generated + "/Amira_Caminhada_Loop.anim");
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState idle = machine.AddState("Parada_Provisoria");
            idle.motion = walk;
            idle.speed = 0;
            AnimatorState walking = machine.AddState("Caminhada");
            walking.motion = walk;
            machine.defaultState = idle;
            AnimatorStateTransition start = idle.AddTransition(walking);
            start.hasExitTime = false;
            start.duration = 0.1f;
            start.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            AnimatorStateTransition stop = walking.AddTransition(idle);
            stop.hasExitTime = false;
            stop.duration = 0.1f;
            stop.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            return controller;
        }

        private static Material MakeMaterial(string name, Color color)
        {
            string path = Generated + "/" + name + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            RenderPipelineAsset pipeline = GraphicsSettings.defaultRenderPipeline;
            string shaderName = pipeline == null ? "Standard"
                : pipeline.GetType().Name.Contains("HDRender") ? "HDRP/Lit" : "Universal Render Pipeline/Lit";
            Shader shader = Shader.Find(shaderName);
            if (shader == null) throw new InvalidOperationException("Shader de material não encontrado: " + shaderName);
            Material material = new Material(shader) { name = name };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void EnsureFolder(string path)
        {
            string parent = "Assets";
            foreach (string part in path.Substring("Assets/".Length).Split('/'))
            {
                string child = parent + "/" + part;
                if (!AssetDatabase.IsValidFolder(child)) AssetDatabase.CreateFolder(parent, part);
                parent = child;
            }
        }

        private static void RegisterScenes()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            foreach (string path in new[] { Exterior, Interior })
            {
                EditorBuildSettingsScene existing = scenes.FirstOrDefault(scene => scene.path == path);
                if (existing == null) scenes.Add(new EditorBuildSettingsScene(path, true));
                else existing.enabled = true;
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void ValidateScene(string path, EgyptPointKind first, EgyptPointKind second)
        {
            Scene scene = EditorSceneManager.OpenScene(path);
            GameObject player = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<EgyptPlayer25D>())
                .Select(component => component.gameObject).Single();
            EgyptPlayer25D movement = player.GetComponent<EgyptPlayer25D>();
            EgyptInteractor interactor = player.GetComponent<EgyptInteractor>();
            if (movement.stage == null || movement.visual == null || movement.animator == null
                || interactor.stage != movement.stage || player.GetComponent<CharacterController>() == null)
                throw new InvalidOperationException("Referências incompletas na personagem: " + path);
            if (interactor.points.Length != 2 || !interactor.points.Any(point => point.kind == first)
                || !interactor.points.Any(point => point.kind == second))
                throw new InvalidOperationException("Pontos de interação inválidos: " + path);
            EgyptCamera25D camera = Camera.main.GetComponent<EgyptCamera25D>();
            if (camera.target != player.transform) throw new InvalidOperationException("Câmera sem alvo: " + path);
            EgyptModelIntegration.Validate(scene);
            ValidateWalkingRoute(player, movement, interactor.points, path);
        }

        private static void ValidateWalkingRoute(GameObject player, EgyptPlayer25D movement,
            EgyptInteractionPoint[] points, string path)
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            Vector3 spawn = player.transform.position;
            Vector3 axis = movement.movementAxis.normalized;
            try
            {
                foreach (EgyptInteractionPoint point in points)
                {
                    controller.enabled = false;
                    player.transform.position = spawn;
                    controller.enabled = true;
                    Physics.SyncTransforms();
                    float vertical = 0;
                    const float dt = 0.05f;
                    for (int step = 0; step < 500 && Vector3.Distance(player.transform.position, point.Position) > point.radius - 0.1f; step++)
                    {
                        float along = Vector3.Dot(point.Position - player.transform.position, axis);
                        float horizontal = Mathf.Abs(along) > 0.15f ? Mathf.Sign(along) : 0;
                        if (controller.isGrounded && vertical < 0) vertical = -2;
                        vertical += movement.gravity * dt;
                        controller.Move((axis * horizontal * movement.speed + Vector3.up * vertical) * dt);
                        Vector3 planeNormal = Vector3.Cross(Vector3.up, axis).normalized;
                        Vector3 position = player.transform.position;
                        position -= planeNormal * Vector3.Dot(position - spawn, planeNormal);
                        player.transform.position = position;
                        Physics.SyncTransforms();
                    }
                    Require(Vector3.Distance(player.transform.position, point.Position) <= point.radius,
                        "O percurso até " + point.name + " não foi alcançado andando em " + path
                        + ". Posição final: " + player.transform.position + ", destino: " + point.Position);
                    Debug.Log("TCC_EGITO_ROUTE_OK: " + path + " -> " + point.name);
                }
            }
            finally
            {
                controller.enabled = false;
                player.transform.position = spawn;
                controller.enabled = true;
                Physics.SyncTransforms();
            }
        }

        private static void ValidateDialogueFlow()
        {
            GameObject testObject = new GameObject("Validacao_Temporaria_Egito");
            try
            {
                EgyptStage stage = testObject.AddComponent<EgyptStage>();
                EgyptStage.Progress.Reset();
                int guideEvents = 0;
                int rewardEvents = 0;
                int completionEvents = 0;
                stage.onGuideFinished.AddListener(() => guideEvents++);
                stage.onScarabReceived.AddListener(() => rewardEvents++);
                stage.onStageCompleted.AddListener(() => completionEvents++);

                stage.Interact(EgyptPointKind.Mummy);
                Require(!stage.DialogueIsOpen && !EgyptStage.Progress.HasScarab, "Múmia não deve conceder item antes do guia e da entrada.");
                stage.Interact(EgyptPointKind.Guide);
                Require(stage.DialogueIsOpen && stage.BlocksMovement, "Diálogo deve abrir e bloquear o controle.");
                for (int i = 0; i < stage.guideLines.Length - 1; i++) stage.AdvanceDialogue();
                Require(EgyptStage.Progress.Step == EgyptQuestStep.MeetGuide, "Objetivo não deve avançar antes da última fala.");
                stage.AdvanceDialogue();
                Require(EgyptStage.Progress.Step == EgyptQuestStep.EnterPyramid && guideEvents == 1, "Guia deve avançar a missão uma vez.");

                stage.interiorScene = "Assets/TCCEgito/Scenes/CenaInexistente.unity";
                stage.Interact(EgyptPointKind.Entrance);
                Require(!stage.IsLoading && EgyptStage.Progress.Step == EgyptQuestStep.EnterPyramid,
                    "Destino ausente não deve bloquear o jogo nem avançar a missão.");
                stage.Interact(EgyptPointKind.Guide);
                stage.AdvanceDialogue();
                Require(guideEvents == 1, "Revisitar o guia não deve repetir o evento.");

                Require(EgyptStage.Progress.EnterPyramid(), "A entrada deve ser permitida após o guia.");
                stage.Interact(EgyptPointKind.Mummy);
                for (int i = 0; i < stage.mummyLines.Length - 1; i++) stage.AdvanceDialogue();
                Require(!EgyptStage.Progress.HasScarab && rewardEvents == 0,
                    "Recompensa não deve ser entregue antes da última fala da múmia.");
                stage.AdvanceDialogue();
                Require(EgyptStage.Progress.HasScarab && rewardEvents == 1 && completionEvents == 1,
                    "Item e conclusão devem ser concedidos exatamente uma vez.");
                Require(stage.CompletionIsOpen && stage.BlocksMovement, "Conclusão deve abrir sua tela.");
                stage.CloseCompletion();
                Require(!stage.BlocksMovement, "Controle deve retornar após fechar a conclusão.");
                stage.Interact(EgyptPointKind.Mummy);
                stage.AdvanceDialogue();
                Require(rewardEvents == 1 && completionEvents == 1, "Revisitar a múmia não deve duplicar a recompensa.");

                EgyptStage.Progress.Reset();
                stage.guideLines = Array.Empty<EgyptLine>();
                stage.Interact(EgyptPointKind.Guide);
                Require(!stage.DialogueIsOpen && EgyptStage.Progress.Step == EgyptQuestStep.MeetGuide,
                    "Um diálogo apagado no Inspector não deve avançar a missão.");
                Debug.Log("TCC_EGITO_DIALOGUE_TESTS_OK: ordem da missão, bloqueio, falas, destino ausente e recompensa única.");
            }
            finally
            {
                EgyptStage.Progress.Reset();
                UnityEngine.Object.DestroyImmediate(testObject);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Validação da missão: " + message);
        }
    }
}
