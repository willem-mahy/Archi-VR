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
        #region UI Event Handlers

        /// <summary>
        /// 'On Click' event handler for the 'POI' button.
        /// </summary>
        public void OnClickPoiButton()
        {
            Application.Logger.Debug("OnClickPoiButton()");

            ApplicationState.StartCreatePOI();
        }

        /// <summary>
        /// 'On Click' event handler for the 'Light' button.
        /// </summary>
        public void OnClickLightButton()
        {
            Application.Logger.Debug("EditMenuPanel.OnClickLightButton()");

            ApplicationState.StartCreateLight();
        }

        /// <summary>
        /// 'On Click' event handler for the 'Prop' button.
        /// </summary>
        public void OnClickPropButton()
        {
            Application.Logger.Debug("EditMenuPanel.OnClickPropButton()");

            ApplicationState.StartCreateProp();
        }

        /// <summary>
        /// 'On Click' event handler for the 'Delete' button.
        /// </summary>
        public void OnClickDeleteButton()
        {
            Application.Logger.Debug("EditMenuPanel.OnClickDeleteButton()");

            ApplicationState.Delete();
        }

        /// <summary>
        /// 'On Click' event handler for the 'Properties' button.
        /// </summary>
        public void OnClickPropertiesButton()
        {
            Application.Logger.Debug("EditMenuPanel.OnClickPropertiesButton()");

            ApplicationState.ShowProperties();
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
