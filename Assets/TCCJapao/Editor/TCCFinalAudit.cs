using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using CrossingTheWorld.TCCJapan;

public static class TCCFinalAudit
{
    public static void Run()
    {
        var lines = new List<string>();
        int errors = 0;
        var scenes = new [] {
            "Assets/TCCMenu/Cenas/Menu_Principal.unity",
            "Assets/TCCEgito/Cenas/Egito_Exterior_Jogavel.unity",
            "Assets/TCCEgito/Cenas/Egito_Interior_Jogavel.unity",
            "Assets/TCCBrasil/Cenas/Brasil_Praia_Jogavel.unity",
            "Assets/TCCItalia/Cenas/ITALIA_RUA.unity",
            "Assets/TCCItalia/Cenas/ITALIA_RESTAURANTE.unity",
            JapanQuest.Exterior, JapanQuest.Interior
        };
        foreach (var path in scenes)
        {
            try
            {
                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (!asset) throw new Exception("Arquivo da cena ausente");
                var scene = EditorSceneManager.OpenScene(path);
                int roots = scene.rootCount;
                int cams = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Camera>(true)).Count();
                int animators = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Animator>(true)).Count();
                lines.Add("OK CENA " + path + " / roots=" + roots + " cameras=" + cams + " animators=" + animators);
                if (path == JapanQuest.Exterior || path == JapanQuest.Interior)
                {
                    var quest = UnityEngine.Object.FindFirstObjectByType<JapanQuest>();
                    if (!quest || !quest.player || !quest.animator) throw new Exception("Missao, jogador ou Animator nao conectado");
                    if (path == JapanQuest.Exterior && (!quest.guide || !quest.door)) throw new Exception("Guia/entrada nao conectados");
                    if (path == JapanQuest.Interior && (!quest.monk || !quest.exit || !quest.katana)) throw new Exception("Monge/saida/katana nao conectados");
                    lines.Add("OK MISSAO JAPAO " + (quest.interior ? "INTERIOR" : "EXTERIOR"));
                }
            }
            catch (Exception ex) { errors++; lines.Add("ERRO CENA " + path + " : " + ex.Message); }
        }
        var controllers = new [] {
            "Assets/TCCExploracao/Gerados/Kenji.controller",
            "Assets/TCCExploracao/Gerados/Sofia.controller",
            "Assets/TCCBrasil/Gerados/Lucas_Movimento.controller",
            "Assets/TCCEgito/Gerados/Amira_Movimento.controller",
            "Assets/TCCItalia/Programacao/Resources/ItaliaPronta/SofiaRestaurante.controller"
        };
        foreach (var path in controllers)
        {
            try
            {
                var c = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                if (!c) throw new Exception("AnimatorController ausente");
                var states = c.layers[0].stateMachine.states.Select(s => s.state).ToArray();
                var idle = states.FirstOrDefault(s=>s.name.ToLowerInvariant().Contains("parad") || s.name.ToLowerInvariant().Contains("repous") || s.name.ToLowerInvariant().Contains("idle"));
                if (idle == null || idle.motion == null) throw new Exception("Estado idle nao possui animacao");
                if (c.layers[0].stateMachine.defaultState != idle) throw new Exception("Idle nao e estado inicial");
                lines.Add("OK IDLE " + path + " / " + idle.motion.name);
            }
            catch (Exception ex) { errors++; lines.Add("ERRO IDLE " + path + " : " + ex.Message); }
        }
        foreach (var path in new[]{JapanQuest.Exterior,JapanQuest.Interior})
            if (!EditorBuildSettings.scenes.Any(s=>s.path==path && s.enabled)) { errors++; lines.Add("ERRO BUILD " + path); }
        lines.Add("TOTAL ERROS: " + errors);
        var dest = "C:/Users/User/Documents/TCC_FINAL_RELATORIO_VALIDACAO.txt";
        File.WriteAllLines(dest, lines);
        foreach (var line in lines) Debug.Log("TCC_AUDIT " + line);
        EditorApplication.Exit(errors==0 ? 0 : 1);
    }
}