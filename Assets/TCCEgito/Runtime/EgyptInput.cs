using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CrossingTheWorld.TCCEgypt
{
    public static class EgyptInput
    {
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

        public static bool InteractPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard k = Keyboard.current;
                return k != null && (k.eKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return);
#else
                return false;
#endif
            }
        }

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
