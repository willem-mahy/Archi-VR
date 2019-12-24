using ArchiVR.Application;
using System;
using System.Xml.Serialization;
using WM.Application;
using WM.Command;

namespace ArchiVR.Command
{
    [Serializable]
    [XmlRoot("TeleportCommand")]
    public class TeleportCommand : ICommand
    {
        [XmlElement("ProjectIndex")]
        public int ProjectIndex { get; set; } = -1;

        [XmlElement("POIName")]
        public string POIName { get; set; } = "";

        public void Execute(UnityApplication application)
        {
            WM.Logger.Debug("TeleportCommand.Execute()");

            var applicationArchiVR = (ApplicationArchiVR)application;

            if ((applicationArchiVR.ActiveProjectIndex == ProjectIndex) && (applicationArchiVR.ActivePOIName == POIName))
            {
                return;
            }

            applicationArchiVR.TeleportCommand = this;

            if (applicationArchiVR.m_fadeAnimator != null)
            {
                applicationArchiVR.SetActiveApplicationState(UnityApplication.ApplicationStates.Teleporting);
            }
            else
            {
                applicationArchiVR.Teleport();
            }
        }
    }
}
