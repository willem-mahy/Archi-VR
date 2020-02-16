﻿using UnityEngine;

namespace WM
{
    public class ControllerState
    {
        #region Constants

        public const string button1ID = "joystick button 0";
        public const string button2ID = "joystick button 1";
        public const string button3ID = "joystick button 2";
        public const string button4ID = "joystick button 3";
        public const string button5ID = "joystick button 4";
        public const string button6ID = "joystick button 5";
        public const string button7ID = "joystick button 6";
        public const string button8ID = "joystick button 7";
        public const string button9ID = "joystick button 8";
        public const string button10ID = "joystick button 9";
        public const string startButtonID = button8ID;
        public const string thumbstickPID = button9ID;
        public const string thumbstickSID = button10ID;

        #endregion

        #region Fields

        public string[] joystickNames = null;

        public bool touchControllersConnected = false;

        // button1 = A
        // button2 = B
        // button3 = X
        // button4 = Y
        // button5 = Right Hand Trigger
        // button6 = Left Hand Trigger
        // button7 = Right Index Trigger
        // button8 = Left Index Trigger
        public bool button1Down = false;
        public bool button2Down = false;
        public bool button3Down = false;
        public bool button4Down = false;
        public bool button5Down = false;
        public bool button6Down = false;
        public bool button7Down = false;
        public bool button8Down = false;
        public bool buttonStartDown = false;
        public bool buttonOculusDown = false;
        public bool buttonThumbstickPDown = false;
        public bool buttonThumbstickSDown = false;

        public bool button1Pressed = false;
        public bool button2Pressed = false;
        public bool button3Pressed = false;
        public bool button4Pressed = false;
        public bool button5Pressed = false;
        public bool button6Pressed = false;
        public bool button7Pressed = false;
        public bool button8Pressed = false;
        public bool buttonStartPressed = false;
        public bool buttonOculusPressed = false;
        public bool buttonThumbstickPPressed = false;
        public bool buttonThumbstickSPressed = false;

        public bool leftControllerActive = false;
        public bool rightControllerActive = false;

        public OVRInput.Controller activeController = OVRInput.Controller.None;

        public bool lRemoteConnected = false;
        public bool rRemoteConnected = false;

        public bool lTouchConnected = false;
        public bool rTouchConnected = false;

        public bool rawButtonLIndexTriggerDown = false;
        public bool rawButtonRIndexTriggerDown = false;

        public Vector2 lThumbStick;
        public Vector2 rThumbStick;

        public bool lThumbstickDirectionLeftDown = false;
        public bool lThumbstickDirectionRightDown = false;
        public bool lThumbstickDirectionUpDown = false;
        public bool lThumbstickDirectionDownDown = false;

        public bool lThumbstickDirectionLeftPressed = false;
        public bool lThumbstickDirectionRightPressed = false;
        public bool lThumbstickDirectionUpPressed = false;
        public bool lThumbstickDirectionDownPressed = false;

        public bool lThumbstickDown = false;

        public bool lThumbstickPressed = false;

        public bool rThumbstickDirectionLeftDown = false;
        public bool rThumbstickDirectionRightDown = false;
        public bool rThumbstickDirectionUpDown = false;
        public bool rThumbstickDirectionDownDown = false;

        public bool rThumbstickDirectionLeftPressed = false;
        public bool rThumbstickDirectionRightPressed = false;
        public bool rThumbstickDirectionUpPressed = false;
        public bool rThumbstickDirectionDownPressed = false;

        public bool rThumbstickDown = false;

        public bool rThumbstickPressed = false;

        #endregion Fields

        #region Public API

        /// <summary>
        /// Updates the controller state using the Unity API.
        /// </summary>
        public void Update_Unity()
        {
            // Get the names of the currently connected joystick devices.          
            joystickNames = UnityEngine.Input.GetJoystickNames();

            touchControllersConnected = joystickNames.Length == 2;

            button1Down = touchControllersConnected && UnityEngine.Input.GetKeyDown(button1ID);
            button2Down = touchControllersConnected && UnityEngine.Input.GetKeyDown(button2ID);
            button3Down = touchControllersConnected && UnityEngine.Input.GetKeyDown(button3ID);
            button4Down = touchControllersConnected && UnityEngine.Input.GetKeyDown(button4ID);
            button5Down = touchControllersConnected && UnityEngine.Input.GetKeyDown(button5ID);
            button6Down = touchControllersConnected && UnityEngine.Input.GetKeyDown(button6ID);
            button7Down = touchControllersConnected && UnityEngine.Input.GetKeyDown(button7ID);
            button8Down = touchControllersConnected && UnityEngine.Input.GetKeyDown(button8ID);

            buttonStartDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(startButtonID);

            buttonThumbstickPDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(thumbstickPID);
            buttonThumbstickSDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(thumbstickSID);

            button1Pressed = touchControllersConnected && UnityEngine.Input.GetButton(button1ID);
            button2Pressed = touchControllersConnected && UnityEngine.Input.GetKey(button2ID);
            button3Pressed = touchControllersConnected && UnityEngine.Input.GetButton(button3ID);
            button4Pressed = touchControllersConnected && UnityEngine.Input.GetButton(button4ID);
            button5Pressed = touchControllersConnected && UnityEngine.Input.GetKey(button5ID);
            button6Pressed = touchControllersConnected && UnityEngine.Input.GetButton(button6ID);
            button7Pressed = touchControllersConnected && UnityEngine.Input.GetButton(button7ID);
            button8Pressed = touchControllersConnected && UnityEngine.Input.GetButton(button8ID);
            buttonStartPressed = touchControllersConnected && UnityEngine.Input.GetButton(startButtonID);

            leftControllerActive = touchControllersConnected && joystickNames[0] == "";
            rightControllerActive = joystickNames.Length == 2 && joystickNames[1] == "";
        }

        /// <summary>
        /// Updates the controller state using the OVRInput API.
        /// </summary>
        /// <param name="prevState"></param>
        public void Update_OVR(ControllerState prevState)
        {
            var activeController = OVRInput.GetActiveController();

            lRemoteConnected = OVRInput.IsControllerConnected(OVRInput.Controller.LTrackedRemote);
            rRemoteConnected = OVRInput.IsControllerConnected(OVRInput.Controller.RTrackedRemote);
            lTouchConnected = OVRInput.IsControllerConnected(OVRInput.Controller.LTouch);
            rTouchConnected = OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);

            // Get existing button presses.
            button1Pressed = OVRInput.Get(OVRInput.Button.One);
            button2Pressed = OVRInput.Get(OVRInput.Button.Two);
            button3Pressed = OVRInput.Get(OVRInput.Button.Three);
            button4Pressed = OVRInput.Get(OVRInput.Button.Four);
            button5Pressed = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger);
            button6Pressed = OVRInput.Get(OVRInput.Button.SecondaryHandTrigger);
            button7Pressed = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger);
            button8Pressed = OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);

            buttonStartPressed = OVRInput.Get(OVRInput.Button.Start);

            buttonThumbstickPPressed = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch);
            buttonThumbstickSPressed = OVRInput.Get(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch);

            var lButton1Pressed = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTrackedRemote);
            var rButton1Pressed = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTrackedRemote);
            var lButton2Pressed = OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTrackedRemote);
            var rButton2Pressed = OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTrackedRemote);

            var aButtonPressed = OVRInput.Get(OVRInput.RawButton.A);
            var bButtonPressed = OVRInput.Get(OVRInput.RawButton.B);
            var xButtonPressed = OVRInput.Get(OVRInput.RawButton.X);
            var yButtonPressed = OVRInput.Get(OVRInput.RawButton.Y);

            // returns true if the left index finger trigger has been pressed more than halfway.  
            // (Interpret the trigger as a button).
            var rawButtonLIndexTriggerDown = OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger);

            // returns true if the left index finger trigger has been pressed more than halfway.  
            // (Interpret the trigger as a button).
            var rawButtonRIndexTriggerDown = OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger);

            lThumbstickDirectionUpPressed = OVRInput.Get(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.Touch);
            lThumbstickDirectionDownPressed = OVRInput.Get(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.Touch);
            lThumbstickDirectionLeftPressed = OVRInput.Get(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.Touch);
            lThumbstickDirectionRightPressed = OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.Touch);

            rThumbstickDirectionUpPressed = OVRInput.Get(OVRInput.Button.SecondaryThumbstickUp, OVRInput.Controller.Touch);
            rThumbstickDirectionDownPressed = OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown, OVRInput.Controller.Touch);
            rThumbstickDirectionLeftPressed = OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft, OVRInput.Controller.Touch);
            rThumbstickDirectionRightPressed = OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight, OVRInput.Controller.Touch);

            // Get new button presses.
            bool GetDownWouldWork = false;
            if (GetDownWouldWork)
            {
                button1Down = OVRInput.GetDown(OVRInput.Button.One);
                button2Down = OVRInput.GetDown(OVRInput.Button.Two);
                button3Down = OVRInput.GetDown(OVRInput.Button.Three);
                button4Down = OVRInput.GetDown(OVRInput.Button.Four);
                button5Down = OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger);
                button6Down = OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger);
                button7Down = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger);
                button8Down = OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);

                buttonStartDown = OVRInput.GetDown(OVRInput.Button.Start);

                buttonThumbstickPDown = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch);
                buttonThumbstickSDown = OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch);
            }
            else
            {
                button1Down = (!prevState.button1Down && !prevState.button1Pressed) && button1Pressed;
                button2Down = (!prevState.button2Down && !prevState.button2Pressed) && button2Pressed;
                button3Down = (!prevState.button3Down && !prevState.button3Pressed) && button3Pressed;
                button4Down = (!prevState.button4Down && !prevState.button4Pressed) && button4Pressed;
                button5Down = (!prevState.button5Down && !prevState.button5Pressed) && button5Pressed;
                button6Down = (!prevState.button6Down && !prevState.button6Pressed) && button6Pressed;
                button7Down = (!prevState.button7Down && !prevState.button7Pressed) && button7Pressed;
                button8Down = (!prevState.button8Down && !prevState.button8Pressed) && button8Pressed;

                buttonStartDown = (!prevState.buttonStartDown && !prevState.buttonStartPressed) && buttonStartPressed;

                buttonThumbstickPDown = (!prevState.buttonThumbstickPDown && !prevState.lThumbstickPressed) && buttonThumbstickPPressed;
                buttonThumbstickSDown = (!prevState.buttonThumbstickSDown && !prevState.rThumbstickPressed) && buttonThumbstickPPressed;

                UpdateThumbStickDirectionDownStates(prevState);
            }

            // Get thumbstick positions. (X/Y range of -1.0f to 1.0f)
            lThumbStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            rThumbStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        }

        /// <summary>
        /// Updates the controller state using Unity API (keyboard and mouse) while running in editor.
        ///
        /// Mapping of keyboard and mouse is as follows:
        /// 
        /// Left controller
        ///     F1      button3 (X)
        ///     F2      button4 (Y)
        ///     F       button5 (Hand Trigger)
        ///     R       button7 (Index Trigger)
        ///     F11     buttonStart
        ///     A       lThumbstick Pressed
        ///     Q       lThumbStickLeft
        ///     D       lThumbStickRight
        ///     S       lThumbStickDown
        ///     Z       lThumbStickUp
        ///     
        /// Right Controller
        ///     F3      button1 (A)
        ///     F4      button2 (B)
        ///     RMB     button6 (Hand trigger)
        ///     LMB     button8 (Index trigger)
        ///     F12     buttonOculus
        ///     A       rThumbstick Pressed
        ///     Left    rThumbStickLeft
        ///     Right   rThumbStickRight
        ///     Down    rThumbStickDown
        ///     Up      rThumbStickUp
        /// </summary>
        public void Update_Editor(ControllerState prevState)
        {
            // Left controller
            UpdateButtonState(KeyCode.F1, ref button3Down, ref button3Pressed);
            UpdateButtonState(KeyCode.F2, ref button4Down, ref button4Pressed);
            UpdateButtonState(KeyCode.F, ref button5Down, ref button5Pressed);
            UpdateButtonState(KeyCode.R, ref button7Down, ref button7Pressed);
            UpdateButtonState(KeyCode.F11, ref buttonStartDown, ref buttonStartPressed);
            UpdateButtonState(KeyCode.A, ref lThumbstickDown, ref lThumbstickPressed);

            if (Input.GetKey(KeyCode.Q)) lThumbStick.x -= 1;
            if (Input.GetKey(KeyCode.D)) lThumbStick.x += 1;
            if (Input.GetKey(KeyCode.S)) lThumbStick.y -= 1;
            if (Input.GetKey(KeyCode.Z)) lThumbStick.y += 1;

            // Right controller
            UpdateButtonState(KeyCode.F3, ref button1Down, ref button1Pressed);
            UpdateButtonState(KeyCode.F4, ref button2Down, ref button2Pressed);
            UpdateButtonState(KeyCode.F12, ref buttonOculusDown, ref buttonOculusPressed);
            if (Input.GetMouseButtonDown(1)) button6Down = true;
            if (Input.GetMouseButton(1)) button6Pressed = true;
            if (Input.GetMouseButtonDown(0)) button8Down = true;
            if (Input.GetMouseButton(0)) button8Pressed = true;

            if (Input.GetKey(KeyCode.LeftArrow)) rThumbStick.x -= 1;
            if (Input.GetKey(KeyCode.RightArrow)) rThumbStick.x += 1;
            if (Input.GetKey(KeyCode.DownArrow)) rThumbStick.y -= 1;
            if (Input.GetKey(KeyCode.UpArrow)) rThumbStick.y += 1;

            UpdateButtonState(KeyCode.Plus, ref rThumbstickDown, ref rThumbstickPressed); 
            
            if (Input.GetMouseButtonDown(2)) rThumbstickDown = true;
            if (Input.GetMouseButton(2)) rThumbstickPressed = true;

            lThumbstickDirectionUpPressed       = lThumbStick.y > 0.5;
            lThumbstickDirectionDownPressed     = lThumbStick.y < -0.5;
            lThumbstickDirectionLeftPressed     = lThumbStick.x < -0.5;
            lThumbstickDirectionRightPressed    = lThumbStick.x > 0.5;

            rThumbstickDirectionUpPressed       = rThumbStick.y > 0.5;
            rThumbstickDirectionDownPressed     = rThumbStick.y < -0.5;
            rThumbstickDirectionLeftPressed     = rThumbStick.x < -0.5;
            rThumbstickDirectionRightPressed    = rThumbStick.x > 0.5;

            UpdateThumbStickDirectionDownStates(prevState);
        }

        /// <summary>
        /// Reset (set to 'false') the 'down' state for all controller buttons.
        /// </summary>
        public void ResetDownStates()
        {
            // Left controller
            button3Down =
            button4Down =
            button5Down =
            button7Down =
            buttonStartDown =
            
            lThumbstickDown =
            lThumbstickDirectionUpDown =
            lThumbstickDirectionDownDown =
            lThumbstickDirectionLeftDown =
            lThumbstickDirectionRightDown =

            // Right controller
            button1Down =
            button2Down =
            buttonOculusDown =
            button6Down = 
            button8Down = 

            rThumbstickDown =
            rThumbstickDirectionUpDown =
            rThumbstickDirectionDownDown =
            rThumbstickDirectionLeftDown =
            rThumbstickDirectionRightDown = false;
        }

        #endregion Public API

        #region Private API

        /// <summary>
        /// Updates the state of a simulated controller button, from the state of a mapped KB button.
        /// </summary>
        /// <param name="kc"></param>
        /// <param name="buttonDown"></param>
        /// <param name="buttonPressed"></param>
        private static void UpdateButtonState(
            KeyCode kc,
            ref bool buttonDown,
            ref bool buttonPressed)
        {
            if (Input.GetKeyDown(kc)) buttonDown = true;
            if (Input.GetKey(kc)) buttonPressed = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prevState"></param>
        private void UpdateThumbStickDirectionDownStates(ControllerState prevState)
        {
            lThumbstickDirectionUpDown = (!prevState.lThumbstickDirectionUpDown && !prevState.lThumbstickDirectionUpPressed) && lThumbstickDirectionUpPressed;
            lThumbstickDirectionDownDown = (!prevState.lThumbstickDirectionDownDown && !prevState.lThumbstickDirectionDownPressed) && lThumbstickDirectionDownPressed;
            lThumbstickDirectionLeftDown = (!prevState.lThumbstickDirectionLeftDown && !prevState.lThumbstickDirectionLeftPressed) && lThumbstickDirectionLeftPressed;
            lThumbstickDirectionRightDown = (!prevState.lThumbstickDirectionRightDown && !prevState.lThumbstickDirectionRightPressed) && lThumbstickDirectionRightPressed;

            rThumbstickDirectionUpDown = (!prevState.rThumbstickDirectionUpDown && !prevState.rThumbstickDirectionUpPressed) && rThumbstickDirectionUpPressed;
            rThumbstickDirectionDownDown = (!prevState.rThumbstickDirectionDownDown && !prevState.rThumbstickDirectionDownPressed) && rThumbstickDirectionDownPressed;
            rThumbstickDirectionLeftDown = (!prevState.rThumbstickDirectionLeftDown && !prevState.rThumbstickDirectionLeftPressed) && rThumbstickDirectionLeftPressed;
            rThumbstickDirectionRightDown = (!prevState.rThumbstickDirectionRightDown && !prevState.rThumbstickDirectionRightPressed) && rThumbstickDirectionRightPressed;
        }

        #endregion Private API
    }
}