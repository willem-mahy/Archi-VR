using UnityEngine;
using UnityEngine.UI;
using WM;
using WM.Application;

namespace WM.UI
{
    /// <summary>
    /// The 'Debug Input' menu panel.
    /// Shows the actual state of the controller input.
    /// </summary>
    public class DebugLogMenu : MenuPanel<UnityApplication>
    {
        #region Fields

        /// <summary>
        /// 
        /// </summary>
        int MaxNumLines = 35;

        /// <summary>
        /// 
        /// </summary>
        Text Text;

        /// <summary>
        /// 
        /// </summary>
        Button _clearButton;

        /// <summary>
        /// 
        /// </summary>
        Toggle _enableToggle;

        #endregion Fields

        #region Public API

        #region GameObject overrides
        
        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        override public void Start()
        {
            base.Start();

            #region Get references to UI components.

            if (Text == null)
            {
                Text = UtilUnity.FindGameObjectElseError(gameObject.scene, "DebugLogMenu_LogText").GetComponent<Text>();
            }

            if (_clearButton == null)
            {
                _clearButton = UtilUnity.FindGameObjectElseError(gameObject.scene, "DebugLogMenu_ClearButton").GetComponent<Button>();
            }

            if (_enableToggle == null)
            {
                _enableToggle = UtilUnity.FindGameObjectElseError(gameObject.scene, "DebugLogMenu_EnableToggle").GetComponent<Toggle>();
            }

            #endregion
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        void Update()
        {
            if (_enableToggle != null)
            {
                _enableToggle.SetIsOnWithoutNotify(Application.Logger.Enabled);
            }

            if (Text != null)
            {
                var text = "";

                var log = Application.Logger;

                var numLinesInLog = log.Count;
                int numLinesToDisplay = System.Math.Min(numLinesInLog, MaxNumLines);

                for (var lineIndex = 0; lineIndex < numLinesToDisplay; ++lineIndex)
                {
                    if (text.Length > 0)
                    {
                        text += "\n";
                    }

                    text += log[numLinesInLog - (lineIndex + 1)];
                }

                Text.text = text;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void OnEnable()
        {
            if (_clearButton != null)
            {
                _clearButton.Select();
            }
        }

        #endregion GameObject overrides

        #region UI Event Handlers

        /// <summary>
        /// 'OnValueChanged' event handler for the 'EnableLogger' toggle.
        /// </summary>
        public void EnableLoggerToggleOnValueChanged(bool value)
        {
            Application.Logger.Enabled = value;
        }

        /// <summary>
        /// 'OnClick' event handler for the 'Clear' button.
        /// </summary>
        public void ClearLogOnCLick()
        {
            Application.Logger.Clear();
        }

        #endregion UI Event Handlers

        #endregion Public API
    }
} // namespace WM.UI
