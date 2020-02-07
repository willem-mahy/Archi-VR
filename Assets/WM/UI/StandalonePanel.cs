using UnityEngine;
using UnityEngine.UI;
using WM.Application;

namespace WM.UI
{
    /// <summary>
    /// The menu that shows the state and options of the 'Standalone' network mode.
    /// </summary>
    public class StandalonePanel : MonoBehaviour
    {
        #region Variables

        public UnityApplication Application;

        public Button JoinServerButton;

        public Button StartServerButton;

        #endregion Variables

        /// <summary>
        /// Start is called once in the lifetime of this behavior, just before the first frame update.
        /// </summary>
        void Start()
        {
            #region Get references to GameObjects.

            if (Application == null)
            {
                var applicationGO = UtilUnity.FindGameObjectElseError(gameObject.scene, "Application");
                Application = applicationGO.GetComponent<UnityApplication>();
            }

            #endregion
        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {
            JoinServerButton.enabled = (Application.ServerDiscovery.GetServerInfos().Count > 0);
        }


        /// <summary>
        /// 
        /// </summary>
        void OnEnable()
        {
            // When this menu is activated...
            if (StartServerButton != null)
            {
                StartServerButton.Select(); // ... then put the UI focus on the 'Server' toogle.
            }
        }
    }
} // namespace WM.UI