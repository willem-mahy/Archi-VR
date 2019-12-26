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

        [XmlElement("ClientPort")]
        public int ClientPort { get; set; }

        [XmlElement("AvatarID")]
        public Guid AvatarID { get; set; }

        public SetClientAvatarCommand()
        {
        }
        
        public SetClientAvatarCommand(
            string clientIP,
            int clientPort,
            Guid avatarID)
        {
            ClientIP = clientIP;
            ClientPort = clientPort;
            AvatarID = avatarID;
        }

        public void Execute(UnityApplication application)
        {
            WM.Logger.Debug("SetClientAvatarCommand.Execute()");

            application.SetClientAvatar(ClientIP + ":" + ClientPort, AvatarID);
        }
    }
}