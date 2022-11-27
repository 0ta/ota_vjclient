using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ota.ndi {
    public class SliderManager : MonoBehaviour
    {
        public TextMeshProUGUI _maxTxt;
        public TextMeshProUGUI _minTxt;
        public TextMeshProUGUI _currentTxt;
        public RawImage DefaultValuePosition;
        public GameObject InputHandlerObj;

        public float DefaultValue;

        private Slider myslider;
        private InputHandler inputHandler;
        public int index;

        void Start()
        {
            var match = Regex.Match(this.name, @"[0-9]+");
            index = int.Parse(match.Value);

            inputHandler = InputHandlerObj.GetComponent<InputHandler>();
            InputHandlerObj.GetComponent<UIControlRegistry>().Regist(this);
            InputHandlerObj.GetComponent<InputHandler>().SetSlider(index, DefaultValue);

            myslider = this.GetComponent<Slider>();
            _minTxt.text = myslider.minValue.ToString();
            _maxTxt.text = "/ " + myslider.maxValue.ToString();

            var t = this.GetComponent<RectTransform>();
            var absposition = (DefaultValue - myslider.minValue) / (myslider.maxValue - myslider.minValue);
            var startposition = DefaultValuePosition.rectTransform.anchoredPosition.x - t.sizeDelta.x / 2;
            var x = Mathf.Lerp(startposition, startposition + t.sizeDelta.x, absposition);
            DefaultValuePosition.rectTransform.anchoredPosition = new Vector3(x, DefaultValuePosition.rectTransform.anchoredPosition.y, 0);

            setDefaultValue();
        }

        void Update()
        {
            _currentTxt.text = myslider.value.ToString();
        }

        void setDefaultValue()
        {
            myslider.value = DefaultValue;
            _currentTxt.text = DefaultValue.ToString();
        }

        public void onClick()
        {
            setDefaultValue();
        }

        public void onValueChanged()
        {
            inputHandler.SetSlider(index, myslider.value);
            //inputHandler.settestslider(myslider.value);
        }

        public void doReset()
        {
            setDefaultValue();
        }
    }
}

