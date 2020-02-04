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
            WM.Logger.Debug("TeleportCommand.Execute()");

            application.TeleportationSystem.Teleport(this);
        }
    }
}
