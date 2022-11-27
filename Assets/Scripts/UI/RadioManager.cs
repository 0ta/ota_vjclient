using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ota.ndi {
    public class RadioManager : MonoBehaviour
    {
        public List<Toggle> RadioGroup;

        private Toggle mytoggle;

        void Start()
        {
            mytoggle = this.GetComponent<Toggle>();
            if (this.GetComponent<ToggleManager>().DefaultValue == true)
            {
                mytoggle.enabled = false;
            }
        }

        public void onClick(bool b)
        {
            if (b) {
                mytoggle.enabled = false;
                var it = RadioGroup.GetEnumerator();
                while (it.MoveNext())
                {
                    if (!it.Current.name.Equals(this.name))
                    {
                        it.Current.isOn = false;
                        it.Current.enabled = true;
                    }
                }
            }
        }
    }
}

