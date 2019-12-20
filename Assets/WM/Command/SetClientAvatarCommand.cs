using System;
using System.Xml.Serialization;

using UnityEngine;
using WM.Application;
using WM.Command;

namespace WM.Command
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

        public void Execute(UnityApplication application)
        {
            WM.Logger.Debug("SetClientAvatarCommand.Execute()");

            application.SetClientAvatar(ClientIP, AvatarIndex);
        }
    }
}