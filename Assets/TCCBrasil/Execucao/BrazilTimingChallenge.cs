using UnityEngine;

namespace CrossingTheWorld.TCCBrazil
{
    // Regras do desafio separadas da tela, para poder testar o tempo dos acertos.
    public sealed class BrazilTimingChallenge
    {
        public const int Goal = 10;
        public const float Duration = 1.4f;
        public const float PreparationDuration = .35f;
        public float WindowStart { get; private set; }
        public float WindowEnd { get; private set; }
        public float WindowCenter => (WindowStart + WindowEnd) * .5f;
        public float PreparationRemaining { get; private set; }
        public bool IsPreparing => PreparationRemaining > 0;
        public int Score { get; private set; }
        public float Phase { get; private set; }
        public bool Finished { get; private set; }
        public bool Won => Finished && Score == Goal;
        public string Failure { get; private set; }
        public bool InWindow => !IsPreparing && Phase >= WindowStart && Phase <= WindowEnd;
        private System.Random random;
        private int previousRegion = -1;

        public void Begin(int? seed = null)
        {
            Score = 0; Finished = false; Failure = "";
            random = new System.Random(seed ?? Random.Range(0,int.MaxValue));
            previousRegion = -1;
            ChooseWindow();
        }

        // A cada acerto, mudo a região e a largura. A faixa fica parada durante aquele toque.
        private void ChooseWindow()
        {
            int region = random.Next(3);
            if (region == previousRegion) region = (region + 1 + random.Next(2)) % 3;
            float width = Mathf.Clamp(Mathf.Lerp(.235f,.17f,Score / 9f) + ((float)random.NextDouble() - .5f) * .04f,.15f,.26f);
            float oldWidth = WindowEnd - WindowStart;
            if (previousRegion >= 0 && Mathf.Abs(width - oldWidth) < .015f)
                width = oldWidth < .21f ? oldWidth + .025f : oldWidth - .025f;
            float start = region == 0 ? .18f : region == 1 ? .40f : .65f;
            WindowStart = start + (float)random.NextDouble() * .08f;
            WindowEnd = Mathf.Min(.99f,WindowStart + width);
            previousRegion = region;
            Phase = 0;
            // Dou um instante para perceber a nova posição antes de o marcador começar a correr.
            PreparationRemaining = PreparationDuration;
        }

        // Cada toque precisa acontecer na faixa verde. Segurar Espaço não gera novos toques.
        public bool Tick(float deltaTime, bool pressed)
        {
            if (Finished) return false;
            float elapsed = Mathf.Max(0,deltaTime);
            float preparation = Mathf.Min(PreparationRemaining,elapsed);
            PreparationRemaining -= preparation;
            Phase += (elapsed - preparation) / Duration;
            if (pressed && InWindow)
            {
                Score++;
                Finished = Score == Goal;
                if (!Finished) ChooseWindow();
                return true;
            }
            if (pressed || Phase > WindowEnd)
            {
                Failure = Phase < WindowStart ? "O toque veio cedo demais." : "A bola caiu antes do toque.";
                Finished = true;
            }
            return false;
        }
    }
}
