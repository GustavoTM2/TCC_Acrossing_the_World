using UnityEngine;
using CrossingTheWorld.TCCMenu;
using CrossingTheWorld.TCCEgypt;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace CrossingTheWorld.TCCExploration {
 // Interface simples das fases que ainda estão em exploração.
 public sealed class ExplorationStage : MonoBehaviour {
  public string title;
  private void Update() {
#if ENABLE_INPUT_SYSTEM
   if(Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ReturnToMenu();
#elif ENABLE_LEGACY_INPUT_MANAGER
   if(Input.GetKeyDown(KeyCode.Escape)) ReturnToMenu();
#endif
  }
  public void ReturnToMenu() { if(!EgyptSceneTransition.IsBusy) EgyptSceneTransition.Load(TravelMenu.ScenePath,null,"Voltando ao menu..."); }
  private void OnGUI() {
   if(EgyptSceneTransition.IsBusy) return;
   var previous=GUI.matrix; GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1600f,Screen.height/900f,1));
   var label=new GUIStyle(GUI.skin.label){fontSize=22,fontStyle=FontStyle.Bold}; label.normal.textColor=new Color(1,.76f,.37f);
   GUI.Box(new Rect(40,30,570,92),GUIContent.none); GUI.Label(new Rect(58,42,540,32),title,label);
   label.fontSize=17;label.fontStyle=FontStyle.Normal;label.normal.textColor=Color.white;
   GUI.Label(new Rect(58,78,540,30),"Explore o cenário com o personagem.",label);
   GUI.Box(new Rect(40,830,650,42),GUIContent.none);GUI.Label(new Rect(55,835,625,30),"A/D ou setas: lados   •   W/S: fundo e frente   •   Esc: menu",label);
   if(GUI.Button(new Rect(1350,30,210,45),"Voltar ao menu")) ReturnToMenu();GUI.matrix=previous;
  }
 }
}