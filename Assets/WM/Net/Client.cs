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
    abstract public class Client : MonoBehaviour
    {
        #region Variables

        public string InitialServerIP = "";//192.168.0.13";

        public int ConnectTimeout = 100;

        #region TCP

        public int TcpPort = 8889; // Must be different than server TCP port probably...

        // The TCP client
        private TcpClient tcpClient;

        // The network stream to the server over TCP.
        private NetworkStream tcpServerStream;

        #endregion

        #region UDP

        public static readonly int UdpPort = 8891; // Must be different than server UDP port probably...

        private UdpClient udpClient;

        private UDPSend udpSend;

        private UDPReceive udpReceive;

        #endregion

        // The client's worker thread.
        private Thread thread;

        #region Internal state

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
        /// Initialize the client.
        /// </summary>
        public void Init()
        {
            shutDown = false;

            thread = new Thread(new ThreadStart(ThreadFunction));
            thread.IsBackground = true;
            thread.Start();

            WM.Logger.Debug("Client started");
        }

        /// <summary>
        /// Disconnect the client.
        /// </summary>
        public void Disconnect()
        {
            SendCommand(new DisconnectClientCommand(NetUtil.GetLocalIPAddress()));

            while (Status != "DisconnectAcknoledged") ;

            Shutdown();
        }

        /// <summary>
        /// 
        /// </summary>
        private void Shutdown()
        {
            shutDown = true;

            if (thread != null)
            {
                thread.Join();

                thread = null;
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
        /// Synchronously start listening at the UDP broadcast port for UdpBroadcastMessages.
        /// </summary>
        /// <returns>A string containing the IP address of the first server from which we receive a valid UdpBroadcastMessage</returns>
        private string GetServerIPFromUdpBroadcast()
        {
            Status = "Listening for servers";

            var udpClient = new UdpClient(Server.BroadcastUdpPort);
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (!shutDown)
            {
                try
                {
                    // Receive bytes from anyone.                    
                    byte[] data = udpClient.Receive(ref remoteEndPoint);

                    // Encode received bytes to UTF8- encoding.
                    string text = Encoding.UTF8.GetString(data);

                    if (text.Contains(Server.UdpBroadcastMessage))
                    {
                        return remoteEndPoint.Address.ToString();
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("UDPReceive.ReceiveData(): Exception: " + e.ToString());
                }
            }

            return "";
        }

        /// <summary>
        /// Thread function executed by the client's worker thread.
        /// </summary>
        private void ThreadFunction()
        {
            if (InitialServerIP == "")
            {
                InitialServerIP = GetServerIPFromUdpBroadcast();
            }

            while (!shutDown)
            {
                if (TryConnect())
                {
                    Status = "Connected to " + ServerIP;
                    WM.Logger.Debug("Client: tcpClient connected.");

                    // Get server stream from TCP client.
                    tcpServerStream = tcpClient.GetStream();

                    // Initialize UDP sockets to/from server.
                    {
                        udpClient = new UdpClient(UdpPort);

                        udpSend = new UDPSend(udpClient);
                        udpSend.remoteIP = ServerIP;
                        udpSend.remotePort = Server.UdpPort;
                        udpSend.Init();

                        udpReceive = new UDPReceive(udpClient);
                        udpReceive.Init();
                    }

                    OnTcpConnected();
                    break;
                }
            }

            while (!shutDown)
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
            if (InitialServerIP != "")
            {
                // connect to a predefined server.
                return TryConnectToServer(InitialServerIP);
            }
            else
            {
                var localSubNet = NetUtil.GetLocalIPSubNet();

                // Search the local subnet for a server.
                for (int i = 0; i < 255; ++i)
                {
                    string serverIP = localSubNet + i;

                    if (TryConnectToServer(serverIP))
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
        private bool TryConnectToServer(string serverIP)
        {
            String tag = serverIP + ":" + Server.TcpPort + ", timeout:" + ConnectTimeout + "ms";
            WM.Logger.Debug("Client.TryConnectToServer(): Server:'" + tag);

            Status = "Trying to connect to " + tag;

            try
            {
                var tcpClient = new TcpClient();

                var connectionAttempt = tcpClient.ConnectAsync(serverIP, Server.TcpPort);

                connectionAttempt.Wait(ConnectTimeout);

                if (connectionAttempt.IsCompleted)
                {
                    this.tcpClient = tcpClient;
                    WM.Logger.Debug("Client.TryConnectToServer(): Connected!");
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
            // XML-deserialize the message.
            var ser = new XmlSerializer(typeof(Message));

            var reader = new StringReader(messageXML);

            var message = (Message)(ser.Deserialize(reader));

            reader.Close();

            // Binary-deserialize the object from the message.
            var obj = message.Deserialize();

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
        /// 
        /// </summary>
        abstract public void OnTcpConnected();
                
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
                udpSend.sendString(data);
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
                udpSend.sendString(Message.EncodeObjectAsXml(obj));
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
            Debug.Log("Client:SendCommand()");

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
            Debug.Log("Client:SendData()");

            try
            {
                if (tcpClient == null)
                {
                    return;
                }

                var networkStream = tcpClient.GetStream();

                if (networkStream == null)
                {
                    return;
                }

                var bytes = Encoding.ASCII.GetBytes(data);
                networkStream.Write(bytes, 0, bytes.Length);
                networkStream.Flush();
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
                        Debug.LogWarning("Client.UpdateAvatarStatesFromUDP(): More than one receive buffer!?!");
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
