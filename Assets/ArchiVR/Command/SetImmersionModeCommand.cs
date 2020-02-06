using ArchiVR.Application;
using System;
using System.Xml.Serialization;

using UnityEngine;
using WM.Application;
using WM.Command;

namespace ArchiVR.Command
{
    [Serializable]
    [XmlRoot("SetImmersionModeCommand")]
    public class SetImmersionModeCommand : ICommand
    {
        [XmlElement("ImmersionMode")]
        public int ImmersionModeIndex { get; set; }

        /// <summary>
        /// <see cref="ICommand.Execute(UnityApplication)"/> implementation.
        /// </summary>
        public void Execute(UnityApplication application)
        {
            application.Logger.Debug("SetImmersionModeCommand.Execute()");

            var applicationArchiVR = application as ApplicationArchiVR;

            applicationArchiVR.SetActiveImmersionMode(ImmersionModeIndex);
        }
    }
}
