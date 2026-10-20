using UnityEngine;

namespace CrossingTheWorld.TCCEgypt
{
    [DefaultExecutionOrder(-50)]
    public sealed class EgyptInteractor : MonoBehaviour
    {
        public EgyptStage stage;
        public EgyptInteractionPoint[] points;

        // Verifico a tecla de interação e o que o jogador pode fazer neste momento.
        private void Update()
        {
            if (stage == null || !stage.isActiveAndEnabled || stage.IsLoading || stage.IsDeliveringScarab) return;
            bool pressed = EgyptInput.InteractPressed;
            // Se uma conversa já está aberta, E ou Enter avança para a próxima fala.
            if (stage.DialogueIsOpen)
            {
                stage.Hint = "";
                if (pressed) stage.AdvanceFromInput();
                return;
            }
            if (stage.CompletionIsOpen)
            {
                // A conclusão espera a escolha do botão, para E não fechar o prêmio por acidente.
                return;
            }

            // Procuro o ponto de interação mais próximo que esteja dentro da distância permitida.
            EgyptInteractionPoint closest = null;
            float best = float.PositiveInfinity;
            if (points != null)
                foreach (EgyptInteractionPoint point in points)
                {
                    if (point == null || !point.isActiveAndEnabled) continue;
                    float distance = (transform.position - point.Position).sqrMagnitude;
                    if (point.CanInteractFrom(transform.position) && distance < best)
                    {
                        best = distance;
                        closest = point;
                    }
                }
            // Mostro o aviso de interação e, ao apertar a tecla, chamo a ação daquele ponto.
            stage.Hint = closest != null ? closest.Prompt : "";
            if (pressed && closest != null) stage.Interact(closest.kind);
        }

        private void OnDisable()
        {
            if (stage != null) stage.Hint = "";
        }
    }
}
