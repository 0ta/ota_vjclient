using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ota.ndi
{
     public static class Utils
    {
        public static int FrameDataCount(int width, int height, bool alpha)
            => width * height * (alpha ? 3 : 2) / 4;

        public static string FormatVector3Position(Vector3 v)
            =>  $"({v.x,7:F2}, {v.y,7:F2}, {v.z,7:F2})";
        
    }

}
