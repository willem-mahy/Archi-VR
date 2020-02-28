using ArchiVR.Command;
using UnityEngine;
using WM;
using WM.Application;
using WM.Net;
using ArchiVR.Application;

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
    public class ImmersionModeMaquette : ApplicationState<ApplicationArchiVR>
    {
        #region variables

        // The surroundings in which the maquette is previewed
        private GameObject m_maquettePreviewContext;

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

        public ImmersionModeMaquette(ApplicationArchiVR application) : base(application)
        {
        }

        /// <summary>
        /// <see cref="ImmersionMode.Init()"/> implementation.
        /// </summary>
        public override void Init()
        {
            m_application.Logger.Debug("ImmersionModeMaquette.Init()");

            if (m_maquettePreviewContext == null)
            {
                m_maquettePreviewContext = UtilUnity.FindGameObjectElseError(m_application.gameObject.scene, "MaquettePreviewContext");
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
            m_application.Logger.Debug("ImmersionModeMaquette.Enter()");

            InitButtonMappingUI();

            if (m_maquettePreviewContext)
                m_maquettePreviewContext.SetActive(true);

            // Disable moving up/down.
            m_application.m_flySpeedUpDown = 0.0f;

            // Enable only R pickray.
            m_application.RPickRay.gameObject.SetActive(true);

            maquetteManipulationMode = MaquetteManipulationMode.None;

            UpdateModelLocationAndScale();
            UpdateTrackingSpacePosition();
        }

        /// <summary>
        /// <see cref="ImmersionMode.Exit()"/> implementation.
        /// </summary>
        public override void Exit()
        {
            m_application.Logger.Debug("ImmersionModeMaquette.Exit()");

            if (m_maquettePreviewContext)
            {
                m_maquettePreviewContext.SetActive(false);
            }

            // Restore default moving up/down.
            m_application.m_flySpeedUpDown = UnityApplication.DefaultFlySpeedUpDown;

            m_application.RPickRay.gameObject.SetActive(false);
        }

        /// <summary>
        /// <see cref="ImmersionMode.Update()"/> implementation.
        /// </summary>
        public override void Update()
        {
            //WM.Logger.Debug("ImmersionModeMaquette.Update()");

            if (m_application.ToggleActiveProject())
            {
                return;
            }

            //if (m_application.ToggleImmersionModeIfInputAndNetworkModeAllows())
            //{
            //    return;
            //}

            // Clients cannot toggle model layer visibility!
            if (m_application.NetworkMode != WM.Net.NetworkMode.Client)
            {
                // Toggle model layer visibility using picking.
                if (m_application.m_controllerInput.m_controllerState.rIndexTriggerDown)
                {
                    if (pickedLayer != null)
                    {
                        m_application.SetModelLayerVisible(pickedLayerIndex, !pickedLayer.activeSelf);
                        //pickedLayer.SetActive(!pickedLayer.activeSelf);
                    }
                    else
                    {
                        int layerIndex = 0;
                        foreach (var layer in m_application.GetModelLayers())
                        {
                            m_application.SetModelLayerVisible(layerIndex, true);
                            ++layerIndex;
                        }

                        //Application.UnhideAllModelLayers();
                    }
                }
            }

            // Show name of picked model layer in right control text.
            m_application.m_rightControllerText.text = (pickedLayer == null) ? "" : pickedLayer.name;

            m_application.Fly();

            #region Maquette manipulation.

            // Clients cannot manipulate model!
            if (m_application.NetworkMode != WM.Net.NetworkMode.Client)
            {
                var cs = m_application.m_controllerInput.m_controllerState;

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

                    if (m_application.NetworkMode == NetworkMode.Server)
                    {
                        m_application.Server.BroadcastCommand(command);
                    }
                    else
                    {
                        command.Execute(m_application);
                    }
                }
            }

            #endregion

            #region Updated picked model layer

            // Clients cannot pick model layers!
            if (m_application.NetworkMode != NetworkMode.Client)
            {
                var pickRay = m_application.RPickRay.GetRay();

                var hitInfo = new RaycastHit();
                hitInfo.distance = float.NaN;

                pickedLayer = null;
                
                foreach (var layer in m_application.GetModelLayers())
                {
                    UtilUnity.PickRecursively(
                        layer.Model,
                        pickRay,
                        layer.Model,
                        ref pickedLayer,
                        ref hitInfo);
                }

                if (pickedLayer == null)
                {
                    pickedLayerIndex = -1;
                }
                else
                {
                    int layerIndex = 0;
                    foreach (var layer in m_application.GetModelLayers())
                    {
                        if (pickedLayer == layer.Model)
                        {
                            pickedLayerIndex = layerIndex;
                            break;
                        }
                        ++layerIndex;
                    }
                    Debug.Assert(pickedLayerIndex != -1);
                }

                m_application.RPickRay.HitDistance = hitInfo.distance;
            }

            #endregion

            var controllerState = m_application.m_controllerInput.m_controllerState;

            // Pressing 'BackSpace' on the keyboard is a shortcut for returning to the default state.
            var returnToDefaultState = controllerState.lIndexTriggerDown || Input.GetKeyDown(KeyCode.Backspace);

            if (returnToDefaultState)
            {
                m_application.PopApplicationState();
            }
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
        public override void UpdateModelLocationAndScale()
        {
            //Logger.Debug("ImmersionModeMaquette.UpdateModelLocationAndScale()");

            var activeProject = m_application.ActiveProject;

            if (activeProject == null)
            {
                return;
            }

            var scale = 0.04f;
            activeProject.transform.position = Vector3.zero;
            activeProject.transform.rotation = Quaternion.identity;
            activeProject.transform.localScale = scale * Vector3.one;

            activeProject.transform.position = m_application.OffsetPerID;

            // Locate around anchor.
            {
                var modelAnchor = UtilUnity.FindGameObjectElseWarn(
                    m_application.gameObject.scene,
                    "ModelAnchor",
                    m_application.Logger);

                if (modelAnchor != null)
                {
                    activeProject.transform.position -= scale * (modelAnchor.transform.localPosition - m_application.OffsetPerID);
                }
            }

            // Add height offset.
            var pos = activeProject.transform.position;
            pos.y = 1 + m_maquetteOffset;
            activeProject.transform.position = pos;

            // Rotate it.
            activeProject.transform.RotateAround(m_application.OffsetPerID, Vector3.up, m_maquetteRotation);
        }

        /// <summary>
        /// <see cref="ImmersionMode.UpdateTrackingSpacePosition()"/> implementation.
        /// </summary>
        public override void UpdateTrackingSpacePosition()
        {
            m_application.Logger.Debug("ImmersionModeMaquette.UpdateTrackingSpacePosition()");

            m_application.ResetTrackingSpacePosition(); // Center around model.

            if (UnityEngine.Application.isEditor)
            {
                m_application.m_ovrCameraRig.transform.position = m_application.m_ovrCameraRig.transform.position + new Vector3(0, 1.8f, 0);
            }
        }

        /// <summary>
        /// <see cref="ImmersionMode.InitButtonMappingUI()"/> implementation.
        /// </summary>
        public /*override*/ void InitButtonMappingUI()
        {
            m_application.Logger.Debug("ImmersionModeMaquette.InitButtonMappingUI()");

            var isEditor = UnityEngine.Application.isEditor;

            // Left controller
            if (m_application.leftControllerButtonMapping != null)
            {
                m_application.leftControllerButtonMapping.HandTrigger.Text = "GFX Quality";

                m_application.leftControllerButtonMapping.IndexTrigger.Text = "Verander schaal" + (isEditor ? " (?)" : "");

                m_application.leftControllerButtonMapping.ButtonStart.Text = "Toggle menu" + (isEditor ? " (F11)" : "");

                m_application.leftControllerButtonMapping.ButtonX.Text = "Vorig project" + (isEditor ? " (F1)" : "");
                m_application.leftControllerButtonMapping.ButtonY.Text = "Volgend project" + (isEditor ? " (F2)" : "");

                m_application.leftControllerButtonMapping.ThumbUp.Text = "Model omhoog" + (isEditor ? " (Z)" : "");
                m_application.leftControllerButtonMapping.ThumbDown.Text = "Model omlaag" + (isEditor ? " (S)" : "");
                m_application.leftControllerButtonMapping.ThumbLeft.Text = "Model links" + (isEditor ? " (Q)" : "");
                m_application.leftControllerButtonMapping.ThumbRight.Text = "Model rechts" + (isEditor ? " (D)" : "");
            }

            // Right controller
            if (m_application.rightControllerButtonMapping != null)
            {
                m_application.rightControllerButtonMapping.IndexTrigger.Text = "";
                m_application.rightControllerButtonMapping.HandTrigger.Text = "";

                m_application.rightControllerButtonMapping.ButtonOculusStart.Text = "Exit";

                m_application.rightControllerButtonMapping.ButtonXA.Text = "";
                m_application.rightControllerButtonMapping.ButtonYB.Text = "";

                m_application.rightControllerButtonMapping.ThumbUp.Text = "Beweeg vooruit" + (isEditor ? "(ArrowUp)" : "");
                m_application.rightControllerButtonMapping.ThumbDown.Text = "Beweeg achteruit" + (isEditor ? " (ArrowDown)" : "");
                m_application.rightControllerButtonMapping.ThumbLeft.Text = "Beweeg links" + (isEditor ? " (ArrowLeft)" : "");
                m_application.rightControllerButtonMapping.ThumbRight.Text = "Beweeg rechts" + (isEditor ? " (ArrowRight)" : "");
            }
        }
    }
}