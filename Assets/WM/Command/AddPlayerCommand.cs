using System;

using WM.Application;

namespace WM.Command
{
    [Serializable]
    public class AddPlayerCommand : ICommand
    {
        #region Variables

        /// <summary>
        /// 
        /// </summary>
        public Player Player
        {
            get;
            private set;
        }

        #endregion Variables

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public AddPlayerCommand()
        { 
        }

        /// <summary>
        /// 
        /// </summary>
        public AddPlayerCommand(Player player)
        {
            Player = player;
        }

        #endregion Constructors

        /// <summary>
        /// <see cref="ICommand.Execute(UnityApplication)"/> implementation.
        /// </summary>
        public void Execute(UnityApplication application)
        {
            WM.Logger.Debug("AddPlayerCommand.Execute()");

            application.AddPlayer(Player);
        }
    }
}