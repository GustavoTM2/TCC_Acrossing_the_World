using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CrossingTheWorld.TCCEgypt;
using Object = UnityEngine.Object;

namespace CrossingTheWorld.TCCBrazil.Editor
{
    // Valida o fluxo completo em Play, incluindo tentativas, prêmio e paredes da praia.
    [InitializeOnLoad]
    public static class BrazilAdventureValidation
    {
        private const string Key = "TCCBrasil.AdventureValidation";
        private static BrazilStage stage;
        private static int step, frames;
        private static int lastGameFrame;
        private static bool ending;
        private static string Output => Environment.GetEnvironmentVariable("TCC_BRAZIL_VALIDATION_OUTPUT")
            ?? Path.GetFullPath(Path.Combine(Application.dataPath,"../Temp/BrazilValidation"));
        static BrazilAdventureValidation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Key,false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode)
                {
                    stage = Object.FindAnyObjectByType<BrazilStage>();
                    step = 0; frames = 0; ending = false;
                    lastGameFrame = -1;
                    Application.runInBackground = true;
                    stage.enabled = false; stage.player.enabled = false;
                }
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    SessionState.SetBool(Key,false);
                    Debug.Log("BRAZIL_ADVENTURE_PLAY_OK");
                    EditorApplication.Exit(0);
                }
            };
        }

        public static void Run()
        {
            Require(Application.unityVersion == "6000.5.7f1", "Versão da Unity");
            TestEgyptConversations();
            TestTiming();
            EditorSceneManager.OpenScene(BrazilSceneSetup.ScenePath);
            BrazilAdventureSetup.ValidateScene();
            Directory.CreateDirectory(Output);
            SessionState.SetBool(Key,true);
            EditorApplication.EnterPlaymode();
        }
        public static void BuildAndRun()
        {
            BrazilAdventureSetup.Build();
            Run();
        }
        public static void TestTiming()
        {
            var challenge = new BrazilTimingChallenge();
            challenge.Begin(); challenge.Tick(.1f,true);
            Require(challenge.Finished && !challenge.Won && challenge.Score == 0, "Toque adiantado deve falhar");
            challenge.Begin(); challenge.Tick(2,false);
            Require(challenge.Finished && !challenge.Won, "Não tocar deve deixar a bola cair");
            challenge.Begin(); challenge.Tick(TimeToCenter(challenge),true); challenge.Tick(.01f,true);
            Require(challenge.Finished && challenge.Score == 1 && !challenge.Won, "Apertar repetidamente não pode vencer");
            // Muitas sequências verificam se cada faixa cabe na barra e realmente muda entre os acertos.
            for(int seed=0;seed<200;seed++)
            {
                challenge.Begin(seed);
                float previousStart=-1,previousWidth=-1;
                for(int i=0;i<10;i++)
                {
                    float start=challenge.WindowStart, width=challenge.WindowEnd-start;
                    Require(start>=.18f && challenge.WindowEnd<=.991f && width>=.149f && width<=.261f,"Faixa alcançável dentro da barra");
                    if(i>0)
                    {
                        Require(Mathf.Abs(start-previousStart)>.1f,"Posição precisa mudar após o acerto");
                        Require(Mathf.Abs(width-previousWidth)>.014f,"Largura precisa mudar após o acerto");
                    }
                    challenge.Tick(.1f,false);
                    Require(challenge.IsPreparing && challenge.Phase==0 && challenge.WindowStart==start,"Tempo para observar a nova faixa");
                    Require(challenge.Tick(TimeToCenter(challenge),true),"Toque válido " + i);
                    previousStart=start; previousWidth=width;
                }
                Require(challenge.Won && challenge.Score==10,"Dez toques vencem");
                challenge.Tick(10,true); Require(challenge.Score==10,"O placar para em dez");
            }
            challenge.Begin(); challenge.Tick(challenge.PreparationRemaining + BrazilTimingChallenge.Duration*(challenge.WindowStart-.01f),true);
            Require(!challenge.Won && challenge.Finished, "Limite anterior à faixa");
            challenge.Begin(); challenge.Tick(challenge.PreparationRemaining + BrazilTimingChallenge.Duration*(challenge.WindowEnd+.01f),true);
            Require(!challenge.Won && challenge.Finished, "Limite posterior à faixa");
            Debug.Log("BRAZIL_TIMING_OK: 200 sequências com posições e larguras variadas; cedo, tarde, ausência de toque, repetição e dez acertos.");
        }

        private static float TimeToCenter(BrazilTimingChallenge challenge) => challenge.PreparationRemaining
            + (challenge.WindowCenter-challenge.Phase)*BrazilTimingChallenge.Duration;

        private static void TestEgyptConversations()
        {
            var root = new GameObject("ValidacaoConversasEgito") { hideFlags = HideFlags.HideAndDontSave };
            EgyptStage.Progress.Reset();
            try
            {
                var egypt = root.AddComponent<EgyptStage>();
                int guideEvents = 0, awards = 0, completions = 0;
                egypt.onGuideFinished.AddListener(() => guideEvents++);
                egypt.onScarabReceived.AddListener(() => awards++);
                egypt.onStageCompleted.AddListener(() => completions++);
                egypt.Interact(EgyptPointKind.Guide);
                for (int i = 0; i < egypt.guideLines.Length; i++) egypt.AdvanceDialogue();
                Require(guideEvents == 1 && EgyptStage.Progress.Step == EgyptQuestStep.EnterPyramid, "Egito: primeira conversa libera a pirâmide");
                for (int i = 0; i < 2; i++)
                {
                    egypt.Interact(EgyptPointKind.Guide);
                    Require(egypt.DialogueIsOpen, "Egito: lembrete do guia abre");
                    egypt.AdvanceDialogue();
                    Require(!egypt.BlocksMovement && guideEvents == 1, "Egito: lembrete curto sem repetir missão");
                }
                EgyptStage.Progress.EnterPyramid();
                egypt.Interact(EgyptPointKind.Mummy);
                for (int i = 0; i < egypt.mummyLines.Length; i++) egypt.AdvanceDialogue();
                Require(EgyptStage.Progress.HasScarab && egypt.CompletionIsOpen && awards == 1 && completions == 1, "Egito: primeira conversa entrega escaravelho");
                egypt.CloseCompletion();
                for (int i = 0; i < 2; i++)
                {
                    egypt.Interact(EgyptPointKind.Mummy);
                    Require(egypt.DialogueIsOpen, "Egito: despedida da múmia abre");
                    egypt.AdvanceDialogue();
                    Require(!egypt.BlocksMovement && awards == 1 && completions == 1 && EgyptStage.Progress.HasScarab, "Egito: despedida sem repetir prêmio ou conclusão");
                }
                egypt.Interact(EgyptPointKind.Guide);
                egypt.AdvanceDialogue();
                Require(!egypt.BlocksMovement && guideEvents == 1 && EgyptStage.Progress.HasScarab, "Egito: guia reconhece a missão concluída");
                Debug.Log("EGYPT_REPEAT_DIALOGUE_OK: lembrete, despedidas e prêmio único.");
            }
            finally { Object.DestroyImmediate(root); EgyptStage.Progress.Reset(); }
        }

        private static void TestBrazilFarewells()
        {
            foreach (bool guide in new[] { false, true, false, true })
            {
                stage.OpenDialogue(guide);
                Require(stage.State == BrazilState.Dialogue && stage.LineCount == 1, "Brasil: despedida curta após a vitória");
                Require(stage.DialogueText.Contains("berimbau"), "Brasil: despedida reconhece o artefato recebido");
                stage.AdvanceDialogue();
                Require(stage.State == BrazilState.Exploring && !stage.BlocksMovement && stage.AwardCount == 1 && stage.HasPrize, "Brasil: despedida não reabre prêmio nem desafio");
                stage.StartChallenge();
                Require(stage.State == BrazilState.Exploring, "Brasil: despedida não habilita novo campeonato");
            }
            Debug.Log("BRAZIL_REPEAT_DIALOGUE_OK: lembrete, despedidas e prêmio único.");
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(Key,false) || !EditorApplication.isPlaying || stage == null || ending) return;
            // Aguardo quadros reais do jogo; o Editor pode atualizar mais rápido que a renderização.
            EditorApplication.QueuePlayerLoopUpdate();
            if (Time.frameCount == lastGameFrame) return;
            lastGameFrame = Time.frameCount;
            try
            {
                frames++;
                if (frames == 25 && step != 7 && step < 9) Capture(new[]{"01-praia-guia.png","02-dialogo-guia.png","03-morador.png","04-instrucoes.png","05-tentativa.png","06-minigame.png","07-premio.png","07-premio.png","08-continuar.png"}[step]);
                if (frames == 25 && step == 10) Capture("09-aviso-guia.png");
                if (frames == 25 && step == 11) Capture("10-aviso-morador.png");
                stage.player.TickMovement(Vector2.zero,.02f);
                if (frames < 45) return;
                frames = 0;
                switch (step++)
                {
                    case 0:
                        Require(stage.player.GetComponent<CharacterController>().isGrounded,"Lucas apoiado no chão");
                        ReportFeet();
                        stage.OpenDialogue(true);
                        stage.ContinueExploring();
                        Require(!stage.GuideFinished, "Cancelar a primeira conversa não conclui o guia");
                        stage.OpenDialogue(true);
                        Require(stage.State == BrazilState.Dialogue && stage.LineCount == 5,"Cinco falas do guia");
                        var start = stage.player.transform.position;
                        stage.player.TickMovement(Vector2.right,.3f);
                        Require(Vector3.Distance(start,stage.player.transform.position)<.05f,"Conversa bloqueia caminhada");
                        break;
                    case 1:
                        for(int i=0;i<5;i++)stage.AdvanceDialogue();
                        Require(stage.GuideFinished && stage.State==BrazilState.Exploring,"Objetivo após guia");
                        stage.OpenDialogue(true);
                        Require(stage.LineCount == 1 && stage.DialogueText.Contains("embaixadinhas"), "Guia dá lembrete do desafio");
                        stage.AdvanceDialogue();
                        Require(stage.State == BrazilState.Exploring && !stage.HasPrize, "Lembrete termina sem entregar prêmio");
                        stage.player.PlaceAt(stage.challengeAnchor.position);
                        stage.OpenDialogue(false);
                        Require(stage.LineCount==6,"Seis falas do morador");
                        break;
                    case 2:
                        for(int i=0;i<6;i++)stage.AdvanceDialogue();
                        Require(stage.State==BrazilState.Ready,"Instruções antes de jogar");
                        break;
                    case 3:
                        stage.StartChallenge(); stage.TickChallenge(3,false);
                        stage.TickChallenge(.1f,true);
                        Require(stage.State==BrazilState.Failed && !stage.HasPrize,"Erro não dá prêmio");
                        break;
                    case 4:
                        stage.StartChallenge(); stage.TickChallenge(3,false); stage.TickChallenge(2,false);
                        Require(stage.State==BrazilState.Failed && stage.Challenge.Score==0,"Bola cai sem tocar");
                        stage.StartChallenge(); stage.TickChallenge(3,false);
                        for(int i=0;i<3;i++)stage.TickChallenge(TimeToCenter(stage.Challenge),true);
                        stage.TickChallenge(TimeToCenter(stage.Challenge),false);
                        stage.SendMessage("UpdateBall");
                        break;
                    case 5:
                        Require(stage.State==BrazilState.Playing && stage.Challenge.Score==3,"Sequência em andamento");
                        stage.TickChallenge(0,true);
                        for(int i=4;i<10;i++)stage.TickChallenge(TimeToCenter(stage.Challenge),true);
                        Require(stage.State==BrazilState.Reward && stage.HasPrize && stage.AwardCount==1,"Prêmio após dez acertos");
                        stage.SendMessage("UpdateBall");
                        break;
                    case 6:
                        Require(stage.rewardDisplay.PreviewTexture!=null && stage.rewardDisplay.PreviewTexture.IsCreated(),"Imagem 3D do prêmio");
                        SavePreview(stage.rewardDisplay.PreviewTexture);
                        break;
                    case 7:
                        stage.TickChallenge(5,true); Require(stage.AwardCount==1,"Entrega única do prêmio");
                        stage.ContinueExploring();
                        break;
                    case 8:
                        Require(!stage.BlocksMovement && stage.player.animator.enabled,"Caminhada e animação liberadas");
                        TestBrazilFarewells();
                        TestMovement();
                        File.WriteAllText(Path.Combine(Output,"resultado.txt"),"APROVADO — Unity " + Application.unityVersion + "\nRegras: toque cedo/tarde, ausência de toque, repetição, dez acertos.\nFluxo: cinco falas do guia, seis do morador, instruções, falha, nova tentativa, prêmio único, continuar.\nFísica: apoio no chão, movimento lateral/profundidade e quatro paredes.\nApresentação: oito capturas em Play e imagem 3D do berimbau.\n");
                        break;
                    case 9:
                        stage.player.PlaceAt(stage.guide.position + Vector3.right);
                        break;
                    case 10:
                        stage.player.PlaceAt(stage.resident.position + Vector3.right);
                        break;
                    case 11:
                        ending=true; EditorApplication.ExitPlaymode(); break;
                }
            }
            catch(Exception e)
            {
                ending=true; SessionState.SetBool(Key,false); Debug.LogException(e);
                File.WriteAllText(Path.Combine(Output,"falha.txt"),e.ToString()); EditorApplication.Exit(1);
            }
        }
        private static void TestMovement()
        {
            var player=stage.player; var bounds=Object.FindAnyObjectByType<EgyptMovementBounds>();
            Vector3 center=stage.challengeAnchor.position;
            foreach(var input in new[]{Vector2.left,Vector2.right,Vector2.up,Vector2.down})
            {
                player.PlaceAt(center); Physics.SyncTransforms();
                for(int i=0;i<700;i++)player.TickMovement(input,.02f);
                Require(bounds.Contains(player.transform.position,.1f),"Parede bloqueia saída " + input);
                Require(Vector3.Distance(player.transform.position,center)>1,"Movimento responde " + input);
                Require(Mathf.Abs(player.transform.position.y-center.y)<.15f,"Pés acompanham chão " + input);
            }
            player.PlaceAt(center);
        }
        private static void ReportFeet()
        {
            float minimum=float.PositiveInfinity;
            foreach(var renderer in stage.player.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var mesh=new Mesh(); renderer.BakeMesh(mesh,true);
                foreach(var vertex in mesh.vertices) minimum=Mathf.Min(minimum,renderer.transform.TransformPoint(vertex).y);
                Object.DestroyImmediate(mesh);
            }
            var terrain=Object.FindAnyObjectByType<Terrain>();
            File.WriteAllText(Path.Combine(Output,"apoio-lucas.txt"),"Menor vértice: " + minimum + "\nChão: " + terrain.SampleHeight(stage.player.transform.position) + "\nRaiz: " + stage.player.transform.position);
        }
        private static void Capture(string name)
        {
            ScreenCapture.CaptureScreenshot(Path.Combine(Output,name));
        }
        private static void SavePreview(RenderTexture texture)
        {
            var previous=RenderTexture.active; RenderTexture.active=texture;
            var image=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);
            image.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0); image.Apply();
            int visible=0; foreach(var color in image.GetPixels32()) if(color.a>32)visible++;
            Require(visible>1000,"Berimbau visível na textura");
            File.WriteAllBytes(Path.Combine(Output,"berimbau-preview.png"),image.EncodeToPNG());
            Object.DestroyImmediate(image); RenderTexture.active=previous;
        }
        private static void Require(bool condition,string message) { if(!condition) throw new InvalidOperationException(message); }
    }
}
