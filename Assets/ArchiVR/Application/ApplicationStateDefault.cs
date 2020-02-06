using WM.Application;

namespace ArchiVR.Application
{
    public class ApplicationStateDefault : ApplicationState
    {
        #region variables
        
        #endregion

        public override void Init()
        {
            m_application.Logger.Debug("ApplicationStateDefault.Init()");
        }

        public override void Enter()
        {
            m_application.Logger.Debug("ApplicationStateDefault.Enter()");

            var aim = ((ApplicationArchiVR)m_application).ActiveImmersionMode;

            if (aim != null)
            {
                aim.InitButtonMappingUI();
            }
        }

        public override void Exit()
        {
            m_application.Logger.Debug("ApplicationStateDefault.Exit()");
        }

        public override void Update()
        {
            //m_application.Logger.Debug("ApplicationStateDefault.Update()");

            var aim = ((ApplicationArchiVR)m_application).ActiveImmersionMode;

            if (aim != null)
            {
                aim.Update();
            }
        }

        public override void UpdateModelLocationAndScale()
        {
            m_application.Logger.Debug("ApplicationStateDefault.UpdateModelLocationAndScale()");

            var aim = ((ApplicationArchiVR)m_application).ActiveImmersionMode;

            if (aim != null)
            {
                aim.UpdateModelLocationAndScale();
            }
        }

        public override void UpdateTrackingSpacePosition()
        {
            m_application.Logger.Debug("ApplicationStateDefault.UpdateTrackingSpacePosition()");

            var aim = ((ApplicationArchiVR)m_application).ActiveImmersionMode;

            if (aim != null)
            {
                aim.UpdateTrackingSpacePosition();
            }
        }

        void InitButtonMappingUI()
        {
            m_application.Logger.Debug("ApplicationStateDefault.InitButtonMappingUI()");

            var aim = ((ApplicationArchiVR)m_application).ActiveImmersionMode;

            if (aim != null)
            {
                aim.InitButtonMappingUI();
            }
        }

        public override void OnTeleportFadeOutComplete()
        {
        }

        public override void OnTeleportFadeInComplete()
        {
        }
    }
} // namespace ArchiVR.Application