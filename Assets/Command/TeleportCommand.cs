using System;
using System.Xml.Serialization;

namespace Assets.Command
{
    [Serializable]
    [XmlRoot("TeleportCommand")]
    public class TeleportCommand
    {
        [XmlElement("ProjectName")]
        public string ProjectName { get; set; } = "";

        [XmlElement("POIName")]
        public string POIName { get; set; } = "";
    }
}
