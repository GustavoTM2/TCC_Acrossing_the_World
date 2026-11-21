using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CrossingTheWorld.TCCJapan;
using CrossingTheWorld.TCCBrazil;
using CrossingTheWorld.TCCEgypt;

public static class JapanQuestVerification
{
    public static void Run()
    {
        var items=new System.Collections.Generic.List<string>();
        int failed=0;
        Action<bool,string> check=(result,name)=>
        {
            items.Add((result?"OK ":"FALHA ")+name);
            if(!result)failed++;
        };
        try
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var monk=AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/TCCJapao/Resources/JapanModels/MongeOkoizume.fbx");
            var katana=AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/TCCJapao/Resources/JapanModels/KatanaOkoizume.fbx");
            var monkMap=AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/TCCJapao/Resources/JapanModels/MongeOkoizume.png");
            var katanaMap=AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/TCCJapao/Resources/JapanModels/KatanaOkoizume.png");
            var italyGuide=AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/TCCJapao/Resources/GuiaItaliaOriginal.fbx");
            check(italyGuide!=null, "Guia turístico original da Itália importado no Japão");
            if(italyGuide!=null)
                check(italyGuide.GetComponentsInChildren<Renderer>(true).Length>0,
                    "Modelo do guia italiano tem malha renderizável");
            check(monk!=null,"Modelo do monge original importado");
            check(katana!=null,"Modelo da katana original importado");
            check(monkMap!=null,"Textura do monge");
            check(katanaMap!=null,"Textura da katana");
            if(monk!=null)check(monk.GetComponentsInChildren<Renderer>(true).Length>0,"Monge tem malha renderizável");
            if(katana!=null)check(katana.GetComponentsInChildren<Renderer>(true).Length>0,"Katana tem malha renderizável");
            foreach(var path in new[]{JapanQuest.Exterior,JapanQuest.Interior})
            {
                var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                check(scene.IsValid() && scene.isLoaded,"Cena "+path);
                var quest=UnityEngine.Object.FindFirstObjectByType<JapanQuest>();
                check(quest!=null,"JapanQuest "+path);
                if(quest!=null)
                {
                    check(quest.player!=null,"Player associado "+path);
                    check(quest.animator!=null,"Animator associado "+path);
                    check(quest.guide!=null || quest.monk!=null,"NPC associado "+path);
                    if(quest.interior)
                    {
                        check(quest.monk!=null && quest.exit!=null && quest.katana!=null,
                            "Monge, saída e katana associados no templo");
                        check(GameObject.Find("BuddhaHall")!=null,"Salão original BuddhaHall está na cena");
                        if(quest.player!=null && quest.exit!=null)
                            check(Vector3.Distance(quest.player.position,quest.exit.position)<3.5f,
                                "Kenji nasce na porta interna");
                        if(quest.monk!=null && quest.player!=null)
                            check(quest.monk.position.z<quest.player.position.z-5f,
                                "Monge posicionado no fundo do salão, à frente de Kenji");
                        if(quest.katana!=null && quest.monk!=null)
                            check(Vector3.Distance(quest.katana.position,quest.monk.position)<5f,
                                "Katana sobre pedestal próxima ao monge");
                        var interiorCamera=UnityEngine.Object.FindFirstObjectByType<JapanInteriorCamera25D>();
                        check(interiorCamera!=null,"Câmera própria para o Japão interno");
                        if(interiorCamera!=null)
                        {
                            check(interiorCamera.target==quest.player,"Câmera interna segue o Kenji");
                            check(interiorCamera.fieldOfView>=55f,"Enquadramento interno aberto");
                            check(interiorCamera.avoidWalls,"Câmera interna evita paredes do templo");
                        }
                    }
                    else
                        check(quest.guide!=null && quest.door!=null,
                            "Guia turístico e entrada do templo associados");
                    if(!quest.interior && quest.guide!=null)
                    {
                        check(quest.guide.name.Contains("Guia turistico"),"A entrada usa guia turístico humano");
                        var visibleGuide=GameObject.Find("Guia_Turistico_Japao_Original");
                        check(visibleGuide!=null,"Modelo 3D do guia já existe na cena sem entrar em Play");
                        if(visibleGuide!=null)
                        {
                            check(visibleGuide.GetComponentsInChildren<Renderer>(true).Length>0,
                                "Modelo do guia na Hierarchy possui malhas");
                            check(Vector3.Distance(visibleGuide.transform.position,quest.guide.position)<1.0f,
                                "Modelo posicionado no ponto de interação");
                        }
                        if(quest.player!=null)
                            check(Vector3.Distance(quest.guide.position,quest.player.position)<3.5f,
                                "Guia visível próximo ao início da fase");
                    }
                    if(!quest.interior && quest.door!=null && quest.player!=null)
                        check(quest.door.position.z<quest.player.position.z-10f,
                            "Porta externa localizada no templo, à frente do início da fase");
                    if(!quest.interior)
                    {
                        var follower=UnityEngine.Object.FindFirstObjectByType<JapanExteriorCamera25D>();
                        check(follower!=null,"Camera exclusiva para o exterior do Japão");
                        if(follower!=null)
                        {
                            check(follower.target==quest.player,"Camera seguindo o Kenji");
                            check(follower.boundaryRoot!=null,"Camera limitada às paredes invisíveis");
                            check(follower.avoidWalls,"Camera com proteção contra paredes");
                        }
                    }
                }
                check(UnityEngine.Object.FindFirstObjectByType<Camera>()!=null,"Câmera "+path);
            }
        }
        catch(Exception ex)
        {
            failed++;items.Add("EXCEÇÃO "+ex);
        }
        items.Add("TOTAL FALHAS: "+failed);
        var target="C:/Users/User/Documents/TCC_JAPAO_NOVOS_ASSETS_VALIDACAO.txt";
        File.WriteAllLines(target,items);
        foreach(var s in items)Debug.Log("TCC_JAPAO_CHECK "+s);
        EditorApplication.Exit(failed>0?1:0);
    }
}
