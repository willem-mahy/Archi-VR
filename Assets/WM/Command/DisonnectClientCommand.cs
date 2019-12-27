using System;
using WM.Application;

namespace WM.Command
{
    [Serializable]
    public class DisconnectClientCommand : ICommand
    {
        #region Variables

        /// <summary>
        /// 
        /// </summary>
        public Guid ClientID { get; set; }

        #endregion Variables
    
        #region Constructors

        public DisconnectClientCommand()
        { 
        }

        public DisconnectClientCommand(
            Guid clientID)
        {
            ClientID = clientID;
        }

        #endregion Constructors

        /// <summary>
        /// <see cref="ICommand.Execute(UnityApplication)"/> implementation.
        /// </summary>
        public void Execute(UnityApplication application)
        {
            WM.Logger.Debug("DisconnectClientCommand.Execute()");

            application.RemovePlayersByClient(ClientID);
        }
    }
}