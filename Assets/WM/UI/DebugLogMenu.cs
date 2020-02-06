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
    }

    // Update is called once per frame
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public void EnableLoggerToggleOnValueChanged(bool value)
    {
        Application.Logger.Enabled = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ClearLogOnCLick()
    {
        Application.Logger.Clear();
    }
}
