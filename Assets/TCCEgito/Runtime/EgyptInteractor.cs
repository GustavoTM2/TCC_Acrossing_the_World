using UnityEngine;

namespace CrossingTheWorld.TCCEgypt
{
    [DefaultExecutionOrder(-50)]
    public sealed class EgyptInteractor : MonoBehaviour
    {
        public EgyptStage stage;
        public EgyptInteractionPoint[] points;

        private void Update()
        {
            if (stage == null || !stage.isActiveAndEnabled || stage.IsLoading) return;
            bool pressed = EgyptInput.InteractPressed;
            if (stage.DialogueIsOpen)
            {
                stage.Hint = "";
                if (pressed) stage.AdvanceFromInput();
                return;
            }
            if (stage.CompletionIsOpen)
            {
                if (pressed) stage.CloseCompletion();
                return;
            }

            EgyptInteractionPoint closest = null;
            float best = float.PositiveInfinity;
            if (points != null)
                foreach (EgyptInteractionPoint point in points)
                {
                    if (point == null || !point.isActiveAndEnabled) continue;
                    float distance = (transform.position - point.Position).sqrMagnitude;
                    if (distance <= point.radius * point.radius && distance < best)
                    {
                        best = distance;
                        closest = point;
                    }
                }
            stage.Hint = closest != null ? closest.Prompt : "";
            if (pressed && closest != null) stage.Interact(closest.kind);
        }

        private void OnDisable()
        {
            if (stage != null) stage.Hint = "";
        }
    }
}
