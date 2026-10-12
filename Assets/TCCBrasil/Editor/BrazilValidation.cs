using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CrossingTheWorld.TCCEgypt;

namespace CrossingTheWorld.TCCBrazil.Editor
{
    [InitializeOnLoad]
    public static class BrazilValidation
    {
        private const string PlayKey = "TCCBrasil.PlayValidation";
        private static BrazilPlayer25D playPlayer;
        private static Vector3 playStart;
        private static double phaseStart;
        private static int phase;
        private static bool stopRequested;

        static BrazilValidation()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(PlayKey,false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode)
                {
                    playPlayer = UnityEngine.Object.FindAnyObjectByType<BrazilPlayer25D>();
                    if (playPlayer == null) { FailPlay("Lucas não foi instanciado em Play."); return; }
                    playPlayer.enabled = false;
                    playStart = playPlayer.transform.position;
                    phase = 0;
                    phaseStart = EditorApplication.timeSinceStartup;
                    stopRequested = false;
                }
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    SessionState.SetBool(PlayKey,false);
                    Debug.Log("BRAZIL_PLAY_OK: caminhada, profundidade, parada, câmera e personagem em Play.");
                    EditorApplication.Exit(0);
                }
            };
            EditorApplication.update += TickPlay;
        }

        public static void RunInPlay()
        {
            Require(Application.unityVersion == "6000.5.7f1","Use Unity 6000.5.7f1.");
            EditorSceneManager.OpenScene(BrazilSceneSetup.ScenePath);
            SessionState.SetBool(PlayKey,true);
            EditorApplication.EnterPlaymode();
        }

        private static void TickPlay()
        {
            if (!SessionState.GetBool(PlayKey,false) || !EditorApplication.isPlaying || playPlayer == null || stopRequested) return;
            try
            {
                float elapsed = (float)(EditorApplication.timeSinceStartup - phaseStart);
                Vector2 input = phase == 1 ? Vector2.right : phase == 2 ? Vector2.up : Vector2.zero;
                playPlayer.TickMovement(input,0.02f);
                var bounds = UnityEngine.Object.FindAnyObjectByType<EgyptMovementBounds>();
                Require(bounds.Contains(playPlayer.transform.position),"Lucas saiu da área em Play.");
                if (phase == 0 && elapsed > 0.8f)
                {
                    Require(playPlayer.GetComponent<CharacterController>().isGrounded,"Lucas não estabilizou no chão em Play.");
                    playStart = playPlayer.transform.position;
                    phase = 1; phaseStart = EditorApplication.timeSinceStartup;
                }
                else if (phase == 1 && elapsed > 1.2f)
                {
                    Require(Vector3.Dot(playPlayer.transform.position-playStart,playPlayer.horizontalAxis)>0.5f,"Sem movimento lateral em Play.");
                    Require(playPlayer.animator.GetCurrentAnimatorStateInfo(0).IsName("Caminhada"),"O Animator não entrou em caminhada.");
                    playStart = playPlayer.transform.position;
                    phase = 2; phaseStart = EditorApplication.timeSinceStartup;
                }
                else if (phase == 2 && elapsed > 0.8f)
                {
                    Require(Vector3.Dot(playPlayer.transform.position-playStart,playPlayer.depthAxis)>0.3f,"Sem profundidade em Play.");
                    phase = 3; phaseStart = EditorApplication.timeSinceStartup;
                }
                else if (phase == 3 && elapsed > 0.7f)
                {
                    Require(playPlayer.animator.GetCurrentAnimatorStateInfo(0).IsName("Parado"),"O Animator não parou ao soltar o controle.");
                    var follow = UnityEngine.Object.FindAnyObjectByType<EgyptCamera25D>();
                    Require(follow.target == playPlayer.transform,"Câmera sem alvo em Play.");
                    Vector3 screen = follow.GetComponent<Camera>().WorldToViewportPoint(playPlayer.transform.position+Vector3.up);
                    Require(screen.z>0 && screen.x>0.2f && screen.x<0.8f && screen.y>0.1f && screen.y<0.9f,"Lucas saiu do enquadramento.");
                    Bounds visible = BrazilSceneSetup.BoundsOf(playPlayer.visual.gameObject);
                    Require(Vector2.Distance(new Vector2(visible.center.x,visible.center.z),new Vector2(playPlayer.transform.position.x,playPlayer.transform.position.z))<0.5f,
                        "O modelo animado se separou da cápsula física.");
                    stopRequested = true;
                    EditorApplication.ExitPlaymode();
                }
            }
            catch(Exception e) { FailPlay(e.Message); }
        }

        private static void FailPlay(string message)
        {
            SessionState.SetBool(PlayKey,false);
            Debug.LogError("BRAZIL_PLAY_FAILED " + message);
            EditorApplication.Exit(1);
        }

        [MenuItem("TCC/Brasil/Validar exploracao da praia")]
        public static void Validate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            try
            {
                Require(Application.unityVersion == "6000.5.7f1", "Abra no Unity 6000.5.7f1 solicitado para o TCC.");
                EditorSceneManager.OpenScene(BrazilSceneSetup.ScenePath);
                var player = UnityEngine.Object.FindFirstObjectByType<BrazilPlayer25D>();
                var bounds = UnityEngine.Object.FindFirstObjectByType<EgyptMovementBounds>();
                Require(player != null && bounds != null, "Faltam jogador ou paredes invisíveis.");
                Require(player.animator != null && player.visual != null, "Faltam modelo ou animação do Lucas.");
                Require(bounds.GetComponentsInChildren<BoxCollider>().Count(c => c.enabled && !c.isTrigger) == 4,
                    "As quatro paredes devem usar colisões físicas.");
                Require(player.animator.runtimeAnimatorController != null && !player.animator.applyRootMotion, "Animação não configurada.");
                Require(player.visual.GetComponentsInChildren<Renderer>().All(r => r.sharedMaterials.All(m => m != null && m.mainTexture != null)),
                    "Lucas precisa usar a textura fornecida.");
                Physics.SyncTransforms();
                Vector3 origin = player.transform.position;
                Simulate(player,Vector2.zero,60);
                float settledY = player.transform.position.y;
                Require(Mathf.Abs(settledY - origin.y) < 0.4f, "Lucas caiu ou está longe do chão no início.");
                Vector3 settled = player.transform.position;
                Simulate(player,Vector2.right,60);
                Require(Vector3.Dot(player.transform.position - settled,player.horizontalAxis) > 1, "Lucas não caminha lateralmente.");
                Place(player,settled);
                Simulate(player,Vector2.up,45);
                Require(Vector3.Dot(player.transform.position - settled,player.depthAxis) > 0.5f, "Lucas não caminha para o fundo.");
                Place(player,settled);
                Simulate(player,Vector2.down,45);
                Require(Vector3.Dot(player.transform.position - settled,-player.depthAxis) > 0.5f, "Lucas não caminha para a frente.");
                // Começa perto de cada limite para testar os colliders sem depender de atravessar os objetos da praia.
                foreach (Vector2 input in new[] {Vector2.left,Vector2.right,Vector2.up,Vector2.down})
                {
                    Vector3 local = bounds.transform.InverseTransformPoint(settled);
                    if (input.x < 0) local.x = bounds.horizontalMin + 1;
                    if (input.x > 0) local.x = bounds.horizontalMax - 1;
                    if (input.y > 0) local.z = bounds.backDistance - 1;
                    if (input.y < 0) local.z = -bounds.frontDistance + 1;
                    Vector3 point = bounds.transform.TransformPoint(local);
                    Terrain terrain = UnityEngine.Object.FindFirstObjectByType<Terrain>();
                    if (terrain != null) point.y = terrain.SampleHeight(point) + terrain.transform.position.y + 0.05f;
                    Place(player,point);
                    Simulate(player,input,240);
                    Require(bounds.Contains(player.transform.position,0.05f), "Lucas atravessou um limite da área: " + input);
                    Require(player.transform.position.y > player.respawnBelowY + 1, "Lucas caiu durante o teste de limite.");
                }
                Place(player,origin);
                CheckAnimation(player);
                var camera = UnityEngine.Object.FindFirstObjectByType<EgyptCamera25D>();
                Require(camera != null && camera.target == player.transform, "A câmera não acompanha Lucas.");
                Debug.Log("BRAZIL_PHYSICS_ANIMATION_OK: lados, profundidade, quatro paredes, chão, câmera e caminhada.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }

        internal static void Place(BrazilPlayer25D player, Vector3 position)
        {
            var controller = player.GetComponent<CharacterController>();
            controller.enabled = false;
            player.transform.position = position;
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static void Simulate(BrazilPlayer25D player, Vector2 input, int frames)
        {
            for (int i = 0; i < frames; i++) player.TickMovement(input,1f/60f);
        }

        private static void CheckAnimation(BrazilPlayer25D player)
        {
            AnimationClip walk = AssetDatabase.LoadAssetAtPath<AnimationClip>(BrazilSceneSetup.Generated + "Lucas_Caminhada_Loop.anim");
            Require(walk != null && walk.length > 0.1f, "Não há clip de caminhada válido.");
            var skin = player.visual.GetComponentInChildren<SkinnedMeshRenderer>();
            Require(skin != null, "Lucas não tem malha animada.");
            var first = new Mesh();
            var second = new Mesh();
            AnimationMode.StartAnimationMode();
            try
            {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(player.animator.gameObject,walk,walk.length*0.1f);
                AnimationMode.EndSampling();
                skin.BakeMesh(first);
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(player.animator.gameObject,walk,walk.length*0.6f);
                AnimationMode.EndSampling();
                skin.BakeMesh(second);
                Vector3[] a=first.vertices, b=second.vertices;
                Require(a.Length == b.Length && a.Length > 0, "Malha vazia ao avaliar a caminhada.");
                float change = 0;
                for (int i=0;i<a.Length;i++) change = Mathf.Max(change,Vector3.Distance(a[i],b[i]));
                Debug.Log("LUCAS_ANIMATION_DELTA " + change);
                Require(change > 0.001f, "A caminhada não deforma o modelo do Lucas.");
            }
            finally
            {
                AnimationMode.StopAnimationMode();
                UnityEngine.Object.DestroyImmediate(first);
                UnityEngine.Object.DestroyImmediate(second);
            }
        }

        internal static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
