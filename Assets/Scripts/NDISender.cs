using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using TMPro;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ota.ndi
{

    public class NDISender : MonoBehaviour
    {

        [SerializeField] private string _ndiName;
        [SerializeField] private ComputeShader _encodeCompute;
        [SerializeField] private bool _enableAlpha = false;
        [SerializeField] private int _frameRateNumerator = 30000;
        [SerializeField] private int _frameRateDenominator = 1001;

        [SerializeField] private Camera _arcamera;

        [SerializeField] private RawImage _depthPreview;
        [SerializeField] private RawImage _stencilPreview;
        [SerializeField] private TextMeshProUGUI _resolutionInfoText;
        [SerializeField] private TextMeshProUGUI _statusInfoText;

        [SerializeField] private Shader _shader;

        [SerializeField]
        [Tooltip("The AROcclusionManager which will produce depth textures.")]
        AROcclusionManager m_OcclusionManager;
        [SerializeField]
        [Tooltip("The ARCameraManager which will produce frame events.")]
        ARCameraManager m_CameraManager;

        [Space]
        [SerializeField] float _minDepth = 0.2f;
        [SerializeField] float _maxDepth = 3.2f;

        [Space]
        [SerializeField] OtavjMeshManager _otavjMeshManager;
        [SerializeField] InputHandler _inputHandler;

        private IntPtr _sendInstance;
        private FormatConverter _formatConverter;
        private int _width;
        private int _height;

        private NativeArray<byte>? _nativeArray;
        private byte[] _bytes;

        private Texture2D _sourceOriginTexture;
        private Texture2D _sourceDepthTexture;
        private Texture2D _sourceStencilTexture;

        private float _textureAspectRatio = 1.0f;

        private Material _muxMaterial;
        private RenderTexture _senderRT;
        private Matrix4x4 _projection;

        private String _metadataStr;
        private String _metadataStr4bg;

        private BgMetadataManager _bgmetadatamanager;

        // Start is called before the first frame update
        void Start()
        {
            //WifiManager.Instance.SetupNetwork();

            if (!NDIlib.Initialize())
            {
                Debug.Log("NDIlib can't be initialized.");
                return;
            }

            _formatConverter = new FormatConverter(_encodeCompute);

            IntPtr nname = Marshal.StringToHGlobalAnsi(_ndiName);
            NDIlib.send_create_t sendSettings = new NDIlib.send_create_t { p_ndi_name = nname };
            _sendInstance = NDIlib.send_create(ref sendSettings);
            Marshal.FreeHGlobal(nname);

            if (_sendInstance == IntPtr.Zero)
            {
                Debug.LogError("NDI can't create a send instance.");
                return;
            }

            _muxMaterial = new Material(_shader);
            _senderRT = new RenderTexture(1920, 1440, 0);
            _senderRT.Create();

            _bgmetadatamanager = new BgMetadataManager();
        }

        void OnEnable()
        {
            // Camera callback setup
            m_CameraManager.frameReceived += OnCameraFrameEventReceived;
            //m_OcclusionManager.frameReceived += OnOcclusionFrameEventReceived;
        }

        void OnDisable()
        {
            // Camera callback termination
            m_CameraManager.frameReceived -= OnCameraFrameEventReceived;
            //m_OcclusionManager.frameReceived -= OnOcclusionFrameEventReceived;
        }

        void OnDestroy()
        {
            ReleaseInternalObjects();
        }

        private void ReleaseInternalObjects()
        {
            Destroy(_muxMaterial);
            Destroy(_senderRT);
            if (_sendInstance != IntPtr.Zero)
            {
                NDIlib.send_destroy(_sendInstance);
                _sendInstance = IntPtr.Zero;
            }

            if (_nativeArray != null)
            {
                _nativeArray.Value.Dispose();
                _nativeArray = null;
            }
        }

        private void Update()
        {
            // Camera manager related information text is displayed
            var config = m_CameraManager.currentConfiguration;
            var configtext = $"{config?.width}x{config?.height}{((bool)(config?.framerate.HasValue) ? $" at {config?.framerate.Value} Hz" : "")}{(config?.depthSensorSupported == Supported.Supported ? " depth sensor" : "")}";
            _resolutionInfoText.text = configtext;
            _statusInfoText.text = "Position: " + Utils.FormatVector3Position(_arcamera.transform.position) + "\n" + "Rotation: " + Utils.FormatVector3Position(_arcamera.transform.rotation.eulerAngles) + "\n" + "Projection:\n" + _projection;

            // Caluculate aspectratio
            float textureAspectRatio = (m_OcclusionManager.humanDepthTexture == null) ? 1.0f : ((float)m_OcclusionManager.humanDepthTexture.width / (float)m_OcclusionManager.humanDepthTexture.height);
            if (_textureAspectRatio != textureAspectRatio)
            {
                UpdateRawImage(textureAspectRatio);
            }

            // [Debug用]Previewに格納
            _depthPreview.texture = _sourceDepthTexture;
            _stencilPreview.texture = _sourceStencilTexture;
        }

        private void UpdateRawImage(float textureAspectRatio)
        {
            float minDimension = 500.0f;
            float maxDimension = Mathf.Round(minDimension * textureAspectRatio);
            Vector2 rectSize = new Vector2(maxDimension, minDimension);

            // Determine the raw image material and maxDistance material parameter based on the display mode.
            // DepthMaterialがなにやっているか不明。。。とりあえず無視。

            // Update the raw image dimensions and the raw image material parameters.
            _depthPreview.rectTransform.sizeDelta = rectSize;
            _stencilPreview.rectTransform.sizeDelta = rectSize;
            _stencilPreview.rectTransform.position = new Vector3(_depthPreview.rectTransform.position.x + maxDimension, _depthPreview.rectTransform.position.y, _depthPreview.rectTransform.position.z);
        }

        unsafe private void OnCameraFrameEventReceived(ARCameraFrameEventArgs cameraFrameEventArgs)
        {
            //1. CreateCameraFeedTexture
            RefreshCameraFeedTexture();

            //2. Meke XML for Metadata used by NDI connection
            makeXML4Metadata(cameraFrameEventArgs);

            //3. Create UYVA image
            ComputeBuffer converted = Capture();
            if (converted == null)
            {
                return;
            }

            //4. Send Image via NDI
            SendVideo(converted);

            //5. Meke XML for BG verices related Metadata used by NDI connection
            if (_otavjMeshManager.m_MeshMap.Count == 0) return;
            var isSend = makeXML4BgMetadata();

            //6. Send Metadata via NDI
            if (!isSend) return;
            SendMetadata();
        }

        private void makeXML4Metadata(ARCameraFrameEventArgs args)
        {
            if (args.projectionMatrix.HasValue)
            {
                _projection = args.projectionMatrix.Value;

                // Aspect ratio compensation (camera vs. 16:9)
                //_projection[1, 1] *= (16.0f / 9) / _camera.aspect;
            }

            MetadataInfo metainfo = new MetadataInfo(_arcamera.transform.position, _arcamera.transform.rotation, _projection, _minDepth, _maxDepth, _inputHandler.GetToggles(), _inputHandler.GetSliders());
            String jsonString = JsonConvert.SerializeObject(metainfo);
            _metadataStr = $"<metadata><![CDATA[{jsonString}]]></metadata>";
            //Debug.Log(_metadataStr);
        }


        private bool makeXML4BgMetadata()
        {
            if (_bgmetadatamanager.SentMeshMap == null)
            {
                _bgmetadatamanager.InitializeManager(_otavjMeshManager.m_MeshMap);
                return false;
            }
            var bgmeshinfo = _bgmetadatamanager.getDeltaVerticesInfo(_otavjMeshManager.m_MeshMap);
            var deletedMesh = _bgmetadatamanager.getDeletedMeshList(_otavjMeshManager.m_MeshMap);
            if (bgmeshinfo.Count == 0 && deletedMesh.Count == 0)
            {
                return false;
            }
            BgMetadataInfo metainfo = new BgMetadataInfo(bgmeshinfo, deletedMesh);
            String jsonString = JsonConvert.SerializeObject(metainfo);
            _metadataStr4bg = $"<metadata><![CDATA[{jsonString}]]></metadata>";
            //Debug.Log(_metadataStr4bg);
            return true;
        }

        unsafe private void RefreshCameraFeedTexture()
        {
            XRCpuImage image;
            if (!m_CameraManager.TryAcquireLatestCpuImage(out image))
                return;

            var conversionParams = new XRCpuImage.ConversionParams
            (
                image,
                TextureFormat.RGBA32,
                XRCpuImage.Transformation.None
            );

            if (_sourceOriginTexture == null || _sourceOriginTexture.width != image.width || _sourceOriginTexture.height != image.height)
            {
                _sourceOriginTexture = new Texture2D(conversionParams.outputDimensions.x,
                                         conversionParams.outputDimensions.y,
                                         conversionParams.outputFormat, false);
            }

            var buffer = _sourceOriginTexture.GetRawTextureData<byte>();
            image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);

            _sourceOriginTexture.Apply();

            buffer.Dispose();
            image.Dispose();

            _sourceDepthTexture = m_OcclusionManager.environmentDepthTexture;
            _sourceStencilTexture = m_OcclusionManager.humanStencilTexture;

            _muxMaterial.SetTexture("_SourceTex", _sourceOriginTexture);
            _muxMaterial.SetTexture("_HumanStencil", _sourceStencilTexture);
            _muxMaterial.SetTexture("_EnvironmentDepth", _sourceDepthTexture);
            var range = new Vector2(_minDepth, _maxDepth);
            _muxMaterial.SetVector("_DepthRange", range);
            _senderRT.Release();
            Graphics.Blit(null, _senderRT, _muxMaterial, 0);
        }

        private ComputeBuffer Capture()
        {
            // #if !UNITY_EDITOR && UNITY_ANDROID
            //             bool vflip = true;
            // #else
            //             bool vflip = false;
            // #endif
            // vflipはiOSの場合常にfalse
            bool vflip = false;
            _width = _senderRT.width;
            _height = _senderRT.height;
            ComputeBuffer converted = _formatConverter.Encode(_senderRT, _enableAlpha, vflip);
            return converted;
        }

        private unsafe void SendVideo(ComputeBuffer buffer)
        {
            if (_nativeArray == null)
            {
                // for UYVY
                int length = Utils.FrameDataCount(_width, _height, _enableAlpha) * 4;
                // for RGB
                //int length = _width * _height * 4;
                _nativeArray = new NativeArray<byte>(length, Allocator.Persistent);
                _bytes = new byte[length];
            }
            buffer.GetData(_bytes);
            _nativeArray.Value.CopyFrom(_bytes);

            void* pdata = NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(_nativeArray.Value);

            // Data size verification
            //if (_nativeArray.Value.Length / sizeof(uint) != Utils.FrameDataCount(_width, _height, _enableAlpha))
            //{
            //    return;
            //}

            // metadata test
            //string stringA = "hello ota!!";
            //string stringA = "<? xml version = \"1.0\" ?><PurchaseOrder PurchaseOrderNumber = \"99503\"></PurchaseOrder>";
            //string stringA = "<PurchaseOrder PurchaseOrderNumber = \"99503\">test!!</PurchaseOrder>";
            IntPtr pmetadata = Marshal.StringToHGlobalAnsi(_metadataStr);


            // Frame data setup
            var frame = new NDIlib.video_frame_v2_t
            {
                xres = _width,
                yres = _height,
                // for yuva
                line_stride_in_bytes = _width * 2,
                // for rgba
                //line_stride_in_bytes = _width * 4,
                frame_rate_N = _frameRateNumerator,
                frame_rate_D = _frameRateDenominator,
                // for yuva
                FourCC = NDIlib.FourCC_type_e.FourCC_type_UYVA,
                // for rgba
                //FourCC = NDIlib.FourCC_type_e.FourCC_type_RGBA,
                frame_format_type = NDIlib.frame_format_type_e.frame_format_type_progressive,
                p_data = (IntPtr)pdata,
                p_metadata = pmetadata
                //p_metadata = IntPtr.Zero,
            };

            // Send via NDI
            NDIlib.send_send_video_async_v2(_sendInstance, ref frame);

            //後処理
            //メモリーリークしているように見えない。。。何故に。。
            //とりあえずコメントにしておく
            //Marshal.FreeHGlobal(pmetadata);
            //pmetadata = IntPtr.Zero;
        }

        private unsafe void SendMetadata()
        {
            IntPtr pmetadata = Marshal.StringToHGlobalAnsi(_metadataStr4bg);

            var frame = new NDIlib.metadata_frame_t
            {
                length = 0,
                p_data = (IntPtr)pmetadata
            };

            // Send via NDI
            NDIlib.send_send_metadata_64(_sendInstance, ref frame);

            //後処理
            //メモリーリークしているように見えない。。。何故に。。
            //とりあえずコメントにしておく
            //Marshal.FreeHGlobal(pmetadata);
            //pmetadata = IntPtr.Zero;
        }

    }
}