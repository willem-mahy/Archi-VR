using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WM;
using WM.UI;

namespace ArchiVR.Application.Properties
{
    public class PropertiesMenu
        : MenuPanel<ApplicationArchiVR>
    {
        public StringPropertyPanel StringPropertyPanel;
        public ColorPropertyPanel ColorPropertyPanel;
        public FloatPropertyPanel FloatPropertyPanel;

        private IProperties _properties;

        private List<GameObject> _propertyPanels = new List<GameObject>();

        public IProperties Properties
        {
            get
            {
                return _properties;
            }
            set
            {
                _properties = value;

                OnPropertiesChanged();
            }
        }

        #region Private API

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        private void CreateProperyPanel(
            IProperty property)
        {
            GameObject propertyPanel = null;

            if (property is Property<string> stringProperty)
            {
                propertyPanel = Instantiate(StringPropertyPanel.gameObject);
                var stringPropertyPanel = propertyPanel.GetComponent<StringPropertyPanel>();
                stringPropertyPanel.Property = stringProperty;
                stringPropertyPanel.Initialize();
            }
            else if (property is Property<Color> colorProperty)
            {
                propertyPanel = Instantiate(ColorPropertyPanel).gameObject;
                var colorPropertyPanel = propertyPanel.GetComponent<ColorPropertyPanel>();
                colorPropertyPanel.Property = colorProperty;
                colorPropertyPanel.Initialize();
            }
            else if (property is Property<float> floatProperty)
            {
                propertyPanel = Instantiate(StringPropertyPanel).gameObject;
                var floatPropertyPanel = propertyPanel.GetComponent<FloatPropertyPanel>();
                floatPropertyPanel.Property = floatProperty;
                floatPropertyPanel.Initialize();
            }

            var rectTransform = propertyPanel.GetComponent<RectTransform>();

            var position = rectTransform.position;
            position.y = _yOffset;
            //rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.position = position;

            propertyPanel.transform.SetParent(this.transform, false);
            propertyPanel.SetActive(true);

            _yOffset -= rectTransform.rect.height;

            _propertyPanels.Add(propertyPanel);
        }

        float _yOffset = 0;

        private void OnPropertiesChanged()
        {
            foreach (var panel in _propertyPanels)
            {
                panel.transform.SetParent(null);
                UtilUnity.Destroy(panel);
            }

            _yOffset = 0;

            foreach (var property in _properties.Properties)
            {
                CreateProperyPanel(property);
            }
        }

        /// <summary>
        /// 'On Click' event handler for the 'Close' button.
        /// </summary>
        public void OnClickCloseButton()
        {
            Application.Logger.Debug("PropertiesMenu.OnClickCloseButton()");

            ApplicationState.CloseProperties();
        }

        #endregion Private API

        public ApplicationStateEdit ApplicationState;
    }
}