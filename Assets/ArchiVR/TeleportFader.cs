using ArchiVR.Application;
using UnityEngine;

namespace ArchiVR
{
    public class TeleportFader : MonoBehaviour
    {
        public ApplicationArchiVR application;

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
