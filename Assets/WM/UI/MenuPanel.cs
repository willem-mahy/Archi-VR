using System;
using UnityEngine;
using WM.Application;

namespace WM.UI
{
    /// <summary>
    /// Base class for all menu panels in a UnityApplication.
    /// Holds a reference to the application, and makes sure it is initialized properly at strartup.
    /// </summary>
    public class MenuPanel<T> : MonoBehaviour
    {
        protected T Application;

        #region Public API

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        virtual public void Start()
        {
            Application = UtilUnity.FindApplication<T>(gameObject);
        }

        #endregion Public API
    }
}
