using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Viaggio.Italia {
public sealed partial class ItaliaFase {
    const int LayerSize=192;
    readonly int[] applied=new int[3];
    readonly Vector2[] toppingPoints=new Vector2[7];
    readonly int[] toppingIds=new int[7];
    int toppingCount,selectedTool=-1,paintSamples;
    float dispensed;
    Texture2D toppingLayer;
    Color32[] pixels;
    static readonly Rect Pie=new Rect(210.5f,281,235,235);
    bool Dosing=>step==1 || step==2;

    int Lower(int id)=>id==1?80:50;
    int Upper(int id)=>id==1?90:60;
    string RangeText(int id)=>Lower(id)+"–"+Upper(id)+"%";
    int DosePenalty(int id){
        int outside=Mathf.Max(0,Mathf.Max(Lower(id)-applied[id],applied[id]-Upper(id)));
        return Mathf.Min(5,Mathf.CeilToInt(outside/4f));
    }
    int BalancePenalty=>DosePenalty(1)+DosePenalty(2);
    string BalanceText(int id){
        int value=applied[id];
        return ingredients[id]+" "+value+"% · "+(value<Lower(id)?"faltou":value>Upper(id)?"em excesso":"no ponto");
    }
    void ClearToppings(){
        if(!toppingLayer){
            toppingLayer=new Texture2D(LayerSize,LayerSize,TextureFormat.RGBA32,false);
            toppingLayer.wrapMode=TextureWrapMode.Clamp;
            pixels=new Color32[LayerSize*LayerSize];
        }
        System.Array.Clear(pixels,0,pixels.Length);
        toppingLayer.SetPixels32(pixels);toppingLayer.Apply(false);
        selectedTool=-1;dispensed=0;paintSamples=0;toppingCount=0;
        System.Array.Clear(applied,0,applied.Length);
    }
    void UpdatePour(){
        pouring=false;
        if(mode!=Mode.Cook || !Dosing || selectedTool!=step || !Application.isFocused)return;
#if ENABLE_INPUT_SYSTEM
        var mouse=Mouse.current;if(mouse==null || !mouse.leftButton.isPressed)return;
        Vector2 screen=mouse.position.ReadValue();
#else
        if(!Input.GetMouseButton(0))return;
        Vector2 screen=Input.mousePosition;
#endif
        var point=new Vector2(1000f*screen.x/Screen.width,600f*(1f-screen.y/Screen.height));
        PourAt(point,Time.deltaTime);
    }
    void PourAt(Vector2 point,float elapsed){
        if(!Dosing || selectedTool!=step || mode!=Mode.Cook)return;
        Vector2 uv=new Vector2((point.x-Pie.x)/Pie.width,1f-(point.y-Pie.y)/Pie.height);
        if((uv-new Vector2(.5f,.5f)).sqrMagnitude>.425f*.425f)return;
        pouring=dispensed<100;
        dispensed=Mathf.Min(100,dispensed+elapsed*(step==1?26:22));
        applied[step]=Mathf.RoundToInt(dispensed);
        int needed=Mathf.FloorToInt(dispensed*4);
        if(needed<=paintSamples)return;
        int x=Mathf.RoundToInt(uv.x*(LayerSize-1)),y=Mathf.RoundToInt(uv.y*(LayerSize-1));
        for(;paintSamples<needed;paintSamples++){
            int seed=paintSamples;
            if(step==1)Stamp(x+(seed*7%11)-5,y+(seed*3%13)-6,9,8,new Color32(188,55,33,250));
            else Stamp(x+(seed*19%27)-13,y+(seed*11%25)-12,4+seed%3,1+seed%2,new Color32(255,(byte)(219+seed%25),155,255));
        }
        toppingLayer.SetPixels32(pixels);toppingLayer.Apply(false);
    }
    void Stamp(int cx,int cy,int rx,int ry,Color32 col){
        for(int y=cy-ry;y<=cy+ry;y++)for(int x=cx-rx;x<=cx+rx;x++){
            if(x<0||y<0||x>=LayerSize||y>=LayerSize)continue;
            float dx=(x-cx)/(float)rx,dy=(y-cy)/(float)ry;
            float ux=(x+.5f)/LayerSize-.5f,uy=(y+.5f)/LayerSize-.5f;
            if(dx*dx+dy*dy<=1 && ux*ux+uy*uy<.445f*.445f)pixels[y*LayerSize+x]=col;
        }
    }
    void ConfirmDose(){
        if(!Dosing || selectedTool!=step)return;
        step++;selectedTool=-1;dispensed=0;paintSamples=0;drag=-1;notice="";
    }
    void DrawPie(Rect rect){
        GUI.DrawTexture(rect,pizzaArt[0],ScaleMode.ScaleToFit);
        if(toppingLayer)GUI.DrawTexture(rect,toppingLayer);
        for(int i=0;i<toppingCount;i++){
            var p=toppingPoints[i];
            float diameter = order==0 ? 48f : toppingIds[i]==3 ? 46f : 42f;
            Vector2 center = new Vector2(rect.x+(p.x-Pie.x)*rect.width/Pie.width,
                rect.y+(p.y-Pie.y)*rect.height/Pie.height);
            Vector2 size = new Vector2(diameter*rect.width/Pie.width,diameter*rect.height/Pie.height);
            Icon(toppingIds[i],new Rect(center-size*.5f,size));
        }
    }
    void DrawDispenser(){
        if(selectedTool!=step || !Dosing)return;
        Vector2 p=Event.current.mousePosition;
        if(Vector2.Distance(p,Pie.center)>Pie.width*.425f)return;
        var prior=GUI.matrix;
        GUIUtility.RotateAroundPivot(-24,p);
        Color food=step==1?new Color32(188,55,33,255):new Color32(250,225,168,255);
        Card(new Rect(p.x-4,p.y-48,12,30),new Color32(111,83,63,255),4);
        Card(new Rect(p.x-10,p.y-28,22,16),Paper,6);
        Card(new Rect(p.x-7,p.y-26,16,11),food,5);
        GUI.matrix=prior;
        if(pouring){
            for(int i=0;i<3;i++){
                float phase=Mathf.Repeat(Time.time*4+i/3f,1);
                float x=p.x+5*(1-phase)+(step==2?Mathf.Sin(i*2f)*4:0);
                Card(new Rect(x-2,p.y-15+phase*15,step==1?5:4,step==1?7:3),food,2);
            }
        }
    }
    bool pouring;
    void DoseCard(){
        Card(new Rect(632,272,332,239),Paper);
        Letter(new Rect(645,281,306,41),ingredients[step]+" · "+applied[step]+"%",24,Ink,true);
        Letter(new Rect(647,327,302,33),"Faixa do pedido: "+RangeText(step),18,Ink);
        var bar=new Rect(660,377,275,16);Card(bar,new Color32(203,166,124,255));
        Card(new Rect(bar.x+bar.width*Lower(step)/100f,bar.y,bar.width*(Upper(step)-Lower(step))/100f,16),Green,1);
        Card(new Rect(bar.x+bar.width*applied[step]/100f-2,bar.y-5,5,26),Ink,1);
        Letter(new Rect(642,400,312,33),applied[step]>Upper(step)?"Em excesso":applied[step]<Lower(step)?"Abaixo da faixa":"No ponto",16,Ink,true);
        if(selectedTool==step && CounterButton(new Rect(679,445,239,48),"Concluir "+ingredients[step].ToLower()))ConfirmDose();
    }
}}