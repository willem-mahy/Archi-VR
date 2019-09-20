using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchiVR
{
    public class LoadingProjectInfo
    {
        public int ProjectIndex { get; set; } = 0;
        public string POIName { get; set; } = null;
    }

    public class ApplicationArchiVR : MonoBehaviour
    {
        #region Variables

        public string m_version = "190920a";

        #region Game objects

        public UnityEngine.GameObject m_ovrCameraRig = null;

        UnityEngine.GameObject m_centerEyeAnchor = null;

        UnityEngine.GameObject m_leftHandAnchor = null;

        UnityEngine.GameObject m_rightHandAnchor = null;

        #endregion

        #region Project

        //! -1 when not loading a project.
        public LoadingProjectInfo m_loadingProjectInfo = null;

        // The index of the currently active project.
        int m_activeProjectIndex = -1;

        // The list of names of all projects included in the build.
        List<string> m_projectNames = new List<string>();

        #endregion

        #region POI

        int m_activePOIIndex = -1;

        List<UnityEngine.GameObject> m_POI = new List<UnityEngine.GameObject>();

        #endregion

        #region Immersion mode

        public const int DefaultImmersionModeIndex = 0;

        // The immersion mode.
        List<ImmersionMode> m_immersionModes = new List<ImmersionMode>();

        // The active immersion mode index.
        private int m_activeImmersionModeIndex = -1;

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

        public ControllerInput m_controllerInput = new ControllerInput();

        #endregion

        #region Fly behavior

        public const float DefaultFlySpeedUpDown = 0.0f;

        public float m_flySpeedUpDown = DefaultFlySpeedUpDown;

        #endregion

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

            m_centerEyeCanvas = GameObject.Find("CenterEyeCanvas");
            m_centerEyePanel = GameObject.Find("CenterEyePanel");
            m_centerEyeText = GameObject.Find("CenterEyeText").GetComponent<UnityEngine.UI.Text>();

            m_leftControllerCanvas = GameObject.Find("LeftControllerCanvas");
            m_leftControllerPanel = GameObject.Find("LeftControllerPanel");
            m_leftControllerText = GameObject.Find("LeftControllerText").GetComponent<UnityEngine.UI.Text>();

            m_rightControllerCanvas = GameObject.Find("RightControllerCanvas");
            m_rightControllerPanel = GameObject.Find("RightControllerPanel");
            m_rightControllerText = GameObject.Find("RightControllerText").GetComponent<UnityEngine.UI.Text>();

            #endregion           

            GatherProjects();

            m_immersionModes.Add(new ImmersionModeWalkthrough());
            m_immersionModes.Add(new ImmersionModeMaquette());


            foreach (var immersionMode in m_immersionModes)
            {
                immersionMode.m_application = this;
                immersionMode.Init();
            }

            SetActiveImmersionMode(DefaultImmersionModeIndex);
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

        public GameObject GetActiveProject()
        {
            if (m_activeProjectIndex == -1)
                return null;

            var activeProject = GameObject.Find("Project");

            return activeProject;
        }

        public GameObject GetActivePOI()
        {
            if (m_activePOIIndex == -1)
                return null;

            return m_POI[m_activePOIIndex];
        }

        void GatherActiveProjectPOI(string poiName)
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

            SetPOI(pois, poiName);
        }

        void SetPOI(
            List<GameObject> pois,
            string poiName)
        {
            m_POI = pois;

            if (poiName != null && (GetPOIIndex(poiName) != -1))
            {                
                SetActivePOI(GetPOIIndex(poiName));
            }
            else
            {
                SetActivePOI(GetDefaultPOIIndex());
            }
        }

        int GetDefaultPOIIndex()
        {
            return m_POI.Count == 0 ? -1 : 0;
        }

        int GetPOIIndex(string poiName)
        {
            int poiIndex = 0;
            foreach (var poi in m_POI)
            {
                if (poi.name == poiName)
                {
                    return poiIndex; // Found it.
                }
                ++poiIndex;
            }

            return -1; // Not found.
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

            var lpi = new LoadingProjectInfo();

            lpi.ProjectIndex = (projectIndex) % m_projectNames.Count;

            while (projectIndex < 0)
            {
                projectIndex += m_projectNames.Count;
            }

            lpi.POIName = GetActivePOI() ? GetActivePOI().name : null;

            m_leftControllerText.text = (projectIndex == -1 ? "" : m_projectNames[projectIndex]);

            m_loadingProjectInfo = lpi;

            StartCoroutine(LoadProject());
        }

        void ToggleCanvas()
        {
            m_centerEyeCanvas.SetActive(!m_centerEyeCanvas.activeSelf);
        }

        public void ResetTrackingSpacePosition()
        {
            m_ovrCameraRig.transform.position = new Vector3();
            m_ovrCameraRig.transform.rotation = new Quaternion();
        }

        void UpdateTrackingSpacePosition()
        {
            var aim = GetActiveImmersionMode();

            if (aim == null)
                return;

            aim.UpdateTrackingSpacePosition();
        }

        void UpdateModelLocationAndScale()
        {
            var aim = GetActiveImmersionMode();

            if (aim == null)
                return;

            aim.UpdateModelLocationAndScale();
        }

        void ToggleImmersionMode()
        {
            SetActiveImmersionMode(1 - m_activeImmersionModeIndex);
        }

        ImmersionMode GetActiveImmersionMode()
        {
            if (m_activeImmersionModeIndex == -1)
                return null;

            return m_immersionModes[m_activeImmersionModeIndex];
        }

        void SetActiveImmersionMode(int immersionModeIndex)
        {
            if (immersionModeIndex == m_activeImmersionModeIndex)
            {
                return; // Nothing to do.
            }

            var aim = GetActiveImmersionMode();

            if (aim != null)
            {
                aim.Exit();
            }

            m_activeImmersionModeIndex = immersionModeIndex;

            aim = GetActiveImmersionMode();

            if (aim != null)
            {
                aim.Enter();
            }

            UpdateModelLocationAndScale();

            UpdateTrackingSpacePosition();
        }

        // Update is called once per frame
        void Update()
        {
            #region Update controller state.

            m_controllerInput.Update();

            #endregion

            if (leftControllerButtonMapping != null)
            {
                leftControllerButtonMapping.Update(m_controllerInput.m_controllerState);
            }

            if (rightControllerButtonMapping != null)
            {
                rightControllerButtonMapping.Update(m_controllerInput.m_controllerState);
            }

            #region Figure out whether there is something to do.

            bool activatePrevProject = false;
            bool activateNextProject = false;

            bool toggleImmersionMode = false;           

            if (m_loadingProjectInfo == null) // While not loading a project...
            {
                // .. active project is toggled using X/Y button, F1/F2 keys.
                activatePrevProject = m_controllerInput.m_controllerState.button3Down || Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.LeftControl);
                activateNextProject = m_controllerInput.m_controllerState.button4Down || Input.GetKeyDown(KeyCode.F2) || Input.GetKeyDown(KeyCode.LeftShift);

                // ... immersion mode is toggled using I
                toggleImmersionMode = m_controllerInput.m_controllerState.button7Down || Input.GetKeyDown(KeyCode.I);               
            }

            // ... menu is toggled using M
            bool toggleMenu = m_controllerInput.m_controllerState.buttonStartDown || Input.GetKeyDown(KeyCode.F11);

            float magnitudeForward = 0.0f;
            float magnitudeRight = 0.0f;
            float magnitudeUp = 0.0f;

            if (Application.isEditor)
            {
                float mag = 1.0f;

                // ... viewpoint is translated horizontally with arrow keys
                if (Input.GetKey(KeyCode.DownArrow)) magnitudeForward -= mag;
                if (Input.GetKey(KeyCode.UpArrow)) magnitudeForward += mag;

                if (Input.GetKey(KeyCode.LeftArrow)) magnitudeRight -= mag;
                if (Input.GetKey(KeyCode.RightArrow)) magnitudeRight += mag;

                magnitudeUp += Input.GetKey(KeyCode.O) ? m_flySpeedUpDown : 0.0f;
                magnitudeUp -= Input.GetKey(KeyCode.L) ? m_flySpeedUpDown : 0.0f;
            }
            else
            {
                magnitudeForward = m_controllerInput.m_controllerState.rThumbStick.y;
                magnitudeRight = m_controllerInput.m_controllerState.rThumbStick.x;

                magnitudeUp += OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger);
                magnitudeUp -= OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger);
            }

            #endregion

            #region Do it           

            if (toggleMenu)
            {
                ToggleMenuMode();
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

            var aim = GetActiveImmersionMode();

            if (aim != null)
            {
                aim.Update();
            }

            UpdateMenu();

            UpdateControllersLocation();

            #endregion
        }

        //! Activates the next menu mode.
        void ToggleMenuMode()
        {
            m_menuMode = (MenuMode)(((int)m_menuMode + 1) % 3);
        }

        //! Updates the location of the controllers.
        //  When running in VR -> NOOP.
        //  When running in editor -> anchors the controllers at a fixed offset in front of the center eye.
        void UpdateControllersLocation()
        {
            if (!Application.isEditor)
            {
                return;
            }

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
            var controllerState = m_controllerInput.m_controllerState;

            text += "\nInput mode: " + (m_controllerInput.m_inputMode == ControllerInput.InputMode.Unity ? "Unity" : "OVR");
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

            if (m_controllerInput.m_inputMode == ControllerInput.InputMode.Unity)
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

        public void OffsetActivePOIIndex(int offset)
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

            if (m_loadingProjectInfo != null)
            {
                var newProjectName = m_projectNames[m_loadingProjectInfo.ProjectIndex];

                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(newProjectName, LoadSceneMode.Additive);

                // Wait until asynchronous loading the old project finishes.
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
            }

            m_activeProjectIndex = m_loadingProjectInfo.ProjectIndex;

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
            GatherActiveProjectPOI(m_loadingProjectInfo.POIName);

            SetActiveImmersionMode(m_activeImmersionModeIndex);

            m_loadingProjectInfo = null;
        }
    }

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
            if (Input.GetKey(KeyCode.F1)) button1Pressed = true;
            if (Input.GetKey(KeyCode.F2)) button2Pressed = true;

            if (Input.GetKey(KeyCode.F)) button5Pressed = true;
            if (Input.GetKey(KeyCode.R)) button7Pressed = true;

            if (Input.GetKey(KeyCode.F11)) buttonStartPressed = true;

            if (Input.GetKeyDown(KeyCode.A)) lThumbstickDown = true;
            if (Input.GetKey(KeyCode.A)) lThumbstickPressed = true;

            if (Input.GetKey(KeyCode.Q)) lThumbStick.x -= 1;
            if (Input.GetKey(KeyCode.D)) lThumbStick.x += 1;
            if (Input.GetKey(KeyCode.Z)) lThumbStick.y -= 1;
            if (Input.GetKey(KeyCode.S)) lThumbStick.y += 1;

            // Right controller
            if (Input.GetKey(KeyCode.F3)) button3Pressed = true;
            if (Input.GetKey(KeyCode.F4)) button4Pressed = true;

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

    public class ImmersionMode
    {
        public ApplicationArchiVR m_application = null;

        //! Called once, right after construction.
        public virtual void Init() { }

        public virtual void Enter() { }

        public virtual void Exit() { }

        public virtual void Update() { }

        public virtual void UpdateModelLocationAndScale() { }

        public virtual void UpdateTrackingSpacePosition() { }
    }

    public class ImmersionModeWalkthrough : ImmersionMode
    {
        public override void Enter()
        {
            InitButtonMappingUI();

            // Restore default moving up/down.
            m_application.m_flySpeedUpDown = 1.0f;
        }

        public override void Exit()
        {
            // Restore default moving up/down.
            m_application.m_flySpeedUpDown = ApplicationArchiVR.DefaultFlySpeedUpDown;
        }

        public override void Update()
        {
            if (m_application.m_loadingProjectInfo == null) // While not loading a project...
            {
                // ... Active POI is toggle using A/B button, F3/F4 key.
                var activateNextPOI = m_application.m_controllerInput.m_controllerState.button1Down || Input.GetKeyDown(KeyCode.F3);
                var activatePrevPOI = m_application.m_controllerInput.m_controllerState.button2Down || Input.GetKeyDown(KeyCode.F4);

                #region Activate POI

                if (activatePrevPOI)
                {
                    m_application.OffsetActivePOIIndex(+1);
                }

                if (activateNextPOI)
                {
                    m_application.OffsetActivePOIIndex(-1);
                }

                #endregion
            }
        }

        public override void UpdateModelLocationAndScale()
        {
            var activeProject = m_application.GetActiveProject();

            if (activeProject == null)
            {
                return;
            }

            activeProject.transform.position = Vector3.zero;
            activeProject.transform.rotation = Quaternion.identity;
            activeProject.transform.localScale = Vector3.one;
        }

        public override void UpdateTrackingSpacePosition()
        {
            var activePOI = m_application.GetActivePOI();

            if (activePOI == null)
            {
                m_application.ResetTrackingSpacePosition();
            }
            else
            {
                m_application.m_ovrCameraRig.transform.position = activePOI.transform.position;
                m_application.m_ovrCameraRig.transform.rotation = activePOI.transform.rotation;
            }

            if (Application.isEditor)
            {
                m_application.m_ovrCameraRig.transform.position = m_application.m_ovrCameraRig.transform.position + new Vector3(0, 1.8f, 0);
            }
        }

        void InitButtonMappingUI()
        {
            // Left controller
            if (m_application.leftControllerButtonMapping != null)
            {
                m_application.leftControllerButtonMapping.textLeftHandTrigger.text = "";

                m_application.leftControllerButtonMapping.textLeftIndexTrigger.text = "Verander shaal";

                m_application.leftControllerButtonMapping.textButtonStart.text = "Toggle menu";

                m_application.leftControllerButtonMapping.textButtonX.text = "Vorig project";
                m_application.leftControllerButtonMapping.textButtonY.text = "Volgend project";

                m_application.leftControllerButtonMapping.textLeftThumbUp.text = "";
                m_application.leftControllerButtonMapping.textLeftThumbDown.text = "";
                m_application.leftControllerButtonMapping.textLeftThumbLeft.text = "";
                m_application.leftControllerButtonMapping.textLeftThumbRight.text = "";
            }

            // Right controller
            if (m_application.rightControllerButtonMapping != null)
            {
                m_application.rightControllerButtonMapping.textRightIndexTrigger.text = "Beweeg omhoog";
                m_application.rightControllerButtonMapping.textRightHandTrigger.text = "Beweeg omlaag";

                m_application.rightControllerButtonMapping.textButtonOculus.text = "Exit";

                m_application.rightControllerButtonMapping.textButtonA.text = "Vorige locatie";
                m_application.rightControllerButtonMapping.textButtonB.text = "Volgende locatie";

                m_application.rightControllerButtonMapping.textRightThumbUp.text = "Beweeg vooruit";
                m_application.rightControllerButtonMapping.textRightThumbDown.text = "Beweeg achteruit";
                m_application.rightControllerButtonMapping.textRightThumbLeft.text = "Beweeg links";
                m_application.rightControllerButtonMapping.textRightThumbRight.text = "Beweeg rechts";
            }
        }
    }

    public class ImmersionModeMaquette : ImmersionMode
    {
        #region variables
        
        GameObject m_maquettePreviewContext = null;

        float m_maquetteOffset = 0;

        float m_maquetteRotation = 0;

        #endregion

        public override void Init()
        {
            if (m_maquettePreviewContext == null)
            {
                m_maquettePreviewContext = GameObject.Find("MaquettePreviewContext");
            }
        }

        public override void Enter()
        {
            InitButtonMappingUI();

            if (m_maquettePreviewContext)
                m_maquettePreviewContext.SetActive(true);

            // Disable moving up/down.
            m_application.m_flySpeedUpDown = 0.0f;
        }

        public override void Exit()
        {
            if (m_maquettePreviewContext)
                m_maquettePreviewContext.SetActive(false);

            // Restore default moving up/down.
            m_application.m_flySpeedUpDown = ApplicationArchiVR.DefaultFlySpeedUpDown;
        }

        public override void Update()
        {
            #region Maquette manipulation.

            float magnitudeRotateMaquette = 0.0f;
            float magnitudeTranslateMaquette = 0.0f;

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
                magnitudeTranslateMaquette += m_application.m_controllerInput.m_controllerState.lThumbStick.y;
                magnitudeRotateMaquette = m_application.m_controllerInput.m_controllerState.lThumbStick.x;
            }

            // Translate Up/Down
            var maquetteMoveSpeed = 1.0f;

            m_maquetteOffset = Mathf.Min(1.0f, m_maquetteOffset + magnitudeTranslateMaquette * maquetteMoveSpeed * Time.deltaTime);

            // Rotate around 'up' vector.
            var maquetteRotateSpeed = 60.0f;

            m_maquetteRotation += magnitudeRotateMaquette * maquetteRotateSpeed * Time.deltaTime;

            UpdateModelLocationAndScale();

            #endregion
        }

        public override void UpdateModelLocationAndScale()
        {
            var activeProject = m_application.GetActiveProject();

            if (activeProject == null)
            {
                return;
            }

            var position = Vector3.zero;
            position.y = 1 + m_maquetteOffset;

            var rotation = Quaternion.AngleAxis(m_maquetteRotation, Vector3.up);

            var scale = 0.04f * Vector3.one;

            activeProject.transform.position = position;
            activeProject.transform.rotation = rotation;
            activeProject.transform.localScale = scale;
        }

        public override void UpdateTrackingSpacePosition()
        {
            m_application.ResetTrackingSpacePosition(); // Center around model.

            if (Application.isEditor)
            {
                m_application.m_ovrCameraRig.transform.position = m_application.m_ovrCameraRig.transform.position + new Vector3(0, 1.8f, 0);
            }
        }

        void InitButtonMappingUI()
        {
            // Left controller
            if (m_application.leftControllerButtonMapping != null)
            {
                m_application.leftControllerButtonMapping.textLeftHandTrigger.text = "";

                m_application.leftControllerButtonMapping.textLeftIndexTrigger.text = "Verander shaal";

                m_application.leftControllerButtonMapping.textButtonStart.text = "Toggle menu";

                m_application.leftControllerButtonMapping.textButtonX.text = "Vorig project";
                m_application.leftControllerButtonMapping.textButtonY.text = "Volgend project";

                m_application.leftControllerButtonMapping.textLeftThumbUp.text = "Model omhoog";
                m_application.leftControllerButtonMapping.textLeftThumbDown.text = "Model omlaag";
                m_application.leftControllerButtonMapping.textLeftThumbLeft.text = "Model links";
                m_application.leftControllerButtonMapping.textLeftThumbRight.text = "Model rechts";
            }

            // Right controller
            if (m_application.rightControllerButtonMapping != null)
            {
                m_application.rightControllerButtonMapping.textRightIndexTrigger.text = "";
                m_application.rightControllerButtonMapping.textRightHandTrigger.text = "";

                m_application.rightControllerButtonMapping.textButtonOculus.text = "Exit";

                m_application.rightControllerButtonMapping.textButtonA.text = "Vorige locatie";
                m_application.rightControllerButtonMapping.textButtonB.text = "Volgende locatie";

                m_application.rightControllerButtonMapping.textRightThumbUp.text = "Beweeg vooruit";
                m_application.rightControllerButtonMapping.textRightThumbDown.text = "Beweeg achteruit";
                m_application.rightControllerButtonMapping.textRightThumbLeft.text = "Beweeg links";
                m_application.rightControllerButtonMapping.textRightThumbRight.text = "Beweeg rechts";
            }
        }
    }
}