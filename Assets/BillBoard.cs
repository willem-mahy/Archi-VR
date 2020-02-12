using UnityEngine;

public class BillBoard : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // look at camera...
        transform.LookAt(Camera.main.transform.position, Vector3.up);
    }
}
