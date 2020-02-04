using UnityEngine;
using UnityEngine.UI;
using WM;
using WM.Application;

public class DebugLogMenu : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public UnityApplication Application;

    /// <summary>
    /// 
    /// </summary>
    int MaxNumLines = 30;

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

    // Start is called before the first frame update
    void Start()
    {
        #region Get references to GameObjects.

        if (Application == null)
        {
            Application = UtilUnity.TryFindGameObject(gameObject.scene, "Application").GetComponent<UnityApplication>();
        }

        #endregion

        #region Get references to UI components.

        if (Text == null)
        {
            Text = UtilUnity.TryFindGameObject(gameObject.scene, "DebugLogMenu_LogText").GetComponent<Text>();
        }

        if (_clearButton == null)
        {
            _clearButton = UtilUnity.TryFindGameObject(gameObject.scene, "DebugLogMenu_ClearButton").GetComponent<Button>();
        }

        if (_enableToggle == null)
        {
            _enableToggle = UtilUnity.TryFindGameObject(gameObject.scene, "DebugLogMenu_EnableToggle").GetComponent<Toggle>();
        }

        #endregion

        if (_enableToggle != null)
        {
            _enableToggle.isOn = WM.Logger.Enabled;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Text != null)
        {
            var text = "";

            var numLinesInLogger = WM.Logger.s_log.Count;
            int numLinesToDisplay = System.Math.Min(numLinesInLogger, MaxNumLines);

            for (var lineIndex = 0; lineIndex < numLinesToDisplay; ++lineIndex)
            {
                if (text.Length > 0)
                {
                    text += "\n";
                }

                text += WM.Logger.s_log[numLinesInLogger - (lineIndex + 1)];
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public void EnableLoggerToggleOnValueChanged(bool value)
    {
        WM.Logger.SetEnabled(value);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ClearLogOnCLick()
    {
        WM.Logger.Clear();
    }
}
