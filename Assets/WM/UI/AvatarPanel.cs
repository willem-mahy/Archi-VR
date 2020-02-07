using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WM;
using WM.Application;


public class AvatarPanel : MonoBehaviour
{
    #region Fields

    /// <summary>
    /// 
    /// </summary>
    public UnityApplication Application;

    /// <summary>
    /// 
    /// </summary>
    public Dropdown AvatarDropdown;

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

        PopulateAvatarDropdown();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        // When this menu is activated...
        if (AvatarDropdown != null)
        {
            AvatarDropdown.Select(); // ... then put the UI focus on the 'Avatar' dropdown.
        }
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        if (AvatarDropdown.options.Count == 0)
        {
            PopulateAvatarDropdown(); // TODO: Design defect: we should register on 'Application.AvatarFactory.OnCHanged' event to repopulate avatar dropdown...
        }

        AvatarDropdown.SetValueWithoutNotify(Application.GetAvatarIndex(Application.Player.AvatarID)); // TODO: Design defect: we should register on 'Application.Player.OnAvatarChanged' event to set selected avatar option...
    }
        
    #region UI event handlers

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public void AvatarDropdownValueChanged(int value)
    {
        Application.SetPlayerAvatar(value);
    }

    /// <summary>
    /// 
    /// </summary>
    public void PrevAvatarButtonOnClick()
    {
        var newAvatarIndex = Application.GetAvatarIndex(Application.Player.AvatarID) - 1;
        newAvatarIndex = UtilIterate.MakeCycle(newAvatarIndex, 0, Application.AvatarFactory.NumRegistered);
        AvatarDropdown.value = newAvatarIndex;
    }

    /// <summary>
    /// 
    /// </summary>
    public void NextAvatarButtonOnClick()
    {
        var newAvatarIndex = Application.GetAvatarIndex(Application.Player.AvatarID) + 1;
        newAvatarIndex = UtilIterate.MakeCycle(newAvatarIndex, 0, Application.AvatarFactory.NumRegistered);
        AvatarDropdown.value = newAvatarIndex;
    }

    #endregion

    #endregion Public API

    #region Non-public API

    /// <summary>
    /// 
    /// </summary>
    private void PopulateAvatarDropdown()
    {
        var options = new List<Dropdown.OptionData>();

        foreach (var avatarPrefabName in Application.AvatarFactory.GetRegisteredGameObjectNames())
        {
            options.Add(new Dropdown.OptionData(avatarPrefabName));
        }

        AvatarDropdown.options = options;
    }

    #endregion Non-public API
}
