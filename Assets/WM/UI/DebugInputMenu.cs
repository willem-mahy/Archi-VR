using UnityEngine;
using UnityEngine.UI;
using WM.Application;

namespace WM.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class DebugInputMenu : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public UnityApplication Application;

        /// <summary>
        /// 
        /// </summary>
        public Text HeaderText = null;

        /// <summary>
        /// 
        /// </summary>
        public Text LControllerText = null;

        /// <summary>
        /// 
        /// </summary>
        public Text RControllerText = null;


        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {
            var controllerInput = Application.m_controllerInput;

            var controllerState = controllerInput.m_controllerState;

            var text = "";

            if (HeaderText != null)
            {
                text += "\nInput mode: " + (controllerInput.m_inputMode == ControllerInput.InputMode.Unity ? "Unity" : "OVR");
                text += "\n";
                //text += "\nRemote connection: L=" + (lRemoteConnected ? "OK" : "NA") + " R=" + (rRemoteConnected ? "OK" : "NA");
                text += "\nTouch controllers:" + (controllerState.lTouchConnected ? "L " : "") + " " + (controllerState.rTouchConnected ? " R" : "") +
                        "(Active Controller: " + (controllerState.activeController == OVRInput.Controller.LTouch ? " L" : "") + (controllerState.activeController == OVRInput.Controller.RTouch ? " R" : "") + ")";
                text += "\n";
                text += "\nThumbstick: L(" + controllerState.lThumbStick.x + ", " + controllerState.lThumbStick.y + ") R(" + controllerState.rThumbStick.x + ", " + controllerState.rThumbStick.y + ")";
                text += "\nL thumbstick:";
                text += "\n Left: " + GetButtonStateText(controllerState.lThumbstickDirectionLeftDown, controllerState.lThumbstickDirectionLeftPressed);
                text += "\n Right: " + GetButtonStateText(controllerState.lThumbstickDirectionRightDown, controllerState.lThumbstickDirectionRightPressed);
                text += "\n Up: " + GetButtonStateText(controllerState.lThumbstickDirectionUpDown, controllerState.lThumbstickDirectionUpPressed);
                text += "\n Down: " + GetButtonStateText(controllerState.lThumbstickDirectionDownDown, controllerState.lThumbstickDirectionDownPressed);

                if (controllerInput.m_inputMode == ControllerInput.InputMode.Unity)
                {
                    text += "\nJoysticks:";
                    foreach (var n in controllerState.joystickNames)
                    {
                        text += "\n -" + n;
                    }
                }
                else
                {
                    // index finger trigger’s current position (range of 0.0f to 1.0f)

                    // primary trigger (typically the Left) 
                    //text += "\nPrimary IndexTrigger = " + OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) + (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger) ? " Down" : ""); ;
                    // secondary trigger (typically the Right) 
                    //text += "\nSecondary IndexTrigger = " + OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) + (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) ? " Down" : ""); ;

                    // left
                    text += "\nL IndexTrigger = " + OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger) + (controllerState.rawButtonLIndexTriggerDown ? " Down" : "");

                    // right
                    text += "\nR IndexTrigger = " + OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) + (controllerState.rawButtonRIndexTriggerDown ? " Down" : "");

                    // returns true if the secondary gamepad button, typically “B”, is currently touched by the user.
                    //text += "\nGetTouchTwo = " + OVRInput.Get(OVRInput.Touch.Two);   
                }
            }
            HeaderText.text = text;

            text  = "\nButton 1 = " + GetButtonStateText(controllerState.button1Down, controllerState.button1Pressed);
            text += "\nButton 2 = " + GetButtonStateText(controllerState.button2Down, controllerState.button2Pressed);
            text += "\nButton 3 = " + GetButtonStateText(controllerState.button3Down, controllerState.button3Pressed);
            text += "\nButton 4 = " + GetButtonStateText(controllerState.button4Down, controllerState.button4Pressed);
            text += "\nButton Start = " + GetButtonStateText(controllerState.buttonStartDown, controllerState.buttonStartPressed);
            LControllerText.text = text;

            text  = "\nButton 5 = " + GetButtonStateText(controllerState.button5Down, controllerState.button5Pressed);
            text += "\nButton 6 = " + GetButtonStateText(controllerState.button6Down, controllerState.button6Pressed);
            text += "\nButton 7 = " + GetButtonStateText(controllerState.button7Down, controllerState.button7Pressed);
            text += "\nButton 8 = " + GetButtonStateText(controllerState.button8Down, controllerState.button8Pressed);            
            text += "\nButton Oculus = " + GetButtonStateText(controllerState.buttonOculusDown, controllerState.buttonOculusPressed);

            RControllerText.text = text;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="downFlag"></param>
        /// <param name="pressedFlag"></param>
        /// <returns></returns>
        private static string GetButtonStateText(
            bool downFlag,
            bool pressedFlag)
        {
            if (downFlag)
            {
                return "Down";
            }

            if (pressedFlag)
            {
                return "Pressed";
            }

            return "";
        }
    }
}
