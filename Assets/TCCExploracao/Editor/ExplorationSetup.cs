using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using CrossingTheWorld.TCCBrazil;
using CrossingTheWorld.TCCEgypt;
using CrossingTheWorld.TCCMenu;
using CrossingTheWorld.TCCExploration;
using Object=UnityEngine.Object;
public static class ExplorationSetup {
 const string Root="Assets/TCCExploracao/";
 public const string Japan=Root+"Scenes/Japao_Exploracao.unity";
 public const string Italy=Root+"Scenes/Italia_Exploracao.unity";
 public static void Build() {
  BuildOne(true); BuildOne(false); UpdateMenu(); AssetDatabase.SaveAssets();
  File.WriteAllText("C:/Users/Gustavo/Downloads/tcc/exploracao-build-ok.txt","Kenji e Sofia preparados na Unity "+Application.unityVersion);
 }
 static void BuildOne(bool japan) {
  string target=japan?Japan:Italy;string source=japan?"Assets/Scenes/Japao.unity":"Assets/Scenes/Itália.unity";
  // A cena original da equipe permanece como referência.
  if(AssetDatabase.LoadAssetAtPath<SceneAsset>(target)!=null) throw new InvalidOperationException("Cena já preparada: "+target);
  AssetDatabase.CopyAsset(source,target);var scene=EditorSceneManager.OpenScene(target);
  foreach(var filter in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)) {
   if(filter.sharedMesh==null || filter.GetComponent<Collider>()!=null) continue;
   filter.gameObject.AddComponent<MeshCollider>().sharedMesh=filter.sharedMesh;
  }
  Physics.SyncTransforms();
  Vector3 spawn=japan?new Vector3(371.5f,0,957.5f):new Vector3(692.8f,0,550);
  if(!Physics.Raycast(spawn+Vector3.up*100,Vector3.down,out RaycastHit hit,200)) throw new InvalidOperationException("Chão não encontrado.");
  spawn.y=hit.point.y+.04f;
  var root=new GameObject("Exploracao_"+(japan?"Japao":"Italia"));
  root.AddComponent<ExplorationStage>().title=japan?"JAPÃO / TEMPLO":"ITÁLIA / PONTES";
  var player=Character(japan?"Kenji":"Sofia",spawn,root.transform);
  player.horizontalAxis=japan?Vector3.left:Vector3.forward;player.depthAxis=japan?Vector3.back:Vector3.left;
  player.visual.rotation=Quaternion.LookRotation(player.horizontalAxis);player.respawnBelowY=spawn.y-25;
  var limits=new GameObject("Paredes_Invisiveis");limits.transform.SetParent(root.transform);
  limits.transform.position=japan?new Vector3(371.5f,spawn.y,950):new Vector3(692.8f,spawn.y,520);
  limits.transform.rotation=Quaternion.LookRotation(player.depthAxis);
  var bounds=limits.AddComponent<EgyptMovementBounds>();bounds.horizontalMin=japan?-14:-85;bounds.horizontalMax=japan?14:65;
  bounds.frontDistance=japan?10:8;bounds.backDistance=japan?11:8;bounds.wallHeight=80;bounds.wallBase=-40;bounds.UpdateColliders();
  foreach(var c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) {c.enabled=false;c.tag="Untagged";foreach(var a in c.GetComponents<AudioListener>()) a.enabled=false;}
  var go=new GameObject("Camera_Exploracao");go.transform.SetParent(root.transform);go.tag="MainCamera";
  var camera=go.AddComponent<Camera>();camera.fieldOfView=60;camera.nearClipPlane=.15f;camera.farClipPlane=2000;go.AddComponent<AudioListener>();
  var follow=go.AddComponent<EgyptCamera25D>();follow.target=player.transform;follow.offset=japan?new Vector3(0,5.5f,7):new Vector3(14,6,0);
  follow.lookOffset=new Vector3(0,1,0);follow.depthAxis=player.depthAxis;follow.depthFollow=0;
  camera.transform.position=spawn+follow.offset;camera.transform.LookAt(spawn+follow.lookOffset);
  Physics.SyncTransforms();
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
  var builds=EditorBuildSettings.scenes.ToList();if(!builds.Any(s=>s.path==target)) builds.Add(new EditorBuildSettingsScene(target,true));EditorBuildSettings.scenes=builds.ToArray();
  Capture(camera,japan?"Japao":"Italia");
  Debug.Log("EXPLORATION_BUILT "+target+" spawn="+spawn);
 }
 static BrazilPlayer25D Character(string name,Vector3 position,Transform parent) {
  string art=Root+"Art/"+name+"/", generated=Root+"Generated/"+name;
  string modelPath=art+name+"_Caminhada.fbx";
  var importer=(ModelImporter)AssetImporter.GetAtPath(modelPath);importer.animationType=ModelImporterAnimationType.Generic;importer.importAnimation=true;importer.isReadable=true;importer.SaveAndReimport();
  var root=new GameObject(name+"_Jogador");root.transform.SetParent(parent);root.transform.position=position;root.tag="Player";
  var cc=root.AddComponent<CharacterController>();cc.height=1.8f;cc.radius=.28f;cc.center=new Vector3(0,.9f,0);cc.skinWidth=.025f;cc.stepOffset=.3f;cc.slopeLimit=50;
  var player=root.AddComponent<BrazilPlayer25D>();
  var pivot=new GameObject("Visual_"+name);pivot.transform.SetParent(root.transform,false);
  var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));model.transform.SetParent(pivot.transform,false);
  foreach(var c in model.GetComponentsInChildren<Collider>()) c.enabled=false;
  foreach(var c in model.GetComponentsInChildren<Camera>()) c.enabled=false;
  foreach(var l in model.GetComponentsInChildren<Light>()) l.enabled=false;
  var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(art+(name=="Kenji"?"kenji1":"sofia")+".png"));material.SetFloat("_Smoothness",.15f);AssetDatabase.CreateAsset(material,generated+".mat");
  foreach(var r in model.GetComponentsInChildren<Renderer>()) r.sharedMaterials=Enumerable.Repeat(material,r.sharedMaterials.Length).ToArray();
  var box=BoundsOf(model);model.transform.localScale*=1.8f/box.size.y;box=BoundsOf(model);
  model.transform.position+=new Vector3(position.x-box.center.x,position.y-box.min.y-.025f,position.z-box.center.z);
  var animator=model.GetComponentInChildren<Animator>()??model.AddComponent<Animator>();
  var clip=Object.Instantiate(AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>().First(c=>!c.name.StartsWith("__preview")));
  var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;settings.keepOriginalPositionXZ=true;settings.keepOriginalPositionY=true;AnimationUtility.SetAnimationClipSettings(clip,settings);AssetDatabase.CreateAsset(clip,generated+"_Caminhada.anim");
  var controller=AnimatorController.CreateAnimatorControllerAtPath(generated+".controller");controller.AddParameter("Speed",AnimatorControllerParameterType.Float);var sm=controller.layers[0].stateMachine;
  var idle=sm.AddState("Parado");idle.motion=clip;idle.speed=0;var walk=sm.AddState("Caminhando");walk.motion=clip;sm.defaultState=idle;
  var start=idle.AddTransition(walk);start.hasExitTime=false;start.duration=.12f;start.AddCondition(AnimatorConditionMode.Greater,.05f,"Speed");var stop=walk.AddTransition(idle);stop.hasExitTime=false;stop.duration=.12f;stop.AddCondition(AnimatorConditionMode.Less,.05f,"Speed");
  animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;player.animator=animator;player.visual=pivot.transform;return player;
 }
 static Bounds BoundsOf(GameObject go) {var rs=go.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs.Skip(1)) b.Encapsulate(r.bounds);return b;}
 static void Capture(Camera camera,string name) {
  var rt=new RenderTexture(1280,720,24);var old=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
  var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
  File.WriteAllBytes(Root+"Generated/"+name+"_Previa.png",image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(image);Object.DestroyImmediate(rt);AssetDatabase.Refresh();
 }
 static void UpdateMenu() {
  EditorSceneManager.OpenScene(TravelMenu.ScenePath);var menu=Object.FindFirstObjectByType<TravelMenu>();
  for(int i=2;i<4;i++) {var picture=menu.cards[i].Find("Previa").GetComponent<UnityEngine.UI.RawImage>();picture.texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Generated/"+(i==2?"Italia":"Japao")+"_Previa.png");picture.uvRect=new Rect(0,0,1,1);picture.color=Color.white;menu.countryStatus[i].text="EXPLORAR";menu.cards[i].Find("Local").GetComponent<UnityEngine.UI.Text>().text=i==2?"Pontes":"Templo";}
  EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();
 }
 public static void PlayJapan() {
 var scene=EditorSceneManager.OpenScene(Japan);
 var follow=Object.FindFirstObjectByType<EgyptCamera25D>();follow.offset=new Vector3(0,5.5f,7);follow.GetComponent<Camera>().fieldOfView=55;
 follow.transform.position=follow.target.position+follow.offset;follow.transform.LookAt(follow.target.position+follow.lookOffset);
 var walls=Object.FindFirstObjectByType<EgyptMovementBounds>();walls.frontDistance=10;walls.UpdateColliders();
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 var game=EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView"));game.Show();game.Focus();
 EditorApplication.EnterPlaymode();Debug.Log("EXPLORATION_USER_PLAY_REQUESTED");
}
}
