﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchiVR
{
    //public class ApplicationStateWalkaround
    //{
    //}

    //public class ApplicationStateDefinePOI
    //{
    //}

    //public class ApplicationStateMaquette
    //{
    //}

    public class ApplicationArchiVR : MonoBehaviour
    {
        #region Variables

        public string m_version = "190908a";

        #region Game objects

        UnityEngine.GameObject m_ovrCameraRig = null;

        UnityEngine.GameObject m_centerEyeAnchor = null;

        UnityEngine.GameObject m_leftHandAnchor = null;

        UnityEngine.GameObject m_rightHandAnchor = null;

        #endregion

        #region Project

        int m_loadingProjectIndex = -1;
        int m_activeProjectIndex = -1;
        List<string> m_projectNames = new List<string>();

        #endregion

        #region POI

        int m_activePOIIndex = -1;

        List<UnityEngine.GameObject> m_POI = new List<UnityEngine.GameObject>();

        #endregion

        #region Immersion mode

        enum ImmersionMode : int
        {
            Walkthrough = 0,
            Maquette
        }

        List<float> m_modelScalesPerImmersionMode = new List<float>();

        // The immersion mode.
        private ImmersionMode m_immersionMode = ImmersionMode.Walkthrough;

        GameObject m_maquettePreviewContext = null;

        #endregion

        #region HUD menu

        public ButtonMappingUI leftControllerButtonMapping = null;
        public ButtonMappingUI rightControllerButtonMapping = null;

        enum MenuMode
        {
            None = 0,
            Debug,
            Info
        }

        // The menu mode.
        private MenuMode m_menuMode = MenuMode.None;

        public UnityEngine.GameObject m_centerEyeCanvas = null;

        UnityEngine.GameObject m_centerEyePanel = null;

        public UnityEngine.UI.Text m_centerEyeText = null;

        // The HUD menu text
        string text = "";

        #endregion

        #region L controller menu

        public UnityEngine.GameObject m_leftControllerCanvas = null;

        UnityEngine.GameObject m_leftControllerPanel = null;

        public UnityEngine.UI.Text m_leftControllerText = null;

        #endregion

        #region R controller menu

        public UnityEngine.GameObject m_rightControllerCanvas = null;

        UnityEngine.GameObject m_rightControllerPanel = null;

        public UnityEngine.UI.Text m_rightControllerText = null;

        #endregion

        #region Input

        enum InputMode
        {
            Unity = 0,
            OVR
        }

        InputMode m_inputMode = InputMode.OVR;

        const string button1ID = "joystick button 0";
        const string button2ID = "joystick button 1";
        const string button3ID = "joystick button 2";
        const string button4ID = "joystick button 3";
        const string button5ID = "joystick button 4";
        const string button6ID = "joystick button 5";
        const string button7ID = "joystick button 6";
        const string button8ID = "joystick button 7";
        const string button9ID = "joystick button 8";
        const string button10ID = "joystick button 9";
        const string startButtonID = button8ID;
        const string thumbstickPID = button9ID;
        const string thumbstickSID = button10ID;
        string[] joystickNames = null;

        bool touchControllersConnected = false;

        // button1 = A
        // button2 = B
        // button3 = X
        // button4 = Y
        // button5 = Right Hand Trigger
        // button6 = Left Hand Trigger
        // button7 = Right Index Trigger
        // button8 = Left Index Trigger
        bool button1Down = false;
        bool button2Down = false;
        bool button3Down = false;
        bool button4Down = false;
        bool button5Down = false;
        bool button6Down = false;
        bool button7Down = false;
        bool button8Down = false;
        bool buttonStartDown = false;
        bool buttonThumbstickPDown = false;
        bool buttonThumbstickSDown = false;

        bool button1Pressed = false;
        bool button2Pressed = false;
        bool button3Pressed = false;
        bool button4Pressed = false;
        bool button5Pressed = false;
        bool button6Pressed = false;
        bool button7Pressed = false;
        bool button8Pressed = false;
        bool buttonStartPressed = false;
        bool buttonThumbstickPPressed = false;
        bool buttonThumbstickSPressed = false;

        bool leftControllerActive = false;
        bool rightControllerActive = false;

        OVRInput.Controller activeController = OVRInput.Controller.None;

        bool lRemoteConnected = false;
        bool rRemoteConnected = false;

        bool lTouchConnected = false;
        bool rTouchConnected = false;

        bool rawButtonLIndexTriggerDown = false;
        bool rawButtonRIndexTriggerDown = false;

        Vector2 lThumbStick;
        Vector2 rThumbStick;

        bool lThumbstickDirectionLeftDown = false;
        bool lThumbstickDirectionRightDown = false;
        bool lThumbstickDirectionUpDown = false;
        bool lThumbstickDirectionDownDown = false;

        bool lThumbstickDirectionLeftPressed = false;
        bool lThumbstickDirectionRightPressed = false;
        bool lThumbstickDirectionUpPressed = false;
        bool lThumbstickDirectionDownPressed = false;

        bool rThumbstickDirectionLeftDown = false;
        bool rThumbstickDirectionRightDown = false;
        bool rThumbstickDirectionUpDown = false;
        bool rThumbstickDirectionDownDown = false;
             
        bool rThumbstickDirectionLeftPressed = false;
        bool rThumbstickDirectionRightPressed = false;
        bool rThumbstickDirectionUpPressed = false;
        bool rThumbstickDirectionDownPressed = false;

        #endregion

        float m_maquetteOffset = 0;

        float m_maquetteRotation = 0;

        #endregion Variables

        // Start is called before the first frame update
        void Start()
        {
            #region Get handles to game objects

            if (m_ovrCameraRig == null)
                m_ovrCameraRig = GameObject.Find("OVRCameraRig");

            if (m_centerEyeAnchor == null)
                m_centerEyeAnchor = GameObject.Find("CenterEyeAnchor");

            if (m_leftHandAnchor == null)
                m_leftHandAnchor = GameObject.Find("LeftHandAnchor");

            if (m_rightHandAnchor == null)
                m_rightHandAnchor = GameObject.Find("RightHandAnchor");

            #endregion

            m_centerEyeCanvas   = GameObject.Find("CenterEyeCanvas");
            m_centerEyePanel    = GameObject.Find("CenterEyePanel");
            m_centerEyeText     = GameObject.Find("CenterEyeText").GetComponent<UnityEngine.UI.Text>();

            m_leftControllerCanvas  = GameObject.Find("LeftControllerCanvas");
            m_leftControllerPanel   = GameObject.Find("LeftControllerPanel");
            m_leftControllerText    = GameObject.Find("LeftControllerText").GetComponent<UnityEngine.UI.Text>();

            m_rightControllerCanvas     = GameObject.Find("RightControllerCanvas");
            m_rightControllerPanel      = GameObject.Find("RightControllerPanel");
            m_rightControllerText       = GameObject.Find("RightControllerText").GetComponent<UnityEngine.UI.Text>();

            m_modelScalesPerImmersionMode.Add(1.0f);
            m_modelScalesPerImmersionMode.Add(0.04f);

            if (m_maquettePreviewContext == null)
            {
                m_maquettePreviewContext = GameObject.Find("MaquettePreviewContext");
            }

            GatherProjects();

            SetImmersionMode(m_immersionMode);
        }

        void GatherProjects()
        {
            m_projectNames = GetProjectNames();

            ActivateProject(0);
        }

        string GetActiveProjectName()
        {
            if (m_activeProjectIndex == -1)
                return null;

            return m_projectNames[m_activeProjectIndex];
        }

        GameObject GetActiveProject()
        {
            if (m_activeProjectIndex == -1)
                return null;

            var activeProject = GameObject.Find("Project");

            return activeProject;
        }

        GameObject GetActivePOI()
        {
            if (m_activePOIIndex == -1)
                return null;

            return m_POI[m_activePOIIndex];
        }

        void GatherActiveProjectPOI()
        {
            m_POI.Clear();

            var activeProject = GetActiveProject();

            if (activeProject == null)
            {
                return;
            }

            // Gather all POI in the current active project.
            var pois = new List<GameObject>();

            foreach (Transform childOfActiveProject in activeProject.transform)
            {
                var childGameObject = childOfActiveProject.gameObject;

                if (childGameObject.name == "POI")
                {
                    var POIs = childGameObject;

                    foreach (Transform childOfPOIs in POIs.transform)
                    {
                        pois.Add(childOfPOIs.gameObject);
                    }

                    break;
                }
            }

            SetPOI(pois);
        }

        void SetPOI(List<GameObject> pois)
        {
            m_POI = pois;

            SetActivePOI(0);
        }

        void ActivateProject(int projectIndex)
        {
            if (m_projectNames.Count == 0)
            {
                projectIndex = -1;
                return;
            }
            else
            {
                projectIndex = (projectIndex) % m_projectNames.Count;

                while (projectIndex < 0)
                {
                    projectIndex += m_projectNames.Count;
                }
            }

            if (projectIndex == m_activeProjectIndex)
            {
                return;
            }

            m_leftControllerText.text = (projectIndex == -1 ? "" : m_projectNames[projectIndex]);

            m_loadingProjectIndex = projectIndex;

            StartCoroutine(LoadProject());
        }

        void ToggleCanvas()
        {
            m_centerEyeCanvas.SetActive(!m_centerEyeCanvas.activeSelf);
        }

        void ResetTrackingSpacePosition()
        {
            m_ovrCameraRig.transform.position = new Vector3();
            m_ovrCameraRig.transform.rotation = new Quaternion();
        }

        void UpdateTrackingSpacePosition()
        {
            if (m_immersionMode == ImmersionMode.Maquette)
            {                    
                ResetTrackingSpacePosition(); // Move towards model.
            }
            else
            {
                var activePOI = GetActivePOI();

                if (activePOI == null)
                {
                    ResetTrackingSpacePosition();
                }
                else
                {
                    m_ovrCameraRig.transform.position = activePOI.transform.position;
                    m_ovrCameraRig.transform.rotation = activePOI.transform.rotation;
                }
            }

            if (Application.isEditor)
            {
                m_ovrCameraRig.transform.position = m_ovrCameraRig.transform.position + new Vector3(0, 1.8f, 0);
            }
        }

        float GetModelScale(ImmersionMode immersionMode)
        {
            return m_modelScalesPerImmersionMode[(int)m_immersionMode];
        }

        void UpdateModelLocationAndScale()
        {
            var activeProject = GetActiveProject();

            if (activeProject == null)
            {
                return;
            }

            var scale = GetModelScale(m_immersionMode);            
            var position = new Vector3();
            var rotation = new Quaternion();

            if (m_immersionMode == ImmersionMode.Maquette)
            {
                position.y = 1 + m_maquetteOffset;
                rotation = Quaternion.AngleAxis(m_maquetteRotation, Vector3.up);
            }

            activeProject.transform.position = position;
            activeProject.transform.rotation = rotation;
            activeProject.transform.localScale = new Vector3(scale, scale, scale);
        }

        void ToggleImmersionMode()
        {
            SetImmersionMode(1 - m_immersionMode);
        }

        void UpdateButtonMappingUI()
        {
            if (leftControllerButtonMapping != null)
            {
                leftControllerButtonMapping.textHandTrigger.text = "";

                leftControllerButtonMapping.textAX.transform.parent.transform.parent.gameObject.SetActive(!Input.GetKey(KeyCode.F1) && !button1Pressed);
                leftControllerButtonMapping.textBY.transform.parent.transform.parent.gameObject.SetActive(!Input.GetKey(KeyCode.F2) && !button2Pressed);
                
                leftControllerButtonMapping.textOculusStart.transform.parent.transform.parent.gameObject.SetActive(!Input.GetKey(KeyCode.F11) && !button8Pressed);

                leftControllerButtonMapping.textHandTrigger.transform.parent.transform.parent.gameObject.SetActive(!button5Pressed);
                leftControllerButtonMapping.textIndexTrigger.transform.parent.transform.parent.gameObject.SetActive(!Input.GetKey(KeyCode.I) && !button7Pressed);

                leftControllerButtonMapping.textThumbDown.transform.parent.transform.parent.gameObject.SetActive(!Input.GetKey(KeyCode.L) && (lThumbStick.y > -0.01));
                leftControllerButtonMapping.textThumbUp.transform.parent.transform.parent.gameObject.SetActive(!Input.GetKey(KeyCode.O) && (lThumbStick.y < 0.01));

                leftControllerButtonMapping.textThumbLeft.transform.parent.transform.parent.gameObject.SetActive(!Input.GetKey(KeyCode.K) && (lThumbStick.x > -0.01));
                leftControllerButtonMapping.textThumbRight.transform.parent.transform.parent.gameObject.SetActive(!Input.GetKey(KeyCode.M) && (lThumbStick.x < 0.01));

                // Left controller
                leftControllerButtonMapping.textIndexTrigger.text = "Verander shaal";

                leftControllerButtonMapping.textOculusStart.text = "Toggle menu";

                leftControllerButtonMapping.textAX.text = "Vorig project";
                leftControllerButtonMapping.textBY.text = "Volgend project";

                if (m_immersionMode == ImmersionMode.Maquette)
                {
                    leftControllerButtonMapping.textThumbUp.text = "Model omhoog";
                    leftControllerButtonMapping.textThumbDown.text = "Model omlaag";
                    leftControllerButtonMapping.textThumbLeft.text = "Model links";
                    leftControllerButtonMapping.textThumbRight.text = "Model rechts";
                }
                else
                {
                    leftControllerButtonMapping.textThumbUp.text = "";
                    leftControllerButtonMapping.textThumbDown.text = "";
                    leftControllerButtonMapping.textThumbLeft.text = "";
                    leftControllerButtonMapping.textThumbRight.text = "";
                }
            }

            // Right controller
            if (rightControllerButtonMapping != null)
            {
                rightControllerButtonMapping.textAX.transform.parent.transform.parent.gameObject.SetActive(!Input.GetKey(KeyCode.F3) && !button3Pressed);
                rightControllerButtonMapping.textBY.transform.parent.transform.parent.gameObject.SetActive(!Input.GetKey(KeyCode.F4) && !button4Pressed);

                rightControllerButtonMapping.textHandTrigger.transform.parent.transform.parent.gameObject.SetActive(!Input.GetKey(KeyCode.RightShift) && !button6Pressed);
                rightControllerButtonMapping.textIndexTrigger.transform.parent.transform.parent.gameObject.SetActive(!Input.GetKey(KeyCode.Return) && !button8Pressed);

                rightControllerButtonMapping.textThumbDown.transform.parent.transform.parent.gameObject.SetActive(!Input.GetKey(KeyCode.DownArrow) && (rThumbStick.y > -0.01));
                rightControllerButtonMapping.textThumbUp.transform.parent.transform.parent.gameObject.SetActive(!Input.GetKey(KeyCode.UpArrow) && (rThumbStick.y < 0.01));

                rightControllerButtonMapping.textThumbLeft.transform.parent.transform.parent.gameObject.SetActive(!Input.GetKey(KeyCode.LeftArrow) && (rThumbStick.x > -0.01));
                rightControllerButtonMapping.textThumbRight.transform.parent.transform.parent.gameObject.SetActive(!Input.GetKey(KeyCode.RightArrow) && (rThumbStick.x < 0.01));


                if (m_immersionMode == ImmersionMode.Maquette)
                {
                    rightControllerButtonMapping.textIndexTrigger.text = "";
                    rightControllerButtonMapping.textHandTrigger.text = "";
                }
                else
                {
                    rightControllerButtonMapping.textIndexTrigger.text = "Beweeg omhoog";
                    rightControllerButtonMapping.textHandTrigger.text = "Beweeg omlaag";
                }

                rightControllerButtonMapping.textOculusStart.text = "Exit";

                rightControllerButtonMapping.textAX.text = "Vorige locatie";
                rightControllerButtonMapping.textBY.text = "Volgende locatie";

                rightControllerButtonMapping.textThumbUp.text = "Beweeg vooruit";
                rightControllerButtonMapping.textThumbDown.text = "Beweeg achteruit";
                rightControllerButtonMapping.textThumbLeft.text = "Beweeg links";
                rightControllerButtonMapping.textThumbRight.text = "Beweeg rechts";
            }
        }

        void SetImmersionMode(ImmersionMode immersionMode)
        {
            m_immersionMode = immersionMode;            
            
            m_maquettePreviewContext.SetActive(m_immersionMode == ImmersionMode.Maquette);            

            UpdateModelLocationAndScale();

            UpdateTrackingSpacePosition();
        }

        // Update is called once per frame
        void Update()
        {
            #region Update input.

            switch (m_inputMode)
            {
                case InputMode.Unity:
                    UpdateInput_Unity();
                    break;
                case InputMode.OVR:
                    UpdateInput_OVR();
                    break;
            }

            #endregion

            #region Figure out whether there is something to do.

            bool activatePrevProject = false;
            bool activateNextProject = false;

            bool activateNextPOI = false;
            bool activatePrevPOI = false;

            bool toggleImmersionMode = false;

            bool toggleMenu = false;

            float magnitudeRotateMaquette = 0.0f;
            float magnitudeTranslateMaquette = 0.0f;

            float magnitudeForward= 0.0f;
            float magnitudeRight = 0.0f;
            float magnitudeUp = 0.0f;

            if (m_loadingProjectIndex == -1)
            {
                // While not loading a project...

                activatePrevProject = button3Down || Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.LeftControl);
                activateNextProject = button4Down || Input.GetKeyDown(KeyCode.F2) || Input.GetKeyDown(KeyCode.LeftShift);

                activateNextPOI = false;
                activatePrevPOI = false;

                // Under all immersionModes...

                // ... immersion mode is toggled using I
                toggleImmersionMode = button7Down || Input.GetKeyDown(KeyCode.I);

                // ... menu is toggled using M
                toggleMenu = buttonStartDown || Input.GetKeyDown(KeyCode.F11);

                if (Application.isEditor)
                {
                    float mag = 1.0f;

                    // ... viewpoint is translated horizontally with arrow keys
                    if (Input.GetKey(KeyCode.DownArrow)) magnitudeForward -= mag;
                    if (Input.GetKey(KeyCode.UpArrow)) magnitudeForward += mag;

                    if (Input.GetKey(KeyCode.LeftArrow)) magnitudeRight -= mag;
                    if (Input.GetKey(KeyCode.RightArrow)) magnitudeRight += mag;
                }
                else
                {
                    magnitudeForward = rThumbStick.y;
                    magnitudeRight = rThumbStick.x;
                }               

                switch (m_immersionMode)
                {
                    case ImmersionMode.Walkthrough:                        
                        activateNextPOI = button1Down || Input.GetKeyDown(KeyCode.F3);
                        activatePrevPOI = button2Down || Input.GetKeyDown(KeyCode.F4);

                        if (Application.isEditor)
                        {
                            float mag = 1.0f;
                            magnitudeUp += Input.GetKey(KeyCode.O) ? mag : 0.0f;
                            magnitudeUp -= Input.GetKey(KeyCode.L) ? mag : 0.0f;
                        }
                        else
                        {
                            magnitudeUp += OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger);
                            magnitudeUp -= OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger);
                        }
                        break;

                    case ImmersionMode.Maquette:

                        if (Application.isEditor)
                        {
                            float mag = 1.0f;
                            magnitudeTranslateMaquette += Input.GetKey(KeyCode.O) ? mag : 0.0f;
                            magnitudeTranslateMaquette -= Input.GetKey(KeyCode.L) ? mag : 0.0f;

                            magnitudeRotateMaquette += Input.GetKey(KeyCode.K) ? mag : 0.0f;
                            magnitudeRotateMaquette -= Input.GetKey(KeyCode.M) ? mag : 0.0f;
                        }
                        else
                        {
                            magnitudeTranslateMaquette += lThumbStick.y;
                            magnitudeRotateMaquette = lThumbStick.x;
                        }
                        break;
                }
            }
            #endregion

            #region Do it            

            if (toggleMenu)
            {
                m_menuMode = (MenuMode)(((int)m_menuMode+1) % 3);
            }

            if (toggleImmersionMode)
            {
                ToggleImmersionMode();
            }

            #region Activate project

            if (activateNextProject)
            {
                ActivateProject(m_activeProjectIndex + 1);
            }

            if (activatePrevProject)
            {
                ActivateProject(m_activeProjectIndex - 1);
            }

            #endregion

            #region Activate POI

            if (activatePrevPOI)
            {
                OffsetActivePOIIndex(+1);
            }

            if (activateNextPOI)
            {
                OffsetActivePOIIndex(-1);
            }

            #endregion

            #region Fly behaviour

            float flySpeed = 1.0f;

            var offsetR = m_centerEyeAnchor.transform.right;
            offsetR.y = 0;
            offsetR.Normalize();
            offsetR *= magnitudeRight;

            var offsetF = m_centerEyeAnchor.transform.forward;
            offsetF.y = 0;
            offsetF.Normalize();
            offsetF *= magnitudeForward;

            var offset = offsetR + offsetF;

            // Clamp to X/Y offset vector to length [0, 1].
            if (offset.magnitude > 1)
            {
                offset.Normalize();
            }

            var offsetUp = magnitudeUp * Vector3.up;

            offset += offsetUp;

            if (offset != Vector3.zero)
            {
                TranslateTrackingSpace(flySpeed * Time.deltaTime * offset);
            }

            #endregion

            switch (m_immersionMode)
            {
                case ImmersionMode.Walkthrough:
                    offset =
                        rThumbStick.x * offsetR +
                        rThumbStick.y * offsetF;

                    break;

                case ImmersionMode.Maquette:
                    #region Maquette manipulation.

                    // Translate Up/Down
                    var maquetteMoveSpeed = 1.0f;

                    m_maquetteOffset = Mathf.Min(1.0f, m_maquetteOffset + magnitudeTranslateMaquette * maquetteMoveSpeed * Time.deltaTime);
                    
                    // Rotate around 'up' vector.
                    var maquetteRotateSpeed = 60.0f;

                    m_maquetteRotation += magnitudeRotateMaquette * maquetteRotateSpeed * Time.deltaTime;

                    UpdateModelLocationAndScale();

                    #endregion
                    
                    break;
            }

            UpdateMenu();

            if (Application.isEditor)
            {
                // When running in editor, anchor the controllers at a fixed offset in front of the center eye.
                var controllerOffsetForward = 0.3f * m_centerEyeAnchor.transform.forward;
                var controllerOffsetRight = 0.2f * m_centerEyeAnchor.transform.right;
                var controllerOffsetUp = 0.2f * m_centerEyeAnchor.transform.up;
                m_leftHandAnchor.transform.position =
                    m_centerEyeAnchor.transform.position
                    + controllerOffsetForward - controllerOffsetRight
                    - controllerOffsetUp;
                m_rightHandAnchor.transform.position =
                    m_centerEyeAnchor.transform.position
                    + controllerOffsetForward
                    + controllerOffsetRight
                    - controllerOffsetUp;

                m_leftHandAnchor.transform.rotation =
                m_rightHandAnchor.transform.rotation = m_centerEyeAnchor.transform.rotation;
            }

            #endregion

            UpdateButtonMappingUI();
        }        

        void TranslateTrackingSpace(Vector3 offset)
        {
            m_ovrCameraRig.transform.position = m_ovrCameraRig.transform.position + offset;
        }

        void UpdateMenu()
        {
            // Reset HUD menu text.
            text = "";

            // Update HUD menu text. (if not 'None')
            switch (m_menuMode)
            {
                case MenuMode.Debug:
                    UpdateMenuDebug();
                    break;
                case MenuMode.Info:
                    UpdateMenuInfo();
                    break;
                case MenuMode.None:
                    break;
                default:
                    text += "Unsupported menu mode: " + m_menuMode.ToString();
                    break;
            }

            // Push HUD menu text to UI.
            m_centerEyeText.text = text;

            m_centerEyePanel.SetActive(MenuMode.None != m_menuMode);
        }

        void UpdateMenuDebug()
        {
            text += "\nInput mode: " + (m_inputMode == InputMode.Unity ? "Unity" : "OVR");
            text += "\n";
            //text += "\nRemote connection: L=" + (lRemoteConnected ? "OK" : "NA") + " R=" + (rRemoteConnected ? "OK" : "NA");
            text += "\nTouch controllers:" + (lTouchConnected ? "L " : "") + " " + (rTouchConnected ? " R" : "") +
                    "(Active Controller: " + (activeController == OVRInput.Controller.LTouch ? " L" : "") + (activeController == OVRInput.Controller.RTouch ? " R" : "") + ")";
            text += "\n";
            text += "\nThumbstick: L(" + lThumbStick.x + ", " + lThumbStick.y + ") R(" + rThumbStick.x + ", " + rThumbStick.y + ")";
            text += "\nL thumbstick:";
            text += "\n Left: " + (lThumbstickDirectionLeftDown ? "Down" : (lThumbstickDirectionLeftPressed ? "Pressed" : ""));
            text += "\n Right: " + (lThumbstickDirectionRightDown ? "Down" : (lThumbstickDirectionRightPressed ? "Pressed" : ""));
            text += "\n Up: " + (lThumbstickDirectionUpDown ? "Down" : (lThumbstickDirectionUpPressed ? "Pressed" : ""));
            text += "\n Down: " + (lThumbstickDirectionDownDown ? "Down" : (lThumbstickDirectionDownPressed ? "Pressed" : ""));
            
            if (m_inputMode == InputMode.Unity)
            {
                text += "\nJoysticks:";
                foreach (var n in joystickNames)
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
                text += "\nL IndexTrigger = " + OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger) + (rawButtonLIndexTriggerDown ? " Down" : "");

                // right
                text += "\nR IndexTrigger = " + OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) + (rawButtonRIndexTriggerDown ? " Down" : "");

                // returns true if the secondary gamepad button, typically “B”, is currently touched by the user.
                //text += "\nGetTouchTwo = " + OVRInput.Get(OVRInput.Touch.Two);   
            }

            text += "\n";

            text += "\nButton 1 = " + (button1Down ? "Down" : (button1Pressed ? "Pressed" : ""));
            text += "\nButton 2 = " + (button2Down ? "Down" : (button2Pressed ? "Pressed" : ""));
            text += "\nButton 3 = " + (button3Down ? "Down" : (button3Pressed ? "Pressed" : ""));
            text += "\nButton 4 = " + (button4Down ? "Down" : (button4Pressed ? "Pressed" : ""));
            text += "\nButton 5 = " + (button5Down ? "Down" : (button5Pressed ? "Pressed" : ""));
            text += "\nButton 6 = " + (button6Down ? "Down" : (button6Pressed ? "Pressed" : ""));
            text += "\nButton 7 = " + (button7Down ? "Down" : (button7Pressed ? "Pressed" : ""));
            text += "\nButton 8 = " + (button8Down ? "Down" : (button8Pressed ? "Pressed" : ""));
            text += "\nButton Start = " + (buttonStartDown ? "Down" : (buttonStartPressed ? "Pressed" : ""));

            // TODO Implement ABXY?
            //text += "\nA button = " + (aButtonDown ? "Down" : (aButtonPressed ? "Pressed" : ""));
            //text += "\nB button = " + (bButtonDown ? "Down" : (bButtonPressed ? "Pressed" : ""));
            //text += "\nX button = " + (xButtonDown ? "Down" : (xButtonPressed ? "Pressed" : ""));
            //text += "\nY button = " + (yButtonDown ? "Down" : (yButtonPressed ? "Pressed" : ""));
        }

        List<string> GetProjectNames()
        {
            var projectNames = new List<string>();

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; ++i)
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));

                if (sceneName.StartsWith("Project"))
                {
                    projectNames.Add(sceneName);
                }
            }

            return projectNames;
        }

        void UpdateMenuInfo()
        {
            var projectNames = GetProjectNames();

            text += "\nProjects:";
            foreach (var projectName in projectNames)
            {
                text += "\n - " + projectName;
            }

            var activeProjectName = GetActiveProjectName();

            if (activeProjectName != null)
            {
                text += "\n";
                text += "\n" + activeProjectName;

                var activePOI = GetActivePOI();

                if (activePOI != null)
                {
                    text += " > " + activePOI.name;
                }
            }

            text += "\n";
            text += "\nversion: " + m_version;
        }        

        void UpdateInput_Unity()
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

        void UpdateInput_OVR()
        {
            var activeController = OVRInput.GetActiveController();

            lRemoteConnected = OVRInput.IsControllerConnected(OVRInput.Controller.LTrackedRemote);
            rRemoteConnected = OVRInput.IsControllerConnected(OVRInput.Controller.RTrackedRemote);
            lTouchConnected = OVRInput.IsControllerConnected(OVRInput.Controller.LTouch);
            rTouchConnected = OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);

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
                button1Down = (!button1Down && !button1Pressed) && OVRInput.Get(OVRInput.Button.One);
                button2Down = (!button2Down && !button2Pressed) && OVRInput.Get(OVRInput.Button.Two);
                button3Down = (!button3Down && !button3Pressed) && OVRInput.Get(OVRInput.Button.Three);
                button4Down = (!button4Down && !button4Pressed) && OVRInput.Get(OVRInput.Button.Four);
                button5Down = (!button5Down && !button5Pressed) && OVRInput.Get(OVRInput.Button.PrimaryHandTrigger);
                button6Down = (!button6Down && !button6Pressed) && OVRInput.Get(OVRInput.Button.SecondaryHandTrigger);
                button7Down = (!button7Down && !button7Pressed) && OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger);
                button8Down = (!button8Down && !button8Pressed) && OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);

                buttonStartDown = (!buttonStartDown && !buttonStartPressed) && OVRInput.Get(OVRInput.Button.Start);                

                buttonThumbstickPDown = (!buttonThumbstickPDown && !buttonThumbstickPPressed) && OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch);
                buttonThumbstickSDown = (!buttonThumbstickPDown && !buttonThumbstickPPressed) && OVRInput.Get(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch);

                lThumbstickDirectionUpDown      = (!lThumbstickDirectionUpDown && !lThumbstickDirectionUpPressed) && OVRInput.Get(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.Touch);
                lThumbstickDirectionDownDown    = (!lThumbstickDirectionDownDown && !lThumbstickDirectionDownPressed) && OVRInput.Get(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.Touch);
                lThumbstickDirectionLeftDown    = (!lThumbstickDirectionLeftDown && !lThumbstickDirectionLeftPressed) && OVRInput.Get(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.Touch);
                lThumbstickDirectionRightDown   = (!lThumbstickDirectionRightDown && !lThumbstickDirectionRightPressed) && OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.Touch);

                rThumbstickDirectionUpDown = (!rThumbstickDirectionUpDown && !rThumbstickDirectionUpPressed) && OVRInput.Get(OVRInput.Button.SecondaryThumbstickUp, OVRInput.Controller.Touch);
                rThumbstickDirectionDownDown = (!rThumbstickDirectionDownDown && !rThumbstickDirectionDownPressed) && OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown, OVRInput.Controller.Touch);
                rThumbstickDirectionLeftDown = (!rThumbstickDirectionLeftDown && !rThumbstickDirectionLeftPressed) && OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft, OVRInput.Controller.Touch);
                rThumbstickDirectionRightDown = (!rThumbstickDirectionRightDown && !rThumbstickDirectionRightPressed) && OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight, OVRInput.Controller.Touch);
            }

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

            // Get thumbstick positions. (X/Y range of -1.0f to 1.0f)
            lThumbStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            rThumbStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        }

        void OffsetActivePOIIndex(int offset)
        {
            SetActivePOI(m_activePOIIndex + offset);
        }

        void SetActivePOI(int newPOIIndex)
        {
            if (m_POI.Count == 0)
            {
                m_activePOIIndex = -1;
                return;
            }

            m_activePOIIndex = (newPOIIndex) % m_POI.Count;

            while (m_activePOIIndex < 0)
            {
                m_activePOIIndex += m_POI.Count;
            }

            UpdateTrackingSpacePosition();

            m_rightControllerText.text = (m_activePOIIndex == -1 ? "" : m_POI[m_activePOIIndex].name);
        }

        IEnumerator LoadProject()
        {
            var oldProjectName = GetActiveProjectName();

            if (m_loadingProjectIndex != -1)
            {
                var newProjectName = m_projectNames[m_loadingProjectIndex];

                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(newProjectName, LoadSceneMode.Additive);

                // Wait until asynchronous loading the old project finishes.
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
            }

            m_activeProjectIndex = m_loadingProjectIndex;

            if (oldProjectName != null)
            {
                AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(oldProjectName);

                // Wait until asynchronous unloading the old project finishes.
                while (!asyncUnload.isDone)
                {
                    yield return null;
                }
            }

            // Gather the POI from the new project.
            GatherActiveProjectPOI();

            SetImmersionMode(m_immersionMode);

            m_loadingProjectIndex = -1;
        }
    }
}
