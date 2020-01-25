using Assets.WM.Unity;
using NUnit.Framework;
using System;
using UnityEngine;
using WM.Net;

namespace Tests
{
    public class TestAvatar
    {
        [Test]
        public void Test_AvatarState_DefaultConstruction()
        {
            var avatarState = new AvatarState();
            
            Assert.IsNotNull(avatarState.PlayerID);
            Assert.IsTrue(avatarState.HeadPosition.Equals(new SerializableVector3()));
            Assert.IsTrue(avatarState.HeadRotation.Equals(new SerializableQuaternion()));
            Assert.IsTrue(avatarState.LHandPosition.Equals(new SerializableVector3()));
            Assert.IsTrue(avatarState.LHandRotation.Equals(new SerializableQuaternion()));
            Assert.IsTrue(avatarState.RHandPosition.Equals(new SerializableVector3()));
            Assert.IsTrue(avatarState.RHandRotation.Equals(new SerializableQuaternion()));
        }

        [Test]
        public void Test_AvatarState_Equals()
        {
            #region Create AvatarStates

            var avatarState = new AvatarState();

            var avatarStateB = new AvatarState();
            
            var avatarState1 = new AvatarState();
            avatarState1.HeadPosition = new Vector3(1, 2, 3);
            avatarState1.HeadRotation = new Quaternion(0, 0, 0, 0);
            avatarState1.LHandPosition = new Vector3(0, 0, 0);
            avatarState1.LHandRotation = new Quaternion(0, 0, 0, 0);
            avatarState1.RHandPosition = new Vector3(0, 0, 0);
            avatarState1.RHandRotation = new Quaternion(0, 0, 0, 0);

            var avatarState2 = new AvatarState();
            avatarState2.PlayerID = Guid.NewGuid();
            avatarState2.HeadPosition = new Vector3(0, 0, 0);
            avatarState2.HeadRotation = new Quaternion(1, 2, 3, 4);
            avatarState2.LHandPosition = new Vector3(0, 0, 0);
            avatarState2.LHandRotation = new Quaternion(0, 0, 0, 0);
            avatarState2.RHandPosition = new Vector3(0, 0, 0);
            avatarState2.RHandRotation = new Quaternion(0, 0, 0, 0);

            var avatarState3 = new AvatarState();
            avatarState3.PlayerID = Guid.NewGuid();
            avatarState3.HeadPosition = new Vector3(0, 0, 0);
            avatarState3.HeadRotation = new Quaternion(0, 0, 0, 0);
            avatarState3.LHandPosition = new Vector3(1, 2, 3);
            avatarState3.LHandRotation = new Quaternion(0, 0, 0, 0);
            avatarState3.RHandPosition = new Vector3(0, 0, 0);
            avatarState3.RHandRotation = new Quaternion(0, 0, 0, 0);

            var avatarState4 = new AvatarState();
            avatarState4.PlayerID = Guid.NewGuid();
            avatarState4.HeadPosition = new Vector3(0, 0, 0);
            avatarState4.HeadRotation = new Quaternion(0, 0, 0, 0);
            avatarState4.LHandPosition = new Vector3(0, 0, 0);
            avatarState4.LHandRotation = new Quaternion(1, 2, 3, 4);
            avatarState4.RHandPosition = new Vector3(0, 0, 0);
            avatarState4.RHandRotation = new Quaternion(0, 0, 0, 0);

            var avatarState5 = new AvatarState();
            avatarState5.PlayerID = Guid.NewGuid();
            avatarState5.HeadPosition = new Vector3(0, 0, 0);
            avatarState5.HeadRotation = new Quaternion(0, 0, 0, 0);
            avatarState5.LHandPosition = new Vector3(0, 0, 0);
            avatarState5.LHandRotation = new Quaternion(0, 0, 0, 0);
            avatarState5.RHandPosition = new Vector3(1, 2, 3);
            avatarState5.RHandRotation = new Quaternion(0, 0, 0, 0);

            var avatarState6 = new AvatarState();
            avatarState6.PlayerID = Guid.NewGuid();
            avatarState6.HeadPosition = new Vector3(0, 0, 0);
            avatarState6.HeadRotation = new Quaternion(0, 0, 0, 0);
            avatarState6.LHandPosition = new Vector3(0, 0, 0);
            avatarState6.LHandRotation = new Quaternion(0, 0, 0, 0);
            avatarState6.RHandPosition = new Vector3(0, 0, 0);
            avatarState6.RHandRotation = new Quaternion(1, 2, 3, 4);

            #endregion Create all-unique AvatarStates

            // Test that each avatar state equals itself.
            Assert.IsTrue(avatarState.Equals(avatarState));
            Assert.IsTrue(avatarState.Equals(avatarStateB));
            Assert.IsTrue(avatarState1.Equals(avatarState1));
            Assert.IsTrue(avatarState2.Equals(avatarState2));
            Assert.IsTrue(avatarState3.Equals(avatarState3));
            Assert.IsTrue(avatarState4.Equals(avatarState4));
            Assert.IsTrue(avatarState5.Equals(avatarState5));
            Assert.IsTrue(avatarState6.Equals(avatarState6));

            // Test that the 1st avatar does not equal any other avatar state.
            Assert.IsFalse(avatarState.Equals(avatarState1));
            Assert.IsFalse(avatarState.Equals(avatarState2));
            Assert.IsFalse(avatarState.Equals(avatarState3));
            Assert.IsFalse(avatarState.Equals(avatarState4));
            Assert.IsFalse(avatarState.Equals(avatarState5));
            Assert.IsFalse(avatarState.Equals(avatarState6));
        }

        [Test]
        public void Test_Avatar_SetState()
        {
            var avatarGO = MockFactory.CreateAvatarGameObject("MyAvatar");
            var avatar = avatarGO.GetComponent<WM.Net.Avatar>();

            var avatarState1 = new AvatarState();
            avatarState1.HeadPosition = new Vector3(1, 2, 3);
            avatar.SetState(avatarState1);
            Assert.IsTrue(avatar.GetState().Equals(avatarState1));

            var avatarState2 = new AvatarState();
            avatarState2.HeadPosition = new Vector3(4, 5, 6);
            avatar.SetState(avatarState2);
            Assert.IsTrue(avatar.GetState().Equals(avatarState2));

            var avatarState3 = new AvatarState();
            avatarState2.HeadPosition = new Vector3(7, 8, 9);
            avatar.SetState(avatarState3);
            Assert.IsTrue(avatar.GetState().Equals(avatarState3));
        }
    }
}
