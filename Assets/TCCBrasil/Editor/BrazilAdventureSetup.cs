using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using CrossingTheWorld.TCCEgypt;
using Object = UnityEngine.Object;

namespace CrossingTheWorld.TCCBrazil.Editor
{
    public static class BrazilAdventureSetup
    {
        private const string Generated = "Assets/TCCBrasil/Generated/";
        private const float Ground = 77.13f;

        [MenuItem("TCC/Brasil/Preparar fase completa")]
        public static void Build()
        {
            if (Application.unityVersion != "6000.5.7f1") throw new InvalidOperationException("Abra com Unity 6000.5.7f1.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(BrazilSceneSetup.ScenePath);
            var terrain = Object.FindAnyObjectByType<Terrain>();
            RepairGround(terrain);
            RestoreCart(scene);
            ResizeProps(terrain);
            var old = GameObject.Find("TCC_Brasil_Aventura");
            if (old != null) Object.DestroyImmediate(old);
            foreach (var hud in Object.FindObjectsByType<BrazilExplorationHUD>(FindObjectsSortMode.None)) Object.DestroyImmediate(hud);
            var root = new GameObject("TCC_Brasil_Aventura");
            var stage = root.AddComponent<BrazilStage>();
            root.AddComponent<BrazilStageUI>();
            stage.rewardDisplay = root.AddComponent<BrazilRewardDisplay>();
            stage.player = Object.FindAnyObjectByType<BrazilPlayer25D>();
            stage.player.stage = stage;
            stage.player.transform.position = new Vector3(369, Ground + .05f, 535);
            stage.player.respawnBelowY = Ground - 12;
            stage.followCamera = Object.FindAnyObjectByType<EgyptCamera25D>();
            stage.followCamera.offset = new Vector3(8.5f, 3.1f, 0);
            stage.followCamera.lookOffset = new Vector3(0, 1.1f, 0);
            stage.followCamera.GetComponent<Camera>().fieldOfView = 58;
            stage.followCamera.GetComponent<Camera>().cullingMask &= ~(1 << 30);
            stage.followCamera.transform.position = stage.player.transform.position + stage.followCamera.offset;
            stage.followCamera.transform.LookAt(stage.player.transform.position + stage.followCamera.lookOffset);
            var bounds = Object.FindAnyObjectByType<EgyptMovementBounds>();
            bounds.transform.position = new Vector3(369, Ground, 550);
            bounds.UpdateColliders();
            stage.guide = MakeGuide(scene, root.transform);
            stage.resident = MakeResident(root.transform);
            stage.challengeAnchor = new GameObject("Posicao_Do_Desafio").transform;
            stage.challengeAnchor.SetParent(root.transform); stage.challengeAnchor.position = new Vector3(369, Ground + .05f, 559);
            stage.ball = MakeBall(root.transform);
            stage.ball.position = stage.challengeAnchor.position + new Vector3(.12f, .08f, .55f);
            stage.rewardDisplay.previewPrefab = MakePrize();
            var oldPose = stage.player.GetComponent<BrazilKickPose>();
            if (oldPose != null) Object.DestroyImmediate(oldPose);
            stage.kickPose = stage.player.gameObject.AddComponent<BrazilKickPose>();
            stage.kickPose.stage = stage; stage.kickPose.animator = stage.player.animator; stage.kickPose.visual = stage.player.visual;
            stage.kickPose.thigh = stage.player.visual.GetComponentsInChildren<Transform>().Single(t => t.name == "coxa.r");
            stage.kickPose.shin = stage.player.visual.GetComponentsInChildren<Transform>().Single(t => t.name == "perna.r");
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            ValidateScene();
            Debug.Log("BRAZIL_ADVENTURE_BUILD_OK");
        }

        private static void RepairGround(Terrain terrain)
        {
            var data = terrain.terrainData;
            const string original = Generated + "Praia_Alturas_Originais.asset";
            if (AssetDatabase.LoadAssetAtPath<TerrainData>(original) == null)
                AssetDatabase.CreateAsset(Object.Instantiate(data), original);
            var source = AssetDatabase.LoadAssetAtPath<TerrainData>(original);
            int resolution = data.heightmapResolution;
            float[,] heights = source.GetHeights(0, 0, resolution, resolution);
            // Nivelo a faixa de caminhada e suavizo a ligação com a praia ao redor.
            for (int z = 0; z < resolution; z++) for (int x = 0; x < resolution; x++)
            {
                float wx = terrain.transform.position.x + x * data.size.x / (resolution - 1);
                float wz = terrain.transform.position.z + z * data.size.z / (resolution - 1);
                float distance = Mathf.Max(Mathf.Max(356 - wx, wx - 382), Mathf.Max(516 - wz, wz - 584));
                float blend = 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01(distance / 16));
                heights[z, x] = Mathf.Lerp(heights[z, x], (Ground - terrain.transform.position.y) / data.size.y, blend);
            }
            data.SetHeights(0, 0, heights);
            var shader = Shader.Find("Universal Render Pipeline/Terrain/Lit");
            if (shader == null) throw new InvalidOperationException("Shader de terreno URP não encontrado.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(Generated + "Areia_Terreno_URP.mat");
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, Generated + "Areia_Terreno_URP.mat"); }
            material.shader = shader; terrain.materialTemplate = material;
            foreach (var layer in data.terrainLayers) { layer.tileSize = new Vector2(5, 5); layer.smoothness = 0; layer.metallic = 0; EditorUtility.SetDirty(layer); }
            terrain.GetComponent<TerrainCollider>().terrainData = data;
            EditorUtility.SetDirty(data); EditorUtility.SetDirty(material);
        }

        private static float TargetSize(string name, out bool height)
        {
            height = false;
            if (name.StartsWith("IceCream_Wheel")) return 0;
            if (name.StartsWith("Lifeguard")) { height = true; return 5.2f; }
            if (name.StartsWith("Umbrella")) return 2.8f;
            if (name.StartsWith("DeckChair")) return 1.85f;
            if (name.StartsWith("IceCream")) { height = true; return 2.1f; }
            if (name.StartsWith("Longboard")) return 1.8f;
            if (name.StartsWith("Towel")) return 1.8f;
            if (name.StartsWith("Slap_Sandal")) return .28f;
            if (name.StartsWith("Cream")) { height = true; return .18f; }
            if (name.StartsWith("Duck")) { height = true; return .55f; }
            return 0;
        }

        private static void RestoreCart(Scene brazil)
        {
            // As rodas pertencem ao carrinho e precisam receber a mesma escala que a carroceria.
            foreach (var part in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.name.StartsWith("IceCream")).ToArray())
                if (part != null) Object.DestroyImmediate(part.gameObject);
            var sourceScene = EditorSceneManager.OpenScene(BrazilSceneSetup.SourcePath,OpenSceneMode.Additive);
            var source = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Single(t => t.gameObject.scene == sourceScene && t.name == "IceCream");
            var cart = Object.Instantiate(source.gameObject); cart.name = "IceCream";
            cart.transform.SetPositionAndRotation(source.position,source.rotation);
            cart.transform.localScale = source.lossyScale;
            SceneManager.MoveGameObjectToScene(cart,brazil);
            EditorSceneManager.CloseScene(sourceScene,true); SceneManager.SetActiveScene(brazil);
        }

        private static void ResizeProps(Terrain terrain)
        {
            var props = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                .Where(t => TargetSize(t.name, out _) > 0 && t.GetComponentsInChildren<Renderer>().Length > 0)
                .OrderByDescending(t => Depth(t)).ToArray();
            var group = GameObject.Find("Objetos_Praia_Ajustados") ?? new GameObject("Objetos_Praia_Ajustados");
            // Alguns objetos estavam dentro do guarda-sol. Separo os grupos antes de redimensionar.
            foreach (var t in props)
            {
                if (PrefabUtility.IsPartOfPrefabInstance(t)) PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(t), PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                t.SetParent(group.transform, true);
            }
            foreach (var t in props)
            {
                var bounds = BoundsOf(t.gameObject);
                float target = TargetSize(t.name, out bool height);
                float dimension = height ? bounds.size.y : Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
                if (dimension > .001f) t.localScale *= target / dimension;
                bounds = BoundsOf(t.gameObject);
                // Deixo livre o caminho entre o guia e a área da bola.
                if (bounds.center.x > 363 && bounds.center.x < 374 && bounds.center.z > 525 && bounds.center.z < 575)
                {
                    float destination = bounds.center.x < 369 ? 360 : 377;
                    t.position += Vector3.right * (destination - bounds.center.x);
                }
                bounds = BoundsOf(t.gameObject);
                t.position += Vector3.up * (terrain.SampleHeight(bounds.center) + terrain.transform.position.y - bounds.min.y + .015f);
            }
        }
        private static int Depth(Transform t) { int depth = 0; while (t.parent != null) { depth++; t = t.parent; } return depth; }
        private static Bounds BoundsOf(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new InvalidOperationException("Modelo sem renderizador: " + go.name);
            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            return bounds;
        }

        private static Transform MakeGuide(Scene brazil, Transform parent)
        {
            var egypt = EditorSceneManager.OpenScene("Assets/TCCEgito/Scenes/Egito_Exterior_Jogavel.unity", OpenSceneMode.Additive);
            var source = Object.FindObjectsByType<EgyptInteractionPoint>(FindObjectsSortMode.None).Single(p => p.gameObject.scene == egypt && p.kind == EgyptPointKind.Guide);
            var guide = Object.Instantiate(source.gameObject);
            guide.name = "Guia_Horizon_Travel";
            SceneManager.MoveGameObjectToScene(guide, brazil);
            foreach (var point in guide.GetComponentsInChildren<EgyptInteractionPoint>()) Object.DestroyImmediate(point);
            foreach (var idle in guide.GetComponentsInChildren<EgyptNpcIdle>()) idle.stage = null;
            guide.transform.SetParent(parent, true);
            var b = BoundsOf(guide); guide.transform.localScale *= 1.8f / b.size.y;
            guide.transform.position = new Vector3(367, Ground, 533);
            guide.transform.rotation = Quaternion.Euler(0, 90, 0);
            b = BoundsOf(guide); guide.transform.position += Vector3.up * (Ground - b.min.y);
            AddPersonCollider(guide);
            EditorSceneManager.CloseScene(egypt, true); SceneManager.SetActiveScene(brazil);
            return guide.transform;
        }

        private static Material Material(string name, Color color, Texture texture = null, bool unlit = false)
        {
            string path = Generated + name + ".mat";
            var result = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (result == null) { result = new Material(Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(result, path); }
            result.SetColor("_BaseColor", color);
            if (texture != null) result.SetTexture("_BaseMap", texture);
            if (result.HasProperty("_Smoothness")) result.SetFloat("_Smoothness", .15f);
            EditorUtility.SetDirty(result); return result;
        }

        private static Transform MakeResident(Transform parent)
        {
            return BrazilResidentSetup.Create(parent, new Vector3(367, Ground, 562), Quaternion.Euler(0,90,0));
        }
        private static void AddPersonCollider(GameObject person)
        {
            var collider = person.GetComponent<CapsuleCollider>();
            if (collider == null) collider = person.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0,.9f,0); collider.height = 1.8f; collider.radius = .24f;
        }
        private static Transform MakeBall(Transform parent)
        {
            var root = new GameObject("Bola_Do_Desafio").transform; root.SetParent(parent,false);
            var model = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TCCBrasil/Art/Bola/Bola.fbx"),root);
            var b = BoundsOf(model); model.transform.localScale *= .26f / b.size.x;
            b = BoundsOf(model); model.transform.position -= b.center;
            // O FBX veio sem textura. Divido os gomos entre preto e branco no modelo de exibição.
            foreach(var filter in model.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = Object.Instantiate(filter.sharedMesh);
                var vertices = mesh.vertices; var triangles = mesh.triangles;
                var white = new List<int>(); var black = new List<int>();
                float phi = (1 + Mathf.Sqrt(5)) / 2;
                var directions = new List<Vector3>();
                foreach(float a in new[]{-1f,1f}) foreach(float signB in new[]{-1f,1f})
                {
                    directions.Add(new Vector3(0,a,signB*phi).normalized);
                    directions.Add(new Vector3(a,signB*phi,0).normalized);
                    directions.Add(new Vector3(signB*phi,0,a).normalized);
                }
                for(int i=0;i<triangles.Length;i+=3)
                {
                    Vector3 normal = ((vertices[triangles[i]]+vertices[triangles[i+1]]+vertices[triangles[i+2]])/3 - mesh.bounds.center).normalized;
                    var target = directions.Any(d => Vector3.Dot(d,normal) > .94f) ? black : white;
                    target.Add(triangles[i]); target.Add(triangles[i+1]); target.Add(triangles[i+2]);
                }
                mesh.subMeshCount = 2; mesh.SetTriangles(white,0); mesh.SetTriangles(black,1);
                string path = Generated + "Bola_Gomos.asset";
                var saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(saved == null) { AssetDatabase.CreateAsset(mesh,path); saved=mesh; }
                else { EditorUtility.CopySerialized(mesh,saved); Object.DestroyImmediate(mesh); EditorUtility.SetDirty(saved); }
                filter.sharedMesh = saved;
                filter.GetComponent<Renderer>().sharedMaterials = new[]{Material("Bola_Branco",new Color(.95f,.95f,.92f)),Material("Bola_Preto",new Color(.025f,.03f,.035f))};
            }
            return root;
        }
        private static GameObject MakePrize()
        {
            var root = new GameObject("Berimbau_Premio");
            var model = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TCCBrasil/Art/Berimbau/Berimbau.fbx"),root.transform);
            var b = BoundsOf(model); model.transform.localScale *= 2.3f / b.size.y;
            b = BoundsOf(model); model.transform.position -= b.center;
            var material = Material("Berimbau_Premio", Color.white, AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TCCBrasil/Art/Berimbau/Berimbau.png"),true);
            foreach (var r in model.GetComponentsInChildren<Renderer>()) r.sharedMaterials = Enumerable.Repeat(material,r.sharedMaterials.Length).ToArray();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root,Generated + "Berimbau_Premio.prefab");
            Object.DestroyImmediate(root); return prefab;
        }

        public static void ValidateScene()
        {
            var stage = Object.FindAnyObjectByType<BrazilStage>();
            if (stage == null || stage.guide == null || stage.resident == null || stage.ball == null || stage.rewardDisplay.previewPrefab == null || stage.kickPose.thigh == null) throw new InvalidOperationException("Referências incompletas da fase.");
            var terrain = Object.FindAnyObjectByType<Terrain>();
            if (terrain.materialTemplate.shader.name != "Universal Render Pipeline/Terrain/Lit") throw new InvalidOperationException("Material incorreto no chão.");
            var cart = GameObject.Find("IceCream");
            var cartBounds = BoundsOf(cart);
            if(cartBounds.center.x < 320 || cartBounds.center.x > 405 || cartBounds.center.z < 510 || cartBounds.center.z > 625 || Mathf.Abs(cartBounds.size.y - 2.1f) > .05f)
                throw new InvalidOperationException("Carrinho fora da praia ou com escala incorreta.");
            foreach (float x in new[]{364f,369f,372f}) foreach(float z in new[]{526f,535f,559f,574f})
                if (Mathf.Abs(terrain.SampleHeight(new Vector3(x,0,z))-Ground) > .04f) throw new InvalidOperationException("Chão desnivelado na faixa jogável.");
            foreach (var script in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(script.gameObject) > 0) throw new InvalidOperationException("Script ausente: " + script.name);
        }

        [MenuItem("TCC/Brasil/Jogar fase completa")]
        public static void Play()
        {
            BrazilPlayPreview.OpenForUser();
        }
    }
}
