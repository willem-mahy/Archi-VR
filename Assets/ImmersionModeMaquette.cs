using UnityEngine;

namespace ArchiVR
{
    public class ImmersionModeMaquette : ImmersionMode
    {
        #region variables
        
        GameObject m_maquettePreviewContext = null;

        float m_maquetteOffset = 0;

        float m_maquetteRotation = 0;

        #endregion

        public override void Init()
        {
            if (m_maquettePreviewContext == null)
            {
                m_maquettePreviewContext = GameObject.Find("MaquettePreviewContext");
            }
        }

        public override void Enter()
        {
            InitButtonMappingUI();

            if (m_maquettePreviewContext)
                m_maquettePreviewContext.SetActive(true);

            // Disable moving up/down.
            m_application.m_flySpeedUpDown = 0.0f;
        }

        public override void Exit()
        {
            if (m_maquettePreviewContext)
                m_maquettePreviewContext.SetActive(false);

            // Restore default moving up/down.
            m_application.m_flySpeedUpDown = ApplicationArchiVR.DefaultFlySpeedUpDown;
        }

        public override void Update()
        {
            #region Maquette manipulation.

            float magnitudeRotateMaquette = 0.0f;
            float magnitudeTranslateMaquette = 0.0f;

            if (Application.isEditor)
            {
                float mag = 1.0f;
                magnitudeTranslateMaquette += Input.GetKey(KeyCode.O) ? mag : 0.0f;
                magnitudeTranslateMaquette -= Input.GetKey(KeyCode.L) ? mag : 0.0f;

                magnitudeRotateMaquette += Input.GetKey(KeyCode.K) ? mag : 0.0f;
                magnitudeRotateMaquette -= Input.GetKey(KeyCode.M) ? mag : 0.0f;
            }
            else
            {
                magnitudeTranslateMaquette += m_application.m_controllerInput.m_controllerState.lThumbStick.y;
                magnitudeRotateMaquette = m_application.m_controllerInput.m_controllerState.lThumbStick.x;
            }

            // Translate Up/Down
            var maquetteMoveSpeed = 1.0f;

            m_maquetteOffset = Mathf.Min(1.0f, m_maquetteOffset + magnitudeTranslateMaquette * maquetteMoveSpeed * Time.deltaTime);

            // Rotate around 'up' vector.
            var maquetteRotateSpeed = 60.0f;

            m_maquetteRotation += magnitudeRotateMaquette * maquetteRotateSpeed * Time.deltaTime;

            UpdateModelLocationAndScale();

            #endregion
        }

        public override void UpdateModelLocationAndScale()
        {
            var activeProject = m_application.GetActiveProject();

            if (activeProject == null)
            {
                return;
            }

            var position = Vector3.zero;
            position.y = 1 + m_maquetteOffset;

            var rotation = Quaternion.AngleAxis(m_maquetteRotation, Vector3.up);

            var scale = 0.04f * Vector3.one;

            activeProject.transform.position = position;
            activeProject.transform.rotation = rotation;
            activeProject.transform.localScale = scale;
        }

        public override void UpdateTrackingSpacePosition()
        {
            m_application.ResetTrackingSpacePosition(); // Center around model.

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

                m_application.leftControllerButtonMapping.textLeftThumbUp.text = "Model omhoog";
                m_application.leftControllerButtonMapping.textLeftThumbDown.text = "Model omlaag";
                m_application.leftControllerButtonMapping.textLeftThumbLeft.text = "Model links";
                m_application.leftControllerButtonMapping.textLeftThumbRight.text = "Model rechts";
            }

            // Right controller
            if (m_application.rightControllerButtonMapping != null)
            {
                m_application.rightControllerButtonMapping.textRightIndexTrigger.text = "";
                m_application.rightControllerButtonMapping.textRightHandTrigger.text = "";

                m_application.rightControllerButtonMapping.textButtonOculus.text = "Exit";

                m_application.rightControllerButtonMapping.textButtonA.text = "";
                m_application.rightControllerButtonMapping.textButtonB.text = "";

                m_application.rightControllerButtonMapping.textRightThumbUp.text = "Beweeg vooruit";
                m_application.rightControllerButtonMapping.textRightThumbDown.text = "Beweeg achteruit";
                m_application.rightControllerButtonMapping.textRightThumbLeft.text = "Beweeg links";
                m_application.rightControllerButtonMapping.textRightThumbRight.text = "Beweeg rechts";
            }
        }
    }
}