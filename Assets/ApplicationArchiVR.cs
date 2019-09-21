using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchiVR
{
    public class ApplicationArchiVR : MonoBehaviour
    {
        #region Variables

        public string m_version = "190920a";

        #region Game objects

        public Animator m_fadeAnimator = null;

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


        public void OnTeleportFadeOutComplete()
        {
            m_fadeAnimator.ResetTrigger("FadeOut");

            UpdateTrackingSpacePosition();

            m_rightControllerText.text = (m_activePOIIndex == -1 ? "" : m_POI[m_activePOIIndex].name);
            
            m_fadeAnimator.SetTrigger("FadeIn");
        }

        public void OnTeleportFadeInComplete()
        {
            m_fadeAnimator.ResetTrigger("FadeIn");
        }

        void SetActivePOI(int newPOIIndex)
        {
            // Determine wheter we need a fading transition.
            bool needFade = m_activePOIIndex != -1;

            // Determine the new POI index.
            if (m_POI.Count == 0)
            {
                m_activePOIIndex = -1;
            }
            else
            {
                m_activePOIIndex = (newPOIIndex) % m_POI.Count;

                while (m_activePOIIndex < 0)
                {
                    m_activePOIIndex += m_POI.Count;
                }
            }

            if (needFade)
            {
                m_fadeAnimator.ResetTrigger("FadeIn");
                m_fadeAnimator.SetTrigger("FadeOut");
            }
            else
            {
                OnTeleportFadeOutComplete();
            }            
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
}