using ArchiVR.Application;
using UnityEngine;
using UnityEngine.UI;
using WM.UI;

namespace ArchiVR.UI
{
    /// <summary>
    /// The menu shown while in edit mode.
    /// </summary>
    public class EditMenuPanel : MenuPanel<ApplicationArchiVR>
    {
        public void Update()
        {
            if (null != ApplicationState)
            {
                var colors = ButtonLight.colors;
                colors.normalColor = (0 == ApplicationState.ActiveObjectTypeIndex) ? ActiveButtonColor : InactiveButtonColor;
                ButtonLight.colors = colors;

                colors = ButtonProp.colors;
                colors.normalColor  = (1 == ApplicationState.ActiveObjectTypeIndex) ? ActiveButtonColor : InactiveButtonColor;
                ButtonProp.colors = colors;

                colors = ButtonPOI.colors;
                colors.normalColor   = (2 == ApplicationState.ActiveObjectTypeIndex) ? ActiveButtonColor : InactiveButtonColor;
                ButtonPOI.colors = colors;
            }
        }
        #region UI Event Handlers

        /// <summary>
        /// 'On Click' event handler for the 'POI' button.
        /// </summary>
        public void OnClickPoiButton()
        {
            Application.Logger.Debug("OnClickPoiButton()");

            ApplicationState.ActiveObjectTypeIndex = 2;
        }

        /// <summary>
        /// 'On Click' event handler for the 'Light' button.
        /// </summary>
        public void OnClickLightButton()
        {
            Application.Logger.Debug("OnClickLightButton()");

            ApplicationState.ActiveObjectTypeIndex = 0;
        }

        /// <summary>
        /// 'On Click' event handler for the 'Prop' button.
        /// </summary>
        public void OnClickPropButton()
        {
            Application.Logger.Debug("OnClickPropButton()");

            ApplicationState.ActiveObjectTypeIndex = 1;
        }

        #endregion UI Event Handlers

        public ApplicationStateEdit ApplicationState;

        public Button ButtonLight;

        public Button ButtonProp;

        public Button ButtonPOI;

        public Color InactiveButtonColor = new Color(1, 1, 1);

        public Color ActiveButtonColor = new Color(0, 1, 0);
    }
}
