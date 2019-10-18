namespace WM
{
    namespace ArchiVR
    {
        public class ApplicationStateTeleporting : ApplicationState
        {
            #region variables

            #endregion

            public override void Init()
            {
                Logger.Debug("ApplicationStateTeleporting.Init()");
            }

            public override void Enter()
            {
                Logger.Debug("ApplicationStateTeleporting.Enter()");


                //m_application.m_leftHandAnchor.SetActive(false); // TODO? Hide only button labels?
                //m_application.m_rightHandAnchor.SetActive(false);

                m_application.m_leftControllerCanvas.SetActive(false);
                m_application.m_rightControllerCanvas.SetActive(false);

                m_application.leftControllerButtonMapping.gameObject.SetActive(false);
                m_application.rightControllerButtonMapping.gameObject.SetActive(false);

                // Determine wheter we need a fading transition.
                bool needFade =
                    (m_application.ActiveProject != null) && (m_application.ActivePOI != null);

                if (needFade)
                {
                    Logger.Debug("Fading out...");
                    m_application.m_fadeAnimator.ResetTrigger("FadeIn");
                    m_application.m_fadeAnimator.SetTrigger("FadeOut");
                }
                else
                {
                    Logger.Debug("No need to fade out.");
                    OnTeleportFadeOutComplete();
                }
            }

            public override void Exit()
            {
                Logger.Debug("ApplicationStateTeleporting.Exit()");

                //m_application.m_leftHandAnchor.SetActive(true);
                //m_application.m_rightHandAnchor.SetActive(true);

                m_application.m_leftControllerCanvas.SetActive(true);
                m_application.m_rightControllerCanvas.SetActive(true);

                m_application.leftControllerButtonMapping.gameObject.SetActive(true);
                m_application.rightControllerButtonMapping.gameObject.SetActive(true);
            }

            public override void Update()
            {
                //Logger.Debug("ApplicationStateTeleporting.Update()");
            }

            public override void UpdateModelLocationAndScale()
            {
                Logger.Debug("ApplicationStateTeleporting.UpdateModelLocationAndScale()");
            }

            public override void UpdateTrackingSpacePosition()
            {
                Logger.Debug("ApplicationStateTeleporting.UpdateTrackingSpacePosition()");
            }

            void InitButtonMappingUI()
            {
            }

            public override void OnTeleportFadeOutComplete()
            {
                Logger.Debug("ApplicationStateTeleporting.OnTeleportFadeOutComplete()");

                if (
                    (m_application.ActiveProjectIndex != m_application.TeleportCommand.ProjectIndex) || // project changed
                    (m_application.ActivePOIName != m_application.TeleportCommand.POIName)) // poi changed
                {
                    m_application.StartCoroutine(m_application.Teleport());
                }
            }

            public override void OnTeleportFadeInComplete()
            {
                Logger.Debug("ApplicationStateTeleporting.OnTeleportFadeInComplete()");

                m_application.SetActiveApplicationState(ApplicationArchiVR.ApplicationStates.Default);
            }
        }
    } // namespace ArchiVR
} // namespace WM