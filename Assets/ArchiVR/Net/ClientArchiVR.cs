using ArchiVR.Application;
using ArchiVR.Command;
using System;
using System.Collections.Generic;
using UnityEngine;
using WM.Command;
using WM.Net;

namespace ArchiVR.Net
{
    public class ClientArchiVR : Client
    {
        #region Variables

        public ApplicationArchiVR application;

        #endregion

        public ClientArchiVR()
        {
            UdpBroadcastMessage = Constants.BroadcastMessage;
        }

        /// <summary>
        /// <see cref="Client"/> implementation.
        /// </summary>
        override public void OnTcpConnected()
        {
            // Broadcast your chosen avatar.
            {
                var scac = new SetClientAvatarCommand(
                    NetUtil.GetLocalIPAddress(),
                    BasePort,
                    application.AvatarID);

                SendCommand(scac);
            }
        }

        /// <summary>
        /// <see cref="Client"/> implementation.
        /// </summary>
        /// <param name="obj"></param>
        override public void DoProcessMessage(object obj)
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
            else if (obj is SetClientAvatarCommand setClientAvatarCommand)
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
                avatarState.ClientIP = WM.Net.NetUtil.GetLocalIPAddress();
                
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
                // From the received messages, build a map ClientID -> AvatarState.
                var receivedAvatarStates = new Dictionary<string, AvatarState>();

                foreach (var obj in receivedMessages)
                {
                    var avatarState = (AvatarState)(obj);

                    receivedAvatarStates[avatarState.ClientIP] = avatarState;
                }

                // Update avatars with received avatar states.
                lock (application.remoteUsers)
                {
                    // Apply the most recent states.
                    foreach (var clientIP in receivedAvatarStates.Keys)
                    {
                        if (application.remoteUsers.ContainsKey(clientIP))
                        {
                            var avatar = application.remoteUsers[clientIP].Avatar;
                            var avatarState = receivedAvatarStates[clientIP];
                            avatar.SetState(avatarState);                            
                        }
                        else
                        {
                            WM.Logger.Warning("Client.UpdateAvatarStatesFromUDP(): Received avatar state for non-existing avatar! (" + clientIP + ")");
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
