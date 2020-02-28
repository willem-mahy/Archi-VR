using System;
using UnityEngine;
using WM.Application;

[assembly: System.Reflection.AssemblyVersion("1.0.*")]

namespace Demo.Application
{
    public class ApplicationDemo : UnityApplication
    {
        #region Variables

        /// <summary>
        /// The OVRManger prefab.
        /// </summary>
        public GameObject ovrManagerPrefab;

        #endregion

        /// <summary>
        /// <see cref="UnityApplication.Name"/> implementation.
        /// </summary>
        public override string Name
        {
            get
            {
                return "Demo";
            }
        }

        /// <summary>
        /// Initialize all necessary stuff before the first frame update.
        /// </summary>
        public override void Init()
        {
            if (OVRManager.instance == null)
            {
                // Instantiate at position (0, 0, 0) and zero rotation.
                Instantiate(ovrManagerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            }

            base.Init();

            PushApplicationState(new ApplicationStateDefault(this));
        }

        /// <summary>
        /// <see cref="UnityApplication.DefaultAvatarID"/> implementation.
        /// </summary>
        public override Guid DefaultAvatarID
        {
            get;
            set;
        } // TODO? = new Guid(...) ???

        /// <summary>
        /// <see cref="UnityApplication.OnEnableInputChanged()"/> implementation.
        /// </summary>
        protected override void OnEnableInputChanged()
        {
            m_ovrCameraRig.GetComponent<OVRCameraRig>().enabled = EnableInput;
            m_ovrCameraRig.GetComponent<OVRHeadsetEmulator>().enabled = EnableInput;
        }
    };
}