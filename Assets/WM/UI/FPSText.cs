using UnityEngine;
using UnityEngine.UI;

namespace WM.UI
{
    /// <summary>
    /// Add this as a component to a UnityEngine.UI.Text object, to make it display the current frames-per-seconds value.
    /// </summary>
    public class FPSText : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        private Text text = null;

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {
            text = gameObject.GetComponent<Text>();
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        void Update()
        {
            if (text == null)
            {
                return;
            }

            var fps = (int)(1f / Time.unscaledDeltaTime);

            text.text = fps.ToString();
        }
    }
} // namespace WM.UI
