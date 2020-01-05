using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WM.Net
{
    /* Continuously (on its own worker thread) receives data on the given UDP socket.
        * Received UDP packets are stored in a dictionary, mapped on the IP address of the sender.
        */
    public class UDPReceive
    {
        #region Variables

        /// <summary>
        /// Receiving Thread
        /// </summary>
        private Thread receiveThread;

        /// <summary>
        /// UdpClient object
        /// </summary>
        private UdpClient udpClient;

        /// <summary>
        /// 
        /// </summary>
        public string lastReceivedUDPPacket = "";

        /// <summary>
        /// All received UDP packets, mapped on remote endpoint key (<see cref="GetRemoteEndpointKey(string, int)"/>).
        /// 
        /// Clean up this from time to time!
        /// </summary>
        public Dictionary<string, string> allReceivedUDPPackets = new Dictionary<string, string>();

        /// <summary>
        /// 
        /// </summary>
        private CancellationTokenSource shutdownTokenSource = new CancellationTokenSource();

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static String GetRemoteEndpointKey(string ip, int port)
        {
            return ip + ":" + port;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="udpClient"></param>
        public UDPReceive(UdpClient udpClient)
        {
            this.udpClient = udpClient;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            WM.Logger.Debug("UDPReceive.Init()");

            if (shutdownTokenSource != null)
            {
                shutdownTokenSource.Dispose();
            }
            shutdownTokenSource = new CancellationTokenSource();

            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();

            WM.Logger.Debug("UDPReceive.Init() End");
        }

        /// <summary>
        /// 
        /// </summary>
        public void Shutdown()
        {
            WM.Logger.Debug("UDPReceive.Shutdown()");

            shutdownTokenSource.Cancel();

            if (receiveThread != null)
            {
                receiveThread.Join();

                receiveThread = null;
            }

            WM.Logger.Debug("UDPReceive.Shutdown() End");
        }

        /// <summary>
        /// Receive thread function.
        /// </summary>
        private void ReceiveData()
        {
            while (true)
            {
                try
                {
                    // Receive bytes from any client.                        
                    var task = udpClient.ReceiveAsync();

                    try
                    {
                        task.Wait(shutdownTokenSource.Token);
                    }
                    catch (OperationCanceledException /*e*/)
                    {
                        // The client has started shutting down, so stop receiving data.
                        return;
                    }

                    var result = task.Result;

                    var remoteEndPoint = result.RemoteEndPoint;

                    // Encode received bytes to UTF8- encoding.
                    string text = Encoding.UTF8.GetString(result.Buffer);

                    // latest UDPpacket
                    lastReceivedUDPPacket = text;

                    // ....
                    var senderIP = GetRemoteEndpointKey(remoteEndPoint.Address.ToString(), remoteEndPoint.Port);

                    lock (allReceivedUDPPackets)
                    {
                        if (allReceivedUDPPackets.ContainsKey(senderIP))
                        {
                            allReceivedUDPPackets[senderIP] = allReceivedUDPPackets[senderIP] + text;
                        }
                        else
                        {
                            allReceivedUDPPackets[senderIP] = text;
                        }
                    }
                }
                catch (Exception e)
                {
                    WM.Logger.Error("UDPReceive.ReceiveData(): Exception: " + e.ToString());
                }
            }
        }
    }
}