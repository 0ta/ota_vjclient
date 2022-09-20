using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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
        [SerializeField] private GameObject _frameTextureSourceContainer;
        [SerializeField] private int _frameRateNumerator = 30000;
        [SerializeField] private int _frameRateDenominator = 1001;

        [SerializeField] private Camera _arcamera;

        [SerializeField] private RawImage _preview;
        [SerializeField] private TextMeshProUGUI _informationText;

        [SerializeField]
        [Tooltip("The AROcclusionManager which will produce depth textures.")]
        AROcclusionManager m_OcclusionManager;
        [SerializeField]
        [Tooltip("The ARCameraManager which will produce frame events.")]
        ARCameraManager m_CameraManager;

        private IFrameTextureSource _frameTextureSource;
        private IntPtr _sendInstance;
        private FormatConverter _formatConverter;
        private int _width;
        private int _height;

        private NativeArray<byte>? _nativeArray;
        private byte[] _bytes;

        private Texture2D _sourceOriginTexture;

        // Start is called before the first frame update
        void Start()
        {
            //WifiManager.Instance.SetupNetwork();

            if (!NDIlib.Initialize())
            {
                Debug.Log("NDIlib can't be initialized.");
                return;
            }

            _frameTextureSource = _frameTextureSourceContainer.GetComponent<IFrameTextureSource>();

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

            m_CameraManager.frameReceived += OnCameraFrameEventReceived;
            //m_DisplayRotationMatrix = Matrix4x4.identity;

            //StartCoroutine(CaptureCoroutine());
        }

        unsafe private void OnCameraFrameEventReceived(ARCameraFrameEventArgs cameraFrameEventArgs)
        {
            //1. CreateCameraFeedTexture
            RefreshCameraFeedTexture();

            //2. Create UYVA image
            ComputeBuffer converted = Capture();
            if (converted == null)
            {
                return;
            }

            //3. Send Image via NDI
            Send(converted);
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
        }

        // Temporary method
        private void Update()
        {
            //
            // まずはLandscapeのみ考慮
            //

            // Camera manager related information text is displayed
            var config = m_CameraManager.currentConfiguration;
            var configtext = $"{config?.width}x{config?.height}{((bool)(config?.framerate.HasValue) ? $" at {config?.framerate.Value} Hz" : "")}{(config?.depthSensorSupported == Supported.Supported ? " depth sensor" : "")}";
            _informationText.text = configtext;

            // Caluculate aspectratio
            float textureAspectRatio = (m_OcclusionManager.humanDepthTexture == null) ? 1.0f : ((float)m_OcclusionManager.humanDepthTexture.width / (float)m_OcclusionManager.humanDepthTexture.height);
            UpdateRawImage(textureAspectRatio);
        }

        void UpdateRawImage(float textureAspectRatio)
        {
            float minDimension =  700.0f;
            float maxDimension = Mathf.Round(minDimension * textureAspectRatio);
            Vector2 rectSize = new Vector2(maxDimension, minDimension);

            // Determine the raw image material and maxDistance material parameter based on the display mode.
            // DepthMaterialがなにやっているか不明。。。とりあえず無視。

            // Update the raw image dimensions and the raw image material parameters.
            _preview.rectTransform.sizeDelta = rectSize;
        }

        void OnDestroy()
        {
            ReleaseInternalObjects();
        }

        private void ReleaseInternalObjects()
        {
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

        private IEnumerator CaptureCoroutine()
        {
            for (var eof = new WaitForEndOfFrame(); true;)
            {
                yield return eof;

                ComputeBuffer converted = Capture();
                if (converted == null)
                {
                    continue;
                }

                Send(converted);
            }
        }

        //private ComputeBuffer Capture()
        //{
        //    // #if !UNITY_EDITOR && UNITY_ANDROID
        //    //             bool vflip = true;
        //    // #else
        //    //             bool vflip = false;
        //    // #endif
        //    bool vflip = false;
        //    if (!_frameTextureSource.IsReady) return null;

        //    Texture texture = _frameTextureSource.GetTexture();
        //    _preview.texture = texture;

        //    _width = texture.width;
        //    _height = texture.height;

        //    ComputeBuffer converted = _formatConverter.Encode(texture, _enableAlpha, vflip);

        //    return converted;
        //}

        private ComputeBuffer Capture()
        {
            // #if !UNITY_EDITOR && UNITY_ANDROID
            //             bool vflip = true;
            // #else
            //             bool vflip = false;
            // #endif
            // vflipはiOSの場合常にfalse
            bool vflip = false;

            // ARCameraのスナップショットを取得してBinary化する
            _arcamera.targetTexture = (RenderTexture)_frameTextureSource.GetTexture();
            var currentRT = RenderTexture.active;
            RenderTexture.active = _arcamera.targetTexture;
            _arcamera.Render();
            Texture texture = _arcamera.targetTexture;

            // [Debug用]Previewに格納
            //_preview.texture = texture;
            _preview.texture = m_OcclusionManager.humanDepthTexture;
            _width = _sourceOriginTexture.width;
            _height = _sourceOriginTexture.height;
            ComputeBuffer converted = _formatConverter.Encode(_sourceOriginTexture, _enableAlpha, vflip);
            //ComputeBuffer converted = _formatConverter.Encode(texture, _enableAlpha, vflip);

            // RenderTextureを元に戻す
            RenderTexture.active = currentRT;
            _arcamera.targetTexture = null;

            return converted;
        }

        private unsafe void Send(ComputeBuffer buffer)
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
            string stringA = "<PurchaseOrder PurchaseOrderNumber = \"99503\">test!!</PurchaseOrder>";
            IntPtr pmetadata = Marshal.StringToHGlobalAnsi(stringA);


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

        //private unsafe void Send(ComputeBuffer buffer)
        //{
        //    if (_nativeArray == null)
        //    {
        //        int length = Utils.FrameDataCount(_width, _height, _enableAlpha) * 4;
        //        _nativeArray = new NativeArray<byte>(length, Allocator.Persistent);

        //        _bytes = new byte[length];
        //    }

        //    buffer.GetData(_bytes);
        //    _nativeArray.Value.CopyFrom(_bytes);

        //    void* pdata = NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(_nativeArray.Value);

        //    // Data size verification
        //    if (_nativeArray.Value.Length / sizeof(uint) != Utils.FrameDataCount(_width, _height, _enableAlpha))
        //    {
        //        return;
        //    }

        //    // Frame data setup
        //    var frame = new NDIlib.video_frame_v2_t
        //    {
        //        xres = _width,
        //        yres = _height,
        //        line_stride_in_bytes = _width * 2,
        //        frame_rate_N = _frameRateNumerator,
        //        frame_rate_D = _frameRateDenominator,
        //        FourCC = _enableAlpha ? NDIlib.FourCC_type_e.FourCC_type_UYVA : NDIlib.FourCC_type_e.FourCC_type_UYVY,
        //        frame_format_type = NDIlib.frame_format_type_e.frame_format_type_progressive,
        //        p_data = (IntPtr)pdata,
        //        p_metadata = IntPtr.Zero,
        //    };

        //    // Send via NDI
        //    NDIlib.send_send_video_async_v2(_sendInstance, ref frame);
        //}
    }
}