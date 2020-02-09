using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WM;
using WM.Application;


public class PlayerNamePanel : MonoBehaviour
{
    #region Fields

    /// <summary>
    /// 
    /// </summary>
    public UnityApplication Application;

    /// <summary>
    /// 
    /// </summary>
    public Dropdown NameDropdown;

    #endregion Fields

    #region Public API

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        #region Get references to GameObjects.

        Application = UtilUnity.FindGameObjectElseError(gameObject.scene, "Application").GetComponent<UnityApplication>();

        #endregion

        PopulateNameDropdown();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        // When this menu is activated...
        if (NameDropdown != null)
        {
            NameDropdown.Select(); // ... then put the UI focus on the 'Name' dropdown.
        }
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        if (NameDropdown.options.Count == 0)
        {
            PopulateNameDropdown(); // TODO: Design defect: we should register on 'Application.OnNamesChanged' event to repopulate name dropdown...
        }

        NameDropdown.SetValueWithoutNotify(Application.GetPlayerNameIndex(Application.Player.Name)); // TODO: Design defect: we should register on 'Application.Player.OnNameChanged' event to set selected name option...
    }
        
    #region UI event handlers

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public void NameDropdownValueChanged(int value)
    {
        Application.SetPlayerName(value);
    }

    /// <summary>
    /// 
    /// </summary>
    public void PrevNameButtonOnClick()
    {
        var newNameIndex = Application.GetPlayerNameIndex(Application.Player.Name) - 1;
        newNameIndex = UtilIterate.MakeCycle(newNameIndex, 0, Application.NumPlayerNames);
        NameDropdown.value = newNameIndex;
    }

    /// <summary>
    /// 
    /// </summary>
    public void NextNameButtonOnClick()
    {
        var newAvatarIndex = Application.GetPlayerNameIndex(Application.Player.Name) + 1;
        newAvatarIndex = UtilIterate.MakeCycle(newAvatarIndex, 0, Application.NumPlayerNames);
        NameDropdown.value = newAvatarIndex;
    }

    #endregion

    #endregion Public API

    #region Non-public API

    /// <summary>
    /// 
    /// </summary>
    private void PopulateNameDropdown()
    {
        var options = new List<Dropdown.OptionData>();

        foreach (var name in Application.PlayerNames)
        {
            options.Add(new Dropdown.OptionData(name));
        }

        NameDropdown.options = options;
    }

    #endregion Non-public API
}
