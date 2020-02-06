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
        /// The log.  Injected during construction.
        /// </summary>
        private readonly Logger _log;

        /// <summary>
        /// Receiving Thread
        /// </summary>
        private Thread _receiveThread;

        /// <summary>
        /// UdpClient object
        /// </summary>
        private UdpClient _udpClient;

        /// <summary>
        /// 
        /// </summary>
        private CancellationTokenSource _shutdownTokenSource = new CancellationTokenSource();

        #endregion

        #region Construction

        /// <summary>
        /// Parametrized constructor.
        /// </summary>
        /// <param name="udpClient">The UDP client.</param>
        /// <param name="log">The log.</param>
        public UDPReceive(
            UdpClient udpClient,
            Logger log)
        {
            this._udpClient = udpClient;
            _log = log;
        }

        #endregion Construction

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
        public void Init()
        {
            _log.Debug("UDPReceive.Init()");

            if (_shutdownTokenSource != null)
            {
                _shutdownTokenSource.Dispose();
            }
            _shutdownTokenSource = new CancellationTokenSource();

            _receiveThread = new Thread(new ThreadStart(ReceiveData));
            _receiveThread.IsBackground = true;
            _receiveThread.Start();

            _log.Debug("UDPReceive.Init() End");
        }

        /// <summary>
        /// 
        /// </summary>
        public void Shutdown()
        {
            _log.Debug("UDPReceive.Shutdown()");

            _shutdownTokenSource.Cancel();

            if (_receiveThread != null)
            {
                _receiveThread.Join();

                _receiveThread = null;
            }

            _log.Debug("UDPReceive.Shutdown() End");
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
                    var task = _udpClient.ReceiveAsync();

                    try
                    {
                        task.Wait(_shutdownTokenSource.Token);
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
                    _log.Error("UDPReceive.ReceiveData(): Exception: " + e.ToString());
                }
            }
        }
    }
}