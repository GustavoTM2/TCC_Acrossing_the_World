using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrossingTheWorld.TCCEgypt.Editor
{
    public static class EgyptFreeDepthSetup
    {
        public const string Exterior = "Assets/TCCEgito/Scenes/Egito_Exterior_Jogavel.unity";
        public const string Interior = "Assets/TCCEgito/Scenes/Egito_Interior_Jogavel.unity";

        [MenuItem("TCC/Atualizar movimentacao e postura do Egito")]
        public static void UpdateFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Saia do Play antes de atualizar a fase.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            try
            {
                PrepareAssets();
                foreach (string path in new[] { Exterior, Interior })
                {
                    Scene scene = EditorSceneManager.OpenScene(path);
                    ApplyToScene(scene);
                    EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Não consegui salvar " + path);
                }
                AssetDatabase.SaveAssets();
                EditorSceneManager.OpenScene(Exterior);
                Debug.Log("Movimento em profundidade, paredes invisíveis e postura dos NPCs atualizados. A/D: lados; W/S: fundo/frente.");
            }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        public static void PrepareAssets()
        {
            ModelImporter importer = AssetImporter.GetAtPath(EgyptModelIntegration.ModelPath("Guia")) as ModelImporter;
            if (importer == null) throw new InvalidOperationException("Falta o FBX do guia em Art/Guia.");
            if (!importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
        }

        public static void ApplyToScene(Scene scene)
        {
            EgyptPlayer25D player = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<EgyptPlayer25D>(true)).Single();
            player.depthAxis = Vector3.Cross(player.movementAxis.normalized, Vector3.up).normalized;
            EgyptCamera25D camera = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<EgyptCamera25D>(true)).Single();
            camera.depthAxis = player.depthAxis;

            EgyptMovementBounds bounds = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<EgyptMovementBounds>(true)).FirstOrDefault();
            if (bounds == null)
            {
                GameObject root = new GameObject("Limites_Invisiveis");
                root.transform.SetParent(player.stage.transform, false);
                root.transform.position = player.transform.position;
                root.transform.rotation = Quaternion.LookRotation(player.depthAxis, Vector3.up);
                bounds = root.AddComponent<EgyptMovementBounds>();
                Vector3[] positions = player.GetComponent<EgyptInteractor>().points
                    .Select(point => point.transform.position).Concat(new[] { player.transform.position }).ToArray();
                float[] horizontal = positions.Select(position => root.transform.InverseTransformPoint(position).x).ToArray();
                bounds.horizontalMin = horizontal.Min() - 5f;
                bounds.horizontalMax = horizontal.Max() + 5f;
                bounds.frontDistance = player.stage.interior ? 1f : 2f;
                bounds.backDistance = player.stage.interior ? 2f : 4f;
                bounds.UpdateColliders();
            }

            foreach (EgyptInteractionPoint point in player.GetComponent<EgyptInteractor>().points)
            {
                string id = point.kind == EgyptPointKind.Guide ? "Guia" : point.kind == EgyptPointKind.Mummy ? "Mumia" : null;
                if (id == null) continue;
                Transform visual = point.transform.Find("Visual_Modelo");
                if (visual == null) throw new InvalidOperationException("Aplique os modelos antes de configurar a postura de " + id);
                ConfigureIdle(visual.gameObject, id);
            }
            Physics.SyncTransforms();
        }

        public static EgyptNpcIdle ConfigureIdle(GameObject visual, string id)
        {
            EgyptNpcIdle existing = visual.GetComponent<EgyptNpcIdle>();
            if (existing != null && visual.transform.Find("Rig_Leve_" + id) != null) return existing;
            if (existing != null)
            {
                existing.RestoreBasePose();
                UnityEngine.Object.DestroyImmediate(existing);
            }
            Transform head, left, right;
            MakeNpcRig(visual, id, out head, out left, out right);
            if (head == null || left == null || right == null)
                throw new InvalidOperationException("Não encontrei cabeça e braços em " + id);
            foreach (Animator animator in visual.GetComponentsInChildren<Animator>(true)) animator.enabled = false;
            EgyptNpcIdle idle = visual.AddComponent<EgyptNpcIdle>();
            idle.coordinateSpace = visual.transform;
            idle.head = head;
            idle.leftArm = left;
            idle.rightArm = right;
            idle.leftArmSide = Mathf.Sign(visual.transform.InverseTransformPoint(left.position).x);
            idle.rightArmSide = Mathf.Sign(visual.transform.InverseTransformPoint(right.position).x);
            idle.phaseOffset = id == "Guia" ? 0.3f : 2.1f;
            idle.CaptureBasePose();
            idle.EvaluatePose(0);
            Debug.Log("TCC_NPC_RIG: " + id + ", head=" + head.name + ", arms=" + left.name + "/" + right.name);
            return idle;
        }

        private static void MakeNpcRig(GameObject visual, string id, out Transform head, out Transform left, out Transform right)
        {
            MeshFilter filter = visual.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault();
            Renderer original;
            Mesh originalMesh;
            Mesh temporary = null;
            if (filter != null)
            {
                original = filter.GetComponent<Renderer>();
                originalMesh = filter.sharedMesh;
            }
            else
            {
                SkinnedMeshRenderer source = visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).Single();
                temporary = new Mesh();
                source.BakeMesh(temporary);
                original = source;
                originalMesh = temporary;
            }
            Matrix4x4 conversion = visual.transform.worldToLocalMatrix * original.transform.localToWorldMatrix;
            Vector3[] vertices = originalMesh.vertices.Select(vertex => conversion.MultiplyPoint3x4(vertex)).ToArray();
            Bounds bounds = new Bounds(vertices[0], Vector3.zero);
            foreach (Vector3 vertex in vertices) bounds.Encapsulate(vertex);
            // BakeMesh pode devolver a escala interna do esqueleto importado.
            // Mantém a altura e o centro visíveis do modelo antes de criar os novos ossos.
            float targetHeight = original.bounds.size.y / Mathf.Max(0.0001f, Mathf.Abs(visual.transform.lossyScale.y));
            float normalization = targetHeight / Mathf.Max(0.0001f, bounds.size.y);
            Vector3 targetCenter = visual.transform.InverseTransformPoint(original.bounds.center);
            Vector3 oldCenter = bounds.center;
            for (int i = 0; i < vertices.Length; i++) vertices[i] = (vertices[i] - oldCenter) * normalization + targetCenter;
            bounds = new Bounds(vertices[0], Vector3.zero);
            foreach (Vector3 vertex in vertices) bounds.Encapsulate(vertex);
            GameObject root = new GameObject("Rig_Leve_" + id);
            root.transform.SetParent(visual.transform, false);
            Transform body = Bone("Corpo", root.transform, Vector3.zero);
            head = Bone("Cabeca", root.transform, new Vector3(bounds.center.x, bounds.min.y + bounds.size.y * 0.8f, bounds.center.z));
            left = Bone("Braco_Esquerdo", root.transform, new Vector3(bounds.center.x + bounds.size.x * 0.23f,
                bounds.min.y + bounds.size.y * 0.72f, bounds.center.z));
            right = Bone("Braco_Direito", root.transform, new Vector3(bounds.center.x - bounds.size.x * 0.23f,
                bounds.min.y + bounds.size.y * 0.72f, bounds.center.z));
            Transform[] bones = { body, head, left, right };
            string meshPath = "Assets/TCCEgito/Generated/" + id + "_Rig_Leve.asset";
            Mesh existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            Mesh mesh;
            {
                mesh = UnityEngine.Object.Instantiate(originalMesh);
                mesh.name = id + "_Rig_Leve";
                mesh.vertices = vertices;
                Matrix4x4 normalConversion = conversion.inverse.transpose;
                if (originalMesh.normals.Length == vertices.Length)
                    mesh.normals = originalMesh.normals.Select(normal => normalConversion.MultiplyVector(normal).normalized).ToArray();
                if (originalMesh.tangents.Length == vertices.Length)
                    mesh.tangents = originalMesh.tangents.Select(tangent =>
                    {
                        Vector3 xyz = conversion.MultiplyVector(new Vector3(tangent.x, tangent.y, tangent.z)).normalized;
                        return new Vector4(xyz.x, xyz.y, xyz.z, tangent.w * Mathf.Sign(conversion.determinant));
                    }).ToArray();
                if (conversion.determinant < 0)
                    for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
                    {
                        int[] triangles = mesh.GetTriangles(submesh);
                        for (int i = 0; i < triangles.Length; i += 3)
                        { int swap = triangles[i + 1]; triangles[i + 1] = triangles[i + 2]; triangles[i + 2] = swap; }
                        mesh.SetTriangles(triangles, submesh);
                    }
                BoneWeight[] weights = new BoneWeight[vertices.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    float y = (vertices[i].y - bounds.min.y) / bounds.size.y;
                    float x = vertices[i].x - bounds.center.x;
                    float headWeight = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.78f, 0.87f, y));
                    float armWeight = Mathf.SmoothStep(0, 1,
                        Mathf.InverseLerp(bounds.size.x * 0.14f, bounds.size.x * 0.27f, Mathf.Abs(x)))
                        * (1f - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.77f, 0.84f, y)))
                        * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.3f, 0.4f, y)) * (1f - headWeight);
                    weights[i] = new BoneWeight { boneIndex0 = 0, weight0 = 1f - headWeight - armWeight,
                        boneIndex1 = 1, weight1 = headWeight, boneIndex2 = x >= 0 ? 2 : 3, weight2 = armWeight };
                }
                mesh.boneWeights = weights;
                mesh.bindposes = bones.Select(bone => bone.worldToLocalMatrix * root.transform.localToWorldMatrix).ToArray();
                mesh.RecalculateBounds();
                if (existingMesh == null) AssetDatabase.CreateAsset(mesh, meshPath);
                else
                {
                    EditorUtility.CopySerialized(mesh, existingMesh);
                    UnityEngine.Object.DestroyImmediate(mesh);
                    mesh = existingMesh;
                    EditorUtility.SetDirty(mesh);
                }
            }
            SkinnedMeshRenderer renderer = root.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = mesh;
            renderer.sharedMaterials = original.sharedMaterials;
            renderer.bones = bones;
            renderer.rootBone = body;
            renderer.localBounds = mesh.bounds;
            renderer.updateWhenOffscreen = true;
            original.enabled = false;
            if (temporary != null) UnityEngine.Object.DestroyImmediate(temporary);
        }

        private static Transform Bone(string name, Transform parent, Vector3 position)
        {
            Transform bone = new GameObject(name).transform;
            bone.SetParent(parent, false);
            bone.localPosition = position;
            return bone;
        }
    }
}
