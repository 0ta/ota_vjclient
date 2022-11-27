using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ota.ndi
{
    public sealed class UIControlRegistry : MonoBehaviour
    {
        private Dictionary<int, SliderManager> _sliderDic = new Dictionary<int, SliderManager>();
        private Dictionary<int, ToggleManager> _toggleDic = new Dictionary<int, ToggleManager>();

        public void Regist(SliderManager sm) {
            _sliderDic.Add(sm.index,sm);
        }

        public void Regist(ToggleManager tm)
        {
            _toggleDic.Add(tm.index, tm);
        }

        public Dictionary<int, SliderManager> GetSliderDic() => _sliderDic;
        public Dictionary<int, ToggleManager> GetToggleDic() => _toggleDic;
    }
}

