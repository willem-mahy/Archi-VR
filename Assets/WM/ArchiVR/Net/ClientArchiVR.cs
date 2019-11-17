using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using WM.ArchiVR.Command;
using WM.Net;
using Avatar = WM.Net.Avatar;

namespace WM.ArchiVR.Net
{
    public class ClientArchiVR : Client
    {
        #region Variables

        public ApplicationArchiVR application;

        #endregion

        override public void OnTcpConnected()
        {
            // Broadcast your chosen avatar.
            {
                var scac = new SetClientAvatarCommand();
                scac.ClientIP = NetUtil.GetLocalIPAddress();
                scac.AvatarIndex = application.AvatarIndex;
                SendCommand(scac);
            }
        }

        /// <summary>
        /// <see cref="Client"/> implementation.
        /// </summary>
        /// <param name="obj"></param>
        override public void DoProcessMessage(object obj)
        {
            if (obj is TeleportCommand)
            {
                var teleportCommand = (TeleportCommand)obj;
                application.QueueCommand(teleportCommand);
            }
            else if (obj is SetImmersionModeCommand)
            {
                var command = (SetImmersionModeCommand)obj;
                application.QueueCommand(command);
            }
            else if (obj is ConnectClientCommand)
            {
                var command = (ConnectClientCommand)obj;
                application.QueueCommand(command);
            }
            else if (obj is DisconnectClientCommand)
            {
                var command = (DisconnectClientCommand)obj;
                application.QueueCommand(command);
            }
            else if (obj is SetClientAvatarCommand)
            {
                var command = (SetClientAvatarCommand)obj;
                application.QueueCommand(command);
            }
            else if (obj is SetModelLocationCommand)
            {
                var command = (SetModelLocationCommand)obj;
                application.QueueCommand(command);
            }
            else if (obj is ServerShutdownCommand)
            {
                var command = (ServerShutdownCommand)obj;
                application.QueueCommand(command);
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

                var ser = new XmlSerializer(typeof(AvatarState));

                var writer = new StringWriter();
                ser.Serialize(writer, avatarState);
                writer.Close();                    

                var data = writer.ToString();

                SendDataUdp(data);
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
            var udpReceive = GetUdpReceive();

            if (udpReceive == null)
            {
                return; // Not connected yet...
            }

            try
            {
                var receivedAvatarStates = new Dictionary<string, AvatarState>();

                lock (udpReceive.allReceivedUDPPackets)
                {
                    if (udpReceive.allReceivedUDPPackets.Keys.Count > 1)
                    {
                        Debug.LogWarning("Client.UpdateAvatarStatesFromUDP(): More than one receive buffer!?!");
                        udpReceive.allReceivedUDPPackets.Clear();
                        return;
                    }

                    if (udpReceive.allReceivedUDPPackets.Keys.Count == 1)
                    {
                        // Get the first and only sender IP.
                        var senderIPEnumerator = udpReceive.allReceivedUDPPackets.Keys.GetEnumerator();
                        senderIPEnumerator.MoveNext();
                        var senderIP = senderIPEnumerator.Current;

                        // Get the corresponding receive buffer.
                        var receiveBuffer = udpReceive.allReceivedUDPPackets[senderIP];

                        while (true)
                        {
                            string frameEndTag = "</AvatarState>";
                            int frameEndTagLength = frameEndTag.Length;

                            int frameBegin = receiveBuffer.IndexOf("<AvatarState ");

                            if (frameBegin < 0)
                            {
                                break; // We have no full avatar states to read left in the receivebuffer -> break parsing received avatar states.
                            }

                            // Get position of first frame begin tag in receive buffer.
                            if (frameBegin > 0)
                            {
                                // Clear old data (older than first frame) from receivebuffer.
                                receiveBuffer = receiveBuffer.Substring(frameBegin);
                                frameBegin = 0;
                            }

                            // Get position of first frame end tag in receive buffer.
                            int frameEnd = receiveBuffer.IndexOf(frameEndTag);

                            if (frameEnd < 0)
                            {
                                break; // We have no full avatar states to read left in the receivebuffer -> break parsing received avatar states.
                            }

                            // Now get the frame string.
                            string frameXML = receiveBuffer.Substring(0, frameEnd + frameEndTagLength);

                            // Clear frame from receivebuffer.
                            receiveBuffer = receiveBuffer.Substring(frameEnd + frameEndTagLength);

                            {
                                var ser = new XmlSerializer(typeof(AvatarState));

                                var reader = new StringReader(frameXML);

                                var avatarState = (AvatarState)(ser.Deserialize(reader));

                                reader.Close();

                                receivedAvatarStates[avatarState.ClientIP] = avatarState;
                            }
                        }

                        // We have parsed all available full avatar states from the framebufer and removed them from it.
                        // Update the framebuffer to the unprocessed remainder.
                        udpReceive.allReceivedUDPPackets[senderIP] = receiveBuffer;
                    }
                }

                lock (application.avatars)
                {
                    // Apply the most recent states.
                    foreach (var clientIP in receivedAvatarStates.Keys)
                    {
                        if (application.avatars.ContainsKey(clientIP))
                        {
                            var avatar = application.avatars[clientIP].GetComponent<Avatar>();
                            var avatarState = receivedAvatarStates[clientIP];

                            avatar.Head.transform.position = avatarState.HeadPosition;
                            avatar.Head.transform.rotation = avatarState.HeadRotation;

                            avatar.Body.transform.position = avatarState.HeadPosition - 0.9f * Vector3.up;
                            avatar.Body.transform.rotation = Quaternion.AngleAxis((float)(Math.Atan2(avatar.Head.transform.forward.x, avatar.Head.transform.forward.z)), Vector3.up);

                            avatar.LHand.transform.position = avatarState.LHandPosition;
                            avatar.LHand.transform.rotation = avatarState.LHandRotation;

                            avatar.RHand.transform.position = avatarState.RHandPosition;
                            avatar.RHand.transform.rotation = avatarState.RHandRotation;
                        }
                        else
                        {
                            Debug.LogWarning("Client.UpdateAvatarStatesFromUDP(): Received avatar state for non-existing avatar! (" + clientIP + ")");
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
