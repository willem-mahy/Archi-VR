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
            public const string XmlBeginTag = "<Message ";
            public const string XmlEndTag = "</Message>";

            public byte[] Data { get; set; }

            public void Serialize(object obj)
            {
                using (var stream = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(stream, obj);
                    stream.Flush();
                    Data = stream.ToArray();
                }
            }

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
