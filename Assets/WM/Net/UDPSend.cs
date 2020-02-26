using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WM.Net
{
    /// <summary>
    /// 
    /// </summary>
    public class UDPSend
    {
        #region Fields

        /// <summary>
        /// The log.  Injected during construction.
        /// </summary>
        private readonly Logger _log;

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

        #endregion Fields

        /// <summary>
        /// 
        /// </summary>
        /// <param name="udpClient"></param>
        public UDPSend(
            UdpClient udpClient,
            Logger log)
        {
            this.udpClient = udpClient;
            _log = log;
        }

        public void Init()
        {
            var logCallTag = "UDPSend.Init()";

            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);

            _log.Debug(logCallTag + ": UDPSend running. (Target: " + remoteIP + ":" + remotePort + ")");
        }

        /// <summary>
        /// Sends the given message to the remote end point.
        /// </summary>
        /// <param name="message"></param>
        public void SendString(string message)
        {
            var logCallTag = "UDPSend.SendString()";

            if (_log.enableLogUDP)
            {
                _log.Debug(logCallTag);
            }

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
                _log.Error(logCallTag + ": Exception: " + e.ToString());
            }
        }

        /// <summary>
        /// Sends the given message to the remote end point.
        /// </summary>
        /// <param name="message"></param>
        public void Send(byte[] data)
        {
            var logCallTag = "UDPSend.Send()";

            if (_log.enableLogUDP)
            {
                _log.Debug(logCallTag);
            }

            if (remoteEndPoint == null)
            {
                return; // Not connected yet...
            }

            try
            {
                // Send data to remote client.
                udpClient.Send(data, data.Length, remoteEndPoint);
            }
            catch (Exception e)
            {
                _log.Error(logCallTag + ": Exception: " + e.ToString());
            }
        }
    }
}