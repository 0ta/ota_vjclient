using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ota.ndi
{
    public class RTTextureSourceContainer : MonoBehaviour, IFrameTextureSource
    {
        [SerializeField] private RenderTexture _ndiClientRenderTexture;
        [SerializeField] private bool _isReady;

        public bool IsReady { get; set; }
        public Texture GetTexture()
        {
            return _ndiClientRenderTexture;
        }

        void Awake()
        {
            this.IsReady = _isReady;
        }


    }
}
