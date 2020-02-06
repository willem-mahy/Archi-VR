using UnityEngine;
using UnityEngine.UI;
using WM.Application;

namespace WM.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class StandalonePanel : MonoBehaviour
    {
        #region Variables

        public UnityApplication Application;

        public Button JoinServerButton;

        public Button StartServerButton;

        #endregion Variables

        // Start is called before the first frame update
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

        // Update is called once per frame
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