using UnityEngine;
using UnityEngine.UI;

namespace WM.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class ServerPanel : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public Button StopServerButton;

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