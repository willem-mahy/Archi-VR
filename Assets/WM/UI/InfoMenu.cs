using UnityEngine;
using UnityEngine.UI;
using WM;
using WM.ArchiVR;

public class InfoMenu : MonoBehaviour
{
    #region Variables

    public ApplicationArchiVR ApplicationArchiVR;

    public Text VersionText;

    #endregion

    #region GameObject overrides

    // Start is called before the first frame update
    void Start()
    {
        #region Get references to GameObjects.

        ApplicationArchiVR = UtilUnity.TryFindGameObject("Application").GetComponent<ApplicationArchiVR>();

        #endregion

        #region Get references to UI components.

        if (VersionText == null)
        {
            var versionTextGO = UtilUnity.TryFindGameObject("InfoMenu_VersionValueText");

            if (versionTextGO != null)
            {
                VersionText = versionTextGO.GetComponent<Text>();
            }
        }

        #endregion

        if (VersionText != null)
        {
            VersionText.text = ApplicationArchiVR.Version;
        }
    }

    #endregion
}
