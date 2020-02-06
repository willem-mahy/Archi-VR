using UnityEngine;
using UnityEngine.UI;
using WM.Application;
using WM.Command;
using WM.Net;

namespace WM.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class NetworkMenuPanel : MonoBehaviour
    {
        #region Variables

        public UnityApplication Application;

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

        bool synchronizingUI = false;

        #endregion

        #region Public API

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {
            #region Get references to GameObjects.

            if (Application == null)
            {
                var applicationGO = UtilUnity.FindGameObjectElseError(gameObject.scene, "Application");
                Application = applicationGO.GetComponent<UnityApplication>();
            }

            #endregion

            synchronizingUI = true;

            switch (Application.NetworkMode)
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

            UpdateOwnIP();

            synchronizingUI = false;

            UpdateUIToNetworkModeSelection(Application.NetworkMode); // If startup mode is Standalone, the UI is not updated accordingly, so force that explicitely here...
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        void Update()
        {
            synchronizingUI = true;

            UpdateOwnIP();
            
            if (Application.NetworkMode == NetworkMode.Server)
            {
                ServerStatusValueText.text = Application.Server.StateText;
            }

            UpdateUIToNetworkModeSelection(Application.NetworkMode);

            synchronizingUI = false;
        }

        /// <summary>
        /// 
        /// </summary>
        void OnEnable()
        {
            // When this menu is avtivated...
            if (ServerToggle != null)
            {
                ServerToggle.Select(); // ... then put the UI focus on the 'Server' toogle.
            }
        }

        #endregion Public API

        #region Non-public API

        private void UpdateOwnIP()
        {
            var ownIP = NetUtil.GetLocalIPAddress().ToString();

            switch (Application.NetworkMode)
            {
                case NetworkMode.Server:
                    {
                        ownIP += ":" + Application.Server.TcpPort;
                    }
                    break;
                case NetworkMode.Client:
                    {
                        ownIP += ":" + Application.Client.TcpPort;
                    }
                    break;
            }

            IPValueText.text = ownIP;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="networkMode"></param>
        private void OnNetworkModeSelection(NetworkMode networkMode)
        {
            if (synchronizingUI)
            {
                return;
            }

            if (Application)
            {
                Application.QueueCommand(new InitNetworkCommand(networkMode));
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="networkMode"></param>
        private void UpdateUIToNetworkModeSelection(NetworkMode networkMode)
        {
            switch (networkMode)
            {
                case NetworkMode.Standalone:
                    StandaloneToggle.SetIsOnWithoutNotify(true);

                    ServerPanel.SetActive(true);
                    ClientPanel.SetActive(false);

                    var serverInfos = Application.ServerDiscovery.GetServerInfos();
                    if (serverInfos.Count == 0)
                    {
                        ClientsValueText.text = "No servers found";
                    }
                    else
                    {
                        var serversList = "";

                        foreach (var serverInfo in serverInfos)
                        {
                            serversList += serverInfo.IP + ":" + serverInfo.TcpPort + "\n";
                        }

                        ClientsValueText.text = serversList;
                    }
                    break;
                case NetworkMode.Server:
                    ServerToggle.SetIsOnWithoutNotify(true);
                    
                    NetworkPanel.SetActive(true);
                    ServerPanel.SetActive(true);
                    ClientPanel.SetActive(false);

                    ClientsValueText.text = Application.Server.GetClientInfo();
                    break;
                case NetworkMode.Client:
                    ClientToggle.SetIsOnWithoutNotify(true);

                    NetworkPanel.SetActive(true);
                    ServerPanel.SetActive(false);
                    ClientPanel.SetActive(true);

                    ClientStatusValueText.text = Application.Client.StateText;

                    if (Application.Client.State == Client.ClientState.Connected)
                    {
                        ClientStatusValueText.text+= " to " + Application.Client.ServerIP + ":" + Application.Client.ServerInfo.TcpPort;
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void StandaloneToggleOnValueChanged(bool value)
        {
            if (value)
                OnNetworkModeSelection(NetworkMode.Standalone);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void ClientToggleOnValueChanged(bool value)
        {
            if (value)
                OnNetworkModeSelection(NetworkMode.Client);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void ServerToggleOnValueChanged(bool value)
        {
            if (value)
                OnNetworkModeSelection(NetworkMode.Server);
        }

        #endregion Non-public API
    }
}
