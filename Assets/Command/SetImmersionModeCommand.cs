using ArchiVR;
using System;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets.Command
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
