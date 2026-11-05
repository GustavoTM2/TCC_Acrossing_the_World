using UnityEngine;
using UnityEngine.UI;
using CrossingTheWorld.TCCEgypt;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CrossingTheWorld.TCCMenu
{
    // Este script escolhe a opção, troca as duas telas e abre o país selecionado.
    public sealed class TravelMenu : MonoBehaviour
    {
        public const string ScenePath = "Assets/TCCMenu/Scenes/Menu_Principal.unity";
        public const string EgyptScene = "Assets/TCCEgito/Scenes/Egito_Exterior_Jogavel.unity";
        public const string BrazilScene = "Assets/TCCBrasil/Scenes/Brasil_Praia_Jogavel.unity";
        public GameObject mainPage, destinationsPage;
        public Text[] mainLabels, countryLabels, countryStatus;
        public RectTransform compass;
        public RectTransform[] cards;
        public Image[] cardBorders;
        public Text notice;
        public int MainIndex { get; private set; }
        public int CountryIndex { get; private set; }
        public bool ChoosingCountry { get; private set; }
        private readonly Color gold = new Color(1f, .76f, .37f);
        private readonly Color cream = new Color(.96f, .89f, .74f);
        private float noticeUntil;

        private void Start()
        {
            Time.timeScale = 1;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            TravelProgress.Reset();
            ShowMain();
        }

        private void Update()
        {
            if (notice.text.Length > 0 && Time.unscaledTime > noticeUntil) notice.text = "";
            if (EgyptSceneTransition.IsBusy) return;
            bool previous = false, next = false, confirm = false, back = false;
#if ENABLE_INPUT_SYSTEM
            Keyboard k = Keyboard.current;
            if (k == null) return;
            previous = ChoosingCountry ? k.leftArrowKey.wasPressedThisFrame || k.aKey.wasPressedThisFrame
                : k.upArrowKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame;
            next = ChoosingCountry ? k.rightArrowKey.wasPressedThisFrame || k.dKey.wasPressedThisFrame
                : k.downArrowKey.wasPressedThisFrame || k.sKey.wasPressedThisFrame;
            confirm = k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame;
            back = k.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            previous = ChoosingCountry ? Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)
                : Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
            next = ChoosingCountry ? Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)
                : Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
            confirm = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
            back = Input.GetKeyDown(KeyCode.Escape);
#endif
            if (back && ChoosingCountry) { ShowMain(); return; }
            if (previous) MoveSelection(-1);
            else if (next) MoveSelection(1);
            if (confirm) ConfirmSelection();
        }

        // O resto da divisão faz a seleção voltar ao início quando chega à última opção.
        public void MoveSelection(int direction)
        {
            if (EgyptSceneTransition.IsBusy) return;
            Select((ChoosingCountry ? CountryIndex : MainIndex) + direction, ChoosingCountry);
        }

        public void Select(int index, bool country)
        {
            if (EgyptSceneTransition.IsBusy || country != ChoosingCountry) return;
            index = (index % 4 + 4) % 4;
            if (country) CountryIndex = index;
            else MainIndex = index;
            notice.text = "";
            UpdateSelection();
        }

        public void ConfirmSelection()
        {
            if (EgyptSceneTransition.IsBusy) return;
            if (!ChoosingCountry)
            {
                if (MainIndex == 0) ShowDestinations();
                else ShowNotice("Esta opção será adicionada em breve.");
                return;
            }
            if (CountryIndex >= 2) { ShowNotice("Este destino estará disponível em breve."); return; }
            string path = CountryIndex == 0 ? EgyptScene : BrazilScene;
            if (!Application.CanStreamedLevelBeLoaded(path)) { ShowNotice("Não foi possível abrir este destino."); return; }
            EgyptSceneTransition.Load(path, null, CountryIndex == 0 ? "Viajando para o Egito..." : "Viajando para o Brasil...");
        }

        // O clique e o teclado usam a mesma ação para não terem comportamentos diferentes.
        public void ClickOption(int index, bool country)
        {
            if (country != ChoosingCountry || EgyptSceneTransition.IsBusy) return;
            Select(index, country);
            ConfirmSelection();
        }

        public void ShowMain()
        {
            ChoosingCountry = false;
            mainPage.SetActive(true); destinationsPage.SetActive(false);
            notice.text = "";
            UpdateSelection();
        }

        public void ShowDestinations()
        {
            if (EgyptSceneTransition.IsBusy) return;
            ChoosingCountry = true;
            mainPage.SetActive(false); destinationsPage.SetActive(true);
            notice.text = "";
            countryStatus[0].text = TravelProgress.EgyptStep == EgyptQuestStep.Complete ? "CONCLUÍDO" : "DISPONÍVEL";
            countryStatus[1].text = TravelProgress.BrazilPrize ? "CONCLUÍDO" : "DISPONÍVEL";
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            for (int i = 0; i < 4; i++)
            {
                bool selected = i == MainIndex;
                mainLabels[i].fontSize = selected ? 44 : 34;
                mainLabels[i].fontStyle = selected ? FontStyle.Bold : FontStyle.Normal;
                mainLabels[i].color = selected ? gold : cream;
                bool chosen = i == CountryIndex;
                cards[i].localScale = Vector3.one * (chosen ? 1.055f : 1f);
                cardBorders[i].color = chosen ? gold : new Color(.45f, .41f, .31f, .7f);
                countryLabels[i].color = chosen ? gold : cream;
            }
            compass.anchoredPosition = new Vector2(56, -(379 + MainIndex * 85));
        }

        private void ShowNotice(string text)
        {
            notice.text = text;
            noticeUntil = Time.unscaledTime + 3;
        }
    }
}
