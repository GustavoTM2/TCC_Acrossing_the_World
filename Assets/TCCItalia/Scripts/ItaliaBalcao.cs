using UnityEngine;

namespace Viaggio.Italia {
public sealed partial class ItaliaFase {
    Texture2D counterArt, foodArt, calabresaArt, gratedCheese, bowlArt;
    const int WholePizza=5;
    Texture2D[] pizzaArt;
    static readonly Color Ink = new Color32(68,48,37,255);
    static readonly Color Paper = new Color32(255,242,207,255);
    static readonly Color Green = new Color32(51,88,70,255);
    static readonly Rect Board = new Rect(100,268,460,265);
    static readonly Rect Oven = new Rect(688,265,280,250);

    int order, portion, targetStart;
    int[] tray={0,1,2,3};
    float sweetStart;
    float serviceRemaining;
    float SweetWidth=>order==0?.12f:.095f;
    int Portions=>step==3 && order==1?4:step>=3?3:1;
    Vector2 TargetPoint(int index){
        float angle=(targetStart+index*(step==2?2:3))*Mathf.PI/4;
        return new Vector2(328+68*Mathf.Cos(angle),398+58*Mathf.Sin(angle));
    }
    float TargetRadius=>order==0?20:15;
    void ResetRecipe(){
        cutsMade=0;
        portion=0;
        ClearToppings();
        targetStart=Random.Range(0,8);
        sweetStart=Random.Range(.53f,.70f);
        for(int i=0;i<tray.Length;i++)tray[i]=i;
        for(int i=tray.Length-1;i>0;i--){
            int j=Random.Range(0,i+1);
            int t=tray[i];tray[i]=tray[j];tray[j]=t;
        }
    }
    float ovenStart;
    int ovenPenalty;
    string bakeFeedback="";
    float OvenProgress => Mathf.Clamp01((Time.time-ovenStart)/(order==0?4.2f:3.5f));
    void PutInOven(){
        ovenStart=Time.time;
        notice="";
        mode=Mode.Bake;
    }
    void ServePizza(){
        float heat=OvenProgress;
        if(heat>=sweetStart && heat<=sweetStart+SweetWidth){ovenPenalty=0;bakeFeedback="Massa dourada no ponto.";}
        else if(heat>=sweetStart-.08f && heat<=sweetStart+SweetWidth+.08f){ovenPenalty=2;bakeFeedback=heat<sweetStart?"Massa um pouco pálida.":"Borda um pouco tostada.";}
        else{ovenPenalty=5;bakeFeedback=heat<sweetStart?"O centro da massa ficou cru.":"A borda queimou.";}
        cutsMade=0;
        carryingPizza=false;
        cutting=false;
        mode=Mode.Pack;
    }
    void DrawOven(){
        Card(new Rect(108,282,435,235),Paper);
        Letter(new Rect(121,296,410,43),"Ponto da massa",27,Ink,true);
        Letter(new Rect(122,342,408,32),"Crua                         Dourada                 Queimada",15,Ink);
        var bar=new Rect(146,386,354,21);
        Card(bar,new Color32(192,139,90,255));
        Card(new Rect(bar.x+sweetStart*bar.width,bar.y,SweetWidth*bar.width,bar.height),Green,1);
        Card(new Rect(bar.x+bar.width*OvenProgress-3,bar.y-6,6,33),Ink,2);
        if(CounterButton(new Rect(206,441,245,51),"Retirar do forno"))ServePizza();
    }
    void LoadCounterArt(){
        if(counterArt) return;
        counterArt=Resources.Load<Texture2D>("ItaliaPronta/BalcaoMarco");
        calabresaArt=Resources.Load<Texture2D>("ItaliaPronta/CalabresaCebola");
        foodArt=Resources.Load<Texture2D>("ItaliaPronta/IngredientesMarco");
        bowlArt=Resources.Load<Texture2D>("ItaliaPronta/TigelasMarco");
        gratedCheese=Resources.Load<Texture2D>("ItaliaPronta/QueijoRalado");
        pizzaArt=new Texture2D[4];
        for(int i=0;i<4;i++)pizzaArt[i]=Resources.Load<Texture2D>("ItaliaPronta/Pizza"+(i+1));
    }
    static void Card(Rect rect,Color color,float radius=8){
        GUI.DrawTexture(rect,Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0,color,0,radius);
    }
    static void Letter(Rect rect,string text,int size,Color color,bool bold=false){
        var style=new GUIStyle(GUI.skin.label){fontSize=size,fontStyle=bold?FontStyle.Bold:FontStyle.Normal,
            alignment=TextAnchor.MiddleCenter,wordWrap=true};
        style.normal.textColor=color;
        style.hover=style.active=style.focused=style.normal;
        GUI.Label(rect,text,style);
    }
    static bool CounterButton(Rect rect,string text){
        Card(rect,Green);
        Letter(rect,text,17,Color.white,true);
        return GUI.Button(rect,GUIContent.none,GUIStyle.none);
    }
    void Icon(int index,Rect rect){
        if(index==2){
            GUI.DrawTexture(rect,gratedCheese,ScaleMode.ScaleToFit);
            return;
        }
        if(order==1 && index>=3){
            GUI.DrawTextureWithTexCoords(rect,calabresaArt,new Rect((index-3)*.5f,0,.5f,1));
            return;
        }
        GUI.DrawTextureWithTexCoords(rect,foodArt,new Rect(index*.25f,0,.25f,1));
    }
    void DrawCounter(){
        LoadCounterArt();
        var old=GUI.matrix;
        GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/1000f,Screen.height/600f,1));
        if(mode==Mode.Pack || mode==Mode.Cut){
            DrawPacking();
            GUI.matrix=old;
            return;
        }
        GUI.DrawTexture(new Rect(0,0,1000,600),counterArt,ScaleMode.StretchToFill);
        Card(new Rect(34,19,550,79),Paper);
        Letter(new Rect(46,22,525,28),"PIZZARIA DI MARCO",17,Green,true);
        Letter(new Rect(46,46,525,43),RecipeName+"  ·  "+SliceCount+" fatias",27,Ink,true);
        Card(new Rect(612,33,137,48),Green);
        int remaining=Mathf.Max(0,Mathf.CeilToInt(deadline-Time.time));
        Letter(new Rect(612,33,137,48),ClockRunning?string.Format("{0}:{1:00}",remaining/60,remaining%60):"Pizzaria",18,Color.white,true);
        Letter(new Rect(773,33,192,48),"Pedido "+(order+1)+" de 2",21,Ink,true);
        for(int i=0;i<ingredients.Length;i++){
            Rect r=TrayRect(i);
            if(i==0){
                Icon(i,new Rect(r.center.x-r.width*.28f,143,r.width*.56f,63));
            }else{
                int bowlIndex=(order==1 && i>=3)?i:i-1;
                float width=Mathf.Min(r.width-4,134f);
                float height=width*110f/180f;
                var bowlRect=new Rect(r.center.x-width/2,211-height,width,height);
                GUI.DrawTextureWithTexCoords(bowlRect,bowlArt,new Rect(bowlIndex*.2f,0,.2f,1));
            }
            Letter(new Rect(r.x,212,r.width,29),ingredients[i],17,Ink,true);
        }
        if(step>0 && mode!=Mode.Bake){
            DrawPie(Pie);
            if(mode==Mode.Review)DrawFinishedCuts(Pie);
        }
        if(mode==Mode.Cook && step>=3 && !ReadyForOven){
            var target=TargetPoint(portion);
            float r=TargetRadius;
            Card(new Rect(target.x-r,target.y-r,2*r,2*r),Green,r);
            Letter(new Rect(target.x-r,target.y-r,2*r,2*r),(portion+1).ToString(),18,Color.white,true);
        }
        if(mode==Mode.Cook && Dosing){DoseCard();DrawDispenser();}
        else Letter(new Rect(716,485,223,34),"FORNO A LENHA",15,Paper,true);
        Card(new Rect(70,544,860,47),Green);
        string instruction=!ReadyForOven?"Arraste "+ingredients[step].ToLower()+(step==0?" para a tábua.":" até o círculo "+(portion+1)+" de "+Portions+"."):"Arraste a pizza pronta para o forno.";
        if(Dosing)instruction=selectedTool==step?"Segure e mova o mouse sobre a pizza. Solte para pausar.":"Selecione "+ingredients[step].ToLower()+" na bandeja para começar.";
        Letter(new Rect(82,545,712,44),mode==Mode.Bake?"Retire a pizza quando o marcador estiver na faixa verde.":notice.Length>0?notice:instruction,18,Color.white,true);
        Letter(new Rect(792,545,128,44),"Esc: menu",14,Paper,true);
        if(mode==Mode.Count){
            Card(new Rect(255,300,490,145),Paper);
            Letter(new Rect(263,312,474,42),"Prepare a bancada",24,Ink,true);
            float left=deadline-Time.time-.5f;
            Letter(new Rect(265,350,470,80),left>0?Mathf.CeilToInt(left).ToString():"Vamos!",48,Green,true);
        }else if(mode==Mode.Review)ReviewCard();
        else if(mode==Mode.Bake)DrawOven();
        else HandleCounterDrag();
        GUI.matrix=old;
    }
    bool ClockRunning => mode==Mode.Cook || mode==Mode.Bake || mode==Mode.Pack || mode==Mode.Cut;
    int SliceCount => order==0 ? 6 : 8;
    int cutsMade;
    bool carryingPizza, cutting;
    Vector2 cutStart;
    static readonly Rect BoxBase=new Rect(510,155,365,365);
    static readonly Rect BoxPie=new Rect(524,169,337,337);

    void CutLine(Vector2 from, Vector2 to, Color color, float width){
        Vector2 delta = to - from;
        if(delta.sqrMagnitude < .001f) return;
        var saved = GUI.matrix;
        var rotation = Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);
        GUI.matrix = saved * Matrix4x4.TRS(from,rotation,Vector3.one);
        Card(new Rect(0,-width/2,delta.magnitude,width),color,0);
        GUI.matrix = saved;
    }

    Vector2 SliceOffset(Rect pizza,int index){
        float angle = -Mathf.PI/2 + index * (2*Mathf.PI/SliceCount);
        // A massa ocupa 224 dos 240 pixels da textura, com margem transparente.
        return new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)) * (pizza.width*112f/240f);
    }

    void DrawFinishedCuts(Rect pizza){
        for(int i=0;i<cutsMade;i++){
            Vector2 offset=SliceOffset(pizza,i);
            CutLine(pizza.center-offset,pizza.center+offset,new Color32(94,49,28,255),2.8f);
        }
    }

    void DrawPacking(){
        var e=Event.current;
        Vector2 mouse=e.mousePosition;
        Card(new Rect(0,0,1000,600),new Color32(171,120,80,255),0);
        for(int i=0;i<7;i++){
            float y=86+i*73;
            Card(new Rect(0,y,1000,2),new Color32(137,89,57,255),0);
            Card(new Rect((i*311+156)%950,y+2,2,71),new Color32(146,100,65,255),0);
        }
        Card(new Rect(0,0,1000,83),Green,0);
        Letter(new Rect(22,10,320,60),mode==Mode.Pack?"Embalagem":"Corte da pizza",29,Paper,true);
        Letter(new Rect(360,10,425,60),RecipeName+"  ·  "+SliceCount+" fatias",23,Paper);
        int seconds=Mathf.Max(0,Mathf.CeilToInt(deadline-Time.time));
        Letter(new Rect(834,10,130,60),string.Format("{0}:{1:00}",seconds/60,seconds%60),27,Paper,true);
        Card(new Rect(516,163,365,365),new Color32(125,84,60,255),7);
        Card(new Rect(515,95,355,72),new Color32(150,99,61,255),8);
        Card(new Rect(521,100,343,61),new Color32(218,177,127,255),6);
        Letter(new Rect(542,108,300,39),"PIZZARIA DI MARCO",18,Ink,true);
        Card(BoxBase,new Color32(143,92,56,255),5);
        Card(new Rect(515,160,355,355),new Color32(223,183,137,255),4);
        Card(new Rect(525,170,335,335),new Color32(250,235,204,255),4);
        Rect bakedPie=new Rect(53,173,330,330);
        Card(new Rect(36,161,366,354),new Color32(133,86,53,255),27);
        Card(new Rect(40,156,358,351),new Color32(223,174,113,255),24);
        string hint;
        if(mode==Mode.Pack){
            DrawPie(carryingPizza ? new Rect(mouse.x-165,mouse.y-165,330,330) : bakedPie);
            hint="Arraste a pizza assada da bancada para dentro da caixa.";
            if(e.type==EventType.MouseDown && e.button==0 && bakedPie.Contains(mouse)){carryingPizza=true;e.Use();}
            else if(carryingPizza && e.type==EventType.MouseUp && e.button==0){
                carryingPizza=false;
                if(BoxBase.Contains(mouse))mode=Mode.Cut;
                e.Use();
            }
        }else{
            Letter(new Rect(58,243,322,64),RecipeName,28,Ink,true);
            Letter(new Rect(58,295,322,42),"Cortes: "+cutsMade+" de "+(SliceCount/2),20,Ink);
            DrawPie(BoxPie);
            int total=SliceCount/2;
            hint="Corte em "+SliceCount+" fatias. Arraste de uma marca à outra.";
            DrawFinishedCuts(BoxPie);
            if(cutsMade==total){
                hint="Pizza cortada em "+SliceCount+" fatias.";
                if(CounterButton(new Rect(96,343,245,51),"Entregar ao chef"))Finish(true);
            }else{
                Vector2 offset=SliceOffset(BoxPie,cutsMade);
                Vector2 a=BoxPie.center-offset, b=BoxPie.center+offset;
                CutLine(a,b,new Color(1,.96f,.8f,.75f),2);
                foreach(var pt in new[]{a,b})Card(new Rect(pt.x-8,pt.y-8,16,16),Green,8);
                if(e.type==EventType.MouseDown && e.button==0 && Mathf.Min(Vector2.Distance(mouse,a),Vector2.Distance(mouse,b))<=17){
                    cutStart=Vector2.Distance(mouse,a)<Vector2.Distance(mouse,b)?a:b;
                    cutting=true;
                    e.Use();
                }
                if(cutting){
                    Vector2 end=cutStart==a?b:a;
                    Vector2 axis=end-cutStart;
                    float progress=Mathf.Clamp01(Vector2.Dot(mouse-cutStart,axis)/axis.sqrMagnitude);
                    CutLine(cutStart,cutStart+axis*progress,Ink,3);
                    if(e.type==EventType.MouseUp && e.button==0){
                        if(Vector2.Distance(mouse,end)<=17)cutsMade++;
                        cutting=false;
                        e.Use();
                    }
                }
            }
        }
        Card(new Rect(70,544,860,47),Green);
        Letter(new Rect(82,545,712,44),hint,18,Color.white,true);
        Letter(new Rect(792,545,128,44),"Esc: menu",14,Paper,true);
    }

    Rect TrayRect(int id){
        int slot=System.Array.IndexOf(tray,Mathf.Min(id,3));
        float x=48+231*slot;
        if(order==1 && id>=3)return new Rect(x+(id-3)*105,134,105,105);
        return new Rect(x,134,210,105);
    }
    void HandleCounterDrag(){
        var e=Event.current;
        if(e.type==EventType.MouseDown && e.button==0){
            for(int i=0;i<ingredients.Length;i++){
                if(TrayRect(i).Contains(e.mousePosition)){if(Dosing && i==step){selectedTool=step;drag=-1;notice="";}
                    else{selectedTool=-1;drag=i;}
                    e.Use();break;}
            }
            if(drag<0 && ReadyForOven && Board.Contains(e.mousePosition)){drag=WholePizza;e.Use();}
        }
        if(drag<0)return;
        var icon=new Rect(e.mousePosition.x-40,e.mousePosition.y-40,80,80);
        if(drag==WholePizza)DrawPie(icon);else Icon(drag,icon);
        if(e.type==EventType.MouseUp && e.button==0){
            DropIngredient(drag,e.mousePosition);
            drag=-1;e.Use();
        }
    }
    void DropIngredient(int id,Vector2 point){
        if(mode!=Mode.Cook)return;
        if(id==WholePizza){if(ReadyForOven && Oven.Contains(point))PutInOven();return;}
        if(!Board.Contains(point))return;
        if(id!=step){errors+=2;notice="Ingrediente trocado (−2). Confira o pedido.";return;}
        if(Dosing)return;
        if(step>0 && Vector2.Distance(point,TargetPoint(portion))>TargetRadius){
            errors++;notice="Fora do círculo (−1). Mire no centro da marca.";return;
        }
        notice="";
        if(step>=3){toppingIds[toppingCount]=id;toppingPoints[toppingCount++]=point;}
        portion++;
        if(portion>=Portions){portion=0;step++;targetStart=(targetStart+1)%8;}
    }
    void ReviewCard(){
        Card(new Rect(0,0,1000,600),new Color(0.16f,.11f,.08f,.63f),0);
        Card(new Rect(217,98,566,404),Paper,14);
        Card(new Rect(240,123,520,49),Green);
        Letter(new Rect(240,123,520,49),"AVALIAÇÃO DO CHEF MARCO",21,Color.white,true);
        bool served=ReadyForOven && !timedOut;
        Letter(new Rect(239,185,522,56),served?"Nota  "+Grade+" / 10":"Pedido fora do prazo",35,Ink,true);
        string review=timedOut?"O pedido não ficou pronto dentro do prazo. Reserve tempo para assar, embalar e cortar.":
            bakeFeedback+" "+(win?"Pedido aprovado, Sofia!":"Vamos refazer a pizza de "+RecipeName+". Confira a receita e o ponto do forno.");
        Letter(new Rect(258,242,484,84),review,20,Ink);
        Letter(new Rect(236,332,528,30),BalanceText(1)+"   |   "+BalanceText(2),15,Ink,true);
        Letter(new Rect(236,362,528,28),"Montagem: −"+errors+"   ·   Equilíbrio: −"+BalancePenalty+"   ·   Forno: −"+ovenPenalty+"   ·   Mínimo: 7",15,Green);
        if(CounterButton(new Rect(327,396,346,54),win?(order==0?"Preparar segundo pedido":"Receber a Ferrari F40"):"Recomeçar o serviço"))AcceptReview();

    }
    bool timedOut;
}}
