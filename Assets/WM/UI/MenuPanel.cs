using System;
using UnityEngine;

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
            var applicationGO = UtilUnity.FindGameObjectElseError(gameObject.scene, "Application");
            
            Application = applicationGO.GetComponent<T>();

            if (Application == null)
            {
                var errorMessage = "No component of type '" + typeof(T).ToString() + "' found on gameobject 'Application'!";
                Debug.LogError(errorMessage);
                throw new Exception(errorMessage);
            }
        }

        #endregion Public API
    }
}
