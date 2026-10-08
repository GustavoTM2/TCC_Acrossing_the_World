using UnityEngine;

namespace CrossingTheWorld.TCCEgypt
{
    public enum EgyptPointKind { Guide, Entrance, Mummy, Exit }

    public sealed class EgyptInteractionPoint : MonoBehaviour
    {
        public EgyptPointKind kind;
        [Min(0.1f)] public float radius = 2.5f;
        public Transform origin;
        public Vector3 Position => origin != null ? origin.position : transform.position;
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
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Position, radius);
        }
    }
}
