
using System;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace WM
{
    namespace Net
    {
        [Serializable]
        [XmlRoot("AvatarState")]
        public class AvatarState
        {
            [XmlElement("CLientIP")]
            public string ClientIP { get; set; } = "";

            [XmlElement("HeadPosition")]
            public Vector3 HeadPosition { get; set; } = new Vector3();

            [XmlElement("HeadRotation")]
            public Quaternion HeadRotation { get; set; } = new Quaternion();

            [XmlElement("LHandPosition")]
            public Vector3 LHandPosition { get; set; } = new Vector3();

            [XmlElement("LHandRotation")]
            public Quaternion LHandRotation { get; set; } = new Quaternion();

            [XmlElement("RHandPosition")]
            public Vector3 RHandPosition { get; set; } = new Vector3();

            [XmlElement("RHandRotation")]
            public Quaternion RHandRotation { get; set; } = new Quaternion();
        }
    } // namespace Net
} // namespace WM