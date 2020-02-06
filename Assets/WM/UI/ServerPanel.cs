using UnityEngine;
using UnityEngine.UI;

namespace WM.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class ServerPanel : MonoBehaviour
    {
        public Button StopServerButton;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        void OnEnable()
        {
            // When this menu is activated...
            if (StopServerButton != null)
            {
                StopServerButton.Select(); // ... then put the UI focus on the 'Server' toogle.
            }
        }
    }
} // namespace WM.UI