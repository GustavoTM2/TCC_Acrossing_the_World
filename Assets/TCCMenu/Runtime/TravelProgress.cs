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
            PlayerPrefs.Save();
        }

        public static void SaveEgypt(EgyptQuestStep step)
        {
            PlayerPrefs.SetInt(KeyPrefix + "Egypt", (int)step);
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
