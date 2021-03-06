﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;
using WM.Application;
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
        /// The log.
        /// </summary>
        protected Logger _log;

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
        /// The unique client ID.
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
            get { return "Client[" + WM.Net.NetUtil.ShortID(ID) + "]"; }
        }

        /// <summary>
        /// The ServerInfo describing the designated server.
        /// </summary>
        public ServerInfo ServerInfo = null; // TODO? new ServerInfo(127.0.0.1, Server.DefaultTcpPort, Server.DefaultUdpPort);

        /// <summary>
        /// The timeout of a connection attempt, in millis.
        /// </summary>
        public int ConnectTimeout = 100;

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

        #region TCP

        /// <summary>
        /// The TCP port.
        /// </summary>
        public int TcpPort
        {
            get { return ((IPEndPoint)tcpClient.Client.LocalEndPoint).Port; }
        }

        /// <summary>
        /// The TCP client.
        /// </summary>
        private TcpClient tcpClient;

        /// <summary>
        /// The network stream to the server over TCP.
        /// </summary>
        private NetworkStream tcpServerStream;

        string dataFromServer = "";

        #endregion

        #region UDP

        /// <summary>
        /// The UDP port.
        /// </summary>
        public int UdpPort
        {
            get { return ((IPEndPoint)udpClient.Client.LocalEndPoint).Port; }
        }

        /// <summary>
        /// 
        /// </summary>
        private UdpClient udpClient;

        /// <summary>
        /// 
        /// </summary>
        private UDPSend udpSend;

        /// <summary>
        /// 
        /// </summary>
        private UDPReceive udpReceive;

        #endregion

        /// <summary>
        /// The client's worker thread.
        /// </summary>
        private Thread thread;

        #region Internal state

        /// <summary>
        /// The possible Client states.
        /// </summary>
        public enum ClientState
        {
            Disconnected,
            Connecting,
            Connected,
            Disconnecting,
        }

        /// <summary>
        /// Locking object for the Client state.
        /// </summary>
        private System.Object stateLock = new System.Object();

        /// <summary>
        /// The Client state.
        /// </summary>
        public ClientState State
        {
            get;
            private set;
        } = ClientState.Disconnected;

        /// <summary>
        /// 
        /// </summary>
        public string StateText
        {
            get
            {
                switch (State)
                {
                    case ClientState.Disconnected:
                        return "Disconnected";
                    case ClientState.Connecting:
                        return "Connecting";
                    case ClientState.Connected:
                        return "Connected";
                    case ClientState.Disconnecting:
                        return "Disconnecting";
                    default:
                        return "Unknown client state '" + State.ToString() + "'";
                }
            }
        }

        /// <summary>
        /// The current action.
        /// </summary>
        public string Action
        {
            get;
            private set;
        } = "";

        #endregion Internal state

        #endregion Variables

        #region Public API

        /// <summary>
        /// Starts initializing the connection to the Server.
        /// 
        /// \pre The Client must be in state 'Disconnected' for this method to succeed.
        /// </summary>
        public void Connect()
        {
            _log.Debug(LogID + ".Connect()");

            lock (stateLock)
            {
                switch (State)
                {
                    case ClientState.Disconnected:
                        State = ClientState.Connecting;
                        break;
                    case ClientState.Connecting:
                        throw new Exception("Connect() can not be called on Client while it is Connecting.");
                    case ClientState.Connected:
                        throw new Exception("Connect() can not be called on Client while it is Connected.");
                    case ClientState.Disconnecting:
                        throw new Exception("Connect() can not be called on Client while it is Disconnecting.");
                }
            }

            thread = new Thread(new ThreadStart(ThreadFunction));
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// Disconnect the client.
        /// 
        /// \pre The Client must be in state 'Connected' for this method to succeed.
        /// </summary>
        public void Disconnect()
        {
            _log.Debug(LogID + ".Disconnect()");

            lock (stateLock)
            {
                switch (State)
                {
                    case ClientState.Connected:
                        State = ClientState.Disconnecting;
                        break;
                    case ClientState.Disconnecting:
                        throw new Exception("Disconnect() can not be called on Client while it is Disconnecting.");
                    case ClientState.Disconnected:
                        throw new Exception("Disconnect() can not be called on Client while it is Disconnnected.");
                    case ClientState.Connecting:
                        throw new Exception("Disconnect() can not be called on Client while it is Connecting.");
                }

                _log.Debug(LogID + "] disconnecting...");

                Shutdown();

                OnDisconnect();

                State = ClientState.Disconnected;
            }
        }
        
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
                _log.Error("Client.SendDataUdp(): Exception:" + e.Message);
            }
        }

        /// <summary>
        /// Send the given object as non-critical (UDP) message to the server.
        /// </summary>
        /// <param name="obj">The object to be sent.</param>
        public void SendMessageUdp(object obj)
        {
            var logCallTag = LogID + ".SendMessageUdp()";

            if (_log.enableLogUDP)
            {
                _log.Debug(logCallTag);
            }

            if (udpSend == null)
            {
                _log.Warning(logCallTag + ": Not connected yet!");
                return; // Not connected yet...
            }

            try
            {
                udpSend.SendString(Message.EncodeObjectAsXml(obj));
            }
            catch (Exception e)
            {
                _log.Error(logCallTag + ": Exception:" + e.Message);
            }
        }

        /// <summary>
        /// Send the given command to the server over TCP.
        /// </summary>
        /// <param name="command"></param>
        public void SendCommand(ICommand command)
        {
            _log.Debug(LogID + ":SendCommand(" + command.ToString() + ")");

            try
            {
                var data = Message.EncodeObjectAsXml(command);

                SendData(data);
            }
            catch (Exception e)
            {
                _log.Error(LogID + ".SendCommand(): Exception:" + e.Message);
            }
        }

        /// <summary>
        /// Send the given string to the server over TCP.
        /// </summary>
        /// <param name="data"></param>
        public void SendData(String data)
        {
            //_log.Debug(LogID + ":SendData(" + data + ")");
            _log.Debug(LogID + ":SendData()");

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
                _log.Error(LogID + ".SendData(): Exception:" + e.Message);
            }
        }

        /// <summary>
        /// Get all objects received from the server over UDP.
        /// </summary>
        public List<object> GetReceivedMessagesUdp()
        {
            var logCallTag = LogID + ".GetReceivedMessagesUdp()";

            if (_log.enableLogUDP)
            {
                _log.Debug(logCallTag);
            }

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
                        _log.Warning(logCallTag + ": More than one receive buffer!?!");
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
                _log.Error(logCallTag + ": Exception:" + e.Message);
                return null;
            }
        }

        #endregion Public API

        #region Non-public API

        /// <summary>
        /// Callback fired upon finilizing connection.
        /// </summary>
        abstract protected void OnConnect();

        /// <summary>
        /// Callback fired upon finilizing disconnection.
        /// </summary>
        abstract protected void OnDisconnect();

        /// <summary>
        /// Perform internal household chores part of shutting down the client.
        /// </summary>
        private void Shutdown()
        {
            _log.Debug(LogID + ".Shutdown()");

            // First make sure the receiving thread is stopped.
            if (thread != null)
            {
                thread.Join();

                thread = null;
            }

            InformServerAboutDisconnection();

            // Dispose of the UDP receive component.
            if (udpReceive != null)
            {
                udpReceive.Shutdown();

                udpReceive = null;
            }

            // Dispose of the UDP send component.
            if (udpSend != null)
            {
                udpSend = null;
            }

            // Close the UDP connection to the Server.
            if (udpClient != null)
            {
                udpClient.Close();

                udpClient = null;
            }

            // Close the TCP connection to the Server.
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
        /// Tries (and retries for a limited number of times) to notify the server and get a DisconnectAcknoledged back.
        /// </summary>
        private void InformServerAboutDisconnection(
            int pollClientDisconnectAcknoledgeMessageNumRetries = 3,
            int pollClientDisconnectAcknoledgeMessageInterval = 200)
        {
            _log.Debug(LogID + ".InformServerAboutDisconnection()");

            try
            {
                SendCommand(new DisconnectClientCommand(ID));
            }
            catch (Exception /*e*/)
            {
                _log.Debug(LogID + ".InformServerAboutDisconnection(): Sending DisconnectClientCommand failed.");
                return;
            }

            // Then wait for the server to respond with a ClientDisconnectAcknoledgeMessage.
            // From then on we can safely tear down all connections (UDP, TCP) to the Server, because it will not longer be using them.
            for (int i = 0; i < pollClientDisconnectAcknoledgeMessageNumRetries; ++i)
            {
                var messages = ReceiveTcpMessagesFromServer();

                foreach (var messageXML in messages)
                {
                    var obj = Message.GetObjectFromMessageXML(messageXML);

                    if (obj is ClientDisconnectAcknoledgeMessage)
                    {
                        _log.Debug(LogID + ".InformServerAboutDisconnection(): DisconnectAcknoledged from Server received after " + i + " polls.");
                        return;
                    }
                }

                Thread.Sleep(pollClientDisconnectAcknoledgeMessageInterval);
            }
        }

        /// <summary>
        /// Synchronously tries to discover any Server to connect to.
        /// 1) Starts listening on UDP port 'Server.UdpBroadcastRemotePort' for discovery messages from any broadcasting Server(s).
        /// 2) Tries to parse any incoming data on the port as a 'ServerInfo' message.
        /// 3) Returns the first received ServerInfo, or 'null' if Client shutdown was initiated before receiving a ServerInfo on the port.
        /// </summary>
        /// <returns>The first received ServerInfo.</returns>
        private ServerInfo GetServerInfoFromUdpBroadcast()
        {
            var callLogID = LogID + ":GetServerInfoFromUdpBroadcast()";
            
            Action = "Listening for servers";

            _log.Debug(string.Format(callLogID + ": Listening on UDP port {0}.", Server.UdpBroadcastRemotePort));

            var discoveryUdpClient = new UdpClient(Server.UdpBroadcastRemotePort);

            // The address from which to receive data.
            // In this case we are interested in data from any IP and any port.
            var discoveryUdpRemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (State != ClientState.Disconnecting)
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
                        _log.Debug(string.Format(callLogID + ": Received ServerInfo('IP:{0}': TCP:{1}, UDP:{2})", serverIP, serverInfo.TcpPort, serverInfo.UdpPort));

                        discoveryUdpClient.Close();

                        return serverInfo;
                    }
                    else
                    {
                        _log.Warning(string.Format(callLogID + ": Received unexpected message '{0}' on server broadcast listener UDP client!", receivedText));
                    }
                }
                catch (Exception e)
                {
                    _log.Warning(callLogID + ": Exception: " + e.ToString());
                }
            }

            discoveryUdpClient.Close();

            return null;
        }

        /// <summary>
        /// Get critical messages received from the Server over TCP.
        /// The received messages (and any potential data fragments in front of/between them) are flushed from the icoming data buffer.
        /// </summary>
        /// <param name="maxNumberOfMessagesToReceive">Optional.  If specified, not all, but only up to the given number of messages are returned.</param>
        /// <returns>A list containing the XML-encoded received messages.</returns>        
        private List<String> ReceiveTcpMessagesFromServer(int maxNumberOfMessagesToReceive = int.MaxValue)
        {
            var receivedMessages = new List<String>();

            // First receive all available data from the TCP connection into the data buffer.
            if (tcpServerStream != null)
            {
                lock (tcpServerStream)
                {
                    while (tcpServerStream.DataAvailable)
                    {
                        var bytesFromServer = new byte[tcpClient.ReceiveBufferSize];
                        var numBytesRead = tcpServerStream.Read(bytesFromServer, 0, (int)tcpClient.ReceiveBufferSize);

                        dataFromServer += Encoding.ASCII.GetString(bytesFromServer, 0, numBytesRead);
                    }
                }
            }

            // Then extract all messages from data buffer.
            if (dataFromServer.Length == 0)
            {
                return receivedMessages; // There is no data to extract messages from.
            }

            int EndTagLength = Message.XmlEndTag.Length;

            while (true)
            {
                // Get the position of the first 'Message Begin' tag in the receive data buffer.
                int firstMessageBegin = dataFromServer.IndexOf(Message.XmlBeginTag);

                if (firstMessageBegin < 0)
                {
                    // Although no 'Message Begin' tag was found, do NOT clear the receive data buffer at this point:
                    // The receive data buffer might be ending on a partial 'Message Begin' tag!
                    return receivedMessages;
                }

                // Remove all data in front of first 'Message Begin' tag in the receive data buffer
                // (since it's unparseable, and thus useless).
                if (firstMessageBegin > 0)
                {
                    // Should this even happen in a normal use case? -> Let's log it to find out...
                    _log.Warning(LogID + ".ReceiveTcpMessagesFromServer: Removing useless data buffer begin '" + dataFromServer.Substring(0, firstMessageBegin) + "'");

                    dataFromServer = dataFromServer.Substring(firstMessageBegin);
                }

                // Get the position of the first 'Message End' tag in the data buffer.
                int firstMessageEnd = dataFromServer.IndexOf(Message.XmlEndTag);

                if (firstMessageEnd < 0)
                {
                    // Although no 'Message Begin' tag was found, do NOT clear the receive data buffer at this point:
                    // The data buffer is probably containing a non-finished message, for which the rest is under way!
                    return receivedMessages;
                }

                // We have a complete Message in the front of the receive data buffer now!
                Debug.Assert(firstMessageBegin < firstMessageEnd);

                // Extract the XML-encoded message string from the data buffer, and add it to the output messages list.
                int messageLength = firstMessageEnd + EndTagLength;
                string messageXML = dataFromServer.Substring(0, messageLength);
                receivedMessages.Add(messageXML);

                // Clear all up to the last extracted message from the receive data buffer.
                int c = dataFromServer.Length;
                var remainder = dataFromServer.Substring(firstMessageEnd + EndTagLength);
                dataFromServer = remainder;

                if (receivedMessages.Count == maxNumberOfMessagesToReceive)
                {
                    return receivedMessages;
                }
            }
        }

        /// <summary>
        /// Thread function executed by the client's worker thread.
        /// </summary>
        private void ThreadFunction()
        {
            var callLogTag = LogID + ":ThreadFunction()";

            Debug.Assert(State == ClientState.Connecting);

            if ((ServerInfo == null) || (ServerInfo.IP == "")) // TODO: why is ServerInfo default-initialized when running ArchiVR from Unity Editor???
            {
                ServerInfo = GetServerInfoFromUdpBroadcast();
            }

            dataFromServer = "";

            while (State != ClientState.Disconnecting)
            {
                lock (stateLock)
                {
                    if (TryConnect())
                    {
                        // Get server stream from TCP client.
                        tcpServerStream = tcpClient.GetStream();

                        // Create UDP client.
                        // Pass '0' to make the system pick an appropriate port for us.
                        udpClient = new UdpClient(0);

                        // Send the ClientInfo to the server.
                        var udpReceivePort = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                        var clientInfo = new ClientInfo(ID, udpReceivePort);
                        var clientInfoMessage = WM.Net.Message.EncodeObjectAsXml(clientInfo);

                        _log.Debug(callLogTag + ": Send ClientInfo");
                        SendData(clientInfoMessage);

                        // Initialize UDP socket from server.
                        {
                            udpReceive = new UDPReceive(udpClient, Log);
                            udpReceive.Init();
                        }

                        // Initialize UDP socket to server.
                        {
                            udpSend = new UDPSend(udpClient, _log);
                            udpSend.remoteIP = ServerInfo.IP;
                            udpSend.remotePort = ServerInfo.UdpPort;
                            udpSend.Init();
                        }

                        break; // We successfully established our private TCP connection to the server...
                    }
                }
            }

            // Now wait for the server to notify us that he finished up...
            _log.Debug(callLogTag + ": Waiting for 'Connection Complete' from server");
            while (State != ClientState.Disconnecting)
            {
                lock (stateLock)
                {
                    // Wait for ConnectionCompleteMessage from Server.
                    var messageFromServer = ReceiveTcpMessagesFromServer(1);

                    switch (messageFromServer.Count)
                    {
                        case 0:
                            continue;
                        case 1:
                            {
                                var messageXML = messageFromServer[0];
                                var message = Message.GetObjectFromMessageXML(messageXML);

                                if (message is string messageString)
                                {
                                    if (messageString == Server.ConnectionCompleteMessage)
                                    {
                                        _log.Debug(string.Format("{0}: Received '{1}' from server.", callLogTag, Server.ConnectionCompleteMessage));

                                        OnConnect();

                                        State = ClientState.Connected;
                                        break;
                                    }
                                    else
                                    {
                                        string warning = string.Format("{0}: Received unexpected string '{1}' from server, expecting '{2}'", callLogTag, messageString, Server.ConnectionCompleteMessage);
                                        _log.Warning(warning);
                                    }
                                }
                                else
                                {
                                    string warning = string.Format("{0}: Received non-string '{1}' from server, expecting '{2}'", callLogTag, messageXML, Server.ConnectionCompleteMessage);
                                    _log.Warning(warning);
                                }
                            }
                            continue;
                        default:
                            string error = string.Format("{0}: ReceiveTcpMessagesFromServer(1) returned more than {1} message!", callLogTag, messageFromServer.Count);
                            _log.Debug(error);
                            continue;
                    }

                    if (State == ClientState.Connected)
                    {
                        break; // We are Connected: stop connecting...
                    }
                }
            }

            /// ... and start communicating with server...

            while (State != ClientState.Disconnecting) // ... until shutdown has been initiated.
            {
                var messagesFromServer = ReceiveTcpMessagesFromServer();

                foreach (var messageXML in messagesFromServer)
                {
                    ProcessMessage(messageXML);
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

                ServerInfo = new ServerInfo("", 8880, 8881); //TODO: Server.DefaultIP, Server.DefaultTcpPort, Server.

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
        /// Try to connect to the server at given IP, or time out.
        /// </summary>
        /// <param name="serverIP"></param>
        /// <returns>Whether the connection was successfull(true), or timed out(false).</returns>
        private bool TryConnectToServer(ServerInfo serverInfo)
        {
            var callLogTag = LogID + ".TryConnectToServer()";

            String serverInfoTag = "ServerInfo=(IP:" + serverInfo.IP + ", TCP:" + serverInfo.TcpPort + ", UDP:" + serverInfo.UdpPort + "), timeout:" + ConnectTimeout + "ms";
            _log.Debug(callLogTag + ": " + serverInfoTag);

            Action = "Trying to connect to " + serverInfoTag;

            try
            {
                var tcpClient = new TcpClient();

                var connectionAttempt = tcpClient.ConnectAsync(serverInfo.IP, serverInfo.TcpPort);

                connectionAttempt.Wait(ConnectTimeout);

                if (connectionAttempt.IsCompleted)
                {
                    this.tcpClient = tcpClient;
                    _log.Debug(callLogTag + ": TcpClient connected!");
                    return true;
                }
                else
                {
                    tcpClient.Close();
                    _log.Warning(callLogTag + ": TcpClient failed to connect!");
                    return false;
                }
            }
            catch (Exception e)
            {
                _log.Error(callLogTag + ": Exception: " + e.Message + ": " + e.InnerException);
                return false;
            }
        }

        public IMessageProcessor MessageProcessor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageXML"></param>
        private void ProcessMessage(
            string messageXML)
        {
            var message = Message.GetObjectFromMessageXML(messageXML);

            if (MessageProcessor != null)
            {
                MessageProcessor.Process(message);
            }
            else
            {
                throw new Exception("Client.ProcessMessage: MessageProcessor is null");
            }
        }
        
        #endregion Non-public API
    }
}
