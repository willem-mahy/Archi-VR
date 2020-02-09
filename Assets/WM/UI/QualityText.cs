using UnityEngine;
using UnityEngine.UI;

namespace WM.UI
{
    /// <summary>
    /// Add this as a component to a UnityEngine.UI.Text object, to make it display the name of the current Quality setting.
    /// </summary>
    public class QualityText : MonoBehaviour
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

            var qualityLevel = QualitySettings.GetQualityLevel();
            var qualityLevelName = QualitySettings.names[qualityLevel];

            text.text = "(" + qualityLevelName + ")";
        }
    }
}
