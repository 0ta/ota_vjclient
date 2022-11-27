using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ota.ndi
{
    public class ResetManager : MonoBehaviour
    {
        public GameObject InputHandlerObj;
        public List<int> SliderList = new List<int>();
        public List<int> ToggleList = new List<int>();

        private UIControlRegistry _registry;

        void Start()
        {
            _registry = InputHandlerObj.GetComponent<UIControlRegistry>();
        }

        public void onClick()
        {
            var sliderIt = SliderList.GetEnumerator();
            while (sliderIt.MoveNext())
            {
                _registry.GetSliderDic()[sliderIt.Current].doReset();
            }
            var toggleIt = ToggleList.GetEnumerator();
            while (toggleIt.MoveNext())
            {
                _registry.GetToggleDic()[toggleIt.Current].doReset();
            }
        }

        public void onClickForAll() {
            var sliderIt = _registry.GetSliderDic().GetEnumerator();
            while(sliderIt.MoveNext()) {
                sliderIt.Current.Value.doReset();
            }
            var toggleIt = _registry.GetToggleDic().GetEnumerator();
            while (toggleIt.MoveNext())
            {
                toggleIt.Current.Value.doReset();
            }
        }
    }
}

