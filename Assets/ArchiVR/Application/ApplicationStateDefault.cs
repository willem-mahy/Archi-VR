using WM.Application;

namespace ArchiVR.Application
{
    public class ApplicationStateDefault : ApplicationState
    {
        #region variables
        
        #endregion

        public override void Init()
        {
            WM.Logger.Debug("ApplicationStateDefault.Init()");
        }

        public override void Enter()
        {
            WM.Logger.Debug("ApplicationStateDefault.Enter()");

            var aim = ((ApplicationArchiVR)m_application).ActiveImmersionMode;

            if (aim != null)
            {
                aim.InitButtonMappingUI();
            }
        }

        public override void Exit()
        {
            WM.Logger.Debug("ApplicationStateDefault.Exit()");
        }

        public override void Update()
        {
            //Logger.Debug("ApplicationStateDefault.Update()");

            var aim = ((ApplicationArchiVR)m_application).ActiveImmersionMode;

            if (aim != null)
            {
                aim.Update();
            }
        }

        public override void UpdateModelLocationAndScale()
        {
            WM.Logger.Debug("ApplicationStateDefault.UpdateModelLocationAndScale()");

            var aim = ((ApplicationArchiVR)m_application).ActiveImmersionMode;

            if (aim != null)
            {
                aim.UpdateModelLocationAndScale();
            }
        }

        public override void UpdateTrackingSpacePosition()
        {
            WM.Logger.Debug("ApplicationStateDefault.UpdateTrackingSpacePosition()");

            var aim = ((ApplicationArchiVR)m_application).ActiveImmersionMode;

            if (aim != null)
            {
                aim.UpdateTrackingSpacePosition();
            }
        }

        void InitButtonMappingUI()
        {
            WM.Logger.Debug("ApplicationStateDefault.InitButtonMappingUI()");

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
} // namespace ArchiVR