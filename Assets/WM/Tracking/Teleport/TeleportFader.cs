using WM.Application;
using UnityEngine;

namespace WM.Tracking.Teleport
{
    public class TeleportFader : MonoBehaviour
    {
        public UnityApplication application;

        public void OnFadeOutComplete()
        {
            application.OnTeleportFadeOutComplete();
        }

        public void OnFadeInComplete()
        {
            application.OnTeleportFadeInComplete();
        }
    }
}
