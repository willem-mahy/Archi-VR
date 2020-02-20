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
        /// The number of lines in the log the last time we updated the UI.
        /// </summary>
        private int NumLogEntriesLastUpdate;

        /// <summary>
        /// 
        /// </summary>
        int MaxNumLines = 100;

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

        /// <summary>
        /// 
        /// </summary>
        Toggle _filterDebugToggle;

        /// <summary>
        /// 
        /// </summary>
        Toggle _filterWarningToggle;

        /// <summary>
        /// 
        /// </summary>
        Toggle _filterErrorToggle;

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

            if (_filterDebugToggle == null)
            {
                _filterDebugToggle = UtilUnity.FindGameObjectElseError(gameObject.scene, "DebugLogMenu_FilterDebugToggle").GetComponent<Toggle>();
            }

            if (_filterWarningToggle == null)
            {
                _filterWarningToggle = UtilUnity.FindGameObjectElseError(gameObject.scene, "DebugLogMenu_FilterWarningToggle").GetComponent<Toggle>();
            }

            if (_filterErrorToggle == null)
            {
                _filterErrorToggle = UtilUnity.FindGameObjectElseError(gameObject.scene, "DebugLogMenu_FilterErrorToggle").GetComponent<Toggle>();
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

            /*
            if (_filterDebugToggle != null)
            {
                _filterDebugToggle.SetIsOnWithoutNotify(Application.Logger.Enabled);
            }

            if (_filterWarningToggle != null)
            {
                _filterWarningToggle.SetIsOnWithoutNotify(Application.Logger.Enabled);
            }

            if (_filterErrorToggle != null)
            {
                _filterErrorToggle.SetIsOnWithoutNotify(Application.Logger.Fil);
            }
            */

            if (Text != null)
            {
                var log = Application.Logger;

                var numLogEntries = log.NumEntries;

                if (NumLogEntriesLastUpdate == numLogEntries)
                {
                    return;
                }

                int numLinesToDisplay = (MaxNumLines > 0) ? System.Math.Min(numLogEntries, MaxNumLines) : numLogEntries;
                int numLinesDisplayed = 0;

                var text = "";

                for (var entryIndex = numLogEntries-1; entryIndex > 0; --entryIndex)
                {
                    var logEntry = log[entryIndex];

                    var filterEntry = false;

                    switch (logEntry.LogType)
                    {
                        case Logger.LogType.Debug:
                            filterEntry = _filterDebugToggle.isOn;
                            break;
                        case Logger.LogType.Warning:
                            filterEntry = _filterWarningToggle.isOn;
                            break;
                        case Logger.LogType.Error:
                            filterEntry = _filterErrorToggle.isOn;
                            break;
                    }

                    if (filterEntry)
                    {
                        var logLine = "";
                        
                        switch (logEntry.LogType)
                        {
                            case Logger.LogType.Debug:
                                logLine = "D: ";
                                break;
                            case Logger.LogType.Warning:
                                logLine = "W: ";
                                break;
                            case Logger.LogType.Error:
                                logLine = "E: ";
                                break;
                        }

                        logLine += logEntry.Text;

                        text = logLine + "\n" + text;

                        numLinesDisplayed += 1;

                        if (numLinesDisplayed == numLinesToDisplay)
                        {
                            break;
                        }
                    }
                }

                Text.text = text;

                NumLogEntriesLastUpdate = numLogEntries;
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
        /// 'OnValueChanged' event handler for the 'FilterDebug' toggle.
        /// </summary>
        public void FilterDebugToggleOnValueChanged(bool value)
        {
            NumLogEntriesLastUpdate = 0;
        }

        /// <summary>
        /// 'OnValueChanged' event handler for the 'FilterWarning' toggle.
        /// </summary>
        public void FilterWarningToggleOnValueChanged(bool value)
        {
            NumLogEntriesLastUpdate = 0;
        }

        /// <summary>
        /// 'OnValueChanged' event handler for the 'FilterDebug' toggle.
        /// </summary>
        public void FilterErrorToggleOnValueChanged(bool value)
        {
            NumLogEntriesLastUpdate = 0;
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
