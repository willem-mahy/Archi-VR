using ArchiVR.Application;
using WM.UI;

namespace ArchiVR.UI
{
    public class WalkthroughMenuPanel : MenuPanel<ApplicationArchiVR>
    {
        #region UI Event Handlers

        /// <summary>
        /// 'On Click' event handler for the 'POI' button.
        /// </summary>
        public void OnClickPoiButton()
        {
            Application.Logger.Debug("OnClickPoiButton()");

            var applicationState = new ApplicationStateEditObject<POIDefinition>(
                Application,
                "POI",
                ref Application.PoiObjects,
                ref Application.ProjectData.POIData.poiDefinitions,
                Application.POIEditSettings);

            Application.PushApplicationState(applicationState);
        }

        /// <summary>
        /// 'On Click' event handler for the 'Light' button.
        /// </summary>
        public void OnClickLightButton()
        {
            Application.Logger.Debug("OnClickLightButton()");

            var applicationState = new ApplicationStateEditObject<LightDefinition>(
                Application,
                "Light",
                ref Application.LightingObjects,
                ref Application.ProjectData.LightingData.lightDefinitions,
                Application.LightingEditSettings);

            Application.PushApplicationState(applicationState);
        }

        /// <summary>
        /// 'On Click' event handler for the 'Prop' button.
        /// </summary>
        public void OnClickPropButton()
        {
            Application.Logger.Debug("OnClickPropButton()");

            var applicationState = new ApplicationStateEditObject<PropDefinition>(
                Application,
                "Prop",
                ref Application.PropObjects,
                ref Application.ProjectData.PropData.propDefinitions,
                Application.PropsEditSettings);

            Application.PushApplicationState(applicationState);
        }

        #endregion UI Event Handlers
    }
}
