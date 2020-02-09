using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WM;
using WM.Application;

public class GraphicsMenu : MonoBehaviour
{
    #region Variables

    public UnityApplication Application;

    public Dropdown QualityDropdown;

    public Toggle ShowFpsToggle;

    #endregion

    #region GameObject overrides

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        #region Get references to GameObjects.

        if (Application == null)
        {
            Application = UtilUnity.FindGameObjectElseError(gameObject.scene, "Application").GetComponent<UnityApplication>();
        }

        #endregion

        #region Get references to UI components.

        if (QualityDropdown == null)
        {
            var qualityDropdownGO = UtilUnity.FindGameObjectElseError(gameObject.scene, "GraphicsMenu_QualityDropdown");

            if (qualityDropdownGO != null)
            {
                QualityDropdown = qualityDropdownGO.GetComponent<Dropdown>();
            }
        }

        if (ShowFpsToggle == null)
        {
            var showFpsToggleGO = UtilUnity.TryFindGameObject("GraphicsMenu_ShowFpsToggle");

            if (showFpsToggleGO != null)
            {
                ShowFpsToggle = showFpsToggleGO.GetComponent<Toggle>();
            }
        }

        #endregion

        #region Initialize quality level options in Quality dropdown.

        var qualityOptions = new List<Dropdown.OptionData>();

        var qualityLevelNames = QualitySettings.names;
        
        foreach (var name in qualityLevelNames)
        {
            var option = new Dropdown.OptionData();
            option.text = name;
            qualityOptions.Add(option);
        }

        QualityDropdown.options = qualityOptions;

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    private void Update()
    {
        if (ShowFpsToggle != null)
        {
            ShowFpsToggle.SetIsOnWithoutNotify(Application.FpsPanelHUD.activeSelf);
        }

        if (QualityDropdown != null)
        {
            QualityDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        if (QualityDropdown != null)
        {
            QualityDropdown.Select();
        }
    }

    #endregion

    #region Quality

    /// <summary>
    /// 'OnValueChanged' handler for the 'Quality' dropdown.
    /// </summary>
    /// <param name="value">The new value of the 'Quality' dropdown.</param>
    public void QualityDropdownOnValueChanged(int value)
    {
        QualitySettings.SetQualityLevel(value);
    }

    /// <summary>
    /// 'OnClick' handler for the 'Previous quality' button.
    /// </summary>
    public void PrevQualityOnClick()
    {
        var newQualityLevel = UtilIterate.MakeCycle(QualitySettings.GetQualityLevel() - 1, 0, QualitySettings.names.Length);
        QualityDropdown.value = newQualityLevel;
    }

    /// <summary>
    /// 'OnClick' handler for the 'Next quality' button.
    /// </summary>
    public void NextQualityOnClick()
    {
        var newQualityLevel = UtilIterate.MakeCycle(QualitySettings.GetQualityLevel() + 1, 0, QualitySettings.names.Length);
        QualityDropdown.value = newQualityLevel;
    }

    #endregion

    #region FPS

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public void ShowFPSToggleOnValueChanged(bool value)
    {
        if (Application)
        {
            if (Application.FpsPanelHUD)
            {
                Application.FpsPanelHUD.SetActive(value);
            }
        }
    }

    #endregion
}
