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

        /// <summary>
        /// 
        /// </summary>
        private void OnClickEditButton()
        {
            Application.Logger.Debug("SharedReferenceSystemPanel:OnClickEditButton()");

            Application.MenuVisible = false;
            Application.SetActiveApplicationState(2);
        }
    }
} // namespace WM.UI
