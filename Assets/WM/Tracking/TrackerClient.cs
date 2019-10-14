using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;

namespace WM
{
    public class ILogger
    {
        public void Add(string text)
        {
            Debug.Log(text);
        }
    }

    public class TrackerClient
    {
        // Marked objects, by name.
        public Dictionary<string, TrackedObject> m_markedObjects = new Dictionary<string, TrackedObject>();

        public static UdpClient udpClient = new UdpClient(8050);

        private UDPSend udpSend;
        private UDPReceive udpReceive;

        public string remoteClientIP = "127.0.0.1";

        public int serverPort = 8888;
        public int clientPort = 8887;

        public float[] position = new float[3];

        private WMTrackerConnection_UdpClient m_client = null;

        private Thread m_thread = null;

        private string m_lastFrame;

        public string LastFrame
        {
            //! Get the last received full frame.
            get
            {
                return m_lastFrame;
            }
        }

        private ILogger logger = null;

        public TrackerClient(
            string remoteClientIP,
            WM.ILogger logger)
        {
            this.remoteClientIP = remoteClientIP;
            this.logger = logger;

            udpSend = new UDPSend(udpClient);
            udpReceive = new UDPReceive(udpClient);
        }

        public void Log(string text)
            {
                if (logger == null)
                {
                    return;
                }

                logger.Add(text);
            }

        // Use this for initialization
        public void Start()
        {
            if (true)
            {
                udpSend.Init();
                udpReceive.Init();
            }
            else
            {
                // First log own IP
                string myIP = WM.Net.NetUtil.GetLocalIPAddress();
                Log("Device IP: " + myIP);

                if (remoteClientIP.Length == 0)
                {
                    remoteClientIP = myIP;
                }

                Log("Connecting to WMTracker UDP Server @ " + remoteClientIP + ":" + serverPort + "...");

                m_client = new WMTrackerConnection_UdpClient(
                    logger,
                    remoteClientIP,
                    serverPort,
                    clientPort);

                // Create the thread object, passing in the Thread funtion via a ThreadStart delegate.
                // This does not start the thread.
                m_thread = new Thread(new ThreadStart(m_client.Run));

                // Start the thread
                m_thread.Start();
            }
        }

        private static readonly string avatarFilePath = @"avatar.xml";

        private readonly object avatarFilePathLock = new object();

        public void UpdatePosition(GameObject avatar)
        {
            UpdatePositionFromUDP(avatar, 0);
            //UpdatePositionFromFile(avatar);
        }

        public void UpdatePositionFromUDP(GameObject avatar, int clientIndex)
        {
            try
            {
                if (udpReceive.allReceivedUDPPackets.Keys.Count == 0)
                {
                    return;
                }

                var remoteIP = udpReceive.allReceivedUDPPackets.Keys.GetEnumerator().Current;

                string frameEndTag = "</TrackedObject>";
                int frameEndTagLength = frameEndTag.Length;
                int lastFrameEnd = udpReceive.allReceivedUDPPackets[remoteIP].LastIndexOf(frameEndTag);

                if (lastFrameEnd < 0)
                {
                    return;
                }

                string temp = udpReceive.allReceivedUDPPackets[remoteIP].Substring(0, lastFrameEnd + frameEndTagLength);

                int lastFrameBegin = temp.LastIndexOf("<TrackedObject ");

                if (lastFrameBegin < 0)
                {
                    return;
                }

                // Now get the frame string.
                m_lastFrame = temp.Substring(lastFrameBegin, temp.Length - lastFrameBegin);

                // Clear old frames from receivebuffer.
                udpReceive.allReceivedUDPPackets[remoteIP] = udpReceive.allReceivedUDPPackets[remoteIP].Substring(lastFrameEnd + frameEndTagLength);

                var ser = new XmlSerializer(typeof(TrackedObject));

                //var reader = new StreamReader(avatarFilePath);
                var reader = new StringReader(m_lastFrame);

                var trackedObject = (TrackedObject)(ser.Deserialize(reader));
                reader.Close();

                avatar.transform.position = trackedObject.Position;
            }
            catch (Exception e)
            {
                Log("Exception:" + e.Message);
            }
        }


        public void UpdatePositionFromFile(GameObject avatar)
        {
            try
            {
                lock (avatarFilePathLock)
                {
                    if (File.Exists(avatarFilePath))
                    {
                        var ser = new XmlSerializer(typeof(TrackedObject));

                        var reader = new StreamReader(avatarFilePath);
                        var trackedObject = (TrackedObject)(ser.Deserialize(reader));
                        reader.Close();

                        avatar.transform.position = trackedObject.Position;

                        //TODO: avatar.transform.rotation = Quaternion.AngleAxis(trackedObject.RotationAngle, trackedObject.RotationAxis);
                    }
                }
            }
            catch (Exception e)
            {
                Log("Exception:" + e.Message);
            }
        }

        public void SendPosition(GameObject avatar)
        {
            SendPositionToUDP(avatar);
            //SendPositionToFile(avatar);
        }

        public void SendPositionToUDP(GameObject avatar)
        {
            try
            {
                var position = avatar.transform.position; // + avatar.transform.forward;

                var to = new TrackedObject();
                to.Name = "Avatar";
                to.Position = position;

                var ser = new XmlSerializer(typeof(TrackedObject));

                var writer = new StringWriter();
                ser.Serialize(writer, to);
                writer.Close();

                var data = writer.ToString();

                udpSend.sendString(data);
            }
            catch (Exception e)
            {
                Log("Exception:" + e.Message);
            }
        }

        public void SendPositionToFile(GameObject avatar)
        {
            try
            {
                var position = avatar.transform.position + avatar.transform.forward;

                var to = new TrackedObject();
                to.Name = "Avatar";
                to.Position = position;

                var ser = new XmlSerializer(typeof(TrackedObject));

                lock (avatarFilePathLock)
                {
                    var writer = new FileStream(avatarFilePath, FileMode.Create);
                    ser.Serialize(writer, to);
                    writer.Close();
                }
            }
            catch (Exception e)
            {
                Log("Exception:" + e.Message);
            }
        }

        public void Stop()
        {
            if (m_client != null)
            {
                m_client.Stop();
            }

            if (m_thread != null)
            {
                m_thread.Join();
            }
        }

        public bool IsConnected() { return m_client.IsConnected(); }
    }
} // namespace WM