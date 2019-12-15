using WM.Net;

namespace WM
{
    public class RemoteUser
    {
        /// <summary>
        /// The IP of the remote user.
        /// </summary>
        public string remoteIP;

        /// <summary>
        /// The index of the user's avatar type.
        /// </summary>
        public int AvatarIndex = 0;

        /// <summary>
        /// The user name.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// 
        /// </summary>
        public Avatar Avatar;
    }
}
