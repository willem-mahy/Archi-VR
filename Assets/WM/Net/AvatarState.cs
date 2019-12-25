
using Assets.WM.Unity;
using System;

namespace WM.Net
{
    [Serializable]
    public class AvatarState
    {
        public string ClientIP { get; set; } = "";

        public SerializableVector3 HeadPosition { get; set; } = new SerializableVector3();

        public SerializableQuaternion HeadRotation { get; set; } = new SerializableQuaternion();

        public SerializableVector3 LHandPosition { get; set; } = new SerializableVector3();

        public SerializableQuaternion LHandRotation { get; set; } = new SerializableQuaternion();

        public SerializableVector3 RHandPosition { get; set; } = new SerializableVector3();

        public SerializableQuaternion RHandRotation { get; set; } = new SerializableQuaternion();
    }
}