using WM.Application;
using UnityEngine;

namespace WM.Tracking.Teleport
{
    /// <summary>
    /// Note: although not referenced according to Intellisense, this is used via Unity editor setup.
    /// </summary>
    public class TeleportFader : MonoBehaviour
    {
        public UnityApplication application;

        /// <summary>
        /// Note: although not referenced according to Intellisense, this is used via Unity editor setup.
        /// </summary>
        public void OnFadeOutComplete()
        {
            application.OnTeleportFadeOutComplete();
        }

        /// <summary>
        /// Note: although not referenced according to Intellisense, this is used via Unity editor setup.
        /// </summary>
        public void OnFadeInComplete()
        {
            application.OnTeleportFadeInComplete();
        }
    }
}
