using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WM.Net
{
    /*
     */
    public class UDPSend
    {
        /// <summary>
        /// 
        /// </summary>
        private static int localPort;

        /// <summary>
        /// The remote end point IP. (Default: loop back)
        /// </summary>
        public string remoteIP = "127.0.0.1";

        /// <summary>
        /// The remote end point port. (Default: 8890)
        /// </summary>
        public int remotePort = 8890;

        /// <summary>
        /// 
        /// </summary>
        IPEndPoint remoteEndPoint;

        /// <summary>
        /// 
        /// </summary>
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
            WM.Logger.Debug("UDPSend.Init()");

            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);

            WM.Logger.Debug("UDPSend.Init(): UDPSend running (Target " + remoteIP + ":" + remotePort + ")");

            WM.Logger.Debug("UDPSend.Init() End");
        }

        /// <summary>
        /// Sends the given message to the remote end point.
        /// </summary>
        /// <param name="message"></param>
        public void SendString(string message)
        {
            if (remoteEndPoint == null)
            {
                return; // Not connected yet...
            }

            try
            {
                var data = (message == null) ? new byte[0] : Encoding.UTF8.GetBytes(message);

                // Send data to remote client.
                udpClient.Send(data, data.Length, remoteEndPoint);
            }
            catch (Exception e)
            {
                WM.Logger.Error("UDPSend.SendString(): Exception: " + e.ToString());
            }
        }
    }
}