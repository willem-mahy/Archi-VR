﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using WM.ArchiVR;
using WM.ArchiVR.Command;
using WM.Net;

namespace WM.ArchiVR.UI
{
    public class NetworkMenuPanel : MonoBehaviour
    {
        public ApplicationArchiVR ApplicationArchiVR;

        public Text IPValueText;

        public Toggle StandaloneToggle;
        public Toggle ServerToggle;
        public Toggle ClientToggle;

        public GameObject NetworkPanel;

        public GameObject ClientPanel;
        public Text ClientStatusValueText;

        public GameObject ServerPanel;
        public Text ServerStatusValueText;
        public Text ClientsValueText;

        public Dropdown AvatarDropdown;
        
        // Start is called before the first frame update
        void Start()
        {
            #region Get references to UI components.

            ApplicationArchiVR = GameObject.Find("Application").GetComponent<ApplicationArchiVR>();

            #endregion

            #region initialize Avartor dropdown options

            AvatarDropdown.options.Clear();

            foreach (var avatar in ApplicationArchiVR.avatarPrefabs)
            {
                AvatarDropdown.options.Add(new Dropdown.OptionData(avatar.name));
            }

            #endregion

            switch (ApplicationArchiVR.NetworkMode)
            {
                case NetworkMode.Standalone:
                    StandaloneToggle.isOn = true;
                    break;
                case NetworkMode.Server:
                    ServerToggle.isOn = true;
                    break;
                case NetworkMode.Client:
                    ClientToggle.isOn = true;
                    break;
            }

            UpdateUIToNetworkModeSelection(ApplicationArchiVR.NetworkMode); // If startup mode is Standalone, the UI is not updated accordingly, so force that explicitely here...
        }

        bool synchronizingUI = false;

        // Update is called once per frame
        void Update()        
        {
            synchronizingUI = true;
            
            IPValueText.text = NetUtil.GetLocalIPAddress();

            if (ApplicationArchiVR.NetworkMode == NetworkMode.Server)
            {
                ServerStatusValueText.text = ApplicationArchiVR.Server.Status;
            }

            UpdateUIToNetworkModeSelection(ApplicationArchiVR.NetworkMode);

            synchronizingUI = false;
        }

        void OnNetworkModeSelection(NetworkMode networkMode)
        {
            ApplicationArchiVR.QueueCommand(new InitNetworkCommand(networkMode));
        }
        
        void UpdateUIToNetworkModeSelection(NetworkMode networkMode)
        {
            switch (networkMode)
            {
                case NetworkMode.Standalone:
                    StandaloneToggle.isOn = true;
                    
                    NetworkPanel.SetActive(false);
                    break;
                case NetworkMode.Server:
                    ServerToggle.isOn = true;
                    
                    NetworkPanel.SetActive(true);
                    ServerPanel.SetActive(true);
                    ClientPanel.SetActive(false);

                    AvatarDropdown.value = ApplicationArchiVR.AvatarIndex;

                    ClientsValueText.text = ApplicationArchiVR.Server.GetClientInfo();
                    break;
                case NetworkMode.Client:
                    ClientToggle.isOn = true;

                    NetworkPanel.SetActive(true);
                    ServerPanel.SetActive(false);
                    ClientPanel.SetActive(true);

                    ClientStatusValueText.text = ApplicationArchiVR.Client.Status;
                    AvatarDropdown.value = ApplicationArchiVR.AvatarIndex;
                    break;
            }
        }

        #region Avatar

        public void AvatarDropdownValueChanged(int value)
        {
            ApplicationArchiVR.SetAvatar(value);
        }

        public void PrevAvatarButtonOnClick()
        {
            var avatarIndex = UtilIterate.MakeCycle(--ApplicationArchiVR.AvatarIndex, 0, ApplicationArchiVR.avatarPrefabs.Count);
            ApplicationArchiVR.SetAvatar(avatarIndex);
            AvatarDropdown.value = avatarIndex;
        }

        public void NextAvatarButtonOnClick()
        {
            var avatarIndex = UtilIterate.MakeCycle(++ApplicationArchiVR.AvatarIndex, 0, ApplicationArchiVR.avatarPrefabs.Count);
            ApplicationArchiVR.SetAvatar(avatarIndex);
            AvatarDropdown.value = avatarIndex;
        }

        #endregion

        public void StandaloneToggleOnValueChanged(bool value)
        {
            if (value)
                OnNetworkModeSelection(NetworkMode.Standalone);
        }

        public void ClientToggleOnValueChanged(bool value)
        {
            if (value)
                OnNetworkModeSelection(NetworkMode.Client);
        }

        public void ServerToggleOnValueChanged(bool value)
        {
            if (value)
                OnNetworkModeSelection(NetworkMode.Server);
        }
    }
}
