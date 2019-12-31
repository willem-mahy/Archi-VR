using System;
using System.Xml.Serialization;
using WM.Application;
using WM.Command;
using WM.Net;

namespace WM.Command
{
    [Serializable]
    public class InitNetworkCommand : ICommand
    {
        /// <summary>
        /// The network mode to initialize the application into.
        /// </summary>
        public NetworkMode NetworkMode { get; set; }

        /// <summary>
        /// Parametrized constructor.
        /// </summary>
        /// <param name="networkMode">The network mode to initialize the application into.</param>
        public InitNetworkCommand(NetworkMode networkMode)
        {
            NetworkMode = networkMode;
        }

        /// <summary>
        /// <see cref="ICommand.Execute(UnityApplication)"/> implementation.
        /// </summary>
        /// <param name="application"></param>
        public void Execute(UnityApplication application)
        {
            WM.Logger.Debug("InitNetworkCommand.Execute(): NetworkMode = " + NetworkMode);

            if (application.NetworkMode == NetworkMode)
            {
                return; // NOOP: already running in requested network mode...
            }

            // Teardown from current network mode.
            switch (application.NetworkMode)
            {
                case NetworkMode.Client:
                    {
                        application.Client.Disconnect();
                    }
                    break;
                case NetworkMode.Server:
                    {
                        application.Client.Disconnect();
                        application.Server.Shutdown();
                    }
                    break;
            }

            // Initialize for new network mode.
            switch (NetworkMode)
            {
                case NetworkMode.Server:
                    {
                        // Init network server
                        application.Server.Init();

                        // Init network client
                        // Let client connect to own server. (TODO: connect directly, ie without network middle layer.)
                        application.Client.ServerInfo = new ServerInfo(
                            NetUtil.GetLocalIPAddress().ToString(),
                            application.Server.TcpPort,
                            application.Server.UdpPort);


                        application.Client.Connect();
                    }
                    break;
                case NetworkMode.Client:
                    {
                        // Init network client only
                        application.Client.Connect();
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
