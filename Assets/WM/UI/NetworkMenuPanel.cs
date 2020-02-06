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

            UpdateUI(); // If startup mode is Standalone, the UI is not updated accordingly, so force that explicitely here...
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        void Update()
        {
            UpdateUI();
        }

        private void UpdateUI()
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

                    ClientsValueText.text = Application.Server.GetClientInfo();
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

            UpdateUI_NetworkModeTitle();

            UpdateUI_IP();
        }

        #endregion Public API

        #region Non-public API

        private void UpdateUI_NetworkModeTitle()
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
            if (Application)
            {
                Application.QueueCommand(new InitNetworkCommand(networkMode));
            }
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        public void StartServerButtonClick()
        {
            OnNetworkModeSelection(NetworkMode.Server);
        }

        /// <summary>
        /// 
        /// </summary>
        public void JoinServerButtonClick()
        {
            OnNetworkModeSelection(NetworkMode.Client);
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopServerButtonClick()
        {
            OnNetworkModeSelection(NetworkMode.Standalone);
        }

        /// <summary>
        /// 
        /// </summary>
        public void DisconnectClientButtonClick()
        {
            OnNetworkModeSelection(NetworkMode.Standalone);
        }

        #endregion Non-public API
    }
}
