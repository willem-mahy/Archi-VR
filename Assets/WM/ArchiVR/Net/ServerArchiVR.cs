using WM.ArchiVR;
using WM.ArchiVR.Command;
using WM.Net;

namespace Assets.WM.ArchiVR.Net
{
    class ServerArchiVR : Server
    {
        #region Variables

        public ApplicationArchiVR application;

        #endregion

        override public void OnClientConnected(ClientConnection newClientConnection)
        {
            // Init new client...

            // Now the client is connected, make him...

            // TODO: Step A should maybee better be performed by the clients themselves upon 'NewClientConnection()'?

            // A) .. know its peer clients:
            lock (application.remoteUsers)
            {
                // For each existing client, ...
                foreach (var remoteUser in application.remoteUsers)
                {
                    //... each EXISTING client, (not the new one) ...
                    if (remoteUser.Key != newClientConnection.remoteIP)
                    {
                        // ... send a 'ClientConnect' command.
                        var cc1 = new ConnectClientCommand();
                        cc1.ClientIP = remoteUser.Value.remoteIP;
                        //cc1.AvatarIndex = remoteUser.Value.AvatarIndex;

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
            var cc = new ConnectClientCommand();
            cc.ClientIP = newClientConnection.remoteIP;
            
            PropagateCommand(cc, newClientConnection);  // This makes the existing clients initialize a remote user for the new client.
        }
    }
}
