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
        public Transform origin;
        public Vector3 Position => origin != null ? origin.position : transform.position;
        // Escolho o aviso que aparece na tela conforme o tipo de interação.
        public string Prompt
        {
            get
            {
                switch (kind)
                {
                    case EgyptPointKind.Guide: return "E — Conversar com o guia";
                    case EgyptPointKind.Entrance: return "E — Entrar na pirâmide";
                    case EgyptPointKind.Mummy: return "E — Conversar com a múmia";
                    default: return "E — Voltar ao exterior";
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
