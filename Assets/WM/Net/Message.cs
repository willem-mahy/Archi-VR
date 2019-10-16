using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace WM
{
    namespace Net
    {
        [Serializable]
        [XmlRoot("Message")]
        public class Message
        {
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
        }
    }
}
