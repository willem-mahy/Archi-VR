
using Assets.WM.Unity;
using System;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace WM
{
    namespace Net
    {
        [Serializable]
        //[XmlRoot("AvatarState")]
        public class AvatarState
        {
            //[XmlElement("CLientIP")]
            public string ClientIP { get; set; } = "";

            //[XmlElement("HeadPosition")]
            public SerializableVector3 HeadPosition { get; set; } = new SerializableVector3();

            //[XmlElement("HeadRotation")]
            public SerializableQuaternion HeadRotation { get; set; } = new SerializableQuaternion();

            //[XmlElement("LHandPosition")]
            public SerializableVector3 LHandPosition { get; set; } = new SerializableVector3();

            //[XmlElement("LHandRotation")]
            public SerializableQuaternion LHandRotation { get; set; } = new SerializableQuaternion();

            //[XmlElement("RHandPosition")]
            public SerializableVector3 RHandPosition { get; set; } = new SerializableVector3();

            //[XmlElement("RHandRotation")]
            public SerializableQuaternion RHandRotation { get; set; } = new SerializableQuaternion();
        }
    } // namespace Net
} // namespace WM