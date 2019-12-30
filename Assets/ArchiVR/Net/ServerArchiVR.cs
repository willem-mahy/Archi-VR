using ArchiVR.Application;
using ArchiVR.Command;
using WM.Command;
using WM.Net;

namespace ArchiVR.Net
{
    public class Constants
    {
        /// <summary>
        /// The message used for discovery of running ArchiVRServer instances by ClientArchiVR instances.
        /// Broadcast by ArchiVRServer, and listenend for by ArchiVRClient.
        /// </summary>
        public static readonly string BroadcastMessage = "Hello from ArchiVR server";
    }

    public class ServerArchiVR : Server
    {
        #region Variables        

        public ApplicationArchiVR application = null;

        #endregion

        public ServerArchiVR()
        {
            UdpBroadcastMessage = Constants.BroadcastMessage;
        }

        /// <summary>
        /// <see cref="Server.OnClientConnected(ClientConnection)"/> implementation.
        /// </summary>        
        override protected void OnClientConnected(ClientConnection newClientConnection)
        {
            // Now the client is connected, initialize it to the server's application state.

            // A) Make the new Client know about existing Players.
            lock (application.Players)
            {
                // For each existing player, ...
                foreach (var player in application.Players.Values)
                {
                    //... except players hostd  by the new client ...
                    if (player.ClientID != newClientConnection.ClientID)
                    {
                        // ... send an AddPlayerCommand.
                        SendCommand(new AddPlayerCommand(player), newClientConnection); // This makes the new client initialize a remote user for the existing client.
                    }
                }
            }

            //B) Initialize new client to...
            if (clientConnections.Count > 1) // Hack to distinguish the local client running on server host. -> must/will be taken care of in ServerClient implementation.
            {
                // ...correct project/POI: spawn at the current Project and POI.
                var teleportCommand = new TeleportCommand();
                teleportCommand.ProjectIndex = application.ActiveProjectIndex;
                teleportCommand.POIName = application.ActivePOIName;

                SendCommand(teleportCommand, newClientConnection);

                // ...the same immersion mode.
                var setImmersionModeCommand = new SetImmersionModeCommand();
                setImmersionModeCommand.ImmersionModeIndex = application.ActiveImmersionModeIndex;

                SendCommand(setImmersionModeCommand, newClientConnection);
            }
        }

        /// <summary>
        /// <see cref="Server.DoProcessMessage(string, ClientConnection, object)"/> implementation.
        /// </summary>
        override protected void DoProcessMessage(
            string messageXML,
            ClientConnection clientConnection,
            object obj)
        {
            WM.Logger.Debug(string.Format("ServerArchiVR.ProcessMessage: {0}", obj.ToString()));
            BroadcastData(messageXML); // TODO: Implement a way to figure out wheter to propagate or broadcast messages here.
        }
    }
}
