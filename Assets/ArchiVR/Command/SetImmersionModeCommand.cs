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

        public void Execute(UnityApplication application)
        {
            Debug.Log("SetImmersionModeCommand.Execute()");

            //if (application.RunAsServer)
            //{
            //    return;
            //}

            ((ApplicationArchiVR)application).SetActiveImmersionMode(ImmersionModeIndex);
        }
    }
}
