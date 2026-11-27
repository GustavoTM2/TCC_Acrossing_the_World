using System.Collections.Generic;
using UnityEngine;
using CrossingTheWorld.TCCAudio;
using CrossingTheWorld.TCCEgypt;

public sealed class MuseumExhibit : MonoBehaviour
{
    public bool egito;
    public string titulo, colecao;
    [TextArea(3, 7)] public string historia;
    [Tooltip("Arraste aqui a imagem do líder Okoizume.")] public Texture2D imagemDoQuadro;
    public Renderer telaDoQuadro;
    public Texture2D versoDaFoto;
    bool mostrarVerso;
    float giro;

    void Update()
    {
        if (reading != this || !versoDaFoto || StageExperience.IsPaused || EgyptSceneTransition.IsBusy) return;
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current?.fKey.wasPressedThisFrame == true) mostrarVerso = !mostrarVerso;
#elif ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.F)) mostrarVerso = !mostrarVerso;
#endif
        giro = Mathf.MoveTowards(giro, mostrarVerso ? 1f : 0f, Time.unscaledDeltaTime * 3.5f);
    }

    static readonly List<MuseumExhibit> exhibits = new List<MuseumExhibit>();
    static MuseumExhibit nearby, reading;
    static int closedFrame = -1;
    public static bool IsReading => reading != null;
    public static bool HasPrompt => nearby != null;

    void OnEnable() { exhibits.Add(this); }
    void OnDisable()
    {
        exhibits.Remove(this);
        if (nearby == this) nearby = null;
        if (reading == this) reading = null;
    }

    void Start()
    {
        if (!imagemDoQuadro || !telaDoQuadro) return;
        var props = new MaterialPropertyBlock();
        props.SetTexture("_BaseMap", imagemDoQuadro);
        props.SetTexture("_MainTex", imagemDoQuadro);
        telaDoQuadro.SetPropertyBlock(props);
    }

    public static bool Handle(Transform player, bool available, bool pressed, float maxDistanceSquared)
    {
        if (reading)
        {
            if (pressed) Close();
            return true;
        }
        nearby = null;
        if (!available || !player || closedFrame == Time.frameCount) return false;
        float best = Mathf.Min(2.35f * 2.35f, maxDistanceSquared);
        foreach (var item in exhibits)
        {
            if (item.gameObject.scene != player.gameObject.scene) continue;
            Vector3 delta = item.transform.position - player.position;
            float distance = delta.x * delta.x + delta.z * delta.z;
            if (Mathf.Abs(delta.y) > 2.2f || distance >= best) continue;
            best = distance;
            nearby = item;
        }
        if (nearby && pressed) reading = nearby;
        return nearby != null;
    }
    static void Close()
    {
        if (reading) { reading.mostrarVerso = false; reading.giro = 0; }
        reading = null;
        closedFrame = Time.frameCount;
    }

    void DrawPortrait(Color ink, Color paper, Color accent)
    {
        Fill(new Rect(95,50,810,500),ink);
        Fill(new Rect(101,56,798,488),paper);
        float width = 327f * Mathf.Abs(Mathf.Cos(giro * Mathf.PI));
        var photo = new Rect(294f-width/2f,82,width,436);
        if (width > 0.5f) GUI.DrawTexture(photo,giro < .5f ? imagemDoQuadro : versoDaFoto,ScaleMode.StretchToFill);
        GUI.Label(new Rect(488,97,372,50),colecao,Type(15,ink,true));
        GUI.Label(new Rect(488,157,372,47),titulo,Type(31,ink,true));
        Fill(new Rect(488,212,365,2),accent);
        GUI.Label(new Rect(488,228,368,145),historia,Type(19,ink));
        var flip = new Rect(488,388,365,50);
        Fill(flip,ink);
        GUI.Label(new Rect(507,388,335,50),mostrarVerso ? "Ver a frente   ·   F" : "Virar a foto   ·   F",Type(19,paper,true));
        if (GUI.Button(flip,GUIContent.none,GUIStyle.none)) mostrarVerso = !mostrarVerso;
        var close = new Rect(488,454,365,50);
        GUI.Label(close,"Fechar   ·   E / Enter",Type(18,ink,true));
        if (GUI.Button(close,GUIContent.none,GUIStyle.none)) Close();
    }

    static void Fill(Rect rect, Color color)
    {
        var old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = old;
    }

    static GUIStyle Type(int size, Color color, bool bold = false)
    {
        var s = new GUIStyle(GUI.skin.label) { fontSize = size, wordWrap = true,
            fontStyle = bold ? FontStyle.Bold : FontStyle.Normal, alignment = TextAnchor.MiddleLeft };
        s.normal.textColor = color;
        s.hover = s.normal;
        return s;
    }

    void OnGUI()
    {
        if (StageExperience.IsPaused || EgyptSceneTransition.IsBusy || (reading ? reading != this : nearby != this)) return;
        var matrix = GUI.matrix;
        int depth = GUI.depth;
        float scale = Mathf.Min(Screen.width / 1000f, Screen.height / 600f);
        if (scale <= 0) return;
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width-1000*scale)/2,(Screen.height-600*scale)/2,0),Quaternion.identity,Vector3.one*scale);
        GUI.depth = -30;
        var ink = egito ? new Color(.06f,.16f,.2f) : new Color(.19f,.12f,.12f);
        var accent = egito ? new Color(.82f,.62f,.28f) : new Color(.67f,.23f,.17f);
        var paper = egito ? new Color(.98f,.91f,.73f) : new Color(.98f,.95f,.88f);
        try
        {
            if (!reading)
            {
                var box = new Rect(320,520,360,48);
                Fill(box,ink);
                Fill(new Rect(box.x,box.y,4,box.height),accent);
                GUI.Label(new Rect(336,522,330,44),"E / Enter   ·   Examinar "+titulo,Type(17,paper,true));
                if (GUI.Button(box, GUIContent.none, GUIStyle.none)) reading = this;
                return;
            }
            if (imagemDoQuadro && versoDaFoto)
            {
                DrawPortrait(ink,paper,accent);
                return;
            }
            Fill(new Rect(143,307,714,263),ink);
            Fill(new Rect(149,313,702,251),paper);
            Fill(new Rect(149,313,702,7),accent);
            GUI.Label(new Rect(176,330,652,23),colecao,Type(15,ink,true));
            GUI.Label(new Rect(175,357,651,44),titulo,Type(31,ink,true));
            Fill(new Rect(177,405,645,2),accent);
            GUI.Label(new Rect(176,411,648,87),historia,Type(19,ink));
            var close = new Rect(600,511,223,37);
            Fill(close,ink);
            GUI.Label(new Rect(619,511,195,37),"Fechar   ·   E / Enter",Type(17,paper,true));
            if (GUI.Button(close,GUIContent.none,GUIStyle.none)) Close();
        }
        finally { GUI.depth = depth; GUI.matrix = matrix; }
    }
}
