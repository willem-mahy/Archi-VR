using WM.Application;

namespace Demo.Application
{
    public class ApplicationStateDefault : ApplicationState<ApplicationDemo>
    {
        #region variables

        #endregion

        public ApplicationStateDefault(ApplicationDemo application) : base(application)
        {
        }

        public override void Init()
        {
            m_application.Logger.Debug("ApplicationStateDefault.Init()");
        }

        public override void Enter()
        {
            m_application.Logger.Debug("ApplicationStateDefault.Enter()");
        }

        public override void Exit()
        {
            m_application.Logger.Debug("ApplicationStateDefault.Exit()");
        }

        public override void Update()
        {
            //Logger.Debug("ApplicationStateDefault.Update()");

            m_application.Fly();

            m_application.UpdateTrackingSpace();
        }
    }
} // namespace Demo.Application