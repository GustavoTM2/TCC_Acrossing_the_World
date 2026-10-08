using UnityEngine;

namespace CrossingTheWorld.TCCEgypt
{
    public sealed class EgyptMovementBounds : MonoBehaviour
    {
        public float horizontalMin = -5f;
        public float horizontalMax = 30f;
        [Min(0.5f)] public float frontDistance = 2f;
        [Min(0.5f)] public float backDistance = 4f;
        [Min(4f)] public float wallHeight = 40f;
        public float wallBase = -10f;
        [Min(0.1f)] public float wallThickness = 0.5f;

        [ContextMenu("Atualizar paredes invisíveis")]
        public void UpdateColliders()
        {
            float width = Mathf.Max(1f, horizontalMax - horizontalMin);
            float depth = frontDistance + backDistance;
            float x = (horizontalMin + horizontalMax) * 0.5f;
            float z = (backDistance - frontDistance) * 0.5f;
            float y = wallBase + wallHeight * 0.5f;
            Wall("Limite_Frente", new Vector3(x, y, -frontDistance - wallThickness * 0.5f),
                new Vector3(width + wallThickness * 2, wallHeight, wallThickness));
            Wall("Limite_Fundo", new Vector3(x, y, backDistance + wallThickness * 0.5f),
                new Vector3(width + wallThickness * 2, wallHeight, wallThickness));
            Wall("Limite_Esquerda", new Vector3(horizontalMin - wallThickness * 0.5f, y, z),
                new Vector3(wallThickness, wallHeight, depth));
            Wall("Limite_Direita", new Vector3(horizontalMax + wallThickness * 0.5f, y, z),
                new Vector3(wallThickness, wallHeight, depth));
        }

        private void Wall(string name, Vector3 position, Vector3 size)
        {
            Transform wall = transform.Find(name);
            if (wall == null)
            {
                wall = new GameObject(name).transform;
                wall.SetParent(transform, false);
            }
            wall.localPosition = position;
            wall.localRotation = Quaternion.identity;
            wall.localScale = Vector3.one;
            BoxCollider collider = wall.GetComponent<BoxCollider>();
            if (collider == null) collider = wall.gameObject.AddComponent<BoxCollider>();
            collider.center = Vector3.zero;
            collider.size = size;
            collider.isTrigger = false;
            collider.enabled = true;
        }

        public bool Contains(Vector3 worldPosition, float tolerance = 0.05f)
        {
            Vector3 local = transform.InverseTransformPoint(worldPosition);
            return local.x >= horizontalMin - tolerance && local.x <= horizontalMax + tolerance
                && local.z >= -frontDistance - tolerance && local.z <= backDistance + tolerance;
        }

        private void OnDrawGizmosSelected()
        {
            Matrix4x4 previous = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0.1f, 0.8f, 1f, 0.7f);
            Gizmos.DrawWireCube(new Vector3((horizontalMin + horizontalMax) * 0.5f, 0.9f,
                (backDistance - frontDistance) * 0.5f),
                new Vector3(horizontalMax - horizontalMin, 1.8f, frontDistance + backDistance));
            Gizmos.matrix = previous;
        }
    }
}
