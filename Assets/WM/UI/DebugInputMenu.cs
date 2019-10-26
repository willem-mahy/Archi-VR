using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WM.ArchiVR;

namespace WM.UI
{
    public class DebugInputMenu : MonoBehaviour
    {
        public ApplicationArchiVR ApplicationArchiVR;

        Text text;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            var controllerInput = ApplicationArchiVR.m_controllerInput;

            var controllerState = controllerInput.m_controllerState;

            var text = "";
            text += "\nInput mode: " + (controllerInput.m_inputMode == ControllerInput.InputMode.Unity ? "Unity" : "OVR");
            text += "\n";
            //text += "\nRemote connection: L=" + (lRemoteConnected ? "OK" : "NA") + " R=" + (rRemoteConnected ? "OK" : "NA");
            text += "\nTouch controllers:" + (controllerState.lTouchConnected ? "L " : "") + " " + (controllerState.rTouchConnected ? " R" : "") +
                    "(Active Controller: " + (controllerState.activeController == OVRInput.Controller.LTouch ? " L" : "") + (controllerState.activeController == OVRInput.Controller.RTouch ? " R" : "") + ")";
            text += "\n";
            text += "\nThumbstick: L(" + controllerState.lThumbStick.x + ", " + controllerState.lThumbStick.y + ") R(" + controllerState.rThumbStick.x + ", " + controllerState.rThumbStick.y + ")";
            text += "\nL thumbstick:";
            text += "\n Left: " + (controllerState.lThumbstickDirectionLeftDown ? "Down" : (controllerState.lThumbstickDirectionLeftPressed ? "Pressed" : ""));
            text += "\n Right: " + (controllerState.lThumbstickDirectionRightDown ? "Down" : (controllerState.lThumbstickDirectionRightPressed ? "Pressed" : ""));
            text += "\n Up: " + (controllerState.lThumbstickDirectionUpDown ? "Down" : (controllerState.lThumbstickDirectionUpPressed ? "Pressed" : ""));
            text += "\n Down: " + (controllerState.lThumbstickDirectionDownDown ? "Down" : (controllerState.lThumbstickDirectionDownPressed ? "Pressed" : ""));

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

            text += "\n";

            text += "\nButton 1 = " + (controllerState.button1Down ? "Down" : (controllerState.button1Pressed ? "Pressed" : ""));
            text += "\nButton 2 = " + (controllerState.button2Down ? "Down" : (controllerState.button2Pressed ? "Pressed" : ""));
            text += "\nButton 3 = " + (controllerState.button3Down ? "Down" : (controllerState.button3Pressed ? "Pressed" : ""));
            text += "\nButton 4 = " + (controllerState.button4Down ? "Down" : (controllerState.button4Pressed ? "Pressed" : ""));
            text += "\nButton 5 = " + (controllerState.button5Down ? "Down" : (controllerState.button5Pressed ? "Pressed" : ""));
            text += "\nButton 6 = " + (controllerState.button6Down ? "Down" : (controllerState.button6Pressed ? "Pressed" : ""));
            text += "\nButton 7 = " + (controllerState.button7Down ? "Down" : (controllerState.button7Pressed ? "Pressed" : ""));
            text += "\nButton 8 = " + (controllerState.button8Down ? "Down" : (controllerState.button8Pressed ? "Pressed" : ""));
            text += "\nButton Start = " + (controllerState.buttonStartDown ? "Down" : (controllerState.buttonStartPressed ? "Pressed" : ""));

            this.text.text = text;
        }
    }
}
