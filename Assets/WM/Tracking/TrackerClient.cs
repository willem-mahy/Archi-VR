using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;

namespace WM
{
    namespace Util
    {
        class Net
        {
            public static string GetLocalIPAddress()
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());

                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }

                throw new WebException("Local IP address not found!");
            }
        }
    }

    public class ILogger
    {
        public void Add(string text)
        {
            UnityEngine.Debug.Log(text);
        }
    }

    public class TrackerClient
    {
        // Marked objects, by name.
        public Dictionary<string, TrackedObject> m_markedObjects = new Dictionary<string, TrackedObject>();

        private string m_receiveBuffer;

        private UDPSend udpSend = new UDPSend();
        private UDPReceive udpReceive = new UDPReceive();

        public string serverIP = "127.0.0.1";

        public int serverPort = 8888;
        public int clientPort = 8887;

        //public bool active = false;

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

        private ILogger m_logger = null;

        public TrackerClient(
            WM.ILogger logger)
        {
            this.m_logger = logger;
        }

        public void Log(string text)
        {
            if (m_logger == null)
            {
                return;
            }

            m_logger.Add(text);
        }

        // Use this for initialization
        public void Start()
        {
            if (true)
            {
                udpSend.Start();
                udpReceive.Start();
            }
            else
            {
                // First log own IP
                string myIP = WM.Util.Net.GetLocalIPAddress();
                Log("Device IP: " + myIP);

                if (serverIP.Length == 0)
                {
                    serverIP = myIP;
                }

                Log("Connecting to WMTracker UDP Server @ " + serverIP + ":" + serverPort + "...");

                m_client = new WMTrackerConnection_UdpClient(
                    m_logger,
                    serverIP,
                    serverPort,
                    clientPort);

                // Create the thread object, passing in the Thread funtion via a ThreadStart delegate.
                // This does not start the thread.
                m_thread = new Thread(new ThreadStart(m_client.Run));

                // Start the thread
                m_thread.Start();
            }
        }

        //public void UpdatePositionFromTrackerXML_XPath()
        //{
        //    try
        //    {
        //        m_lastFrame = "";

        //        if (m_client != null)
        //        {
        //            m_receiveBuffer += m_client.GetReceivedText();
        //        }

        //        int lastFrameEnd = m_receiveBuffer.LastIndexOf("</Frame>");

        //        if (lastFrameEnd < 0)
        //        {
        //            return;
        //        }

        //        string temp = m_receiveBuffer.Substring(0, lastFrameEnd + 8);

        //        int lastFrameBegin = temp.LastIndexOf("<Frame>");

        //        if (lastFrameBegin < 0)
        //        {
        //            return;
        //        }

        //        // Now get the frame string.
        //        m_lastFrame = temp.Substring(lastFrameBegin, temp.Length - lastFrameBegin);

        //        if (m_receiveBuffer.Length == 0)
        //        {
        //            return;
        //        }

        //        // Writing to console is detrimental to performance! only use for debugging!
        //        //Console.WriteLine("m_receiveBuffer: " + m_receiveBuffer);
        //        //Console.WriteLine("frame: " + frame);

        //        var xmlReaderSettings = new XmlReaderSettings();
        //        xmlReaderSettings.ConformanceLevel = ConformanceLevel.Fragment;

        //        var doc = new XPathDocument(XmlReader.Create(new StringReader(m_lastFrame), xmlReaderSettings));

        //        var nav = doc.CreateNavigator();

        //        // Read the number of Marked Objects in the frame.
        //        string nums = nav.SelectSingleNode("/Frame/Objects/Num").Value;
        //        int num = XmlConvert.ToInt32(nums);

        //        for (int markedObjectIndex = 0; markedObjectIndex < num; ++markedObjectIndex)
        //        {
        //            string objectElementPath = "/Frame/Objects/Object" + markedObjectIndex;

        //            string name = (nav.SelectSingleNode(objectElementPath + "/Name").Value);

        //            if (!m_markedObjects.ContainsKey(name))
        //            {
        //                m_markedObjects[name] = new TrackedObject();
        //                m_markedObjects[name].Name = name;
        //            }

        //            TrackedObject m = m_markedObjects[name];

        //            m.FromXML(ref nav, objectElementPath);
        //        }

        //        // Clear receive buffer
        //        // TODO: only discard up to and including processed message
        //        m_receiveBuffer = "";

        //        //if (text != null)
        //        //{
        //        //    text.text += "\nPos: " + position.ToString();
        //        //}

        //        //if (m_target)
        //        //{
        //        //    var basePosition = new Vector3(-6, 4, -12);
        //        //    var positionYZSwapped = new Vector3(position.x, position.z, position.y);
        //        //    m_target.transform.position = basePosition + 0.01f * positionYZSwapped;
        //        //}
        //    }
        //    catch (Exception e)
        //    {
        //        Log("Exception:" + e.Message);
        //    }
        //}

        private static readonly string avatarFilePath = @"avatar.xml";

        private readonly object avatarFilePathLock = new object();

        public void UpdatePosition(GameObject avatar)
        {
            UpdatePositionFromUDP(avatar);
            //UpdatePositionFromFile(avatar);
        }

        public void UpdatePositionFromUDP(GameObject avatar)
        {
            try
            {
                m_receiveBuffer += udpReceive.getLatestUDPPacket();

                string frameEndTag = "</TrackedObject>";
                int frameEndTagLength = frameEndTag.Length;
                int lastFrameEnd = m_receiveBuffer.LastIndexOf(frameEndTag);

                if (lastFrameEnd < 0)
                {
                    return;
                }

                string temp = m_receiveBuffer.Substring(0, lastFrameEnd + frameEndTagLength);

                int lastFrameBegin = temp.LastIndexOf("<TrackedObject ");

                if (lastFrameBegin < 0)
                {
                    return;
                }

                // Now get the frame string.
                m_lastFrame = temp.Substring(lastFrameBegin, temp.Length - lastFrameBegin);

                // Clear old frames from receivebuffer.
                m_receiveBuffer = m_receiveBuffer.Substring(lastFrameEnd + frameEndTagLength);

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
                var position = avatar.transform.position + avatar.transform.forward;

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