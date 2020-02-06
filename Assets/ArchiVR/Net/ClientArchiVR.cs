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

        /// <summary>
        /// The ArchiVR application.
        /// </summary>
        public ApplicationArchiVR applicationArchiVR; // TODO: Design defect: this should be 'private' but because of unit testing we cannot make it so!

        #endregion

        /// <summary>
        /// <see cref="Client"/> implementation.
        /// </summary>
        override protected void OnConnect()
        {
            // Notify other Clients about existence of your own player.
            var addPlayerCommand = new AddPlayerCommand(applicationArchiVR.Player);
            SendCommand(addPlayerCommand);
        }

        /// <summary>
        /// <see cref="Client"/> implementation.
        /// </summary>
        override protected void OnDisconnect()
        {
            var callLogTag = LogID + ".OnDisconnect()";
            _log.Debug(callLogTag);

            lock (applicationArchiVR.Players)
            {
                if (applicationArchiVR.Players.Count > 0)
                {
                    var remotePlayerIDs = new Guid[applicationArchiVR.Players.Keys.Count];
                    applicationArchiVR.Players.Keys.CopyTo(remotePlayerIDs, 0);

                    foreach (var playerID in remotePlayerIDs)
                    {
                        applicationArchiVR.RemovePlayer(playerID);
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

            _log.Debug(logCallTag);

            try
            {
                var avatarState = new AvatarState();
                avatarState.PlayerID = applicationArchiVR.Player.ID;
                
                avatarState.HeadPosition = avatarHead.transform.position - applicationArchiVR.OffsetPerID;
                avatarState.HeadRotation = avatarHead.transform.rotation;

                avatarState.LHandPosition = avatarLHand.transform.position - applicationArchiVR.OffsetPerID;
                avatarState.LHandRotation = avatarLHand.transform.rotation;

                avatarState.RHandPosition = avatarRHand.transform.position - applicationArchiVR.OffsetPerID;
                avatarState.RHandRotation = avatarRHand.transform.rotation;

                SendMessageUdp(avatarState);
            }
            catch (Exception e)
            {
                _log.Error(logCallTag + ": Exception:" + e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateAvatarStatesFromUdp()
        {
            var logCallTag = LogID + ".UpdateAvatarStatesFromUdp()";

            _log.Debug(logCallTag);

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

                    _log.Debug(logCallTag + ": Received avatar state for player[" + avatarState.PlayerID + "].");

                    receivedAvatarStates[avatarState.PlayerID] = avatarState;
                }

                // Update avatars with received avatar states.
                lock (applicationArchiVR.Players)
                {
                    // Apply the most recent states.
                    foreach (var clientID in receivedAvatarStates.Keys)
                    {
                        if (applicationArchiVR.Players.ContainsKey(clientID))
                        {
                            var avatar = applicationArchiVR.Players[clientID].Avatar;
                            var avatarState = receivedAvatarStates[clientID];

                            avatarState.HeadPosition = avatarState.HeadPosition + applicationArchiVR.OffsetPerID;
                            avatarState.LHandPosition = avatarState.LHandPosition + applicationArchiVR.OffsetPerID;
                            avatarState.RHandPosition = avatarState.RHandPosition + applicationArchiVR.OffsetPerID;

                            avatar.SetState(avatarState);                            
                        }
                        else
                        {
                            _log.Warning(logCallTag + ".UpdateAvatarStatesFromUDP(): Received avatar state for non-existing avatar! (" + clientID + ")");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                 _log.Error(logCallTag + ": Exception:" + e.Message);
            }
        }
    }
}
