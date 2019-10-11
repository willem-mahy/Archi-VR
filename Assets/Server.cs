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

public class Server : MonoBehaviour
{
    #region Variables

    public ApplicationArchiVR application;

    public int port = 8888;

    private Thread receiveFromClientsThread;

    private Thread acceptClientThread;

    // The server socket.
    TcpListener tcpListener;

    // The client socket.
    List<TcpClient> clientSockets = new List<TcpClient>();

    private object clientsLock = new object();
    #endregion

    // Start is called before the first frame update
    void Start()
    {   
    }

    public void Init()
    {
        // TODO: Why is this needed?
        System.Text.ASCIIEncoding ASCII = new System.Text.ASCIIEncoding();

        // Get host name for local machine.
        var hostName = Dns.GetHostName();

        // Get host entry.
        var hostEntry = Dns.GetHostEntry(hostName);

        // Print all IP adresses:
        foreach (var ipAddress in hostEntry.AddressList)
        {
            Debug.Log("- IP address" + ipAddress);
        }

        // Get first IP address.
        var serverIpAddress = hostEntry.AddressList[1];
        Debug.Log("Server IP address: " + serverIpAddress.ToString());

        // Create the server socket.
        tcpListener = new TcpListener(serverIpAddress, port);

        // Start the server socket.
        tcpListener.Start();

        acceptClientThread = new Thread(new ThreadStart(AcceptClientFunction));
        acceptClientThread.IsBackground = true;
        acceptClientThread.Name = "acceptClientThread";
        acceptClientThread.Start();

        receiveFromClientsThread = new Thread(new ThreadStart(ReceiveFromClientsFunction));
        receiveFromClientsThread.IsBackground = true;
        receiveFromClientsThread.Name = "receiveFromClientsThread";
        receiveFromClientsThread.Start();

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

                    var teleportCommand = new TeleportCommand();
                    teleportCommand.ProjectIndex = application.ActiveProjectIndex;
                    teleportCommand.POIName = application.ActivePOIName;

                    SendCommand(teleportCommand, newTcpClient);

                    lock (clientsLock)
                    {
                        clientsLockOwner = "AcceptClientFunction";

                        clientSockets.Add(newTcpClient);

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

    private string clientsLockOwner = "";

    private void ReceiveFromClientsFunction()
    {
        while (true)
        {
            try
            {
                lock (clientsLock)
                {
                    clientsLockOwner = "ReceiveFromClientsFunction";

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

                            Debug.Log(" >> Message from client - " + messageFromClient);
                            messageEnd = dataFromClient.IndexOf("$");
                        }
                    }

                    clientsLockOwner = "None (last:ReceiveFromClientsFunction)";
                }
                Thread.Sleep(10);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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
        try
        {
            var ser = new XmlSerializer(typeof(TeleportCommand));

            var writer = new StringWriter();
            ser.Serialize(writer, teleportCommand);
            writer.Close();

            var data = writer.ToString();

            lock (clientsLock)
            {
                clientsLockOwner = "BroadcastCommand";

                foreach (var tcpClient in clientSockets)
                {
                    //if (tcpClient != null)
                    {
                        SendToClient(data, tcpClient);
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
        try
        {
            var ser = new XmlSerializer(typeof(TeleportCommand));

            var writer = new StringWriter();
            ser.Serialize(writer, teleportCommand);
            writer.Close();

            var data = writer.ToString();

            SendToClient(data, tcpClient);
        }
        catch (Exception e)
        {
            Debug.LogError("Exception:" + e.Message);
        }
    }

    public void SendToClient(
        String data,
        TcpClient tcpClient)
    {
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
}
