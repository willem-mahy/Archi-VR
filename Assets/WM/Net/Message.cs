using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace WM
{
    namespace Net
    {
        // Sent by the server, when he has successfully disconnected the client.
        [Serializable]
        public class ClientDisconnectAcknoledgeMessage
        {
        }

        [Serializable]
        [XmlRoot("Message")]
        public class Message
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="messageXML"></param>
            /// <returns></returns>
            public static object GetObjectFromMessageXML(string messageXML)
            {
                try
                {
                    // XML-deserialize the message.
                    var ser = new XmlSerializer(typeof(Message));

                    var reader = new StringReader(messageXML);

                    var message = (Message)(ser.Deserialize(reader));

                    reader.Close();

                    // Binary-deserialize the object from the message.
                    var obj = message.Deserialize();

                    // Return the object that has been parsed from the MessageXML
                    return obj;
                }
                catch (Exception e)
                {
                    WM.Logger.Error("GetObjectFromMessageXML: Failed to parse object from message XML '" + messageXML + "'");
                    throw e;
                }
            }

            public const string XmlBeginTag = "<Message ";
            public const string XmlEndTag = "</Message>";

            public byte[] Data { get; set; }

            /// <summary>
            /// Serialize the given object into the message.
            /// </summary>
            public void Serialize(object obj)
            {
                using (var stream = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(stream, obj);
                    stream.Flush();
                    Data = stream.ToArray();
                }
            }

            /// <summary>
            /// Deserialize the object that is contained in the message.
            /// </summary>
            /// <returns>The deserialized object.</returns>
            public object Deserialize()
            {
                object result = null;
                using (var stream = new MemoryStream(Data))
                {
                    result = new BinaryFormatter().Deserialize(stream);
                }
                return result;
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public static string EncodeObjectAsXml(object obj)
            {
                var message = new Message();
                message.Serialize(obj);

                var ser = new XmlSerializer(typeof(Message));

                var writer = new StringWriter();
                ser.Serialize(writer, message);
                writer.Close();

                var data = writer.ToString();

                return data;
            }
        }
    }
}
