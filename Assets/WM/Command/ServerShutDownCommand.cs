using System;
using System.Xml.Serialization;

using UnityEngine;
using WM.Net;

namespace WM.ArchiVR.Command
{
    [Serializable]
    [XmlRoot("ServerShutdownCommand")]
    public class ServerShutdownCommand : ICommand
    {
        public ServerShutdownCommand()
        { 
        }

        public void Execute(ApplicationArchiVR application)
        {
            Debug.Log("ServerShutdownCommand.Execute()");

            lock (application.avatars)
            {
                foreach (var avatar in application.avatars.Values)
                {
                    GameObject.Destroy(avatar);
                }

                application.avatars.Clear();
            }

            new InitNetworkCommand(NetworkMode.Standalone).Execute(application);
        }
    }
}