using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WM;
using WM.Application;

public class MenuSettingsPanel : MonoBehaviour
{
    #region Variables

    /// <summary>
    /// 
    /// </summary>
    public UnityApplication Application;

    /// <summary>
    /// 
    /// </summary>
    public Slider MenuSizeSlider;

    /// <summary>
    /// 
    /// </summary>
    public Slider MenuHeightSlider;

    #endregion

    // Start is called before the first frame update
    void Start()    
    {
        #region Get references to GameObjects.

        if (Application == null)
        {
            Application = UtilUnity.FindGameObjectElseError(gameObject.scene, "Application").GetComponent<UnityApplication>();
        }

        #endregion

        MenuSizeSlider.onValueChanged.AddListener((float value) => { MenuSizeSliderOnValueChange(value); });

        MenuHeightSlider.onValueChanged.AddListener((float value) => { MenuHeightSliderOnValueChange(value); });
    }

    // Update is called once per frame
    void Update()
    {   
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public void MenuSizeSliderOnValueChange(float value)
    {
        Application.WorldSpaceMenu.gameObject.transform.localScale = new Vector3(MenuSizeSlider.value, MenuSizeSlider.value, MenuSizeSlider.value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public void MenuHeightSliderOnValueChange(float value)
    {
        Application.WorldSpaceMenu.UpdateOffsetY(MenuHeightSlider.value);
    }
}
