using System;
using WM.Net;

namespace WM.Application
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Player
    {
        #region Variables

        /// <summary>
        /// 
        /// </summary>
        public Guid ID
        {
            get;
            private set;
        } = Guid.NewGuid();

        /// <summary>
        /// Short version of the player ID.
        /// To be used for debug logging purposes only - the short ID is NOT guaranteed to be unique!
        /// </summary>
        public String LogID
        {
            get { return WM.Net.NetUtil.ShortID(ID); }
        }

        /// <summary>
        /// The ID of the Client that is hosting this player.
        /// </summary>
        public Guid ClientID
        {
            get;
            set;
        } = Guid.Empty;

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get;
            set;
        } = "";

        /// <summary>
        /// The ID of the avatar to be used for this player.
        /// </summary>
        public Guid AvatarID
        {
            get;
            set;
        } = Guid.NewGuid();

        /// <summary>
        /// The avatar instantiation that represents this player.
        /// May be null, for example in the case of a local player in first-person application.
        /// </summary>
        [field: NonSerialized]
        public Avatar Avatar
        {
            get;
            set;
        }

        #endregion Variables

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public Player()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public Player(Player other)
        {
            ID = other.ID;
            ClientID = other.ClientID;
            Name = other.Name;
            AvatarID = other.AvatarID;            
        }

        /// <summary>
        /// 
        /// </summary>
        public Player(
            Guid id,
            Guid clientID,
            String name,
            Guid avatarID)
        {
            ID = id;
            ClientID = clientID;
            Name = name;
            AvatarID = avatarID;
        }

        #endregion Constructors
    }
}



