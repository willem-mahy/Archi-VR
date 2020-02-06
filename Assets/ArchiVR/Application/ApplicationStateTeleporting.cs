using WM.Application;

namespace ArchiVR.Application
{
    public class ApplicationStateTeleporting : ApplicationState
        {
            #region variables

            public ITeleportationSystem TeleportationSystem;

            #endregion

            public override void Init()
            {
                m_application.Logger.Debug("ApplicationStateTeleporting.Init()");
            }

            public override void Enter()
            {
                m_application.Logger.Debug("ApplicationStateTeleporting.Enter()");

                //m_application.m_leftHandAnchor.SetActive(false); // TODO? Hide only button labels?
                //m_application.m_rightHandAnchor.SetActive(false);

                m_application.m_leftControllerCanvas.SetActive(false);
                m_application.m_rightControllerCanvas.SetActive(false);

                if (m_application.leftControllerButtonMapping != null)
                {
                    m_application.leftControllerButtonMapping.gameObject.SetActive(false);
                }

                if (m_application.rightControllerButtonMapping != null)
                {
                    m_application.rightControllerButtonMapping.gameObject.SetActive(false);
                }

                // Determine whether we need a fade-out transition.
                bool needFadeOut = TeleportationSystem.NeedFadeOut; //m_application.ActiveProject != null) && (m_application.ActivePOI != null);

                if (needFadeOut)
                {
                    m_application.Logger.Debug("Fading out...");
                    m_application.m_fadeAnimator.ResetTrigger("FadeIn");
                    m_application.m_fadeAnimator.SetTrigger("FadeOut");
                }
                else
                {
                    m_application.Logger.Debug("No need to fade out.");
                    OnTeleportFadeOutComplete();
                }
            }

            public override void Exit()
            {
                m_application.Logger.Debug("ApplicationStateTeleporting.Exit()");

                //m_application.m_leftHandAnchor.SetActive(true);
                //m_application.m_rightHandAnchor.SetActive(true);

                m_application.m_leftControllerCanvas.SetActive(true);
                m_application.m_rightControllerCanvas.SetActive(true);

                if (m_application.leftControllerButtonMapping != null)
                {
                    m_application.leftControllerButtonMapping.gameObject.SetActive(true);
                }

                if (m_application.rightControllerButtonMapping != null)
                {
                    m_application.rightControllerButtonMapping.gameObject.SetActive(true);
                }
            }

            public override void Update()
            {
                //m_application.Logger.Debug("ApplicationStateTeleporting.Update()");
            }

            public override void UpdateModelLocationAndScale()
            {
                m_application.Logger.Debug("ApplicationStateTeleporting.UpdateModelLocationAndScale()");
            }

            public override void UpdateTrackingSpacePosition()
            {
                m_application.Logger.Debug("ApplicationStateTeleporting.UpdateTrackingSpacePosition()");
            }

            void InitButtonMappingUI()
            {
            }

            public override void OnTeleportFadeOutComplete()
            {
                m_application.Logger.Debug("ApplicationStateTeleporting.OnTeleportFadeOutComplete()");

                var application = (ApplicationArchiVR)m_application;

                if (
                    (application.ActiveProjectIndex != application.TeleportCommand.ProjectIndex) || // project changed
                    (application.ActivePOIName != application.TeleportCommand.POIName)) // poi changed
                {
                    application.StartCoroutine(application.Teleport());
                }
            }

            public override void OnTeleportFadeInComplete()
            {
                m_application.Logger.Debug("ApplicationStateTeleporting.OnTeleportFadeInComplete()");

                m_application.SetActiveApplicationState(UnityApplication.ApplicationStates.Default);
            }
        }
} // namespace ArchiVR.Application