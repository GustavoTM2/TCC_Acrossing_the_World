using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CrossingTheWorld.TCCEgypt
{
    // Centralizo a leitura das teclas. Estes controles também são usados no Brasil.
    public static class EgyptInput
    {
        // A/D e as setas retornam -1 para esquerda, 1 para direita e 0 quando parado.
        public static Vector3 GroundDirection(Vector2 input, Vector3 right, Vector3 up, float depthScale)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                Vector3 screenRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up);
                if (screenRight.sqrMagnitude > .001f)
                {
                    right = screenRight.normalized;
                    // O vetor positivo indica o fundo da tela. No Egito a tecla S o usa; no Brasil a tecla W.
                    up = Vector3.Cross(Vector3.up, right);
                }
            }
            return right * input.x + up * (input.y * depthScale);
        }

        public static float Horizontal
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard k = Keyboard.current;
                if (k == null) return 0;
                float left = k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0;
                float right = k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0;
                return right - left;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f)
                    - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
#else
                return 0;
#endif
            }
        }

        // W/S e as setas controlam o movimento para o fundo e para frente.
        public static float Depth
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard k = Keyboard.current;
                if (k == null) return 0;
                return (k.wKey.isPressed || k.upArrowKey.isPressed ? 1f : 0f)
                    - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1f : 0f);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f)
                    - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
#else
                return 0;
#endif
            }
        }

        // Teclas W/S invertidas no Egito. As setas continuam convencionais.
        // Brasil e Japão também usam essa direção por decisão de controle,
        // mas cada um tem sua propriedade para manter os projetos independentes.
        public static float EgyptDepth
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard k = Keyboard.current;
                if (k == null) return 0f;
                return (k.sKey.isPressed || k.upArrowKey.isPressed ? 1f : 0f)
                    - (k.wKey.isPressed || k.downArrowKey.isPressed ? 1f : 0f);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f)
                    - (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
#else
                return 0f;
#endif
            }
        }

        // Lucas (Brasil) e Kenji (Japão) usam o mesmo controlador 2.5D.
        // S = fundo, W = frente; as setas para cima/baixo mantêm o padrão.
        public static float BrazilDepth => EgyptDepth;

        // Detecto o momento em que E ou Enter é apertado para iniciar ou avançar uma interação.
        public static bool InteractPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard k = Keyboard.current;
                return k != null && (k.eKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
#else
                return false;
#endif
            }
        }

        // Espaço aciona o pulo no Egito. Lucas não usa pulo nesta versão do Brasil.
        public static bool JumpPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.Space);
#else
                return false;
#endif
            }
        }
    }
}
