using ArchiVR.Command;
using UnityEngine;
using WM;
using WM.Application;
using WM.Net;
using ArchiVR.Application;

namespace ArchiVR
{
    /// <summary>
    /// In this immersion mode, the project is visualized as a scale model at a 1/25 scale.
    /// This usual scale for scale models in construction.
    /// 
    /// The user can hide and unhide model layers using picking:
    /// * pick a visible model layer to hide it
    /// * pick into 'nothing' to unhide all model layers.
    /// 
    /// The user can also manipulate the location of the scale model:
    /// 
    /// 1) rotate in the horizontal plane, around its anchor point.
    ///     - Using the left thumb Left/Right directions.
    ///     - The anchor point is predefined in the project and usually located in the middle of the construction model.
    /// 2) translate up or down along the vertical axis.
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
    public class ApplicationStateScaleModel : ApplicationState<ApplicationArchiVR>
    {
        #region variables

        /// <summary>
        /// The translational model manipulation speed.
        /// </summary>
        private float _modelMoveSpeed = 1.0f;

        /// <summary>
        /// The rotational model manipulation speed.
        /// </summary>
        private float _modelRotateSpeed = 60.0f;

        /// <summary>
        /// The translational offset distance of the model along the up vector.
        /// </summary>
        private float _modelOffset = 0;

        /// <summary>
        /// The rotational offset angle of the model around the up vector.
        /// </summary>
        private float _modelRotation = 0;

        /// <summary>
        /// The model layer currently being picked.
        /// </summary>
        private GameObject pickedLayer;

        /// <summary>
        /// The index of the model layer currently being picked.
        /// </summary>
        private int pickedLayerIndex = -1;

        enum ModelManipulationMode
        {
            None = 0,
            Translate,
            Rotate
        };

        private ModelManipulationMode maquetteManipulationMode = ModelManipulationMode.None;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="application"></param>
        public ApplicationStateScaleModel(ApplicationArchiVR application) : base(application)
        {
        }

        #endregion Constructors

        /// <summary>
        /// <see cref="ApplicationState{T}.Init()"/> implementation.
        /// </summary>
        public override void Init()
        {
            m_application.Logger.Debug("ApplicationStateScaleModel.Init()");
        }

        /// <summary>
        /// <see cref="ApplicationState{T}.Enter()"/> implementation.
        /// </summary>
        public override void Enter()
        {
            m_application.Logger.Debug("ApplicationStateScaleModel.Enter()");

            m_application.MaquettePreviewContext.SetActive(true);

            // Disable moving up/down.
            m_application.m_flySpeedUpDown = 0.0f;

            // Enable only R pickray.
            m_application.RPickRay.gameObject.SetActive(true);

            maquetteManipulationMode = ModelManipulationMode.None;

            UpdateModelLocationAndScale();
            UpdateTrackingSpacePosition();
        }

        /// <summary>
        /// <see cref="ApplicationState{T}.Exit()"/> implementation.
        /// </summary>
        public override void Exit()
        {
            m_application.Logger.Debug("ApplicationStateScaleModel.Exit()");

            m_application.MaquettePreviewContext.SetActive(false);

            // Restore default moving up/down.
            m_application.m_flySpeedUpDown = UnityApplication.DefaultFlySpeedUpDown;

            m_application.RPickRay.gameObject.SetActive(false);
        }

        /// <summary>
        /// <see cref="ApplicationState{T}.Update()"/> implementation.
        /// </summary>
        public override void Update()
        {
            //WM.Logger.Debug("ApplicationStateScaleModel.Update()");

            UpdateControllerUI();

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
                    }
                    else
                    {
                        m_application.UnhideAllModelLayers();
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

                if (maquetteManipulationMode == ModelManipulationMode.None)
                {
                    if (manipulating)
                    {
                        maquetteManipulationMode = (Mathf.Abs(magnitudeRotateMaquette) > Mathf.Abs(magnitudeTranslateMaquette))
                            ? ModelManipulationMode.Rotate
                            : ModelManipulationMode.Translate;
                    }
                    else
                        maquetteManipulationMode = ModelManipulationMode.None;
                }
                else
                {
                    if (!manipulating)
                        maquetteManipulationMode = ModelManipulationMode.None;
                }

                #endregion

                float positionOffset = _modelOffset;
                float rotationOffset = _modelRotation;

                switch (maquetteManipulationMode)
                {
                    case ModelManipulationMode.Translate:
                        {
                            positionOffset = Mathf.Clamp(_modelOffset + magnitudeTranslateMaquette * _modelMoveSpeed * Time.deltaTime, -1.0f, 0.6f);
                        }
                        break;
                    case ModelManipulationMode.Rotate:
                        {
                            rotationOffset += magnitudeRotateMaquette * _modelRotateSpeed * Time.deltaTime;
                        }
                        break;
                }

                if (maquetteManipulationMode != ModelManipulationMode.None)
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

                foreach (var layer in m_application.GetLayers())
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
                    foreach (var layer in m_application.GetLayers())
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
            _modelOffset = positionOffset;
            _modelRotation = rotationOffset;
            UpdateModelLocationAndScale();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void UpdateModelLocationAndScale()
        {
            //Logger.Debug("ApplicationStateScaleModel.UpdateModelLocationAndScale()");

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
            pos.y = 1 + _modelOffset;
            activeProject.transform.position = pos;

            // Rotate it.
            activeProject.transform.RotateAround(m_application.OffsetPerID, Vector3.up, _modelRotation);

            foreach (var editData in m_application.EditDatas)
            {
                var t = editData.ContainerGameObject.transform;
                t.position = activeProject.transform.position;
                t.rotation = activeProject.transform.rotation;
                t.localScale = activeProject.transform.localScale;
            }
        }

        /// <summary>
        /// <see cref="ImmersionMode.UpdateTrackingSpacePosition()"/> implementation.
        /// </summary>
        public override void UpdateTrackingSpacePosition()
        {
            m_application.Logger.Debug("ApplicationStateScaleModel.UpdateTrackingSpacePosition()");

            m_application.ResetTrackingSpacePosition(); // Center around model.

            if (UnityEngine.Application.isEditor)
            {
                m_application.m_ovrCameraRig.transform.position = m_application.m_ovrCameraRig.transform.position + new Vector3(0, 1.8f, 0);
            }
        }

        /// <summary>
        /// <see cref="ImmersionMode.InitButtonMappingUI()"/> implementation.
        /// </summary>
        public /*override*/ void UpdateControllerUI()
        {
            //m_application.Logger.Debug("ApplicationStateScaleModel.UpdateControllerUI()");

            var isEditor = UnityEngine.Application.isEditor;

            // Left controller
            if (m_application.leftControllerButtonMapping != null)
            {
                m_application.leftControllerButtonMapping.HandTrigger.Text = "";

                m_application.leftControllerButtonMapping.IndexTrigger.Text = "Verander schaal";

                m_application.leftControllerButtonMapping.ButtonStart.Text = "Toggle menu";

                m_application.leftControllerButtonMapping.ButtonX.Text = "Vorig project";
                m_application.leftControllerButtonMapping.ButtonY.Text = "Volgend project";

                m_application.leftControllerButtonMapping.ThumbUp.Text = "Model omhoog";
                m_application.leftControllerButtonMapping.ThumbDown.Text = "Model omlaag";
                m_application.leftControllerButtonMapping.ThumbLeft.Text = "Model links";
                m_application.leftControllerButtonMapping.ThumbRight.Text = "Model rechts";
            }

            // Right controller
            if (m_application.rightControllerButtonMapping != null)
            {
                m_application.rightControllerButtonMapping.IndexTrigger.Text = "";
                m_application.rightControllerButtonMapping.HandTrigger.Text = "";

                m_application.rightControllerButtonMapping.ButtonOculusStart.Text = "Exit";

                m_application.rightControllerButtonMapping.ButtonXA.Text = "";
                m_application.rightControllerButtonMapping.ButtonYB.Text = "";

                m_application.DisplayFlyControls();
            }
        }
    }
}