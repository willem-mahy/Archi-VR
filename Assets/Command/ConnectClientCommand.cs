using ArchiVR;
using System;
using System.Xml.Serialization;
using UnityEngine;
using static ArchiVR.ApplicationArchiVR;

namespace Assets.Command
{
    [Serializable]
    [XmlRoot("ConnectClientCommand")]
    public class ConnectClientCommand : ICommand
    {
        [XmlElement("ClientIP")]
        public string ClientIP { get; set; }

        [XmlElement("AvatarIndex")]
        public int AvatarIndex { get; set; }

        public void Execute(ApplicationArchiVR application)
        {
            Debug.Log("ConnectClientCommand.Execute()");

            application.ConnectClient(ClientIP, AvatarIndex);
        }
    }
}
