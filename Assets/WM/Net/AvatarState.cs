
using Assets.WM.Unity;
using System;

namespace WM.Net
{
    [Serializable]
    public class AvatarState
    {
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
    }
}