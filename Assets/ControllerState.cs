using UnityEngine;

namespace ArchiVR
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

        #region Variables

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

        #endregion

        //! Updates the controller state using the Unity API.
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

        //! Updates the controller state using the OVRInput API.
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

                buttonStartDown = (!prevState.buttonStartDown && !prevState.buttonStartPressed) && buttonStartDown;

                buttonThumbstickPDown = (!prevState.buttonThumbstickPDown && !prevState.lThumbstickPressed) && buttonThumbstickPPressed;
                buttonThumbstickSDown = (!prevState.buttonThumbstickSDown && !prevState.rThumbstickPressed) && buttonThumbstickPPressed;

                lThumbstickDirectionUpDown = (!prevState.lThumbstickDirectionUpDown && !prevState.lThumbstickDirectionUpPressed) && lThumbstickDirectionUpPressed;
                lThumbstickDirectionDownDown = (!prevState.lThumbstickDirectionDownDown && !prevState.lThumbstickDirectionDownPressed) && lThumbstickDirectionDownPressed;
                lThumbstickDirectionLeftDown = (!prevState.lThumbstickDirectionLeftDown && !prevState.lThumbstickDirectionLeftPressed) && lThumbstickDirectionLeftPressed;
                lThumbstickDirectionRightDown = (!prevState.lThumbstickDirectionRightDown && !prevState.lThumbstickDirectionRightPressed) && lThumbstickDirectionRightPressed;

                rThumbstickDirectionUpDown = (!prevState.rThumbstickDirectionUpDown && !prevState.rThumbstickDirectionUpPressed) && rThumbstickDirectionUpPressed;
                rThumbstickDirectionDownDown = (!prevState.rThumbstickDirectionDownDown && !prevState.rThumbstickDirectionDownPressed) && rThumbstickDirectionDownPressed;
                rThumbstickDirectionLeftDown = (!prevState.rThumbstickDirectionLeftDown && !prevState.rThumbstickDirectionLeftPressed) && rThumbstickDirectionLeftPressed;
                rThumbstickDirectionRightDown = (!prevState.rThumbstickDirectionRightDown && !prevState.rThumbstickDirectionRightPressed) && rThumbstickDirectionRightPressed;
            }

            // Get thumbstick positions. (X/Y range of -1.0f to 1.0f)
            lThumbStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            rThumbStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        }

        public void Update_Editor()
        {
            // Left controller
            if (Input.GetKey(KeyCode.F1)) button3Pressed = true;
            if (Input.GetKey(KeyCode.F2)) button4Pressed = true;
           
            if (Input.GetKey(KeyCode.F)) button5Pressed = true;
            if (Input.GetKey(KeyCode.R)) button7Pressed = true;

            if (Input.GetKey(KeyCode.F11)) buttonStartPressed = true;

            if (Input.GetKeyDown(KeyCode.A)) lThumbstickDown = true;
            if (Input.GetKey(KeyCode.A)) lThumbstickPressed = true;

            if (Input.GetKey(KeyCode.Q)) lThumbStick.x -= 1;
            if (Input.GetKey(KeyCode.D)) lThumbStick.x += 1;
            if (Input.GetKey(KeyCode.S)) lThumbStick.y -= 1;
            if (Input.GetKey(KeyCode.Z)) lThumbStick.y += 1;

            // Right controller
            if (Input.GetKey(KeyCode.F3)) button1Pressed = true;
            if (Input.GetKey(KeyCode.F4)) button2Pressed = true;

            if (Input.GetKey(KeyCode.F12)) buttonOculusPressed = true;

            if (Input.GetKey(KeyCode.RightShift)) button6Pressed = true;
            if (Input.GetKey(KeyCode.Return)) button8Pressed = true;

            if (Input.GetKey(KeyCode.LeftArrow)) rThumbStick.x -= 1;
            if (Input.GetKey(KeyCode.RightArrow)) rThumbStick.x += 1;
            if (Input.GetKey(KeyCode.DownArrow)) rThumbStick.y -= 1;
            if (Input.GetKey(KeyCode.UpArrow)) rThumbStick.y += 1;

            if (Input.GetKeyDown(KeyCode.Plus)) rThumbstickDown = true;
            if (Input.GetKey(KeyCode.Plus)) rThumbstickPressed = true;
        }
    }
}