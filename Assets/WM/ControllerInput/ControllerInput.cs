namespace WM
{
    /// <summary>
    /// 
    /// </summary>
    public class ControllerInput
    {
        /// <summary>
        /// 
        /// </summary>
        public enum InputMode
        {
            Unity = 0,
            OVR
        }

        #region Fields

        /// <summary>
        /// 
        /// </summary>
        public InputMode m_inputMode = InputMode.OVR;

        /// <summary>
        /// 
        /// </summary>
        public ControllerState m_controllerState = new ControllerState();

        #endregion Fields

        #region Public API

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            m_controllerState = new ControllerState();
        }

        /// <summary>
        /// Reset (set to 'false') the 'down' state for all controller buttons.
        /// </summary>
        public void ResetDownStates()
        {
            m_controllerState.ResetDownStates();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            var controllerState = new ControllerState();

            if (UnityEngine.Application.isEditor)
            {
                controllerState.Update_Editor(m_controllerState);
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

        #endregion Public API
    }
}