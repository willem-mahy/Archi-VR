using ArchiVR;
using Assets.Command;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;
using WM;

namespace WM
{
    namespace Net
    {
        public class Client : MonoBehaviour
        {
            #region Variables

            public ApplicationArchiVR application;

            public string serverIP = "";//192.168.0.13";

            #region TCP

            public int port = 8888;

            // The TCP client
            private TcpClient tcpClient;

            #endregion

            #region UDP

            public static readonly int UdpPort = 8890; // Must be different than server UDP port probably...

            public UdpClient udpClient;// = new UdpClient(UdpPort);

            private UDPSend udpSend;

            private UDPReceive udpReceive;

            private string udpReceiveBuffer;

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

            // Update is called once per frame
            void Update()
            {

            }

            private void ThreadFunction()
            {
                Connect();

                Debug.Log("Client: tcpClient connected.");

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
                udpSend.remoteIP = serverIP;
                udpSend.remotePort = Server.UdpPort;
                udpReceive = new UDPReceive(udpClient);

                while (true)
                {
                    // Receive message from server.
                    var bytesFromServer = new byte[tcpClient.ReceiveBufferSize];
                    serverStream.Read(bytesFromServer, 0, (int)tcpClient.ReceiveBufferSize);
                    string dataFromServer = Encoding.ASCII.GetString(bytesFromServer);
                    Debug.Log("Client: Data from server: " + dataFromServer);

                    var ser = new XmlSerializer(typeof(TeleportCommand));

                    //var reader = new StreamReader(avatarFilePath);
                    var reader = new StringReader(dataFromServer);

                    var teleportCommand = (TeleportCommand)(ser.Deserialize(reader));
                    reader.Close();

                    application.QueueCommand(teleportCommand);
                }
            }

            public int connectTimeout = 100;

            private void Connect()
            {
                while (true)
                {
                    if (this.serverIP != "")
                    {
                        // connect to a predefined server.
                        if (ConnectToServer(this.serverIP))
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
                                this.serverIP = serverIP;
                                return;
                            }
                        }
                    }
                }
            }

            private bool ConnectToServer(string serverIP)
            {
                Debug.Log("Client: Trying to connect tcpClient to '" + serverIP + ":" + port + "' (timeout: " + connectTimeout + "ms)");

                var tcpClient = new TcpClient();

                var connectionAttempt = tcpClient.ConnectAsync(serverIP, port);

                connectionAttempt.Wait(connectTimeout);

                if (tcpClient.Connected)
                {
                    this.tcpClient = tcpClient;
                }

                return tcpClient.Connected;
            }



            public void SendPositionToUDP(GameObject avatar)
            {
                try
                {
                    var position = avatar.transform.position; // + avatar.transform.forward;

                    var to = new TrackedObject();
                    to.Name = "Avatar";
                    to.Position = position;

                    var ser = new XmlSerializer(typeof(TrackedObject));

                    var writer = new StringWriter();
                    ser.Serialize(writer, to);
                    writer.Close();

                    var data = writer.ToString();

                    udpSend.sendString(data);
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception:" + e.Message);
                }
            }

            public void UpdatePositionFromUDP(GameObject avatar)
            {
                try
                {
                    udpReceiveBuffer += udpReceive.getLatestUDPPacket();

                    string frameEndTag = "</TrackedObject>";
                    int frameEndTagLength = frameEndTag.Length;
                    int lastFrameEnd = udpReceiveBuffer.LastIndexOf(frameEndTag);

                    if (lastFrameEnd < 0)
                    {
                        return;
                    }

                    string temp = udpReceiveBuffer.Substring(0, lastFrameEnd + frameEndTagLength);

                    int lastFrameBegin = temp.LastIndexOf("<TrackedObject ");

                    if (lastFrameBegin < 0)
                    {
                        return;
                    }

                    // Now get the frame string.
                    string lastFrame = temp.Substring(lastFrameBegin, temp.Length - lastFrameBegin);

                    // Clear old frames from receivebuffer.
                    udpReceiveBuffer = udpReceiveBuffer.Substring(lastFrameEnd + frameEndTagLength);

                    var ser = new XmlSerializer(typeof(TrackedObject));

                    //var reader = new StreamReader(avatarFilePath);
                    var reader = new StringReader(lastFrame);

                    var trackedObject = (TrackedObject)(ser.Deserialize(reader));
                    reader.Close();

                    avatar.transform.position = trackedObject.Position;
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception:" + e.Message);
                }
            }
        }
    }
}
