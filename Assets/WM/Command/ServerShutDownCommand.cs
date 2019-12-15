using System;
using System.Xml.Serialization;

using UnityEngine;
using WM.Application;
using WM.Command;
using WM.Net;

namespace WM.Command
{
    [Serializable]
    [XmlRoot("ServerShutdownCommand")]
    public class ServerShutdownCommand : ICommand
    {
        public ServerShutdownCommand()
        { 
        }

        public void Execute(UnityApplication application)
        {
            Debug.Log("ServerShutdownCommand.Execute()");

            lock (application.remoteUsers)
            {
                foreach (var remoteUsers in application.remoteUsers.Values)
                {
                    GameObject.Destroy(remoteUsers.Avatar.gameObject);
                }

                application.remoteUsers.Clear();
            }

            new InitNetworkCommand(NetworkMode.Standalone).Execute(application);
        }
    }
}