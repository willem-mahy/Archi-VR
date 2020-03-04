using UnityEngine;
using WM;
using WM.Application;
using WM.Command;
using ArchiVR.Application;
using WM.Colocation;

namespace ArchiVR
{
    /// <summary>
    /// In this immersion mode, the user can walk around in the real-scale model.
    /// The user can jump between a list of Points-Of-Interest, predefined in the project.
    /// </summary>
    public class ApplicationStateWalkthrough : ApplicationState<ApplicationArchiVR>
    {
        #region variables

        /// <summary>
        /// A reference system representing the active POI.
        /// </summary>
        ReferenceSystem6DOF activePoiReferenceSystem;

        #endregion

        public ApplicationStateWalkthrough(ApplicationArchiVR application) : base(application)
        {
        }

        /// <summary>
        /// <see cref="ImmersionMode.Enter()"/> implementation.
        /// </summary>
        public override void Enter()
        {
            m_application.Logger.Debug("ApplicationStateWalkthrough.Enter()");

            if (m_application._teleportAreaGO == null)
            {
                m_application._teleportAreaGO = UtilUnity.FindGameObjectElseError(m_application.gameObject.scene, "TeleportArea");

                var teleportAreaVolumeGO = m_application._teleportAreaGO.transform.Find("Volume");

                if (teleportAreaVolumeGO == null)
                {
                    m_application.Logger.Error("TeleportArea.Volume gameobject not found.");
                }

                m_application._teleportAreaVolume = teleportAreaVolumeGO.GetComponent<TeleportAreaVolume>();

                if (m_application._teleportAreaVolume == null)
                {
                    m_application.Logger.Error("TeleportArea.Volume: TeleportAreaVolume component not found.");
                }

                m_application._teleportAreaGO.SetActive(false);
            }

            // Restore default moving up/down.
            m_application.m_flySpeedUpDown = 1.0f;

            m_application.UnhideAllModelLayers();

            if (activePoiReferenceSystem == null)
            {
                activePoiReferenceSystem = m_application.CreateReferenceSystem("POI", null);
            }

            UpdateModelLocationAndScale();
            UpdateTrackingSpacePosition();
        }

        /// <summary>
        /// <see cref="ImmersionMode.Exit()"/> implementation.
        /// </summary>
        public override void Exit()
        {
            m_application.Logger.Debug("ApplicationStateWalkthrough.Exit()");

            // Restore default moving up/down.
            m_application.m_flySpeedUpDown = UnityApplication.DefaultFlySpeedUpDown;

            // We might have made the boundary visible, so make sure it is hidden again.
            OVRManager.boundary.SetVisible(false);
        }

        /// <summary>
        /// <see cref="ImmersionMode.Resume()"/> implementation.
        /// </summary>
        public override void Resume()
        {
            m_application.Logger.Debug("ApplicationStateWalkthrough.Resume()");

            // Restore default moving up/down.
            m_application.m_flySpeedUpDown = 1.0f;

            m_application.UnhideAllModelLayers();

            UpdateModelLocationAndScale();
            UpdateTrackingSpacePosition();
        }

        /// <summary>
        /// <see cref="ImmersionMode.Update()"/> implementation.
        /// </summary>
        public override void Update()
        {
            //m_application.Logger.Debug("ApplicationStateWalkthrough.Update()");

            UpdateControllerUI();

            if (_teleportCommand != null && m_application._teleportAreaVolume.AllPlayersPresent)
            {
                m_application.Teleport(_teleportCommand);
                
                _teleportCommand = null;
                //m_application._teleportAreaVolume.AllPlayersPresent = false;
            }

            if (_teleportCommand == null)
            {
                if (m_application.ActivateNextProject)
                {
                    _teleportCommand = m_application.GetTeleportCommandForProject(m_application.ActiveProjectIndex + 1);
                }

                if (m_application.ActivatePreviousProject)
                {
                    _teleportCommand = m_application.GetTeleportCommandForProject(m_application.ActiveProjectIndex - 1);
                }

                if (m_application.ActivateNextPOI)
                {
                    _teleportCommand = m_application.GetTeleportCommandForPOI(m_application.ActivePOIIndex + 1);
                }

                if (m_application.ActivatePreviousPOI)
                {
                    _teleportCommand = m_application.GetTeleportCommandForPOI(m_application.ActivePOIIndex - 1);
                }

                // If we just started a teleport procedure...
                if (_teleportCommand != null)
                {
                    m_application._teleportAreaVolume.Players.Clear();

                    var tic = new TeleportInitiatedCommand();
                    
                    if (m_application.NetworkInitialized && m_application.NetworkMode == WM.Net.NetworkMode.Server)
                    {
                        m_application.Server.BroadcastCommand(tic);
                    }
                    else
                    {
                        tic.Execute(m_application);
                    }
                }
            }
            
            /*
            if (m_application.ToggleImmersionModeIfInputAndNetworkModeAllows())
            {
                return;
            }
            */

            var controllerState = m_application.m_controllerInput.m_controllerState;

            // Toggle Enable/Disable translating tracking space Up/Down using left thumbstick click.
            if (controllerState.lThumbstickDown)
            {
                m_application.EnableTrackingSpaceTranslationUpDown = !m_application.EnableTrackingSpaceTranslationUpDown;
                UpdateControllerUI();
            }

            // By default, do not show the boundary.  We will set it visible in Fly() and UpdateTrackingSpace() below, if needed.
            OVRManager.boundary.SetVisible(false);

            m_application.Fly();

            m_application.m_rightControllerText.text = m_application.ActivePOIName ?? "";

            m_application.UpdateTrackingSpace();

            // Starting the SRF measurement procedure is done by (editor-mode shortcut only)
            // - Pressing 'C' on the keyboard.
            if (Input.GetKeyDown(KeyCode.C))
            {
                m_application.PushApplicationState(new ApplicationStateDefineSharedReferenceSystem(m_application));
            }

            // Edit mode is activated by:
            // - Pressing 'E' on the keyboard.
            // - Clicking with the L or R thumbstick.
            var activateEditMode =
                Input.GetKeyDown(KeyCode.E)
                || controllerState.lHandTriggerDown || controllerState.rHandTriggerDown;

            if (activateEditMode)
            {
                var applicationState = new ApplicationStateEdit(m_application);

                m_application.PushApplicationState(applicationState);
            }

            // Returning the the default state, is done by:
            // - Pressing the left controller index trigger.
            var returnToDefaultState = controllerState.lIndexTriggerDown;

            if (returnToDefaultState)
            {
                m_application.PopApplicationState();
            }
        }

        /// <summary>
        /// Called when a teleport procedure has started.
        /// </summary>
        public override void InitTeleport()
        {
            // Show the guidance UI for directing users to the teleport area.
            m_application._teleportAreaGO.SetActive(true);
            m_application.HudInfoPanel.SetActive(true);
            m_application.HudInfoText.text = "Move to teleport area";
        }

        /// <summary>
        /// <see cref="ImmersionMode.UpdateModelLocationAndScale()"/> implementation.
        /// </summary>
        public override void UpdateModelLocationAndScale()
        {
            m_application.Logger.Debug("ApplicationStateWalkthrough.UpdateModelLocationAndScale()");

            var activeProject = m_application.ActiveProject;

            if (activeProject == null)
            {
                return;
            }

            activeProject.transform.position = m_application.OffsetPerID;
            activeProject.transform.rotation = Quaternion.identity;
            activeProject.transform.localScale = Vector3.one;

            foreach (var editData in m_application.EditDatas)
            {
                var t = editData.ContainerGameObject.transform;
                t.position      = activeProject.transform.position;
                t.rotation      = activeProject.transform.rotation;
                t.localScale    = activeProject.transform.localScale;
            }
        }

        /// <summary>
        /// <see cref="ImmersionMode.UpdateTrackingSpacePosition()"/> implementation.
        /// </summary>
        public override void UpdateTrackingSpacePosition()
        {
            m_application.Logger.Debug("ApplicationStateWalkthrough.UpdateTrackingSpacePosition()");

            var activePOI = m_application.ActivePOI;

            if (activePOI == null)
            {
                m_application.ResetTrackingSpacePosition();

                UnityApplication.SetReferenceSystemLocation(
                    activePoiReferenceSystem,
                    Vector3.zero,
                    Quaternion.identity);

                m_application._teleportAreaGO.transform.position = Vector3.zero;
            }
            else
            {
                UnityApplication.SetReferenceSystemLocation(
                    activePoiReferenceSystem,
                    activePOI.transform.position,
                    activePOI.transform.rotation);

                m_application._teleportAreaGO.transform.position = activePOI.transform.position;

                if (m_application.ColocationEnabled)
                {
                    m_application.m_ovrCameraRig.transform.SetPositionAndRotation(
                            Vector3.zero,
                            Quaternion.identity);

                    var transformTRF = m_application.trackingSpace.gameObject.transform;
                    var transformSRF = m_application.SharedReferenceSystem.gameObject.transform;

                    var matrix_S_T = transformSRF.worldToLocalMatrix * transformTRF.localToWorldMatrix;
                    //var matrix_T_S = transformTRF.worldToLocalMatrix * transformSRF.localToWorldMatrix;
                    
                    //var matrix_T_Si = matrix_S_T.inverse;
                    //var matrix_S_Ti = matrix_T_S.inverse;

                    var matrix_POI_W = activePOI.transform.localToWorldMatrix;

                    var matrix_result = matrix_POI_W * matrix_S_T;

                    m_application.m_ovrCameraRig.transform.FromMatrix(matrix_result);
                }
                else
                {
                    bool alignEyeToPOI = false;

                    if (alignEyeToPOI)
                    {
                        var rotTrackingSpace = m_application.m_ovrCameraRig.transform.rotation.eulerAngles;
                        var rotEye = m_application.m_centerEyeCanvas.transform.parent.rotation.eulerAngles;

                        var rot = activePOI.transform.rotation.eulerAngles;

                        rot.y = rot.y + (rotTrackingSpace.y - rotEye.y);

                        m_application.m_ovrCameraRig.transform.SetPositionAndRotation(
                            activePOI.transform.position,
                            Quaternion.Euler(rot));
                    }
                    else
                    {
                        m_application.m_ovrCameraRig.transform.SetPositionAndRotation(
                            activePOI.transform.position,
                            activePOI.transform.rotation);
                    }
                }

                if (UnityEngine.Application.isEditor)
                {
                    m_application.m_ovrCameraRig.transform.position = m_application.m_ovrCameraRig.transform.position + new Vector3(0, 1.8f, 0);
                }
            }
        }

        /// <summary>
        /// <see cref="ImmersionMode.InitButtonMappingUI()"/> implementation.
        /// </summary>
        public /*override*/ void UpdateControllerUI()
        {
            //m_application.Logger.Debug("ApplicationStateWalkthrough.UpdateControllerUI()");

            var isEditor = UnityEngine.Application.isEditor;

            // Update left controller UI displaying the project name.
            m_application.m_leftControllerText.text = (m_application.ActiveProjectName != null) ? m_application.GetProjectName(m_application.ActiveProjectName) : "No project loaded.";

            // Left controller
            {
                var buttonMapping = m_application.leftControllerButtonMapping;

                if (buttonMapping != null)
                {
                    buttonMapping.HandTrigger.Text = "Edit";

                    buttonMapping.IndexTrigger.Text = "Back";

                    buttonMapping.ButtonStart.Text = "Toggle menu";

                    buttonMapping.ButtonX.Text = "Vorig project";
                    buttonMapping.ButtonY.Text = "Volgend project";

                    if (m_application.EnableTrackingSpaceTranslationUpDown)
                    {
                        buttonMapping.ThumbUp.Text = "Beweeg omhoog";
                        buttonMapping.ThumbDown.Text = "Beweeg omlaag";
                    }
                    else
                    {
                        buttonMapping.ThumbUp.Text = "";
                        buttonMapping.ThumbDown.Text = "";
                    }

                    buttonMapping.ThumbLeft.Text = "< Tracking";
                    buttonMapping.ThumbRight.Text = "Tracking >";
                }
            }

            // Right controller
            {
                var buttonMapping = m_application.rightControllerButtonMapping;

                if (buttonMapping != null)
                {
                    buttonMapping.IndexTrigger.Text = "";
                    buttonMapping.HandTrigger.Text = "Edit";

                    buttonMapping.ButtonOculusStart.Text = "Exit";

                    buttonMapping.ButtonXA.Text = "Vorige locatie";
                    buttonMapping.ButtonYB.Text = "Volgende locatie";

                    buttonMapping.ThumbUp.Text = "Forward";
                    buttonMapping.ThumbDown.Text = "Backward";
                    buttonMapping.ThumbLeft.Text = "Left";
                    buttonMapping.ThumbRight.Text = "Right";
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private TeleportCommand _teleportCommand;
    }
}