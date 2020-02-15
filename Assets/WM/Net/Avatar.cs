using System;
using UnityEngine;

namespace WM.Net
{
    public class Avatar : MonoBehaviour
    {
        /// <summary>
        /// Reference to the 'Body' Gameobject.
        /// </summary>
        public GameObject Body = null;

        /// <summary>
        /// Reference to the 'Head' Gameobject.
        /// </summary>
        public GameObject Head = null;

        /// <summary>
        /// Reference to the 'LHand' Gameobject.
        /// </summary>
        public GameObject LHand = null;

        /// <summary>
        /// Reference to the 'RHand' Gameobject.
        /// </summary>
        public GameObject RHand = null;

        /// <summary>
        /// Sets the avatar to the given state.
        /// </summary>
        /// <param name="state"></param>
        public void SetState(AvatarState state)
        {
            Head.transform.position = state.HeadPosition;
            Head.transform.rotation = state.HeadRotation;

            var offsetFromEyesToNeckBase = -0.2f * Head.transform.forward - 0.2f * Head.transform.up;
            
            var fwd = Head.transform.forward;
            fwd.y = 0;
            Body.transform.LookAt(Body.transform.position + fwd, Vector3.up);

            LHand.transform.position = state.LHandPosition;
            LHand.transform.rotation = state.LHandRotation;

            RHand.transform.position = state.RHandPosition;
            RHand.transform.rotation = state.RHandRotation;
        }

        /// <summary>
        /// Get a new AvatarState with the current state of this Avatar.
        /// </summary>
        public AvatarState GetState()
        {
            var state = new AvatarState();

            state.HeadPosition = Head.transform.position;
            state.HeadRotation = Head.transform.rotation;

            state.LHandPosition = LHand.transform.position;
            state.LHandRotation = LHand.transform.rotation;

            state.RHandPosition = RHand.transform.position;
            state.RHandRotation = RHand.transform.rotation;

            return state;
        }
    }
} // namespace WM.Net