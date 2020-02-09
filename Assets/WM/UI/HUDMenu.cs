using System;
using UnityEngine;
using WM;
using WM.Application;

namespace WM.UI
{
    /// <summary>
    /// 
    /// </summary>
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
        /// The composed world space translation offset.
        /// </summary>
        private Vector3 _worldSpaceTranslationOffset;

        /// <summary>
        /// The separate components for the world space translation offset.
        /// </summary>
        private Vector3 _offsetX = Vector3.zero;
        private Vector3 _offsetY = Vector3.zero;
        private Vector3 _offsetZ = Vector3.zero;

        /// <summary>
        /// Rotation.
        /// </summary>
        private Quaternion _worldSpaceRotation;

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
        /// 
        /// </summary>
        /// <param name="y"></param>
        public void UpdateOffsetY(float y)
        {
            float yAdjustment = y - Offset.y;

            Offset = Offset + (yAdjustment * Vector3.up);

            UpdateWorldSpaceTranslationOffsetY();
            UpdateWorldSpaceTranslationOffset();
        }

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

            UpdateWorldSpaceTranslationOffsetX();
            UpdateWorldSpaceTranslationOffsetY();
            UpdateWorldSpaceTranslationOffsetZ();
            UpdateWorldSpaceTranslationOffset();

            var angle = Math.Atan2(Anchor.transform.forward.x, Anchor.transform.forward.z) * 180.0f / Math.PI;
            _worldSpaceRotation = Quaternion.AngleAxis((float)angle, Vector3.up);

            UpdateLocation();
        }

        #endregion Public API

        #region Non-public API

        /// <summary>
        /// 
        /// </summary>
        private void UpdateWorldSpaceTranslationOffsetX()
        {
            _offsetX = Anchor.transform.right;
            _offsetX.y = 0;
            _offsetX.Normalize();
            _offsetX *= Offset.x;
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateWorldSpaceTranslationOffsetY()
        {
            _offsetY = new Vector3(0, Offset.y, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateWorldSpaceTranslationOffsetZ()
        {
            _offsetZ = Anchor.transform.forward;
            _offsetZ.y = 0;
            _offsetZ.Normalize();
            _offsetZ *= Offset.z;
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateWorldSpaceTranslationOffset()
        {
            _worldSpaceTranslationOffset = _offsetX + _offsetY + _offsetZ;
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateLocation()
        {
            if (Anchor == null)
            {
                return;
            }

            gameObject.transform.position = Anchor.transform.position + _worldSpaceTranslationOffset;
            gameObject.transform.rotation = _worldSpaceRotation;
        }

        #endregion Non-public API
    }
} // namespace WM.UI