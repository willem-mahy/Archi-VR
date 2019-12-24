using System;
using System.Xml.Serialization;

using UnityEngine;
using WM.Application;
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
            WM.Logger.Debug("ServerShutdownCommand.Execute()");

            lock (application.remoteUsers)
            {
                foreach (var remoteUsers in application.remoteUsers.Values)
                {
                    // We need to destroy ojects defferently in Edit Mode, otherwise Edit Mode Unit Tests complain.  :-(
                    if (UnityEngine.Application.isEditor)
                    {
                        GameObject.DestroyImmediate(remoteUsers.Avatar.gameObject);
                    }
                    else
                    {
                        GameObject.Destroy(remoteUsers.Avatar.gameObject);
                    }
                }

                application.remoteUsers.Clear();
            }

            new InitNetworkCommand(NetworkMode.Standalone).Execute(application);
        }
    }
}