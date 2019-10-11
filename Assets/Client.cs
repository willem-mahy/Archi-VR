using Assets.Command;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;
using WM;

public class Client : MonoBehaviour
{
    #region Variables

    public string serverIP = "";//192.168.0.13";

    public int port = 8888;

    private TcpClient tcpClient;

    private Thread thread;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // TODO: Why is this needed?
        ASCIIEncoding ASCII = new ASCIIEncoding();

        // Create client socket
        tcpClient = new TcpClient();

        thread = new Thread(new ThreadStart(ThreadFunction));
        thread.IsBackground = true;
        thread.Start();

        Debug.Log("Server started");
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void ThreadFunction()
    {
        Connect();
        Debug.Log("Client: tcpClient connected.");

        var serverStream = tcpClient.GetStream();

        // Send message to server
        var myIP = WM.Util.Net.GetLocalIPAddress();
        var messageToServer = "Hello from client '" + myIP + "'$";
        var bytesToServer = Encoding.ASCII.GetBytes(messageToServer);
        serverStream.Write(bytesToServer, 0, bytesToServer.Length);
        serverStream.Flush();

        while (true)
        {
            // Receive message from server.
            var bytesFromServer = new byte[tcpClient.ReceiveBufferSize];
            serverStream.Read(bytesFromServer, 0, (int)tcpClient.ReceiveBufferSize);
            string dataFromServer = Encoding.ASCII.GetString(bytesFromServer);
            Debug.Log("Client: Data from server: " + dataFromServer);

            var ser = new XmlSerializer(typeof(TeleportCommand));

            //var reader = new StreamReader(avatarFilePath);
            var reader = new StringReader(dataFromServer);

            var trackedObject = (TeleportCommand)(ser.Deserialize(reader));
            reader.Close();

            Thread.Sleep(1000);
        }
    }

    public int connectTimeout = 500;

    private void Connect()
    {
        while (true)
        {
            if (this.serverIP != "")
            {
                // connect to a predefined server.
                if (ConnectToServer(this.serverIP))
                {
                    return;
                }
            }
            else
            {
                // Search the local subnet for a server.
                for (int i = 0; i < 256; ++i)
                {
                    string serverIP = "192.168.0." + i;

                    if (ConnectToServer(serverIP))
                    {
                        // Remember server IP.
                        this.serverIP = serverIP;
                        return;
                    }
                }
            }
        }
    }

    private bool ConnectToServer(string serverIP)
    {
        Debug.Log("Client: Trying to connect tcpClient to '" + serverIP + ":" + port + "' (timeout: " + connectTimeout + "ms)");

        var tcpClient = new TcpClient();

        var connectionAttempt = tcpClient.ConnectAsync(serverIP, port);
        
        connectionAttempt.Wait(connectTimeout);

        if (tcpClient.Connected)
        {
            this.tcpClient = tcpClient;
        }

        return tcpClient.Connected;
    }
}
