using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WM;

namespace ArchiVR.Application.Properties
{
    public class StringPropertyPanel
        : MonoBehaviour
    {
        public Dropdown Dropdown;

        public Property<string> Property;

        public void Initialize()
        {
            Dropdown.options.Clear();

            foreach (var value in Property.DefaultValues)
            {
                Dropdown.options.Add(new Dropdown.OptionData(value));
            }

            var options = new List<string>(Property.DefaultValues);

            var index = options.IndexOf(Property.Value);

            Dropdown.SetValueWithoutNotify(index);

            Dropdown.onValueChanged.AddListener((int value) => { DropdownOnValueChanged(value); });
        }

        /// <summary>
        /// 'OnValueChanged' handler for the 'Value' dropdown.
        /// </summary>
        public void DropdownOnValueChanged(int value)
        {
            Property.Value = Dropdown.options[value].text;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Dropdown.value = UtilIterate.MakeCycle(Dropdown.value + 1, 0, Dropdown.options.Count);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Dropdown.value = UtilIterate.MakeCycle(Dropdown.value - 1, 0, Dropdown.options.Count);
            }
        }
    }
}


