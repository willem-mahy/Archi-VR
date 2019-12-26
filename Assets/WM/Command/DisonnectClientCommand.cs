using System;
using System.Xml.Serialization;

using UnityEngine;
using WM.Application;

namespace WM.Command
{
    [Serializable]
    [XmlRoot("DisonnectClientCommand")]
    public class DisconnectClientCommand : ICommand
    {
        [XmlElement("ClientID")]
        public string ClientID { get; set; }

        public DisconnectClientCommand()
        { 
        }

        public DisconnectClientCommand(
            string clientID)
        {
            ClientID = clientID;
        }

        public void Execute(UnityApplication application)
        {
            WM.Logger.Debug("DisconnectClientCommand.Execute()");

            application.DisconnectClient(ClientID);
        }
    }
}