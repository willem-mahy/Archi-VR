using WM.Application;

[assembly: System.Reflection.AssemblyVersion("1.0.*")]

namespace Demo.Application
{
    public class ApplicationDemo : UnityApplication
    {
        #region Variables

        // The typed application states.
        public ApplicationStateDefault applicationStateDefault = new ApplicationStateDefault();
        
        #endregion

        /// <summary>
        /// Initialize all necessary stuff before the first frame update.
        /// </summary>
        public override void Init()
        {
            m_applicationStates.Add(applicationStateDefault);

            base.Init();

            SetActiveApplicationState(UnityApplication.ApplicationStates.Default);
        }
    };
}