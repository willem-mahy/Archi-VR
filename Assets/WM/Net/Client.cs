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
    /// Used to send Client information to the Server over TCP upon connection initialization.
    /// </summary>
    [Serializable]
    public class ClientInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid ID;

        /// <summary>
        /// 
        /// </summary>
        public int UdpPort;

        public ClientInfo(Guid id, int udpPort)
        {
            this.ID = id;
            UdpPort = udpPort;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    abstract public class Client : MonoBehaviour
    {
        #region Variables

        /// <summary>
        /// The client ID.
        /// </summary>
        public Guid ID
        {
            get;
        } = Guid.NewGuid();

        public ServerInfo ServerInfo = null; // TODO? new ServerInfo(127.0.0.1, Server.DefaultTcpPort, Server.DefaultUdpPort);

        public int ConnectTimeout = 100;

        #region TCP

        public int TcpPort
        {
            get { return ((IPEndPoint)tcpClient.Client.LocalEndPoint).Port; }
        }

        public int UdpPort
        {
            get { return ((IPEndPoint)udpClient.Client.LocalEndPoint).Port; }
        }

        // The TCP client
        private TcpClient tcpClient;

        // The network stream to the server over TCP.
        private NetworkStream tcpServerStream;

        #endregion

        #region UDP

        /// <summary>
        /// The broadcast message to be broadcasted by the server.
        /// To be implemented by concrete server types.
        /// </summary>
        public string UdpBroadcastMessage
        {
            get;
            protected set;
        }

        private UdpClient udpClient;

        private UDPSend udpSend;

        private UDPReceive udpReceive;

        #endregion

        // The client's worker thread.
        private Thread thread;

        #region Internal state

        public enum ClientState
        {
            Disconnected,
            Connecting,
            Connected,
            Disconnecting,
        }

        private System.Object stateLock = new System.Object();
        public ClientState state
        {
            get;
            private set;
        } = ClientState.Disconnected;

        /// <summary>
        /// 
        /// </summary>
        public string Status = "Not initialized";

        /// <summary>
        /// Whether we are shutting down(true) or not(false).
        /// </summary>
        private bool shutDown = false;

        #endregion

        #endregion

        /// <summary>
        /// Starts initializing the connection to the Server.
        /// </summary>
        public void Connect()
        {
            WM.Logger.Debug("Client.Connect()");

            lock (stateLock)
            {
                switch (state)
                {
                    case ClientState.Disconnected:
                        state = ClientState.Connecting;
                        break;
                    case ClientState.Connecting:
                        throw new Exception("Connect() can not be called on Client while it is Connnecting.");
                    case ClientState.Connected:
                        throw new Exception("Connect() can not be called on CLient while it is Connected.");
                    case ClientState.Disconnecting:
                        throw new Exception("Connect() can not be called on CLient while it is Disconnecting.");
                }
            }

            shutDown = false;

            thread = new Thread(new ThreadStart(ThreadFunction));
            thread.IsBackground = true;
            thread.Start();
        }


        /// <summary>
        /// 
        /// </summary>
        abstract public void OnConnect();


        /// <summary>
        /// 
        /// </summary>
        abstract public void OnDisconnect();

        /// <summary>
        /// Disconnect the client.
        /// </summary>
        public void Disconnect()
        {
            WM.Logger.Debug("Client.Disconnect()");

            lock (stateLock)
            {
                switch (state)
                {
                    case ClientState.Connected:
                        state = ClientState.Disconnecting;
                        break;
                    case ClientState.Disconnecting:
                        throw new Exception("Disconnect() can not be called on Client while it is Disconnecting.");
                    case ClientState.Disconnected:
                        throw new Exception("Disconnect() can not be called on Client while it is Disconnnected.");
                    case ClientState.Connecting:
                        throw new Exception("Disconnect() can not be called on Client while it is Connecting.");
                }

                WM.Logger.Debug("Client disconnecting...");

                OnDisconnect();

                SendCommand(new DisconnectClientCommand(ID));

                while (Status != "DisconnectAcknoledged") ;

                WM.Logger.Debug("Client.Disconnect(): DisconnectAcknoledged received: Shutting down...");
                Shutdown();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void Shutdown()
        {
            WM.Logger.Debug("Client.Shutdown(): Start");
            shutDown = true;

            if (thread != null)
            {
                thread.Join();

                thread = null;
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

        /// <summary>
        /// Synchronously tries to discover any Server to connect to.
        /// 1) Starts listening at port 'Server.UdpBroadcastRemotePort' for discovery messages from a Server.
        /// 2) Tries to parse any incoming data on the port as a 'ServerInfo' message.
        /// 3) Returns the first received ServerInfo, or 'null' if Client shutdown was initiated before receiving a ServerInfo on the port.
        /// </summary>
        /// <returns>The first received ServerInfo.</returns>
        private ServerInfo GetServerInfoFromUdpBroadcast()
        {
            Status = "Listening for servers";

            WM.Logger.Debug(string.Format("Client: Listening to UDP broadcast Message '{0}' on port {1}", UdpBroadcastMessage, Server.UdpBroadcastRemotePort));

            var discoveryUdpClient = new UdpClient(Server.UdpBroadcastRemotePort);

            // The address from which to receive data.
            // In this case we are interested in data from any IP and any port.
            var discoveryUdpRemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (!shutDown)
            {
                try
                {
                    // Receive bytes from anyone on local port 'Server.UdpBroadcastRemotePort'.
                    byte[] data = discoveryUdpClient.Receive(ref discoveryUdpRemoteEndPoint);

                    // Encode received bytes to UTF8- encoding.
                    string receivedText = Encoding.UTF8.GetString(data);

                    var obj = Message.GetObjectFromMessageXML(receivedText);
                    
                    if (obj is ServerInfo serverInfo)
                    {
                        var serverIP = discoveryUdpRemoteEndPoint.Address.ToString();
                        WM.Logger.Debug(string.Format("Client: Received UDP broadcast Message from server '{0}': TCP {1}, UDP {2}", serverIP, serverInfo.TcpPort, serverInfo.UdpPort));

                        discoveryUdpClient.Close();

                        return serverInfo;
                    }
                    else
                    {
                        WM.Logger.Warning(string.Format("Client: Received unexpected message '{0}' on server broadcast listener UDP client!", receivedText));
                    }
                }
                catch (Exception e)
                {
                    WM.Logger.Debug("UDPReceive.ReceiveData(): Exception: " + e.ToString());
                }
            }

            discoveryUdpClient.Close();

            return null;
        }

        /// <summary>
        /// Thread function executed by the client's worker thread.
        /// </summary>
        private void ThreadFunction()
        {
            Debug.Assert(state == ClientState.Connecting);

            if (ServerInfo == null)
            {
                ServerInfo = GetServerInfoFromUdpBroadcast();
            }

            while (!shutDown)
            {
                lock (stateLock)
                {
                    if (TryConnect())
                    {
                        Status = "Connected to " + ServerIP;
                        
                        // Get server stream from TCP client.
                        tcpServerStream = tcpClient.GetStream();

                        // Create UDP client.
                        // Pass '0' to make the system pick an appropriate port for us.
                        udpClient = new UdpClient(0);

                        // Send the ClientInfo to the server.
                        var udpReceivePort = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;                        
                        var clientInfo = new ClientInfo(ID, udpReceivePort);
                        var clientInfoMessage = WM.Net.Message.EncodeObjectAsXml(clientInfo);

                        WM.Logger.Debug("Client:TrheadFunction: Send ClientInfo");
                        SendData(clientInfoMessage);

                        // Initialize UDP socket from server.
                        {   
                            udpReceive = new UDPReceive(udpClient);
                            udpReceive.Init();
                        }

                        // Initialize UDP socket to server.
                        {
                            udpSend = new UDPSend(udpClient);
                            udpSend.remoteIP = ServerInfo.IP;
                            udpSend.remotePort = ServerInfo.UdpPort;
                            udpSend.Init();
                        }

                        WM.Logger.Debug("Client:ThreadFunction: Waiting for 'Connection Complete' from server");
                        while (!tcpServerStream.DataAvailable)
                        {
                        }

                        // Receive data from server.
                        var bytesFromServer = new byte[tcpClient.ReceiveBufferSize];
                        var numBytesRead = tcpServerStream.Read(bytesFromServer, 0, (int)tcpClient.ReceiveBufferSize);

                        var textFromServer = Encoding.ASCII.GetString(bytesFromServer, 0, numBytesRead);

                        if (textFromServer != "Connection Complete")
                        {
                            WM.Logger.Error("Client:ThreadFunction: Received '" + textFromServer + "' instead of 'Connection Complete'");
                        }
                        else
                        {
                            WM.Logger.Debug("Client:ThreadFunction: Received 'Connection Complete' from Server");
                        }

                        OnConnect();

                        state = ClientState.Connected;
                        break; // We are Connected: stop connecting...
                    }                    
                }
            }

            /// ... and start communicating with server...
            while (!shutDown) // ... until shutdown has been initiated.
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
                    int EndTagLength = Message.XmlEndTag.Length;

                    while (true)
                    {
                        int firstMessageBegin = dataFromServer.IndexOf(Message.XmlBeginTag);

                        if (firstMessageBegin < 0)
                        {
                            break;
                        }

                        // Remove all data in front of first message.
                        dataFromServer = dataFromServer.Substring(firstMessageBegin);

                        int firstMessageEnd = dataFromServer.IndexOf(Message.XmlEndTag);

                        if (firstMessageEnd < 0)
                        {
                            break;
                        }

                        // Get the string with the XML-encoded message in it.
                        int messageLength = firstMessageEnd + EndTagLength;
                        string messageXML = dataFromServer.Substring(0, messageLength);

                        // Clear all up to the last processed message from the receive buffer.
                        int c = dataFromServer.Length;
                        var remainder = dataFromServer.Substring(firstMessageEnd + EndTagLength);
                        dataFromServer = remainder;

                        ProcessMessage(messageXML);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool TryConnect()
        {
            if (ServerInfo != null)
            {
                // connect to a predefined server.
                return TryConnectToServer(ServerInfo);
            }
            else
            {
                // Connect to any server.
                var localSubNet = NetUtil.GetLocalIPSubNet();

                ServerInfo = new ServerInfo("", 8880,8881); //TODO: Server.DefaultIP, Server.DefaultTcpPort, Server.

                // Iterate all addresses in the local subnet, and try to connect to each address at default port.
                for (int i = 0; i < 255; ++i)
                {
                    ServerInfo.IP = localSubNet + i; // TODO? skip own IP

                    if (TryConnectToServer(ServerInfo))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Try to connect to the server at given IP.
        /// </summary>
        /// <param name="serverIP"></param>
        /// <returns></returns>
        private bool TryConnectToServer(ServerInfo serverInfo)
        {
            String tag = "Server(IP:" + serverInfo.IP + ", TCP:" + serverInfo.TcpPort + ", UDP:" + serverInfo.UdpPort + "), timeout:" + ConnectTimeout + "ms";
            WM.Logger.Debug("Client.TryConnectToServer(): " + tag);

            Status = "Trying to connect to " + tag;

            try
            {
                var tcpClient = new TcpClient();

                var connectionAttempt = tcpClient.ConnectAsync(serverInfo.IP, serverInfo.TcpPort);

                connectionAttempt.Wait(ConnectTimeout);

                if (connectionAttempt.IsCompleted)
                {
                    this.tcpClient = tcpClient;
                    WM.Logger.Debug("Client.TryConnectToServer(): TcpClient connected!");
                    return true;
                }
                else
                {
                    tcpClient.Close();
                    WM.Logger.Debug("Client.TryConnectToServer(): Failed to connect!");
                    return false;
                }
            }
            catch (Exception e)
            {
                var txt = "Client.TryConnectToServer(): Exception: " + e.Message + ": " + e.InnerException;
                Status = txt;
                WM.Logger.Error(txt);
                return false;
            }
        }

        /// <summary>
        /// Whether this client is connected to a server.
        /// </summary>
        public bool Connected
        {
            get
            {
                return (tcpClient != null) && tcpClient.Connected;
            }
        }

        /// <summary>
        /// Returns a string containing the IP address of the server to which this client is connected, or 'Not available' if not connected.
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageXML"></param>
        private void ProcessMessage(
            string messageXML)
        {
            var obj = Message.GetObjectFromMessageXML(messageXML);

            // If it is a generic message, process it here.
            if (obj is ClientDisconnectAcknoledgeMessage)
            {
                Status = "DisconnectAcknoledged";
                return;
            }

            // It is a not a generic message, delegate processing to application-specific logic.
            DoProcessMessage(obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        abstract public void DoProcessMessage(object obj);
                        
        /// <summary>
        /// Send non-critical data to the server over UDP.
        /// </summary>
        /// <param name="data"></param>
        public void SendDataUdp(String data)
        {
            if (udpSend == null)
            {
                return; // Not connected yet...
            }

            try
            {
                udpSend.SendString(data);
            }
            catch (Exception e)
            {
                WM.Logger.Error("Client.SendDataUdp(): Exception:" + e.Message);
            }
        }

        /// <summary>
        /// Send the given object as non-critical (UDP) message to the server.
        /// </summary>
        /// <param name="obj">The object to be sent.</param>
        public void SendMessageUdp(object obj)
        {
            if (udpSend == null)
            {
                return; // Not connected yet...
            }

            try
            {
                udpSend.SendString(Message.EncodeObjectAsXml(obj));
            }
            catch (Exception e)
            {
                WM.Logger.Error("Client.SendMessageUdp(object obj): Exception:" + e.Message);
            }
        }

        /// <summary>
        /// Send the given command to the server over TCP.
        /// </summary>
        /// <param name="command"></param>
        public void SendCommand(ICommand command)
        {
            WM.Logger.Debug("Client:SendCommand(" + command.ToString() + ")");

            try
            {
                var data = Message.EncodeObjectAsXml(command);

                SendData(data);
            }
            catch (Exception e)
            {
                 WM.Logger.Error("Client.SendCommand(): Exception:" + e.Message);
            }
        }

        /// <summary>
        /// Send the given string to the server over TCP.
        /// </summary>
        /// <param name="data"></param>
        public void SendData(String data)
        {
            WM.Logger.Debug("Client:SendData(" + data + ")");

            try
            {
                if (tcpClient == null)
                {
                    return;
                }

                if (tcpServerStream == null)
                {
                    return;
                }

                var bytes = Encoding.ASCII.GetBytes(data);
                tcpServerStream.Write(bytes, 0, bytes.Length);
                tcpServerStream.Flush();
            }
            catch (Exception e)
            {
                 WM.Logger.Error("Client.SendData(): Exception:" + e.Message);
            }
        }

        /// <summary>
        /// Get all objects received from the server over UDP.
        /// </summary>
        public List<object> GetReceivedMessagesUdp()
        {
            if (udpReceive == null)
            {
                return null; // Not connected yet...
            }

            try
            {
                var receivedMessages = new List<object>();

                lock (udpReceive.allReceivedUDPPackets)
                {
                    if (udpReceive.allReceivedUDPPackets.Keys.Count > 1)
                    {
                        WM.Logger.Warning("Client.GetReceivedMessagesUdp(): More than one receive buffer!?!");
                        udpReceive.allReceivedUDPPackets.Clear();
                        return null;
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
                            string messageEndTag = "</Message>";
                            int messageEndTagLength = messageEndTag.Length;

                            int messageBegin = receiveBuffer.IndexOf("<Message ");

                            if (messageBegin < 0)
                            {
                                break; // We have no full avatar states to read left in the receivebuffer -> break parsing received avatar states.
                            }

                            // Get position of first frame begin tag in receive buffer.
                            if (messageBegin > 0)
                            {
                                // Clear old data (older than first frame) from receivebuffer.
                                receiveBuffer = receiveBuffer.Substring(messageBegin);
                                messageBegin = 0;
                            }

                            // Get position of first frame end tag in receive buffer.
                            int messageEnd = receiveBuffer.IndexOf(messageEndTag);

                            if (messageEnd < 0)
                            {
                                break; // We have no full messages to read left in the receivebuffer -> break parsing received messagess.
                            }

                            // Now get the message XML string.
                            string messageXML = receiveBuffer.Substring(0, messageEnd + messageEndTagLength);

                            // Remove message XML from receivebuffer.
                            receiveBuffer = receiveBuffer.Substring(messageEnd + messageEndTagLength);

                            {
                                var ser = new XmlSerializer(typeof(Message));

                                var reader = new StringReader(messageXML);

                                var message = (Message)(ser.Deserialize(reader));

                                reader.Close();

                                receivedMessages.Add(message.Deserialize());
                            }
                        }

                        // We have processed all available fully received messages from the receive buffer.
                        // Update the receive buffer to the unprocessed remainder.
                        udpReceive.allReceivedUDPPackets[senderIP] = receiveBuffer;
                    }
                }

                return receivedMessages;
            }
            catch (Exception e)
            {
                WM.Logger.Error("Client.GetReceivedMessagesUdp(): Exception:" + e.Message);
                return null;
            }
        }
    }
}
