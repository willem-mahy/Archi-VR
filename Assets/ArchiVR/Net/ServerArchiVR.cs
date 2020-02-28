using ArchiVR.Application;
using ArchiVR.Command;
using WM.Command;
using WM.Net;

namespace ArchiVR.Net
{
    /// <summary>
    /// Archi-VR application specific Server implementation.
    /// </summary>
    public class ServerArchiVR : Server
    {
        #region Variables        

        /// <summary>
        /// The ArchiVR application.
        /// </summary>
        public ApplicationArchiVR applicationArchiVR; // TODO: Design defect: this should be 'private' but because of unit testing we cannot make it so!

        #endregion

        /// <summary>
        /// <see cref="Server.OnClientConnected(ClientConnection)"/> implementation.
        /// </summary>        
        override protected void OnClientConnected(ClientConnection newClientConnection)
        {
            _log.Warning(string.Format("ServerArchiVR.OnClientConnected Client[{0}]", newClientConnection.ClientID));

            // Now the client is connected, initialize it to the server's application state.

            // A) Make the new Client know about existing Players.
            lock (applicationArchiVR.Players)
            {
                if (applicationArchiVR.Players.Count == 0)
                {
                    _log.Warning(string.Format("ServerArchiVR.OnClientConnected Client[{0}]: No pre-existing clients", newClientConnection.ClientID));
                }

                // For each existing player, ...
                foreach (var player in applicationArchiVR.Players.Values)
                {
                    //... except players hosted  by the new client ...
                    if (player.ClientID != newClientConnection.ClientID)
                    {
                        _log.Warning(string.Format("ServerArchiVR.OnClientConnected Client[{0}]: Notify new client about existing player {1}", newClientConnection.ClientID, player.LogID));
                        // ... send an AddPlayerCommand.
                        SendCommand(new AddPlayerCommand(player), newClientConnection); // This makes the new client initialize a remote user for the existing client.
                    }
                    else
                    {
                        _log.Warning(string.Format("ServerArchiVR.OnClientConnected Client[{0}]: Do not notify new client about existing player {1} because it is his/hers", newClientConnection.ClientID, player.ID));
                    }
                }
            }

            //B) Initialize new client to...
            if (clientConnections.Count > 1) // Hack to distinguish the local client running on server host. -> must/will be taken care of in ServerClient implementation.
            {
                // ...correct project/POI: spawn at the current Project and POI.
                var teleportCommand = new TeleportCommand();
                teleportCommand.ProjectIndex = applicationArchiVR.ActiveProjectIndex;
                teleportCommand.POIName = applicationArchiVR.ActivePOIName;

                SendCommand(teleportCommand, newClientConnection);

                // ...the same immersion mode.
                //var setImmersionModeCommand = new SetImmersionModeCommand();
                //setImmersionModeCommand.ImmersionModeIndex = applicationArchiVR.ActiveImmersionModeIndex;

                //SendCommand(setImmersionModeCommand, newClientConnection);
            }

            bool playerForClientReceived = false;
            while (!playerForClientReceived)
            {
                lock (applicationArchiVR.Players)
                {
                    foreach (var player in applicationArchiVR.Players.Values)
                    {
                        if (player.ClientID == newClientConnection.ClientID)
                        {
                            playerForClientReceived = true;
                            break;
                        }
                    }
                }
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
            _log.Debug(string.Format("ServerArchiVR.ProcessMessage: {0}", obj.ToString()));
            BroadcastData(messageXML); // TODO: Implement a way to figure out wheter to propagate or broadcast messages here.
        }
    }
}
