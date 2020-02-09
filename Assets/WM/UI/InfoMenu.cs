using UnityEngine;
using UnityEngine.UI;
using WM.Application;

namespace WM.UI
{
    /// <summary>
    /// Menu that shows general information about the software and the system on which it is running.
    /// </summary>
    public class InfoMenu : MonoBehaviour
    {
        #region Variables

        /// <summary>
        /// The application.
        /// </summary>
        public UnityApplication Application;

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

        // Start is called before the first frame update
        void Start()
        {
            #region Get references to GameObjects.

            if (Application == null)
            {
                Application = UtilUnity.FindGameObjectElseError(gameObject.scene, "Application").GetComponent<UnityApplication>();
            }

            #endregion

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