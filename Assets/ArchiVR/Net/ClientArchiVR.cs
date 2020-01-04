using ArchiVR.Application;
using ArchiVR.Command;
using System;
using System.Collections.Generic;
using UnityEngine;
using WM.Command;
using WM.Net;

namespace ArchiVR.Net
{
    /// <summary>
    /// Archi-VR application specific Client implementation.
    /// </summary>
    public class ClientArchiVR : Client
    {
        #region Variables

        public ApplicationArchiVR application;

        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ClientArchiVR()
        {
        }

        /// <summary>
        /// <see cref="Client"/> implementation.
        /// </summary>
        override protected void OnConnect()
        {
            // Notify other Clients about existence of your own player.
            var addPlayerCommand = new AddPlayerCommand(application.Player);
            SendCommand(addPlayerCommand);
        }

        /// <summary>
        /// <see cref="Client"/> implementation.
        /// </summary>
        override protected void OnDisconnect()
        {
            var callLogTag = LogID + ".OnDisconnect()";
            WM.Logger.Debug(callLogTag);

            lock (application.Players)
            {
                if (application.Players.Count > 0)
                {
                    var remotePlayerIDs = new Guid[application.Players.Keys.Count];
                    application.Players.Keys.CopyTo(remotePlayerIDs, 0);

                    foreach (var playerID in remotePlayerIDs)
                    {
                        application.RemovePlayer(playerID);
                    }
                }
            }
        }

        /// <summary>
        /// <see cref="Client"/> implementation.
        /// </summary>
        /// <param name="obj"></param>
        override protected void DoProcessMessage(object obj)
        {
            if (obj is TeleportCommand teleportCommand)
            {
                application.QueueCommand(teleportCommand);
            }
            else if (obj is SetImmersionModeCommand setImmersionModeCommand)
            {
                application.QueueCommand(setImmersionModeCommand);
            }
            else if (obj is ConnectClientCommand connectClientCommand)
            {
                application.QueueCommand(connectClientCommand);
            }
            else if (obj is DisconnectClientCommand disconnectClientCommand)
            {
                application.QueueCommand(disconnectClientCommand);
            }
            else if (obj is SetPlayerNameCommand setPlayerNameCommand)
            {
                application.QueueCommand(setPlayerNameCommand);
            }
            else if (obj is SetPlayerAvatarCommand setClientAvatarCommand)
            {
                application.QueueCommand(setClientAvatarCommand);
            }
            else if (obj is SetModelLocationCommand setModelLocationCommand)
            {
                application.QueueCommand(setModelLocationCommand);
            }
            else if (obj is ServerShutdownCommand serverShutdownCommand)
            {
                application.QueueCommand(serverShutdownCommand);
            }
            else if (obj is AddPlayerCommand addPlayerCommand)
            {
                application.QueueCommand(addPlayerCommand);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="avatarHead"></param>
        /// <param name="avatarLHand"></param>
        /// <param name="avatarRHand"></param>
        public void SendAvatarStateToUdp(
            GameObject avatarHead,
            GameObject avatarLHand,
            GameObject avatarRHand)
        {
            try
            {
                var avatarState = new AvatarState();
                avatarState.PlayerID = application.Player.ID;
                
                avatarState.HeadPosition = avatarHead.transform.position;
                avatarState.HeadRotation = avatarHead.transform.rotation;

                avatarState.LHandPosition = avatarLHand.transform.position;
                avatarState.LHandRotation = avatarLHand.transform.rotation;

                avatarState.RHandPosition = avatarRHand.transform.position;
                avatarState.RHandRotation = avatarRHand.transform.rotation;

                SendMessageUdp(avatarState);
            }
            catch (Exception e)
            {
                WM.Logger.Error("Client.SendAvatarStateToUdp(): Exception:" + e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateAvatarStatesFromUdp()
        {
            try
            {
                // Get all received UDP messages.
                var receivedMessages = GetReceivedMessagesUdp();

                if (receivedMessages == null)
                {
                    return;
                }

                // We know that received UDP messages are always AvatarStates.
                // From the received messages, build a map PlayerID -> AvatarState.
                var receivedAvatarStates = new Dictionary<Guid, AvatarState>();

                foreach (var obj in receivedMessages)
                {
                    var avatarState = (AvatarState)(obj);

                    receivedAvatarStates[avatarState.PlayerID] = avatarState;
                }

                // Update avatars with received avatar states.
                lock (application.Players)
                {
                    // Apply the most recent states.
                    foreach (var clientID in receivedAvatarStates.Keys)
                    {
                        if (application.Players.ContainsKey(clientID))
                        {
                            var avatar = application.Players[clientID].Avatar;
                            var avatarState = receivedAvatarStates[clientID];
                            avatar.SetState(avatarState);                            
                        }
                        else
                        {
                            WM.Logger.Warning("Client.UpdateAvatarStatesFromUDP(): Received avatar state for non-existing avatar! (" + clientID + ")");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                 WM.Logger.Error("Client.UpdateAvatarStatesFromUDP(): Exception:" + e.Message);
            }
        }
    }
}
