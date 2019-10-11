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
        private string IP = "127.0.0.1";  // LocalHost
        
        public int port = 8050;  // define in init

        // "connection" things
        IPEndPoint remoteEndPoint;
        UdpClient udpClient;

        // gui
        string strMessage = "";

        public UDPSend(UdpClient udpClient)
        {
            this.udpClient = udpClient;
        }

        // call it from shell (as program)
        //private static void Main()
        //{
        //    UDPSend sendObj = new UDPSend();
        //    sendObj.init();

        //    // testing via console
        //    // sendObj.inputFromConsole();

        //    // as server sending endless
        //    sendObj.sendEndless(" endless infos \n");

        //}

        // start from unity3d
        public void Start()
        {
            if (!Application.isEditor)
            {
                // Running on quest -> Send position to Aorus
                IP = "192.168.0.13";  // Aorus
            }
            else
            {
                // Running on Aorus -> Send position to Quest
                //IP = "192.168.0.X"; // Quest
            }

            init();
        }

        // OnGUI
        void OnGUI()
        {
            Rect rectObj = new Rect(40, 380, 200, 400);
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.UpperLeft;
            GUI.Box(rectObj, "# UDPSend-Data\n" + IP + " " + port + " #\n"
                        + "shell> nc -lu " + IP + "  " + port + " \n"
                    , style);

            // ------------------------
            // send it
            // ------------------------
            strMessage = GUI.TextField(new Rect(40, 420, 140, 20), strMessage);
            if (GUI.Button(new Rect(190, 420, 40, 20), "send"))
            {
                sendString(strMessage + "\n");
            }
        }

        // init
        public void init()
        {
            // Endpunkt definieren, von dem die Nachrichten gesendet werden.
            print("UDPSend.init()");

            // ----------------------------
            // Senden
            // ----------------------------
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);

            // status
            print("Sending to " + IP + " : " + port);
            print("Testing: nc -lu " + IP + " : " + port);
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