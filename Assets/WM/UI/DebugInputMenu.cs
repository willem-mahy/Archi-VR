using UnityEngine;
using UnityEngine.UI;
using WM.Application;

namespace WM.UI
{
    /// <summary>
    /// The 'Debug Input' menu panel.
    /// Shows the actual state of the controller input.
    /// </summary>
    public class DebugInputMenu : MenuPanel<UnityApplication>
    {
        #region Fields

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

        #endregion Fields

        #region Public API

        #region GameObject overrides

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
                text += "\nTouch controllers:" + (controllerState.lTouchConnected ? "L " : "") + " " + (controllerState.rTouchConnected ? " R" : "") +
                        "(Active Controller: " + (controllerState.activeController == OVRInput.Controller.LTouch ? " L" : "") + (controllerState.activeController == OVRInput.Controller.RTouch ? " R" : "") + ")";
                text += "\n";
                

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

            text = "";
            text += "\nThumbstick:" + GetButtonStateText(controllerState.lThumbstickDown, controllerState.lThumbstickPressed);
            text += "\n\tAxes: " + GetThumbStickAxesStateText(controllerState.lThumbStick);
            text += "\n\tLeft: " + GetButtonStateText(controllerState.lThumbstickDirectionLeftDown, controllerState.lThumbstickDirectionLeftPressed);
            text += "\n\tRight: " + GetButtonStateText(controllerState.lThumbstickDirectionRightDown, controllerState.lThumbstickDirectionRightPressed);
            text += "\n\tUp: " + GetButtonStateText(controllerState.lThumbstickDirectionUpDown, controllerState.lThumbstickDirectionUpPressed);
            text += "\n\tDown: " + GetButtonStateText(controllerState.lThumbstickDirectionDownDown, controllerState.lThumbstickDirectionDownPressed);
            text += "\nButton 1 = " + GetButtonStateText(controllerState.aButtonDown, controllerState.aButtonPressed);
            text += "\nButton 2 = " + GetButtonStateText(controllerState.bButtonDown, controllerState.bButtonPressed);
            text += "\nButton 3 = " + GetButtonStateText(controllerState.xButtonDown, controllerState.xButtonPressed);
            text += "\nButton 4 = " + GetButtonStateText(controllerState.yButtonDown, controllerState.yButtonPressed);
            text += "\nButton Start = " + GetButtonStateText(controllerState.startButtonDown, controllerState.startButtonPressed);
            LControllerText.text = text;

            text = "";
            text += "\nThumbstick:" + GetButtonStateText(controllerState.rThumbstickDown, controllerState.rThumbstickPressed);
            text += "\n\tAxes: " + GetThumbStickAxesStateText(controllerState.rThumbStick);
            text += "\n\tLeft: " + GetButtonStateText(controllerState.rThumbstickDirectionLeftDown, controllerState.rThumbstickDirectionLeftPressed);
            text += "\n\tRight: " + GetButtonStateText(controllerState.rThumbstickDirectionRightDown, controllerState.rThumbstickDirectionRightPressed);
            text += "\n\tUp: " + GetButtonStateText(controllerState.rThumbstickDirectionUpDown, controllerState.rThumbstickDirectionUpPressed);
            text += "\n\tDown: " + GetButtonStateText(controllerState.rThumbstickDirectionDownDown, controllerState.rThumbstickDirectionDownPressed);
            text += "\nButton 5 = " + GetButtonStateText(controllerState.lHandTriggerDown, controllerState.lHandTriggerPressed);
            text += "\nButton 6 = " + GetButtonStateText(controllerState.rHandTriggerDown, controllerState.rHandTriggerPressed);
            text += "\nButton 7 = " + GetButtonStateText(controllerState.lIndexTriggerDown, controllerState.lIndexTriggerPressed);
            text += "\nButton 8 = " + GetButtonStateText(controllerState.rIndexTriggerDown, controllerState.rIndexTriggerPressed);            
            text += "\nButton Oculus = " + GetButtonStateText(controllerState.buttonOculusDown, controllerState.buttonOculusPressed);

            RControllerText.text = text;
        }

        #endregion GameObject overrides

        #endregion Public API

        #region Non-public API

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thumbstickAxes"></param>
        /// <returns></returns>
        private static string GetThumbStickAxesStateText(Vector2 thumbstickAxes)
        {
            return "(" + thumbstickAxes.x + ", " + thumbstickAxes.y + ")";
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

        #endregion Non-public API
    }
} // namespace WM.UI
