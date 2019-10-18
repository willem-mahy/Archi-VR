using System;
using System.IO;
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

        public string ServerIP = "";//192.168.0.13";

        public int ConnectTimeout = 100;

        #region TCP

        public int TcpPort = 8889; // Must be different than server TCP port probably...

        // The TCP client
        private TcpClient tcpClient;

        #endregion

        #region UDP

        public static readonly int UdpPort = 8891; // Must be different than server UDP port probably...

        private UdpClient udpClient;

        private UDPSend udpSend;

        private UDPReceive udpReceive;

        #endregion

        // The thread.
        private Thread thread;

        #endregion

        public void Init()
        {
            /*
            // TODO: Why is this needed?
            ASCIIEncoding ASCII = new ASCIIEncoding();
                               
            // Create TCP client socket
            tcpClient = new TcpClient(GetLocalIpAddress);

            // Start UDP sockets
            udpSend.Init();
            udpReceive.Init();
            */

            thread = new Thread(new ThreadStart(ThreadFunction));
            thread.IsBackground = true;
            thread.Start();

            Debug.Log("Client started");
        }

        private void ThreadFunction()
        {
            Connect();

            Debug.Log("Client: tcpClient connected.");

            // Broadcast your chosen avatar.
            var ac = new SetClientAvatarCommand();
            ac.ClientIP = WM.Net.NetUtil.GetLocalIPAddress();
            ac.AvatarIndex = application.AvatarIndex;

            SendCommand(ac);

            var serverStream = tcpClient.GetStream();

            // Send message to server
            var myIP = NetUtil.GetLocalIPAddress();
            var messageToServer = "Hello from client '" + myIP + "'$";
            var bytesToServer = Encoding.ASCII.GetBytes(messageToServer);
            serverStream.Write(bytesToServer, 0, bytesToServer.Length);
            serverStream.Flush();

            // Initialize UDP sockets to/from server.
            udpClient = new UdpClient(UdpPort);

            udpSend = new UDPSend(udpClient);
            udpSend.remoteIP = ServerIP;
            udpSend.remotePort = Server.UdpPort;
            udpSend.Init();

            udpReceive = new UDPReceive(udpClient);
            udpReceive.Init();

            while (true)
            {
                string dataFromServer = "";

                while (serverStream.DataAvailable)
                {
                    // Receive data from server.
                    var bytesFromServer = new byte[tcpClient.ReceiveBufferSize];
                    var numBytesRead = serverStream.Read(bytesFromServer, 0, (int)tcpClient.ReceiveBufferSize);

                    dataFromServer += Encoding.ASCII.GetString(bytesFromServer, 0, numBytesRead);
                }

                if (dataFromServer.Length > 0)
                {
                    Debug.Log("Client: Data from server: " + dataFromServer);

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
                        else if (obj is SetClientAvatarCommand)
                        {
                            var command = (SetClientAvatarCommand)obj;
                            application.QueueCommand(command);
                        }
                        //else if (obj is DisconnectCommand)
                        //{
                        //    this.Disconnect()
                        //}
                    }
                }
            }
        }

        private void Connect()
        {
            while (true)
            {
                if (ServerIP != "")
                {
                    // connect to a predefined server.
                    if (ConnectToServer(this.ServerIP))
                    {
                        return;
                    }
                }
                else
                {
                    // Search the local subnet for a server.
                    for (int i = 0; i < 256; ++i)
                    {
                        string serverIP = "192.168.0." + i;

                        if (ConnectToServer(serverIP))
                        {
                            // Remember server IP.
                            ServerIP = serverIP;
                            return;
                        }
                    }
                }
            }
        }

        private bool ConnectToServer(string serverIP)
        {
            Debug.Log("Client: Trying to connect tcpClient to '" + serverIP + ":" + Server.TcpPort + "' (timeout: " + ConnectTimeout + "ms)");

            var tcpClient = new TcpClient();

            var connectionAttempt = tcpClient.ConnectAsync(serverIP, Server.TcpPort);

            connectionAttempt.Wait(ConnectTimeout);

            if (tcpClient.Connected)
            {
                this.tcpClient = tcpClient;
            }

            return tcpClient.Connected;
        }

        public void SendPositionToUDP(GameObject avatar)
        {
            // Temporarily disabled the below check: udpSend is not null but still the if-clause evaluates to true?!? :-s
            if (udpSend == null)
            {
                return; // Not connected yet...
            }

            try
            {
                var position = avatar.transform.position; // + avatar.transform.forward;
                var rotation = avatar.transform.rotation;
                    
                var to = new TrackedObject();
                to.Name = "Avatar";
                to.Position = position;
                to.Rotation = rotation;

                var ser = new XmlSerializer(typeof(TrackedObject));

                var writer = new StringWriter();
                ser.Serialize(writer, to);
                writer.Close();                    

                var data = writer.ToString();

                udpSend.sendString(data);

                //Debug.Log("Client: Sent frame " + frameIndex++);
            }
            catch (Exception e)
            {
                Debug.LogError("Clien.SendPositionToUDP(): Exception:" + e.Message);
            }
        }

        public void UpdatePositionFromUDP(GameObject avatar, string remoteIP)
        {
            if (udpReceive == null)
            {
                return; // Not connected yet...
            }

            try
            {
                string lastFrame;

                lock (udpReceive.allReceivedUDPPackets)
                {
                    //udpReceiveBuffer += udpReceive.getAllReceivedData();
                    if (udpReceive.allReceivedUDPPackets.Keys.Count == 0)
                    {
                        return;
                    }

                    //// For now use the first (because only) remote client's data.
                    //var keysEnumerator = udpReceive.allReceivedUDPPackets.Keys.GetEnumerator();
                    //keysEnumerator.MoveNext();
                    //var remoteIP = keysEnumerator.Current;

                    if (!udpReceive.allReceivedUDPPackets.ContainsKey(remoteIP))
                    {
                        return;
                    }

                    string frameEndTag = "</TrackedObject>";
                    int frameEndTagLength = frameEndTag.Length;

                    int lastFrameEnd = udpReceive.allReceivedUDPPackets[remoteIP].LastIndexOf(frameEndTag);

                    if (lastFrameEnd < 0)
                    {
                        return;
                    }

                    string temp = udpReceive.allReceivedUDPPackets[remoteIP].Substring(0, lastFrameEnd + frameEndTagLength);

                    int lastFrameBegin = temp.LastIndexOf("<TrackedObject ");

                    if (lastFrameBegin < 0)
                    {
                        return;
                    }

                    // Now get the frame string.
                    lastFrame = temp.Substring(lastFrameBegin, temp.Length - lastFrameBegin);

                    // Clear old frames from receivebuffer.
                    udpReceive.allReceivedUDPPackets[remoteIP] = udpReceive.allReceivedUDPPackets[remoteIP].Substring(lastFrameEnd + frameEndTagLength);
                }

                var ser = new XmlSerializer(typeof(TrackedObject));

                //var reader = new StreamReader(avatarFilePath);
                var reader = new StringReader(lastFrame);

                var trackedObject = (TrackedObject)(ser.Deserialize(reader));
                reader.Close();

                avatar.transform.position = trackedObject.Position - 1.8f * Vector3.up;

                avatar.transform.rotation = trackedObject.Rotation;

                //Debug.Log("Client.UpdatePositionFromUDP(): trackedObject.Position: " + trackedObject.Position);
                //Debug.Log("Client.UpdatePositionFromUDP(): trackedObject.Rotation: " + trackedObject.Rotation);
            }
            catch (Exception e)
            {
                Debug.LogError("Client.UpdatePositionFromUDP(): Exception:" + e.Message);
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
                Debug.LogError("Server.SendCommand(): Exception:" + e.Message);
            }
        }

        public void SendData(String data)
        {
            Debug.Log("Server:SendData()");

            try
            {
                var networkStream = tcpClient.GetStream();

                var bytes = Encoding.ASCII.GetBytes(data);
                networkStream.Write(bytes, 0, bytes.Length);
                networkStream.Flush();
            }
            catch (Exception e)
            {
                Debug.LogError("Server.SendData(): Exception:" + e.Message);
            }
        }
    }
}
