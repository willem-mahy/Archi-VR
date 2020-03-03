using UnityEngine;

namespace WM
{
    // The Quest controller buttons are mapped as follows:
    // button1 = A
    // button2 = B
    // button3 = X
    // button4 = Y
    // button5 = Right Hand Trigger
    // button6 = Left Hand Trigger
    // button7 = Right Index Trigger
    // button8 = Left Index Trigger
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

        
        public bool aButtonDown = false;
        public bool bButtonDown = false;
        public bool xButtonDown = false;
        public bool yButtonDown = false;
        public bool lHandTriggerDown = false;
        public bool rHandTriggerDown = false;
        public bool lIndexTriggerDown = false;
        public bool rIndexTriggerDown = false;
        public bool startButtonDown = false;
        public bool buttonOculusDown = false;
        public bool buttonThumbstickPDown = false;
        public bool buttonThumbstickSDown = false;

        public bool aButtonPressed = false;
        public bool bButtonPressed = false;
        public bool xButtonPressed = false;
        public bool yButtonPressed = false;
        public bool lHandTriggerPressed = false;
        public bool rHandTriggerPressed = false;
        public bool lIndexTriggerPressed = false;
        public bool rIndexTriggerPressed = false;
        public bool startButtonPressed = false;
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

            aButtonDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(button1ID);
            bButtonDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(button2ID);
            xButtonDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(button3ID);
            yButtonDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(button4ID);
            lHandTriggerDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(button5ID);
            rHandTriggerDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(button6ID);
            lIndexTriggerDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(button7ID);
            rIndexTriggerDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(button8ID);

            startButtonDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(startButtonID);

            buttonThumbstickPDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(thumbstickPID);
            buttonThumbstickSDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(thumbstickSID);

            aButtonPressed = touchControllersConnected && UnityEngine.Input.GetButton(button1ID);
            bButtonPressed = touchControllersConnected && UnityEngine.Input.GetKey(button2ID);
            xButtonPressed = touchControllersConnected && UnityEngine.Input.GetButton(button3ID);
            yButtonPressed = touchControllersConnected && UnityEngine.Input.GetButton(button4ID);
            lHandTriggerPressed = touchControllersConnected && UnityEngine.Input.GetKey(button5ID);
            rHandTriggerPressed = touchControllersConnected && UnityEngine.Input.GetButton(button6ID);
            lIndexTriggerPressed = touchControllersConnected && UnityEngine.Input.GetButton(button7ID);
            rIndexTriggerPressed = touchControllersConnected && UnityEngine.Input.GetButton(button8ID);
            startButtonPressed = touchControllersConnected && UnityEngine.Input.GetButton(startButtonID);

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
            this.aButtonPressed = OVRInput.Get(OVRInput.Button.One);
            this.bButtonPressed = OVRInput.Get(OVRInput.Button.Two);
            this.xButtonPressed = OVRInput.Get(OVRInput.Button.Three);
            this.yButtonPressed = OVRInput.Get(OVRInput.Button.Four);
            lHandTriggerPressed = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger);
            rHandTriggerPressed = OVRInput.Get(OVRInput.Button.SecondaryHandTrigger);
            lIndexTriggerPressed = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger);
            rIndexTriggerPressed = OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);

            startButtonPressed = OVRInput.Get(OVRInput.Button.Start);

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
                aButtonDown = OVRInput.GetDown(OVRInput.Button.One);
                bButtonDown = OVRInput.GetDown(OVRInput.Button.Two);
                xButtonDown = OVRInput.GetDown(OVRInput.Button.Three);
                yButtonDown = OVRInput.GetDown(OVRInput.Button.Four);
                lHandTriggerDown = OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger);
                rHandTriggerDown = OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger);
                lIndexTriggerDown = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger);
                rIndexTriggerDown = OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);

                startButtonDown = OVRInput.GetDown(OVRInput.Button.Start);

                buttonThumbstickPDown = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch);
                buttonThumbstickSDown = OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch);
            }
            else
            {
                aButtonDown = (!prevState.aButtonDown && !prevState.aButtonPressed) && this.aButtonPressed;
                bButtonDown = (!prevState.bButtonDown && !prevState.bButtonPressed) && this.bButtonPressed;
                xButtonDown = (!prevState.xButtonDown && !prevState.xButtonPressed) && this.xButtonPressed;
                yButtonDown = (!prevState.yButtonDown && !prevState.yButtonPressed) && this.yButtonPressed;
                lHandTriggerDown = (!prevState.lHandTriggerDown && !prevState.lHandTriggerPressed) && lHandTriggerPressed;
                rHandTriggerDown = (!prevState.rHandTriggerDown && !prevState.rHandTriggerPressed) && rHandTriggerPressed;
                lIndexTriggerDown = (!prevState.lIndexTriggerDown && !prevState.lIndexTriggerPressed) && lIndexTriggerPressed;
                rIndexTriggerDown = (!prevState.rIndexTriggerDown && !prevState.rIndexTriggerPressed) && rIndexTriggerPressed;

                startButtonDown = (!prevState.startButtonDown && !prevState.startButtonPressed) && startButtonPressed;

                buttonThumbstickPDown = (!prevState.buttonThumbstickPDown && !prevState.lThumbstickPressed) && buttonThumbstickPPressed;
                buttonThumbstickSDown = (!prevState.buttonThumbstickSDown && !prevState.rThumbstickPressed) && buttonThumbstickSPressed;

                UpdateThumbStickDirectionDownStates(prevState);
            }

            lThumbstickDown = buttonThumbstickPDown;
            rThumbstickDown = buttonThumbstickSDown;


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
        ///     F1      button1 (X)
        ///     F2      button2 (Y)
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
        ///     F3      button3 (A)
        ///     F4      button4 (B)
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
            UpdateButtonState(KeyCode.F1, ref xButtonDown, ref xButtonPressed);
            UpdateButtonState(KeyCode.F2, ref yButtonDown, ref yButtonPressed);
            UpdateButtonState(KeyCode.F, ref lHandTriggerDown, ref lHandTriggerPressed);
            UpdateButtonState(KeyCode.R, ref lIndexTriggerDown, ref lIndexTriggerPressed);
            UpdateButtonState(KeyCode.F11, ref startButtonDown, ref startButtonPressed);
            UpdateButtonState(KeyCode.A, ref lThumbstickDown, ref lThumbstickPressed);

            if (Input.GetKey(KeyCode.Q)) lThumbStick.x -= 1;
            if (Input.GetKey(KeyCode.D)) lThumbStick.x += 1;
            if (Input.GetKey(KeyCode.S)) lThumbStick.y -= 1;
            if (Input.GetKey(KeyCode.Z)) lThumbStick.y += 1;

            // Right controller
            UpdateButtonState(KeyCode.F3, ref aButtonDown, ref aButtonPressed);
            UpdateButtonState(KeyCode.F4, ref bButtonDown, ref bButtonPressed);
            if (Input.GetMouseButtonDown(1)) rHandTriggerDown = true;
            if (Input.GetMouseButton(1)) rHandTriggerPressed = true;
            if (Input.GetMouseButtonDown(0)) rIndexTriggerDown = true;
            if (Input.GetMouseButton(0)) rIndexTriggerPressed = true;
            UpdateButtonState(KeyCode.F12, ref buttonOculusDown, ref buttonOculusPressed);

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
            xButtonDown =
            yButtonDown =
            lHandTriggerDown =
            lIndexTriggerDown =
            startButtonDown =
            
            lThumbstickDown =
            lThumbstickDirectionUpDown =
            lThumbstickDirectionDownDown =
            lThumbstickDirectionLeftDown =
            lThumbstickDirectionRightDown =

            // Right controller
            aButtonDown =
            bButtonDown =
            buttonOculusDown =
            rHandTriggerDown = 
            rIndexTriggerDown = 

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