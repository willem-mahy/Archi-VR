using WM.Application;

namespace ArchiVR.Application
{
    public class ApplicationStateTeleporting : ApplicationState
        {
            #region variables

            public ITeleportationSystem TeleportationSystem = null;

            #endregion

            public override void Init()
            {
                WM.Logger.Debug("ApplicationStateTeleporting.Init()");
            }

            public override void Enter()
            {
                WM.Logger.Debug("ApplicationStateTeleporting.Enter()");

                //m_application.m_leftHandAnchor.SetActive(false); // TODO? Hide only button labels?
                //m_application.m_rightHandAnchor.SetActive(false);

                m_application.m_leftControllerCanvas.SetActive(false);
                m_application.m_rightControllerCanvas.SetActive(false);

                m_application.leftControllerButtonMapping.gameObject.SetActive(false);
                m_application.rightControllerButtonMapping.gameObject.SetActive(false);

                // Determine wheter we need a fading transition.
                bool needFade = TeleportationSystem.NeedFadeOut; //m_application.ActiveProject != null) && (m_application.ActivePOI != null);

                if (needFade)
                {
                    WM.Logger.Debug("Fading out...");
                    m_application.m_fadeAnimator.ResetTrigger("FadeIn");
                    m_application.m_fadeAnimator.SetTrigger("FadeOut");
                }
                else
                {
                    WM.Logger.Debug("No need to fade out.");
                    OnTeleportFadeOutComplete();
                }
            }

            public override void Exit()
            {
                WM.Logger.Debug("ApplicationStateTeleporting.Exit()");

                //m_application.m_leftHandAnchor.SetActive(true);
                //m_application.m_rightHandAnchor.SetActive(true);

                m_application.m_leftControllerCanvas.SetActive(true);
                m_application.m_rightControllerCanvas.SetActive(true);

                m_application.leftControllerButtonMapping.gameObject.SetActive(true);
                m_application.rightControllerButtonMapping.gameObject.SetActive(true);
            }

            public override void Update()
            {
                //WM.Logger.Debug("ApplicationStateTeleporting.Update()");
            }

            public override void UpdateModelLocationAndScale()
            {
                WM.Logger.Debug("ApplicationStateTeleporting.UpdateModelLocationAndScale()");
            }

            public override void UpdateTrackingSpacePosition()
            {
                WM.Logger.Debug("ApplicationStateTeleporting.UpdateTrackingSpacePosition()");
            }

            void InitButtonMappingUI()
            {
            }

            public override void OnTeleportFadeOutComplete()
            {
                WM.Logger.Debug("ApplicationStateTeleporting.OnTeleportFadeOutComplete()");

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
                WM.Logger.Debug("ApplicationStateTeleporting.OnTeleportFadeInComplete()");

                m_application.SetActiveApplicationState(UnityApplication.ApplicationStates.Default);
            }
        }
} // namespace WM