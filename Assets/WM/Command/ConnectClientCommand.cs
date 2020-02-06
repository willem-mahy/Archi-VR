using System;

using WM.Application;

namespace WM.Command
{
    [Serializable]
    public class ConnectClientCommand : ICommand
    {
        #region Variables

        /// <summary>
        /// 
        /// </summary>
        public Guid ClientID
        {
            get;
            private set;
        }

        #endregion Variables

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public ConnectClientCommand()
        { 
        }

        /// <summary>
        /// 
        /// </summary>
        public ConnectClientCommand(Guid clientID)
        {
            ClientID = clientID;
        }

        #endregion Constructors

        /// <summary>
        /// <see cref="ICommand.Execute(UnityApplication)"/> implementation.
        /// </summary>
        public void Execute(UnityApplication application)
        {
            application.Logger.Debug("ConnectClientCommand.Execute()");
        }
    }
}