using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using WM.ArchiVR;
using WM.Net;

namespace WM.ArchiVR.UI
{
    public class NetworkMenuPanel : MonoBehaviour
    {
        public ApplicationArchiVR application;

        public Text IPValueText;

        public Toggle StandaloneToggle;
        public Toggle ServerToggle;
        public Toggle ClientToggle;

        public GameObject NetworkPanel;

        public GameObject ClientPanel;
        public Text ServerIPValueText;

        public GameObject ServerPanel;
        public Text ClientsValueText;

        public Dropdown AvatarDropdown;

        // Start is called before the first frame update
        void Start()
        {
            AvatarDropdown.options.Clear();

            foreach (var avatar in application.avatarPrefabs)
            {
                AvatarDropdown.options.Add(new Dropdown.OptionData(avatar.name));
            }

            AvatarDropdown.onValueChanged.AddListener(delegate {
                AvatarDropdownValueChanged(AvatarDropdown);
            });

        }

        // Update is called once per frame
        void Update()
        {
            IPValueText.text = NetUtil.GetLocalIPAddress();

            switch (application.NetworkMode)
            {
                case NetworkMode.Standalone:
                    StandaloneToggle.isOn = true;
                    ServerToggle.isOn = false;
                    ClientToggle.isOn = false;
                    NetworkPanel.SetActive(false);                    
                    break;
                case NetworkMode.Server:
                    StandaloneToggle.isOn = false;
                    ServerToggle.isOn = true;
                    ClientToggle.isOn = false;

                    NetworkPanel.SetActive(true);
                    ServerPanel.SetActive(true);
                    ClientPanel.SetActive(false);

                    AvatarDropdown.value = application.AvatarIndex;

                    ClientsValueText.text = application.Server.GetClientInfo();
                    break;
                case NetworkMode.Client:
                    StandaloneToggle.isOn = false;
                    ServerToggle.isOn = false;
                    ClientToggle.isOn = true;

                    NetworkPanel.SetActive(true);
                    ServerPanel.SetActive(false);
                    ClientPanel.SetActive(true);

                    ServerIPValueText.text = application.Client.ServerIP;
                    AvatarDropdown.value = application.AvatarIndex;
                    break;
            }
        }

        void AvatarDropdownValueChanged(Dropdown change)
        {
            //TODO: application.SetAvatar(AvatarDropdown.value);
        }
    }
}
