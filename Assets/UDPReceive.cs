
/*
 
    -----------------------
    UDP-Receive (send to)
    -----------------------
    // [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]
   
   
    // > receive
    // 127.0.0.1 : 8051
   
    // send
    // nc -u 127.0.0.1 8051
 
*/
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace WM
{
    public class UDPReceive : MonoBehaviour
    {

        // receiving Thread
        private Thread receiveThread;

        // udpclient object
        private UdpClient udpClient;

        //
        public string lastReceivedUDPPacket = "";
        
        // clean up this from time to time!
        public string allReceivedUDPPackets = "";

        public UDPReceive(UdpClient udpClient)
        {
            this.udpClient = udpClient;
        }

        public void Init()
        {
            // Endpunkt definieren, von dem die Nachrichten gesendet werden.
            print("UDPReceive.init()");

            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        // receive thread
        private void ReceiveData()
        {
            while (true)
            {

                try
                {
                    // Bytes empfangen.
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = udpClient.Receive(ref anyIP);

                    // Bytes mit der UTF8-Kodierung in das Textformat kodieren.
                    string text = Encoding.UTF8.GetString(data);

                    // latest UDPpacket
                    lastReceivedUDPPacket = text;

                    // ....
                    allReceivedUDPPackets = allReceivedUDPPackets + text;

                }
                catch (Exception err)
                {
                    print(err.ToString());
                }
            }
        }

        //public string getLatestUDPPacket()
        //{
        //    allReceivedUDPPackets = "";
        //    return lastReceivedUDPPacket;
        //}

        public string getAllReceivedData()
        {
            string r = allReceivedUDPPackets;
            allReceivedUDPPackets = "";
            return r;
        }
    }
}