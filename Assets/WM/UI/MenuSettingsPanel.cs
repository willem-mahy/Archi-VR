using UnityEngine;
using UnityEngine.UI;
using WM.Application;

namespace WM.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class MenuSettingsPanel : MonoBehaviour
    {
        #region Variables

        /// <summary>
        /// 
        /// </summary>
        public UnityApplication Application;

        /// <summary>
        /// 
        /// </summary>
        public Slider MenuSizeSlider;

        /// <summary>
        /// 
        /// </summary>
        public Slider MenuHeightSlider;

        #endregion

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

            MenuSizeSlider.onValueChanged.AddListener((float value) => { MenuSizeSliderOnValueChange(value); });

            MenuHeightSlider.onValueChanged.AddListener((float value) => { MenuHeightSliderOnValueChange(value); });

            if (UnityEngine.Application.isEditor)
            {
                MenuSizeSlider.maxValue = 0.005f;
            }
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        void Update()
        {
            MenuSizeSlider.SetValueWithoutNotify(Application.WorldSpaceMenu.gameObject.transform.localScale.x);
            MenuHeightSlider.SetValueWithoutNotify(Application.WorldSpaceMenu.Offset.y);
        }

        #region UI Event Handlers

        /// <summary>
        /// 'OnValueChange' handler for the 'Menu size' slider.
        /// </summary>
        public void MenuSizeSliderOnValueChange(float value)
        {
            Application.WorldSpaceMenu.gameObject.transform.localScale = new Vector3(MenuSizeSlider.value, MenuSizeSlider.value, MenuSizeSlider.value);
        }

        /// <summary>
        /// 'OnValueChange' handler for the 'Menu height' slider.
        /// </summary>
        /// <param name="value"></param>
        public void MenuHeightSliderOnValueChange(float value)
        {
            Application.WorldSpaceMenu.UpdateOffsetY(MenuHeightSlider.value);
        }

        #endregion UI Event Handlers
    }
} // namespace WM.UI