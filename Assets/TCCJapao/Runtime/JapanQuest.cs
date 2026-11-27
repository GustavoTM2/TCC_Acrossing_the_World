using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using CrossingTheWorld.TCCEgypt;
using CrossingTheWorld.TCCMenu;
using CrossingTheWorld.TCCAudio;
using CrossingTheWorld.TCCBrazil;

namespace CrossingTheWorld.TCCJapan
{
    // Missão cultural no mesmo ritmo da fase Egito: guia -> templo -> monge -> prêmio -> despedida.
    // A família Okoizume é uma linhagem FICTÍCIA da narrativa; não se apresenta como história documentada.
    public sealed class JapanQuest : MonoBehaviour
    {
        public const string Exterior = "Assets/TCCJapao/Scenes/JAPAO_EXTERNO.unity";
        public const string Interior = "Assets/TCCJapao/Scenes/JAPAO_INTERIOR.unity";

        [Header("Referências das cenas")]
        public bool interior;
        public Transform player, guide, door, monk, exit, katana;
        [SerializeField] private Transform guideModel;
        public bool CanTalkToGuide => !interior && Near(guideModel ? guideModel : guide);
        public Animator animator;

        [Header("Interação")]
        [Range(1.5f, 5f)] public float interactionRadius = 2.8f;

        private static bool guideComplete, prizeAcquired, farewellComplete, returning;
        private bool talking, completionVisible, rewardPresentation, delivering, justReturned;
        private BrazilPlayer25D movement;
        private int lineIndex, lastAdvanceFrame = -1;
        private DialogueLine[] dialogue;
        private Action afterDialogue;
        private Transform katanaModel, monkModel;
        private Renderer[] katanaRenderers;
        private Camera prizeCamera;
        private RenderTexture prizeTexture;
        private Transform previewModel;
        private GUIStyle titleStyle, bodyStyle, captionStyle, buttonStyle, speakerStyle, countStyle, objectiveStyle, promptStyle, rewardStyle;
        private static readonly Color vermilion = new Color(.80f,.30f,.33f);
        private static readonly Color ink = new Color(.07f,.095f,.11f,.98f);
        private static readonly Color cream = new Color(.96f,.92f,.86f);
        private static readonly Color gold = new Color(.91f,.70f,.40f);
        private static readonly int Speed = Animator.StringToHash("Speed");
        private struct DialogueLine
        {
            public readonly string who, text;
            public DialogueLine(string who, string text) {this.who=who;this.text=text;}
        }

        private static readonly DialogueLine[] GuideText =
        {
            new DialogueLine("Guia turístico", "Bem-vindo ao Japão, Kenji! Vou acompanhar sua visita e apresentar a história deste templo."),
            new DialogueLine("Kenji", "Minha família conta histórias sobre os Okoizume, um antigo clã samurai. Quero entender o legado deles."),
            new DialogueLine("Guia turístico", "O Japão passou por diferentes épocas: cortes imperiais, governos militares, desenvolvimento das cidades e modernização. Cada período deixou marcas na arte e nos costumes."),
            new DialogueLine("Guia turístico", "Os santuários xintoístas estão ligados aos kami, enquanto os templos budistas oferecem lugares de ensinamento, reflexão e memória. As duas tradições fazem parte da cultura japonesa."),
            new DialogueLine("Guia turístico", "Os samurais tiveram papéis militares, políticos e culturais ao longo da história. As katanas também se tornaram símbolos de responsabilidade e pertencimento."),
            new DialogueLine("Guia turístico", "Dentro do templo, o monge preserva uma katana ligada à lenda dos Okoizume. Converse com ele para descobrir por que essa relíquia espera por você."),
            new DialogueLine("Kenji", "Obrigado pela explicação. Vou entrar com respeito e ouvir o que o monge tem a contar.")
        };

        private static readonly DialogueLine[] MonkText =
        {
            new DialogueLine("Monge", "Bem-vindo, Kenji Okoizume. Eu aguardava alguém que tivesse coragem de conhecer a própria história."),
            new DialogueLine("Kenji", "O senhor conhece meu clã? Cresci ouvindo falar de uma katana transmitida entre gerações."),
            new DialogueLine("Monge", "Conheço a lenda de seu clã, os Okoizume. Seus antepassados teriam protegido estas terras e confiado uma katana cerimonial a este templo."),
            new DialogueLine("Monge", "No Japão histórico, os samurais serviram a diferentes senhores e desenvolveram tradições militares e culturais. Nem todos viveram da mesma forma."),
            new DialogueLine("Monge", "A cultura japonesa também foi marcada por literatura, pintura, artesanato e pelos ensinamentos de templos budistas. Uma espada não conta sozinha a história de um povo."),
            new DialogueLine("Kenji", "Então o valor de uma katana não está apenas em vencer batalhas?"),
            new DialogueLine("Monge", "Exatamente. Na tradição fictícia dos Okoizume, a lâmina só deveria ser entregue a quem compreendesse disciplina, compaixão e responsabilidade."),
            new DialogueLine("Monge", "Seu ancestral prometeu que ela não seria usada para conquistar glória, mas para recordar o dever de proteger pessoas e preservar a memória dos que vieram antes."),
            new DialogueLine("Monge", "Você não precisa provar sua força. Ouvindo essas histórias e respeitando outras culturas, demonstrou estar preparado para receber o símbolo de sua família."),
            new DialogueLine("Kenji", "Aceito esse legado. Vou honrar os Okoizume através das minhas escolhas, não apenas pelo nome que carrego."),
            new DialogueLine("Monge", "Então receba a Katana Cerimonial Okoizume. Ela é uma lembrança de seus ancestrais e da responsabilidade que agora escolhe assumir.")
        };

        private static readonly DialogueLine[] FarewellText =
        {
            new DialogueLine("Monge", "A katana agora pertence à sua jornada, Kenji. Antes de partir, há algo importante que ainda preciso lhe ensinar."),
            new DialogueLine("Kenji", "Eu pensava que receber a espada seria o fim da história, mas sinto que tudo está começando."),
            new DialogueLine("Monge", "O legado Okoizume não vive no metal: vive nos seus atos. Valorize a paz, a curiosidade e o respeito por quem é diferente de você."),
            new DialogueLine("Monge", "Conte o que descobriu sobre os templos, os santuários e as muitas épocas da história japonesa. Nossas tradições continuam vivas porque são compartilhadas."),
            new DialogueLine("Monge", "Convide outros viajantes a visitar o Japão: conhecer cidades, jardins, museus e pessoas também é uma forma de compreender nossa cultura."),
            new DialogueLine("Kenji", "Prometo levar adiante essas histórias e mostrar que os Okoizume podem ser lembrados pela sabedoria, não pela violência."),
            new DialogueLine("Monge", "Sua missão está concluída. O templo estará de portas abertas quando quiser voltar. Siga sua viagem e aprenda com cada novo destino.")
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            guideComplete = prizeAcquired = farewellComplete = returning = false;
        }

        private void Awake()
        {
            guideComplete |= TravelProgress.JapanGuide;
            prizeAcquired |= TravelProgress.JapanPrize;
            farewellComplete |= TravelProgress.JapanFarewell;
            if (player == null) { var found = GameObject.FindWithTag("Player"); if (found) player = found.transform; }
            if (animator == null && player != null) animator = player.GetComponentInChildren<Animator>();
            if (player != null) movement = player.GetComponent<BrazilPlayer25D>();
            if (animator != null) { animator.applyRootMotion=false; animator.SetFloat(Speed,0f); }
            if (!interior && returning && player != null && door != null)
            {
                returning=false;
                justReturned=true;
                Vector3 point=door.position + (player.position-door.position).normalized*2.5f;
                point.y=player.position.y;
                if (movement!=null) movement.PlaceAt(point);
                else player.position=point;
            }
            if (katana != null)
            {
                // Os cubos criados durante a integração eram apenas um substituto visual.
                // Mantemos seus objetos para preservar as referências serializadas.
                foreach(var renderer in katana.GetComponentsInChildren<Renderer>(true)) renderer.enabled=false;
                katana.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            if (interior) RestoreOriginalTempleCollision();
            InstantiateNPC();
            if (!interior) SetupExteriorCamera();
            if (interior) { PlaceKenjiAtTempleDoor(); SetupInteriorCamera(); InstantiateKatana(); }
            else if(player!=null && door!=null)
            {
                FaceKenji(justReturned ? player.position-door.position : door.position-player.position);
            }
            if (!interior && prizeAcquired)
            {
                var existingPrize=Resources.Load<GameObject>("JapanModels/KatanaOkoizume");
                if (existingPrize!=null) BuildRewardPreview(existingPrize);
            }
        }

        private void SetupExteriorCamera()
        {
            // A cena conserva o controlador do Egito para não perder referências,
            // mas na fase externa do Japão o novo enquadramento o substitui.
            var cameraObject = GameObject.Find("Camera_Exploracao");
            if(cameraObject == null || player == null)
            {
                Debug.LogWarning("Japão: não foi possível encontrar Camera_Exploracao ou Kenji.");
                return;
            }
            var camera = cameraObject.GetComponent<Camera>();
            if(camera == null)
            {
                Debug.LogWarning("Japão: Camera_Exploracao está sem componente Camera.");
                return;
            }
            var walls = GameObject.Find("Paredes_Invisiveis");
            var follower = cameraObject.GetComponent<JapanExteriorCamera25D>();
            if(follower == null) follower = cameraObject.AddComponent<JapanExteriorCamera25D>();
            follower.Configure(player, walls != null ? walls.transform : null);
        }

        private void SetupInteriorCamera()
        {
            var cameraObject = GameObject.Find("Camera_Japao_Interior");
            if(cameraObject == null || player == null)
            {
                Debug.LogWarning("Japão interno: câmera ou Kenji não encontrados.");
                return;
            }
            var follow = cameraObject.GetComponent<JapanInteriorCamera25D>();
            if(follow == null) follow = cameraObject.AddComponent<JapanInteriorCamera25D>();
            follow.Configure(player);
        }

        private void PlaceKenjiAtTempleDoor()
        {
            // Nasce dentro da porta do salão, voltado para o altar e o monge.
            if(player==null || exit==null)return;
            Vector3 position=new Vector3(exit.position.x,player.position.y,exit.position.z-1.9f);
            if(movement!=null)movement.PlaceAt(position);
            else player.position=position;
            FaceKenji(Vector3.back);
        }

        private void FaceKenji(Vector3 lookDirection)
        {
            if(player==null)return;
            lookDirection.y=0f;
            if(lookDirection.sqrMagnitude<.001f)lookDirection=Vector3.back;
            var facing=Quaternion.LookRotation(lookDirection.normalized,Vector3.up);
            player.rotation=facing;
            if(movement!=null && movement.visual!=null)
                movement.visual.rotation=facing*Quaternion.Euler(0f,movement.visualYawOffset,0f);
            if(animator!=null)animator.SetFloat(Speed,0f);
        }

        private void RestoreOriginalTempleCollision()
        {
            var hall=GameObject.Find("BuddhaHall");
            if (hall==null) return;
            foreach(var filter in hall.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh==null || !filter.gameObject.activeInHierarchy || filter.GetComponent<Collider>()!=null) continue;
                var c=filter.gameObject.AddComponent<MeshCollider>();
                c.sharedMesh=filter.sharedMesh; c.convex=false;
            }
        }

        private static void FitModel(GameObject instance, Transform target, float height, float yaw)
        {
            instance.transform.SetParent(target,false);
            instance.transform.localPosition=Vector3.zero;
            instance.transform.localRotation=Quaternion.Euler(0,yaw,0);
            Renderer[] renderers=instance.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0) return;
            Bounds bounds=renderers[0].bounds;
            foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            float scale=height/Mathf.Max(.001f,bounds.size.y);
            instance.transform.localScale*=scale;
            bounds=renderers[0].bounds;
            foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            instance.transform.position+=target.position-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
        }

        private static void TextureModel(GameObject model, string image)
        {
            Texture2D texture=Resources.Load<Texture2D>("JapanModels/"+image);
            if(texture==null) {Debug.LogWarning("Textura japonesa não encontrada: "+image);return;}
            var shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null) shader=Shader.Find("Standard");
            var mat=new Material(shader);
            mat.name="TCC_Japao_"+image;
            if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",texture);
            if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",texture);
            foreach(var renderer in model.GetComponentsInChildren<Renderer>(true)) renderer.material=mat;
        }

        private void InstantiateNPC()
        {
            if(!interior)
            {
                // O mesmo FBX de guiaturistico da Itália agora é um prefab visível
                // na Hierarchy da cena (inclusive antes de apertar Play).
                var guideCharacter = guideModel ? guideModel.gameObject : GameObject.Find("Guia_Turistico_Japao_Original");
                if(guideCharacter == null && guide != null)
                {
                    // Recuperação para cenas antigas que ainda não têm o prefab fixo.
                    var prefab = Resources.Load<GameObject>("GuiaItaliaOriginal");
                    if(prefab != null)
                    {
                        guideCharacter = Instantiate(prefab);
                        guideCharacter.name = "Guia_Turistico_Japao_Original";
                        FitModel(guideCharacter, guide, 1.75f, 0f);
                        Debug.LogWarning("Japão: guia restaurado automaticamente; salve a cena atualizada.");
                    }
                }
                if(guideCharacter == null)
                {
                    // Um marcador visível é melhor que um NPC totalmente ausente.
                    var proxy = guide ? guide.GetComponent<Renderer>() : null;
                    if(proxy != null) proxy.enabled = true;
                    Debug.LogError("Japão: o guia turístico da Itália não foi encontrado na cena nem em Resources.");
                    return;
                }
                guideModel = guideCharacter.transform;
                guideCharacter.SetActive(true);
                // A instância da cena já tem altura e posição dos pés ajustadas.
                if(player != null)
                {
                    Vector3 facing = player.position - guideModel.position;
                    facing.y = 0f;
                    if(facing.sqrMagnitude > .01f)
                        guideCharacter.transform.rotation = Quaternion.LookRotation(facing.normalized, Vector3.up);
                }
                var renderers = guideCharacter.GetComponentsInChildren<Renderer>(true);
                foreach(var renderer in renderers) renderer.enabled = true;
                foreach(var collider in guideCharacter.GetComponentsInChildren<Collider>(true))
                    collider.enabled = false;
                var guideAnimator = guideCharacter.GetComponentInChildren<Animator>();
                if(guideAnimator != null) guideAnimator.applyRootMotion = false;
                var guideStand = guide ? guide.GetComponent<Renderer>() : null;
                if(guideStand != null) guideStand.enabled = renderers.Length == 0;
                if(renderers.Length == 0)
                    Debug.LogError("Japão: o modelo do guia foi carregado, mas não contém Renderer.");
                else
                    Debug.Log("Japão: posição do guia turístico na cena externa: " +
                        guideCharacter.transform.position);
                return;
            }
            var marker=monk;
            if(marker==null)return;
            GameObject pref=Resources.Load<GameObject>("JapanModels/MongeOkoizume");
            if(pref==null) {Debug.LogWarning("NPC japonês não encontrado.");return;}
            var character=Instantiate(pref);
            character.name="Monge_Okoizume_Original";
            FitModel(character,marker,1.75f,0f);
            foreach(var c in character.GetComponentsInChildren<Collider>(true))c.enabled=false;
            TextureModel(character,"MongeOkoizume");monkModel=character.transform;
            var anim=character.GetComponentInChildren<Animator>();
            if(anim!=null){anim.applyRootMotion=false; if(anim.parameters.Length>0) {foreach(var par in anim.parameters)if(par.name=="Speed" && par.type==AnimatorControllerParameterType.Float)anim.SetFloat("Speed",0f);}}
            var stand=marker.GetComponent<Renderer>();
            if(stand!=null) stand.enabled=false;
        }

        private void InstantiateKatana()
        {
            if(katana==null)return;
            var prefab=Resources.Load<GameObject>("JapanModels/KatanaOkoizume");
            if(prefab==null) {Debug.LogWarning("Katana Cerimonial Okoizume FBX não encontrada.");return;}
            // Ativa temporariamente o pai para medir corretamente o tamanho do FBX importado.
            katana.gameObject.SetActive(true);
            var obj=Instantiate(prefab);
            obj.name="Katana_Cerimonial_Okoizume_Original";
            FitModel(obj,katana,1.35f,90f);
            TextureModel(obj,"KatanaOkoizume");
            katanaModel=obj.transform;
            katanaRenderers=obj.GetComponentsInChildren<Renderer>(true);
            foreach(var collider in obj.GetComponentsInChildren<Collider>(true))collider.enabled=false;
            // No padrão do Egito, a relíquia está exposta antes da missão e
            // desaparece do cenário depois de recebida; fica apenas no HUD 3D.
            PlacePedestal();
            katana.gameObject.SetActive(!prizeAcquired);
            BuildRewardPreview(prefab);
        }

        private void PlacePedestal()
        {
            if(katana==null)return;
            var altar=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            altar.name="Pedestal_Katana_Okoizume";
            altar.transform.SetParent(transform,true);
            altar.transform.position=katana.position+Vector3.down*.61f;
            altar.transform.localScale=new Vector3(.55f,.51f,.48f);
            var materialShader=Shader.Find("Universal Render Pipeline/Lit");
            if(materialShader==null)materialShader=Shader.Find("Standard");
            var material=new Material(materialShader);
            material.name="Madeira_Escura_Pedestal_Katana";
            material.color=new Color(.20f,.13f,.095f);
            altar.GetComponent<Renderer>().material=material;
            var top=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            top.name="Acabamento_Dourado_Pedestal";
            top.transform.SetParent(altar.transform,true);
            top.transform.position=katana.position+Vector3.down*.065f;
            top.transform.localScale=new Vector3(.64f,.065f,.57f);
            var goldMat=new Material(materialShader);
            goldMat.color=new Color(.67f,.49f,.25f);
            top.GetComponent<Renderer>().material=goldMat;
        }

        private void BuildRewardPreview(GameObject prefab)
        {
            var previewRoot=new GameObject("Katana_3D_Preview_Camera");
            previewRoot.transform.SetParent(transform,false);
            previewRoot.transform.localPosition=Vector3.zero;
            previewRoot.transform.position=new Vector3(5000,5000,5000);
            previewModel=Instantiate(prefab,previewRoot.transform).transform;
            previewModel.name="Katana_3D_Preview";
            TextureModel(previewModel.gameObject,"KatanaOkoizume");
            FitModel(previewModel.gameObject,previewRoot.transform,2.1f,30f);
            foreach(var r in previewModel.GetComponentsInChildren<Renderer>(true)) r.gameObject.layer=30;
            prizeTexture=new RenderTexture(512,512,16,RenderTextureFormat.ARGB32);
            prizeTexture.name="TCC_Katana_3D";
            prizeTexture.antiAliasing=2;
            prizeTexture.Create();
            var cameraObj=new GameObject("Camera_Katana_3D");
            cameraObj.transform.SetParent(previewRoot.transform,false);
            prizeCamera=cameraObj.AddComponent<Camera>();
            // Igual ao escaravelho egípcio: câmera renderiza para RenderTexture
            // por requisição URP, sem depender da câmera principal da fase.
            prizeCamera.enabled=false;
            prizeCamera.clearFlags=CameraClearFlags.SolidColor;
            prizeCamera.backgroundColor=new Color(0f,0f,0f,0f);
            prizeCamera.orthographic=true;
            prizeCamera.orthographicSize=1.8f;
            prizeCamera.nearClipPlane=.1f;
            prizeCamera.farClipPlane=20f;
            prizeCamera.cullingMask=1<<30;
            prizeCamera.targetTexture=prizeTexture;
            prizeCamera.transform.position=previewRoot.transform.position+new Vector3(0,1.1f,5f);
            prizeCamera.transform.LookAt(previewRoot.transform.position+Vector3.up*1.1f);
            // Cameras criadas para previews não devem reproduzir áudio.
            prizeCamera.depth=-30;
            prizeCamera.allowHDR=false;
            var additional=prizeCamera.GetUniversalAdditionalCameraData();
            additional.renderPostProcessing=false;
            additional.renderShadows=false;
            additional.requiresColorOption=CameraOverrideOption.Off;
            additional.requiresDepthOption=CameraOverrideOption.Off;
            additional.volumeLayerMask=0;
            var lightObject=new GameObject("Luz_Visualizacao_Katana");
            lightObject.transform.SetParent(previewRoot.transform,false);
            lightObject.transform.rotation=Quaternion.Euler(45f,-25f,0f);
            var lamp=lightObject.AddComponent<Light>();
            lamp.type=LightType.Directional;
            lamp.intensity=1.25f;
            lamp.cullingMask=1<<30;
        }

        private DialogueShot shot;
        private void LateUpdate()
        {
            if (talking && movement && dialogue != null)
            {
                if (!shot) shot = gameObject.AddComponent<DialogueShot>();
                shot.Frame(player, interior ? monkModel : guideModel, dialogue[lineIndex].who == "Kenji", movement.visual, movement.visualYawOffset, interior && monkModel ? monkModel.forward : guideModel ? guideModel.forward : Vector3.zero);
            }
            if(prizeCamera==null || prizeTexture==null || previewModel==null)return;
            if(!prizeAcquired && !rewardPresentation && !completionVisible)return;
            previewModel.localRotation=Quaternion.Euler(0f,30f+Time.unscaledTime*35f,0f);
            RenderPipeline.SubmitRenderRequest(prizeCamera,
                new UniversalRenderPipeline.SingleCameraRequest{destination=prizeTexture});
        }

        private void Update()
        {
            if(EgyptSceneTransition.IsBusy || StageExperience.IsPaused) return;
            if(katanaModel!=null && katana!=null && katana.gameObject.activeSelf)katana.Rotate(Vector3.up,30f*Time.deltaTime);
            if(animator!=null && (talking || completionVisible || rewardPresentation || delivering))animator.SetFloat(Speed,0f);
            bool busy = talking || completionVisible || rewardPresentation || delivering;
            float priority = float.PositiveInfinity;
            if (player && !busy)
            {
                var npc = interior ? monk : (guideModel ? guideModel : guide);
                var portal = interior ? exit : door;
                if (Near(npc)) priority = Vector3.ProjectOnPlane(npc.position-player.position,Vector3.up).sqrMagnitude;
                if (Near(portal)) priority = Mathf.Min(priority,Vector3.ProjectOnPlane(portal.position-player.position,Vector3.up).sqrMagnitude);
            }
            bool inspecting = MuseumExhibit.Handle(player, !busy, EgyptInput.InteractPressed, priority);
            if(movement!=null)movement.enabled=!(busy || MuseumExhibit.IsReading);
            if(inspecting) { if(animator && MuseumExhibit.IsReading) animator.SetFloat(Speed,0f); return; }
            if(!EgyptInput.InteractPressed)return;
            if(talking) {AdvanceDialogue();return;}
            if(rewardPresentation || completionVisible) {ContinueFromReward();return;}
            if(player==null)return;
            if(!interior)
            {
                if(CanTalkToGuide)
                {
                    OpenDialogue(GuideText,()=>{guideComplete=true;SaveProgress();});
                }
                else if(Near(door) && guideComplete) Load(Interior,"Entrando no templo...");
            }
            else
            {
                if(Near(monk) && !prizeAcquired) OpenDialogue(MonkText,GiveReward);
                else if(Near(monk) && prizeAcquired && !farewellComplete)
                    OpenDialogue(FarewellText,()=>{farewellComplete=true;completionVisible=true;SaveProgress();});
                else if(Near(exit) && farewellComplete)
                {
                    returning=true;Load(Exterior,"Saindo do templo...");
                }
            }
        }

        private bool Near(Transform obj)
        {
            if(obj==null || player==null)return false;
            var a=player.position;var b=obj.position;
            return Mathf.Abs(a.y-b.y)<3f && Vector2.Distance(new Vector2(a.x,a.z),new Vector2(b.x,b.z))<=interactionRadius;
        }

        private void OpenDialogue(DialogueLine[] text, Action done)
        {
            dialogue=text;lineIndex=0;talking=true;afterDialogue=done;
            if(movement!=null)movement.enabled=false;
            if(animator!=null)animator.SetFloat(Speed,0f);
        }
        private void AdvanceDialogue()
        {
            if(lastAdvanceFrame==Time.frameCount)return;
            lastAdvanceFrame=Time.frameCount;
            if(++lineIndex<dialogue.Length)return;
            talking=false;
            dialogue=null;
            Action after=afterDialogue;afterDialogue=null;
            if(after!=null)after();
        }
        private void GiveReward()
        {
            if(delivering || prizeAcquired)return;
            if(katana!=null)katana.gameObject.SetActive(true);
            StartCoroutine(DeliverKatana());
        }

        private IEnumerator DeliverKatana()
        {
            delivering=true;
            Vector3 start=katana!=null?katana.position:Vector3.zero;
            Vector3 destination=player!=null?player.position+Vector3.up*1.2f:start;
            Vector3 startScale=katana!=null?katana.localScale:Vector3.one;
            float elapsed=0f;
            while(elapsed<1.45f)
            {
                if(StageExperience.IsPaused){yield return null;continue;}
                float t=Mathf.Clamp01(elapsed/1.45f);
                float smooth=t*t*(3f-2f*t);
                if(katana!=null)
                {
                    katana.position=Vector3.Lerp(start,destination,smooth)+Vector3.up*Mathf.Sin(t*Mathf.PI)*.40f;
                    katana.localScale=startScale*Mathf.Lerp(1f,.2f,smooth);
                }
                elapsed+=Time.unscaledDeltaTime;
                yield return null;
            }
            if(katana!=null)
            {
                katana.position=start;
                katana.localScale=startScale;
                katana.gameObject.SetActive(false);
            }
            prizeAcquired=true;
            SaveProgress();
            delivering=false;
            rewardPresentation=true;
        }
        private void ContinueFromReward()
        {
            rewardPresentation=false;
            completionVisible=false;
        }
        private static void ReturnToMenu()
        {
            SaveProgress();
            Load(TravelMenu.ScenePath, "Voltando ao menu...");
        }
        private static void SaveProgress()
        {
            TravelProgress.SaveJapan(guideComplete,prizeAcquired,farewellComplete);
        }
        private static void Load(string path,string note)
        {
            EgyptSceneTransition.Load(path,null,note);
        }

        private void Style()
        {
            if(titleStyle!=null)return;
            titleStyle=new GUIStyle(GUI.skin.label){fontSize=27,fontStyle=FontStyle.Bold,wordWrap=true};
            speakerStyle=new GUIStyle(titleStyle){fontSize=24};
            bodyStyle=new GUIStyle(GUI.skin.label){fontSize=21,wordWrap=true};
            captionStyle=new GUIStyle(GUI.skin.label){fontSize=15,wordWrap=true};
            countStyle=new GUIStyle(captionStyle){alignment=TextAnchor.MiddleRight};
            objectiveStyle=new GUIStyle(bodyStyle){fontSize=18};
            promptStyle=new GUIStyle(bodyStyle){fontSize=18,alignment=TextAnchor.MiddleCenter};
            rewardStyle=new GUIStyle(speakerStyle){fontSize=18};
            buttonStyle=new GUIStyle(GUI.skin.button){fontSize=18,fontStyle=FontStyle.Bold};
            titleStyle.normal.textColor=gold;
            speakerStyle.normal.textColor=gold;
            bodyStyle.normal.textColor=cream;
            objectiveStyle.normal.textColor=cream;
            promptStyle.normal.textColor=cream;
            rewardStyle.normal.textColor=gold;
            captionStyle.normal.textColor=new Color(.77f,.83f,.81f);
            countStyle.normal.textColor=cream;
            foreach (var label in new[] { titleStyle, speakerStyle, bodyStyle, captionStyle, countStyle, objectiveStyle, promptStyle, rewardStyle })
                label.hover = label.normal;
        }
        private static void Panel(Rect bounds)
        {
            var old=GUI.color;
            GUI.color=new Color(.02f,.03f,.03f,.35f);
            GUI.DrawTexture(new Rect(bounds.x+4,bounds.y+5,bounds.width,bounds.height),Texture2D.whiteTexture);
            GUI.color=vermilion;
            GUI.DrawTexture(bounds,Texture2D.whiteTexture);
            GUI.color=ink;
            GUI.DrawTexture(new Rect(bounds.x+2,bounds.y+2,bounds.width-4,bounds.height-4),Texture2D.whiteTexture);
            GUI.color=old;
        }
        private string Objective
        {
            get
            {
                if(!interior)return !guideComplete?"Converse com o guia turístico sobre a história do Japão.":"Siga até a entrada do templo e converse com o monge.";
                if(!prizeAcquired)return "Converse com o monge sobre a história dos Okoizume.";
                if(!farewellComplete)return "Volte ao monge para descobrir o significado da katana.";
                return "Legado Okoizume concluído. Você pode sair do templo.";
            }
        }
        private string Prompt
        {
            get
            {
                if(!interior)
                {
                    if(CanTalkToGuide)return "E / Enter   Conversar com o guia turístico";
                    if(Near(door))return guideComplete?"E / Enter   Entrar no templo":"Primeiro, converse com o guia turístico.";
                    return "";
                }
                if(Near(monk))
                    return !prizeAcquired?"E / Enter   Ouvir o monge":!farewellComplete?"E / Enter   Conversa final":"Legado concluído";
                if(Near(exit))return farewellComplete?"E / Enter   Sair do templo":"Primeiro, conclua o diálogo com o monge.";
                return "";
            }
        }
        private void OnGUI()
        {
            if(EgyptSceneTransition.IsBusy || StageExperience.IsPaused || MuseumExhibit.IsReading)return;
            Style();
            var old=GUI.matrix;
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)*.5f,(Screen.height-720*scale)*.5f,0),
                Quaternion.identity,Vector3.one*scale);
            if (!talking)
            {
                Panel(new Rect(24,22,610,100));
                GUI.Label(new Rect(46,34,561,34),"A cidade flutuante",titleStyle);
                GUI.Label(new Rect(46,74,560,41),Objective,objectiveStyle);
                if(prizeAcquired && !rewardPresentation && !completionVisible)
                {
                    Panel(new Rect(944,24,312,124));
                    if(prizeTexture!=null)GUI.DrawTexture(new Rect(952,28,112,112),prizeTexture,ScaleMode.ScaleToFit,true);
                    GUI.Label(new Rect(1074,48,170,28),"RELÍQUIA",captionStyle);
                    GUI.Label(new Rect(1074,75,170,55),"Katana Cerimonial\nOkoizume",rewardStyle);
                }
            }
            if(talking && dialogue!=null)
            {
                Panel(new Rect(88,455,1104,240));
                GUI.Label(new Rect(122,474,800,39),dialogue[lineIndex].who,speakerStyle);
                GUI.Label(new Rect(1078,478,84,28),(lineIndex+1)+" / "+dialogue.Length,countStyle);
                GUIStyle dyn=new GUIStyle(bodyStyle);
                while(dyn.fontSize>17 && dyn.CalcHeight(new GUIContent(dialogue[lineIndex].text),1027)>113)dyn.fontSize--;
                GUI.Label(new Rect(123,518,1034,113),dialogue[lineIndex].text,dyn);
                GUI.Label(new Rect(124,659,550,24),"E ou Enter para continuar",captionStyle);
                if(GUI.Button(new Rect(954,644,209,41),lineIndex+1==dialogue.Length?"Concluir conversa":"Continuar",buttonStyle))
                    AdvanceDialogue();
            }
            else if(delivering)
            {
                Panel(new Rect(390,613,500,67));
                GUI.Label(new Rect(415,624,464,41),"Recebendo a Katana Okoizume...",speakerStyle);
            }
            else if(rewardPresentation || completionVisible)
            {
                Panel(new Rect(210,128,860,507));
                GUI.Label(new Rect(267,162,751,47),completionVisible?"MISSÃO CONCLUÍDA":"CONQUISTA DESBLOQUEADA",titleStyle);
                if(prizeTexture!=null)GUI.DrawTexture(new Rect(260,225,320,303),prizeTexture,ScaleMode.ScaleToFit,true);
                GUI.Label(new Rect(620,240,410,56),"KATANA OKOIZUME",speakerStyle);
                GUI.Label(new Rect(620,305,409,164),completionVisible?
                    "Kenji reconheceu seu passado e decidiu levar o legado dos Okoizume adiante com respeito e sabedoria.":
                    "Uma relíquia de seus ancestrais. O valor da katana está no compromisso que Kenji assumiu com a sua história.",bodyStyle);
                if(GUI.Button(new Rect(312,554,305,53),"Voltar ao Menu",buttonStyle)) ReturnToMenu();
                if(GUI.Button(new Rect(640,554,328,53),"Continuar Explorando",buttonStyle)) ContinueFromReward();
            }
            else
            {
                string prompt=MuseumExhibit.HasPrompt ? "" : Prompt;
                if(!string.IsNullOrEmpty(prompt))
                {
                    // Painel de interação separado dos controles à esquerda.
                    Panel(new Rect(385,608,620,65));
                    GUI.Label(new Rect(403,618,584,43),prompt,promptStyle);
                }
                StageControlPanel.Draw(new Rect(24,561,342,140),
                    new Color(.88f,.49f,.51f),
                    new StageControlPanel.Control("A / D", "Esquerda / direita"),
                    new StageControlPanel.Control("S / W", "Cima / baixo"),
                    new StageControlPanel.Control("E / Enter", "Interagir"),
                    new StageControlPanel.Control("Esc", "Menu da fase"));
            }
            GUI.matrix=old;
        }

        private void OnDestroy()
        {
            if(prizeCamera!=null) {prizeCamera.targetTexture=null;Destroy(prizeCamera.gameObject);}
            if(prizeTexture!=null){prizeTexture.Release();Destroy(prizeTexture);}
        }
    }
}
