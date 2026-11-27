using UnityEngine;

namespace CrossingTheWorld.TCCEgypt
{
    // Cada ponto identifica uma ação: guia, entrada, múmia ou saída.
    public enum EgyptPointKind { Guide, Entrance, Mummy, Exit }

    public sealed class EgyptInteractionPoint : MonoBehaviour
    {
        public EgyptPointKind kind;
        // Esta distância define o quanto o jogador precisa se aproximar para interagir.
        [Min(0.1f)] public float radius = 2.5f;
        [Tooltip("Diferença máxima de altura para conversar. Zero desativa esta restrição.")]
        [Min(0)] public float maxHeightDifference;
        public Transform origin;
        public Vector3 Position => origin != null ? origin.position : transform.position;
        // Na escadaria, preciso chegar à altura da múmia antes de iniciar a conversa.
        public bool CanInteractFrom(Vector3 playerPosition)
        {
            Vector3 difference = playerPosition - Position;
            // A porta da pirâmide fica em uma área com degraus e piso irregular.
            // Para portas, a proximidade depende do plano horizontal, e não da
            // elevação do personagem acima do marcador da entrada.
            float horizontalDistance = new Vector2(difference.x, difference.z).sqrMagnitude;
            bool portal = kind == EgyptPointKind.Entrance || kind == EgyptPointKind.Exit;
            float distance = portal ? horizontalDistance : difference.sqrMagnitude;
            return distance <= radius * radius
                && (portal || maxHeightDifference <= 0 ||
                    Mathf.Abs(difference.y) <= maxHeightDifference);
        }
        // Escolho o aviso que aparece na tela conforme o tipo de interação.
        public string Prompt
        {
            get
            {
                switch (kind)
                {
                    case EgyptPointKind.Guide: return "E — Conversar com o guia";
                    case EgyptPointKind.Entrance: return "E / Enter — Entrar na pirâmide";
                    case EgyptPointKind.Mummy: return "E — Conversar com Rami, a múmia";
                    default: return "E / Enter — Voltar ao exterior";
                }
            }
        }
        // Desenho a distância de interação no Editor para facilitar o ajuste.
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Position, radius);
        }
    }
}
