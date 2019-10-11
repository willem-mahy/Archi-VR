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

    public int port = 8888;

    private Thread thread;

    private int requestCount = 0;

    // The server socket.
    TcpListener tcpListener;

    // The client socket.
    List<TcpClient> clientSockets = new List<TcpClient>();

    #endregion

    // Start is called before the first frame update
    void Start()
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

        thread = new Thread(new ThreadStart(ThreadFunction));
        thread.IsBackground = true;
        thread.Start();

        Debug.Log("Server started");
    }

    private void ThreadFunction()
    {
        requestCount = 0;

        while ((true))
        {
            try
            {
                var newTcpClient = AcceptNewClient();

                var teleportCommand = new TeleportCommand();
                teleportCommand.ProjectName = "Foo";
                teleportCommand.POIName = "Bar";

                SendCommand(teleportCommand, newTcpClient);

                requestCount = requestCount + 1;

                foreach (var tcpClient in clientSockets)
                {
                    var dataFromClient = "";
                    {
                        var networkStream = tcpClient.GetStream();

                        // Receive from client.
                        byte[] bytesFromClient = new byte[tcpClient.ReceiveBufferSize];
                        networkStream.Read(bytesFromClient, 0, (int)tcpClient.ReceiveBufferSize);
                        dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFromClient);
                    }

                    var messageEnd = dataFromClient.IndexOf("$");
                    while (messageEnd != -1)
                    {
                        var messageFromClient = dataFromClient.Substring(0, messageEnd);
                        dataFromClient = dataFromClient.Substring(messageEnd+1);

                        Debug.Log(" >> Message from client - " + messageFromClient);
                        messageEnd = dataFromClient.IndexOf("$");

                        // Respond to client.
                        /*
                        string dataToClient = "I received your message: '" + messageFromClient + "'";
                        SendToClient(dataToClient, tcpClient);
                        */
                    }
                }
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

    private TcpClient AcceptNewClient()
    {
        // Create the client socket.
        var newTcpClient = default(TcpClient);

        // Accept the client socket.
        newTcpClient = tcpListener.AcceptTcpClient();

        Debug.Log("Server: Client connected: " + newTcpClient.Client.RemoteEndPoint.ToString());

        clientSockets.Add(newTcpClient);

        return newTcpClient;
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

        // Close the client sockets.
        foreach (var clientSocket in clientSockets)
        {
            clientSocket.Close();
        }
        clientSockets.Clear();

        // Stop the server socket.
        tcpListener.Stop();
    }
}
