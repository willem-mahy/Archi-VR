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
            var logCallTag = LogID + ".SendAvatarStateToUdp()";

            WM.Logger.Debug(logCallTag);

            try
            {
                var avatarState = new AvatarState();
                avatarState.PlayerID = application.Player.ID;
                
                avatarState.HeadPosition = avatarHead.transform.position - application.OffsetPerID;
                avatarState.HeadRotation = avatarHead.transform.rotation;

                avatarState.LHandPosition = avatarLHand.transform.position - application.OffsetPerID;
                avatarState.LHandRotation = avatarLHand.transform.rotation;

                avatarState.RHandPosition = avatarRHand.transform.position - application.OffsetPerID;
                avatarState.RHandRotation = avatarRHand.transform.rotation;

                SendMessageUdp(avatarState);
            }
            catch (Exception e)
            {
                WM.Logger.Error(logCallTag + ": Exception:" + e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateAvatarStatesFromUdp()
        {
            var logCallTag = LogID + ".UpdateAvatarStatesFromUdp()";

            WM.Logger.Debug(logCallTag);

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

                    WM.Logger.Debug(logCallTag + ": Received avatar state for player[" + avatarState.PlayerID + "].");

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

                            avatarState.HeadPosition = avatarState.HeadPosition + application.OffsetPerID;
                            avatarState.LHandPosition = avatarState.LHandPosition + application.OffsetPerID;
                            avatarState.RHandPosition = avatarState.RHandPosition + application.OffsetPerID;

                            avatar.SetState(avatarState);                            
                        }
                        else
                        {
                            WM.Logger.Warning(logCallTag + ".UpdateAvatarStatesFromUDP(): Received avatar state for non-existing avatar! (" + clientID + ")");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                 WM.Logger.Error(logCallTag + ": Exception:" + e.Message);
            }
        }
    }
}
