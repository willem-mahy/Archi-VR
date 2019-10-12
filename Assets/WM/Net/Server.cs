using ArchiVR;
using Assets.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;

namespace WM
{
    namespace Net
    {
        class UdpConnection
        {
            public UDPSend udpSend;
            public UDPReceive udpReceive;
            public string udpReceiveBuffer = "";
        }

        public class Server : MonoBehaviour
        {
            #region Variables

            public ApplicationArchiVR application;

            #region TCP

            public int tcpPort = 8888;

            // The server socket.
            TcpListener tcpListener;

            // Used for debugging client list lock related deadlocks.
            private string clientsLockOwner = "";

            // Lock for the clients list.
            private object clientsLock = new object();

            // The client TCP sockets.
            List<TcpClient> clientSockets = new List<TcpClient>();

            // The thread that accepts TCP data from connected clients.
            private Thread receiveTcpThread;

            // The thread that accepts client connections.
            private Thread acceptClientThread;

            #endregion

            #region UDP

            // Port for the UDP client.
            public static readonly int UdpPort = 8890; // Must be different than server TCP port probably...

            public UdpClient udpClient;// =  new UdpClient(UdpPort); //TODO: seems to not work?

            List<UdpConnection> udpConnections;// = new List<UdpConnection>(); //TODO: seems to not work?

            private Thread receiveUdpThread;

            #endregion

            #endregion

            public void Init()
            {
                udpClient = new UdpClient(UdpPort);

                udpConnections = new List<UdpConnection>();

                // TODO: Why is this needed?
                System.Text.ASCIIEncoding ASCII = new System.Text.ASCIIEncoding();

                // Get host name for local machine.
                var hostName = Dns.GetHostName();

                // Get host entry.
                var hostEntry = Dns.GetHostEntry(hostName);

                // Print all IP adresses:
                //foreach (var ipAddress in hostEntry.AddressList)
                //{
                //    Debug.Log("- IP address" + ipAddress);
                //}

                // Get first IP address.
                var serverIpAddress = hostEntry.AddressList[1];
                Debug.Log("Server IP address: " + serverIpAddress.ToString());

                // Create the server socket.
                Debug.Log("Server TCP port: " + tcpPort.ToString());
                tcpListener = new TcpListener(serverIpAddress, tcpPort);

                // Start the server socket.
                tcpListener.Start();

                acceptClientThread = new Thread(new ThreadStart(AcceptClientFunction));
                acceptClientThread.IsBackground = true;
                acceptClientThread.Name = "acceptClientThread";
                acceptClientThread.Start();

                receiveTcpThread = new Thread(new ThreadStart(ReceiveTcpFunction));
                receiveTcpThread.IsBackground = true;
                receiveTcpThread.Name = "receiveTcpThread";
                receiveTcpThread.Start();

                receiveUdpThread = new Thread(new ThreadStart(ReceiveUdpFunction));
                receiveUdpThread.IsBackground = true;
                receiveUdpThread.Name = "receiveUdpThread";
                receiveUdpThread.Start();

                Debug.Log("Server started");
            }

            private void AcceptClientFunction()
            {
                while ((true))
                {
                    try
                    {
                        if (tcpListener.Pending())
                        {
                            // Create the client socket.
                            var newTcpClient = default(TcpClient);

                            // Accept the client socket.
                            newTcpClient = tcpListener.AcceptTcpClient();

                            Debug.Log("Server: Client connected: " + newTcpClient.Client.RemoteEndPoint.ToString());

                            if (application.ActiveProjectIndex != -1)
                            {
                                var teleportCommand = new TeleportCommand();
                                teleportCommand.ProjectIndex = application.ActiveProjectIndex;
                                teleportCommand.POIName = application.ActivePOIName;

                                SendCommand(teleportCommand, newTcpClient);
                            }

                            var newUdpConnection = new UdpConnection();                            
                            newUdpConnection.udpSend = new UDPSend(udpClient);
                            newUdpConnection.udpSend.remoteIP = newTcpClient.Client.RemoteEndPoint.ToString();
                            newUdpConnection.udpSend.remotePort = Client.UdpPort;
                            newUdpConnection.udpReceive = new UDPReceive(udpClient);

                            lock (clientsLock)
                            {
                                clientsLockOwner = "AcceptClientFunction";

                                clientSockets.Add(newTcpClient);

                                udpConnections.Add(newUdpConnection);

                                clientsLockOwner = "None (AcceptClientFunction)";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.ToString());
                    }
                }
            }

            private void ReceiveUdpFunction()
            {
                while (true)
                {
                    try
                    {
                        lock (clientsLock)
                        {
                            clientsLockOwner = "ReceiveUdpFunction";

                            for (int clientIndex = 0; clientIndex < clientSockets.Count; ++clientIndex)
                            {
                                // Try to receive the x-th client frame.
                                var trackedObjectXML = GetTrackedObjectFromFromUdp(clientIndex);

                                // Broadcast the x-th client frame to all but the originating client. (so avatars can be updated.)
                                if (trackedObjectXML != null)
                                {
                                    for (int broadcastClientIndex = 0; broadcastClientIndex < clientSockets.Count; ++broadcastClientIndex)
                                    {
                                        if (clientIndex == broadcastClientIndex)
                                            return; // don't send own client updates back to self...

                                        SendDataToUdp(trackedObjectXML, broadcastClientIndex);
                                    }
                                }
                            }

                            clientsLockOwner = "None (last:ReceiveFromClientsFunction)";
                        }
                        //Thread.Sleep(10);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.ToString());
                    }
                }
            }

            private void ReceiveTcpFunction()
            {
                while (true)
                {
                    try
                    {
                        lock (clientsLock)
                        {
                            clientsLockOwner = "ReceiveTcpFunction";

                            foreach (var tcpClient in clientSockets)
                            {
                                //if (tcpClient == null)
                                //{
                                //    continue;
                                //}

                                var dataFromClient = "";
                                {
                                    var networkStream = tcpClient.GetStream();

                                    var numBytesAvailable = tcpClient.Available;
                                    if (numBytesAvailable > 0)
                                    {
                                        // Receive from client.
                                        byte[] bytesFromClient = new byte[numBytesAvailable];
                                        networkStream.Read(bytesFromClient, 0, numBytesAvailable);
                                        dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFromClient);
                                    }
                                }

                                var messageEnd = dataFromClient.IndexOf("$");
                                while (messageEnd != -1)
                                {
                                    var messageFromClient = dataFromClient.Substring(0, messageEnd);
                                    dataFromClient = dataFromClient.Substring(messageEnd + 1);

                                    Debug.Log("Server:ReceiveTcpFunction(): Data from client: '" + messageFromClient + "'");
                                    messageEnd = dataFromClient.IndexOf("$");
                                }
                            }

                            clientsLockOwner = "None (last:ReceiveFromClientsFunction)";
                        }
                        //Thread.Sleep(10);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.ToString());
                    }
                }
            }
                        
            public void Stop()
            {
                Debug.Log("Server::Stop");

                DisconnectClients();

                CloseSockets();

                Debug.Log("Server stopped");
            }

            public void BroadcastCommand(
                TeleportCommand teleportCommand)
            {
                Debug.Log("Server:BoadcastCommand()");

                try
                {
                    var ser = new XmlSerializer(typeof(TeleportCommand));

                    var writer = new StringWriter();
                    ser.Serialize(writer, teleportCommand);
                    writer.Close();

                    var data = writer.ToString();
                    Debug.Log(data);

                    lock (clientsLock)
                    {
                        clientsLockOwner = "BroadcastCommand";

                        foreach (var tcpClient in clientSockets)
                        {
                            //if (tcpClient != null)
                            {
                                SendData(data, tcpClient);
                            }
                        }

                        clientsLockOwner = "None (BroadcastCommand)";
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception:" + e.Message);
                }
            }

            public void SendCommand(
                TeleportCommand teleportCommand,
                TcpClient tcpClient)
            {
                Debug.Log("Server:SendCommand()");

                try
                {
                    var ser = new XmlSerializer(typeof(TeleportCommand));

                    var writer = new StringWriter();
                    ser.Serialize(writer, teleportCommand);
                    writer.Close();

                    var data = writer.ToString();

                    SendData(data, tcpClient);
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception:" + e.Message);
                }
            }

            public void SendData(
                String data,
                TcpClient tcpClient)
            {
                Debug.Log("Server:SendData()");

                var networkStream = tcpClient.GetStream();

                var bytes = Encoding.ASCII.GetBytes(data);
                networkStream.Write(bytes, 0, bytes.Length);
                networkStream.Flush();
            }

            private void DisconnectClients()
            {
                Debug.Log("Server::DisconnectClients");
            }

            private void CloseSockets()
            {
                Debug.Log("Server::CloseSockets");

                lock (clientsLock)
                {
                    clientsLockOwner = "CloseSockets";

                    // Close the client sockets.
                    foreach (var clientSocket in clientSockets)
                    {
                        clientSocket.Close();
                    }

                    clientSockets.Clear();

                    clientsLockOwner = "None (CloseSockets)";
                }

                // Stop the server socket.
                tcpListener.Stop();
            }




            //public void SendPositionToUdp(
            //    TrackedObject to,
            //    int clientIndex)
            //{
            //    if (udpConnections.Count < clientIndex - 1)
            //        return;

            //    var udpConnection = udpConnections[clientIndex];

            //    if (udpConnection == null)
            //        return;

            //    try
            //    {
            //        var ser = new XmlSerializer(typeof(TrackedObject));

            //        var writer = new StringWriter();
            //        ser.Serialize(writer, to);
            //        writer.Close();

            //        var data = writer.ToString();

            //        udpConnection.udpSend.sendString(data);
            //    }
            //    catch (Exception e)
            //    {
            //        Debug.LogError("Exception:" + e.Message);
            //    }
            //}

            public void SendDataToUdp(string data, int clientIndex)
            {
                if (udpConnections.Count < clientIndex - 1)
                    return;

                var udpConnection = udpConnections[clientIndex];

                if (udpConnection == null)
                    return;

                try
                {
                    udpConnection.udpSend.sendString(data);
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception:" + e.Message);
                }
            }

            public string GetTrackedObjectFromFromUdp(int clientIndex)// TODO: make list or map [ip, avatar]
            {
                if (udpConnections.Count < clientIndex - 1)
                    return null;

                var udpConnection = udpConnections[clientIndex];

                if (udpConnection == null)
                    return null;

                try
                {
                    udpConnection.udpReceiveBuffer += udpConnection.udpReceive.getLatestUDPPacket();

                    string frameEndTag = "</TrackedObject>";
                    int frameEndTagLength = frameEndTag.Length;
                    int lastFrameEnd = udpConnection.udpReceiveBuffer.LastIndexOf(frameEndTag);

                    if (lastFrameEnd < 0)
                    {
                        return null;
                    }

                    string temp = udpConnection.udpReceiveBuffer.Substring(0, lastFrameEnd + frameEndTagLength);

                    int lastFrameBegin = temp.LastIndexOf("<TrackedObject ");

                    if (lastFrameBegin < 0)
                    {
                        return null;
                    }

                    // Now get the frame string.
                    string trackedObjectXML = temp.Substring(lastFrameBegin, temp.Length - lastFrameBegin);

                    // Clear old frames from receivebuffer.
                    udpConnection.udpReceiveBuffer = udpConnection.udpReceiveBuffer.Substring(lastFrameEnd + frameEndTagLength);

                    var ser = new XmlSerializer(typeof(TrackedObject));

                    //var reader = new StreamReader(avatarFilePath);
                    var reader = new StringReader(trackedObjectXML);

                    var trackedObject = (TrackedObject)(ser.Deserialize(reader));
                    reader.Close();

                    return trackedObjectXML;
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception:" + e.Message);
                    return null;
                }
            }
        }
    }
}
