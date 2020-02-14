using UnityEngine;
using WM;
using WM.Application;

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

        /// <summary>
        /// <see cref="ImmersionMode.Update()"/> implementation.
        /// </summary>
        public override void Update()
        {
            //WM.Logger.Debug("ImmersionModeWalkthrough.Update()");

            if (Application.ToggleActiveProject())
            {
                return;
            }

            if (Application.ToggleActivePOI())
            {
                return;
            }

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
            }
            else
            {
                UnityApplication.SetReferenceSystemLocation(
                    activePoiReferenceSystem,
                    activePOI.transform.position,
                    activePOI.transform.rotation);

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
                    buttonMapping.textLeftHandTrigger.text = "GFX Quality";

                    buttonMapping.textLeftIndexTrigger.text = "Verander schaal" + (isEditor ? " (R)" : "");

                    buttonMapping.textButtonStart.text = "Toggle menu" + (isEditor ? " (F11)" : "");

                    buttonMapping.textButtonX.text = "Vorig project" + (isEditor ? " (F1)" : "");
                    buttonMapping.textButtonY.text = "Volgend project" + (isEditor ? " (F2)" : "");

                    if (Application.EnableTrackingSpaceTranslationUpDown)
                    {
                        buttonMapping.textLeftThumbUp.text = "Beweeg omhoog" + (isEditor ? " (Z)" : "");
                        buttonMapping.textLeftThumbDown.text = "Beweeg omlaag" + (isEditor ? " (S)" : "");
                    }
                    else
                    {
                        buttonMapping.textLeftThumbUp.text = "";
                        buttonMapping.textLeftThumbDown.text = "";
                    }

                    buttonMapping.textLeftThumbLeft.text = "< Tracking " + (isEditor ? " (Q)" : "");
                    buttonMapping.textLeftThumbRight.text = "Tracking >" + (isEditor ? " (D)" : "");
                }
            }

            // Right controller
            {
                var buttonMapping = Application.rightControllerButtonMapping;

                if (buttonMapping != null)
                {
                    buttonMapping.textRightIndexTrigger.text = "";
                    buttonMapping.textRightHandTrigger.text = "";

                    buttonMapping.textButtonOculus.text = "Exit" + (isEditor ? " ()" : "");

                    buttonMapping.textButtonA.text = "Vorige locatie" + (isEditor ? " (F3)" : "");
                    buttonMapping.textButtonB.text = "Volgende locatie" + (isEditor ? " (F4)" : "");

                    buttonMapping.textRightThumbUp.text = "Beweeg vooruit" + (isEditor ? " (ArrowUp)" : "");
                    buttonMapping.textRightThumbDown.text = "Beweeg achteruit" + (isEditor ? " (ArrowDown)" : "");
                    buttonMapping.textRightThumbLeft.text = "Beweeg links" + (isEditor ? " (ArrowLeft)" : "");
                    buttonMapping.textRightThumbRight.text = "Beweeg rechts" + (isEditor ? " (ArrowRight)" : "");
                }
            }
        }
    }
}