using System;
using System.Xml.Serialization;

using UnityEngine;
using WM.Application;

namespace WM.Command
{
    [Serializable]
    [XmlRoot("ConnectClientCommand")]
    public class ConnectClientCommand : ICommand
    {
        [XmlElement("ClientIP")]
        public string ClientIP { get; set; }

        [XmlElement("ClientPort")]
        public int ClientPort { get; set; }

        public ConnectClientCommand()
        { 
        }

        public void Execute(UnityApplication application)
        {
            WM.Logger.Debug("ConnectClientCommand.Execute()");

            application.ConnectClient(ClientIP, ClientPort);
        }
    }
}