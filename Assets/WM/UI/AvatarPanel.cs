using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WM;
using WM.Application;

public class AvatarPanel : MonoBehaviour
{
    public UnityApplication Application;

    public Dropdown AvatarDropdown;

    bool synchronizingUI = false;

    // Start is called before the first frame update
    void Start()
    {
        synchronizingUI = true;

        #region Get references to GameObjects.

        Application = GameObject.Find("Application").GetComponent<UnityApplication>();

        #endregion

        #region initialize Avatar dropdown options

        var options = new List<Dropdown.OptionData>();

        foreach (var avatar in Application.avatarPrefabs)
        {
            options.Add(new Dropdown.OptionData(avatar.name));
        }

        AvatarDropdown.options = options;

        #endregion

        synchronizingUI = false;
    }

    // Update is called once per frame
    void Update()
    {
        synchronizingUI = true;

         AvatarDropdown.value = Application.AvatarIndex;

        synchronizingUI = false;

        #region Temporary keyboard shortcuts to aid in debugging until control trigger can be emulated in editor mode.

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PrevAvatarButtonOnClick();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            NextAvatarButtonOnClick();
        }

        #endregion
    }

    #region UI event handlers

    public void AvatarDropdownValueChanged(int value)
    {
        if (synchronizingUI)
        {
            return;
        }

        Application.SetAvatar(value);
    }

    public void PrevAvatarButtonOnClick()
    {
        if (synchronizingUI)
        {
            return;
        }

        var avatarIndex = UtilIterate.MakeCycle(--Application.AvatarIndex, 0, Application.avatarPrefabs.Count);
        Application.SetAvatar(avatarIndex);
        AvatarDropdown.value = avatarIndex;
    }

    public void NextAvatarButtonOnClick()
    {
        if (synchronizingUI)
        {
            return;
        }

        var avatarIndex = UtilIterate.MakeCycle(++Application.AvatarIndex, 0, Application.avatarPrefabs.Count);
        Application.SetAvatar(avatarIndex);
        AvatarDropdown.value = avatarIndex;
    }

    #endregion
}
