using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace WM
{
    namespace Net
    {
        /*
         */
        public class UDPReceive
        {
            // receiving Thread
            private Thread receiveThread;

            // udpclient object
            private UdpClient udpClient;

            //
            public string lastReceivedUDPPacket = "";

            // clean up this from time to time!
            public Dictionary<string, string> allReceivedUDPPackets = new Dictionary<string, string>();

            public UDPReceive(UdpClient udpClient)
            {
                this.udpClient = udpClient;
            }

            public void Init()
            {
                // Endpunkt definieren, von dem die Nachrichten gesendet werden.
                Debug.Log("UDPReceive.Init()");

                receiveThread = new Thread(new ThreadStart(ReceiveData));
                receiveThread.IsBackground = true;
                receiveThread.Start();

                Debug.Log("UDPReceive running");
            }

            private bool shutDown = false;

            /// <summary>
            /// 
            /// </summary>
            public void ShutDown()
            {
                shutDown = true;

                if (receiveThread != null)
                {
                    receiveThread.Join();

                    receiveThread = null;
                }
            }

            /// <summary>
            /// Receive thread function.
            /// </summary>
            private void ReceiveData()
            {
                while (!shutDown)
                {

                    try
                    {
                        // Recive bytes from any client.
                        var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        byte[] data = udpClient.Receive(ref remoteEndPoint);

                        // Encode received bytes to UTF8- encoding.
                        string text = Encoding.UTF8.GetString(data);

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
                        Debug.Log("UDPReceive.ReceiveData(): Exception: " + e.ToString());
                    }
                }
            }
        }
    }
}