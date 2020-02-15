using System;
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
    public class ServerInfo : IEquatable<ServerInfo>
    {
        #region Fields

        /// <summary>
        /// The IP address at which the server is running.
        /// </summary>
        public string IP;

        /// <summary>
        /// The TCP port at which the server is running.
        /// </summary>
        public int TcpPort;

        /// <summary>
        /// The UDP port at which the server is running.
        /// </summary>
        public int UdpPort;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public ServerInfo()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="tcpPort"></param>
        /// <param name="udpPort"></param>
        public ServerInfo(String IP, int tcpPort, int udpPort)
        {
            this.IP = IP;
            TcpPort = tcpPort;
            UdpPort = udpPort;
        }

        #endregion Constructors

        #region public API

        override public int GetHashCode()
        {
            return (IP + "-" + TcpPort + "-" + UdpPort).GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverInfo"></param>
        /// <returns></returns>
        override public bool Equals(object o)
        {
            var si = o as ServerInfo;

            if (o == null)
            {
                return false;
            }

            return this.Equals(si);
        }

        /// <summary>
        /// <see cref="IEquatable<ServerInfo>.Equals(ServerInfo)"/> implementation.
        /// </summary>
        /// <param name="serverInfo"></param>
        /// <returns></returns>
        public bool Equals(ServerInfo serverInfo)
        {
            return IP == serverInfo.IP
                && TcpPort == serverInfo.TcpPort
                && UdpPort == serverInfo.UdpPort;
        }

        #endregion public API
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
            /// The log.  Injected during construction.
            /// </summary>
            protected Logger _log;

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

            #region Constructors

            /// <summary>
            /// Parametrized constructor.
            /// </summary>
            /// <param name="tcpClient">The TCP client.</param>
            /// <param name="udpClient">The UDP client.</param>
            /// <param name="log">The log.</param>
            public ClientConnection(
                TcpClient tcpClient,
                UdpClient udpClient,
                WM.Logger log)
            {
                _log = log;
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
                    _log.Debug("ClientConnection: Received ClientInfo");

                    ClientID = clientInfo.ID;
                    RemotePortUDP = clientInfo.UdpPort;

                    udpSend = new UDPSend(udpClient, _log);
                    udpSend.remoteIP = RemoteIP;
                    udpSend.remotePort = RemotePortUDP;
                    udpSend.Init();
                }
                else
                {
                    throw new Exception("ClientConnection: Received non - ClientInfo");
                }
            }

            #endregion Constructors

            #region public API

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
                    _log.Error("ClientConnection.SendUDP(): udpSend is null!");
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
                    _log.Error("ClientConnection.SendTCP(): tcpClient is null!");
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
                    _log.Error("ClientConnection.ReceiveDataTCP(): tcpClient is null!");
                    return false;
                }

                if (tcpNetworkStream == null)
                {
                    _log.Error("ClientConnection.ReceiveDataTCP(): tcpNetworkStream is null!");
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

            #endregion public API
        }

        #region Variables

        /// <summary>
        /// The log.
        /// </summary>
        protected Logger _log;

        private bool _logBroadCasts = false;
        //private bool _logReceivedMessageDataTCP = false;
        //private bool _logReceivedMessageDataUDP = false;

        /// <summary>
        /// Gets the log.
        /// </summary>
        public Logger Log
        {
            set
            {
                _log = value;
            }
            protected get
            {
                return _log;
            }
        }

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
        /// <param name="localClientTcpPort">The TCP port of the local client.</param>
        /// <returns></returns>
        public string GetClientInfo(int localClientTcpPort)
        {
            string info = "";
            lock (this.clientConnections)
            {
                foreach (var clientConnection in clientConnections)
                {
                    info += "- IP: " + clientConnection.RemoteIP + ":" + clientConnection.RemotePortTCP;
                    if (localClientTcpPort == clientConnection.RemotePortTCP)
                    {
                        info += " (local)";
                    }
                    info += "\n";
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

        /// <summary>
        /// The TCP listener, to which clients can connect.
        /// </summary>
        TcpListener tcpListener;

        /// <summary>
        /// The port of the TCP Listener.
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

        /// <summary>
        /// The UDP client.
        /// </summary>
        UdpClient udpClient;

        /// <summary>
        /// The UDP Receiver.
        /// </summary>
        UDPReceive udpReceive;

        /// <summary>
        /// The UDP Receive thread.
        /// </summary>
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
        public String StateText
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

        #region Public API

        /// <summary>
        /// \pre The Server must be in state 'NotRunning' for this method to succeed.
        /// </summary>
        public void Init()
        {
            var callLogTag = LogID + ".Init()";
            _log.Debug(callLogTag);

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
                    _log.Debug(callLogTag + ": UdpClient bound to port " + UdpPort);

                    udpReceive = new UDPReceive(udpClient, Log);
                    udpReceive.Init();
                    _log.Debug(callLogTag + ": UdpReceive initialized");

                    var ipAddress = WM.Net.NetUtil.GetLocalIPAddress();

                    // Create the TCP listener, and bind it to any available port.
                    tcpListener = new TcpListener(ipAddress, 0);
                    tcpListener.Start();
                    _log.Debug(callLogTag + ": TCP listener bound to port " + TcpPort);

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
                    _log.Debug(callLogTag + ": Server running");
                }
                catch (Exception ex)
                {
                    _log.Error(callLogTag + ": Exception: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// \pre The Server must be in state 'Running' for this method to succeed.
        /// </summary>
        public void Shutdown()
        {
            var callLogTag = LogID + ".Shutdown()";
            _log.Debug(callLogTag);

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
                _log.Debug(callLogTag + ": Server not running");
            }
        }

        #endregion Public API

        /// <summary>
        /// Thread function executed by the 'Broadcast' thread.
        /// </summary>
        private void BroadcastFunction()
        {
            var callLogTag = LogID + ".BroadcastFunction()";
            _log.Debug(callLogTag);

            try
            {
                var broadcastUdpClient = new UdpClient(0);

                var broadcastUdpLocalEndPoint = broadcastUdpClient.Client.LocalEndPoint as IPEndPoint;
                var broadcastUdpRemoteEndPoint = new IPEndPoint(IPAddress.Broadcast, Server.UdpBroadcastRemotePort);

                var broadcastMessage = WM.Net.Message.EncodeObjectAsXml(new ServerInfo(((IPEndPoint)this.tcpListener.LocalEndpoint).Address.ToString() ,TcpPort, UdpPort));

                if (_logBroadCasts)
                {
                    var logText = string.Format(callLogTag + ": Starting to UDP broadcast ServerInfo from port {0} to port {1}",
                                            broadcastUdpLocalEndPoint.Port,
                                            broadcastUdpRemoteEndPoint.Port);

                    _log.Debug(logText);
                }

                // Encode data to UTF8-encoding.
                byte[] broadcastMessageData = Encoding.UTF8.GetBytes(broadcastMessage);

                while (State != ServerState.ShuttingDown)
                {
                    if (_logBroadCasts)
                    {
                        var logText = string.Format(callLogTag + ": UDP broadcasting ServerInfo from port {0} to port {1}",
                                            broadcastUdpLocalEndPoint.Port,
                                            broadcastUdpRemoteEndPoint.Port);

                        _log.Debug(logText);
                    }

                    // Send udpBroadcastMessageData to any potential clients.
                    broadcastUdpClient.Send(broadcastMessageData, broadcastMessageData.Length, broadcastUdpRemoteEndPoint);

                        Thread.Sleep(500);
                }

                broadcastUdpClient.Close();
            }
            catch (Exception ex)
            {
                _log.Error(callLogTag + ": Exception: " + ex.ToString());
            }
        }

        /// <summary>
        /// Thread function executed by the 'Accept Client' thread.
        /// </summary>
        private void AcceptClientFunction()
        {
            var callLogTag = LogID + ".AcceptClientFunction()";
            _log.Debug(callLogTag);

            while (State != ServerState.ShuttingDown)
            {
                try
                {
                    if (tcpListener.Pending())
                    {
                        _log.Debug(callLogTag + ": Client connecting...");
                        
                        // Accept the client TCP socket.
                        var tcpClient = tcpListener.AcceptTcpClient();

                        // Create a ClientConnection targeting the new Client.
                        var newClientConnection = new ClientConnection(tcpClient, udpClient, _log);

                        lock (clientConnections)
                        {
                            clientsLockOwner = "AcceptClientFunction";

                            clientConnections.Add(newClientConnection);

                            clientsLockOwner = "None (AcceptClientFunction)";
                        }

                        _log.Debug(callLogTag + ": Client '" + newClientConnection.ClientID + "' connected.");
                        
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
                    _log.Error(callLogTag + ": Exception: " + ex.ToString());
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
            _log.Debug(callLogTag);

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
                                _log.Debug(callLogTag + ": Received a UDP message from client " + clientIndex);

                                for (int broadcastClientIndex = 0; broadcastClientIndex < clientConnections.Count; ++broadcastClientIndex)
                                {
                                    if (clientIndex == broadcastClientIndex)
                                    {
                                        continue; // don't send own client updates back to self...
                                    }

                                    SendDataToUdp(messageXML, broadcastClientIndex);
                                }
                            }
                        }

                        clientsLockOwner = "None (last:ReceiveFromClientsFunction)";
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(callLogTag + ".ReceiveUdpFunction(): Exception: " + ex.ToString());
                }
            }

            _log.Debug(callLogTag + ": end");
        }

        /// <summary>
        /// Thread function executed by the 'Receive TCP' thread.
        /// </summary>
        private void ReceiveTcpFunction()
        {
            var callLogTag = LogID + ".ReceiveTcpFunction()";
            _log.Debug(callLogTag);

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
                    _log.Error(callLogTag + ": Exception: " + ex.ToString());
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
            _log.Debug(callLogTag);

            var obj = Message.GetObjectFromMessageXML(messageXML);

            if (obj is DisconnectClientCommand)
            {
                _log.Debug(string.Format(callLogTag + ": Client[{0}] disconnecting.", WM.Net.NetUtil.ShortID(clientConnection.ClientID)));

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
            _log.Debug(callLogTag);

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
                _log.Error(callLogTag + ": Exception: " + e.Message);
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
            _log.Debug(callLogTag);

            try
            {
                string data = Message.EncodeObjectAsXml(command);

                PropagateData(data, sourceClientConnection);
            }
            catch (Exception e)
            {
                _log.Error(callLogTag + ": Exception: " + e.Message);
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
            _log.Debug(callLogTag);

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
                _log.Error(callLogTag + ": Exception: " + e.Message);
            }

            _log.Debug(callLogTag + ": End");
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
            _log.Debug(callLogTag);

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
                _log.Error(callLogTag + ": Exception: " + e.Message);
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
            _log.Debug(callLogTag);

            try
            {
                /*
                if (clientConnection.tcpNetworkStream == null)
                {
                    _log.Warning("Server.SendCommand(): clientConnection.tcpNetworkStream == null");
                    return;
                }
                */

                var data = Message.EncodeObjectAsXml(command);

                SendData(data, clientConnection);
            }
            catch (Exception e)
            {
                _log.Error(callLogTag + ": Exception:" + e.Message);
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
            var callLogTag = LogID + ".SendData(Client:" + WM.Net.NetUtil.ShortID(clientConnection.ClientID) + ")";
            _log.Debug(callLogTag);

            try
            {
                clientConnection.SendTCP(data);
            }
            catch (Exception e)
            {
                _log.Error(callLogTag + ": Exception:" + e.Message);
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
            _log.Debug(callLogTag);

            lock (clientConnections)
            {
                if (clientConnections.Count < clientIndex - 1)
                {
                    return;
                }

                var clientConnection = clientConnections[clientIndex];

                if (clientConnection == null)
                {
                    _log.Error(callLogTag + ": ClientConnection[" + clientIndex + "] is null!");
                }

                try
                {
                    clientConnection.SendUDP(data);                    
                }
                catch (Exception e)
                {
                    _log.Error(callLogTag + ": Exception:" + e.Message);
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

            //_log.Debug(callLogTag);

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

                    clientKey = UDPReceive.GetRemoteEndpointKey(clientConnection.RemoteIP, clientConnection.RemotePortUDP);
                }

                string messageXML = "";

                lock (udpReceive.allReceivedUDPPackets)
                {
                    if (udpReceive.allReceivedUDPPackets.Count == 0)
                    {
                        return null;
                    }

                    if (!udpReceive.allReceivedUDPPackets.ContainsKey(clientKey))
                    {
                        /*
                        _log.Debug(callLogTag + ": udpReceive.allReceivedUDPPackets does not contain '" + clientKey + "'!");

                        foreach (var key in udpReceive.allReceivedUDPPackets)
                        {
                            _log.Debug("udpReceive.allReceivedUDPPackets key: " + key);
                        }
                        */

                        return null;
                    }

                    if (udpReceive.allReceivedUDPPackets[clientKey] != "")
                    {
                        _log.Debug(callLogTag + ": udpReceive.allReceivedUDPPackets[" + clientKey + "] = '" + udpReceive.allReceivedUDPPackets[clientKey] + "'");
                    }

                    int frameEndTagLength = Message.XmlEndTag.Length;
                    int lastMessageEnd = udpReceive.allReceivedUDPPackets[clientKey].LastIndexOf(Message.XmlEndTag);

                    if (lastMessageEnd < 0)
                    {
                        return null;
                    }

                    _log.Debug(callLogTag + ": found message end!");

                    string temp = udpReceive.allReceivedUDPPackets[clientKey].Substring(0, lastMessageEnd + frameEndTagLength);

                    int lastMessageBegin = temp.LastIndexOf(Message.XmlBeginTag);

                    if (lastMessageBegin < 0)
                    {
                        return null;
                    }

                    _log.Debug(callLogTag + ": found message begin!");

                    // Now get the message XML string.
                    messageXML = temp.Substring(lastMessageBegin, temp.Length - lastMessageBegin);

                    // TODO?:
                    // test that the message can be XML deserialized properly
                    // test that the object contained in the message can be deserialized properly
                    // -> If not, continue search for older message...

                    // Clear old messages from receivebuffer.
                    udpReceive.allReceivedUDPPackets[clientKey] = udpReceive.allReceivedUDPPackets[clientKey].Substring(lastMessageEnd + frameEndTagLength);
                }

                return messageXML;
            }
            catch (Exception e)
            {
                _log.Error(callLogTag + ": Exception: " + e.Message);
                return null;
            }
        }
    }
}
