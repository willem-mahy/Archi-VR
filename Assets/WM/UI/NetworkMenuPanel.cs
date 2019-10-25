using System.Collections;
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
        
        // Start is called before the first frame update
        void Start()
        {
            #region Get references to GameObjects.

            ApplicationArchiVR = GameObject.Find("Application").GetComponent<ApplicationArchiVR>();

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
            if (synchronizingUI)
            {
                return;
            }

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

                    ClientsValueText.text = ApplicationArchiVR.Server.GetClientInfo();
                    break;
                case NetworkMode.Client:
                    ClientToggle.isOn = true;

                    NetworkPanel.SetActive(true);
                    ServerPanel.SetActive(false);
                    ClientPanel.SetActive(true);

                    ClientStatusValueText.text = ApplicationArchiVR.Client.Status;                    
                    break;
            }
        }

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
