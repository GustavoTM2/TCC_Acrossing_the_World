using UnityEngine;
using CrossingTheWorld.TCCEgypt;

namespace CrossingTheWorld.TCCJapan
{
    // Câmera 2.5D externa: acompanha Kenji com inércia e respeita as
    // paredes invisíveis para não revelar a área fora do cenário.
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(400)]
    public sealed class JapanExteriorCamera25D : MonoBehaviour
    {
        [Header("Alvo e limite da fase")]
        public Transform target;
        public Transform boundaryRoot;

        [Header("Enquadramento")]
        [Range(30f, 65f)] public float downwardAngle = 39f;
        [Range(40f, 65f)] public float angleNearTemple = 47f;
        [Range(30f, 60f)] public float verticalFieldOfView = 46f;
        [Min(1f)] public float heightAbovePlayer = 5.3f;
        [Min(.2f)] public float distanceBehindPlayer = 7.7f;
        [Range(.05f, .65f)] public float followSmoothTime = .19f;
        [Range(0f, .4f)] public float movementLookAhead = .15f;
        [Range(0f, 1f)] public float boundarySafety = .55f;

        [Header("Proteção visual")]
        public bool avoidWalls = true;
        [Range(.05f, .5f)] public float collisionRadius = .22f;

        private Camera gameCamera;
        private CharacterController playerController;
        private Vector3 smoothVelocity;
        private bool ready;
        private float minX, maxX, minZ, maxZ;
        private float configuredAspect;
        private float baseHorizontalTan;

        // Quando Kenji chega perto do templo, a câmera eleva levemente o ângulo
        // para manter o personagem maior na imagem sem mostrar atrás do cenário.
        private float CurrentAngle
        {
            get
            {
                if (target == null) return downwardAngle;
                float nearTemple = Mathf.InverseLerp(minZ + 12f, minZ + 2.5f, target.position.z);
                return Mathf.Lerp(downwardAngle, angleNearTemple, nearTemple);
            }
        }

        public void Configure(Transform player, Transform walls)
        {
            target = player;
            boundaryRoot = walls;
            gameCamera = GetComponent<Camera>();
            if (gameCamera == null || target == null)
            {
                Debug.LogWarning("Japão: câmera sem componente Camera ou alvo Kenji.");
                return;
            }
            var oldController = GetComponent<EgyptCamera25D>();
            if (oldController != null) oldController.enabled = false;
            playerController = target.GetComponent<CharacterController>();

            // Garante um ângulo alto o suficiente para a câmera não enxergar
            // fora das bordas frontal e traseira do percurso.
            baseHorizontalTan = Mathf.Tan(verticalFieldOfView * Mathf.Deg2Rad * .5f) * (16f / 9f);
            gameCamera.nearClipPlane = .12f;
            gameCamera.farClipPlane = 250f;
            gameCamera.orthographic = false;
            FindBounds();

            ready = true;
            smoothVelocity = Vector3.zero;
            UpdateFraming();
            transform.position = DesiredPosition(false);
            transform.rotation = FollowRotation();
        }

        private void FindBounds()
        {
            // Limites físicos já existentes no mapa, sem modificar colisão do player.
            minX = target.position.x - 14f;
            maxX = target.position.x + 14f;
            minZ = target.position.z - 18f;
            maxZ = target.position.z + 3.5f;
            if (boundaryRoot == null)
            {
                Debug.LogWarning("Japão: limites invisíveis não encontrados; usando região padrão.");
                return;
            }

            bool left = false, right = false, front = false, back = false;
            foreach (var collider in boundaryRoot.GetComponentsInChildren<BoxCollider>(true))
            {
                if (collider.name.StartsWith("Limite_Esquerda"))
                {
                    minX = collider.bounds.center.x;
                    left = true;
                }
                else if (collider.name.StartsWith("Limite_Direita"))
                {
                    maxX = collider.bounds.center.x;
                    right = true;
                }
                else if (collider.name.StartsWith("Limite_Frente"))
                {
                    minZ = collider.bounds.center.z;
                    front = true;
                }
                else if (collider.name.StartsWith("Limite_Fundo"))
                {
                    maxZ = collider.bounds.center.z;
                    back = true;
                }
            }
            // O grupo Paredes_Invisiveis está rotacionado 180 graus.
            // Por isso nomes esquerdo/direito/frente/fundo podem estar invertidos.
            if (left && right && minX > maxX)
            {
                float swap = minX;
                minX = maxX;
                maxX = swap;
            }
            if (front && back && minZ > maxZ)
            {
                float swap = minZ;
                minZ = maxZ;
                maxZ = swap;
            }
            if (maxX - minX < 3f || maxZ - minZ < 3f)
            {
                minX = target.position.x - 14f;
                maxX = target.position.x + 14f;
                minZ = target.position.z - 18f;
                maxZ = target.position.z + 3.5f;
            }
        }

        private void UpdateFraming()
        {
            if (gameCamera == null) return;
            // Em monitores ultrawide, reduz FOV vertical para impedir que
            // o enquadramento horizontal fique maior que o do cenário.
            float aspect = Mathf.Max(.5f, gameCamera.aspect);
            if (Mathf.Abs(configuredAspect - aspect) < .002f) return;
            configuredAspect = aspect;
            float fitted = 2f * Mathf.Atan(baseHorizontalTan / aspect) * Mathf.Rad2Deg;
            gameCamera.fieldOfView = Mathf.Min(verticalFieldOfView, fitted);
        }

        private Vector3 DesiredPosition(bool applyLookAhead)
        {
            Vector3 p = target.position;
            if (applyLookAhead && playerController != null)
            {
                var speed = playerController.velocity;
                p += new Vector3(speed.x * movementLookAhead, 0f,
                    speed.z * movementLookAhead * .35f);
            }

            // Calcula a área realmente vista pela lente até o chão.
            // O cálculo antigo considerava somente a largura perto do jogador:
            // nas bordas superiores da tela ainda aparecia a área fora do mapa.
            float halfVertical = gameCamera.fieldOfView * Mathf.Deg2Rad * .5f;
            float angle = CurrentAngle;
            float topAngle = Mathf.Max(12f, angle - gameCamera.fieldOfView * .5f)
                * Mathf.Deg2Rad;
            float bottomAngle = Mathf.Min(89f, angle + gameCamera.fieldOfView * .5f)
                * Mathf.Deg2Rad;
            float height = Mathf.Max(1f, heightAbovePlayer);
            float farReach = height / Mathf.Tan(topAngle);
            float nearReach = height / Mathf.Tan(bottomAngle);

            // Orientação da câmera: vista para -Z, então o extremo superior
            // da imagem corresponde a cameraZ - farReach.
            float minCameraZ = minZ + farReach + boundarySafety;
            float maxCameraZ = maxZ + nearReach - boundarySafety;
            if (minCameraZ > maxCameraZ)
                minCameraZ = maxCameraZ = (minZ + maxZ + farReach + nearReach) * .5f;

            // Largura do enquadramento no canto MAIS DISTANTE da lente.
            float farLateral = height / Mathf.Sin(topAngle)
                * Mathf.Tan(halfVertical) * gameCamera.aspect;
            float sideInset = farLateral + boundarySafety;
            float xLo = minX + sideInset;
            float xHi = maxX - sideInset;
            if (xLo > xHi) xLo = xHi = (minX + maxX) * .5f;

            var result = new Vector3(
                Mathf.Clamp(p.x, xLo, xHi),
                target.position.y + height,
                Mathf.Clamp(p.z + distanceBehindPlayer, minCameraZ, maxCameraZ)
            );

            if (!avoidWalls) return result;

            // Evita atravessar telhados, pilares e paredes quando Kenji se aproxima.
            var origin = target.position + Vector3.up * 1.25f;
            Vector3 segment = result - origin;
            float clear = ClearDistance(origin, segment);
            if (clear >= segment.magnitude) return result;

            // Tenta contornar a peça do pátio antes de encurtar o plano.
            for (int side = -1; side <= 1; side += 2)
            {
                var alternate = result;
                alternate.x = Mathf.Clamp(result.x + side * 1.8f, xLo, xHi);
                if (Mathf.Abs(alternate.x - result.x) < .5f) continue;
                Vector3 path = alternate - origin;
                if (ClearDistance(origin, path) >= path.magnitude) return alternate;
            }
            return origin + segment.normalized * Mathf.Max(.25f, clear - .3f);
        }

        private float ClearDistance(Vector3 origin, Vector3 segment)
        {
            float distance = segment.magnitude;
            if (distance <= .3f) return distance;
            float best = distance;
            foreach (var hit in Physics.SphereCastAll(origin, collisionRadius,
                segment / distance, distance, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == null || hit.collider.transform.IsChildOf(target)) continue;
                if (hit.collider.transform.IsChildOf(transform)) continue;
                if (boundaryRoot != null && hit.collider.transform.IsChildOf(boundaryRoot)) continue;
                if (hit.normal.y > .72f || hit.distance < .65f) continue;
                best = Mathf.Min(best, hit.distance);
            }
            return best;
        }

        private Quaternion FollowRotation()
        {
            // Nas bordas, o rig para antes de Kenji. Reserva espaço para a cabeça
            // mesmo quando a posição da câmera fica limitada pelo pátio.
            float playerHeight = playerController ? playerController.height : 1.75f;
            Vector3 head = target.position + Vector3.up * playerHeight;
            float depth = Mathf.Max(.1f, transform.position.z - head.z);
            float headAngle = Mathf.Atan2(transform.position.y - head.y, depth) * Mathf.Rad2Deg;
            float marginAngle = Mathf.Atan(.72f * Mathf.Tan(gameCamera.fieldOfView * .5f * Mathf.Deg2Rad)) * Mathf.Rad2Deg;
            return Quaternion.Euler(Mathf.Min(CurrentAngle, headAngle + marginAngle), 180f, 0f);
        }

        private void LateUpdate()
        {
            if (!ready || target == null) return;
            if (Time.deltaTime <= 0f) return;
            UpdateFraming();
            var desired = DesiredPosition(true);
            transform.position = Vector3.SmoothDamp(transform.position, desired,
                ref smoothVelocity, followSmoothTime, 40f, Time.deltaTime);
            // Fixar a orientação evita que a lente oscile ao pular,
            // e evita mostrar o lado de fora do cenário ao girar junto ao Kenji.
            transform.rotation = FollowRotation();
        }
    }
}
