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
        [XmlElement("ClientIP")]
        public string ClientIP { get; set; }

        public DisconnectClientCommand()
        { 
        }

        public DisconnectClientCommand(string clientIP)
        {
            ClientIP = clientIP;
        }

        public void Execute(UnityApplication application)
        {
            Debug.Log("DisconnectClientCommand.Execute()");

            application.DisconnectClient(ClientIP);
        }
    }
}