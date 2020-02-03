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

        Application = UtilUnity.TryFindGameObject(gameObject.scene, "Application").GetComponent<UnityApplication>();

        #endregion

        #region initialize Avatar dropdown options

        var options = new List<Dropdown.OptionData>();

        foreach (var avatarID in Application.AvatarFactory.GetRegisteredIDs())
        {
            var avatar = Application.AvatarFactory.Create(avatarID, new Vector3(), new Quaternion()); // TODO: Design defect: we should not be forced to create avatar instances just to get their names!
            var avatarName = avatar.name.Replace("(Clone)", ""); // En plus, it is super-ugly that we need to remove the 'Clone' suffix here!!!
            options.Add(new Dropdown.OptionData(avatarName));

            // En plus, it is super-ugly that we need to afterwards remove the temporary avatar instance here!!!            
            UtilUnity.Destroy(avatar);
        }

        AvatarDropdown.options = options;

        #endregion

        synchronizingUI = false;
    }

    // Update is called once per frame
    void Update()
    {
        synchronizingUI = true;

        AvatarDropdown.value = Application.GetAvatarIndex(Application.Player.AvatarID);

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

        Application.SetPlayerAvatar(value);
    }

    public void PrevAvatarButtonOnClick()
    {
        if (synchronizingUI)
        {
            return;
        }

        var newAvatarIndex = Application.AvatarIndex - 1;
        UtilIterate.MakeCycle(newAvatarIndex, 0, Application.avatarPrefabs.Count);
        Application.SetPlayerAvatar(newAvatarIndex);
        AvatarDropdown.value = newAvatarIndex;
    }

    public void NextAvatarButtonOnClick()
    {
        if (synchronizingUI)
        {
            return;
        }

        var newAvatarIndex = Application.AvatarIndex + 1;
        UtilIterate.MakeCycle(newAvatarIndex, 0, Application.avatarPrefabs.Count);
        Application.SetPlayerAvatar(newAvatarIndex);
        AvatarDropdown.value = newAvatarIndex;
    }

    #endregion
}
