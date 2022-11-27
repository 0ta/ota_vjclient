using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ota.ndi {
    public class ToggleManager : MonoBehaviour
    {
        public GameObject InputHandlerObj;

        public bool DefaultValue;

        private Toggle mytoggle;
        //private InputHandler inputHandler;
        public int index;

        void Start()
        {
            var match = Regex.Match(this.name, @"[0-9]+");
            index = int.Parse(match.Value);

            //inputHandler = InputHandlerObj.GetComponent<InputHandler>();
            InputHandlerObj.GetComponent<UIControlRegistry>().Regist(this);
            InputHandlerObj.GetComponent<InputHandler>().SetToggle(index, DefaultValue);

            mytoggle = this.GetComponent<Toggle>();

            setDefaultValue();
        }

        void setDefaultValue()
        {
            mytoggle.isOn = DefaultValue;
        }

        public void doReset()
        {
            setDefaultValue();
        }
    }
}

