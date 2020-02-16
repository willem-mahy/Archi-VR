using UnityEngine;
using UnityEngine.UI;
using WM.Application;

namespace WM.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class SharedReferenceSystemPanel : MonoBehaviour
    {
        #region Variables

        public UnityApplication Application;

        public Button EditButton;

        #endregion

        #region GameObject overrides

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {
            #region Get references to GameObjects.

            if (Application == null)
            {
                Application = UtilUnity.FindGameObjectElseError(gameObject.scene, "Application").GetComponent<UnityApplication>();
            }

            #endregion

            #region Get references to UI components.

            if (EditButton == null)
            {
                var editButtonGO = UtilUnity.FindGameObjectElseError(gameObject.scene, "EditSharedReferenceSystemButton");

                if (editButtonGO != null)
                {
                    EditButton = editButtonGO.GetComponent<Button>();
                }
            }

            EditButton.onClick.AddListener(() => { OnClickEditButton(); });
            #endregion
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        void Update()
        {
        
        }

        #endregion GameObject overrides

        #region UI Event Handlers

        /// <summary>
        /// 'On Click' event handler for the 'Edit SRF' button.
        /// </summary>
        private void OnClickEditButton()
        {
            Application.Logger.Debug("SharedReferenceSystemPanel:OnClickEditButton()");

            Application.MenuVisible = false;

            // Prior to entering the 'EditSharedReference' application state,
            // we need to clear the 'Down' state for all controller buttons,
            // in order to prevent a spurious measurement of the first point at the location where the controller is located when clicking the 'Edit SRF' button.
            Application.m_controllerInput.ResetDownStates();

            // Start the 'EditSharedReference' application state,
            Application.SetActiveApplicationState(2);
        }

        #endregion UI Event Handlers
    }
} // namespace WM.UI
