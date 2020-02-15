using UnityEngine.UI;
using WM.Application;

namespace WM.UI
{
    /// <summary>
    /// The 'Info' menu panel.
    /// Displays general information about:
    /// - the application.
    /// - the system on which the application is running.
    /// </summary>
    public class InfoMenu : MenuPanel<UnityApplication>
    {
        #region Variables

        /// <summary>
        /// The UI Text to display the application version.
        /// </summary>
        public Text ApplicationVersionText;

        /// <summary>
        /// The UI Text to display the application version.
        /// </summary>
        public Text ApplicationNameText;

        /// <summary>
        /// The UI Text to display the system's IP.
        /// </summary>
        public Text SystemIPText;

        #endregion

        #region GameObject overrides

        /// <summary>
        /// Start is called before the first frame update.
        /// </summary>
        override public void Start()
        {
            base.Start();

            #region Get references to UI components.

            if (ApplicationNameText == null)
            {
                var applicationNameGO = UtilUnity.TryFindGameObject("InfoMenu_Application_NameValueText");

                if (applicationNameGO != null)
                {
                    ApplicationNameText = applicationNameGO.GetComponent<Text>();
                }
            }

            if (ApplicationVersionText == null)
            {
                var applicationVersionGO = UtilUnity.TryFindGameObject("InfoMenu_Application_VersionValueText");

                if (applicationVersionGO != null)
                {
                    ApplicationVersionText = applicationVersionGO.GetComponent<Text>();
                }
            }

            if (SystemIPText == null)
            {
                var systemIPGO = UtilUnity.TryFindGameObject("InfoMenu_System_IPValueText");

                if (systemIPGO != null)
                {
                    SystemIPText = systemIPGO.GetComponent<Text>();
                }
            }

            #endregion

            if (ApplicationNameText != null)
            {
                ApplicationNameText.text = (Application == null) ? "NA" : Application.Name;
            }

            if (ApplicationVersionText != null)
            {
                ApplicationVersionText.text = (Application == null) ? "NA" : Application.Version;
            }

            if (SystemIPText != null)
            {
                SystemIPText.text = WM.Net.NetUtil.GetLocalIPAddress().ToString();
            }
        }

        #endregion
    }
} // namespace WM.UI