using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickRay : MonoBehaviour
{
    // The gameobject representing the pick ray.
    private GameObject rayGameObject = null;

    private Material rayMaterial = null;

    // The gameobject representing the pick hit.
    private GameObject hitGameObject = null;


    private float hitDistance = float.NaN;

    public float HitDistance
    {
        get
        {
            return hitDistance;
        }

        set
        {
            hitDistance = value;

            UpdateRay();
            UpdateHit();
        }
    }

    public bool Hit
    {
        get
        {
            return !float.IsNaN(hitDistance);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rayGameObject = gameObject.transform.Find("Ray").gameObject;
        rayMaterial = rayGameObject.GetComponent<Renderer>().material;

        hitGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        hitGameObject.transform.parent = gameObject.transform;
        hitGameObject.transform.localScale = 0.025f * Vector3.one;

        UpdateRay();
        UpdateHit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Color defaultColor = new Color(1,0,0,0.3f); // Default: 30% opaque red.

    public Color DefaultColor
    {
        get
        {
            return defaultColor;
        }

        set
        {
            defaultColor = value;

            UpdateRay();
        }
    }

    private Color hitColor = new Color(1, 1, 0, 0.3f); // Default: 30% opaque yellow;

    public Color HitColor
    {
        get
        {
            return hitColor;
        }

        set
        {
            hitColor = value;

            UpdateRay();
        }
    }

    private void UpdateRay()
    {
        rayMaterial.color = (Hit ? hitColor : defaultColor);
    }

    private void UpdateHit()
    {
        if (Hit)
        {
            hitGameObject.transform.position =
                gameObject.transform.position
                + hitDistance * gameObject.transform.forward;
        }

        hitGameObject.SetActive(Hit);
    }
}
