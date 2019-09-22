using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchiVR
{
    public class ApplicationStateLoadingProject : ApplicationState
    {
        #region variables


        #endregion

        public override void Init()
        {
            Logger.Debug("ApplicationStateLoadingProject.Init()");
        }

        public override void Enter()
        {
            Logger.Debug("ApplicationStateLoadingProject.Enter()");            
        }

        public override void Exit()
        {
            Logger.Debug("ApplicationStateLoadingProject.Exit()");
        }

        public override void Update()
        {
            //Logger.Debug("ApplicationStateLoadingProject.Update()");
        }

        public override void UpdateModelLocationAndScale()
        {
        }

        public override void UpdateTrackingSpacePosition()
        {
        }

        void InitButtonMappingUI()
        {
        }

        public override void OnTeleportFadeOutComplete()
        {
        }

        public override void OnTeleportFadeInComplete()
        {
        }
    }
}