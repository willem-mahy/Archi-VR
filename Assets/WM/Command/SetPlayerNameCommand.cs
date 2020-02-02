using System;
using WM.Application;

namespace WM.Command
{
    [Serializable]
    public class SetPlayerNameCommand : ICommand
    {
        #region Variables

        /// <summary>
        /// 
        /// </summary>
        public Guid PlayerID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PlayerName { get; set; }

        #endregion Variables

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SetPlayerNameCommand()
        { 
        }

        /// <summary>
        /// 
        /// </summary>
        public SetPlayerNameCommand(
            Guid playerID,
            string playerName)
        {
            PlayerID = playerID;
            PlayerName = playerName;
        }

        #endregion Constructors

        /// <summary>
        /// <see cref="ICommand.Execute(UnityApplication)"/> implementation.
        /// </summary>
        public void Execute(UnityApplication application)
        {
            WM.Logger.Debug("SetPlayerNameCommand.Execute()");

            application.SetPlayerName(PlayerID, PlayerName);
        }
    }
}