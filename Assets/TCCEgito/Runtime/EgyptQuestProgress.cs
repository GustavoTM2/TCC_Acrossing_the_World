namespace CrossingTheWorld.TCCEgypt
{
    public enum EgyptQuestStep { MeetGuide, EnterPyramid, ListenToMummy, Complete }

    // Sem dependência de Unity: a missão permanece separada da interface e das cenas.
    public sealed class EgyptQuestProgress
    {
        public EgyptQuestStep Step { get; private set; }
        public bool HasScarab => Step == EgyptQuestStep.Complete;

        public void Reset() { Step = EgyptQuestStep.MeetGuide; }
        public bool FinishGuide()
        {
            if (Step != EgyptQuestStep.MeetGuide) return false;
            Step = EgyptQuestStep.EnterPyramid;
            return true;
        }
        public bool EnterPyramid()
        {
            if (Step != EgyptQuestStep.EnterPyramid) return false;
            Step = EgyptQuestStep.ListenToMummy;
            return true;
        }
        public bool FinishMummy()
        {
            if (Step != EgyptQuestStep.ListenToMummy) return false;
            Step = EgyptQuestStep.Complete;
            return true;
        }
    }
}
