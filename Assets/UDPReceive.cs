
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
using System.Collections.Generic;
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
        public Dictionary<string, string> allReceivedUDPPackets = new Dictionary<string, string>();

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
                    var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = udpClient.Receive(ref remoteEndPoint);

                    // Bytes mit der UTF8-Kodierung in das Textformat kodieren.
                    string text = Encoding.UTF8.GetString(data);

                    // latest UDPpacket
                    lastReceivedUDPPacket = text;

                    // ....
                    var senderIP = remoteEndPoint.Address.ToString();

                    if (allReceivedUDPPackets.ContainsKey(senderIP))
                    {
                        allReceivedUDPPackets[senderIP] = allReceivedUDPPackets[senderIP] + text;
                    }
                    else
                    {
                        allReceivedUDPPackets[senderIP] = text;
                    }

                }
                catch (Exception err)
                {
                    print(err.ToString());
                }
            }
        }                
    }
}