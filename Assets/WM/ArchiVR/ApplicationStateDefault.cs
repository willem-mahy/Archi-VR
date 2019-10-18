using UnityEngine;

namespace WM
{
    namespace ArchiVR
    {
        public class ApplicationStateDefault : ApplicationState
        {
            #region variables
        
            #endregion

            public override void Init()
            {
                Logger.Debug("ApplicationStateDefault.Init()");
            }

            public override void Enter()
            {
                Logger.Debug("ApplicationStateDefault.Enter()");

                var aim = m_application.ActiveImmersionMode;

                if (aim != null)
                {
                    aim.InitButtonMappingUI();
                }
            }

            public override void Exit()
            {
                Logger.Debug("ApplicationStateDefault.Exit()");
            }

            public override void Update()
            {
                //Logger.Debug("ApplicationStateDefault.Update()");

                var aim = m_application.ActiveImmersionMode;

                if (aim != null)
                {
                    aim.Update();
                }
            }

            public override void UpdateModelLocationAndScale()
            {
                Logger.Debug("ApplicationStateDefault.UpdateModelLocationAndScale()");

                var aim = m_application.ActiveImmersionMode;

                if (aim != null)
                {
                    aim.UpdateModelLocationAndScale();
                }
            }

            public override void UpdateTrackingSpacePosition()
            {
                Logger.Debug("ApplicationStateDefault.UpdateTrackingSpacePosition()");

                var aim = m_application.ActiveImmersionMode;

                if (aim != null)
                {
                    aim.UpdateTrackingSpacePosition();
                }
            }

            void InitButtonMappingUI()
            {
                Logger.Debug("ApplicationStateDefault.InitButtonMappingUI()");

                var aim = m_application.ActiveImmersionMode;

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
} // namespace WM