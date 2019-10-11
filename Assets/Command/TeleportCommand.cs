using ArchiVR;
using System;
using System.Xml.Serialization;
using static ArchiVR.ApplicationArchiVR;

namespace Assets.Command
{
    [Serializable]
    [XmlRoot("TeleportCommand")]
    public class TeleportCommand
    {
        [XmlElement("ProjectIndex")]
        public int ProjectIndex { get; set; } = -1;

        [XmlElement("POIName")]
        public string POIName { get; set; } = "";

        public void Execute(ApplicationArchiVR application)
        {
            //if (application.RunAsServer)
            //{
            //    return;
            //}
            
            //if ((application.ActiveProjectIndex == ProjectIndex) && (application.ActivePOIName == POIName))
            //{
            //    return;
            //}

            application.TeleportCommand = this;

            application.SetActiveApplicationState(ApplicationStates.Teleporting);
        }
    }
}
