using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WM;
using WM.ArchiVR;

public class AvatarPanel : MonoBehaviour
{
    public ApplicationArchiVR ApplicationArchiVR;

    public Dropdown AvatarDropdown;

    bool synchronizingUI = false;

    // Start is called before the first frame update
    void Start()
    {
        synchronizingUI = true;

        #region Get references to GameObjects.

        ApplicationArchiVR = GameObject.Find("Application").GetComponent<ApplicationArchiVR>();

        #endregion

        #region initialize Avatar dropdown options

        var options = new List<Dropdown.OptionData>();

        foreach (var avatar in ApplicationArchiVR.avatarPrefabs)
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

         AvatarDropdown.value = ApplicationArchiVR.AvatarIndex;

        synchronizingUI = false;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PrevAvatarButtonOnClick();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextAvatarButtonOnClick();
        }
    }

    #region UI event handlers

    public void AvatarDropdownValueChanged(int value)
    {
        if (synchronizingUI)
        {
            return;
        }

        ApplicationArchiVR.SetAvatar(value);
    }

    public void PrevAvatarButtonOnClick()
    {
        if (synchronizingUI)
        {
            return;
        }

        var avatarIndex = UtilIterate.MakeCycle(--ApplicationArchiVR.AvatarIndex, 0, ApplicationArchiVR.avatarPrefabs.Count);
        ApplicationArchiVR.SetAvatar(avatarIndex);
        AvatarDropdown.value = avatarIndex;
    }

    public void NextAvatarButtonOnClick()
    {
        if (synchronizingUI)
        {
            return;
        }

        var avatarIndex = UtilIterate.MakeCycle(++ApplicationArchiVR.AvatarIndex, 0, ApplicationArchiVR.avatarPrefabs.Count);
        ApplicationArchiVR.SetAvatar(avatarIndex);
        AvatarDropdown.value = avatarIndex;
    }

    #endregion
}
