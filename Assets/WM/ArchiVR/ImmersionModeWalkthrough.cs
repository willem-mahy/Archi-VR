using UnityEngine;

namespace WM
{
    namespace ArchiVR
    {
        public class ImmersionModeWalkthrough : ImmersionMode
        {
            #region variables

            // TODO: remove below: I think we can just manimpulate  trackingspace to align model with play space...

            //#region Model location (ued to align with play space)

            //float m_modelRotation = 0;

            //Vector3 m_modelTranslation = new Vector3();

            #endregion

            public override void Enter()
            {
                Logger.Debug("ImmersionModeWalkthrough.Enter()");

                InitButtonMappingUI();

                // Restore default moving up/down.
                m_application.m_flySpeedUpDown = 1.0f;

                m_application.UnhideAllModelLayers();
            }

            public override void Exit()
            {
                Logger.Debug("ImmersionModeWalkthrough.Exit()");

                // Restore default moving up/down.
                m_application.m_flySpeedUpDown = ApplicationArchiVR.DefaultFlySpeedUpDown;

                // We might have made the boundary visible, so make sure it is hidden again.
                OVRManager.boundary.SetVisible(false);
            }

            public override void Update()
            {
                //Logger.Debug("ImmersionModeWalkthrough.Update()");

                if (m_application.ToggleActiveProject())
                {
                    return;
                }

                if (m_application.ToggleActivePOI())
                {
                    return;
                }

                if (m_application.ToggleImmersionModeIfInputAndNetworkModeAllows())
                {
                    return;
                }

                // Toggle Enable/Disable translating tracking space Up/Down using left thumbstick click.
                if (m_application.m_controllerInput.m_controllerState.lThumbstickDown)
                {
                    m_application.EnableTrackingSpaceTranslationUpDown = !m_application.EnableTrackingSpaceTranslationUpDown;
                    InitButtonMappingUI();
                }

                // By default, do not show the boundary.  We will set it visible in Fly() and UpdateTrackingSpace() below, if needed.
                OVRManager.boundary.SetVisible(false);

                m_application.Fly();

                m_application.m_rightControllerText.text = m_application.ActivePOIName ?? "";

                m_application.UpdateTrackingSpace();
            }

            public override void UpdateModelLocationAndScale()
            {
                Logger.Debug("ImmersionModeWalkthrough.UpdateModelLocationAndScale()");

                var activeProject = m_application.ActiveProject;

                if (activeProject == null)
                {
                    return;
                }

                activeProject.transform.position = Vector3.zero;
                activeProject.transform.rotation = Quaternion.identity;
                activeProject.transform.localScale = Vector3.one;
            }

            public override void UpdateTrackingSpacePosition()
            {
                Logger.Debug("ImmersionModeWalkthrough.UpdateTrackingSpacePosition()");

                var activePOI = m_application.ActivePOI;

                if (activePOI == null)
                {
                    m_application.ResetTrackingSpacePosition();
                }
                else
                {
                    var rotTrackingSpace = m_application.m_ovrCameraRig.transform.rotation.eulerAngles;
                    var rotEye = m_application.m_centerEyeCanvas.transform.parent.rotation.eulerAngles;

                    m_application.m_ovrCameraRig.transform.position = activePOI.transform.position;

                    var rot = activePOI.transform.rotation.eulerAngles;
                    rot.y = rot.y + (rotTrackingSpace.y - rotEye.y);
                    m_application.m_ovrCameraRig.transform.rotation = Quaternion.Euler(rot);
                }

                if (Application.isEditor)
                {
                    m_application.m_ovrCameraRig.transform.position = m_application.m_ovrCameraRig.transform.position + new Vector3(0, 1.8f, 0);
                }
            }

            public override void InitButtonMappingUI()
            {
                Logger.Debug("ImmersionModeWalkthrough.InitButtonMappingUI()");

                var isEditor = Application.isEditor;

                // Left controller
                {
                    var buttonMapping = m_application.leftControllerButtonMapping;

                    if (buttonMapping != null)
                    {
                        buttonMapping.textLeftHandTrigger.text = "GFX Quality";

                        buttonMapping.textLeftIndexTrigger.text = "Verander schaal" + (isEditor ? " (R)" : "");

                        buttonMapping.textButtonStart.text = "Toggle menu" + (isEditor ? " (F11)" : "");

                        buttonMapping.textButtonX.text = "Vorig project" + (isEditor ? " (F1)" : "");
                        buttonMapping.textButtonY.text = "Volgend project" + (isEditor ? " (F2)" : "");

                        if (m_application.EnableTrackingSpaceTranslationUpDown)
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
                    var buttonMapping = m_application.rightControllerButtonMapping;

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
    } // namespace ArchiVR
} // namespace WM