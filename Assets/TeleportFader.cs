using UnityEngine;

public class TeleportFader : MonoBehaviour
{
    public ArchiVR.ApplicationArchiVR m_application = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnFadeOutComplete()
    {
        m_application.OnTeleportFadeOutComplete();
    }

    public void OnFadeInComplete()
    {
        m_application.OnTeleportFadeInComplete();
    }
}
