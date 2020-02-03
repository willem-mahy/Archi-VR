using System;

using UnityEngine;
using WM.Application;
using WM.Net;

namespace WM.Command
{
    [Serializable]
    public class ServerShutdownCommand : ICommand
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ServerShutdownCommand()
        { 
        }

        /// <summary>
        /// <see cref="ICommand.Execute(UnityApplication)"/> implementation.
        /// </summary>
        public void Execute(UnityApplication application)
        {
            WM.Logger.Debug("ServerShutdownCommand.Execute()");

            lock (application.Players)
            {
                foreach (var player in application.Players.Values)
                {
                    if (player.Avatar != null)
                    {
                        UtilUnity.Destroy(player.Avatar.gameObject);
                    }
                }

                application.Players.Clear();
            }

            new InitNetworkCommand(NetworkMode.Standalone).Execute(application);
        }
    }
}