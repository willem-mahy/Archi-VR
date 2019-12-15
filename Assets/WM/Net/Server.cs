using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

using UnityEngine;
using WM.Command;

namespace WM
{
    namespace Net
    {
        // Holds all data related to a client connected to the server.
        public class ClientConnection
        {
            //! The IP of the client.
            public string remoteIP;

            #region TCP stuff

            // The client-specific TCP client.
            public TcpClient tcpClient;

            // The client-specific TCP network stream.
            public NetworkStream tcpNetworkStream;

            //
            public string tcpReceivedData = "";

            #endregion

            #region UDP stuff

            // The client-specific UDP sender.
            public UDPSend udpSend;

            #endregion

            public void Close()
            {
                tcpNetworkStream.Close();
                tcpNetworkStream = null;

                tcpClient.Close();
                tcpClient = null;
            }
        }

        abstract public class Server : MonoBehaviour
        {
            #region Variables

            public string Status = "";

            #region TCP

            public static readonly int TcpPort =
                //8886;
                //8887;
                8888;

            //! The TCP listener.
            TcpListener tcpListener;

            // Used for debugging client list lock related deadlocks.
            private string clientsLockOwner = "";

            public string ClientsLockOwner
            {
                get { return clientsLockOwner; }
            }

            /// <summary>
            /// The client connections.
            /// </summary>
            /*private*/
            protected List<ClientConnection> clientConnections = new List<ClientConnection>();

            /// <summary>
            /// Get a string with information about connected clients.
            /// </summary>
            /// <returns></returns>
            public string GetClientInfo()
            {
                string info = "";
                lock (this.clientConnections)
                {
                    foreach (var clientConnection in clientConnections)
                    {
                        info += "- IP: " + clientConnection.remoteIP + "\n";
                    }
                }
                return info;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public int NumClients
            {
                get
                {
                    lock (clientConnections)
                    {
                        return clientConnections.Count;
                    }
                }
            }

            // The thread that accepts TCP data from connected clients.
            private Thread receiveTcpThread;

            // The thread that broadcasts messages to any potential clients in order for them to find the server.
            private Thread broadcastThread;

            // The thread that accepts client connections.
            private Thread acceptClientThread;

            #endregion

            #region UDP

            public static readonly string UdpBroadcastMessage = "Hello from ArchiVR server";

            // Broadcast UDP port.
            public static readonly int BroadcastUdpPort = 8892;

            // CLient UDP port.
            public static readonly int UdpPort = 8890; // Must be different than server TCP port probably...

            // The UDP cLient.
            UdpClient udpClient;// =  new UdpClient(UdpPort); //TODO: seems to not work?

            //! The UDP Receiver.
            UDPReceive udpReceive;

            Thread receiveUdpThread;

            #endregion

            private bool shutDown = false;

            #endregion

            /// <summary>
            /// 
            /// </summary>
            public void Init()
            {
                WM.Logger.Debug("Server.Init() Start");

                shutDown = false;

                Status = "Initializing";

                try
                {
                    Debug.Log("Server.Init() Create UDP client at port " + UdpPort);
                    udpClient = new UdpClient(UdpPort);

                    Status = "UdpClient initialized";
                    WM.Logger.Debug("Server.Init(): UdpClient initialized");

                    udpReceive = new UDPReceive(udpClient);
                    udpReceive.Init();

                    Status = "UdpReceive initialized";
                    WM.Logger.Debug("Server.Init(): UdpReceive initialized");

                    // Get host name for local machine.
                    var hostName = Dns.GetHostName();

                    // Get host entry.
                    var hostEntry = Dns.GetHostEntry(hostName);

                    // Print all IP adresses:
                    {
                        WM.Logger.Debug("Server IP addresses:");

                        foreach (var ipAddress in hostEntry.AddressList)
                        {
                            WM.Logger.Debug("    - " + ipAddress);
                        }
                    }

                    // Get first IPv4 address.
                    var serverIpAddress = hostEntry.AddressList[hostEntry.AddressList.Length - 1];

                    // Create the TCP listener.
                    WM.Logger.Debug("Server.Init(); Create TCP listener @ " + serverIpAddress.ToString() + ":" + TcpPort.ToString());
                    tcpListener = new TcpListener(serverIpAddress, TcpPort);

                    // Start the server socket.
                    WM.Logger.Debug("Server.Init(): Start TCP listener");
                    tcpListener.Start();

                    Status = "TcpListener started";

                    WM.Logger.Debug("Server.Init() TCP listener started");

                    // Start a thread to broadcast via UDP.
                    broadcastThread = new Thread(new ThreadStart(BroadcastFunction));
                    broadcastThread.IsBackground = true;
                    broadcastThread.Name = "broadcastThread";
                    broadcastThread.Start();

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

                    Status = "Running";
                    WM.Logger.Debug("Server running");
                }
                catch (Exception ex)
                {
                    WM.Logger.Error("Server.Init(): Exception: " + ex.ToString());
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void Shutdown()
            {
                shutDown = true;

                // Stop broadcasting for potential new clients.
                if (broadcastThread != null)
                {
                    broadcastThread.Join();

                    broadcastThread = null;
                }

                // Stop listening for new clients.
                if (acceptClientThread != null)
                {
                    acceptClientThread.Join();

                    acceptClientThread = null;
                }

                // Close TCP listener used to listen for new clients.
                if (tcpListener != null)
                {
                    tcpListener.Stop();

                    tcpListener = null;
                }

                // Notify existing clients that the server is shutting down.
                BroadcastCommand(new ServerShutdownCommand());

                // Stop listening to existing clients for UDP messages
                if (receiveUdpThread != null)
                {
                    receiveUdpThread.Join();

                    receiveUdpThread = null;
                }

                if (udpClient != null)
                {
                    udpClient.Close();

                    udpClient = null;
                }

                // Stop listening to existing clients for TCP messages
                if (receiveTcpThread != null)
                {
                    receiveTcpThread.Join();

                    receiveTcpThread = null;
                }

                lock (clientConnections)
                {
                    clientsLockOwner = "Shutdown";

                    foreach (var clientConnection in clientConnections)
                    {
                        clientConnection.tcpNetworkStream.Close();
                        clientConnection.tcpNetworkStream = null;
                        
                        clientConnection.tcpClient.Close();
                        clientConnection.tcpClient = null;
                    }

                    clientConnections.Clear();

                    clientsLockOwner = "None (Shutdown)";
                }
            }

            /// <summary>
            /// Thread function executed by the 'Accept Client' thread.
            /// </summary>
            private void BroadcastFunction()
            {
                try
                {
                    var broadcastUdpClient = new UdpClient(BroadcastUdpPort);

                    // Encode data to UTF8-encoding.
                    byte[] udpBroadcastMessageData = Encoding.UTF8.GetBytes(UdpBroadcastMessage);

                    var ep = new IPEndPoint(IPAddress.Broadcast, Server.BroadcastUdpPort);

                    while (!shutDown)
                    {
                    
                            // Send udpBroadcastMessageData to any potential clients.
                            broadcastUdpClient.Send(udpBroadcastMessageData, udpBroadcastMessageData.Length, ep);

                            Thread.Sleep(500);
                    }

                    broadcastUdpClient.Close();
                }
                catch (Exception ex)
                {
                    WM.Logger.Error("Server.BroadcastFunction(): Exception: " + ex.ToString());
                }
            }

            /// <summary>
            /// Thread function executed by the 'Accept Client' thread.
            /// </summary>
            private void AcceptClientFunction()
            {
                while (!shutDown)
                {
                    try
                    {
                        if (tcpListener.Pending())
                        {
                            var newClientConnection = new ClientConnection();

                            // Accept the client TCP socket.
                            newClientConnection.tcpClient = tcpListener.AcceptTcpClient();
                            newClientConnection.tcpNetworkStream = newClientConnection.tcpClient.GetStream();

                            var clientEndPoint = newClientConnection.tcpClient.Client.RemoteEndPoint as IPEndPoint;
                            var clientIP = clientEndPoint.Address.ToString();
                            newClientConnection.remoteIP = clientIP;

                            WM.Logger.Debug("Server.AcceptClientFunction(): Client connected: " + clientIP);

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

                            OnClientConnected(newClientConnection);
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                    }
                    catch (Exception ex)
                    {
                        WM.Logger.Error("Server.AcceptClientFunction(): Exception: " + ex.ToString());
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="newClientConnection"></param>
            abstract public void OnClientConnected(ClientConnection newClientConnection);

            /// <summary>
            /// Thread function executed by the 'Receive UDP' thread.
            /// </summary>
            private void ReceiveUdpFunction()
            {
                while (!shutDown)
                {
                    try
                    {
                        lock (clientConnections)
                        {
                            clientsLockOwner = "ReceiveUdpFunction";

                            for (int clientIndex = 0; clientIndex < clientConnections.Count; ++clientIndex)
                            {
                                // Try to receive the x-th client frame.
                                var messageXML = GetLastMessageXML(clientIndex);

                                //Debug.Log("Server: Received frame from client " + clientIndex);

                                // Broadcast the x-th client frame to all but the originating client. (so avatars can be updated.)
                                if (messageXML != null)
                                {
                                    for (int broadcastClientIndex = 0; broadcastClientIndex < clientConnections.Count; ++broadcastClientIndex)
                                    {
                                        if (clientIndex == broadcastClientIndex)
                                            continue; // don't send own client updates back to self...

                                        SendDataToUdp(messageXML, broadcastClientIndex);
                                    }
                                }
                            }

                            clientsLockOwner = "None (last:ReceiveFromClientsFunction)";
                        }
                    }
                    catch (Exception ex)
                    {
                         WM.Logger.Error("Server.ReceiveUdpFunction(): Exception: " + ex.ToString());
                    }
                }
            }

            /// <summary>
            /// Thread function executed by the 'Receive TCP' thread.
            /// </summary>
            private void ReceiveTcpFunction()
            {
                while (!shutDown)
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

                                if (clientConnection.tcpNetworkStream == null)
                                {
                                    continue;
                                }

                                {
                                    var networkStream = clientConnection.tcpNetworkStream;

                                    if (clientConnection.tcpNetworkStream.DataAvailable)
                                    {
                                        // Receive from client.
                                        var bytesFromClient = new byte[clientConnection.tcpClient.Available];
                                        int bytesRead = networkStream.Read(bytesFromClient, 0, clientConnection.tcpClient.Available);
                                        clientConnection.tcpReceivedData+= System.Text.Encoding.ASCII.GetString(bytesFromClient, 0, bytesRead);
                                    }
                                }

                                int EndTagLength = Message.XmlEndTag.Length;

                                int firstMessageBegin = clientConnection.tcpReceivedData.IndexOf(Message.XmlBeginTag);

                                if (firstMessageBegin < 0)
                                {
                                    continue;
                                }

                                // Remove all data in front of first message.
                                clientConnection.tcpReceivedData = clientConnection.tcpReceivedData.Substring(firstMessageBegin);

                                int firstMessageEnd = clientConnection.tcpReceivedData.IndexOf(Message.XmlEndTag);

                                if (firstMessageEnd < 0)
                                {
                                    continue;
                                }

                                // XML-deserialize the message.
                                int messageLength = firstMessageEnd + EndTagLength;
                                string messageXML = clientConnection.tcpReceivedData.Substring(0, messageLength);

                                // Remove the first message (and all data in front of it) from the client connection receive buffer.
                                var remainder = clientConnection.tcpReceivedData.Substring(firstMessageEnd + EndTagLength);
                                clientConnection.tcpReceivedData = remainder;

                                // Process the message
                                if (!ProcessMessage(messageXML, clientConnection))
                                {
                                    break;
                                }
                            }

                            clientsLockOwner = "None (last:ReceiveFromClientsFunction)";
                        }
                        //Thread.Sleep(10);
                    }
                    catch (Exception ex)
                    {
                        WM.Logger.Error("Server.ReceiveTcpFunction(): Exception: " + ex.ToString());
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="messageXML"></param>
            /// <param name="clientConnection"></param>
            /// <returns></returns>
            private bool ProcessMessage(
                string messageXML,
                ClientConnection clientConnection)
            {
                // XML-deserialize the Message.
                var ser = new XmlSerializer(typeof(Message));

                var reader = new StringReader(messageXML);

                var message = (Message)(ser.Deserialize(reader));

                reader.Close();

                // Binary-deserialize the object from the message.
                var obj = message.Deserialize();

                if (obj is ConnectClientCommand)
                {
                    var ccc = (ConnectClientCommand)obj;
                    PropagateData(messageXML, clientConnection);
                }
                else if (obj is DisconnectClientCommand)
                {
                    // Client indicates that it is disconnecting.
                    var dcc = (DisconnectClientCommand)obj;                    
                    PropagateData(messageXML, clientConnection);

                    // Step 1: remove connection from list of connections, so that no UDP or TCP reads/Writes will happen on its network streams anymore.
                    clientConnections.Remove(clientConnection);

                    // Step 2: send an acknoledge to the client that it is safe to continue disconnecting.
                    SendData(Message.EncodeObjectAsXml(new ClientDisconnectAcknoledgeMessage()), clientConnection);

                    // Step 3: close the clientconnection network streams.
                    clientConnection.Close();                    

                    return false;
                }
                else
                {
                    // Application-specific message, EG ArchiVR.SetModelLocationCommand
                    BroadcastData(messageXML); // TODO: Implement a way to figure out wheter to propagate or broadcast messages here.
                }

                return true;
            }
            
            /// <summary>
            /// 
            /// </summary>
            /// <param name="command"></param>
            public void BroadcastCommand(
                ICommand command)
            {
                Debug.Log("Server:BoadcastCommand()");

                try
                {
                    string data = Message.EncodeObjectAsXml(command);

                    BroadcastData(data);
                }
                catch (Exception e)
                {
                    WM.Logger.Error("Server.BroadcastCommand(): Exception: " + e.Message);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="command"></param>
            /// <param name="sourceClientConnection"></param>
            /*private*/
            protected void PropagateCommand(
                ICommand command,
                ClientConnection sourceClientConnection)
            {
                Debug.Log("Server:PropagateCommand()");

                try
                {
                    string data = Message.EncodeObjectAsXml(command);

                    PropagateData(data, sourceClientConnection);
                }
                catch (Exception e)
                {
                    WM.Logger.Error("Server.PropagateCommand(): Exception: " + e.Message);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            private void BroadcastData(
                string data)
            {
                Debug.Log("Server:BroadcastData()");

                try
                {
                    lock (clientConnections)
                    {
                        clientsLockOwner = "BroadcastData";

                        foreach (var clientConnection in clientConnections)
                        {
                            SendData(data, clientConnection);
                        }

                        clientsLockOwner = "None (BoadcastData)";
                    }
                }
                catch (Exception e)
                {
                    WM.Logger.Error("Server.BroadcastData(): Exception: " + e.Message);
                }
            }
            
            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="sourceClientConnection"></param>
            private void PropagateData(
                string data,
                ClientConnection sourceClientConnection)
            {
                Debug.Log("Server:PropagateData()");

                try
                {
                    lock (clientConnections)
                    {
                        clientsLockOwner = "PropagateData";

                        foreach (var clientConnection in clientConnections)
                        {
                            if (clientConnection == sourceClientConnection)
                            {
                                continue; // Do not send to the source client connection.
                            }

                            SendData(data, clientConnection);
                        }

                        clientsLockOwner = "None (PropagateData)";
                    }
                }
                catch (Exception e)
                {
                    WM.Logger.Error("Server.PropagateData(): Exception: " + e.Message);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="command"></param>
            /// <param name="clientConnection"></param>
            /*private*/
            protected void SendCommand(
                ICommand command,
                ClientConnection clientConnection)
            {
                Debug.Log("Server:SendCommand()");

                try
                {
                    if (clientConnection.tcpNetworkStream == null)
                    {
                        Debug.LogWarning("Server.SendCommand(): clientConnection.tcpNetworkStream == null");
                        return;
                    }

                    var data = Message.EncodeObjectAsXml(command);

                    SendData(data, clientConnection);
                }
                catch (Exception e)
                {
                    WM.Logger.Error("Server.SendCommand(): Exception:" + e.Message);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="clientConnection"></param>
            private void SendData(
                String data,
                ClientConnection clientConnection)
            {
                Debug.Log("Server:SendData()");

                try
                {
                    if (clientConnection.tcpNetworkStream == null)
                    {
                        Debug.LogWarning("Server.SendData(): clientConnection.tcpNetworkStream == null");
                        return;
                    }

                    var bytes = Encoding.ASCII.GetBytes(data);
                    clientConnection.tcpNetworkStream.Write(bytes, 0, bytes.Length);
                    clientConnection.tcpNetworkStream.Flush();
                }
                catch (Exception e)
                {
                    WM.Logger.Error("Server.SendData(): Exception:" + e.Message);
                }
            }

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
                         WM.Logger.Error("Exception:" + e.Message);
                    }
                }
            }

            /// <summary>
            /// Gets a string with the XML-encoded last message from the client at given index.
            /// Then clears the content of the client receive buffer up to and including the returned message.
            /// </summary>
            /// <param name="clientIndex"></param>
            /// <returns>String with the XML-encoded last message from the client at given index. If no message was received from the given client, returns null.</returns>
            public string GetLastMessageXML(int clientIndex)
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

                    string messageXML = "";

                    lock (udpReceive.allReceivedUDPPackets)
                    {
                        if (!udpReceive.allReceivedUDPPackets.ContainsKey(clientIP))
                        {
                            return null;
                        }

                        int frameEndTagLength = Message.XmlEndTag.Length;
                        int lastFrameEnd = udpReceive.allReceivedUDPPackets[clientIP].LastIndexOf(Message.XmlEndTag);

                        if (lastFrameEnd < 0)
                        {
                            return null;
                        }

                        string temp = udpReceive.allReceivedUDPPackets[clientIP].Substring(0, lastFrameEnd + frameEndTagLength);

                        int lastFrameBegin = temp.LastIndexOf(Message.XmlBeginTag);

                        if (lastFrameBegin < 0)
                        {
                            return null;
                        }

                        // Now get the message XML string.
                        messageXML = temp.Substring(lastFrameBegin, temp.Length - lastFrameBegin);

                        // TODO?:
                        // test that the message can be XML deserialized properly
                        // test that the object contained in the message can be deserialized properly
                        // -> If not, continue search for older message...

                        // Clear old messages from receivebuffer.
                        udpReceive.allReceivedUDPPackets[clientIP] = udpReceive.allReceivedUDPPackets[clientIP].Substring(lastFrameEnd + frameEndTagLength);
                    }

                    /*
                        var ser = new XmlSerializer(typeof(AvatarState));

                        //var reader = new StreamReader(avatarFilePath);
                        var reader = new StringReader(messageXML);

                        var trackedObject = (AvatarState)(ser.Deserialize(reader));
                        reader.Close();
                    */

                    return messageXML;
                }
                catch (Exception e)
                {
                     WM.Logger.Error("Server.GetLastMessageXML(" + clientIndex + "): Exception: " + e.Message);
                    return null;
                }
            }
        }
    }
}
