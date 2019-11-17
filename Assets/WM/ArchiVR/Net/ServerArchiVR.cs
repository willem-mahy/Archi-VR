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
            if (clientConnections.Count > 1) // Hack to distinguish the local client running on server host. -> will be taken cae of in ServerClient implementation.
            {
                // ...spawn at the current Project and POI.
                var teleportCommand = new TeleportCommand();
                teleportCommand.ProjectIndex = application.ActiveProjectIndex;
                teleportCommand.POIName = application.ActivePOIName;

                SendCommand(teleportCommand, newClientConnection);

                // ...be in the same immersion mode.
                var setImmersionModeCommand = new SetImmersionModeCommand();
                setImmersionModeCommand.ImmersionModeIndex = application.ActiveImmersionModeIndex;

                SendCommand(setImmersionModeCommand, newClientConnection);
            }

            // Notify clients that another client connected.
            var cc = new ConnectClientCommand();
            cc.ClientIP = newClientConnection.remoteIP;
            cc.AvatarIndex = newClientConnection.AvatarIndex;
            PropagateCommand(cc, newClientConnection);
        }
    }
}
