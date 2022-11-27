using UnityEngine;

namespace ota.ndi
{
    public sealed class InputHandler : MonoBehaviour
    {
        bool[] _buttons = new bool[16];
        bool[] _toggles = new bool[32];
        float[] _sliders = new float[16];

        public bool Button0 { get => _buttons[0]; set => _buttons[0] = value; }
        public bool Button1 { get => _buttons[1]; set => _buttons[1] = value; }
        public bool Button2 { get => _buttons[2]; set => _buttons[2] = value; }
        public bool Button3 { get => _buttons[3]; set => _buttons[3] = value; }
        public bool Button4 { get => _buttons[4]; set => _buttons[4] = value; }
        public bool Button5 { get => _buttons[5]; set => _buttons[5] = value; }
        public bool Button6 { get => _buttons[6]; set => _buttons[6] = value; }
        public bool Button7 { get => _buttons[7]; set => _buttons[7] = value; }
        public bool Button8 { get => _buttons[8]; set => _buttons[8] = value; }
        public bool Button9 { get => _buttons[9]; set => _buttons[9] = value; }
        public bool Button10 { get => _buttons[10]; set => _buttons[10] = value; }
        public bool Button11 { get => _buttons[11]; set => _buttons[11] = value; }
        public bool Button12 { get => _buttons[12]; set => _buttons[12] = value; }
        public bool Button13 { get => _buttons[13]; set => _buttons[13] = value; }
        public bool Button14 { get => _buttons[14]; set => _buttons[14] = value; }
        public bool Button15 { get => _buttons[15]; set => _buttons[15] = value; }

        public bool Toggle0 { get => _toggles[0]; set => _toggles[0] = value; }
        public bool Toggle1 { get => _toggles[1]; set => _toggles[1] = value; }
        public bool Toggle2 { get => _toggles[2]; set => _toggles[2] = value; }
        public bool Toggle3 { get => _toggles[3]; set => _toggles[3] = value; }
        public bool Toggle4 { get => _toggles[4]; set => _toggles[4] = value; }
        public bool Toggle5 { get => _toggles[5]; set => _toggles[5] = value; }
        public bool Toggle6 { get => _toggles[6]; set => _toggles[6] = value; }
        public bool Toggle7 { get => _toggles[7]; set => _toggles[7] = value; }
        public bool Toggle8 { get => _toggles[8]; set => _toggles[8] = value; }
        public bool Toggle9 { get => _toggles[9]; set => _toggles[9] = value; }
        public bool Toggle10 { get => _toggles[10]; set => _toggles[10] = value; }
        public bool Toggle11 { get => _toggles[11]; set => _toggles[11] = value; }
        public bool Toggle12 { get => _toggles[12]; set => _toggles[12] = value; }
        public bool Toggle13 { get => _toggles[13]; set => _toggles[13] = value; }
        public bool Toggle14 { get => _toggles[14]; set => _toggles[14] = value; }
        public bool Toggle15 { get => _toggles[15]; set => _toggles[15] = value; }
        public bool Toggle16 { get => _toggles[16]; set => _toggles[16] = value; }
        public bool Toggle17 { get => _toggles[17]; set => _toggles[17] = value; }
        public bool Toggle18 { get => _toggles[18]; set => _toggles[18] = value; }
        public bool Toggle19 { get => _toggles[19]; set => _toggles[19] = value; }
        public bool Toggle20 { get => _toggles[20]; set => _toggles[20] = value; }
        public bool Toggle21 { get => _toggles[21]; set => _toggles[21] = value; }
        public bool Toggle22 { get => _toggles[22]; set => _toggles[22] = value; }
        public bool Toggle23 { get => _toggles[23]; set => _toggles[23] = value; }
        public bool Toggle24 { get => _toggles[24]; set => _toggles[24] = value; }
        public bool Toggle25 { get => _toggles[25]; set => _toggles[25] = value; }
        public bool Toggle26 { get => _toggles[26]; set => _toggles[26] = value; }
        public bool Toggle27 { get => _toggles[27]; set => _toggles[27] = value; }
        public bool Toggle28 { get => _toggles[28]; set => _toggles[28] = value; }
        public bool Toggle29 { get => _toggles[29]; set => _toggles[29] = value; }
        public bool Toggle30 { get => _toggles[30]; set => _toggles[30] = value; }
        public bool Toggle31 { get => _toggles[31]; set => _toggles[31] = value; }

        public float Slider0 { get => _sliders[0]; set => _sliders[0] = value; }
        public float Slider1 { get => _sliders[1]; set => _sliders[1] = value; }
        public float Slider2 { get => _sliders[2]; set => _sliders[2] = value; }
        public float Slider3 { get => _sliders[3]; set => _sliders[3] = value; }
        public float Slider4 { get => _sliders[4]; set => _sliders[4] = value; }
        public float Slider5 { get => _sliders[5]; set => _sliders[5] = value; }
        public float Slider6 { get => _sliders[6]; set => _sliders[6] = value; }
        public float Slider7 { get => _sliders[7]; set => _sliders[7] = value; }
        public float Slider8 { get => _sliders[8]; set => _sliders[8] = value; }
        public float Slider9 { get => _sliders[9]; set => _sliders[9] = value; }
        public float Slider10 { get => _sliders[10]; set => _sliders[10] = value; }
        public float Slider11 { get => _sliders[11]; set => _sliders[11] = value; }
        public float Slider12 { get => _sliders[12]; set => _sliders[12] = value; }
        public float Slider13 { get => _sliders[13]; set => _sliders[13] = value; }
        public float Slider14 { get => _sliders[14]; set => _sliders[14] = value; }
        public float Slider15 { get => _sliders[15]; set => _sliders[15] = value; }

        public bool GetButton(int index) => _buttons[index];
        public void SetButton(int index, bool value) => _buttons[index] = value;

        public bool GetToggle(int index) => _toggles[index];
        public void SetToggle(int index, bool value) => _toggles[index] = value;

        public float GetSlider(int index) => _sliders[index];
        public void SetSlider(int index, float value) => _sliders[index] = value;

        public bool[] GetButtons()
        {
            //var ret = _buttons.Clone() as bool[];
            //for (int i = 0; i < _buttons.Length; i++)
            //{
            //    _buttons[i] = false;
            //}
            //return ret;
            return _buttons;
        }

        public bool[] GetToggles()
        {
            return _toggles;
        }

        public float[] GetSliders()
        {
            return _sliders;
        }

        public void settesttoggle(bool b)
        {
            Debug.Log(b);
        }

        public void settestslider(float f)
        {
            Debug.Log(f);
        }
    }
}
