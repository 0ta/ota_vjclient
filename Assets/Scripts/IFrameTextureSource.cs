using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ota.ndi
{
    public interface IFrameTextureSource
    {
        bool IsReady { get; set; }
        Texture GetTexture();
    }
}
