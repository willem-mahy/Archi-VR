using UnityEngine;
using WM.Application;

namespace ArchiVR.Application
{
    public class ApplicationStateDefault : ApplicationState<ApplicationArchiVR>
    {
        public ApplicationStateDefault(ApplicationArchiVR application) : base(application)
        {
        }

        public override void Init()
        {
            m_application.Logger.Debug("ApplicationStateDefault.Init()");
        }

        public override void Enter()
        {
            m_application.Logger.Debug("ApplicationStateDefault.Enter()");

            Resume();
        }

        public override void Exit()
        {
            m_application.Logger.Debug("ApplicationStateDefault.Exit()");
        }
        
        public override void Resume()
        {
            m_application.Logger.Debug("ApplicationStateDefault.Resume()");

            m_application.m_leftControllerText.text = "";
            m_application.m_rightControllerText.text = "Please choose immersion mode";

            m_application.ProjectVisible = false;
            m_application.MaquettePreviewContext.SetActive(true);

            UpdateTrackingSpacePosition();
        }

        public override void Pause()
        {
            m_application.Logger.Debug("ApplicationStateDefault.Pause()");

            m_application.MaquettePreviewContext.SetActive(false);
            m_application.ProjectVisible = true;
        }

        public override void Update()
        {
            //m_application.Logger.Debug("ApplicationStateDefault.Update()");

            UpdateControllerUI();

            var controllerState = m_application.m_controllerInput.m_controllerState; 
            
            if (controllerState.rIndexTriggerDown)
            {
                m_application.PushApplicationState(new ApplicationStateWalkthrough(m_application));
            }

            if (controllerState.rHandTriggerDown)
            {
                m_application.PushApplicationState(new ApplicationStateMaquette(m_application));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public /*override*/ void UpdateControllerUI()
        {
            m_application.Logger.Debug("ApplicationStateDefault.InitButtonMappingUI()");

            var isEditor = UnityEngine.Application.isEditor;

            // Left controller
            {
                var buttonMapping = m_application.leftControllerButtonMapping;

                if (buttonMapping != null)
                {
                    buttonMapping.HandTrigger.Text =
                    buttonMapping.IndexTrigger.Text =
                    buttonMapping.ButtonStart.Text =
                    buttonMapping.ButtonX.Text =
                    buttonMapping.ButtonY.Text =
                    buttonMapping.ThumbUp.Text =
                    buttonMapping.ThumbDown.Text =
                    buttonMapping.ThumbUp.Text =
                    buttonMapping.ThumbDown.Text =
                    buttonMapping.ThumbLeft.Text =
                    buttonMapping.ThumbRight.Text = "";
                }
            }

            // Right controller
            {
                var buttonMapping = m_application.rightControllerButtonMapping;

                if (buttonMapping != null)
                {
                    buttonMapping.IndexTrigger.Text = "Walkthrough";
                    buttonMapping.HandTrigger.Text = "Maquette";

                    buttonMapping.ButtonOculusStart.Text = "Exit" + (isEditor ? " ()" : "");

                    buttonMapping.ButtonXA.Text = "";
                    buttonMapping.ButtonYB.Text = "";

                    buttonMapping.ThumbUp.Text = "";
                    buttonMapping.ThumbDown.Text = "";
                    buttonMapping.ThumbLeft.Text = "";
                    buttonMapping.ThumbRight.Text = "";
                }
            }
        }

        /// <summary>
        /// Called when a teleport procedure has started.
        /// </summary>
        public override void InitTeleport()
        {
            //var aim = ((ApplicationArchiVR)m_application).ActiveImmersionMode;

            //if (aim != null)
            //{
            //    aim.InitTeleport();
            //}
        }

        public override void UpdateModelLocationAndScale()
        {
            //m_application.Logger.Debug("ApplicationStateDefault.UpdateModelLocationAndScale()");

            //var aim = ((ApplicationArchiVR)m_application).ActiveImmersionMode;

            //if (aim != null)
            //{
            //    aim.UpdateModelLocationAndScale();
            //}
        }

        public override void UpdateTrackingSpacePosition()
        {
            m_application.Logger.Debug("ApplicationStateDefault.UpdateTrackingSpacePosition()");

            m_application.ResetTrackingSpacePosition(); // Center around model.

            if (UnityEngine.Application.isEditor)
            {
                m_application.m_ovrCameraRig.transform.position = m_application.m_ovrCameraRig.transform.position + new Vector3(0, 1.8f, 0);
            }
        }
    }
} // namespace ArchiVR.Application