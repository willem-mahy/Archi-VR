using System;
using UnityEngine;
using WM;
using WM.Application;

public class HUDMenu : MonoBehaviour
{
    #region Variables

    public UnityApplication Application;

    //! Whether anchoring to the eye anchor is enabled.
    public bool AnchorEnabled = false;

    //! The eye anchor.
    public GameObject EyeAnchor { get; set; }

    //! Translational offset from the eye anchor.
    Vector3 offset;

    //! Rotation.
    Quaternion rot;

    #endregion

    #region GameObject overrides

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        #region Get references to GameObjects.

        if (Application == null)
        {
            Application = UtilUnity.FindGameObject(gameObject.scene, "Application").GetComponent<UnityApplication>();
        }

        if (EyeAnchor == null)
        {
            EyeAnchor = UtilUnity.FindGameObject(gameObject.scene, "CenterEyeAnchor");
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        //WM.Logger.Warning("HudMenu.OnEnable()");

        if (Application)
        {
            Application.AddSelectionTarget(gameObject);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        //WM.Logger.Warning("HudMenu.OnDisable()");

        if (Application)
        {
            Application.RemoveSelectionTarget(gameObject);
        }
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
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
