using System.Collections;
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
        UnityEngine.GameObject m_ovrCameraRig = null;

        UnityEngine.GameObject m_hudCanvas = null;

        UnityEngine.GameObject m_debugPanel = null;

        UnityEngine.UI.Text m_debugText = null;

        int m_loadingProjectIndex = -1;
        int m_activeProjectIndex = -1;
        List<string> m_projectNames = new List<string>();

        int m_activePOIIndex = -1;
        List<UnityEngine.GameObject> m_POI = new List<UnityEngine.GameObject>();

        int m_activeScaleIndex = 0;
        List<float> m_scales = new List<float>();

        #endregion Variables

        // Start is called before the first frame update
        void Start()
        {
            m_ovrCameraRig = GameObject.Find("OVRCameraRig");

            m_hudCanvas = GameObject.Find("HUDCanvas");
            m_debugPanel = GameObject.Find("DebugPanel");
            m_debugText = GameObject.Find("DebugText").GetComponent<UnityEngine.UI.Text>();

            m_scales.Add(1.0f);
            m_scales.Add(0.04f);

            GatherProjects();
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

            m_loadingProjectIndex = projectIndex;

            StartCoroutine(LoadProject());
        }

        void ToggleCanvas()
        {
            m_hudCanvas.SetActive(!m_hudCanvas.activeSelf);
        }

        void UpdatePosition()
        {
            var activePOI = GetActivePOI();

            var position = new Vector3();
            var rotation = new Quaternion();

            if (activePOI == null)
            {
                rotation = Quaternion.identity;
                position = Vector3.zero;
            }
            else
            {
                rotation = m_POI[m_activePOIIndex].transform.rotation;
                position = m_POI[m_activePOIIndex].transform.position + new Vector3(0, 1.7f, 0);
            }

            m_ovrCameraRig.transform.position = position;
            m_ovrCameraRig.transform.rotation = rotation;
        }

        void ToggleModelScale()
        {
            m_activeScaleIndex = 1 - m_activeScaleIndex;

            var activeProject = GetActiveProject();

            if (activeProject == null)
            {
                return;
            }

            var scale = m_scales[m_activeScaleIndex];
            activeProject.transform.localScale = new Vector3(scale, scale, scale);
        }

        private void FixedUpdate()
        {
            //OVRInput.FixedUpdate();
        }

        // Update is called once per frame
        void Update()
        {
            //OVRInput.Update();

            UpdateInput_Unity();
            //UpdateInput_OVR();

            //-------------------


            // Figure out whether there is something to do.
            bool prevProject = button1Down || Input.GetKeyDown(KeyCode.DownArrow);
            bool nextProject = button2Down || Input.GetKeyDown(KeyCode.UpArrow);

            bool prev = button3Down || Input.GetKeyDown(KeyCode.LeftArrow);
            bool next = button4Down || Input.GetKeyDown(KeyCode.RightArrow);

            bool toggleModelScale = buttonThumbstickPDown || buttonThumbstickSDown || Input.GetKeyDown(KeyCode.S);

            bool toggleCanvas = false;//buttonStartDown || Input.GetKeyDown(KeyCode.M);
            bool toggleMenu = buttonStartDown || Input.GetKeyDown(KeyCode.M);
            bool toggleInput = Input.GetKeyDown(KeyCode.I);

            if (toggleInput)
            {
                inputMode = 1 - inputMode;
            }


            if (toggleCanvas)
            {
                ToggleCanvas();
            }

            if (toggleMenu)
            {
                menuMode = ++menuMode % 3;
            }

            if (toggleModelScale)
            {
                ToggleModelScale();
            }

            if (nextProject)
            {
                ActivateProject(m_activeProjectIndex + 1);
            }

            if (prevProject)
            {
                ActivateProject(m_activeProjectIndex - 1);
            }

            if (next)
            {
                OffsetActivePOIIndex(+1);
            }

            if (prev)
            {
                OffsetActivePOIIndex(-1);
            }

            //----------------------

            UpdateMenu();
        }

        private int menuMode = MenuDebug;

        const int MenuNone = 0;
        const int MenuDebug = 1;
        const int MenuInfo = 2;

        // The HUD menu text
        string text = "";

        void UpdateMenu()
        {
            // Reset HUD menu text.
            text = "";

            // Update HUD menu text. (if not 'None')
            switch (menuMode)
            {
                case MenuDebug:
                    UpdateMenuDebug();
                    break;
                case MenuInfo:
                    UpdateMenuInfo();
                    break;
                case MenuNone:
                    break;
                default:
                    text += "Unsupported menu mode: " + menuMode.ToString();
                    break;
            }

            // Push HUD menu text to UI.
            m_debugText.text = text;

            m_debugPanel.SetActive(MenuNone != menuMode);
        }

        void UpdateMenuDebug()
        {
            text += "\nInput mode: " + (inputMode == InputUnity ? "Unity" : "OVR");
            text += "\n";
            text += "\nRemote connection: L=" + (lRemoteConnected ? "OK" : "NA") + " R=" + (rRemoteConnected ? "OK" : "NA");
            text += "\nTouch connection: L=" + (lTouchConnected ? "OK" : "NA") + " R=" + (rTouchConnected ? "OK" : "NA");
            text += "\nActive Controller: " + (activeController == OVRInput.Controller.LTouch ? " L" : "") + (activeController == OVRInput.Controller.RTouch ? " R" : "");
            text += "\n";
            text += "\nThumbstick: L(" + lThumbStick.x + ", " + lThumbStick.y + ") R(" + rThumbStick.x + ", " + rThumbStick.y + ")";
            text += "\n";
            text += "\nL thumbstick:";
            text += "\n Left: " + (lThumbstickDirectionLeftDown ? "Down" : (lThumbstickDirectionLeftPressed ? "Pressed" : ""));
            text += "\n Right: " + (lThumbstickDirectionRightDown ? "Down" : (lThumbstickDirectionRightPressed ? "Pressed" : ""));
            text += "\n Up: " + (lThumbstickDirectionUpDown ? "Down" : (lThumbstickDirectionUpPressed ? "Pressed" : ""));
            text += "\n Down: " + (lThumbstickDirectionDownDown ? "Down" : (lThumbstickDirectionDownDown ? "Pressed" : ""));


            if (inputMode == InputUnity)
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
                text += "\nPrimary IndexTrigger = " + OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) + (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger) ? " Down" : ""); ;
                // secondary trigger (typically the Right) 
                text += "\nSecondary IndexTrigger = " + OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) + (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) ? " Down" : ""); ;

                // left
                text += "\nL IndexTrigger = " + OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger) + (rawButtonLIndexTriggerDown ? " Down" : "");

                // right
                text += "\nR IndexTrigger = " + OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) + (rawButtonRIndexTriggerDown ? " Down" : "");

                // returns true if the secondary gamepad button, typically “B”, is currently touched by the user.
                //text += "\nGetTouchTwo = " + OVRInput.Get(OVRInput.Touch.Two);   
            }

            text += "\n";

            //TODO Probably obsolete -> Remove?
            //text += "\nbuttonOneDown: " + button1Down;
            //text += "\nbuttonTwoDown: " + button2Down;
            //text += "\nbuttonThreeDown: " + button3Down;
            //text += "\nbuttonFourDown: " + button4Down;
            //text += "\nbuttonStartDown: " + buttonStartDown;

            //text += "\n";

            //text += "\nbuttonOnePressed: " + button1Pressed;
            //text += "\nbuttonTwoPressed: " + button2Pressed;
            //text += "\nbuttonThreePressed: " + button3Pressed;
            //text += "\nbuttonFourPressed: " + button4Pressed;
            //text += "\nbuttonStartPressed: " + buttonStartPressed;

            text += "\nButton 1 = " + (button1Down ? "Down" : (button1Pressed ? "Pressed" : ""));
            text += "\nButton 2 = " + (button2Down ? "Down" : (button2Pressed ? "Pressed" : ""));
            text += "\nButton 3 = " + (button3Down ? "Down" : (button3Pressed ? "Pressed" : ""));
            text += "\nButton 4 = " + (button4Down ? "Down" : (button4Pressed ? "Pressed" : ""));

            // TODO Implement ABXY?
            //text += "\nA button = " + (aButtonDown ? "Down" : (aButtonPressed ? "Pressed" : ""));
            //text += "\nB button = " + (bButtonDown ? "Down" : (bButtonPressed ? "Pressed" : ""));
            //text += "\nX button = " + (xButtonDown ? "Down" : (xButtonPressed ? "Pressed" : ""));
            //text += "\nY button = " + (yButtonDown ? "Down" : (yButtonPressed ? "Pressed" : ""));
        }

        new List<string> GetProjectNames()
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
            text += "\nversion: 190905a";
        }

        const int InputUnity = 0;
        const int InputOVR = 1;

        int inputMode = InputUnity;

        //const string button1ID = "Fire1";
        //const string button2ID = "Fire2";
        //const string button3ID = "Fire3";
        //const string button4ID = "Jump";
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

        bool button1Down = false;
        bool button2Down = false;
        bool button3Down = false;
        bool button4Down = false;
        bool buttonStartDown = false;
        bool buttonThumbstickPDown = false;
        bool buttonThumbstickSDown = false;

        bool button1Pressed = false;
        bool button2Pressed = false;
        bool button3Pressed = false;
        bool button4Pressed = false;
        bool buttonStartPressed = false;

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

        void UpdateInput_Unity()
        {
            // Get the names of the currently connected joystick devices.          
            joystickNames = UnityEngine.Input.GetJoystickNames();

            touchControllersConnected = joystickNames.Length == 2;

            button1Down = touchControllersConnected && UnityEngine.Input.GetKeyDown(button1ID);
            button2Down = touchControllersConnected && UnityEngine.Input.GetKeyDown(button2ID);
            button3Down = touchControllersConnected && UnityEngine.Input.GetKeyDown(button3ID);
            button4Down = touchControllersConnected && UnityEngine.Input.GetKeyDown(button4ID);
            buttonStartDown =
                touchControllersConnected && UnityEngine.Input.GetKeyDown(startButtonID)
                || touchControllersConnected && UnityEngine.Input.GetKeyDown(button5ID)
                || touchControllersConnected && UnityEngine.Input.GetKeyDown(button6ID)
                || touchControllersConnected && UnityEngine.Input.GetKeyDown(button7ID);

            buttonThumbstickPDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(thumbstickPID);
            buttonThumbstickSDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(thumbstickSID);

            button1Pressed = touchControllersConnected && UnityEngine.Input.GetButton(button1ID);// "Fire1");
            button2Pressed = touchControllersConnected && UnityEngine.Input.GetKey(button2ID);// "joystick button 1");
            button3Pressed = touchControllersConnected && UnityEngine.Input.GetButton(button3ID);//"Fire3");
            button4Pressed = touchControllersConnected && UnityEngine.Input.GetButton(button4ID);// "Jump");
            buttonStartPressed = touchControllersConnected && UnityEngine.Input.GetButton(startButtonID);

            leftControllerActive = touchControllersConnected && joystickNames[0] == "";
            rightControllerActive = joystickNames.Length == 2 && joystickNames[1] == "";
        }

        void UpdateInput_OVR()
        {
            var activeController = OVRInput.GetActiveController();

            var lRemoteConnected = OVRInput.IsControllerConnected(OVRInput.Controller.LTrackedRemote);
            var rRemoteConnected = OVRInput.IsControllerConnected(OVRInput.Controller.RTrackedRemote);
            var lTouchConnected = OVRInput.IsControllerConnected(OVRInput.Controller.LTouch);
            var rTouchConnected = OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);

            // Get existing button presses.
            button1Pressed = OVRInput.Get(OVRInput.Button.One);
            button2Pressed = OVRInput.Get(OVRInput.Button.Two);
            button3Pressed = OVRInput.Get(OVRInput.Button.Three);
            button4Pressed = OVRInput.Get(OVRInput.Button.Four);

            var lButton1Pressed = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTrackedRemote);
            var rButton1Pressed = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTrackedRemote);
            var lButton2Pressed = OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTrackedRemote);
            var rButton2Pressed = OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTrackedRemote);

            var aButtonPressed = OVRInput.Get(OVRInput.RawButton.A);
            var bButtonPressed = OVRInput.Get(OVRInput.RawButton.B);
            var xButtonPressed = OVRInput.Get(OVRInput.RawButton.X);
            var yButtonPressed = OVRInput.Get(OVRInput.RawButton.Y);
            var startButtonPressed = OVRInput.Get(OVRInput.RawButton.Start);

            // Get new button presses.
            button1Down = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.Touch);
            button2Down = OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.Touch);
            button3Down = OVRInput.GetDown(OVRInput.Button.Three, OVRInput.Controller.Touch);
            button4Down = OVRInput.GetDown(OVRInput.Button.Four, OVRInput.Controller.Touch);

            var aButtonDown = OVRInput.GetDown(OVRInput.RawButton.A);
            var bButtonDown = OVRInput.GetDown(OVRInput.RawButton.B);
            var xButtonDown = OVRInput.GetDown(OVRInput.RawButton.X);
            var yButtonDown = OVRInput.GetDown(OVRInput.RawButton.Y);
            var startButtonDown = OVRInput.GetDown(OVRInput.RawButton.Start);

            buttonThumbstickPDown = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch);
            buttonThumbstickSDown = OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch);

            // returns true if the left index finger trigger has been pressed more than halfway.  
            // (Interpret the trigger as a button).
            var rawButtonLIndexTriggerDown = OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger);

            // returns true if the left index finger trigger has been pressed more than halfway.  
            // (Interpret the trigger as a button).
            var rawButtonRIndexTriggerDown = OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger);

            lThumbstickDirectionUpDown = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.Touch);
            lThumbstickDirectionDownDown = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.Touch);
            lThumbstickDirectionLeftDown = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.Touch);
            lThumbstickDirectionRightDown = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.Touch);

            lThumbstickDirectionUpPressed = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.Touch);
            lThumbstickDirectionDownPressed = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.Touch);
            lThumbstickDirectionLeftPressed = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.Touch);
            lThumbstickDirectionRightPressed = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.Touch);

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

            UpdatePosition();
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

            // Apply necessary scaling (we might be in 'Maquette' mode)
            //ApplyScaling(); 
        }
    }
}
