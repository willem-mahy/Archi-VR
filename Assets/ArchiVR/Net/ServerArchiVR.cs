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

        override public void OnClientConnected(ClientConnection newClientConnection)
        {
            // Init new client...

            // Now the client is connected, make him...

            // TODO: Step A should maybee better be performed by the clients themselves upon 'NewClientConnection()'?

            // A) .. know its peer clients:  (TODO? Make this 'Hey I exist already' notification part of the Client class?)
            lock (clientConnections)
            {
                // For each existing client, ...
                foreach (var clientConnection in clientConnections)
                {
                    //... each EXISTING client, (not the new one) ...
                    if (clientConnection.ClientID != newClientConnection.ClientID)
                    {
                        // ... send a 'ClientConnect' command.
                        var cc1 = new ConnectClientCommand();
                        cc1.ClientIP = clientConnection.remoteIP;
                        cc1.ClientPort = clientConnection.remotePortTCP;
                        
                        SendCommand(cc1, newClientConnection); // This makes the new client initialize a remote user for the existing client.
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

            // TODO: Step C should maybee better be moved to Client.Connect()?

            //C) Notify existing clients that the new client connected.
            var connectClientCommand = new ConnectClientCommand();
            connectClientCommand.ClientIP = newClientConnection.remoteIP;
            connectClientCommand.ClientPort = newClientConnection.remotePortTCP;

            PropagateCommand(connectClientCommand, newClientConnection);  // This makes the existing clients initialize a remote user for the new client.
        }
    }
}
