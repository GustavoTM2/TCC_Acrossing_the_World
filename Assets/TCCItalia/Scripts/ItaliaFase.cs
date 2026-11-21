using UnityEngine;
using UnityEngine.SceneManagement;
using CrossingTheWorld.TCCEgypt;
using CrossingTheWorld.TCCMenu;
using CrossingTheWorld.TCCAudio;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Viaggio.Italia {
public sealed partial class ItaliaFase : MonoBehaviour {
    public const string Rua="Assets/TCCItalia/Scenes/ITALIA_RUA.unity", Sala="Assets/TCCItalia/Scenes/ITALIA_RESTAURANTE.unity";
    public bool interior;
    public Transform player,npc,door,reward;
    public Camera cameraFase;
    public float unit=1;
    public Bounds area,bridge;
    public Vector3 outward=Vector3.forward;
    public Animator animator;
    float verticalVelocity;
    bool waitForMovementRelease;
    static readonly int Speed = Animator.StringToHash("Speed");
    CharacterController controller;
    enum Mode { Explore, Talk, Count, Cook, Bake, Pack, Cut, Review, Result }
    Mode mode;
    public bool InPizzaMinigame => mode >= Mode.Count && mode <= Mode.Review;
    static bool guideDone,won,returning,finalDone;
    bool farewell,win;
    int line,step,drag=-1,errors;
    float deadline;
    string notice="";
    readonly string[] guide={"Bem-vinda, Sofia! Esta praça é nosso ponto de partida. Nas cidades italianas, a piazza reúne feiras, festas e conversas há gerações.","Na Roma antiga, estradas e aquedutos ligavam cidades e levavam água aos moradores. Em Roma ainda dá para ver vestígios dessas construções.","Séculos depois, durante o Renascimento, oficinas de cidades como Florença reuniram pintores, arquitetos e aprendizes. Cada região desenvolveu tradições próprias.","Uma dessas tradições vem de Nápoles. Procure a porta do prédio com os dois assentos ao lado da porta e entre na pizzaria. Marco está esperando por você!"};
    readonly string[] chef={"Benvenuta, Sofia! Sou Marco. A pizza que vamos preparar tem raízes em Nápoles, onde os pizzaioli passam o ofício de geração em geração.","Abra a massa, espalhe o molho e distribua a mozzarella. Algumas folhas de manjericão completam a Margherita.","Você vai atender dois pedidos seguidos. Você tem dois minutos para as duas pizzas juntas, incluindo o forno. Em ambas, use de 80 a 90% de molho e de 50 a 60% de queijo. Coloque a massa e selecione a bandeja de molho. Segure e mova o mouse sobre a pizza para despejar. Solte para pausar e confirme a quantidade.","Faça o mesmo com o queijo. A falta ou o excesso desconta pontos. A primeira é uma Margherita com três folhas de manjericão. A segunda leva quatro rodelas de calabresa e três anéis de cebola. Distribua as coberturas nos círculos. Retire cada pizza na faixa verde, coloque na caixa e corte a Margherita em 6 fatias e a Calabresa em 8 e tire pelo menos 7 nas duas. Se uma reprovar, recomeçamos o serviço."};
    readonly string[] after={"As duas pizzas ficaram prontas, Sofia! Você preparou uma Margherita e uma pizza de calabresa.","A miniatura da Ferrari F40 lembra outra tradição italiana, o desenho de automóveis. Leve essa lembrança e convide outros viajantes para conhecer a Itália!"};
    static readonly string[] margherita={"Massa","Molho","Mozzarella","Manjericão"};
    static readonly string[] calabresa={"Massa","Molho","Mozzarella","Calabresa","Cebola"};
    string[] ingredients=>order==0?margherita:calabresa;
    string RecipeName=>order==0?"Margherita":"Calabresa";
    bool ReadyForOven=>step==ingredients.Length;
    string[] Lines=>interior?(farewell?after:chef):guide;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetTrip(){guideDone=won=returning=finalDone=false;}
    void Start(){
        Time.timeScale=1;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
        won = won || TravelProgress.ItalyPrize;
        controller=player.GetComponent<CharacterController>();
        if(!interior && returning){controller.enabled=false;player.position=door.position+outward*(1.6f*unit)+Vector3.up*.12f;controller.enabled=true;returning=false;}
        cameraFase.transform.position=CameraPosition();
        Vector3 spawnForward = interior
            ? Vector3.ProjectOnPlane(npc.position - player.position, Vector3.up)
            : Vector3.back;
        if (spawnForward.sqrMagnitude > 0.0001f)
            player.rotation = Quaternion.LookRotation(spawnForward, Vector3.up);
        waitForMovementRelease = interior;
        if(reward) PrepareReward();
    }
    bool Near(Transform t)=>Vector2.Distance(new Vector2(player.position.x,player.position.z),new Vector2(t.position.x,t.position.z))<2.35f*unit;
    void Update(){
        if(StageExperience.IsPaused) { if(animator) animator.SetFloat(Speed,0); return; }
        bool act=false;float x=0,z=0;
#if ENABLE_INPUT_SYSTEM
        var k=Keyboard.current;
        if(k!=null){
            x=(k.dKey.isPressed||k.rightArrowKey.isPressed?1:0)-(k.aKey.isPressed||k.leftArrowKey.isPressed?1:0);
            // Itália exterior e interior: S avança para o fundo, W retorna.
            float keys = (k.sKey.isPressed?1f:0f)-(k.wKey.isPressed?1f:0f);
            float arrows=(k.upArrowKey.isPressed?1f:0f)-(k.downArrowKey.isPressed?1f:0f);
            z=Mathf.Clamp(keys+arrows,-1f,1f);
            act=k.eKey.wasPressedThisFrame||k.enterKey.wasPressedThisFrame||k.numpadEnterKey.wasPressedThisFrame;}
#else
        x=Input.GetAxisRaw("Horizontal");
        float keys=(Input.GetKey(KeyCode.S)?1f:0f)-(Input.GetKey(KeyCode.W)?1f:0f);
        float arrows=(Input.GetKey(KeyCode.UpArrow)?1f:0f)
            -(Input.GetKey(KeyCode.DownArrow)?1f:0f);
        z=Mathf.Clamp(keys+arrows,-1f,1f);
        act=Input.GetKeyDown(KeyCode.E)||Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.KeypadEnter);
#endif
        if(waitForMovementRelease){
            if(x==0 && z==0)waitForMovementRelease=false;
            x=z=0;
        }
        UpdateStage(x,z,act);
    }
    void UpdateStage(float x,float z,bool act){
        if(EgyptSceneTransition.IsBusy){animator.SetFloat(Speed,0);return;}
        if(mode==Mode.Explore){
            Vector3 screenRight = Vector3.ProjectOnPlane(cameraFase.transform.right, Vector3.up).normalized;
            // S avança para o fundo, W retorna; as setas continuam convencionais.
            Vector3 screenUp = Vector3.Cross(Vector3.up, screenRight);
            Vector3 direction = screenRight * x + screenUp * (z * .75f);
            var move = Vector3.ClampMagnitude(direction, 1f) * (4f * unit * Time.deltaTime);
            var dest=player.position+move;
            if(interior){
                dest.x=Mathf.Clamp(dest.x,-4.3f,4.65f); dest.z=Mathf.Clamp(dest.z,-2.4f,2f);
            }else{
                bool pastStreet=dest.z>area.max.z-unit;
                float radius=controller.radius;
                dest.x=Mathf.Clamp(dest.x,pastStreet?bridge.min.x+radius:area.min.x+unit,pastStreet?bridge.max.x-radius:area.max.x-unit);
                dest.z=Mathf.Clamp(dest.z,area.min.z+unit,Mathf.Max(area.max.z-unit,bridge.max.z-radius));
            }
            if(controller.isGrounded && verticalVelocity<=0) verticalVelocity=-2f*unit;
            verticalVelocity=Mathf.Max(verticalVelocity-22f*unit*Time.deltaTime,-30f*unit);
            controller.Move(dest-player.position+Vector3.up*verticalVelocity*Time.deltaTime);
            if(move.sqrMagnitude>.000001f)player.rotation=Quaternion.RotateTowards(player.rotation,Quaternion.LookRotation(move),720f*Time.deltaTime);
            if(act && Near(npc)){line=0;farewell=won;mode=Mode.Talk;}
            else if(act && Near(door) && (interior?finalDone:guideDone)){
                returning=interior;
                if(interior)Load(Rua);
                else EgyptSceneTransition.Load(Sala,null,"Entrando no restaurante...");
            }
        }else if(mode==Mode.Talk && act && ++line>=Lines.Length){
            if(!interior){guideDone=true;mode=Mode.Explore;}
            else if(farewell){finalDone=true;mode=Mode.Explore;}else Begin();
        }else if(mode==Mode.Count && Time.time>=deadline){deadline=Time.time+serviceRemaining;mode=Mode.Cook;}
        else if(ClockRunning && Time.time>=deadline)Finish(false);
        else if(mode==Mode.Bake && OvenProgress>=1f)ServePizza();
        UpdatePour();
        var velocity=controller.velocity;
        bool moving=mode==Mode.Explore && (x!=0 || z!=0) && new Vector2(velocity.x,velocity.z).magnitude>.05f*unit;
        animator.SetFloat(Speed, moving ? new Vector2(velocity.x,velocity.z).magnitude/unit : 0f);
        cameraFase.transform.position=Vector3.Lerp(cameraFase.transform.position,CameraPosition(),1-Mathf.Exp(-9*Time.deltaTime));
        if(reward && reward.gameObject.activeSelf)reward.Rotate(Vector3.up,40*Time.deltaTime,Space.World);
    }
    RenderTexture preview;
    Camera display;
    void PrepareReward(){
        reward.position=new Vector3(-15000,15000,-15000);
        var host=new GameObject("Câmera da Ferrari");host.transform.SetParent(transform);
        display=host.AddComponent<Camera>();display.transform.position=reward.position+new Vector3(2,1.55f,-3);
        display.transform.LookAt(reward.position+Vector3.up*.35f);
        display.orthographic=true;display.orthographicSize=1.24f;
        display.clearFlags=CameraClearFlags.SolidColor;display.backgroundColor=Color.clear;display.farClipPlane=8;
        preview=new RenderTexture(384,384,24,RenderTextureFormat.ARGB32);
        display.targetTexture=preview;display.enabled=won;reward.gameObject.SetActive(won);
    }
    void OnDestroy(){if(toppingLayer)Destroy(toppingLayer);if(preview){if(display)display.targetTexture=null;preview.Release();Destroy(preview);}}
    Vector3 streetCameraHome,streetFocus;
    public void SetupStreetCamera(){
        var rig = cameraFase.transform;
        rig.rotation = Quaternion.Euler(34f, rig.eulerAngles.y, 0f);
        cameraFase.fieldOfView = 42f;
        streetFocus = player.position;
        streetCameraHome = streetFocus + Vector3.up * unit - rig.forward * (8f * unit);
    }
    public Vector3 CameraPosition(){
        if(!interior){
            Vector3 focus = player.position;
            float margin = Mathf.Min(1.7f * unit, area.extents.x * .4f);
            focus.x = Mathf.Clamp(focus.x, area.min.x + margin, area.max.x - margin);
            focus.z = Mathf.Clamp(focus.z, area.min.z + unit, bridge.max.z - 2.2f * unit);
            Vector3 shift = focus - streetFocus;
            return streetCameraHome+shift;
        }
        return new Vector3(Mathf.Clamp(player.position.x-1.05f,-4.65f,3.6f),3.7f,
            Mathf.Clamp(player.position.z-4.1f,-6.4f,-2.1f));
    }
    void Begin(){serviceRemaining=120f;order=0;PrepareOrder();}
    void PrepareOrder(){ResetRecipe();step=errors=ovenPenalty=0;bakeFeedback="";drag=-1;notice="";deadline=Time.time+3.5f;mode=Mode.Count;}
    int Grade => Mathf.Clamp(10-errors-ovenPenalty-BalancePenalty,0,10);
    void Finish(bool served){
        timedOut=!served;
        serviceRemaining=Mathf.Max(0f,deadline-Time.time);
        win=served && Grade>=7;
        drag=-1;
        notice="";
        mode=Mode.Review;
    }
    void AcceptReview(){
        if(!win){Begin();return;}
        if(order==0){order++;PrepareOrder();return;}
        won=true;
        TravelProgress.SaveItaly(true);
        display.enabled=true;
        reward.gameObject.SetActive(true);
        mode=Mode.Result;
    }
    static void Load(string path){
#if UNITY_EDITOR
        var sibling=System.IO.Path.GetDirectoryName(SceneManager.GetActiveScene().path).Replace((char)92,(char)47)+"/"+System.IO.Path.GetFileName(path);
        if((path==Rua || path==Sala) && System.IO.File.Exists(sibling))path=sibling;
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(path,new LoadSceneParameters(LoadSceneMode.Single));
#else
        SceneManager.LoadScene(System.IO.Path.GetFileNameWithoutExtension(path));
#endif
    }
    void Menu(){
        // A conquista permanece registrada no menu até o início da próxima execução.
        SceneManager.LoadScene("Assets/TCCMenu/Scenes/Menu_Principal.unity");
    }
    void OnGUI(){
        if(EgyptSceneTransition.IsBusy)return;
        if(StageExperience.IsPaused)return;
        if(mode==Mode.Count || ClockRunning || mode==Mode.Review){DrawCounter();return;}
        var old=GUI.matrix;float scale=Mathf.Min(Screen.width/1000f,Screen.height/600f);
        GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1000*scale)/2,(Screen.height-600*scale)/2,0),Quaternion.identity,Vector3.one*scale);
        var label=new GUIStyle(GUIStyle.none){alignment=TextAnchor.MiddleCenter,wordWrap=true,fontSize=19,padding=new RectOffset(10,10,5,5)};
        label.font=GUI.skin.font;label.normal.textColor=new Color(.96f,.95f,.90f);
        label.hover=label.active=label.focused=label.normal;
        if(mode!=Mode.Result){
            Panel(new Rect(14,14,455,101),interior?"ITÁLIA  /  Pizzaria de Marco":"ITÁLIA  /  A cidade da ponte",Objective(),label);
            if(won){Panel(new Rect(813,68,174,124),"","Ferrari F40 adquirida",label,true);Car(new Rect(847,77,106,69));}
        }
        if(mode==Mode.Explore){
            StageControlPanel.Draw(new Rect(14,430,284,154),
                new Color(.82f,.43f,.29f),
                new StageControlPanel.Control("A / D", "Esquerda / direita"),
                new StageControlPanel.Control("S / W", "Cima / baixo"),
                new StageControlPanel.Control("E / Enter", "Interagir"),
                new StageControlPanel.Control("Esc", "Menu da fase"));
            string prompt=Near(npc)?"E / Enter · conversar com "+(interior?"Marco":"o guia"):Near(door)?(interior?(finalDone?"E / Enter · sair":"Conclua a visita com Marco"):guideDone?"E / Enter · entrar":"Converse com o guia primeiro"):"";
            if(prompt!="")Panel(new Rect(306,524,388,55),"",prompt,label);
        }else if(mode==Mode.Talk){
            Panel(new Rect(115,385,770,185),interior?"CHEF MARCO":"GUIA TURÍSTICO",Lines[line]+"\n\nE / Enter · continuar",label);
        }
        else if(mode==Mode.Result){
            Panel(new Rect(190,95,620,400),win?"CONQUISTA  /  DESAFIO CONCLUÍDO":"TEMPO ESGOTADO","",label);
            if(win)Car(new Rect(390,135,220,168));
            GUI.Label(new Rect(250,302,500,90),win?"Ferrari F40 adquirida\nPizza aprovada por Marco · Nota "+Grade+"/10":"Marco: Vamos tentar novamente? Siga a ordem da receita.",label);
            if(GUI.Button(new Rect(265,415,235,45),win?"CONTINUAR EXPLORANDO":"TENTAR NOVAMENTE")){
                if(win){mode=Mode.Explore;}else Begin();
            }
        }
        if(notice!="" && mode!=Mode.Result)
        {
            // O aviso ficava em cima do título da fase e do modelo da Ferrari.
            Panel(new Rect(200,127,590,42),"",notice,label);
        }
        GUI.matrix=old;
    }
    string Objective(){
        if(interior)return finalDone?"Volte à rua pela saída do restaurante.":won?"Converse mais uma vez com Marco.":"Prepare os dois pedidos com o chef Marco.";
        return won?"Explore a cidade com a sua conquista.":guideDone?"Entre pela porta do prédio com os dois assentos ao lado da porta.":"Converse com o guia sobre a história da Itália.";
    }
    void Car(Rect rect){if(preview)GUI.DrawTexture(rect,preview,ScaleMode.ScaleToFit,true);}
    void Panel(Rect rect,string title,string body,GUIStyle style,bool bottom=false){
        var old=GUI.color;
        GUI.color=new Color(.055f,.09f,.105f,.96f);GUI.DrawTexture(rect,Texture2D.whiteTexture);
        GUI.color=new Color(.77f,.40f,.27f);GUI.DrawTexture(new Rect(rect.x,rect.y,4,rect.height),Texture2D.whiteTexture);
        if(title!="")GUI.Label(new Rect(rect.x+6,rect.y+4,rect.width-12,30),title,style);
        GUI.color=old;
        float offset=bottom?rect.height-39:title==""?0:32;
        var bodyStyle=new GUIStyle(style){fontSize=rect.width<300?14:18};
        GUI.Label(new Rect(rect.x+4,rect.y+offset,rect.width-8,rect.height-offset),body,bodyStyle);
    }
}}