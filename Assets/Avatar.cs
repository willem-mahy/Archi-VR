using UnityEngine;
using WM;

public class IAvatarController
{
    public virtual void Update(GameObject avatarGameObject)
    { }
}

public class AvatarControllerUDP : IAvatarController
{
    TrackerClient trackerClient = null;

    public override void Update(GameObject avatarGameObject)
    {
        if (trackerClient == null)
        {
            trackerClient = new TrackerClient(new WM.ILogger());
            trackerClient.Start();
        }

        trackerClient.UpdatePosition(avatarGameObject);
    }
}

public class AvatarControllerMock : IAvatarController
{
    float alfa = 0;

    public override void Update(GameObject avatarGameObject)
    {
        alfa += 0.2f;

        var rot = Quaternion.Euler(new Vector3(0, alfa, 0));

        avatarGameObject.transform.position = rot * (1.0f * Vector3.left);
        avatarGameObject.transform.rotation = rot;
    }
}



public class Avatar : MonoBehaviour
{
    public GameObject Body = null;

    public GameObject Head = null;

    public GameObject LHand = null;

    public GameObject RHand = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
