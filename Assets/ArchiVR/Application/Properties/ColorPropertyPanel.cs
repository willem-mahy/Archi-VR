using UnityEngine;
using UnityEngine.UI;

namespace ArchiVR.Application.Properties
{
    public class ColorPropertyPanel
        : MonoBehaviour
    {
        public Slider sliderR;
        public Slider sliderG;
        public Slider sliderB;
        public Slider sliderA;

        public Property<Color> Property;

        public void Initialize()
        {
            sliderR.minValue = 0;
            sliderR.maxValue = 1;
            sliderR.SetValueWithoutNotify(Property.Value.r);
            sliderR.onValueChanged.AddListener((float value) => { SliderROnValueChanged(value); });

            sliderG.minValue = 0;
            sliderG.maxValue = 1;
            sliderG.SetValueWithoutNotify(Property.Value.g);
            sliderG.onValueChanged.AddListener((float value) => { SliderGOnValueChanged(value); });

            sliderB.minValue = 0;
            sliderB.maxValue = 1;
            sliderB.SetValueWithoutNotify(Property.Value.b);
            sliderB.onValueChanged.AddListener((float value) => { SliderBOnValueChanged(value); });

            sliderA.minValue = 0;
            sliderA.maxValue = 1;
            sliderA.SetValueWithoutNotify(Property.Value.a);
            sliderA.onValueChanged.AddListener((float value) => { SliderAOnValueChanged(value); });
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                sliderR.value+=0.1f;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                sliderR.value -= 0.1f;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                sliderG.value += 0.1f;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                sliderG.value -= 0.1f;
            }
        }

        private void SliderROnValueChanged(float value)
        {
            var color = Property.Value;
            color.r = value;
            Property.Value = color;
        }

        private void SliderGOnValueChanged(float value)
        {
            var color = Property.Value;
            color.g = value;
            Property.Value = color;
        }

        private void SliderBOnValueChanged(float value)
        {
            var color = Property.Value;
            color.b = value;
            Property.Value = color;
        }

        private void SliderAOnValueChanged(float value)
        {
            var color = Property.Value;
            color.a = value;
            Property.Value = color;
        }
    }
}