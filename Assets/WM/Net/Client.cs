using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

using UnityEngine;

using WM.ArchiVR;
using WM.ArchiVR.Command;

namespace WM.Net
{
    public class Client : MonoBehaviour
    {
        #region Variables

        public ApplicationArchiVR application;

        public string Status = "Not initialized";

        public string InitialServerIP = "";//192.168.0.13";

        public int ConnectTimeout = 100;

        #region TCP

        public int TcpPort = 8889; // Must be different than server TCP port probably...

        // The TCP client
        private TcpClient tcpClient;

        // The network stream to the server over TCP.
        private NetworkStream tcpServerStream;

        #endregion

        #region UDP

        public static readonly int UdpPort = 8891; // Must be different than server UDP port probably...

        private UdpClient udpClient;

        private UDPSend udpSend;

        private UDPReceive udpReceive;

        #endregion

        // The thread.
        private Thread thread;

        private bool shutDown = false;

        #endregion

        //! 
        public void Init()
        {
            shutDown = false;

            thread = new Thread(new ThreadStart(ThreadFunction));
            thread.IsBackground = true;
            thread.Start();

            WM.Logger.Debug("Client started");
        }

        //! 
        public void Disconnect()
        {
            SendCommand(new DisconnectClientCommand(NetUtil.GetLocalIPAddress()));

            while (Status != "DisconnectAcknoledged") ;

            Shutdown();
        }

        //! 
        public void Shutdown()
        {
            shutDown = true;

            if (thread != null)
            {
                thread.Join();

                thread = null;
            }

            if (udpClient != null)
            {
                udpClient.Close();

                udpClient = null;
            }

            if (tcpServerStream != null)
            {
                lock (tcpServerStream)
                {
                    tcpServerStream.Close();

                    tcpServerStream = null;
                }
            }

            if (tcpClient != null)
            {
                tcpClient.Close();

                tcpClient = null;
            }
        }

        //! Thread function executed by the thread.
        private void ThreadFunction()
        {
            while (!shutDown)
            {
                if (TryConnect())
                {
                    break;
                }
            }

            Status = "Connected to " + ServerIP;
            WM.Logger.Debug("Client: tcpClient connected.");

            // Broadcast your chosen avatar.
            {
                var scac = new SetClientAvatarCommand();
                scac.ClientIP = WM.Net.NetUtil.GetLocalIPAddress();
                scac.AvatarIndex = application.AvatarIndex;
                SendCommand(scac);
            }

            // Get server stream from TCP client.
            tcpServerStream = tcpClient.GetStream();
                        
            // Initialize UDP sockets to/from server.
            {
                udpClient = new UdpClient(UdpPort);

                udpSend = new UDPSend(udpClient);
                udpSend.remoteIP = ServerIP;
                udpSend.remotePort = Server.UdpPort;
                udpSend.Init();

                udpReceive = new UDPReceive(udpClient);
                udpReceive.Init();
            }

            while (!shutDown)
            {
                string dataFromServer = "";

                if (tcpServerStream != null)
                {
                    lock (tcpServerStream)
                    {
                        while (tcpServerStream.DataAvailable)
                        {
                            // Receive data from server.
                            var bytesFromServer = new byte[tcpClient.ReceiveBufferSize];
                            var numBytesRead = tcpServerStream.Read(bytesFromServer, 0, (int)tcpClient.ReceiveBufferSize);

                            dataFromServer += Encoding.ASCII.GetString(bytesFromServer, 0, numBytesRead);
                        }
                    }
                }

                if (dataFromServer.Length > 0)
                {
                    //WM.Logger.Debug("Client: Data from server: " + dataFromServer);

                    while (true)
                    {
                        string beginTag = "<Message ";
                        string endTag = "</Message>";
                        int EndTagLength = endTag.Length;

                        int firstMessageBegin = dataFromServer.IndexOf(beginTag);

                        if (firstMessageBegin < 0)
                        {
                            break;
                        }

                        // Remove all data in front of first message.
                        dataFromServer = dataFromServer.Substring(firstMessageBegin);

                        int firstMessageEnd = dataFromServer.IndexOf(endTag);

                        if (firstMessageEnd < 0)
                        {
                            break;
                        }

                        //XML-deserialize the message.
                        int messageLength = firstMessageEnd + EndTagLength;
                        string messageXML = dataFromServer.Substring(0, messageLength);

                        int c = dataFromServer.Length;
                        var remainder = dataFromServer.Substring(firstMessageEnd + EndTagLength);
                        dataFromServer = remainder;

                        var ser = new XmlSerializer(typeof(Message));

                        var reader = new StringReader(messageXML);

                        var message = (Message)(ser.Deserialize(reader));

                        reader.Close();

                        // Binary-deserialize the object from the message.
                        var obj = message.Deserialize();

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
                        else if (obj is ServerShutdownCommand)
                        {
                            var command = (ServerShutdownCommand)obj;
                            application.QueueCommand(command);
                        }
                        else if (obj is ClientDisconnectAcknoledgeMessage)
                        {
                            Status = "DisconnectAcknoledged";
                        }
                    }
                }
            }
        }

        private bool TryConnect()
        {
            if (InitialServerIP != "")
            {
                // connect to a predefined server.
                return TryConnectToServer(InitialServerIP);
            }
            else
            {
                // Search the local subnet for a server.
                for (int i = 0; i < 256; ++i)
                {
                    string serverIP = "192.168.0." + i;

                    if (TryConnectToServer(serverIP))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private bool TryConnectToServer(string serverIP)
        {
            String tag = serverIP + ":" + Server.TcpPort + ", timeout:" + ConnectTimeout + "ms";
            WM.Logger.Debug("Client.TryConnectToServer(): Server:'" + tag);
            
            Status = "Trying to connected to " + tag;

            var tcpClient = new TcpClient();

            var connectionAttempt = tcpClient.ConnectAsync(serverIP, Server.TcpPort);

            connectionAttempt.Wait(ConnectTimeout);

            if (tcpClient.Connected)
            {
                this.tcpClient = tcpClient;
                WM.Logger.Debug("Client.TryConnectToServer(): Connected!");
            }
            else
            {
                WM.Logger.Debug("Client.TryConnectToServer(): Failed to connect!");
            }

            return tcpClient.Connected;
        }

        //! Returns whether this client is connected to a server.
        public bool Connected
        {
            get
            {
                return (tcpClient != null) && tcpClient.Connected;
            }
        }

        //! Returns the IP address of the server to which this client is connected, or 'Not available' if not connected.
        public string ServerIP
        {
            get
            {
                if (!Connected)
                {
                    return "Not available";
                }

                return ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
            }
        }

        public void SendAvatarStateToUdp(
            GameObject avatarHead,
            GameObject avatarLHand,
            GameObject avatarRHand)
        {
            // Temporarily disabled the below check: udpSend is not null but still the if-clause evaluates to true?!? :-s
            if (udpSend == null)
            {
                return; // Not connected yet...
            }

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

                udpSend.sendString(data);

                //Debug.Log("Client: Sent frame " + frameIndex++);
            }
            catch (Exception e)
            {
                WM.Logger.Error("Clien.SendPositionToUDP(): Exception:" + e.Message);
            }
        }

        public void UpdateAvatarStatesFromUdp()
        {
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

        private string GetCommandAsData(ICommand command)
        {
            var message = new Message();
            message.Serialize(command);

            var ser = new XmlSerializer(typeof(Message));

            var writer = new StringWriter();
            ser.Serialize(writer, message);
            writer.Close();

            var data = writer.ToString();

            return data;
        }

        public void SendCommand(ICommand command)
        {
            Debug.Log("Client:SendCommand()");

            try
            {
                var data = GetCommandAsData(command);

                SendData(data);
            }
            catch (Exception e)
            {
                 WM.Logger.Error("Client.SendCommand(): Exception:" + e.Message);
            }
        }

        public void SendData(String data)
        {
            Debug.Log("Client:SendData()");

            try
            {
                if (tcpClient == null)
                {
                    return;
                }

                var networkStream = tcpClient.GetStream();

                if (networkStream == null)
                {
                    return;
                }

                var bytes = Encoding.ASCII.GetBytes(data);
                networkStream.Write(bytes, 0, bytes.Length);
                networkStream.Flush();
            }
            catch (Exception e)
            {
                 WM.Logger.Error("Client.SendData(): Exception:" + e.Message);
            }
        }
    }
}
