using System;
using System.Xml.Serialization;

using UnityEngine;

using WM.ArchiVR;

namespace WM.ArchiVR.Command
{
    [Serializable]
    [XmlRoot("SetImmersionModeCommand")]
    public class SetImmersionModeCommand : ICommand
    {
        [XmlElement("ImmersionMode")]
        public int ImmersionModeIndex { get; set; }

        public void Execute(ApplicationArchiVR application)
        {
            Debug.Log("SetImmersionModeCommand.Execute()");

            //if (application.RunAsServer)
            //{
            //    return;
            //}

            application.SetActiveImmersionMode(ImmersionModeIndex);
        }
    }
}
