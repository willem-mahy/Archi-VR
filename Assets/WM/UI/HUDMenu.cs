using System;
using UnityEngine;
using WM;
using WM.Application;

public class HUDMenu : MonoBehaviour
{
    #region Variables

    /// <summary>
    /// The camera to which this gameobject is anchored.
    /// This gameobject will update every frame to be at the given translational offset from the anchor camera.
    /// </summary>
    public GameObject Anchor { get; set; }

    /// <summary>
    /// Translational offset from the eye anchor.
    /// X is the wanted distance of the HUD menu irt the anchor camera, along the projection of the anchor camera right direction on the horizontal plane.
    /// Y is the wanted distance of the HUD menu irt the anchor camera, along the global UP vector.
    /// Z is the wanted distance of the HUD menu irt the anchor camera, along the projection of the anchor camera forward direction on the horizontal plane.
    /// </summary>
    public Vector3 Offset = new Vector3(0.0f, -0.2f, 2.0f);

    /// <summary>
    /// Rotation.
    /// </summary>
    private Vector3 _offset;

    /// <summary>
    /// Rotation.
    /// </summary>
    private Quaternion _rotation;

    #endregion

    #region Public API

    #region GameObject overrides

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        #region Get references to GameObjects.

        if (Anchor == null)
        {
            Anchor = UtilUnity.FindGameObject(gameObject.scene, "CenterEyeAnchor");
        }

        #endregion
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        UpdateLocation();
    }

    #endregion GameObject overrides

    /// <summary>
    /// Update the effective relative translational offset, and rotation,
    /// from the current location of the anchor camera.
    /// </summary>
    public void UpdateAnchoring()
    {
        if (Anchor == null)
        {
            return;
        }

        var offsetX = Anchor.transform.right;
        offsetX.y = 0;
        offsetX.Normalize();
        offsetX *= Offset.x;

        var offsetY = new Vector3(0, Offset.y, 0);

        var offsetZ = Anchor.transform.forward;
        offsetZ.y = 0;
        offsetZ.Normalize();
        offsetZ *= Offset.z;

        _offset = offsetX + offsetY + offsetZ;

        var angle = Math.Atan2(Anchor.transform.forward.x, Anchor.transform.forward.z) * 180.0f / Math.PI;
        _rotation = Quaternion.AngleAxis((float)angle, Vector3.up);

        UpdateLocation();
    }

    #endregion Public API

    #region Non-public API

    /// <summary>
    /// 
    /// </summary>
    private void UpdateLocation()
    {
        if (Anchor == null)
        {
            return;
        }

        gameObject.transform.position = Anchor.transform.position + _offset;
        gameObject.transform.rotation = _rotation;
    }

    #endregion Non-public API
}
