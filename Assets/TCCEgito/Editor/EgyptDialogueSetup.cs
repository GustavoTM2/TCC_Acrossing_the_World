using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CrossingTheWorld.TCCEgypt.Editor
{
    public static class EgyptDialogueSetup
    {
        [MenuItem("TCC/Egito/Aplicar falas Horizon Travel")]
        public static void FromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ApplyAndSave();
        }

        public static void ApplyAndSave()
        {
            if (Application.unityVersion != "6000.5.7f1") throw new InvalidOperationException("Use o Unity 6000.5.7f1.");
            foreach (string path in new[] { EgyptPresentationSetup.Exterior, EgyptPresentationSetup.Interior })
            {
                var scene = EditorSceneManager.OpenScene(path);
                var stage = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<EgyptStage>(true)).Single();
                // Atualizo também os textos gravados na cena, não apenas os valores iniciais do script.
                stage.guideLines = EgyptDialogueContent.CreateGuide();
                stage.mummyLines = EgyptDialogueContent.CreateMummy();
                foreach (var point in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<EgyptInteractionPoint>(true)))
                    foreach (var idle in point.GetComponentsInChildren<EgyptNpcIdle>(true))
                    {
                        idle.stage = stage;
                        idle.speaker = point.kind == EgyptPointKind.Mummy
                            ? EgyptDialogueContent.MummySpeaker : EgyptDialogueContent.GuideSpeaker;
                    }
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Não consegui salvar " + path);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("EGYPT_DIALOGUES_APPLIED: 5 caixas do guia e 11 de Rami nas cenas jogáveis.");
        }
    }
}
