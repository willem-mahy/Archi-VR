using UnityEngine;
using UnityEngine.UI;
using WM;
using WM.ArchiVR;

public class InfoMenu : MonoBehaviour
{
    public ApplicationArchiVR ApplicationArchiVR;

    public Text VersionText;
    
    // Start is called before the first frame update
    void Start()
    {
        #region Get references to UI components.

        ApplicationArchiVR = UtilUnity.TryFindGameObject("Application").GetComponent<ApplicationArchiVR>();

        if (VersionText == null)
        {
            var versionTextGO = UtilUnity.TryFindGameObject("InfoMenu_VersionText");

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

    // Update is called once per frame
    void Update()
    {        
    }
}
