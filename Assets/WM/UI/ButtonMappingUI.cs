using UnityEngine;
using UnityEngine.UI;

namespace WM.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class ButtonMappingUI : MonoBehaviour
    {
        #region Fields

        public enum Side
        {
            Left,
            Right
        }

        /// <summary>
        /// 
        /// </summary>
        public Side Hand = ButtonMappingUI.Side.Left;

        /*! The size of the 'Dead zone' for thumb sticks.
            *
            * As long as the absolute value of a a thumb stick axis
            * is smaller than this value, the axis button is considered 'not pressed'. 
            */
        private readonly float thumbDeadZone = 0.2f;

        #region Colors

        //! The color for down buttons' labels.
        public Color DownColor { get; set; } = Color.yellow;

        //! The color for up buttons' labels.
        public Color UpColor { get; set; } = Color.red;

        //! The color for pressed buttons' labels.
        public Color PressedColor { get; set; } = Color.green;

        //! The color for non-pressed buttons' labels.
        public Color DefaultColor { get; set; } = Color.black;

        #endregion

        #region Handles to Button UI components

        // The buttons
        public ControllerButtonUI ButtonOculusStart;
        public ControllerButtonUI ButtonXA;
        public ControllerButtonUI ButtonYB;
        public ControllerButtonUI HandTrigger;
        public ControllerButtonUI IndexTrigger;
        public ControllerButtonUI ThumbUp;
        public ControllerButtonUI ThumbDown;
        public ControllerButtonUI ThumbLeft;
        public ControllerButtonUI ThumbRight;

        #region Aliases for buttons with different names Left <> Righ controller

        public ControllerButtonUI ButtonStart => ButtonOculusStart;

        public ControllerButtonUI ButtonX => ButtonXA;
        public ControllerButtonUI ButtonA => ButtonXA;

        public ControllerButtonUI ButtonY => ButtonYB;
        public ControllerButtonUI ButtonB => ButtonYB;

        #endregion Aliases for buttons with different names Left <> Righ controller

        #endregion Handles to Button UI components

        #endregion Fields

        #region Public API

        #region GameObject overrides

        private void Start()
        {

        }

        #endregion GameObject overrides

        /// <summary>
        /// Updates the button mapping labels to the pressed state of their corresponding button.
        /// </summary>
        /// <param name="controllerState"></param>
        public void SetControllerState(ControllerState controllerState)
        {
            switch (Hand)
            {
                case Side.Left:
                    UpdateState(ButtonX, controllerState.xButtonPressed);
                    UpdateState(ButtonY, controllerState.yButtonPressed);

                    UpdateState(ButtonStart, controllerState.startButtonPressed);

                    UpdateState(HandTrigger, controllerState.lHandTriggerPressed);
                    UpdateState(IndexTrigger, controllerState.lIndexTriggerPressed);

                    UpdateState(ThumbDown, controllerState.lThumbStick.y < -thumbDeadZone);
                    UpdateState(ThumbUp, controllerState.lThumbStick.y > thumbDeadZone);

                    UpdateState(ThumbLeft, controllerState.lThumbStick.x < -thumbDeadZone);
                    UpdateState(ThumbRight, controllerState.lThumbStick.x > thumbDeadZone);
                    break;
                case Side.Right:
                    UpdateState(ButtonA, controllerState.aButtonPressed);
                    UpdateState(ButtonB, controllerState.bButtonPressed);

                    UpdateState(ButtonOculusStart, controllerState.buttonOculusPressed);

                    UpdateState(HandTrigger, controllerState.rHandTriggerPressed);
                    UpdateState(IndexTrigger, controllerState.rIndexTriggerPressed);

                    UpdateState(ThumbDown, controllerState.rThumbStick.y < -thumbDeadZone);
                    UpdateState(ThumbUp, controllerState.rThumbStick.y > thumbDeadZone);

                    UpdateState(ThumbLeft, controllerState.rThumbStick.x < -thumbDeadZone);
                    UpdateState(ThumbRight, controllerState.rThumbStick.x > thumbDeadZone);
                    break;
            }
        }

        #endregion Public API

        #region Non-public API

        /// <summary>
        /// Updates the given button mapping label to the given pressed state.
        /// </summary>
        /// <param name="controllerButtonUI"></param>
        /// <param name="pressed"></param>
        private void UpdateState(
            ControllerButtonUI controllerButtonUI,
            bool pressed) // TODO: refactor to a four-state ButtonState (default/down/pressed/up), and use all four colors...
        {
            if (controllerButtonUI == null)
            {
                return;
            }

            controllerButtonUI.TextColor = (pressed ? PressedColor : DefaultColor);
        }

        #endregion Non-public API
    }
}
