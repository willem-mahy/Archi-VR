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

        /// <summary>
        /// The Text.
        /// </summary>
        private string _label;

        /// <summary>
        /// The Text.
        /// </summary>
        private string _editorMapping;

        /// <summary>
        /// The Text.
        /// </summary>
        public string EditorMapping
        {
            get
            {
                return _editorMapping;
            }
            set
            {
                _editorMapping = value;

                UpdateTextText();
            }
        }

        #region Public API

        void Awake()
        {
            _canvas = gameObject.GetComponent<Canvas>();

            _panel = transform.Find("Panel").gameObject;

            if (_panel == null)
            {
                Debug.LogError("ControllerButtonUI.Start(): Could not locate 'Panel' in GameObject '" + gameObject.name + "'!");
            }

            _text = _panel.transform.Find("Text").gameObject.GetComponent<Text>();

            if (_text == null)
            {
                Debug.LogError("ControllerButtonUI.Start(): Could not locate 'Text' in GameObject '" + gameObject.name + "'!");
            }
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
                _label = value;

                UpdateTextText();
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

        /// <summary>
        /// Updates the text of the Text.
        /// </summary>
        private void UpdateTextText()
        {
            if (string.IsNullOrEmpty(_label))
            {
                gameObject.SetActive(false);
                return;
            }

            var text = _label;

            if (UnityEngine.Application.isEditor && !string.IsNullOrEmpty(_editorMapping))
            {
                text += string.Format(" ({0})", _editorMapping);
            }

            _text.text = text;

            gameObject.SetActive(true);
        }
    }
}
