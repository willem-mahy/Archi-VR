using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WM;
using WM.ArchiVR;

public class HUDMenu : MonoBehaviour
{
    #region Variables

    public ApplicationArchiVR ApplicationArchiVR;

    //! WHether anchoring to the eye anchor is enabled.
    public bool AnchorEnabled = false;

    //! The eye anchor.
    public GameObject EyeAnchor { get; set; }

    //! Translational offset from the eye anchor.
    Vector3 offset;

    //! Rotation.
    Quaternion rot;

    #endregion

    #region GameObject overrides

    // Start is called before the first frame update
    void Start()
    {
        #region Get references to GameObjects.

        ApplicationArchiVR = UtilUnity.TryFindGameObject("Application").GetComponent<ApplicationArchiVR>();

        EyeAnchor = UtilUnity.TryFindGameObject("CenterEyeAnchor");

        #endregion
    }

    void OnEnable()
    {
        WM.Logger.Warning("HudMenu.OnEnable()");
        if (ApplicationArchiVR)
        {
            ApplicationArchiVR.AddSelectionTarget(gameObject);
        }
    }

    void OnDisable()
    {
        WM.Logger.Warning("HudMenu.OnDisable()");
        if (ApplicationArchiVR)
        {
            ApplicationArchiVR.RemoveSelectionTarget(gameObject);
        }
    }

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

    #endregion

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
}
