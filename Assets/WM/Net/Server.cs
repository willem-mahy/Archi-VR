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
        class ClientConnection
        {
            //! The IP of the client.
            public string remoteIP;

            // The TCP client.
            public TcpClient tcpClient;

            // The UDP sender.
            public UDPSend udpSend;
        }

        public class Server : MonoBehaviour
        {
            readonly bool XML = false;

            #region Variables

            public ApplicationArchiVR application;

            #region TCP

            public static readonly int TcpPort = 8887;

            //! The TCP listener.
            TcpListener tcpListener;

            // Used for debugging client list lock related deadlocks.
            private string clientsLockOwner = "";

            // The client connections.
            List<ClientConnection> clientConnections = new List<ClientConnection>();

            // The thread that accepts TCP data from connected clients.
            private Thread receiveTcpThread;

            // The thread that accepts client connections.
            private Thread acceptClientThread;

            #endregion

            #region UDP

            // Port for the UDP client.
            public static readonly int UdpPort = 8890; // Must be different than server TCP port probably...

            // The UDP cLient.
            public UdpClient udpClient;// =  new UdpClient(UdpPort); //TODO: seems to not work?

            //! The UDP Receiver.
            public UDPReceive udpReceive;

            private Thread receiveUdpThread;

            #endregion

            #endregion

            public void Init()
            {
                Debug.Log("Server.Init() Start");

                try
                {
                    Debug.Log("Server.Init() Create UDP client at port " + UdpPort);
                    udpClient = new UdpClient(UdpPort);

                    udpReceive = new UDPReceive(udpClient);
                    udpReceive.Init();

                    clientConnections = new List<ClientConnection>();

                    // Get host name for local machine.
                    var hostName = Dns.GetHostName();

                    // Get host entry.
                    var hostEntry = Dns.GetHostEntry(hostName);

                    // Print all IP adresses:
                    {
                        Debug.Log("Server IP addresses:");

                        foreach (var ipAddress in hostEntry.AddressList)
                        {
                            Debug.Log("    - " + ipAddress);
                        }
                    }

                    // Get first IPv4 address.
                    var serverIpAddress = hostEntry.AddressList[1];

                    // Create the TCP listener.
                    Debug.Log("Server.Init() Create TCP listener @ " + serverIpAddress.ToString() + ":" + TcpPort.ToString());
                    tcpListener = new TcpListener(serverIpAddress, TcpPort);

                    // Start the server socket.
                    Debug.Log("Server.Init() Start TCP listener");
                    tcpListener.Start();

                    Debug.Log("Server.Init() TCP listener started");

                    // Start a thread to listen for incoming connections from clients on the server TCP socket.
                    acceptClientThread = new Thread(new ThreadStart(AcceptClientFunction));
                    acceptClientThread.IsBackground = true;
                    acceptClientThread.Name = "acceptClientThread";
                    acceptClientThread.Start();

                    // Start a thread to listen for incoming data from connected clients on the server TCP socket.
                    receiveTcpThread = new Thread(new ThreadStart(ReceiveTcpFunction));
                    receiveTcpThread.IsBackground = true;
                    receiveTcpThread.Name = "receiveTcpThread";
                    receiveTcpThread.Start();

                    // Start a thread to listen for incoming data from connected clients on the server UDP socket.
                    receiveUdpThread = new Thread(new ThreadStart(ReceiveUdpFunction));
                    receiveUdpThread.IsBackground = true;
                    receiveUdpThread.Name = "receiveUdpThread";
                    receiveUdpThread.Start();

                    Debug.Log("Server started");
                }
                catch (Exception ex)
                {
                    Debug.LogError("Server.Init(): Exception: " + ex.ToString());
                }
            }

            private void AcceptClientFunction()
            {
                while ((true))
                {
                    try
                    {
                        if (tcpListener.Pending())
                        {
                            var newClientConnection = new ClientConnection();

                            // Create the client TCP socket.
                            newClientConnection.tcpClient = default(TcpClient);

                            // Accept the client TCP socket.
                            newClientConnection.tcpClient = tcpListener.AcceptTcpClient();

                            var clientEndPoint = newClientConnection.tcpClient.Client.RemoteEndPoint as IPEndPoint;
                            var clientIP = clientEndPoint.Address.ToString();
                            newClientConnection.remoteIP = clientIP;

                            Debug.Log("Server: Client connected: " + clientIP);

                            newClientConnection.udpSend = new UDPSend(udpClient);

                            newClientConnection.udpSend.remoteIP = clientIP;
                            newClientConnection.udpSend.remotePort = Client.UdpPort;
                            newClientConnection.udpSend.Init();                            

                            lock (clientConnections)
                            {
                                clientsLockOwner = "AcceptClientFunction";

                                clientConnections.Add(newClientConnection);

                                clientsLockOwner = "None (AcceptClientFunction)";
                            }

                            if (application.ActiveProjectIndex != -1)
                            {
                                // Now the client is connected, make him...
                                
                                // A) ...spawn at the current Project and POI.
                                var teleportCommand = new TeleportCommand();
                                teleportCommand.ProjectIndex = application.ActiveProjectIndex;
                                teleportCommand.POIName = application.ActivePOIName;

                                SendCommand(teleportCommand, newClientConnection.tcpClient);

                                // B) ...be in the same immersion mode.
                                var setImmersionModeCommand = new SetImmersionModeCommand();
                                setImmersionModeCommand.ImmersionModeIndex = application.ActiveImmersionModeIndex;

                                SendCommand(setImmersionModeCommand, newClientConnection.tcpClient);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Server.AcceptClientFunction(): Exception: " + ex.ToString());
                    }
                }
            }

            private void ReceiveUdpFunction()
            {
                while (true)
                {
                    try
                    {
                        lock (clientConnections)
                        {
                            clientsLockOwner = "ReceiveUdpFunction";

                            for (int clientIndex = 0; clientIndex < clientConnections.Count; ++clientIndex)
                            {
                                // Try to receive the x-th client frame.
                                var trackedObjectXML = GetTrackedObjectFromFromUdp(clientIndex);

                                //Debug.Log("Server: Received frame from client " + clientIndex);

                                // Broadcast the x-th client frame to all but the originating client. (so avatars can be updated.)
                                if (trackedObjectXML != null)
                                {
                                    for (int broadcastClientIndex = 0; broadcastClientIndex < clientConnections.Count; ++broadcastClientIndex)
                                    {
                                        if (clientIndex == broadcastClientIndex)
                                            continue; // don't send own client updates back to self...

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
                        Debug.LogError("Server.ReceiveUdpFunction(): Exception: " + ex.ToString());
                    }
                }
            }

            private void ReceiveTcpFunction()
            {
                while (true)
                {
                    try
                    {
                        lock (clientConnections)
                        {
                            clientsLockOwner = "ReceiveTcpFunction";

                            foreach (var clientConnection in clientConnections)
                            {
                                if (clientConnection == null)
                                {
                                    continue;
                                }

                                if (clientConnection.tcpClient == null)
                                {
                                    continue;
                                }

                                var dataFromClient = "";
                                {
                                    var networkStream = clientConnection.tcpClient.GetStream();

                                    var numBytesAvailable = clientConnection.tcpClient.Available;
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
                        Debug.LogError("Server.ReceiveTcpFunction(): Exception: " + ex.ToString());
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
                ICommand command)
            {
                Debug.Log("Server:BoadcastCommand()");

                try
                {
                    string data = GetCommandAsData(command);

                    lock (clientConnections)
                    {
                        clientsLockOwner = "BroadcastCommand";

                        foreach (var clientConnection in clientConnections)
                        {
                            if (clientConnection.tcpClient == null)
                            {
                                Debug.LogWarning("Server.BroadcastCommand(): clientConnection.tcpClient == null");
                                continue;
                            }
                            
                            SendData(data, clientConnection.tcpClient);
                        }

                        clientsLockOwner = "None (BroadcastCommand)";
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Server.BroadcastCommand(): Exception: " + e.Message);
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

            public void SendCommand(
                ICommand command,
                TcpClient tcpClient)
            {
                Debug.Log("Server:SendCommand()");

                try
                {
                    var data = GetCommandAsData(command);

                    SendData(data, tcpClient);
                }
                catch (Exception e)
                {
                    Debug.LogError("Server.SendCommand(): Exception:" + e.Message);
                }
            }

            public void SendData(
                String data,
                TcpClient tcpClient)
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

            private void DisconnectClients()
            {
                Debug.Log("Server::DisconnectClients");

                BroadcastMessage("ServerShuttingDown");
            }

            private void CloseSockets()
            {
                Debug.Log("Server::CloseSockets");

                try
                {
                    lock (clientConnections)
                    {
                        clientsLockOwner = "CloseSockets";

                        // Close the client sockets.
                        foreach (var clientConnection in clientConnections)
                        {
                            clientConnection.tcpClient.Close();
                            //clientConnection.udpSend.Close();
                        }

                        clientConnections.Clear();

                        clientsLockOwner = "None (CloseSockets)";
                    }

                    // Stop the TCP listener.
                    tcpListener.Stop();
                }
                catch (Exception e)
                {
                    Debug.LogError("Server.SendData(): Exception:" + e.Message);
                }
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
                lock (clientConnections)
                {
                    if (clientConnections.Count < clientIndex - 1)
                    {
                        return;
                    }

                    var clientConnection = clientConnections[clientIndex];

                    if (clientConnection == null)
                    {
                        return;
                    }

                    // Temporarily disabled the below check: udpSend is not null but still the if-clause evaluates to true?!? :-s
                    if (clientConnection.udpSend == null)
                    {
                        return;
                    }

                    try
                    {
                        clientConnection.udpSend.sendString(data);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Exception:" + e.Message);
                    }
                }
            }

            public string GetTrackedObjectFromFromUdp(int clientIndex)
            {
                try
                {
                    var clientIP = "";

                    lock (clientConnections)
                    {
                        if (clientConnections.Count < clientIndex - 1)
                        {
                            return null;
                        }

                        var clientConnection = clientConnections[clientIndex];

                        if (clientConnection == null)
                        {
                            return null;
                        }

                        clientIP = clientConnections[clientIndex].remoteIP;
                    }

                    string trackedObjectXML = "";

                    lock (udpReceive.allReceivedUDPPackets)
                    {
                        if (!udpReceive.allReceivedUDPPackets.ContainsKey(clientIP))
                        {
                            return null;
                        }

                        string frameEndTag = "</TrackedObject>";
                        int frameEndTagLength = frameEndTag.Length;
                        int lastFrameEnd = udpReceive.allReceivedUDPPackets[clientIP].LastIndexOf(frameEndTag);

                        if (lastFrameEnd < 0)
                        {
                            return null;
                        }

                        string temp = udpReceive.allReceivedUDPPackets[clientIP].Substring(0, lastFrameEnd + frameEndTagLength);

                        int lastFrameBegin = temp.LastIndexOf("<TrackedObject ");

                        if (lastFrameBegin < 0)
                        {
                            return null;
                        }

                        // Now get the frame string.
                        trackedObjectXML = temp.Substring(lastFrameBegin, temp.Length - lastFrameBegin);

                        // Clear old frames from receivebuffer.
                        udpReceive.allReceivedUDPPackets[clientIP] = udpReceive.allReceivedUDPPackets[clientIP].Substring(lastFrameEnd + frameEndTagLength);
                    }

                    var ser = new XmlSerializer(typeof(TrackedObject));

                    //var reader = new StreamReader(avatarFilePath);
                    var reader = new StringReader(trackedObjectXML);

                    var trackedObject = (TrackedObject)(ser.Deserialize(reader));
                    reader.Close();

                    return trackedObjectXML;
                }
                catch (Exception e)
                {
                    Debug.LogError("Server.GetTrackedObjectFromFromUdp(): Exception: " + e.Message);
                    return null;
                }
            }
        }
    }
}
