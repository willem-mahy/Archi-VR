using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

using UnityEngine;

using WM.ArchiVR;
using WM.ArchiVR.Command;

namespace WM
{
    namespace Net
    {
        // Holds all data related to a client connected to the server.
        class ClientConnection
        {
            //! The IP of the client.
            public string remoteIP;

            //! The index of the client's avatar.
            public int AvatarIndex = 0;

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

        public class Server : MonoBehaviour
        {
            #region Variables

            public ApplicationArchiVR application;

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

            // The client connections.
            private List<ClientConnection> clientConnections = new List<ClientConnection>();

            //! Get a string with information about connected clients.
            public string GetClientInfo()
            {
                string info = "";
                lock (this.clientConnections)
                {
                    foreach (var clientConnection in clientConnections)
                    {
                        info += "- " + clientConnection.remoteIP + " AvatarType:" + clientConnection.AvatarIndex + "\n";
                    }
                }
                return info;
            }

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

            private bool shutDown = false;

            #endregion

            //!
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

            //!
            public void Shutdown()
            {
                shutDown = true;

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

            //! Thread function executed by the 'Accept Client' thread.
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

                            // Now the client is connected, make him...

                            // A) .. know its peer clients.  (For each existing client, send a 'ClientConnect' command to the new client.
                            lock (clientConnections)
                            {
                                foreach (var clientConnection in clientConnections)
                                {
                                    if (clientConnection != newClientConnection)
                                    {
                                        var cc1 = new ConnectClientCommand();
                                        cc1.ClientIP = clientConnection.remoteIP;
                                        cc1.AvatarIndex = clientConnection.AvatarIndex;

                                        SendCommand(cc1, newClientConnection);
                                    }
                                }
                            }

                            // B) ...spawn at the current Project and POI.
                            if (clientConnections.Count > 1) // Hack to distinguish the local client running on server host. -> will be taken cae of in ServerClient implementation.
                            {
                                var teleportCommand = new TeleportCommand();
                                teleportCommand.ProjectIndex = application.ActiveProjectIndex;
                                teleportCommand.POIName = application.ActivePOIName;

                                SendCommand(teleportCommand, newClientConnection);

                                // C) ...be in the same immersion mode.
                                var setImmersionModeCommand = new SetImmersionModeCommand();
                                setImmersionModeCommand.ImmersionModeIndex = application.ActiveImmersionModeIndex;

                                SendCommand(setImmersionModeCommand, newClientConnection);
                            }

                            // Notify clients that another client connected.
                            var cc = new ConnectClientCommand();
                            cc.ClientIP = newClientConnection.remoteIP;
                            cc.AvatarIndex = newClientConnection.AvatarIndex;
                            PropagateCommand(cc, newClientConnection);
                        }
                    }
                    catch (Exception ex)
                    {
                        WM.Logger.Error("Server.AcceptClientFunction(): Exception: " + ex.ToString());
                    }
                }
            }

            //! Thread function executed by the 'Receive UDP' thread.
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
                                var trackedObjectXML = GetAvatarStateFromUdp(clientIndex);

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
                    }
                    catch (Exception ex)
                    {
                         WM.Logger.Error("Server.ReceiveUdpFunction(): Exception: " + ex.ToString());
                    }
                }
            }

            //! Thread function executed by the 'Receive TCP' thread.
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

                                string beginTag = "<Message ";
                                string endTag = "</Message>";
                                int EndTagLength = endTag.Length;

                                int firstMessageBegin = clientConnection.tcpReceivedData.IndexOf(beginTag);

                                if (firstMessageBegin < 0)
                                {
                                    continue;
                                }

                                // Remove all data in front of first message.
                                clientConnection.tcpReceivedData = clientConnection.tcpReceivedData.Substring(firstMessageBegin);

                                int firstMessageEnd = clientConnection.tcpReceivedData.IndexOf(endTag);

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

                if (obj is TeleportCommand)
                {
                    BroadcastData(messageXML);
                }
                else if (obj is SetImmersionModeCommand)
                {
                    BroadcastData(messageXML);
                }
                else if (obj is ConnectClientCommand)
                {
                    var ccc = (ConnectClientCommand)obj;
                    clientConnection.AvatarIndex = ccc.AvatarIndex;
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
                    SendData(GetObjectAsData(new ClientDisconnectAcknoledgeMessage()), clientConnection);

                    // Step 3: close the clientconnection network streams.
                    clientConnection.Close();                    

                    return false;
                }
                else if (obj is SetClientAvatarCommand)
                {
                    var scac = (SetClientAvatarCommand)obj;
                    clientConnection.AvatarIndex = scac.AvatarIndex;
                    PropagateData(messageXML, clientConnection);
                }
                else if (obj is SetClientAvatarCommand)
                {
                    var scac = (SetClientAvatarCommand)obj;
                    clientConnection.AvatarIndex = scac.AvatarIndex;
                    PropagateData(messageXML, clientConnection);
                }

                return true;
            }
                        
            public void BroadcastCommand(
                ICommand command)
            {
                Debug.Log("Server:BoadcastCommand()");

                try
                {
                    string data = GetObjectAsData(command);

                    BroadcastData(data);
                }
                catch (Exception e)
                {
                    WM.Logger.Error("Server.BroadcastCommand(): Exception: " + e.Message);
                }
            }

            private void PropagateCommand(
                ICommand command,
                ClientConnection sourceClientConnection)
            {
                Debug.Log("Server:PropagateCommand()");

                try
                {
                    string data = GetObjectAsData(command);

                    PropagateData(data, sourceClientConnection);
                }
                catch (Exception e)
                {
                    WM.Logger.Error("Server.PropagateCommand(): Exception: " + e.Message);
                }
            }

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

            //!
            private string GetObjectAsData(object obj)
            {
                var ser = new XmlSerializer(typeof(Message));

                var writer = new StringWriter();
                ser.Serialize(writer, GetObjectAsMessage(obj));
                writer.Close();

                var data = writer.ToString();

                return data;
            }

            private Message GetObjectAsMessage(object obj)
            {
                var message = new Message();
                message.Serialize(obj);

                return message;
            }

            //!
            private void SendCommand(
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

                    var data = GetObjectAsData(command);

                    SendData(data, clientConnection);
                }
                catch (Exception e)
                {
                    WM.Logger.Error("Server.SendCommand(): Exception:" + e.Message);
                }
            }

            //!
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

            //!
            public string GetAvatarStateFromUdp(int clientIndex)
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

                        string frameEndTag = "</AvatarState>";
                        int frameEndTagLength = frameEndTag.Length;
                        int lastFrameEnd = udpReceive.allReceivedUDPPackets[clientIP].LastIndexOf(frameEndTag);

                        if (lastFrameEnd < 0)
                        {
                            return null;
                        }

                        string temp = udpReceive.allReceivedUDPPackets[clientIP].Substring(0, lastFrameEnd + frameEndTagLength);

                        int lastFrameBegin = temp.LastIndexOf("<AvatarState ");

                        if (lastFrameBegin < 0)
                        {
                            return null;
                        }

                        // Now get the frame string.
                        trackedObjectXML = temp.Substring(lastFrameBegin, temp.Length - lastFrameBegin);

                        // Clear old frames from receivebuffer.
                        udpReceive.allReceivedUDPPackets[clientIP] = udpReceive.allReceivedUDPPackets[clientIP].Substring(lastFrameEnd + frameEndTagLength);
                    }

                    var ser = new XmlSerializer(typeof(AvatarState));

                    //var reader = new StreamReader(avatarFilePath);
                    var reader = new StringReader(trackedObjectXML);

                    var trackedObject = (AvatarState)(ser.Deserialize(reader));
                    reader.Close();

                    return trackedObjectXML;
                }
                catch (Exception e)
                {
                     WM.Logger.Error("Server.GetAvatarStateFromUdp(): Exception: " + e.Message);
                    return null;
                }
            }
        }
    }
}
