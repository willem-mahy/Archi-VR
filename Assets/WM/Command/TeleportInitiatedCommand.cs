using System;

using UnityEngine;
using WM.Application;
using WM.Net;

namespace WM.Command
{
    [Serializable]
    public class TeleportInitiatedCommand : ICommand
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public TeleportInitiatedCommand()
        { 
        }

        /// <summary>
        /// <see cref="ICommand.Execute(UnityApplication)"/> implementation.
        /// </summary>
        public void Execute(UnityApplication application)
        {
            application.Logger.Debug("ServerShutdownCommand.Execute()");

            application.InitTeleport();
        }
    }
}