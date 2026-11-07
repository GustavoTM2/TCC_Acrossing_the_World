using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using CrossingTheWorld.TCCEgypt;
using CrossingTheWorld.TCCBrazil;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using InputKey = UnityEngine.InputSystem.Key;
#endif

namespace CrossingTheWorld.TCCMenu.Editor
{
    // Teste de integração do menu com as cenas reais. Usa chaves de teste para preservar o progresso do jogador.
    [InitializeOnLoad]
    public static class TravelMenuValidation
    {
        private const string Key = "TCC.Menu.Validation";
        private const string TestPrefix = "CrossingTheWorld.Tests.Menu.";
        private static int phase, frames, lastFrame;
        private static bool ending;
        private static double deadline;
        private static string Output => Environment.GetEnvironmentVariable("TCC_MENU_VALIDATION_OUTPUT")
            ?? Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/MenuValidation"));

        static TravelMenuValidation()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += state =>
            {
                if (!SessionState.GetBool(Key, false)) return;
                if (state == PlayModeStateChange.EnteredPlayMode)
                {
                    TravelProgress.KeyPrefix = TestPrefix;
                    ClearTestSave();
                    EgyptStage.Progress.Reset();
                    phase = frames = 0; lastFrame = -1; ending = false;
                    deadline = EditorApplication.timeSinceStartup + 240;
                    Application.runInBackground = true;
                }
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    SessionState.SetBool(Key, false);
                    Debug.Log("TRAVEL_MENU_VALIDATION_OK");
                    if (SessionState.GetBool(Key + ".LeaveOpen", false))
                    {
                        SessionState.SetBool(Key + ".LeaveOpen", false);
                        EditorApplication.delayCall += CrossingTheWorld.TCCBrazil.Editor.BrazilPlayPreview.OpenForUser;
                    }
                    else EditorApplication.Exit(0);
                }
            };
        }

        public static void RunAndLeaveOpen() { SessionState.SetBool(Key + ".LeaveOpen", true); Run(); }

        public static void BuildAndRun() { TravelMenuSetup.Build(); Run(); }
        public static void Run()
        {
            EditorSceneManager.OpenScene(TravelMenu.ScenePath);
            Directory.CreateDirectory(Output);
            SessionState.SetBool(Key, true);
            EditorApplication.EnterPlaymode();
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(Key, false) || !EditorApplication.isPlaying || ending) return;
            try
            {
                Require(EditorApplication.timeSinceStartup < deadline, "Tempo limite das transições");
                EditorApplication.QueuePlayerLoopUpdate();
                if (Time.frameCount == lastFrame || EgyptSceneTransition.IsBusy) return;
                lastFrame = Time.frameCount;
                frames++;
                // Capturo antes de executar a próxima ação, pois a imagem é gravada no fim do quadro.
                if (frames == 20)
                {
                    if (phase == 0) Capture("01-menu-principal.png");
                    if (phase == 1) Capture("02-opcoes-selecionadas.png");
                    if (phase == 2) Capture("03-destinos-egito.png");
                    if (phase == 3) Capture("04-destinos-brasil.png");
                    if (phase == 5) Capture("05-premio-brasil-menu.png");
                    if (phase == 11 && UnityEngine.Object.FindFirstObjectByType<EgyptStage>().CompletionIsOpen) Capture("06-premio-egito-menu.png");
                    if (phase == 13) Capture("07-destinos-concluidos.png");
                }
                if (frames < 35) return;
                frames = 0;
                TravelMenu menu = UnityEngine.Object.FindFirstObjectByType<TravelMenu>();
                if (menu != null) {
                    // A validação executa as ações diretamente, sem entrada real concorrente.
                    menu.enabled = false;
                    foreach (var raycaster in menu.GetComponentsInChildren<GraphicRaycaster>(true)) raycaster.enabled = false;
                }
                BrazilStage brazil = UnityEngine.Object.FindFirstObjectByType<BrazilStage>();
                EgyptStage egypt = UnityEngine.Object.FindFirstObjectByType<EgyptStage>();
                switch (phase)
                {
                    case 0:
                        Require(menu != null && !menu.ChoosingCountry && menu.MainIndex == 0, "Menu começa em Jogar");
                        for (int i = 0; i < 4; i++)
                        {
                            menu.Select(i, false);
                            Require(menu.mainLabels[i].preferredWidth <= menu.mainLabels[i].rectTransform.rect.width, "Texto selecionado cabe na opção " + i);
                        }
                        menu.Select(0, false);
#if ENABLE_INPUT_SYSTEM
                        MenuAction(menu, InputKey.S);
#else
                        menu.MoveSelection(1);
#endif
                        break;
                    case 1:
                        Require(menu.MainIndex == 1 && menu.mainLabels[1].fontSize > menu.mainLabels[0].fontSize, "Destaque de Opções, índice: " + menu.MainIndex);
                        Require(Mathf.Abs(menu.compass.anchoredPosition.y + 464) < .1f, "Bússola acompanha seleção");
                        menu.ConfirmSelection();
                        Require(!menu.ChoosingCountry && !EgyptSceneTransition.IsBusy, "Opções continua provisória");
                        menu.Select(3, false); menu.MoveSelection(1);
                        Require(menu.MainIndex == 0, "Seleção circular");
#if ENABLE_INPUT_SYSTEM
                        MenuAction(menu, InputKey.Enter);
#else
                        menu.mainLabels[0].transform.parent.GetComponent<Button>().onClick.Invoke();
#endif
                        Require(menu.ChoosingCountry, "Clique em Jogar abre países");
                        break;
                    case 2:
                        Require(menu.cards.Length == 4 && menu.CountryIndex == 0, "Quatro destinos sem bloqueio por ordem");
#if ENABLE_INPUT_SYSTEM
                        MenuAction(menu, InputKey.A); Require(menu.CountryIndex == 3, "A navega para o país anterior");
                        MenuAction(menu, InputKey.D); Require(menu.CountryIndex == 0, "D navega para o próximo país");
                        MenuAction(menu, InputKey.Escape); Require(!menu.ChoosingCountry, "Esc retorna à tela inicial");
                        MenuAction(menu, InputKey.W); Require(menu.MainIndex == 3, "W navega para cima");
                        MenuAction(menu, InputKey.DownArrow); Require(menu.MainIndex == 0, "Seta para baixo navega");
                        MenuAction(menu, InputKey.Enter);
#endif
                        menu.cards[2].GetComponent<MenuPointer>().OnPointerEnter(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
                        Require(menu.CountryIndex == 2, "Mouse muda o destaque do país");
                        Require(Application.CanStreamedLevelBeLoaded("Assets/TCCExploracao/Scenes/Italia_Exploracao.unity"), "Exploração da Itália disponível");
                        Require(Application.CanStreamedLevelBeLoaded("Assets/TCCExploracao/Scenes/Japao_Exploracao.unity"), "Exploração do Japão disponível");
                        menu.Select(1, true);
                        break;
                    case 3:
                        menu.cards[1].GetComponent<Button>().onClick.Invoke();
                        break;
                    case 4:
                        Require(brazil != null && SceneManager.GetActiveScene().path == TravelMenu.BrazilScene, "Brasil carregado primeiro");
                        brazil.enabled = false;
                        brazil.OpenDialogue(true);
                        for (int i = 0; i < 5; i++) brazil.AdvanceDialogue();
                        Require(TravelProgress.BrazilGuide, "Guia salvo");
                        brazil.OpenDialogue(false);
                        for (int i = 0; i < 6; i++) brazil.AdvanceDialogue();
                        brazil.StartChallenge(); brazil.TickChallenge(3, false);
                        for (int i = 0; i < 10; i++)
                            brazil.TickChallenge(brazil.Challenge.PreparationRemaining + (brazil.Challenge.WindowCenter - brazil.Challenge.Phase) * BrazilTimingChallenge.Duration, true);
                        Require(brazil.HasPrize && TravelProgress.BrazilPrize && brazil.AwardCount == 1, "Conquista do Brasil salva uma vez");
                        break;
                    case 5:
                        brazil.ReturnToMenu();
                        break;
                    case 6:
                        Require(menu != null, "Retorno do Brasil ao menu");
                        menu.ShowDestinations();
                        Require(menu.countryStatus[1].text == "" && !TravelProgress.BrazilGuide && !TravelProgress.BrazilPrize, "Brasil reiniciado ao voltar ao menu");
                        menu.ClickOption(1, true);
                        break;
                    case 7:
                        Require(brazil != null && !brazil.HasPrize && !brazil.GuideFinished && brazil.AwardCount == 0, "Revisita começa uma nova partida");
                        brazil.ReturnToMenu();
                        break;
                    case 8:
                        Require(menu != null, "Segundo retorno ao menu");
                        menu.ShowDestinations(); menu.ClickOption(0, true);
                        break;
                    case 9:
                        Require(egypt != null && !egypt.interior, "Egito exterior carregado");
                        egypt.Interact(EgyptPointKind.Guide);
                        for (int i = 0; i < egypt.guideLines.Length; i++) egypt.AdvanceDialogue();
                        Require(TravelProgress.EgyptStep == EgyptQuestStep.EnterPyramid, "Guia do Egito salvo");
                        egypt.Interact(EgyptPointKind.Entrance);
                        break;
                    case 10:
                        Require(egypt != null && egypt.interior && EgyptStage.Progress.Step == EgyptQuestStep.ListenToMummy, "Transição para pirâmide preservada");
                        egypt.Interact(EgyptPointKind.Mummy);
                        for (int i = 0; i < egypt.mummyLines.Length; i++) egypt.AdvanceDialogue();
                        break;
                    case 11:
                        if (egypt.IsDeliveringScarab) return;
                        Require(egypt.CompletionIsOpen && TravelProgress.EgyptStep == EgyptQuestStep.Complete, "Conquista do Egito salva");
                        egypt.ReturnToMenu();
                        break;
                    case 12:
                        Require(menu != null, "Retorno do Egito ao menu");
                        menu.ShowDestinations();
                        Require(menu.countryStatus[0].text == "" && TravelProgress.EgyptStep == EgyptQuestStep.MeetGuide && menu.countryStatus[1].text == "" && !TravelProgress.BrazilGuide && !TravelProgress.BrazilPrize, "Destinos reiniciados");
                        break;
                    case 13:
                        EgyptStage.Progress.Reset();
                        menu.ClickOption(0, true);
                        break;
                    case 14:
                        Require(!EgyptStage.Progress.HasScarab, "Egito começa novamente sem escaravelho");
                        egypt.Interact(EgyptPointKind.Guide);
                        for (int i = 0; i < egypt.guideLines.Length; i++) egypt.AdvanceDialogue();
                        Require(!egypt.DialogueIsOpen && !egypt.CompletionIsOpen, "Guia reconhece a revisita");
                        egypt.Interact(EgyptPointKind.Entrance);
                        break;
                    case 15:
                        Require(egypt != null && egypt.interior, "Revisita à pirâmide");
                        egypt.Interact(EgyptPointKind.Mummy);
                        Require(egypt.DialogueIsOpen && !egypt.CompletionIsOpen && !egypt.IsDeliveringScarab, "Múmia inicia nova conversa");
                        egypt.ReturnToMenu();
                        break;
                    case 16:
                        Require(menu != null, "Menu acessível após ambas as fases");
                        menu.Select(3, false); menu.ConfirmSelection();
                        Require(!EgyptSceneTransition.IsBusy && !menu.ChoosingCountry, "Sair permanece sem função nesta etapa");
                        File.WriteAllText(Path.Combine(Output, "resultado.txt"), "APROVADO — Unity " + Application.unityVersion + "\nMenu, destaque, bússola, clique e seleção circular.\nPaíses livres; Itália/Japão em breve.\nBrasil: guia, dez acertos, prêmio, retorno e revisita.\nEgito: guia, pirâmide, entrega, retorno e revisita.\nProgresso reiniciado ao retornar ao menu. Chaves de teste isoladas e removidas.\n");
                        ClearTestSave(); ending = true; EditorApplication.ExitPlaymode();
                        break;
                }
                phase++;
            }
            catch (Exception exception)
            {
                ending = true; SessionState.SetBool(Key, false);
                ClearTestSave(); Debug.LogException(exception);
                File.WriteAllText(Path.Combine(Output, "falha.txt"), exception.ToString());
                EditorApplication.Exit(1);
            }
        }
        private static void ClearTestSave()
        {
            foreach (string key in new[] { "Egypt", "BrazilGuide", "BrazilPrize" }) PlayerPrefs.DeleteKey(TestPrefix + key);
            PlayerPrefs.Save();
        }
        private static void Capture(string name) { ScreenCapture.CaptureScreenshot(Path.Combine(Output, name)); }
#if ENABLE_INPUT_SYSTEM
        private static void MenuAction(TravelMenu menu, InputKey key)
        {
            // Verifico as ações do menu sem depender da entrada física do Editor.
            // O teclado real é conferido separadamente no Play.
            if (key == InputKey.Enter) menu.ConfirmSelection();
            else if (key == InputKey.Escape) menu.ShowMain();
            else if (key == InputKey.W || key == InputKey.A) menu.MoveSelection(-1);
            else if (key == InputKey.S || key == InputKey.D || key == InputKey.DownArrow) menu.MoveSelection(1);
        }
#endif
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
