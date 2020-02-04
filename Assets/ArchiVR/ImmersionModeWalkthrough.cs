using UnityEngine;
using WM.Application;

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
            WM.Logger.Debug("ImmersionModeWalkthrough.Enter()");

            InitButtonMappingUI();

            // Restore default moving up/down.
            Application.m_flySpeedUpDown = 1.0f;

            Application.UnhideAllModelLayers();
        }

        public override void Exit()
        {
            WM.Logger.Debug("ImmersionModeWalkthrough.Exit()");

            // Restore default moving up/down.
            Application.m_flySpeedUpDown = UnityApplication.DefaultFlySpeedUpDown;

            // We might have made the boundary visible, so make sure it is hidden again.
            OVRManager.boundary.SetVisible(false);
        }

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
        }

        public override void UpdateModelLocationAndScale()
        {
            WM.Logger.Debug("ImmersionModeWalkthrough.UpdateModelLocationAndScale()");

            var activeProject = Application.ActiveProject;

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
            WM.Logger.Debug("ImmersionModeWalkthrough.UpdateTrackingSpacePosition()");

            var activePOI = Application.ActivePOI;

            if (activePOI == null)
            {
                Application.ResetTrackingSpacePosition();
            }
            else
            {
                var rotTrackingSpace = Application.m_ovrCameraRig.transform.rotation.eulerAngles;
                var rotEye = Application.m_centerEyeCanvas.transform.parent.rotation.eulerAngles;

                Application.m_ovrCameraRig.transform.position = activePOI.transform.position;

                var rot = activePOI.transform.rotation.eulerAngles;
                rot.y = rot.y + (rotTrackingSpace.y - rotEye.y);
                Application.m_ovrCameraRig.transform.rotation = Quaternion.Euler(rot);
            }

            if (UnityEngine.Application.isEditor)
            {
                Application.m_ovrCameraRig.transform.position = Application.m_ovrCameraRig.transform.position + new Vector3(0, 1.8f, 0);
            }
        }

        public override void InitButtonMappingUI()
        {
            WM.Logger.Debug("ImmersionModeWalkthrough.InitButtonMappingUI()");

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