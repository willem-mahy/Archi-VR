using UnityEngine;

namespace ArchiVR
{
    public class ImmersionModeWalkthrough : ImmersionMode
    {
        public override void Enter()
        {
            InitButtonMappingUI();

            // Restore default moving up/down.
            m_application.m_flySpeedUpDown = 1.0f;
        }

        public override void Exit()
        {
            // Restore default moving up/down.
            m_application.m_flySpeedUpDown = ApplicationArchiVR.DefaultFlySpeedUpDown;
        }

        public override void Update()
        {
            if (m_application.m_loadingProjectInfo == null) // While not loading a project...
            {
                // ... Active POI is toggle using A/B button, F3/F4 key.
                var activateNextPOI = m_application.m_controllerInput.m_controllerState.button1Down || Input.GetKeyDown(KeyCode.F3);
                var activatePrevPOI = m_application.m_controllerInput.m_controllerState.button2Down || Input.GetKeyDown(KeyCode.F4);

                #region Activate POI

                if (activatePrevPOI)
                {
                    m_application.OffsetActivePOIIndex(+1);
                }

                if (activateNextPOI)
                {
                    m_application.OffsetActivePOIIndex(-1);
                }

                #endregion
            }
        }

        public override void UpdateModelLocationAndScale()
        {
            var activeProject = m_application.GetActiveProject();

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
            var activePOI = m_application.GetActivePOI();

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

        void InitButtonMappingUI()
        {
            // Left controller
            if (m_application.leftControllerButtonMapping != null)
            {
                m_application.leftControllerButtonMapping.textLeftHandTrigger.text = "";

                m_application.leftControllerButtonMapping.textLeftIndexTrigger.text = "Verander shaal";

                m_application.leftControllerButtonMapping.textButtonStart.text = "Toggle menu";

                m_application.leftControllerButtonMapping.textButtonX.text = "Vorig project";
                m_application.leftControllerButtonMapping.textButtonY.text = "Volgend project";

                m_application.leftControllerButtonMapping.textLeftThumbUp.text = "";
                m_application.leftControllerButtonMapping.textLeftThumbDown.text = "";
                m_application.leftControllerButtonMapping.textLeftThumbLeft.text = "";
                m_application.leftControllerButtonMapping.textLeftThumbRight.text = "";
            }

            // Right controller
            if (m_application.rightControllerButtonMapping != null)
            {
                m_application.rightControllerButtonMapping.textRightIndexTrigger.text = "Beweeg omhoog";
                m_application.rightControllerButtonMapping.textRightHandTrigger.text = "Beweeg omlaag";

                m_application.rightControllerButtonMapping.textButtonOculus.text = "Exit";

                m_application.rightControllerButtonMapping.textButtonA.text = "Vorige locatie";
                m_application.rightControllerButtonMapping.textButtonB.text = "Volgende locatie";

                m_application.rightControllerButtonMapping.textRightThumbUp.text = "Beweeg vooruit";
                m_application.rightControllerButtonMapping.textRightThumbDown.text = "Beweeg achteruit";
                m_application.rightControllerButtonMapping.textRightThumbLeft.text = "Beweeg links";
                m_application.rightControllerButtonMapping.textRightThumbRight.text = "Beweeg rechts";
            }
        }
    }
}