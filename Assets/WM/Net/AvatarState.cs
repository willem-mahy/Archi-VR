
using Assets.WM.Unity;
using System;

namespace WM.Net
{
    [Serializable]
    public class AvatarState
    {
        #region Fields

        /// <summary>
        /// 
        /// </summary>
        public Guid PlayerID { get; set; } = Guid.Empty;

        /// <summary>
        /// 
        /// </summary>
        public SerializableVector3 HeadPosition { get; set; } = new SerializableVector3();

        /// <summary>
        /// 
        /// </summary>
        public SerializableQuaternion HeadRotation { get; set; } = new SerializableQuaternion();

        /// <summary>
        /// 
        /// </summary>
        public SerializableVector3 LHandPosition { get; set; } = new SerializableVector3();

        /// <summary>
        /// 
        /// </summary>
        public SerializableQuaternion LHandRotation { get; set; } = new SerializableQuaternion();

        /// <summary>
        /// 
        /// </summary>
        public SerializableVector3 RHandPosition { get; set; } = new SerializableVector3();

        /// <summary>
        /// 
        /// </summary>
        public SerializableQuaternion RHandRotation { get; set; } = new SerializableQuaternion();

        #endregion Fields

        public bool Equals(AvatarState s)
        {
            return
                PlayerID.Equals(s.PlayerID) &&
                HeadPosition.Equals(s.HeadPosition) &&
                HeadRotation.Equals(s.HeadRotation) &&
                LHandPosition.Equals(s.LHandPosition) &&
                LHandRotation.Equals(s.LHandRotation) &&
                RHandPosition.Equals(s.RHandPosition) &&
                RHandRotation.Equals(s.RHandRotation);
        }
    }
}