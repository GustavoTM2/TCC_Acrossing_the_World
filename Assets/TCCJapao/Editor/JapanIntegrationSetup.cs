using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using CrossingTheWorld.TCCBrazil;
using CrossingTheWorld.TCCJapan;
using CrossingTheWorld.TCCEgypt;

public static class JapanIntegrationSetup
{
    private static readonly string OriginalOutside = "Assets/TCCExploracao/Scenes/Japao_Exploracao.unity";
    private static readonly string OriginalInside = "Assets/Scenes/Japao (interior) 1.unity";

    public static void Build()
    {
        if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(JapanQuest.Exterior))
            if (!AssetDatabase.CopyAsset(OriginalOutside, JapanQuest.Exterior)) throw new Exception("Falha ao duplicar exterior.");
        if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(JapanQuest.Interior))
            if (!AssetDatabase.CopyAsset(OriginalInside, JapanQuest.Interior)) throw new Exception("Falha ao duplicar interior.");
        var outside = EditorSceneManager.OpenScene(JapanQuest.Exterior, OpenSceneMode.Single);
        var outsidePlayer = UnityEngine.Object.FindFirstObjectByType<BrazilPlayer25D>();
        if (!outsidePlayer) throw new Exception("Kenji nao foi encontrado na cena exterior.");
        var playerGo = outsidePlayer.gameObject;
        var originalCamera = UnityEngine.Object.FindFirstObjectByType<EgyptCamera25D>();
        if (!originalCamera) throw new Exception("Camera de acompanhamento nao encontrada.");
        if (!UnityEngine.Object.FindFirstObjectByType<JapanQuest>())
        {
            var root = new GameObject("Missao_Japao_Exterior");
            var quest = root.AddComponent<JapanQuest>();
            quest.player = playerGo.transform;
            quest.animator = playerGo.GetComponentInChildren<Animator>();
            quest.guide = Marker("Guia turistico - conversar", playerGo.transform.position + Vector3.left * 3.4f, PrimitiveType.Capsule, new Color(.25f,.6f,.45f), root.transform);
            quest.door = Marker("Entrada do templo - interagir", playerGo.transform.position + Vector3.left * 9f, PrimitiveType.Cube, new Color(.82f,.55f,.21f), root.transform);
            quest.door.localScale = new Vector3(1.3f,2.7f,.35f);
            quest.door.position += Vector3.up * 1.3f;
        }
        EditorSceneManager.MarkSceneDirty(outside);
        EditorSceneManager.SaveScene(outside);

        var inside = EditorSceneManager.OpenScene(JapanQuest.Interior, OpenSceneMode.Additive);
        if (!inside.GetRootGameObjects().Any(x => x.name == "Missao_Japao_Interior"))
        {
            var currentCameras = inside.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<Camera>(true)).ToArray();
            foreach (var c in currentCameras)
            {
                c.enabled = false; c.tag = "Untagged";
                foreach(var listener in c.GetComponents<AudioListener>()) listener.enabled = false;
            }
            var root = new GameObject("Missao_Japao_Interior");
            SceneManager.MoveGameObjectToScene(root,inside);
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Chao Jogavel do Templo";
            floor.transform.SetParent(root.transform);
            floor.transform.position = new Vector3(0,-.14f,0);
            floor.transform.localScale = new Vector3(24,.3f,20);
            floor.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(.29f,.29f,.25f));
            var player = UnityEngine.Object.Instantiate(playerGo);
            player.name = "Kenji_Jogador_Interior";
            player.transform.SetParent(null,true);
            SceneManager.MoveGameObjectToScene(player,inside);
            player.transform.position = Vector3.up*.1f;
            player.GetComponent<BrazilPlayer25D>().enabled = true;
            var followGO = UnityEngine.Object.Instantiate(originalCamera.gameObject);
            followGO.name = "Camera_Japao_Interior";
            followGO.transform.SetParent(null,true);
            SceneManager.MoveGameObjectToScene(followGO, inside);
            followGO.tag="MainCamera";
            var follow = followGO.GetComponent<EgyptCamera25D>();
            follow.target = player.transform;
            follow.offset = new Vector3(0,4.2f,8.5f);
            follow.depthFollow = 0;
            followGO.transform.position = player.transform.position + follow.offset;
            followGO.transform.LookAt(player.transform.position+Vector3.up);
            var cam = followGO.GetComponent<Camera>();cam.enabled=true;cam.fieldOfView=60;
            var audio = followGO.GetComponent<AudioListener>(); if (audio) audio.enabled=true;
            var quest = root.AddComponent<JapanQuest>();
            quest.interior=true;
            quest.player=player.transform;
            quest.animator=player.GetComponentInChildren<Animator>();
            quest.monk=Marker("Monge - conversar",new Vector3(4,0,1),PrimitiveType.Capsule,new Color(.65f,.41f,.18f),root.transform);
            quest.exit=Marker("Saida do templo",new Vector3(-5,0,0),PrimitiveType.Cube,new Color(.3f,.49f,.66f),root.transform);
            quest.exit.localScale=new Vector3(1.2f,2.5f,.5f);
            quest.exit.position+=Vector3.up*1.25f;
            var prize=new GameObject("Katana - recompensa").transform;
            prize.SetParent(root.transform);
            prize.position=new Vector3(4,1.3f,2.3f);
            var blade=Marker("Lamina",prize.position,PrimitiveType.Cube,new Color(.8f,.86f,.94f),prize);
            blade.localPosition=new Vector3(0,.55f,0);blade.localScale=new Vector3(.09f,1.1f,.04f);
            var handle=Marker("Cabo",prize.position,PrimitiveType.Cylinder,new Color(.12f,.12f,.15f),prize);
            handle.localPosition=new Vector3(0,-.27f,0);handle.localScale=new Vector3(.09f,.35f,.09f);
            var guard=Marker("Guarda",prize.position,PrimitiveType.Cube,new Color(.8f,.62f,.18f),prize);
            guard.localPosition=new Vector3(0,-.03f,0);guard.localScale=new Vector3(.38f,.07f,.12f);
            quest.katana=prize;
        }
        EditorSceneManager.MarkSceneDirty(inside);
        EditorSceneManager.SaveScene(inside);
        var scenes=EditorBuildSettings.scenes.ToList();
        foreach(var path in new[]{JapanQuest.Exterior,JapanQuest.Interior})
        {
            if(!scenes.Any(s=>s.path==path)) scenes.Add(new EditorBuildSettingsScene(path,true));
        }
        EditorBuildSettings.scenes=scenes.ToArray();
        AssetDatabase.SaveAssets();
        EditorSceneManager.CloseScene(inside,true);
        EditorSceneManager.OpenScene(JapanQuest.Exterior,OpenSceneMode.Single);
        Debug.Log("JAPAO_INTEGRACAO_OK: exterior, interior, Kenji, guia, monge, saida e katana");
    }
    private static Transform Marker(string name, Vector3 position,PrimitiveType primitive,Color color,Transform parent)
    {
        var go=GameObject.CreatePrimitive(primitive);
        go.name=name;
        go.transform.SetParent(parent,true);
        go.transform.position=position;
        var c=go.GetComponent<Collider>();if(c)c.enabled=false;
        go.GetComponent<Renderer>().sharedMaterial=CreateMaterial(color);
        return go.transform;
    }
    private static Material CreateMaterial(Color color)
    {
        var shader=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat=new Material(shader);
        mat.color=color;
        return mat;
    }
}