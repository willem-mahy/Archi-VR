using UnityEngine;
using UnityEngine.UI;

public class FPSText : MonoBehaviour
{
    private Text text = null;

    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (text == null)
        {
            return;
        }

        var qualityLevel = QualitySettings.GetQualityLevel();
        var qualityLevelName = QualitySettings.names[qualityLevel];
        var fps = (int)(1f / Time.unscaledDeltaTime);

        text.text = qualityLevelName + " FPS: " + fps;
    }
}
