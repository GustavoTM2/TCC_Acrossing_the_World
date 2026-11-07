using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using CrossingTheWorld.TCCBrazil;
using CrossingTheWorld.TCCMenu;
using CrossingTheWorld.TCCEgypt;
[InitializeOnLoad]
public static class ExplorationValidation {
 static int phase,frames,lastFrame;const string Key="TCC.Exploration.Validation";
 static ExplorationValidation(){EditorApplication.update+=Tick;}
 public static void Run(){SessionState.SetBool(Key,true);EditorSceneManager.OpenScene(ExplorationSetup.Japan);EditorApplication.EnterPlaymode();}
 static void Tick(){
  if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EgyptSceneTransition.IsBusy)return;
  Application.runInBackground=true;EditorApplication.QueuePlayerLoopUpdate();if(lastFrame==Time.frameCount)return;lastFrame=Time.frameCount;if(++frames<90)return;frames=0;
  try {
   if(phase<2){
    var p=UnityEngine.Object.FindFirstObjectByType<BrazilPlayer25D>();if(p==null)throw new Exception("Jogador ausente");p.enabled=false;var start=p.transform.position;
    foreach(var v in new[]{Vector2.right,Vector2.left,Vector2.up,Vector2.down}) {
     p.PlaceAt(start);Physics.SyncTransforms();for(int i=0;i<30;i++)p.TickMovement(v,1f/60);
     var delta=p.transform.position-start;delta.y=0;if(delta.magnitude<.3f)throw new Exception("Movimento bloqueado "+p.name+" "+v);
     if(!UnityEngine.Object.FindFirstObjectByType<EgyptMovementBounds>().Contains(p.transform.position))throw new Exception("Fora dos limites");
    }
    p.PlaceAt(start);p.enabled=true;
    Capture(phase==0?"Japao":"Italia");
    if(phase==0) SceneManager.LoadScene(ExplorationSetup.Italy);else SceneManager.LoadScene(TravelMenu.ScenePath);
   }else if(phase==2){var m=UnityEngine.Object.FindFirstObjectByType<TravelMenu>();m.ShowDestinations();m.ClickOption(3,true);}
   else if(phase==3){if(SceneManager.GetActiveScene().path!=ExplorationSetup.Japan)throw new Exception("Menu não abriu Japão");SceneManager.LoadScene(TravelMenu.ScenePath);}
   else if(phase==4){var m=UnityEngine.Object.FindFirstObjectByType<TravelMenu>();m.ShowDestinations();m.ClickOption(2,true);}
   else {if(SceneManager.GetActiveScene().path!=ExplorationSetup.Italy)throw new Exception("Menu não abriu Itália");File.WriteAllText("C:/Users/Gustavo/Downloads/tcc/exploracao-validacao.txt","APROVADO: Kenji e Sofia, movimentos nos quatro sentidos, chão, limites e acesso aos dois países pelo menu.");SessionState.SetBool(Key,false);EditorApplication.Exit(0);}
   phase++;
  }catch(Exception e){Debug.LogException(e);File.WriteAllText("C:/Users/Gustavo/Downloads/tcc/exploracao-falha.txt",e.ToString());SessionState.SetBool(Key,false);EditorApplication.Exit(1);}
 }
 static void Capture(string name){var cam=Camera.main;var rt=new RenderTexture(1280,720,24);var old=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var img=new Texture2D(1280,720,TextureFormat.RGB24,false);img.ReadPixels(new Rect(0,0,1280,720),0,0);img.Apply();File.WriteAllBytes("C:/Users/Gustavo/Downloads/tcc/"+name+"-exploracao.png",img.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(img);UnityEngine.Object.DestroyImmediate(rt);}
}