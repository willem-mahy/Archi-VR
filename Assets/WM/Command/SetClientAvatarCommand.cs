using System;
using System.Xml.Serialization;

using UnityEngine;

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

        public SetClientAvatarCommand()
        {
        }
        
        public SetClientAvatarCommand(
            string clientIP,
            int avatarIndex)
        {
            ClientIP = clientIP;
            AvatarIndex = avatarIndex;
        }

        public void Execute(ApplicationArchiVR application)
        {
            Debug.Log("SetClientAvatarCommand.Execute()");

            application.SetClientAvatar(ClientIP, AvatarIndex);
        }
    }
}