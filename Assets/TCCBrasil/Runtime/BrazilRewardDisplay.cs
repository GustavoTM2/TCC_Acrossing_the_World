using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CrossingTheWorld.TCCBrazil
{
    [RequireComponent(typeof(BrazilStage))]
    public sealed class BrazilRewardDisplay : MonoBehaviour
    {
        public GameObject previewPrefab;
        public RenderTexture PreviewTexture { get; private set; }
        private BrazilStage stage;
        private GameObject studio;
        private Transform pivot;
        private Camera previewCamera;
        private void Awake() { stage = GetComponent<BrazilStage>(); }
        private void LateUpdate()
        {
            if (!stage.HasPrize || previewPrefab == null) return;
            if (studio == null) Prepare();
            pivot.localRotation = Quaternion.Euler(0, Time.unscaledTime * 30, 0);
            RenderPipeline.SubmitRenderRequest(previewCamera, new UniversalRenderPipeline.SingleCameraRequest { destination = PreviewTexture });
        }
        private void Prepare()
        {
            // Uma câmera separada mostra o modelo do prêmio girando na interface.
            studio = new GameObject("Preview_Berimbau");
            studio.transform.SetParent(transform, false); studio.transform.position = Vector3.one * -12000;
            pivot = new GameObject("Giro").transform; pivot.SetParent(studio.transform, false);
            var model = Instantiate(previewPrefab, pivot, false);
            foreach (Transform part in model.GetComponentsInChildren<Transform>(true)) part.gameObject.layer = 30;
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true)) { renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false; }
            PreviewTexture = new RenderTexture(512, 768, 24, RenderTextureFormat.ARGB32) { name = "Berimbau_Interface", antiAliasing = 2 };
            PreviewTexture.Create();
            previewCamera = new GameObject("Camera_Berimbau").AddComponent<Camera>();
            previewCamera.transform.SetParent(studio.transform, false);
            previewCamera.transform.localPosition = new Vector3(0, .15f, -4);
            previewCamera.transform.LookAt(studio.transform.position);
            previewCamera.enabled = false; previewCamera.orthographic = true; previewCamera.orthographicSize = 1.4f;
            previewCamera.nearClipPlane = .1f; previewCamera.farClipPlane = 10;
            previewCamera.cullingMask = 1 << 30; previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = Color.clear; previewCamera.allowHDR = false; previewCamera.targetTexture = PreviewTexture;
            var data = previewCamera.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = false; data.renderShadows = false; data.volumeLayerMask = 0;
            data.requiresColorOption = CameraOverrideOption.Off; data.requiresDepthOption = CameraOverrideOption.Off;
        }
        private void OnDestroy()
        {
            if (previewCamera != null) previewCamera.targetTexture = null;
            if (studio != null) Destroy(studio);
            if (PreviewTexture != null) { PreviewTexture.Release(); Destroy(PreviewTexture); }
        }
    }
}
