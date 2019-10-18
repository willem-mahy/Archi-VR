
using UnityEngine;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;
using System;

namespace WM
{
    namespace Net
    {
        [Serializable]
        [XmlRoot("TrackedObject")]
        public class TrackedObject
        {
            [XmlElement("Name")]
            public string Name { get; set; } = "";

            [XmlElement("Position")]
            public Vector3 Position { get; set; } = new Vector3();

            [XmlElement("Rotation")]
            public Quaternion Rotation { get; set; } = new Quaternion();
        }
    } // namespace Net
} // namespace WM