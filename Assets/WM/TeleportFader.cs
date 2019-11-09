using UnityEngine;
using WM.ArchiVR;

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
