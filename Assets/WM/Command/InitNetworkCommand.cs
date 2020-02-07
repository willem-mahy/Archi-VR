using System;
using WM.Application;
using WM.Net;

namespace WM.Command
{
    [Serializable]
    public class InitNetworkCommand : ICommand
    {
        /// <summary>
        /// The network mode to initialize the application into.
        /// </summary>
        public NetworkMode NetworkMode { get; set; }

        /// <summary>
        /// Parametrized constructor.
        /// </summary>
        /// <param name="networkMode">The network mode to initialize the application into.</param>
        public InitNetworkCommand(NetworkMode networkMode)
        {
            NetworkMode = networkMode;
        }

        /// <summary>
        /// <see cref="ICommand.Execute(UnityApplication)"/> implementation.
        /// </summary>
        /// <param name="application"></param>
        public void Execute(UnityApplication application)
        {
            application.Logger.Debug("InitNetworkCommand.Execute(): NetworkMode = " + NetworkMode);

            application.InitNetworkMode(NetworkMode);
        }
    }
}
