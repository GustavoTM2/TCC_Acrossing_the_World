using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CrossingTheWorld.TCCEgypt;
using Object = UnityEngine.Object;

namespace CrossingTheWorld.TCCBrazil.Editor
{
    // Organizo a caminhada na diagonal da praia, com a montanha atrás do desafio.
    public static class BrazilBeachFraming
    {
        private const float Ground = 77.13f;
        private static readonly Vector3 Center = new Vector3(375, Ground, 559);
        private static readonly Vector3 Right = new Vector3(10.7f, 0, 10).normalized;
        private static readonly Vector3 Depth = Vector3.Cross(Right, Vector3.up);

        [MenuItem("TCC/Brasil/Ajustar visão da praia e Corcovado")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(BrazilSceneSetup.ScenePath);
            Configure();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            BrazilAdventureSetup.ValidateScene();
            Debug.Log("BRAZIL_BEACH_FRAMING_OK");
        }

        public static void Configure()
        {
            var terrain = BeachTerrain();
            LevelBeach(terrain);
            // Evito simplificar o chão perto dos pés: a superfície exibida deve coincidir com a colisão.
            terrain.heightmapMaximumLOD = 0;
            terrain.heightmapPixelError = 1;
            var stage = Object.FindFirstObjectByType<BrazilStage>();
            var player = stage.player;
            player.transform.position = Center - Right * 15 + Vector3.up * .05f;
            player.horizontalAxis = Right;
            player.depthAxis = Depth;
            player.visual.rotation = Quaternion.LookRotation(Right) * Quaternion.Euler(0, player.visualYawOffset, 0);
            stage.challengeAnchor.SetPositionAndRotation(Center + Right * 9 + Vector3.up * .05f, Quaternion.LookRotation(Right));
            PlacePerson(stage.guide, Center - Right * 15.8f + Depth * 1.9f);
            PlacePerson(stage.resident, Center + Right * 11 + Depth * 1.1f);
            stage.ball.position = stage.BallRestPosition;

            // As paredes e as teclas acompanham a mesma direção vista na tela.
            var bounds = Object.FindFirstObjectByType<EgyptMovementBounds>();
            bounds.transform.SetPositionAndRotation(Center, Quaternion.LookRotation(Depth));
            bounds.horizontalMin = -23; bounds.horizontalMax = 23;
            bounds.frontDistance = 3.5f; bounds.backDistance = 5;
            bounds.UpdateColliders();
            ArrangeProps(terrain, bounds);

            var follow = stage.followCamera;
            follow.target = player.transform;
            follow.offset = new Vector3(7.5f, 3.1625f, -8.025f);
            follow.lookOffset = new Vector3(0, 1.25f, 0);
            follow.depthAxis = Depth; follow.depthFollow = .2f;
            follow.useWallRail = false; follow.fixedHorizontalFraming = false; follow.clampFramingZ = false;
            follow.GetComponent<Camera>().fieldOfView = 60;
            follow.transform.position = player.transform.position + follow.offset;
            follow.transform.LookAt(player.transform.position + follow.lookOffset);
            Physics.SyncTransforms();
        }

        internal static Terrain BeachTerrain() => Terrain.activeTerrains.Single(t =>
            AssetDatabase.GetAssetPath(t.terrainData) == "Assets/TCCBrasil/Generated/Praia_Terreno.asset");

        private static void PlacePerson(Transform person, Vector3 position)
        {
            person.SetPositionAndRotation(position, Quaternion.LookRotation(-Depth));
            var renderers = person.GetComponentsInChildren<Renderer>();
            float bottom = renderers.Min(r => r.bounds.min.y);
            person.position += Vector3.up * (Ground - bottom);
        }

        private static void LevelBeach(Terrain terrain)
        {
            var data = terrain.terrainData;
            int resolution = data.heightmapResolution;
            var heights = data.GetHeights(0, 0, resolution, resolution);
            // Nivelo a areia alta em toda a praia. O fundo do mar e o Terrain separado da montanha são preservados.
            for (int z = 0; z < resolution; z++) for (int x = 0; x < resolution; x++)
            {
                var position = terrain.transform.position + new Vector3(x * data.size.x / (resolution - 1), 0, z * data.size.z / (resolution - 1));
                var delta = position - Center;
                float side = Mathf.Abs(Vector3.Dot(delta, Right)) - 27;
                float depth = Mathf.Abs(Vector3.Dot(delta, Depth)) - 10;
                float blend = 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01(Mathf.Max(side, depth) / 14));
                float level = (Ground - terrain.transform.position.y) / data.size.y;
                heights[z, x] = Mathf.Min(level, Mathf.Lerp(heights[z, x], level, blend));
            }
            data.SetHeights(0, 0, heights);
            EditorUtility.SetDirty(data);
        }

        private static void ArrangeProps(Terrain terrain, EgyptMovementBounds limits)
        {
            var group = GameObject.Find("Objetos_Praia_Ajustados");
            if (group == null) return;
            foreach (Transform prop in group.transform)
            {
                var renderers = prop.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0) continue;
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                var local = limits.transform.InverseTransformPoint(bounds.center);
                float radius = Mathf.Max(bounds.extents.x, bounds.extents.z);
                if (Mathf.Abs(local.x) < 24 + radius && local.z > -3.5f - radius && local.z < 5 + radius)
                {
                    float edge = local.z < 0 ? -5.5f - radius : 7 + radius;
                    prop.position += Depth * (edge - local.z);
                }
                bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                prop.position += Vector3.up * (terrain.SampleHeight(bounds.center) + terrain.transform.position.y - bounds.min.y + .015f);
            }
        }

        internal static void ValidateWalkway()
        {
            var terrain = BeachTerrain();
            var bounds = Object.FindFirstObjectByType<EgyptMovementBounds>();
            for (float x = -22; x <= 22; x += 2) for (float z = -3; z <= 4; z += 1)
            {
                var p = bounds.transform.TransformPoint(new Vector3(x, 0, z));
                if (Mathf.Abs(terrain.SampleHeight(p) + terrain.transform.position.y - Ground) > .04f)
                    throw new InvalidOperationException("Desnível na faixa de caminhada: " + p);
            }
            var stage = Object.FindFirstObjectByType<BrazilStage>();
            foreach (var target in new[] { stage.player.transform, stage.guide, stage.resident, stage.challengeAnchor })
                if (!bounds.Contains(target.position)) throw new InvalidOperationException("Personagem fora dos limites: " + target.name);
        }
    }
}
