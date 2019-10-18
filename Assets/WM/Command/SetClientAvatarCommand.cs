using System;
using System.Xml.Serialization;

using UnityEngine;

using WM.ArchiVR;

namespace WM.ArchiVR.Command
{
    [Serializable]
    [XmlRoot("SetClientAvatarCommand")]
    public class SetClientAvatarCommand : ICommand
    {
        [XmlElement("ClientIP")]
        public string ClientIP { get; set; }

        [XmlElement("AvatarIndex")]
        public int AvatarIndex { get; set; }

        public void Execute(ApplicationArchiVR application)
        {
            Debug.Log("SetClientAvatarCommand.Execute()");

            application.SetClientAvatar(ClientIP, AvatarIndex);
        }
    }
}