using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using CrossingTheWorld.TCCEgypt;
using CrossingTheWorld.TCCBrazil;
using CrossingTheWorld.TCCJapan;
using CrossingTheWorld.TCCMenu;
using Viaggio.Italia;

[InitializeOnLoad]
public static class TCCPlaySmoke
{
    const string SessionKey = "TCC.QA.PlaySmoke.Active";
    static readonly string[] Scenes = {
        TravelMenu.ScenePath,
        TravelMenu.EgyptScene,
        "Assets/TCCEgito/Scenes/Egito_Interior_Jogavel.unity",
        TravelMenu.BrazilScene,
        ItaliaFase.Rua,
        ItaliaFase.Sala,
        JapanQuest.Exterior,
        JapanQuest.Interior
    };
    static readonly List<string> Results = new List<string>();
    static int index, frames, errors;
    static bool loading;
    static TCCPlaySmoke()
    {
        EditorApplication.update += Tick;
    }
    public static void Run()
    {
        index=0;frames=0;errors=0;loading=false;Results.Clear();
        SessionState.SetBool(SessionKey,true);
        EditorSceneManager.OpenScene(Scenes[0],OpenSceneMode.Single);
        EditorApplication.EnterPlaymode();
    }
    static void Tick()
    {
        if (!SessionState.GetBool(SessionKey,false) || !EditorApplication.isPlaying) return;
        Application.runInBackground=true;
        EditorApplication.QueuePlayerLoopUpdate();
        if (loading) return;
        if (++frames < 110) return;
        frames=0;
        try
        {
            var active=SceneManager.GetActiveScene();
            if (active.path != Scenes[index])
                throw new Exception("Cena ativa inesperada: " + active.path);
            var cameras=UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include);
            int activeCameras=0;
            foreach(var c in cameras) if(c.isActiveAndEnabled && c.gameObject.scene==active) activeCameras++;
            if(activeCameras<1) throw new Exception("Nenhuma camera ativa");
            if(index==0 && UnityEngine.Object.FindFirstObjectByType<TravelMenu>()==null) throw new Exception("Menu indisponivel");
            if((index==1 || index==2) && UnityEngine.Object.FindFirstObjectByType<EgyptStage>()==null) throw new Exception("EgyptStage indisponivel");
            if(index==3 && UnityEngine.Object.FindFirstObjectByType<BrazilStage>()==null) throw new Exception("BrazilStage indisponivel");
            if((index==4 || index==5))
            {
                var italy=UnityEngine.Object.FindFirstObjectByType<ItaliaFase>();
                if(italy==null || italy.player==null || italy.animator==null) throw new Exception("Sofia ou ItaliaFase indisponivel");
            }
            if(index>=6)
            {
                var japan=UnityEngine.Object.FindFirstObjectByType<JapanQuest>();
                if(japan==null || japan.player==null || japan.animator==null) throw new Exception("Kenji ou JapanQuest indisponivel");
            }
            Results.Add("OK RUNTIME "+active.path+" camerasAtivas="+activeCameras);
        }
        catch(Exception e) { errors++; Results.Add("FALHA RUNTIME "+Scenes[index]+": "+e); }
        index++;
        if (index>=Scenes.Length)
        {
            SessionState.SetBool(SessionKey,false);
            Results.Add("TOTAL FALHAS: "+errors);
            File.WriteAllLines("C:/Users/User/Documents/TCC_QA_SMOKE_RESULTADO.txt",Results);
            foreach(var item in Results) Debug.Log("TCC_SMOKE "+item);
            EditorApplication.Exit(errors==0?0:1);
        }
        else
        {
            try { SceneManager.LoadScene(Scenes[index]); }
            catch(Exception ex)
            {
                errors++;Results.Add("FALHA TRANSICAO "+Scenes[index]+": "+ex.Message);
                SessionState.SetBool(SessionKey,false);
                File.WriteAllLines("C:/Users/User/Documents/TCC_QA_SMOKE_RESULTADO.txt",Results);
                EditorApplication.Exit(1);
            }
        }
    }
}