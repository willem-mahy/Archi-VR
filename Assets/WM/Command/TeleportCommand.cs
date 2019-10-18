using System;
using System.Xml.Serialization;
using UnityEngine;

using WM.ArchiVR;

namespace WM.ArchiVR.Command
{
    [Serializable]
    [XmlRoot("TeleportCommand")]
    public class TeleportCommand : ICommand
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

            if ((application.ActiveProjectIndex == ProjectIndex) && (application.ActivePOIName == POIName))
            {
                return;
            }

            application.TeleportCommand = this;

            application.SetActiveApplicationState(WM.ArchiVR.ApplicationArchiVR.ApplicationStates.Teleporting);
        }
    }
}
