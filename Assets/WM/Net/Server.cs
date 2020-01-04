﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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
            #region Variables

            /// <summary>
            /// The client ID of the connected Client.
            /// </summary>
            public Guid ClientID
            {
                get;
                private set;
            } = Guid.Empty;

            /// <summary>
            /// The remote endpoint IP.
            /// </summary>
            public string RemoteIP
            {
                get;
                private set;
            } = "";

            /// <summary>
            /// The remote endpoint port.
            /// </summary>
            public int RemotePortTCP
            {
                get;
                private set;
            } = -1;

            /// <summary>
            /// The remote endpoint port.
            /// </summary>
            public int RemotePortUDP
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

            #endregion TCP stuff

            #region UDP stuff

            // The client-specific UDP sender (owned by self.)
            private UDPSend udpSend;

            #endregion UDP stuff

            #endregion Variables

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
                RemoteIP = tcpRemoteEndPoint.Address.ToString();
                RemotePortTCP = tcpRemoteEndPoint.Port;

                // Receive the port of the Client UDPReceive at the Client's side.
                while (!ReceiveDataTCP())
                {
                }

                var obj = Message.GetObjectFromMessageXML(tcpReceivedData);
                tcpReceivedData = "";

                if (obj is ClientInfo clientInfo)
                {
                    WM.Logger.Debug("ClientConnection: Received ClientInfo");

                    ClientID = clientInfo.ID;
                    RemotePortUDP = clientInfo.UdpPort;

                    udpSend = new UDPSend(udpClient);
                    udpSend.remoteIP = RemoteIP;
                    udpSend.remotePort = RemotePortUDP;
                    udpSend.Init();
                }
                else
                {
                    throw new Exception("ClientConnection: Received non - ClientInfo");
                }
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

        /// <summary>
        /// The unique Server ID.
        /// </summary>
        public Guid ID
        {
            get;
        } = Guid.NewGuid();

        /// <summary>
        /// Short version of the client ID.
        /// To be used for debug logging purposes only - the short ID is NOT guaranteed to be unique!
        /// </summary>
        public String LogID
        {
            get { return "Server[" + WM.Net.NetUtil.ShortID(ID) + "]"; }
        }

        /// <summary>
        /// Used for debugging client list lock related deadlocks.
        /// </summary>
        private string clientsLockOwner = "";

        /// <summary>
        /// Used for debugging client list lock related deadlocks.
        /// </summary>
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
                    info += "- IP: " + clientConnection.RemoteIP + "\n";
                }
            }
            return info;
        }

        /// <summary>
        /// Gets the number of connected Clients.
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

        /// <summary>
        /// The thread that accepts TCP data from connected clients.
        /// </summary>
        private Thread receiveTcpThread;

        /// <summary>
        /// The thread that broadcasts messages to any potential clients in order for them to find the server.
        /// </summary>
        private Thread broadcastThread;

        /// <summary>
        /// The thread that accepts client connections.
        /// </summary>
        private Thread acceptClientThread;

        #region TCP

        //! The TCP listener.
        TcpListener tcpListener;

        /// <summary>
        /// The port of the TCP Listener, to which clients can connect.
        /// </summary>
        public int TcpPort
        {
            get
            {
                return ((IPEndPoint)(tcpListener.LocalEndpoint)).Port;
            }
        }

        #endregion

        #region UDP

        #region UDP Broadcast

        /// <summary>
        /// The UDP port to which the server, while running, continuously broadcasts its own ServerInfo to.
        /// Clients should make an UDP endpoint on this port and listen to it to discover running servers.
        /// 
        /// Note:
        /// This is the only hard-coded port in the WM.Net codebase.
        /// It is reused by multiple Clients in unit testing.
        /// Although these Clients run on the same host, this is not an issue, because:
        ///     A) Clients are only using this port while discovering server during their Connect(), and 
        ///     B) and clients always connect sequentially (never concurrently) in unit testing.
        /// </summary>
        public static readonly int UdpBroadcastRemotePort = 8881;

        /// <summary>
        /// The port of the local endpoint of the UdpCLient used to send/receive non-critical data to/from Clients.
        /// </summary>
        public int UdpPort
        {
            get
            {
                return ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
            }
        }

        #endregion UDP Broadcast

        #region UDP Message send/receive

        // The UDP client.
        UdpClient udpClient;

        //! The UDP Receiver.
        UDPReceive udpReceive;

        Thread receiveUdpThread;

        #endregion UDP Message send/receive

        #endregion

        #region Internal state

        /// <summary>
        ///  The possible server states.
        ///  
        /// A freshly constructed Server is always in 'NotRunning' state.
        /// 
        /// By calling 'Init()', the Server transitions into state 'Initializing'.
        /// 
        /// If initialization procedure succeeds, the Server automatically transitions into state 'Running'
        /// 
        /// TODO? If initialization fails, the Server automatically transitions into state 'InitializationFailed'
        /// 
        /// By calling 'Shutdown()', the Server transitions into state 'ShuttingDown'.
        /// 
        /// If shutdown procedure succeeds, the Server automatically transitions into state 'Running'
        /// 
        /// TODO? If initialization procedure fails, the Server automatically transitions into state 'ShutdownFailed'
        /// </summary>
        public enum ServerState
        {
            Initializing,
            Running,
            ShuttingDown,
            NotRunning,
        }

        /// <summary>
        /// Lock object for the 'State'.
        /// </summary>
        private System.Object stateLock = new System.Object();

        /// <summary>
        /// The state.
        /// </summary>
        public ServerState State
        {
            get;
            private set;
        } = ServerState.NotRunning;

        /// <summary>
        /// Returns a string representing the internal server state.
        /// To be used for displaying in the UI.
        /// </summary>
        public String Status
        {
            get
            {
                switch (State)
                {
                    case ServerState.NotRunning:
                        return "Not running";
                    case ServerState.Initializing:
                        return "Initializing";
                    case ServerState.Running:
                        return "Running";
                    case ServerState.ShuttingDown:
                        return "Shutting down";
                    default:
                        return "Unknown server state '" + State.ToString() + "'";
                }
            }
        }

        #endregion Internal State

        #endregion

        /// <summary>
        /// \pre The Server must be in state 'NotRunning' for this method to succeed.
        /// </summary>
        public void Init()
        {
            var callLogTag = LogID + ".Init()";
            WM.Logger.Debug(callLogTag);

            lock (stateLock)
            {
                switch (State)
                {
                    case ServerState.NotRunning:
                        State = ServerState.Initializing;
                        break;
                    case ServerState.Initializing:
                        throw new Exception("Init() can not be called on Server while it is Initializing.");
                    case ServerState.Running:
                        throw new Exception("Init() can not be called on Server while it is Running.");
                    case ServerState.ShuttingDown:
                        throw new Exception("Init() can not be called on Server while it is ShuttingDown.");
                }

                try
                {
                    udpClient = new UdpClient(0);
                    WM.Logger.Debug(callLogTag + ": UdpClient bound to port " + UdpPort);

                    udpReceive = new UDPReceive(udpClient);
                    udpReceive.Init();
                    WM.Logger.Debug(callLogTag + ": UdpReceive initialized");

                    var ipAddress = WM.Net.NetUtil.GetLocalIPAddress();

                    // Create the TCP listener, and bind it to any available port.
                    tcpListener = new TcpListener(ipAddress, 0);
                    tcpListener.Start();
                    WM.Logger.Debug(callLogTag + ": TCP listener bound to port " + TcpPort);

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

                    State = ServerState.Running;
                    WM.Logger.Debug(callLogTag + ": Server running");
                }
                catch (Exception ex)
                {
                    WM.Logger.Error(callLogTag + ": Exception: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// \pre The Server must be in state 'Running' for this method to succeed.
        /// </summary>
        public void Shutdown()
        {
            var callLogTag = LogID + ".Shutdown()";
            WM.Logger.Debug(callLogTag);

            lock (stateLock)
            {
                switch (State)
                {
                    case ServerState.Running:
                        State = ServerState.ShuttingDown;
                        break;
                    case ServerState.ShuttingDown:
                        throw new Exception("Shutdown() can not be called on Client while it is ShuttingDown.");
                    case ServerState.NotRunning:
                        throw new Exception("Shutdown() can not be called on Client while it is NotRunning.");
                    case ServerState.Initializing:
                        throw new Exception("Shutdown() can not be called on Client while it is Initializing.");
                }
                
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

                State = ServerState.NotRunning;
                WM.Logger.Debug(callLogTag + ": Server not running");
            }
        }

        /// <summary>
        /// Thread function executed by the 'Broadcast' thread.
        /// </summary>
        private void BroadcastFunction()
        {
            var callLogTag = LogID + ".BroadcastFunction()";
            WM.Logger.Debug(callLogTag);

            try
            {
                var broadcastUdpClient = new UdpClient(0);
                var broadcastUdpRemoteEndPoint = new IPEndPoint(IPAddress.Broadcast, Server.UdpBroadcastRemotePort);

                var broadcastMessage = WM.Net.Message.EncodeObjectAsXml(new ServerInfo(((IPEndPoint)this.tcpListener.LocalEndpoint).Address.ToString() ,TcpPort, UdpPort));

                //var logText = string.Format(callLogTag + ": Starting to UDP broadcast Message '{0}' from port {1} to port {2}",
                //                            broadcastMessage,
                //                            broadcastUdpRemoteEndPoint.Port,
                //                            UdpBroadcastRemotePort);

                var logText = string.Format(callLogTag + ": Starting to UDP broadcast ServerInfo from port {0} to port {1}",
                                            broadcastUdpRemoteEndPoint.Port,
                                            UdpBroadcastRemotePort);

                WM.Logger.Debug(
                    logText);

                // Encode data to UTF8-encoding.
                byte[] broadcastMessageData = Encoding.UTF8.GetBytes(broadcastMessage);

                while (State != ServerState.ShuttingDown)
                {   
                        // Send udpBroadcastMessageData to any potential clients.
                        broadcastUdpClient.Send(broadcastMessageData, broadcastMessageData.Length, broadcastUdpRemoteEndPoint);

                        Thread.Sleep(500);
                }

                broadcastUdpClient.Close();
            }
            catch (Exception ex)
            {
                WM.Logger.Error(callLogTag + ": Exception: " + ex.ToString());
            }
        }

        /// <summary>
        /// Thread function executed by the 'Accept Client' thread.
        /// </summary>
        private void AcceptClientFunction()
        {
            var callLogTag = LogID + ".AcceptClientFunction()";
            WM.Logger.Debug(callLogTag);

            while (State != ServerState.ShuttingDown)
            {
                try
                {
                    if (tcpListener.Pending())
                    {
                        WM.Logger.Debug(callLogTag + ": Client connecting...");
                        
                        // Accept the client TCP socket.
                        var tcpClient = tcpListener.AcceptTcpClient();

                        // Create a ClientConnection targeting the new Client.
                        var newClientConnection = new ClientConnection(tcpClient, udpClient);

                        lock (clientConnections)
                        {
                            clientsLockOwner = "AcceptClientFunction";

                            clientConnections.Add(newClientConnection);

                            clientsLockOwner = "None (AcceptClientFunction)";
                        }

                        WM.Logger.Debug(callLogTag + ": Client '" + newClientConnection.ClientID + "' connected.");

                        newClientConnection.SendTCP("Connection Complete");

                        OnClientConnected(newClientConnection);
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
                catch (Exception ex)
                {
                    WM.Logger.Error(callLogTag + ": Exception: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Called when a Client is connected to this Server.
        /// </summary>
        /// <param name="newClientConnection"></param>
        abstract protected void OnClientConnected(ClientConnection newClientConnection);

        /// <summary>
        /// To be implemented by application-specific Server implementations, to process application-specific messages.
        /// </summary>
        /// <param name="messageXML">The message, as received from the ClientCOnnection.  (a string containing the XML-encoded message)</param>
        /// <param name="clientConnection">The ClientConnection from which the meaasga was received.</param>
        /// <param name="obj">The object parsed from the messageXML.</param>
        abstract protected void DoProcessMessage(
            string messageXML,
            ClientConnection clientConnection,
            object obj);

        /// <summary>
        /// Thread function executed by the 'Receive UDP' thread.
        /// </summary>
        private void ReceiveUdpFunction()
        {
            var callLogTag = LogID + ".ReceiveUdpFunction()";
            WM.Logger.Debug(callLogTag);

            while (State != ServerState.ShuttingDown)
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
                                WM.Logger.Debug(callLogTag + ": Received a UDP message from client " + clientIndex);

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
                    WM.Logger.Error(callLogTag + ".ReceiveUdpFunction(): Exception: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Thread function executed by the 'Receive TCP' thread.
        /// </summary>
        private void ReceiveTcpFunction()
        {
            var callLogTag = LogID + ".ReceiveTcpFunction()";
            WM.Logger.Debug(callLogTag);

            while (State != ServerState.ShuttingDown)
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
                    WM.Logger.Error(callLogTag + ": Exception: " + ex.ToString());
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
            var callLogTag = LogID + ".ProcessMessage()";
            WM.Logger.Debug(callLogTag);

            var obj = Message.GetObjectFromMessageXML(messageXML);

            if (obj is DisconnectClientCommand)
            {
                WM.Logger.Debug(string.Format(callLogTag + ": Client[{0}] disconnecting.", WM.Net.NetUtil.ShortID(clientConnection.ClientID)));

                // Client indicates that it is disconnecting.
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
                // Application-specific message. (EG ArchiVR.SetModelLocationCommand)
                // Delegate processing to the application-specific Server implementation.
                DoProcessMessage(messageXML, clientConnection, obj);
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
            var callLogTag = LogID + ".BroadcastCommand()";
            WM.Logger.Debug(callLogTag);

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
                WM.Logger.Error(callLogTag + ": Exception: " + e.Message);
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
            var callLogTag = LogID + ".PropagateCommand()";
            WM.Logger.Debug(callLogTag);

            try
            {
                string data = Message.EncodeObjectAsXml(command);

                PropagateData(data, sourceClientConnection);
            }
            catch (Exception e)
            {
                WM.Logger.Error(callLogTag + ": Exception: " + e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        protected void BroadcastData(
            string data)
        {
            var callLogTag = LogID + ".BroadcastData()";
            WM.Logger.Debug(callLogTag);

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
                WM.Logger.Error(callLogTag + ": Exception: " + e.Message);
            }

            WM.Logger.Debug(callLogTag + ": End");
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
            var callLogTag = LogID + ".PropagateData()";
            WM.Logger.Debug(callLogTag);

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
                WM.Logger.Error(callLogTag + ": Exception: " + e.Message);
            }
        }

        /// <summary>
        /// Send the given command to the Client of the given ClientConnection.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="clientConnection"></param>
        /*private*/
        protected void SendCommand(
            ICommand command,
            ClientConnection clientConnection)
        {
            var callLogTag = LogID + ".SendCommand(" + command.ToString() + ")";
            WM.Logger.Debug(callLogTag);

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
                WM.Logger.Error(callLogTag + ": Exception:" + e.Message);
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
            var callLogTag = LogID + ".SendData()";
            WM.Logger.Debug(callLogTag);

            try
            {
                clientConnection.SendTCP(data);
            }
            catch (Exception e)
            {
                WM.Logger.Error(callLogTag + ": Exception:" + e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="clientIndex"></param>
        public void SendDataToUdp(string data, int clientIndex)
        {
            var callLogTag = LogID + ".SendDataToUdp()";
            WM.Logger.Debug(callLogTag);

            lock (clientConnections)
            {
                if (clientConnections.Count < clientIndex - 1)
                {
                    return;
                }

                var clientConnection = clientConnections[clientIndex];

                if (clientConnection == null)
                {
                    Logger.Error(callLogTag + ": ClientConnection[" + clientIndex + "] is null!");
                }

                try
                {
                    clientConnection.SendUDP(data);                    
                }
                catch (Exception e)
                {
                        WM.Logger.Error(callLogTag + ": Exception:" + e.Message);
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
            var callLogTag = LogID + ".GetLastMessageXML(" + clientIndex + ")";

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

                    clientKey = UDPReceive.GetRemoteEndpointKey(clientConnection.RemoteIP, clientConnection.RemotePortTCP);
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
                WM.Logger.Error(callLogTag + ": Exception: " + e.Message);
                return null;
            }
        }
    }
}
