using ArchiVR.Application;
using ArchiVR.Net;
using NUnit.Framework;
using System.Threading;
using UnityEngine;
using WM.Command;
using WM.Net;

namespace Tests
{
    class MockFactory
    {
        /// <summary>
        /// Creates a GameObject initialized with an Avatar behavior.
        /// </summary>
        /// <param name="name">The name for the GameObject.</param>
        /// <returns></returns>
        public static GameObject CreateAvatarGameObject(string name)
        {
            var avatarGO = new GameObject(name);

            var avatar = avatarGO.AddComponent(typeof(WM.Net.Avatar)) as WM.Net.Avatar;

            avatar.Head = new GameObject("Avatar Head");
            avatar.Head.transform.SetParent(avatarGO.transform);

            avatar.Body = new GameObject("Avatar Body");
            avatar.Body.transform.SetParent(avatarGO.transform);

            avatar.LHand = new GameObject("Avatar Left Hand");
            avatar.LHand.transform.SetParent(avatarGO.transform);

            avatar.RHand = new GameObject("Avatar Right Hand");
            avatar.RHand.transform.SetParent(avatarGO.transform);

            return avatarGO;
        }
    }

    class Log
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="caption"></param>
        public static void Header(string caption)
        {
            WM.Logger.Debug("");
            WM.Logger.Debug("=======[" + caption + "]===============================");
        }
    }

    public class TestUtil
    {     
        [Test]
        public void XXX()
        {
        }
    }
}