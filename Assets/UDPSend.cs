/*

-----------------------
UDP-Send
-----------------------
// [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]

// > gesendetes unter
// 127.0.0.1 : 8050 empfangen

// nc -lu 127.0.0.1 8050

    // todo: shutdown thread at the end
*/
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace WM
{

    public class UDPSend : MonoBehaviour
    {
        private static int localPort;

        // prefs
        public string remoteIP = "127.0.0.1"; // 'loop back by default...
        public int remotePort = 8890;

        // "connection" things
        IPEndPoint remoteEndPoint;
        UdpClient udpClient;

        // gui
        string strMessage = "";

        public UDPSend(UdpClient udpClient)
        {
            this.udpClient = udpClient;
        }

        // start from unity3d
        public void Init()
        {
            Debug.LogError("UDPSend.init()");

            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);

            Debug.LogError("Initialized for sending to remote end point (" + remoteIP + ":" + remotePort + ")");
        }

        // inputFromConsole
        private void inputFromConsole()
        {
            try
            {
                string text;
                do
                {
                    text = Console.ReadLine();

                    // Den Text zum Remote-Client senden.
                    if (text != "")
                    {
                        // Encode data to UTF8-encoding.
                        byte[] data = Encoding.UTF8.GetBytes(text);

                        // Send data to remote client.
                        udpClient.Send(data, data.Length, remoteEndPoint);
                    }
                } while (text != "");
            }
            catch (Exception err)
            {
                print(err.ToString());
            }

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
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }
}