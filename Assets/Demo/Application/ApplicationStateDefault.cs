using WM.Application;

namespace Demo.Application
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
        }

        public override void Exit()
        {
            WM.Logger.Debug("ApplicationStateDefault.Exit()");
        }

        public override void Update()
        {
            //Logger.Debug("ApplicationStateDefault.Update()");

            m_application.Fly();

            m_application.UpdateTrackingSpace();
        }
    }
} // namespace Demo.Application