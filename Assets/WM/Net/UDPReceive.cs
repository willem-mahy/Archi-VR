using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WM
{
    namespace Net
    {
        /*
         */
        public class UDPReceive
        {
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
            /// Clean up this from time to time!
            /// </summary>
            public Dictionary<string, string> allReceivedUDPPackets = new Dictionary<string, string>();

            /// <summary>
            /// 
            /// </summary>
            CancellationTokenSource shutdownTokenSource = new CancellationTokenSource();

            public UDPReceive(UdpClient udpClient)
            {
                this.udpClient = udpClient;
            }

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
                        catch (OperationCanceledException e)
                        {
                            return;
                        }

                        var result = task.Result;

                        var remoteEndPoint = result.RemoteEndPoint;

                        // Encode received bytes to UTF8- encoding.
                        string text = Encoding.UTF8.GetString(result.Buffer);

                        // latest UDPpacket
                        lastReceivedUDPPacket = text;

                        // ....
                        var senderIP = remoteEndPoint.Address.ToString();

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
}