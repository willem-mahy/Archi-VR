using System;
using WM.Application;

namespace WM.Command
{
    [Serializable]
    public class SetPlayerAvatarCommand : ICommand
    {
        #region Variables

        /// <summary>
        /// 
        /// </summary>
        public Guid PlayerID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid AvatarID { get; set; }

        #endregion Variables

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SetPlayerAvatarCommand()
        {
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientIP"></param>
        /// <param name="clientPort"></param>
        /// <param name="avatarID"></param>
        public SetPlayerAvatarCommand(
            Guid playerID,
            Guid avatarID)
        {
            PlayerID = playerID;
            AvatarID = avatarID;
        }

        #endregion Constructors

        /// <summary>
        /// <see cref="ICommand.Execute(UnityApplication)"/> implementation.
        /// </summary>
        public void Execute(UnityApplication application)
        {
            WM.Logger.Debug("SetClientAvatarCommand.Execute()");

            application.SetPlayerAvatar(PlayerID, AvatarID);
        }
    }
}