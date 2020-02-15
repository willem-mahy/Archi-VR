using UnityEngine;
using UnityEngine.UI;

namespace WM.UI
{
    /// <summary>
    /// World-space UI components attached to a controller button.
    /// This UI consists of a Canvas, containing a Panel, Containing a Text.
    /// The UI is used to display to the user:
    /// - the functionality for the associated button. (What command/action is mapped to it.)
    /// - the state of the associated button. (Down, Pressed, Up)
    /// </summary>
    public class ControllerButtonUI : MonoBehaviour
    {
        /// <summary>
        ///  The Canvas.
        /// </summary>
        private Canvas _canvas;

        /// <summary>
        /// The Panel.
        /// </summary>
        private GameObject _panel;

        /// <summary>
        /// The Text.
        /// </summary>
        private Text _text;

        #region Public API

        void Start()
        {
            _canvas = gameObject.GetComponent<Canvas>();

            _panel = transform.Find("Panel").gameObject;

            _text = _panel.transform.Find("Text").gameObject.GetComponent<Text>();
        }

        /// <summary>
        /// The displayed text.
        /// </summary>
        public string Text
        {
            get
            {
                return _text.text;
            }
            set
            {
                _text.text = value;

                gameObject.SetActive((value != null) && (value != ""));
            }
        }

        /// <summary>
        /// The text color.
        /// </summary>
        public Color TextColor
        {
            get
            {
                return _text.color;
            }
            set
            {
                _text.color = value;
            }
        }

        #endregion Public API
    }
}
