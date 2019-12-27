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
                        // We need to destroy ojects differently in Edit Mode, otherwise Edit Mode Unit Tests complain.  :-(
                        if (UnityEngine.Application.isEditor)
                        {
                            GameObject.DestroyImmediate(player.Avatar.gameObject);
                        }
                        else
                        {
                            GameObject.Destroy(player.Avatar.gameObject);
                        }
                    }
                }

                application.Players.Clear();
            }

            new InitNetworkCommand(NetworkMode.Standalone).Execute(application);
        }
    }
}