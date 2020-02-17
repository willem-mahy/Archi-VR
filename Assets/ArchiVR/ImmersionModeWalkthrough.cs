using UnityEngine;
using WM;
using WM.Application;
using WM.Command;

namespace ArchiVR
{
    /// <summary>
    /// In this immersion mode, the user can walk around in the real-scale model.
    /// The user can jump between a list of Points-Of-Interest, predefined in the project.
    /// </summary>
    public class ImmersionModeWalkthrough : ImmersionMode
    {
        #region variables

        /// <summary>
        /// The teleport area.
        /// </summary>
        private GameObject _teleportAreaGO;

        /// <summary>
        /// The teleport area.
        /// </summary>
        private TeleportAreaVolume _teleportAreaVolume;

        /// <summary>
        /// A reference system representing the active POI.
        /// </summary>
        ReferenceSystem6DOF activePoiReferenceSystem;

        #endregion

        /// <summary>
        /// <see cref="ImmersionMode.Enter()"/> implementation.
        /// </summary>
        public override void Enter()
        {
            Application.Logger.Debug("ImmersionModeWalkthrough.Enter()");

            if (_teleportAreaGO == null)
            {
                _teleportAreaGO = UtilUnity.FindGameObjectElseError(Application.gameObject.scene, "TeleportArea");

                var teleportAreaVolumeGO = _teleportAreaGO.transform.Find("TeleportAreaVolume");

                if (teleportAreaVolumeGO == null)
                {
                    Application.Logger.Error("TeleoportAreaVolume gameobject not found.");
                }

                _teleportAreaVolume = teleportAreaVolumeGO.GetComponent<TeleportAreaVolume>();

                if (_teleportAreaVolume == null)
                {
                    Application.Logger.Error("TeleoportAreaVolume component not found.");
                }

                _teleportAreaGO.SetActive(false);
            }

            InitButtonMappingUI();

            // Restore default moving up/down.
            Application.m_flySpeedUpDown = 1.0f;

            Application.UnhideAllModelLayers();

            if (activePoiReferenceSystem == null)
            {
                activePoiReferenceSystem = Application.CreateReferenceSystem("POI", null);
            }
        }

        /// <summary>
        /// <see cref="ImmersionMode.Exit()"/> implementation.
        /// </summary>
        public override void Exit()
        {
            Application.Logger.Debug("ImmersionModeWalkthrough.Exit()");

            // Restore default moving up/down.
            Application.m_flySpeedUpDown = UnityApplication.DefaultFlySpeedUpDown;

            // We might have made the boundary visible, so make sure it is hidden again.
            OVRManager.boundary.SetVisible(false);
        }

        TeleportCommand tc;


        /// <summary>
        /// <see cref="ImmersionMode.Update()"/> implementation.
        /// </summary>
        public override void Update()
        {
            //WM.Logger.Debug("ImmersionModeWalkthrough.Update()");
            

            _teleportAreaGO.SetActive(tc != null);

            if (tc != null)
            {
                Application.HudInfoPanel.SetActive(true);
                Application.HudInfoText.text = "Move to teleport area";
            }

            if (tc != null && _teleportAreaVolume.AllPlayersPresent)
            {
                Application.Teleport(tc);
                tc = null;
                _teleportAreaGO.SetActive(false);
                _teleportAreaVolume.AllPlayersPresent = false;
                Application.HudInfoPanel.SetActive(false);
                Application.HudInfoText.text = "";
                return;
            }

            if (Application.ActivateNextProject)
            {
                tc = Application.GetTeleporCommandForProject(Application.ActiveProjectIndex - 1);
            }

            if (Application.ActivatePreviousProject)
            {
                tc = Application.GetTeleporCommandForProject(Application.ActiveProjectIndex + 1);
            }

            /*
            if (Application.ToggleActiveProject())
            {
                return;
            }

            if (Application.ToggleActivePOI())
            {
                return;
            }
            */

            if (Application.ToggleImmersionModeIfInputAndNetworkModeAllows())
            {
                return;
            }

            // Toggle Enable/Disable translating tracking space Up/Down using left thumbstick click.
            if (Application.m_controllerInput.m_controllerState.lThumbstickDown)
            {
                Application.EnableTrackingSpaceTranslationUpDown = !Application.EnableTrackingSpaceTranslationUpDown;
                InitButtonMappingUI();
            }

            // By default, do not show the boundary.  We will set it visible in Fly() and UpdateTrackingSpace() below, if needed.
            OVRManager.boundary.SetVisible(false);

            Application.Fly();

            Application.m_rightControllerText.text = Application.ActivePOIName ?? "";

            Application.UpdateTrackingSpace();

            if (Input.GetKeyDown(KeyCode.C))
            {
                Application.SetActiveApplicationState(2);
            }
        }

        /// <summary>
        /// <see cref="ImmersionMode.Update()"/> implementation.
        /// </summary>
        public override void UpdateModelLocationAndScale()
        {
            Application.Logger.Debug("ImmersionModeWalkthrough.UpdateModelLocationAndScale()");

            var activeProject = Application.ActiveProject;

            if (activeProject == null)
            {
                return;
            }

            activeProject.transform.position = Application.OffsetPerID;
            activeProject.transform.rotation = Quaternion.identity;
            activeProject.transform.localScale = Vector3.one;
        }

        /// <summary>
        /// <see cref="ImmersionMode.UpdateTrackingSpacePosition()"/> implementation.
        /// </summary>
        public override void UpdateTrackingSpacePosition()
        {
            Application.Logger.Debug("ImmersionModeWalkthrough.UpdateTrackingSpacePosition()");

            var activePOI = Application.ActivePOI;

            if (activePOI == null)
            {
                Application.ResetTrackingSpacePosition();

                UnityApplication.SetReferenceSystemLocation(
                    activePoiReferenceSystem,
                    Vector3.zero,
                    Quaternion.identity);

                _teleportAreaGO.transform.position = Vector3.zero;
            }
            else
            {
                UnityApplication.SetReferenceSystemLocation(
                    activePoiReferenceSystem,
                    activePOI.transform.position,
                    activePOI.transform.rotation);

                _teleportAreaGO.transform.position = activePOI.transform.position;

                if (Application.ColocationEnabled)
                {
                    Application.m_ovrCameraRig.transform.SetPositionAndRotation(
                            Vector3.zero,
                            Quaternion.identity);

                    var transformTRF = Application.trackingSpace.gameObject.transform;
                    var transformSRF = Application.SharedReferenceSystem.gameObject.transform;

                    var matrix_S_T = transformSRF.worldToLocalMatrix * transformTRF.localToWorldMatrix;
                    //var matrix_T_S = transformTRF.worldToLocalMatrix * transformSRF.localToWorldMatrix;
                    
                    //var matrix_T_Si = matrix_S_T.inverse;
                    //var matrix_S_Ti = matrix_T_S.inverse;

                    var matrix_POI_W = activePOI.transform.localToWorldMatrix;

                    var matrix_result = matrix_POI_W * matrix_S_T;

                    Application.m_ovrCameraRig.transform.FromMatrix(matrix_result);
                }
                else
                {
                    bool alignEyeToPOI = false;

                    if (alignEyeToPOI)
                    {
                        var rotTrackingSpace = Application.m_ovrCameraRig.transform.rotation.eulerAngles;
                        var rotEye = Application.m_centerEyeCanvas.transform.parent.rotation.eulerAngles;

                        var rot = activePOI.transform.rotation.eulerAngles;

                        rot.y = rot.y + (rotTrackingSpace.y - rotEye.y);

                        Application.m_ovrCameraRig.transform.SetPositionAndRotation(
                            activePOI.transform.position,
                            Quaternion.Euler(rot));
                    }
                    else
                    {
                        Application.m_ovrCameraRig.transform.SetPositionAndRotation(
                            activePOI.transform.position,
                            activePOI.transform.rotation);
                    }
                }

                if (UnityEngine.Application.isEditor)
                {
                    Application.m_ovrCameraRig.transform.position = Application.m_ovrCameraRig.transform.position + new Vector3(0, 1.8f, 0);
                }
            }
        }

        /// <summary>
        /// <see cref="ImmersionMode.InitButtonMappingUI()"/> implementation.
        /// </summary>
        public override void InitButtonMappingUI()
        {
            Application.Logger.Debug("ImmersionModeWalkthrough.InitButtonMappingUI()");

            var isEditor = UnityEngine.Application.isEditor;

            // Left controller
            {
                var buttonMapping = Application.leftControllerButtonMapping;

                if (buttonMapping != null)
                {
                    buttonMapping.HandTrigger.Text = "GFX Quality";

                    buttonMapping.IndexTrigger.Text = "Verander schaal" + (isEditor ? " (R)" : "");

                    buttonMapping.ButtonStart.Text = "Toggle menu" + (isEditor ? " (F11)" : "");

                    buttonMapping.ButtonX.Text = "Vorig project" + (isEditor ? " (F1)" : "");
                    buttonMapping.ButtonY.Text = "Volgend project" + (isEditor ? " (F2)" : "");

                    if (Application.EnableTrackingSpaceTranslationUpDown)
                    {
                        buttonMapping.ThumbUp.Text = "Beweeg omhoog" + (isEditor ? " (Z)" : "");
                        buttonMapping.ThumbDown.Text = "Beweeg omlaag" + (isEditor ? " (S)" : "");
                    }
                    else
                    {
                        buttonMapping.ThumbUp.Text = "";
                        buttonMapping.ThumbDown.Text = "";
                    }

                    buttonMapping.ThumbLeft.Text = "< Tracking " + (isEditor ? " (Q)" : "");
                    buttonMapping.ThumbRight.Text = "Tracking >" + (isEditor ? " (D)" : "");
                }
            }

            // Right controller
            {
                var buttonMapping = Application.rightControllerButtonMapping;

                if (buttonMapping != null)
                {
                    buttonMapping.IndexTrigger.Text = "";
                    buttonMapping.HandTrigger.Text = "";

                    buttonMapping.ButtonOculusStart.Text = "Exit" + (isEditor ? " ()" : "");

                    buttonMapping.ButtonXA.Text = "Vorige locatie" + (isEditor ? " (F3)" : "");
                    buttonMapping.ButtonYB.Text = "Volgende locatie" + (isEditor ? " (F4)" : "");

                    buttonMapping.ThumbUp.Text = "Beweeg vooruit" + (isEditor ? " (ArrowUp)" : "");
                    buttonMapping.ThumbDown.Text = "Beweeg achteruit" + (isEditor ? " (ArrowDown)" : "");
                    buttonMapping.ThumbLeft.Text = "Beweeg links" + (isEditor ? " (ArrowLeft)" : "");
                    buttonMapping.ThumbRight.Text = "Beweeg rechts" + (isEditor ? " (ArrowRight)" : "");
                }
            }
        }
    }
}