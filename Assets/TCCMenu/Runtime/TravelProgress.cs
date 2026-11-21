using UnityEngine;
using CrossingTheWorld.TCCEgypt;

namespace CrossingTheWorld.TCCMenu
{
    // O progresso vale durante a sessão atual e é limpo ao iniciar ou voltar ao menu.
    public static class TravelProgress
    {
        private const string DefaultKeyPrefix = "CrossingTheWorld.V1.";
        public static string KeyPrefix = DefaultKeyPrefix;
        public static EgyptQuestStep EgyptStep => (EgyptQuestStep)Mathf.Clamp(PlayerPrefs.GetInt(KeyPrefix + "Egypt", 0), 0, 3);
        public static bool BrazilGuide => PlayerPrefs.GetInt(KeyPrefix + "BrazilGuide", 0) == 1;
        public static bool BrazilPrize => PlayerPrefs.GetInt(KeyPrefix + "BrazilPrize", 0) == 1;
        public static bool ItalyPrize => PlayerPrefs.GetInt(KeyPrefix + "ItalyPrize", 0) == 1;
        public static bool JapanGuide => PlayerPrefs.GetInt(KeyPrefix + "JapanGuide", 0) == 1;
        public static bool JapanPrize => PlayerPrefs.GetInt(KeyPrefix + "JapanPrize", 0) == 1;
        public static bool JapanFarewell => PlayerPrefs.GetInt(KeyPrefix + "JapanFarewell", 0) == 1;

        // Cada nova execução começa sem conquistas guardadas da execução anterior.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void StartFreshSession()
        {
            KeyPrefix = DefaultKeyPrefix;
            Reset();
        }

        public static void Reset()
        {
            PlayerPrefs.DeleteKey(KeyPrefix + "Egypt");
            PlayerPrefs.DeleteKey(KeyPrefix + "BrazilGuide");
            PlayerPrefs.DeleteKey(KeyPrefix + "BrazilPrize");
            PlayerPrefs.DeleteKey(KeyPrefix + "ItalyPrize");
            PlayerPrefs.DeleteKey(KeyPrefix + "JapanGuide");
            PlayerPrefs.DeleteKey(KeyPrefix + "JapanPrize");
            PlayerPrefs.DeleteKey(KeyPrefix + "JapanFarewell");
            PlayerPrefs.Save();
        }

        public static void SaveEgypt(EgyptQuestStep step)
        {
            PlayerPrefs.SetInt(KeyPrefix + "Egypt", (int)step);
            PlayerPrefs.Save();
        }

        public static void SaveItaly(bool hasPrize)
        {
            PlayerPrefs.SetInt(KeyPrefix + "ItalyPrize", hasPrize ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void SaveJapan(bool guideFinished, bool hasPrize, bool farewellFinished)
        {
            PlayerPrefs.SetInt(KeyPrefix + "JapanGuide", guideFinished ? 1 : 0);
            PlayerPrefs.SetInt(KeyPrefix + "JapanPrize", hasPrize ? 1 : 0);
            PlayerPrefs.SetInt(KeyPrefix + "JapanFarewell", farewellFinished ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void SaveBrazil(bool guideFinished, bool hasPrize)
        {
            PlayerPrefs.SetInt(KeyPrefix + "BrazilGuide", guideFinished ? 1 : 0);
            PlayerPrefs.SetInt(KeyPrefix + "BrazilPrize", hasPrize ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
