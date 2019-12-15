using System;
using UnityEngine;

namespace WM
{
    namespace Net
    {
        public class Avatar : MonoBehaviour
        {
            public GameObject Body = null;

            public GameObject Head = null;

            public GameObject LHand = null;

            public GameObject RHand = null;

            /// <summary>
            /// Sets the avatar to the given state.
            /// </summary>
            /// <param name="state"></param>
            public void SetState(AvatarState state)
            {
                Head.transform.position = state.HeadPosition;
                Head.transform.rotation = state.HeadRotation;

                Body.transform.position = state.HeadPosition - 0.9f * Vector3.up;
                Body.transform.rotation = Quaternion.AngleAxis((float)(Math.Atan2(Head.transform.forward.x, Head.transform.forward.z)), Vector3.up);

                LHand.transform.position = state.LHandPosition;
                LHand.transform.rotation = state.LHandRotation;

                RHand.transform.position = state.RHandPosition;
                RHand.transform.rotation = state.RHandRotation;
            }
        }
    } // namespace Net
} // namespace WM
