using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using CrossingTheWorld.TCCMenu;
using Object=UnityEngine.Object;
namespace CrossingTheWorld.TCCBrazil.Editor {
 [InitializeOnLoad]
 public static class BrazilKickValidation {
  const string Key="TCC.Brazil.KickValidation";static int frames,last;
  static BrazilKickValidation(){EditorApplication.update+=Tick;}
  public static void RunAndLeaveOpen(){SessionState.SetBool(Key+".LeaveOpen",true);Run();}
  public static void Run(){EditorSceneManager.OpenScene(BrazilSceneSetup.ScenePath);SessionState.SetBool(Key,true);EditorApplication.EnterPlaymode();}
  static void Require(bool value,string message){if(!value)throw new Exception(message);}
  static void Tick(){
   if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying)return;
   Application.runInBackground=true;EditorApplication.QueuePlayerLoopUpdate();if(last==Time.frameCount)return;last=Time.frameCount;if(++frames<30)return;
   SessionState.SetBool(Key,false);
   try {
    var stage=Object.FindFirstObjectByType<BrazilStage>();stage.enabled=false;stage.player.enabled=false;
    TravelProgress.KeyPrefix="CrossingTheWorld.Tests.Kick.";TravelProgress.Reset();
    stage.OpenDialogue(true);while(stage.State==BrazilState.Dialogue)stage.AdvanceDialogue();stage.OpenDialogue(false);while(stage.State==BrazilState.Dialogue)stage.AdvanceDialogue();
    stage.StartChallenge();var pose=stage.kickPose;pose.enabled=false;pose.UpdatePresentation(0);stage.TickChallenge(3,false);pose.UpdatePresentation(0);
    float maxStep=0;int ticks=0;
    while(stage.State==BrazilState.Playing && ticks++<4000) {
     float target=Mathf.Lerp(stage.Challenge.WindowStart,stage.Challenge.WindowEnd,(stage.Challenge.Score%3)*.5f);
     float remain=stage.Challenge.PreparationRemaining+Mathf.Max(0,target-stage.Challenge.Phase)*BrazilTimingChallenge.Duration;
     float dt=Mathf.Min(1f/120,remain);
     Vector3 before=stage.ball.position;stage.TickChallenge(dt,false);pose.UpdatePresentation(dt);maxStep=Mathf.Max(maxStep,Vector3.Distance(before,stage.ball.position));
     if(remain<=1f/120+.00001f) {
      Require(Vector3.Distance(stage.ball.position,pose.ContactPoint)<.015f,"Bola fora do pé na faixa verde");
      if(stage.Challenge.Score==3) Capture("contato",stage);
      before=stage.ball.position;stage.TickChallenge(0,true);pose.UpdatePresentation(0);
      Require(Vector3.Distance(before,stage.ball.position)<.06f,"Salto visual após acerto");
     }
    }
    Require(stage.HasPrize && stage.AwardCount==1,"Dez acertos e prêmio único");Require(maxStep<.15f,"Movimento brusco da bola: "+maxStep);
    stage.ContinueExploring();pose.UpdatePresentation(.3f);Require(stage.player.animator.enabled,"Animator não restaurado");
    File.WriteAllText("C:/Users/Gustavo/Downloads/tcc/brasil-sincronia-validacao.txt","APROVADO: dez contatos no início/meio/fim das faixas, sem salto no acerto, prêmio único, Animator restaurado. Passo máximo da bola: "+maxStep+". Pé: "+(pose.foot!=null?pose.foot.name:"posição de apoio"));
    TravelProgress.Reset();
    if(SessionState.GetBool(Key+".LeaveOpen",false)) {
     SessionState.SetBool(Key+".LeaveOpen",false);TravelProgress.KeyPrefix="CrossingTheWorld.V1.";
     UnityEngine.SceneManagement.SceneManager.LoadScene(BrazilSceneSetup.ScenePath);
     var game=EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView"));game.Show();game.Focus();Debug.Log("BRAZIL_USER_PLAY_READY");
    } else EditorApplication.Exit(0);
   }catch(Exception e){Debug.LogException(e);File.WriteAllText("C:/Users/Gustavo/Downloads/tcc/brasil-sincronia-falha.txt",e.ToString());EditorApplication.Exit(1);}
  }
  static void Capture(string name,BrazilStage stage){
   Camera c=Camera.main;c.transform.position=stage.player.transform.position+stage.followCamera.offset;c.transform.LookAt(stage.player.transform.position+stage.followCamera.lookOffset);var rt=new RenderTexture(1280,720,24);var old=RenderTexture.active;c.targetTexture=rt;c.Render();RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes("C:/Users/Gustavo/Downloads/tcc/brasil-"+name+".png",image.EncodeToPNG());c.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(image);Object.DestroyImmediate(rt);
  }
  public static void PlayForUser(){EditorSceneManager.OpenScene(BrazilSceneSetup.ScenePath);var game=EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView"));game.Show();game.Focus();EditorApplication.EnterPlaymode();}
 }
}
