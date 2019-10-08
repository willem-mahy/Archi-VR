using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace WM
{
    public class WMTrackerConnection_UdpClient
    {
        private string serverIp = "127.0.0.1";
        private int serverPort = 8888;
        private int clientPort = 8887;

        private bool m_connected = false;

        public bool IsConnected() { return m_connected; }

        private bool m_running = true;

        private string m_receivedText;

        private System.Object m_receivedTextLock = new System.Object();

        private ILogger m_logger;

        public WMTrackerConnection_UdpClient(
            ILogger logger,
            string serverIp,
            int serverPort,
            int clientPort)
        {
            this.m_logger = logger;
            this.serverIp = serverIp;
            this.serverPort = serverPort;
            this.clientPort = clientPort;
            m_receivedText = "";
        }

        public void Run()
        {
            Console.WriteLine("WMTrackerConnection_UdpClient.Run()");
            try
            {
                m_running = true;
                m_connected = false;

                UdpClient client = null;

                while ((client == null) && m_running)
                {
                    try
                    {
                        client = new UdpClient(clientPort);

                        Thread.Sleep(1);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("WMTrackerConnection_UdpClient.Run(): Failed to create UdpCLient! Error message:" + e.Message + ", source:" + e.Source);
                    }
                }

                IPEndPoint ep = null;
                if (client !=null)
                {
                    ep = new IPEndPoint(IPAddress.Parse(serverIp), serverPort); // endpoint where server is listening

                    Console.WriteLine("WMTrackerConnection_UdpClient.Run(): Connecting to UDP server (" + serverIp + ":" + serverPort + ") ...");
                    try
                    {
                        client.Connect(ep);
                        m_connected = true;
                        Console.WriteLine("WMTrackerConnection_UdpClient.Run(): Connected.");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("WMTrackerConnection_UdpClient.Run(): Failed to create UdpCLient!");
                    }
                }

                Console.WriteLine("WMTrackerConnection_UdpClient.Run(): Sending connect message 'Hello'...");
                var connectMessage = Encoding.ASCII.GetBytes("Hello");

                var connectionMessageSent = false;
                do
                {
                    try
                    {
                        // Notify the server that we're connecting.                 

                        // This synchronous call does not seem to throw, 
                        // even while the server is not yet listening.
                        client.Send(connectMessage, connectMessage.Length);

                        Console.WriteLine("WMTrackerConnection_UdpClient.Run():  Connect message sent.");

                        if (client.Available > 0)
                        {
                            // This synchronous call does seem to throw, 
                            // while the server is not yet listening.
                            var receivedData = client.Receive(ref ep);

                            if (receivedData.Length > 0)
                            {
                                // As soon as the server starts sending data, we are connected.
                                m_connected = true;
                                Console.WriteLine("WMTrackerConnection_UdpClient.Run(): Connected.");
                            }

                            connectionMessageSent = true;
                        }

                        Thread.Sleep(500);
                    }
                    catch (System.Net.Sockets.SocketException e)
                    {
                        Console.WriteLine("WMTrackerConnection_UdpClient.Run(): Exception while trying to say 'Hello' to UDP server at " + ep.Address.ToString() + ":" + ep.Port.ToString() + ".  Source: " + e.Source + ", message:" + e.Message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("WMTrackerConnection_UdpClient.Run(): Exception while trying to say 'Hello' to UDP server at " + ep.Address.ToString() + ":" + ep.Port.ToString() + ".  Source: " + e.Source + ", message:" + e.Message);
                    }
                }
                while (!connectionMessageSent && m_running);                

                bool first = true;

                while (m_running)
                {
                    //Debug.Log("WMTrackerConnection_UdpClient.Running...");

                    var receivedData = client.Receive(ref ep);

                    int numBytesReceived = receivedData.Length;

                    if (numBytesReceived > 0)
                    {
                        string newText = Encoding.ASCII.GetString(receivedData, 0, numBytesReceived);

                        if (first)
                        {
                            first = false;
                            //textboxInfo.Text = 
                            Console.WriteLine("Received from WMTracker: " + newText);
                        }

                        lock (m_receivedTextLock)
                        {
                            m_receivedText += newText;
                        }
                    }

                    Thread.Sleep(1);
                }

                // Notify the server that we're disconnecting.
                if (m_connected)
                {
                    Console.WriteLine("WMTrackerConnection_UdpClient.Run: Disconnecting...");
                    var bytes = Encoding.ASCII.GetBytes("Bye");
                    client.Send(bytes, bytes.Length);
                    m_connected = false;
                    Console.WriteLine("Done!");
                }

                Thread.Sleep(1000);

                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("WMTrackerConnection_UdpClient.Run(): Exception occured: " + e.Message + " @ " + e.StackTrace);
            }
        }

        public string GetReceivedText()
        {
            lock (m_receivedTextLock)
            {
                string receivedText = (string)m_receivedText.Clone();
                m_receivedText = "";
                return receivedText;
            }
        }

        public void Stop()
        {
            m_running = false;
        }
    }
} // WM