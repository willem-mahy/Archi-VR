using System;
using UnityEngine;
using UnityEngine.UI;
using WM.Application;
using WM.Command;
using WM.Net;

namespace WM.UI
{
    /// <summary>
    /// The 'Network' menu panel.
    /// </summary>
    public class NetworkMenuPanel : MenuPanel<UnityApplication>
    {
        #region Variables

        public Text NetworkModeText;

        public Text IPValueText;

        public GameObject NetworkPanel;

        public GameObject StandalonePanel;
        public Text ServersValueText;

        public GameObject ClientPanel;
        
        public Text ClientStatusValueText;

        public GameObject ServerPanel;
        public Text ServerStatusValueText;
        public Text ClientsValueText;

        public Toggle ColocationToggle;

        #endregion

        #region Public API

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        override public void Start()
        {
            base.Start();

            UpdateUI(); // If startup mode is Standalone, the UI is not updated accordingly, so force that explicitely here...
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        void Update()
        {
            UpdateUI();
        }

        #endregion Public API

        #region Non-public API

        /// <summary>
        /// 
        /// </summary>
        private void UpdateUI()
        {
            if (Application.NetworkInitialized)
            {
                switch (Application.NetworkMode)
                {
                    case NetworkMode.Standalone:
                        StandalonePanel.SetActive(true);
                        ServerPanel.SetActive(false);
                        ClientPanel.SetActive(false);

                        var serverInfos = Application.ServerDiscovery.GetServerInfos();
                        if (serverInfos.Count == 0)
                        {
                            ServersValueText.text = "No servers found";
                        }
                        else
                        {
                            var serversList = "";

                            foreach (var serverInfo in serverInfos)
                            {
                                serversList += serverInfo.IP + ":" + serverInfo.TcpPort + "\n";
                            }

                            ServersValueText.text = serversList;
                        }
                        break;
                    case NetworkMode.Server:
                        StandalonePanel.SetActive(false);
                        ServerPanel.SetActive(true);
                        ClientPanel.SetActive(false);

                        ServerStatusValueText.text = Application.Server.StateText;

                        ClientsValueText.text = Application.Server.GetClientInfo(Application.Client.TcpPort);
                        break;
                    case NetworkMode.Client:
                        StandalonePanel.SetActive(false);
                        ServerPanel.SetActive(false);
                        ClientPanel.SetActive(true);

                        ClientStatusValueText.text = Application.Client.StateText;

                        if (Application.Client.State == Client.ClientState.Connected)
                        {
                            ClientStatusValueText.text += " to " + Application.Client.ServerIP + ":" + Application.Client.ServerInfo.TcpPort;
                        }
                        break;
                }
            }
            else
            {
                StandalonePanel.SetActive(false);
                ServerPanel.SetActive(false);
                ClientPanel.SetActive(false);
            }

            UpdateUI_NetworkModeTitle();

            UpdateUI_IP();

            if (ColocationToggle != null)
            {
                ColocationToggle.SetIsOnWithoutNotify(Application.ColocationEnabled);
            }
        }

        /// <summary>
        /// Updates the 'Network mode' UI control to show which network mode is active.
        /// </summary>
        private void UpdateUI_NetworkModeTitle()
        {
            if (Application.NetworkInitialized)
            {
                switch (Application.NetworkMode)
                {
                    case NetworkMode.Standalone:
                        NetworkModeText.text = "Running standalone.";
                        break;

                    case NetworkMode.Client:
                        NetworkModeText.text = "Running client. (TCP port:" + Application.Client.TcpPort + ")";
                        break;

                    case NetworkMode.Server:
                        NetworkModeText.text = "Running server. (TCP port:" + Application.Server.TcpPort + ")";
                        break;
                }
            }
            else
            {
                NetworkModeText.text = "Not initialized";
            }
        }

        /// <summary>
        /// Updates the 'IP' UI control to show the current IP of the local system.
        /// </summary>
        private void UpdateUI_IP()
        {
            var ownIP = NetUtil.GetLocalIPAddress().ToString();

            IPValueText.text = ownIP;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="networkMode"></param>
        private void OnNetworkModeSelection(NetworkMode networkMode)
        {
            if (networkMode != NetworkMode.Standalone)
            {
                if (Application != null)
                {
                    if (Application.ServerDiscovery.State == Net.ServerDiscovery.ServerDiscoveryState.Running)
                    {
                        Application.ServerDiscovery.Stop();
                    }
                }
            }

            if (Application)
            {
                Application.QueueCommand(new InitNetworkCommand(networkMode));
            }
        }

        #region UI Event Handlers

        /// <summary>
        /// 'On Click' event handler for the 'Start Server' button.
        /// </summary>
        public void StartServerButtonClick()
        {
            OnNetworkModeSelection(NetworkMode.Server);
        }

        /// <summary>
        /// 'On Click' event handler for the 'Join Server' button.
        /// </summary>
        public void JoinServerButtonClick()
        {
            OnNetworkModeSelection(NetworkMode.Client);
        }

        /// <summary>
        /// 'On Click' event handler for the 'Stop Server' button.
        /// </summary>
        public void StopServerButtonClick()
        {
            OnNetworkModeSelection(NetworkMode.Standalone);
        }

        /// <summary>
        /// 'On Click' event handler for the 'Disconnect' button.
        /// </summary>
        public void DisconnectClientButtonClick()
        {
            OnNetworkModeSelection(NetworkMode.Standalone);
        }

        /// <summary>
        /// 'Value Changed' event handler for the 'Colocation Enabled' button.
        /// </summary>
        public void ColocationToggleOnValueChanged(bool value)
        {
            if (Application)
            {
                Application.ColocationEnabled = value;
            }
        }

        #endregion UI Event Handlers

        #endregion Non-public API
    }
}
