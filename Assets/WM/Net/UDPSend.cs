using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace WM
{
    namespace Net
    {
        /*
         */
        public class UDPSend
        {
            private static int localPort;

            // prefs
            public string remoteIP = "127.0.0.1"; // 'loop back by default...
            public int remotePort = 8890;

            // "connection" things
            IPEndPoint remoteEndPoint;

            UdpClient udpClient;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="udpClient"></param>
            public UDPSend(UdpClient udpClient)
            {
                this.udpClient = udpClient;
            }

            public void Init()
            {
                WM.Logger.Debug("UDPSend.Init(): Start");

                remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);

                WM.Logger.Debug("UDPSend.Init(): UDPSend running @ " + remoteIP + ":" + remotePort + ")");
            }

            //! Sends the given message to the remote client.
            public void sendString(string message)
            {
                if (remoteEndPoint == null)
                {
                    return; // Not connected yet...
                }

                try
                {
                    if (message != "")
                    {
                        // Encode data to UTF8-encoding.
                        byte[] data = Encoding.UTF8.GetBytes(message);

                        // Send data to remote client.
                        udpClient.Send(data, data.Length, remoteEndPoint);
                    }
                }
                catch (Exception e)
                {
                    WM.Logger.Error("UDPSend.sendString(): Exception: " + e.ToString());
                }
            }
        }
    }
}