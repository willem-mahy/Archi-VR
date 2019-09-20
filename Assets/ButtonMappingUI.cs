using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArchiVR
{
    public class ButtonMappingUI : MonoBehaviour
    {
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

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private float thumbDeadZone = 0.05f;

        public void Update(ControllerState controllerState)
        {
            // Left controller
            UpdateState(textButtonA, controllerState.button1Pressed);
            UpdateState(textButtonB, controllerState.button2Pressed);

            UpdateState(textButtonOculus, controllerState.buttonOculusPressed);

            UpdateState(textLeftHandTrigger, controllerState.button5Pressed);
            UpdateState(textLeftIndexTrigger, controllerState.button7Pressed);

            UpdateState(textLeftThumbDown, controllerState.lThumbStick.y < -thumbDeadZone);
            UpdateState(textLeftThumbUp, controllerState.lThumbStick.y > thumbDeadZone);

            UpdateState(textLeftThumbLeft, controllerState.lThumbStick.x < -thumbDeadZone);
            UpdateState(textLeftThumbRight, controllerState.lThumbStick.x > thumbDeadZone);

            // Right controller
            UpdateState(textButtonX, controllerState.button3Pressed);
            UpdateState(textButtonY, controllerState.button4Pressed);

            UpdateState(textButtonStart, controllerState.buttonStartPressed);

            UpdateState(textRightHandTrigger, controllerState.button6Pressed);
            UpdateState(textRightIndexTrigger, controllerState.button8Pressed);

            UpdateState(textRightThumbDown, controllerState.rThumbStick.y < -thumbDeadZone);
            UpdateState(textRightThumbUp, controllerState.rThumbStick.y > thumbDeadZone);

            UpdateState(textRightThumbLeft, controllerState.rThumbStick.x < -thumbDeadZone);
            UpdateState(textRightThumbRight, controllerState.rThumbStick.x > thumbDeadZone);
        }

        private void UpdateState(
            UnityEngine.UI.Text buttonText,
            bool pressed)
        {
            if (buttonText == null)
                return;

            buttonText.color = (pressed ? Color.green : Color.black);
        }
    }
}
