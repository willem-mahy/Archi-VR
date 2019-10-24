using System;
using System.Xml.Serialization;
using UnityEngine;
using WM.Net;

namespace WM.ArchiVR.Command
{
    [Serializable]
    [XmlRoot("InitNetworkCommand")]
    public class InitNetworkCommand : ICommand
    {
        [XmlElement("NetworkMode")]
        public NetworkMode NetworkMode { get; set; }

        public InitNetworkCommand(NetworkMode networkMode)
        {
            NetworkMode = networkMode;
        }

        public void Execute(ApplicationArchiVR application)
        {
            Debug.Log("InitNetworkCommand.Execute()");
            WM.Logger.Debug("InitNetworkCommand.Execute(): NetworkMode = " + NetworkMode);

            if (application.NetworkMode == NetworkMode)
            {
                return; // NOOP: already running in requested network mode...
            }

            if (application.Server != null)
            {
                application.Client.Shutdown();
            }

            if (application.Server != null)
            {
                application.Server.Shutdown();
            }

            switch (NetworkMode)
            {
                case NetworkMode.Server:
                    {
                        // Init network server
                        application.Server.Init();

                        // Init network client
                        application.Client.InitialServerIP = NetUtil.GetLocalIPAddress(); // Let client connect to own server. (TODO: connect directly, ie without network middle layer.)
                        application.Client.Init();
                    }
                    break;
                case NetworkMode.Client:
                    {
                        // Init network client only
                        application.Client.Init();
                    }
                    break;
                case NetworkMode.Standalone:
                    {
                        // Init no network
                    }
                    break;
            }

            application.NetworkMode = NetworkMode;
        }
    }
}
