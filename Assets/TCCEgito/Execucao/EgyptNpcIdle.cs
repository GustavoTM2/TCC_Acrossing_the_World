using UnityEngine;

namespace CrossingTheWorld.TCCEgypt
{
    // Movimenta apenas as articulações das instâncias; os FBX originais são preservados.
    public sealed class EgyptNpcIdle : MonoBehaviour
    {
        public Transform coordinateSpace;
        public EgyptStage stage;
        public string speaker = "Múmia";
        public Transform head;
        public Transform leftArm;
        public Transform rightArm;
        public float leftArmSide = 1f;
        public float rightArmSide = -1f;
        [Range(0, 20)] public float relaxArmDegrees = 10f;
        [Range(0, 6)] public float headTurnDegrees = 3f;
        [Range(0, 4)] public float headNodDegrees = 1.2f;
        [Range(0, 4)] public float armSwayDegrees = 1.2f;
        public float phaseOffset;
        [SerializeField, HideInInspector] private bool basePoseCaptured;
        [SerializeField, HideInInspector] private Quaternion headBase;
        [SerializeField, HideInInspector] private Quaternion leftBase;
        [SerializeField, HideInInspector] private Quaternion rightBase;

        // Guardo a posição original das articulações para usar como base dos movimentos.
        public void CaptureBasePose()
        {
            if (head != null) headBase = head.localRotation;
            if (leftArm != null) leftBase = leftArm.localRotation;
            if (rightArm != null) rightBase = rightArm.localRotation;
            basePoseCaptured = true;
        }

        public void RestoreBasePose()
        {
            if (!basePoseCaptured) return;
            if (head != null) head.localRotation = headBase;
            if (leftArm != null) leftArm.localRotation = leftBase;
            if (rightArm != null) rightArm.localRotation = rightBase;
        }

        // Atualizo os pequenos movimentos de cabeça e braços a cada quadro.
        private void LateUpdate() { EvaluatePose(Time.time); }

        public void EvaluatePose(float time)
        {
            if (!basePoseCaptured) CaptureBasePose();
            if (coordinateSpace == null) coordinateSpace = transform;
            float t = time + phaseOffset;
            // Aumento um pouco os gestos quando este personagem está falando.
            float gesture = stage != null && stage.CurrentSpeaker == speaker ? 1f : 0f;
            // Uso o seno para a cabeça se mover devagar de um lado para o outro.
            if (head != null)
            {
                Vector3 up = head.parent.InverseTransformDirection(coordinateSpace.up);
                Vector3 right = head.parent.InverseTransformDirection(coordinateSpace.right);
                head.localRotation = Quaternion.AngleAxis(Mathf.Sin(t * 0.45f) * headTurnDegrees, up)
                    * Quaternion.AngleAxis(Mathf.Sin(t * (0.65f + gesture * 0.7f) + 0.4f) * (headNodDegrees + gesture * 1.4f), right) * headBase;
            }
            Arm(leftArm, leftBase, leftArmSide, -relaxArmDegrees + Mathf.Sin(t * 0.8f) * armSwayDegrees + Mathf.Sin(t * 1.25f) * gesture * 3f);
            Arm(rightArm, rightBase, rightArmSide, -relaxArmDegrees * 0.85f
                + Mathf.Sin(t * 0.63f + 1.1f) * armSwayDegrees + Mathf.Sin(t * 0.95f + 0.4f) * gesture * 2f);
        }

        // Aplico uma rotação leve no braço para deixar a postura mais natural.
        private void Arm(Transform arm, Quaternion rest, float side, float angle)
        {
            if (arm == null) return;
            Vector3 axis = arm.parent.InverseTransformDirection(coordinateSpace.forward);
            arm.localRotation = Quaternion.AngleAxis(side * angle, axis) * rest;
        }
    }
}
