using UnityEngine;

namespace WM
{
    public class ControllerInput
    {
        public enum InputMode
        {
            Unity = 0,
            OVR
        }

        public InputMode m_inputMode = InputMode.OVR;

        public ControllerState m_controllerState = new ControllerState();

        public void Update()
        {
            var controllerState = new ControllerState();

            if (Application.isEditor)
            {
                controllerState.Update_Editor();
            }
            else
            {
                switch (m_inputMode)
                {
                    case InputMode.Unity:
                        controllerState.Update_Unity();
                        break;
                    case InputMode.OVR:
                        controllerState.Update_OVR(m_controllerState);
                        break;
                }
            }

            m_controllerState = controllerState;
        }
    }
}