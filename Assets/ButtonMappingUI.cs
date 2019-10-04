using UnityEngine;

namespace ArchiVR
{
    public class ButtonMappingUI : MonoBehaviour
    {
        #region Variables

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

        #region Handles to Button UI Labels

        // Left Controller
        public UnityEngine.UI.Text textButtonStart = null;
        public UnityEngine.UI.Text textButtonX = null;
        public UnityEngine.UI.Text textButtonY = null;
        public UnityEngine.UI.Text textLeftHandTrigger = null;
        public UnityEngine.UI.Text textLeftIndexTrigger = null;
        public UnityEngine.UI.Text textLeftThumbUp = null;
        public UnityEngine.UI.Text textLeftThumbDown = null;
        public UnityEngine.UI.Text textLeftThumbLeft = null;
        public UnityEngine.UI.Text textLeftThumbRight = null;

        // Right controller
        public UnityEngine.UI.Text textButtonOculus = null;
        public UnityEngine.UI.Text textButtonA = null;
        public UnityEngine.UI.Text textButtonB = null;
        public UnityEngine.UI.Text textRightHandTrigger = null;
        public UnityEngine.UI.Text textRightIndexTrigger = null;
        public UnityEngine.UI.Text textRightThumbUp = null;
        public UnityEngine.UI.Text textRightThumbDown = null;
        public UnityEngine.UI.Text textRightThumbLeft = null;
        public UnityEngine.UI.Text textRightThumbRight = null;

        #endregion

        #endregion

        //! Start is called before the first frame update
        void Start()
        {
        
        }

        //! Update is called once per frame
        void Update()
        {
        
        }

        //! Updates the button mapping labels to the pressed state of their corresponding button.
        public void Update(ControllerState controllerState)
        {
            #region Left controller

            UpdateState(textButtonA, controllerState.button1Pressed);
            UpdateState(textButtonB, controllerState.button2Pressed);

            UpdateState(textButtonStart, controllerState.buttonStartPressed);

            UpdateState(textLeftHandTrigger, controllerState.button5Pressed);
            UpdateState(textLeftIndexTrigger, controllerState.button7Pressed);

            UpdateState(textLeftThumbDown, controllerState.lThumbStick.y < -thumbDeadZone);
            UpdateState(textLeftThumbUp, controllerState.lThumbStick.y > thumbDeadZone);

            UpdateState(textLeftThumbLeft, controllerState.lThumbStick.x < -thumbDeadZone);
            UpdateState(textLeftThumbRight, controllerState.lThumbStick.x > thumbDeadZone);

            #endregion

            #region Right controller

            UpdateState(textButtonX, controllerState.button3Pressed);
            UpdateState(textButtonY, controllerState.button4Pressed);

            UpdateState(textButtonOculus, controllerState.buttonOculusPressed);

            UpdateState(textRightHandTrigger, controllerState.button6Pressed);
            UpdateState(textRightIndexTrigger, controllerState.button8Pressed);

            UpdateState(textRightThumbDown, controllerState.rThumbStick.y < -thumbDeadZone);
            UpdateState(textRightThumbUp, controllerState.rThumbStick.y > thumbDeadZone);

            UpdateState(textRightThumbLeft, controllerState.rThumbStick.x < -thumbDeadZone);
            UpdateState(textRightThumbRight, controllerState.rThumbStick.x > thumbDeadZone);

            #endregion
        }

        //! Updates the given button mapping label to the given pressed state.
        private void UpdateState(
            UnityEngine.UI.Text buttonText,
            bool pressed) // TODO: refactor to a four-state ButtonState (default/down/pressed/up), and use all four colors...
        {
            if (buttonText == null)
                return;

            buttonText.color = (pressed ? PressedColor : DefaultColor);
        }
    }
}
