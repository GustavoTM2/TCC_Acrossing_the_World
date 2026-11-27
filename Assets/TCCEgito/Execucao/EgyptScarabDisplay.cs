using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CrossingTheWorld.TCCEgypt
{
    [RequireComponent(typeof(EgyptStage))]
    public sealed class EgyptScarabDisplay : MonoBehaviour
    {
        public GameObject previewPrefab;
        [Range(10, 90)] public float rotationSpeed = 35f;
        public RenderTexture PreviewTexture { get; private set; }
        private EgyptStage stage;
        private GameObject studio;
        private Transform pivot;
        private Camera previewCamera;

        private void Awake() { stage = GetComponent<EgyptStage>(); }

        private void LateUpdate()
        {
            bool visible = EgyptStage.Progress.HasScarab && stage != null && !stage.IsLoading;
            if (visible && studio == null && previewPrefab != null) PreparePreview();
            if (previewCamera == null) return;
            if (!visible) return;
            // Giro o modelo de verdade; a imagem que aparece na interface acompanha essa rotação.
            pivot.localRotation = Quaternion.Euler(0, Time.unscaledTime * rotationSpeed, 0);
            // Atualizo a textura explicitamente: a câmera da interface não depende da ordem das câmeras da fase.
            RenderPipeline.SubmitRenderRequest(previewCamera,
                new UniversalRenderPipeline.SingleCameraRequest { destination = PreviewTexture });
        }

        private void PreparePreview()
        {
            // Um pequeno estúdio longe da fase permite mostrar o artefato sem ele ficar preso no cenário.
            studio = new GameObject("Preview_Escaravelho");
            studio.transform.SetParent(transform, false);
            studio.transform.position = new Vector3(-10000, -10000, -10000);
            pivot = new GameObject("Giro").transform;
            pivot.SetParent(studio.transform, false);
            GameObject model = Instantiate(previewPrefab, pivot, false);
            model.SetActive(true);
            foreach (Transform part in model.GetComponentsInChildren<Transform>(true)) part.gameObject.layer = 31;
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            PreviewTexture = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32)
                { name = "Escaravelho_Interface", antiAliasing = 2 };
            PreviewTexture.Create();
            GameObject cameraObject = new GameObject("Camera_Escaravelho");
            cameraObject.transform.SetParent(studio.transform, false);
            cameraObject.transform.localPosition = new Vector3(0, 0.75f, -3);
            cameraObject.transform.LookAt(studio.transform.position);
            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.enabled = false;
            previewCamera.orthographic = true;
            previewCamera.orthographicSize = 1.05f;
            previewCamera.nearClipPlane = 0.1f;
            previewCamera.farClipPlane = 10;
            previewCamera.cullingMask = 1 << 31;
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = Color.clear;
            previewCamera.allowHDR = false;
            previewCamera.targetTexture = PreviewTexture;
            UniversalAdditionalCameraData data = previewCamera.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = false;
            data.renderShadows = false;
            data.requiresColorOption = CameraOverrideOption.Off;
            data.requiresDepthOption = CameraOverrideOption.Off;
            data.volumeLayerMask = 0;
        }

        private void OnDisable() { if (previewCamera != null) previewCamera.enabled = false; }

        // Libero a câmera e a textura ao sair da fase para não acumular recursos a cada visita.
        private void OnDestroy()
        {
            if (previewCamera != null) previewCamera.targetTexture = null;
            if (studio != null) Destroy(studio);
            if (PreviewTexture != null)
            {
                PreviewTexture.Release();
                Destroy(PreviewTexture);
            }
        }
    }
}
