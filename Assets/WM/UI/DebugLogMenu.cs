using UnityEngine;
using UnityEngine.UI;
using WM;
using WM.Application;

public class DebugLogMenu : MonoBehaviour
{
    public UnityApplication Application;

    int MaxNumLines = 30;

    Text Text;
    
    // Start is called before the first frame update
    void Start()
    {
        #region Get references to UI components.

        Application = UtilUnity.TryFindGameObject("Application").GetComponent<UnityApplication>();
        Text = UtilUnity.TryFindGameObject("DebugLogMenu_LogText").GetComponent<Text>();

        #endregion
    }

    // Update is called once per frame
    void Update()
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
