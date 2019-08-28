using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TogglePOI : MonoBehaviour
{
    public List<UnityEngine.GameObject> m_POI = new List<UnityEngine.GameObject>();
    int m_currentPOIIndex = -1;

    public int m_activeScaleIndex = 0;
    List<float> m_scales = new List<float>();

    // Start is called before the first frame update
    void Start()
    {
        m_scales.Add(1.0f);
        m_scales.Add(0.04f);

        UpdatePosition();
    }

    void UpdatePosition()
    {
        if (m_currentPOIIndex > -1)
        {
            gameObject.transform.position = m_POI[m_currentPOIIndex].transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // returns true if the "A" button is pressed on the Right Touch Controller.
        bool prev = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch);

        bool next = OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.RTouch);

        bool toggleModelScale = OVRInput.Get(OVRInput.Button.Three, OVRInput.Controller.RTouch);

        if (toggleModelScale)
        {
            m_activeScaleIndex = 1 - m_activeScaleIndex;

            var world = GameObject.Find("World");
            var scale = m_scales[m_activeScaleIndex];
            world.transform.localScale = new Vector3(scale, scale, scale);
        }

        if (next)
        {
            OffsetActivePOIIndex(+1);
        }

        if (prev)
        {
            OffsetActivePOIIndex(-1);
        }
    }

    void OffsetActivePOIIndex(int offset)
    {
        if (m_POI.Count == 0)
        {
            m_currentPOIIndex = -1;
        }
        else
        {
            m_currentPOIIndex = (m_currentPOIIndex + offset) % m_POI.Count;

            while (m_currentPOIIndex < 0)
            {
                m_currentPOIIndex += m_POI.Count;
            }
        }
        
        UpdatePosition();            
    }
}
