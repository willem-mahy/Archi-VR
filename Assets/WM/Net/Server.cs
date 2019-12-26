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

namespace WM.Net
{
    /// <summary>
    /// Used to send Server information to Clients over TCP upon connection initialization.
    /// </summary>
    [Serializable]
    public class ServerInfo
    {
        public string IP;

        public int TcpPort;

        public int UdpPort;

        public ServerInfo(String IP, int tcpPort, int udpPort)
        {
            this.IP = IP;
            TcpPort = tcpPort;
            UdpPort = udpPort;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    abstract public class Server : MonoBehaviour
    {
        /// <summary>
        /// Holds all data related to a client connected to the server.
        /// </summary>    
        public class ClientConnection
        {
            public string ClientID
            {
                get
                {
                    return remoteIP + ":" + remotePortTCP;
                }
            }

            /// <summary>
            /// The remote endpoint IP.
            /// </summary>
            public string remoteIP
            {
                get;
                private set;
            } = "";

            /// <summary>
            /// The remote endpoint port.
            /// </summary>
            public int remotePortTCP
            {
                get;
                private set;
            } = -1;

            /// <summary>
            /// The remote endpoint port.
            /// </summary>
            public int remotePortUDP
            {
                get;
                private set;
            } = -1;

            #region TCP stuff

            // The client-specific TCP client. (owned by the parent Server instance)
            private TcpClient tcpClient;

            // The client-specific TCP network stream. (TODO: make private!)
            private NetworkStream tcpNetworkStream;

            //
            public string tcpReceivedData = "";

            #endregion

            #region UDP stuff

            // The client-specific UDP sender (owned by self.)
            private UDPSend udpSend;

            #endregion

            /// <summary>
            /// 
            /// </summary>
            /// <param name="tcpClient"></param>
            /// <param name="udpClient"></param>
            public ClientConnection(
                TcpClient tcpClient,
                UdpClient udpClient)
            {
                this.tcpClient = tcpClient;
                tcpNetworkStream = tcpClient.GetStream();

                var tcpRemoteEndPoint = tcpClient.Client.RemoteEndPoint as IPEndPoint;
                remoteIP = tcpRemoteEndPoint.Address.ToString();
                remotePortTCP = tcpRemoteEndPoint.Port;

                // Receive the port of the Client UDPReceive at the Client's side.
                while (!ReceiveDataTCP())
                {
                }

                remotePortUDP = int.Parse(tcpReceivedData);

                udpSend = new UDPSend(udpClient);
                udpSend.remoteIP = remoteIP;
                udpSend.remotePort = remotePortUDP;
                udpSend.Init();

                WM.Logger.Debug("ClientConnection: Client UDP port is " + remotePortUDP);
            }

            /// <summary>
            /// 
            /// </summary>
            public void Close()
            {
                tcpNetworkStream.Close();
                tcpNetworkStream = null;

                tcpClient.Close();
                tcpClient = null;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            public void SendUDP(string data)
            {
                if (udpSend == null)
                {
                    Logger.Error("ClientConnection.SendUDP(): udpSend is null!");
                    return;
                }

                udpSend.SendString(data);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            public void SendTCP(string data)
            {
                if (tcpClient == null)
                {
                    Logger.Error("ClientConnection.SendTCP(): tcpClient is null!");
                    return;
                }

                var bytes = Encoding.ASCII.GetBytes(data);
                tcpNetworkStream.Write(bytes, 0, bytes.Length);
                tcpNetworkStream.Flush();
            }

            /// <summary>
            /// Receives any available data from the TCP socket.
            /// </summary>
            /// <returns>A bool indicating hether data was received.</returns>
            public bool ReceiveDataTCP()
            {
                if (tcpClient == null)
                {
                    Logger.Error("ClientConnection.ReceiveDataTCP(): tcpClient is null!");
                    return false;
                }

                if (tcpNetworkStream == null)
                {
                    Logger.Error("ClientConnection.ReceiveDataTCP(): tcpNetworkStream is null!");
                    return false;
                }

                if (!tcpNetworkStream.DataAvailable)
                {
                    return false;
                }

                var bytesFromClient = new byte[tcpClient.Available];
                int bytesRead = tcpNetworkStream.Read(bytesFromClient, 0, tcpClient.Available);
                tcpReceivedData += System.Text.Encoding.ASCII.GetString(bytesFromClient, 0, bytesRead);

                return true; // Data received
            }
        }

        #region Variables

        public string Status = "";

        #region TCP

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

        #region UDP Broadcast

        /// <summary>
        /// The broadcast message to be broadcasted by the server.
        /// To be implemented by concrete server types.
        /// </summary>
        public string UdpBroadcastMessage
        {
            get;
            protected set;
        }

        /// <summary>
        /// The UDP port to which the server, while running, continuously broadcasts the UdpBroadcastMessage to.
        /// Clients should make an UDP endpoint on this port and listen to it to discover running servers.
        /// 
        /// Note:
        /// This UDP port is reused by multiple Clients in unit testing.
        /// This is not an issue, because:
        ///     A) Clients are only using this port while discovering server during their Connect(), and 
        ///     B) and clients always connect sequentially (never concurrently) in unit testing.
        /// </summary>
        public static readonly int UdpBroadcastRemotePort = 8881;

        #endregion UDP Broadcast

        #region UDP Message send/receive

        // The UDP client.
        UdpClient udpClient;

        //! The UDP Receiver.
        UDPReceive udpReceive;

        Thread receiveUdpThread;

        #endregion UDP Message send/receive

        #endregion

        private bool shutDown = false;

        #endregion

        public int TcpPort
        {
            get
            {
                return ((IPEndPoint)(tcpListener.LocalEndpoint)).Port;
            }
        }

        public int UdpPort
        {
            get
            {
                return ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            WM.Logger.Debug("Server.Init()");

            shutDown = false;

            Status = "Initializing";

            try
            {
                udpClient = new UdpClient(0);
                WM.Logger.Debug("Server.Init(): UdpClient bound to port " + UdpPort);

                udpReceive = new UDPReceive(udpClient);
                udpReceive.Init();
                WM.Logger.Debug("Server.Init(): UdpReceive initialized");

                // Get host name for local machine.
                var hostName = Dns.GetHostName();

                // Get host entry.
                var hostEntry = Dns.GetHostEntry(hostName);

                // Print all IP adresses:
                {
                    WM.Logger.Debug("Server.Init(): Host IP addresses:");

                    foreach (var ipAddress in hostEntry.AddressList)
                    {
                        WM.Logger.Debug("    - " + ipAddress);
                    }
                }

                // Get first IPv4 address.
                var serverIpAddress = hostEntry.AddressList[hostEntry.AddressList.Length - 1];

                // Create the TCP listener.
                tcpListener = new TcpListener(serverIpAddress, 0);
                WM.Logger.Debug("Server.Init(): TCP listener bound to port " + TcpPort);

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
            WM.Logger.Debug("Server.Shutdown(): Start");

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

            if (udpReceive != null)
            {
                udpReceive.Shutdown();
                udpReceive = null;
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
                    clientConnection.Close();
                }

                clientConnections.Clear();

                clientsLockOwner = "None (Shutdown)";
            }

            WM.Logger.Debug("Server.Shutdown(): End");
        }

        /// <summary>
        /// Thread function executed by the 'Broadcast' thread.
        /// </summary>
        private void BroadcastFunction()
        {
            try
            {
                var broadcastUdpClient = new UdpClient(0);
                var broadcastUdpRemoteEndPoint = new IPEndPoint(IPAddress.Broadcast, Server.UdpBroadcastRemotePort);

                var UdpBroadcastMessage = WM.Net.Message.EncodeObjectAsXml(new ServerInfo(((IPEndPoint)this.tcpListener.LocalEndpoint).Address.ToString() ,TcpPort, UdpPort));

                WM.Logger.Debug(
                    string.Format("Server: Starting to UDP broadcast Message '{0}' from port {1} to port {2}",
                    UdpBroadcastMessage,
                    broadcastUdpRemoteEndPoint.Port,
                    UdpBroadcastRemotePort));

                // Encode data to UTF8-encoding.
                byte[] udpBroadcastMessageData = Encoding.UTF8.GetBytes(UdpBroadcastMessage);

                while (!shutDown)
                {   
                        // Send udpBroadcastMessageData to any potential clients.
                        broadcastUdpClient.Send(udpBroadcastMessageData, udpBroadcastMessageData.Length, broadcastUdpRemoteEndPoint);

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
            WM.Logger.Debug("AcceptClientFunction() Start()");

            while (!shutDown)
            {
                try
                {
                    if (tcpListener.Pending())
                    {
                        WM.Logger.Debug("Server.AcceptClientFunction(): Client connecting...");
                        
                        // Accept the client TCP socket.
                        var tcpClient = tcpListener.AcceptTcpClient();

                        // Send Udp Port
                        {
                            var tcpNetworkStream = tcpClient.GetStream();
                            var bytes = Encoding.ASCII.GetBytes("" + UdpPort);
                            tcpNetworkStream.Write(bytes, 0, bytes.Length);
                            tcpNetworkStream.Flush();
                        }

                        var newClientConnection = new ClientConnection(tcpClient, udpClient);

                        lock (clientConnections)
                        {
                            clientsLockOwner = "AcceptClientFunction";

                            clientConnections.Add(newClientConnection);

                            clientsLockOwner = "None (AcceptClientFunction)";
                        }

                        WM.Logger.Debug("Server.AcceptClientFunction(): Client '" + newClientConnection.ClientID + "' connected.");

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
                            // Get the last newly received message from the client. (null if none received.)
                            var messageXML = GetLastMessageXML(clientIndex);

                            // Broadcast the message to all but the originating client. (so avatars can be updated.)
                            if (messageXML != null)
                            {
                                WM.Logger.Debug("Server: Received a UDP message from client " + clientIndex);

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

                            if (clientConnection.ReceiveDataTCP())
                            {
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
                        }

                        clientsLockOwner = "None (last:ReceiveFromClientsFunction)";
                    }
                    Thread.Sleep(10);
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
                WM.Logger.Debug(string.Format("Server.ProcessMessage: Client {0} disconnecting.", clientConnection.ClientID));

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
            WM.Logger.Debug("Server:BroadcastCommand()");

            if (NumClients == 0)
            {
                return;
            }

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
            WM.Logger.Debug("Server:PropagateCommand()");

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
            WM.Logger.Debug("Server:BroadcastData()");

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
            WM.Logger.Debug("Server:PropagateData()");

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
            WM.Logger.Debug("Server:SendCommand()");

            try
            {
                /*
                if (clientConnection.tcpNetworkStream == null)
                {
                    WM.Logger.Warning("Server.SendCommand(): clientConnection.tcpNetworkStream == null");
                    return;
                }
                */

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
            WM.Logger.Debug("Server.SendData()");

            try
            {
                clientConnection.SendTCP(data);
            }
            catch (Exception e)
            {
                WM.Logger.Error("Server.SendData(): Exception:" + e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="clientIndex"></param>
        public void SendDataToUdp(string data, int clientIndex)
        {
            WM.Logger.Debug("Server.SendDataToUdp()");

            lock (clientConnections)
            {
                if (clientConnections.Count < clientIndex - 1)
                {
                    return;
                }

                var clientConnection = clientConnections[clientIndex];

                if (clientConnection == null)
                {
                    Logger.Error("ClientConnection is null!");
                }

                try
                {
                    clientConnection.SendUDP(data);                    
                }
                catch (Exception e)
                {
                        WM.Logger.Error("Server.SendDataToUdp(): Exception:" + e.Message);
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
                var clientKey = "";

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

                    clientKey = UDPReceive.GetRemoteEndpointKey(clientConnection.remoteIP, clientConnection.remotePortTCP);
                }

                string messageXML = "";

                lock (udpReceive.allReceivedUDPPackets)
                {
                    if (!udpReceive.allReceivedUDPPackets.ContainsKey(clientKey))
                    {
                        return null;
                    }

                    int frameEndTagLength = Message.XmlEndTag.Length;
                    int lastFrameEnd = udpReceive.allReceivedUDPPackets[clientKey].LastIndexOf(Message.XmlEndTag);

                    if (lastFrameEnd < 0)
                    {
                        return null;
                    }

                    string temp = udpReceive.allReceivedUDPPackets[clientKey].Substring(0, lastFrameEnd + frameEndTagLength);

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
                    udpReceive.allReceivedUDPPackets[clientKey] = udpReceive.allReceivedUDPPackets[clientKey].Substring(lastFrameEnd + frameEndTagLength);
                }

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
