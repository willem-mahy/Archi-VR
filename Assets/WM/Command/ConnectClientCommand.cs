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

        public ConnectClientCommand()
        { 
        }

        public void Execute(UnityApplication application)
        {
            Debug.Log("ConnectClientCommand.Execute()");

            application.ConnectClient(ClientIP);
        }
    }
}