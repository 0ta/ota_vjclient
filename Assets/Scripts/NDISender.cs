using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

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

        [SerializeField] private RawImage _preview;
        [SerializeField] private RawImage _tmppreview;

        private IFrameTextureSource _frameTextureSource;
        private IntPtr _sendInstance;
        private FormatConverter _formatConverter;
        private int _width;
        private int _height;

        private NativeArray<byte>? _nativeArray;
        private byte[] _bytes;

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

            StartCoroutine(CaptureCoroutine());
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

        private ComputeBuffer Capture()
        {
            // #if !UNITY_EDITOR && UNITY_ANDROID
            //             bool vflip = true;
            // #else
            //             bool vflip = false;
            // #endif
            bool vflip = false;
            if (!_frameTextureSource.IsReady) return null;

            Texture texture = _frameTextureSource.GetTexture();
            _preview.texture = texture;

            _width = texture.width;
            _height = texture.height;

            ComputeBuffer converted = _formatConverter.Encode(texture, _enableAlpha, vflip);

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






            //for check
            //Debug.Log("test!!");
            //StreamWriter sw = new StreamWriter("../TextDataClient.txt", false);
            //for (int i = 0; i < _bytes.Length; i++)
            //{
            //    Debug.Log(_bytes[i]);
            //    sw.WriteLine(_bytes[i]);
            //}
            //sw.Flush();
            //sw.Close();
            //throw new SystemException();
            //Texture2D testtex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            //testtex.SetPixelData(_bytes, 0, 0);
            //testtex.LoadRawTextureData(_bytes);
            //testtex.Apply();
            //Destroy(_tmppreview.texture);
            //_tmppreview.texture = testtex;

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