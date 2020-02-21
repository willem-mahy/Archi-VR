using UnityEngine;

namespace WM.Unity
{
    /// <summary>
    /// Add this as a component to a GameObject, in order to make it be oriented to the camera always.
    /// This is done by rotating the parent GameObject around its position.
    /// </summary>
    public class BillBoard : MonoBehaviour
    {
        /// <summary>
        /// Update is called once per frame
        /// </summary>
        void Update()
        {
            // look at camera...
            transform.LookAt(Camera.main.transform.position, Vector3.up);
        }
    }
}
