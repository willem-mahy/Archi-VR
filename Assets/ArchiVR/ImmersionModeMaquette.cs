using ArchiVR.Command;
using UnityEngine;
using WM;
using WM.Application;
using WM.Net;

namespace ArchiVR
{
    /// <summary>
    /// In this immersion mode, the user can walk around the model,
    /// which is visualized at a 1/25 scale (The usual scale for construction model maquettes).
    /// The user can pick parts of the model with his right hand, to hide them.
    /// To unhide the entire model, the user can pick into empty space.
    /// 
    /// The Maquette can also be manipulated:
    /// 
    /// 1) rotated in the horizontal plane, around its anchor point.
    ///     - Using the left thumb Left/Right directions.
    ///     - The anchor point is predefined in the project and usually located in the middle of the construction model.
    /// 2) translated up or down along the vertical axis.
    ///     - Using the left thumb stick Up/Down directions.
    ///     - Translation is limited to a sensible range of [0, 2] meter height offset.
    ///     
    /// The user can perform any number of manipulations (rotation and/or translation)
    /// in sequence.
    /// However, the user can only perform one of manipulation at a given time.
    /// i.e. the user can either be rotating or translating the model, but not both concurrently.
    /// This is by design:
    /// We first  started off supporting concurrent translation and rotation,
    /// but it turned out very hard to position the model correctly.
    /// (because of unwanted rotations/translations being triggered
    /// by small accidental offsets on the other thumbstick axis).
    /// </summary>
    public class ImmersionModeMaquette : ImmersionMode
    {
        #region variables

        // The surroundings in which the maquette is previewed
        private GameObject m_maquettePreviewContext = null;

        // The maquette translational manipulation speed.
        float maquetteMoveSpeed = 1.0f;

        // The maquette rotational manipulation speed.
        float maquetteRotateSpeed = 60.0f;

        // The translational offset distance along the up vector.
        private float m_maquetteOffset = 0;

        // The rotational offset angle around the up vector.
        private float m_maquetteRotation = 0;

        /// <summary>
        /// The layer currently being picked.
        /// </summary>
        private GameObject pickedLayer;

        /// <summary>
        /// The index of the layer currently being picked.
        /// </summary>
        private int pickedLayerIndex = -1;

        enum MaquetteManipulationMode
        {
            None = 0,
            Translate,
            Rotate
        };

        private MaquetteManipulationMode maquetteManipulationMode = MaquetteManipulationMode.None;

        #endregion

        /// <summary>
        /// <see cref="ImmersionMode.Init()"/> implementation.
        /// </summary>
        public override void Init()
        {
            Application.Logger.Debug("ImmersionModeMaquette.Init()");

            if (m_maquettePreviewContext == null)
            {
                m_maquettePreviewContext = UtilUnity.TryFindGameObject(Application.gameObject.scene, "MaquettePreviewContext");
            }

            if (m_maquettePreviewContext)
            {
                m_maquettePreviewContext.SetActive(false);
            }
        }

        /// <summary>
        /// <see cref="ImmersionMode.Enter()"/> implementation.
        /// </summary>
        public override void Enter()
        {
            Application.Logger.Debug("ImmersionModeMaquette.Enter()");

            InitButtonMappingUI();

            if (m_maquettePreviewContext)
                m_maquettePreviewContext.SetActive(true);

            // Disable moving up/down.
            Application.m_flySpeedUpDown = 0.0f;

            // Enable only R pickray.
            Application.RPickRay.gameObject.SetActive(true);

            maquetteManipulationMode = MaquetteManipulationMode.None;
        }

        /// <summary>
        /// <see cref="ImmersionMode.Exit()"/> implementation.
        /// </summary>
        public override void Exit()
        {
            Application.Logger.Debug("ImmersionModeMaquette.Exit()");

            if (m_maquettePreviewContext)
            {
                m_maquettePreviewContext.SetActive(false);
            }

            // Restore default moving up/down.
            Application.m_flySpeedUpDown = UnityApplication.DefaultFlySpeedUpDown;

            Application.RPickRay.gameObject.SetActive(false);
        }

        /// <summary>
        /// <see cref="ImmersionMode.Update()"/> implementation.
        /// </summary>
        public override void Update()
        {
            //WM.Logger.Debug("ImmersionModeMaquette.Update()");

            if (Application.ToggleActiveProject())
            {
                return;
            }

            if (Application.ToggleImmersionModeIfInputAndNetworkModeAllows())
            {
                return;
            }

            // Clients cannot toggle model layer visibility!
            if (Application.NetworkMode != WM.Net.NetworkMode.Client)
            {
                // Toggle model layer visibility using picking.
                if (Application.m_controllerInput.m_controllerState.button8Down)
                {
                    if (pickedLayer != null)
                    {
                        Application.SetModelLayerVisible(pickedLayerIndex, !pickedLayer.activeSelf);
                        //pickedLayer.SetActive(!pickedLayer.activeSelf);
                    }
                    else
                    {
                        int layerIndex = 0;
                        foreach (var layer in Application.GetModelLayers())
                        {
                            Application.SetModelLayerVisible(layerIndex, true);
                            ++layerIndex;
                        }

                        //Application.UnhideAllModelLayers();
                    }
                }
            }

            // Show name of picked model layer in right control text.
            Application.m_rightControllerText.text = (pickedLayer == null) ? "" : pickedLayer.name;

            Application.Fly();

            #region Maquette manipulation.

            // Clients cannot manipulate model!
            if (Application.NetworkMode != WM.Net.NetworkMode.Client)
            {
                var cs = Application.m_controllerInput.m_controllerState;

                float magnitudeRotateMaquette = cs.lThumbStick.x;
                float magnitudeTranslateMaquette = cs.lThumbStick.y;

                #region Update MaquetteManipulationMode

                bool manipulating = (Mathf.Abs(magnitudeRotateMaquette) > 0.1f) || (Mathf.Abs(magnitudeTranslateMaquette) > 0.1f);
                   
                if (maquetteManipulationMode == MaquetteManipulationMode.None)
                {
                    if (manipulating)
                    {
                        maquetteManipulationMode = (Mathf.Abs(magnitudeRotateMaquette) > Mathf.Abs(magnitudeTranslateMaquette))
                            ? MaquetteManipulationMode.Rotate
                            : MaquetteManipulationMode.Translate;
                    }
                    else
                        maquetteManipulationMode = MaquetteManipulationMode.None;
                }
                else
                {
                    if (!manipulating)
                        maquetteManipulationMode = MaquetteManipulationMode.None;
                }

                #endregion

                float positionOffset = m_maquetteOffset;
                float rotationOffset = m_maquetteRotation;

                switch (maquetteManipulationMode)
                {
                    case MaquetteManipulationMode.Translate:
                        {
                            positionOffset = Mathf.Clamp(m_maquetteOffset + magnitudeTranslateMaquette * maquetteMoveSpeed * Time.deltaTime, -1.0f, 0.6f);
                        }
                        break;
                    case MaquetteManipulationMode.Rotate:
                        {
                            rotationOffset += magnitudeRotateMaquette * maquetteRotateSpeed * Time.deltaTime;
                        }
                        break;
                }

                if (maquetteManipulationMode != MaquetteManipulationMode.None)
                {
                    var command = new SetModelLocationCommand(positionOffset, rotationOffset);

                    if (Application.NetworkMode == NetworkMode.Server)
                    {
                        Application.Server.BroadcastCommand(command);
                    }
                    else
                    {
                        command.Execute(Application);
                    }
                }
            }

            #endregion

            #region Updated picked model layer

            // Clients cannot pick model layers!
            if (Application.NetworkMode != NetworkMode.Client)
            {
                var pickRay = Application.RPickRay.GetRay();

                float minHitDistance = float.NaN;

                pickedLayer = null;
                
                foreach (var layer in Application.GetModelLayers())
                {
                    PickRecursively(
                        layer,
                        pickRay,
                        layer,
                        ref pickedLayer,
                        ref minHitDistance);
                }

                if (pickedLayer == null)
                {
                    pickedLayerIndex = -1;
                }
                else
                {
                    int layerIndex = 0;
                    foreach (var layer in Application.GetModelLayers())
                    {
                        if (pickedLayer == layer)
                        {
                            pickedLayerIndex = layerIndex;
                            break;
                        }
                        ++layerIndex;
                    }
                    Debug.Assert(pickedLayerIndex != -1);
                }

                Application.RPickRay.HitDistance = minHitDistance;
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="positionOffset"></param>
        /// <param name="rotationOffset"></param>
        public void SetModelLocation(
            float positionOffset,
            float rotationOffset)
        {
            m_maquetteOffset = positionOffset;
            m_maquetteRotation = rotationOffset;
            UpdateModelLocationAndScale();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="pickRay"></param>
        /// <param name="gameObject"></param>
        /// <param name="pickedLayer"></param>
        /// <param name="minHitDistance"></param>
        private void PickRecursively(
            GameObject layer,
            Ray pickRay,
            GameObject gameObject,
            ref GameObject pickedLayer,
            ref float minHitDistance)
        {
            // Pick-test on self.
            var geometryCollider = gameObject.GetComponent<Collider>();

            if (geometryCollider)
            {
                float hitDistance = float.NaN;

                // Raycast
                RaycastHit hitInfo = new RaycastHit();

                if (geometryCollider.Raycast(pickRay, out hitInfo, 9000))
                {
                    hitDistance = hitInfo.distance;
                }

                // Bounds-check.
                /*
                if (geometryCollider.bounds.IntersectRay(pickRay, out hitDistance))
                {
                }
                else
                {
                    hitDistance = float.Nan;
                }
                */

                if (!float.IsNaN(hitDistance))
                {
                    if (float.IsNaN(minHitDistance))
                    {
                        minHitDistance = hitDistance;
                        pickedLayer = layer;
                    }
                    else if (hitDistance < minHitDistance)
                    {
                        minHitDistance = hitDistance;
                        pickedLayer = layer;
                    }
                }
            }

            // Recurse.
            foreach (Transform childTransform in gameObject.transform)
            {
                PickRecursively(
                        layer,
                        pickRay,
                        childTransform.gameObject,
                        ref pickedLayer,
                        ref minHitDistance);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void UpdateModelLocationAndScale()
        {
            //Logger.Debug("ImmersionModeMaquette.UpdateModelLocationAndScale()");

            var activeProject = Application.ActiveProject;

            if (activeProject == null)
            {
                return;
            }

            var scale = 0.04f;
            activeProject.transform.position = Vector3.zero;
            activeProject.transform.rotation = Quaternion.identity;
            activeProject.transform.localScale = scale * Vector3.one;

            // Locate around anchor.
            var modelAnchor = UtilUnity.TryFindGameObject(Application.gameObject.scene, "ModelAnchor");

            activeProject.transform.position = Application.OffsetPerID;

            if (modelAnchor != null)
            {
                activeProject.transform.position-= scale * (modelAnchor.transform.localPosition - Application.OffsetPerID);
            }

            // Add height offset.
            var pos = activeProject.transform.position;
            pos.y = 1 + m_maquetteOffset;
            activeProject.transform.position = pos;

            // Rotate it.
            activeProject.transform.RotateAround(Application.OffsetPerID, Vector3.up, m_maquetteRotation);
        }

        /// <summary>
        /// <see cref="ImmersionMode.UpdateTrackingSpacePosition()"/> implementation.
        /// </summary>
        public override void UpdateTrackingSpacePosition()
        {
            Application.Logger.Debug("ImmersionModeMaquette.UpdateTrackingSpacePosition()");

            Application.ResetTrackingSpacePosition(); // Center around model.

            if (UnityEngine.Application.isEditor)
            {
                Application.m_ovrCameraRig.transform.position = Application.m_ovrCameraRig.transform.position + new Vector3(0, 1.8f, 0);
            }
        }

        /// <summary>
        /// <see cref="ImmersionMode.InitButtonMappingUI()"/> implementation.
        /// </summary>
        public override void InitButtonMappingUI()
        {
            Application.Logger.Debug("ImmersionModeMaquette.InitButtonMappingUI()");

            var isEditor = UnityEngine.Application.isEditor;

            // Left controller
            if (Application.leftControllerButtonMapping != null)
            {
                Application.leftControllerButtonMapping.textLeftHandTrigger.text = "GFX Quality";

                Application.leftControllerButtonMapping.textLeftIndexTrigger.text = "Verander schaal" + (isEditor ? " (?)" : "");

                Application.leftControllerButtonMapping.textButtonStart.text = "Toggle menu" + (isEditor ? " (F11)" : "");

                Application.leftControllerButtonMapping.textButtonX.text = "Vorig project" + (isEditor ? " (F1)" : "");
                Application.leftControllerButtonMapping.textButtonY.text = "Volgend project" + (isEditor ? " (F2)" : "");

                Application.leftControllerButtonMapping.textLeftThumbUp.text = "Model omhoog" + (isEditor ? " (Z)" : "");
                Application.leftControllerButtonMapping.textLeftThumbDown.text = "Model omlaag" + (isEditor ? " (S)" : "");
                Application.leftControllerButtonMapping.textLeftThumbLeft.text = "Model links" + (isEditor ? " (Q)" : "");
                Application.leftControllerButtonMapping.textLeftThumbRight.text = "Model rechts" + (isEditor ? " (D)" : "");
            }

            // Right controller
            if (Application.rightControllerButtonMapping != null)
            {
                Application.rightControllerButtonMapping.textRightIndexTrigger.text = "";
                Application.rightControllerButtonMapping.textRightHandTrigger.text = "";

                Application.rightControllerButtonMapping.textButtonOculus.text = "Exit";

                Application.rightControllerButtonMapping.textButtonA.text = "";
                Application.rightControllerButtonMapping.textButtonB.text = "";

                Application.rightControllerButtonMapping.textRightThumbUp.text = "Beweeg vooruit" + (isEditor ? "(ArrowUp)" : "");
                Application.rightControllerButtonMapping.textRightThumbDown.text = "Beweeg achteruit" + (isEditor ? " (ArrowDown)" : "");
                Application.rightControllerButtonMapping.textRightThumbLeft.text = "Beweeg links" + (isEditor ? " (ArrowLeft)" : "");
                Application.rightControllerButtonMapping.textRightThumbRight.text = "Beweeg rechts" + (isEditor ? " (ArrowRight)" : "");
            }
        }
    }
}