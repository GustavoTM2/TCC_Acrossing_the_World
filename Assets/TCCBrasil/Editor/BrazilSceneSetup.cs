using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using CrossingTheWorld.TCCEgypt;

namespace CrossingTheWorld.TCCBrazil.Editor
{
    public static class BrazilSceneSetup
    {
        public const string ScenePath = "Assets/TCCBrasil/Scenes/Brasil_Praia_Jogavel.unity";
        public const string SourcePath = "Assets/Scenes/brasil.unity";
        public const string Art = "Assets/TCCBrasil/Art/Lucas/";
        public const string Generated = "Assets/TCCBrasil/Generated/";

        public static void BuildForValidation()
        {
            try
            {
                if (Application.unityVersion != "6000.5.7f1") throw new InvalidOperationException("Use Unity 6000.5.7f1.");
                Folder("Assets/TCCBrasil/Scenes");
                Folder(Generated.TrimEnd('/'));
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                {
                    EditorSceneManager.OpenScene(ScenePath);
                    Debug.Log("BRAZIL_SCENE_PRESERVED: a cena jogável existente não foi remontada.");
                    BrazilValidation.Validate();
                    return;
                }
                if (!AssetDatabase.CopyAsset(SourcePath,ScenePath)) throw new InvalidOperationException("Não consegui copiar a praia original.");
                var scene = EditorSceneManager.OpenScene(ScenePath);
                Terrain terrain = UnityEngine.Object.FindAnyObjectByType<Terrain>();
                if (terrain == null) throw new InvalidOperationException("Não encontrei o terreno da praia.");
                RepairSand(terrain);
                var setup = new GameObject("TCC_Brasil_Exploracao");
                setup.AddComponent<BrazilExplorationHUD>();
                Vector3 spawn = new Vector3(369,0,535);
                spawn.y = terrain.SampleHeight(spawn) + terrain.transform.position.y + 0.05f;
                GameObject lucas = CreateLucas(spawn,setup.transform);
                var player = lucas.GetComponent<BrazilPlayer25D>();
                player.horizontalAxis = Vector3.forward;
                player.depthAxis = Vector3.left;
                player.respawnBelowY = spawn.y - 15;
                player.visual.rotation = Quaternion.LookRotation(player.horizontalAxis);
                var limitsRoot = new GameObject("Limites_Invisiveis_Brasil");
                limitsRoot.transform.SetParent(setup.transform,true);
                limitsRoot.transform.position = new Vector3(369,spawn.y,550);
                limitsRoot.transform.rotation = Quaternion.LookRotation(player.depthAxis,Vector3.up);
                var bounds = limitsRoot.AddComponent<EgyptMovementBounds>();
                bounds.horizontalMin = -25;
                bounds.horizontalMax = 25;
                bounds.frontDistance = 3;
                bounds.backDistance = 5;
                bounds.UpdateColliders();
                AddObstacleColliders(scene.GetRootGameObjects());
                foreach (Camera old in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                {
                    old.enabled = false;
                    old.tag = "Untagged";
                    foreach (AudioListener listener in old.GetComponents<AudioListener>()) listener.enabled = false;
                }
                GameObject cameraRoot = new GameObject("Camera_Lucas_25D");
                cameraRoot.transform.SetParent(setup.transform,false);
                cameraRoot.tag = "MainCamera";
                Camera camera = cameraRoot.AddComponent<Camera>();
                camera.fieldOfView = 65;
                camera.nearClipPlane = 0.15f;
                camera.farClipPlane = 2500;
                cameraRoot.AddComponent<AudioListener>();
                var follow = cameraRoot.AddComponent<EgyptCamera25D>();
                follow.target = lucas.transform;
                follow.offset = new Vector3(10,3.6f,0);
                follow.lookOffset = new Vector3(0,1,0);
                follow.depthAxis = player.depthAxis;
                follow.depthFollow = 0.2f;
                camera.transform.position = spawn + follow.offset;
                camera.transform.LookAt(spawn + follow.lookOffset);
                RenderSettings.ambientMode = AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = new Color(0.68f,0.75f,0.83f);
                RenderSettings.ambientEquatorColor = new Color(0.5f,0.55f,0.6f);
                RenderSettings.ambientGroundColor = new Color(0.3f,0.28f,0.24f);
                RenderSettings.ambientIntensity = 1;
                foreach (Light light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                    if (light.type == LightType.Directional) light.intensity = light == RenderSettings.sun ? 1.15f : 0.25f;
                Physics.SyncTransforms();
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Não consegui salvar a praia jogável.");
                var scenes = EditorBuildSettings.scenes.ToList();
                if (!scenes.Any(s => s.path == ScenePath)) scenes.Add(new EditorBuildSettingsScene(ScenePath,true));
                EditorBuildSettings.scenes = scenes.ToArray();
                AssetDatabase.SaveAssets();
                Debug.Log("BRAZIL_SCENE_BUILT spawn=" + spawn + " area: Z 525..575, X 364..372");
                BrazilValidation.Validate();
                Debug.Log("BRAZIL_BUILD_VALIDATION_OK");
            }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        private static void RepairSand(Terrain terrain)
        {
            string dataPath = Generated + "Praia_Terreno.asset";
            if (AssetDatabase.LoadAssetAtPath<TerrainData>(dataPath) != null) throw new InvalidOperationException("Já existe terreno gerado. Preserve as alterações antes de remontar.");
            TerrainData data = UnityEngine.Object.Instantiate(terrain.terrainData);
            data.name = "Praia_Terreno";
            AssetDatabase.CreateAsset(data,dataPath);
            var layers = data.terrainLayers;
            Texture2D sand = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Beach - resort/Models/Textures/sand_AlbedoTransparency.png");
            if (sand == null) throw new InvalidOperationException("Falta a textura de areia do pacote original.");
            for (int i=0;i<layers.Length;i++)
            {
                if (layers[i] != null && layers[i].diffuseTexture != null) continue;
                var layer = layers[i] == null ? new TerrainLayer() : UnityEngine.Object.Instantiate(layers[i]);
                layer.name = "Areia_Praia";
                layer.diffuseTexture = sand;
                layer.tileSize = new Vector2(8,8);
                AssetDatabase.CreateAsset(layer,Generated + "Areia_Praia_" + i + ".terrainlayer");
                layers[i] = layer;
            }
            data.terrainLayers = layers;
            terrain.terrainData = data;
            terrain.GetComponent<TerrainCollider>().terrainData = data;
            EditorUtility.SetDirty(data);
        }

        private static void AddObstacleColliders(GameObject[] roots)
        {
            foreach (GameObject root in roots)
                foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (root.name == "TCC_Brasil_Exploracao" || root.name == "Water Surface" || filter.sharedMesh == null) continue;
                    Renderer renderer = filter.GetComponent<Renderer>();
                    if (renderer == null || !renderer.enabled || filter.GetComponent<Collider>() != null) continue;
                    // Evita colisão com a superfície da água e objetos distantes da faixa de exploração.
                    Bounds b = renderer.bounds;
                    if (b.max.x < 363 || b.min.x > 373 || b.max.z < 524 || b.min.z > 576) continue;
                    MeshCollider collider = filter.gameObject.AddComponent<MeshCollider>();
                    collider.sharedMesh = filter.sharedMesh;
                }
        }

        [MenuItem("TCC/Brasil/Abrir praia jogavel")]
        public static void Open()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                Debug.LogWarning("A cena Brasil_Praia_Jogavel precisa estar instalada junto com o projeto.");
                return;
            }
            EditorSceneManager.OpenScene(ScenePath);
        }

        internal static void Folder(string path)
        {
            string parent = "Assets";
            foreach (string part in path.Substring(7).Split('/'))
            {
                if (!AssetDatabase.IsValidFolder(parent + "/" + part)) AssetDatabase.CreateFolder(parent,part);
                parent += "/" + part;
            }
        }

        internal static GameObject CreateLucas(Vector3 position, Transform parent)
        {
            Folder(Generated.TrimEnd('/'));
            string modelPath = Art + "Lucas_Caminhada.fbx";
            ModelImporter importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null) throw new InvalidOperationException("Falta o modelo animado do Lucas.");
            if (!importer.importAnimation || importer.animationType != ModelImporterAnimationType.Generic || !importer.isReadable)
            {
                importer.importAnimation = true;
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Art + "Lucas.png");
            if (model == null || texture == null) throw new InvalidOperationException("Faltam o modelo ou a textura do Lucas.");
            var root = new GameObject("Lucas_Jogador");
            root.transform.SetParent(parent,true);
            root.transform.position = position;
            root.tag = "Player";
            var capsule = root.AddComponent<CharacterController>();
            capsule.height = 1.8f;
            capsule.radius = 0.28f;
            capsule.center = new Vector3(0,0.9f,0);
            capsule.skinWidth = 0.025f;
            capsule.stepOffset = 0.25f;
            capsule.slopeLimit = 50;
            var player = root.AddComponent<BrazilPlayer25D>();
            GameObject pivot = new GameObject("Visual_Lucas");
            pivot.transform.SetParent(root.transform,false);
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
            visual.name = "Lucas_Caminhada_Modelo";
            visual.transform.SetParent(pivot.transform,false);
            foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            foreach (Camera camera in visual.GetComponentsInChildren<Camera>(true)) camera.enabled = false;
            foreach (Light light in visual.GetComponentsInChildren<Light>(true)) light.enabled = false;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(Generated + "Lucas_Texturizado.mat");
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit")) {name="Lucas_Texturizado"};
                material.SetTexture("_BaseMap", texture);
                material.SetColor("_BaseColor", Color.white);
                material.SetFloat("_Smoothness",0.15f);
                AssetDatabase.CreateAsset(material,Generated + "Lucas_Texturizado.mat");
            }
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers) r.sharedMaterials = Enumerable.Repeat(material,r.sharedMaterials.Length).ToArray();
            Bounds bounds = BoundsOf(visual);
            visual.transform.localScale *= 1.8f / bounds.size.y;
            bounds = BoundsOf(visual);
            // Alinha os pés ao controlador e compensa sua margem física sem mexer na colisão.
            visual.transform.position += new Vector3(position.x - bounds.center.x,
                position.y - bounds.min.y - 0.025f, position.z - bounds.center.z);
            Animator animator = visual.GetComponentInChildren<Animator>();
            if (animator == null) animator = visual.AddComponent<Animator>();
            animator.runtimeAnimatorController = MakeAnimator(modelPath);
            animator.applyRootMotion = false;
            player.visual = pivot.transform;
            player.animator = animator;
            return root;
        }

        internal static Bounds BoundsOf(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new InvalidOperationException("Lucas não tem geometria.");
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers.Skip(1)) bounds.Encapsulate(r.bounds);
            return bounds;
        }

        private static AnimatorController MakeAnimator(string modelPath)
        {
            string path = Generated + "Lucas_Movimento.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller != null) return controller;
            AnimationClip source = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>()
                .FirstOrDefault(c => !c.name.StartsWith("__preview"));
            if (source == null || source.length <= 0) throw new InvalidOperationException("Não encontrei a caminhada de Lucas no FBX.");
            AnimationClip walk = UnityEngine.Object.Instantiate(source);
            walk.name = "Lucas_Caminhada_Loop";
            var settings = AnimationUtility.GetAnimationClipSettings(walk);
            settings.loopTime = true;
            settings.keepOriginalPositionXZ = true;
            settings.keepOriginalPositionY = true;
            AnimationUtility.SetAnimationClipSettings(walk,settings);
            AssetDatabase.CreateAsset(walk,Generated + "Lucas_Caminhada_Loop.anim");
            controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Speed",AnimatorControllerParameterType.Float);
            var machine = controller.layers[0].stateMachine;
            var idle = machine.AddState("Parado");
            idle.motion = walk;
            idle.speed = 0;
            var walking = machine.AddState("Caminhada");
            walking.motion = walk;
            machine.defaultState = idle;
            var start = idle.AddTransition(walking);
            start.hasExitTime = false;
            start.duration = 0.12f;
            start.AddCondition(AnimatorConditionMode.Greater,0.05f,"Speed");
            var stop = walking.AddTransition(idle);
            stop.hasExitTime = false;
            stop.duration = 0.12f;
            stop.AddCondition(AnimatorConditionMode.Less,0.05f,"Speed");
            return controller;
        }
    }
}
