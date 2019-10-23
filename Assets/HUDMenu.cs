using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDMenu : MonoBehaviour
{
    public GameObject EyeAnchor { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        EyeAnchor = GameObject.Find("CenterEyeAnchor");
    }

    Vector3 offset;
    Quaternion rot;

    public void UpdateAnchoring()
    {
        if (AnchorEnabled)
        {
            if (EyeAnchor != null)
            {
                offset = EyeAnchor.transform.forward;
                offset.y = 0;
                offset.Normalize();
                offset *= 1.0f;
                
                var angle = Math.Atan2(EyeAnchor.transform.forward.x, EyeAnchor.transform.forward.z) * 180.0f / Math.PI;
                rot = Quaternion.AngleAxis((float)angle, Vector3.up);

                gameObject.transform.position = EyeAnchor.transform.position + offset;
                gameObject.transform.rotation = rot;
            }
        }
    }

    public bool AnchorEnabled = false;

    // Update is called once per frame
    void Update()
    {
        if (AnchorEnabled)
        {
            if (EyeAnchor != null)
            {
                gameObject.transform.position = EyeAnchor.transform.position + offset;
                gameObject.transform.rotation = rot;
            }
        }
    }
}
