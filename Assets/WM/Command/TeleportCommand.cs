//using ArchiVR.Application;
using System;
using System.Xml.Serialization;
using WM.Application;
using WM.Command;

namespace WM.Command
{
    [Serializable]
    [XmlRoot("TeleportCommand")]
    public class TeleportCommand : ICommand
    {
        [XmlElement("ProjectIndex")]
        public int ProjectIndex { get; set; } = -1;

        [XmlElement("POIName")]
        public string POIName { get; set; } = "";

        /// <summary>
        /// <see cref="ICommand.Execute(UnityApplication)"/> implementation.
        /// </summary>
        public void Execute(UnityApplication application)
        {
            application.Logger.Debug("TeleportCommand.Execute()");

            // Hide the guidance UI for directing users to the teleport area.
            application._teleportAreaGO.SetActive(false);
            application.HudInfoPanel.SetActive(false);
            application.HudInfoText.text = "";

            // Teleport
            application.TeleportationSystem.Teleport(this);
        }
    }
}
