using ArchiVR;
using System;
using System.Xml.Serialization;
using UnityEngine;
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
            Debug.Log("TeleportCommand.Execute()");

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
