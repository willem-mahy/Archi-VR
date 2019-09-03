using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        UnityEngine.GameObject m_ovrCameraRig = null;

        UnityEngine.GameObject m_hudCanvas = null;

        UnityEngine.GameObject m_debugPanel = null;

        UnityEngine.UI.Text m_debugText = null;

        int m_activeProjectIndex = -1;
        List<UnityEngine.GameObject> m_projects = new List<UnityEngine.GameObject>();

        int m_activePOIIndex = -1;
        List<UnityEngine.GameObject> m_POI = new List<UnityEngine.GameObject>();
              
        int m_activeScaleIndex = 0;
        List<float> m_scales = new List<float>();

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
            m_projects.Clear();

            // Gather all projects
            var projects = GameObject.Find("Projects");

            if (projects != null)
            {
                foreach (Transform child in projects.transform)
                {
                    m_projects.Add(child.gameObject);
                }
            }

            ActivateProject(0);
        }

        GameObject GetActiveProject()
        {
            if (m_activeProjectIndex == -1)
                return null;

            return m_projects[m_activeProjectIndex];
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

            if (activeProject != null)
            {
                // Gather all POI in the current active project.
                foreach (Transform childOfActiveProject in activeProject.transform)
                {
                    var childGameObject = childOfActiveProject.gameObject;

                    if (childGameObject.name == "POI")
                    {
                        var POIs = childGameObject;

                        foreach (Transform childOfPOIs in POIs.transform)
                        {
                            m_POI.Add(childOfPOIs.gameObject);
                        }

                        break;
                    }
                }
            }

            SetActivePOI(0);
        }

        void ActivateProject(int projectIndex)
        {
            if (m_projects.Count == 0)
            {
                m_activeProjectIndex = -1;
            }
            else
            {
                m_activeProjectIndex = (projectIndex) % m_projects.Count;

                while (m_activeProjectIndex < 0)
                {
                    m_activeProjectIndex += m_projects.Count;
                }
            }

            // Set only the active project visible.
            for (int i = 0; i < m_projects.Count; ++i)
            {
                m_projects[i].SetActive(i == m_activeProjectIndex);
            }

            GatherActiveProjectPOI();
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
        }

        void UpdateInput_Unity()
        {
            //var button1ID = "Fire1";
            //var button2ID = "Fire2";
            //var button3ID = "Fire3";
            //var button4ID = "Jump";

            var button1ID = "joystick button 0";
            var button2ID = "joystick button 1";
            var button3ID = "joystick button 2";
            var button4ID = "joystick button 3";
            var startButtonID = "joystick button 7";
            var thumbstickPID = "joystick button 8";
            var thumbstickSID = "joystick button 9";
            var joystickNames = UnityEngine.Input.GetJoystickNames();

            var touchControllersConnected = joystickNames.Length == 2;

            //bool button1Down = touchControllersConnected && UnityEngine.Input.GetButtonDown(button1ID);
            //bool button2Down = touchControllersConnected && UnityEngine.Input.GetButtonDown(button2ID);
            //bool button3Down = touchControllersConnected && UnityEngine.Input.GetButtonDown(button3ID);
            //bool button4Down = touchControllersConnected && UnityEngine.Input.GetButtonDown(button4ID);

            bool button1Down = touchControllersConnected && UnityEngine.Input.GetKeyDown(button1ID);
            bool button2Down = touchControllersConnected && UnityEngine.Input.GetKeyDown(button2ID);
            bool button3Down = touchControllersConnected && UnityEngine.Input.GetKeyDown(button3ID);
            bool button4Down = touchControllersConnected && UnityEngine.Input.GetKeyDown(button4ID);
            bool buttonStartDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(startButtonID);
            bool buttonThumbstickPDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(thumbstickPID);
            bool buttonThumbstickSDown = touchControllersConnected && UnityEngine.Input.GetKeyDown(thumbstickSID);

            bool button1Pressed = touchControllersConnected && UnityEngine.Input.GetButton(button1ID);// "Fire1");
            bool button2Pressed = touchControllersConnected && UnityEngine.Input.GetKey(button2ID);// "joystick button 1");
            bool button3Pressed = touchControllersConnected && UnityEngine.Input.GetButton(button3ID);//"Fire3");
            bool button4Pressed = touchControllersConnected && UnityEngine.Input.GetButton(button4ID);// "Jump");
            bool buttonStartPressed = touchControllersConnected && UnityEngine.Input.GetButton(startButtonID);

            var text = "\nJoysticks:";
            foreach (var n in joystickNames)
            {
                text += "\n -" + n;
            }

            var leftControllerActive = touchControllersConnected && joystickNames[0] == "";
            var rightControllerActive = joystickNames.Length == 2 && joystickNames[1] == "";

            text += "\n";

            text += "\nbuttonOneDown: " + button1Down;
            text += "\nbuttonTwoDown: " + button2Down;
            text += "\nbuttonThreeDown: " + button3Down;
            text += "\nbuttonFourDown: " + button4Down;
            text += "\nbuttonStartDown: " + buttonStartDown;

            text += "\n";

            text += "\nbuttonOnePressed: " + button1Pressed;
            text += "\nbuttonTwoPressed: " + button2Pressed;
            text += "\nbuttonThreePressed: " + button3Pressed;
            text += "\nbuttonFourPressed: " + button4Pressed;
            text += "\nbuttonStartPressed: " + buttonStartPressed;

            text += "\n";

            text += "version: 190903b";

            var activeProject = GetActiveProject();

            if (activeProject != null)
            {
                text += "\n\n" + activeProject.name;

                var activePOI = GetActivePOI();

                if (activePOI != null)
                {
                    text += " > " + activePOI.name;
                }
            }

            // Figure out whether there is something to do.
            bool prevProject = button1Down || Input.GetKeyDown(KeyCode.DownArrow);
            bool nextProject = button2Down || Input.GetKeyDown(KeyCode.UpArrow);

            bool prev = button3Down || Input.GetKeyDown(KeyCode.LeftArrow);
            bool next = button4Down || Input.GetKeyDown(KeyCode.RightArrow);

            bool toggleModelScale = buttonStartDown || Input.GetKeyDown(KeyCode.S);

            bool toggleCanvas = buttonThumbstickPDown || buttonThumbstickSDown || Input.GetKeyDown(KeyCode.D);

            if (toggleCanvas)
            {
                ToggleCanvas();
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

            m_debugText.text = text;
        }

        void UpdateInput_OVR()
        {
            var activeController = OVRInput.GetActiveController();

            var lRemoteConnected = OVRInput.IsControllerConnected(OVRInput.Controller.LTrackedRemote);
            var rRemoteConnected = OVRInput.IsControllerConnected(OVRInput.Controller.RTrackedRemote);
            var lTouchConnected = OVRInput.IsControllerConnected(OVRInput.Controller.LTouch);
            var rTouchConnected = OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);

            // Get existing button presses.
            var button1Pressed = OVRInput.Get(OVRInput.Button.One);
            var lButton1Pressed = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTrackedRemote);
            var rButton1Pressed = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTrackedRemote);
            var button2Pressed = OVRInput.Get(OVRInput.Button.Two);
            var lButton2Pressed = OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTrackedRemote);
            var rButton2Pressed = OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTrackedRemote);
            var button3Pressed = OVRInput.Get(OVRInput.Button.Three);
            var button4Pressed = OVRInput.Get(OVRInput.Button.Four);

            var aButtonPressed = OVRInput.Get(OVRInput.RawButton.A);
            var bButtonPressed = OVRInput.Get(OVRInput.RawButton.B);
            var xButtonPressed = OVRInput.Get(OVRInput.RawButton.X);
            var yButtonPressed = OVRInput.Get(OVRInput.RawButton.Y);
            var startButtonPressed = OVRInput.Get(OVRInput.RawButton.Start);

            // Get new button presses.
            var button1Down = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.Touch);
            var button2Down = OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.Touch);
            var button3Down = OVRInput.GetDown(OVRInput.Button.Three, OVRInput.Controller.Touch);
            var button4Down = OVRInput.GetDown(OVRInput.Button.Four, OVRInput.Controller.Touch);

            var aButtonDown = OVRInput.GetDown(OVRInput.RawButton.A);
            var bButtonDown = OVRInput.GetDown(OVRInput.RawButton.B);
            var xButtonDown = OVRInput.GetDown(OVRInput.RawButton.X);
            var yButtonDown = OVRInput.GetDown(OVRInput.RawButton.Y);
            var startButtonDown = OVRInput.GetDown(OVRInput.RawButton.Start);

            var lThumbstickButtonDown = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch);
            var rThumbstickButtonDown = OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch);

            // returns true if the left index finger trigger has been pressed more than halfway.  
            // (Interpret the trigger as a button).
            var rawButtonLIndexTriggerDown = OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger);

            // returns true if the left index finger trigger has been pressed more than halfway.  
            // (Interpret the trigger as a button).
            var rawButtonRIndexTriggerDown = OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger);

            var lThumbstickDirectionUp = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.Touch);
            var lThumbstickDirectionDown = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.Touch);
            var lThumbstickDirectionLeft = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.Touch);
            var lThumbstickDirectionRight = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.Touch);

            // Get thumbstick positions. (X/Y range of -1.0f to 1.0f)
            var lThumbStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            var rThumbStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

            var text = "";
            text += "\nRemote connection: L=" + (lRemoteConnected ? "OK" : "NA") + " R=" + (rRemoteConnected ? "OK" : "NA");
            text += "\nTouch connection: L=" + (lTouchConnected ? "OK" : "NA") + " R=" + (rTouchConnected ? "OK" : "NA");
            text += "\nActive Controller: " + (activeController == OVRInput.Controller.LTouch ? " L" : "") + (activeController == OVRInput.Controller.RTouch ? " R" : "");

            text += "\nButton 1 = " + (button1Down ? "Down" : (button1Pressed ? "Pressed" : ""));
            text += "\nButton 2 = " + (button2Down ? "Down" : (button2Pressed ? "Pressed" : ""));
            text += "\nButton 3 = " + (button3Down ? "Down" : (button3Pressed ? "Pressed" : ""));
            text += "\nButton 4 = " + (button4Down ? "Down" : (button4Pressed ? "Pressed" : ""));

            text += "\nA button = " + (aButtonDown ? "Down" : (aButtonPressed ? "Pressed" : ""));
            text += "\nB button = " + (bButtonDown ? "Down" : (bButtonPressed ? "Pressed" : ""));
            text += "\nX button = " + (xButtonDown ? "Down" : (xButtonPressed ? "Pressed" : ""));
            text += "\nY button = " + (yButtonDown ? "Down" : (yButtonPressed ? "Pressed" : ""));

            text += "\nL thumbstick: (" + lThumbStick.x + ", " + lThumbStick.y + ")";
            text += "\nR thumbstick: (" + lThumbStick.x + ", " + lThumbStick.y + ")";

            text += "\nL thumbstick Left Down: " + lThumbstickDirectionLeft;            
            text += "\nL thumbstick Right Down: " + lThumbstickDirectionRight;

            // returns a float of the index finger trigger’s current state.  
            // (range of 0.0f to 1.0f)

            // secondary (typically the Right) 
            text += "\nPrimary IndexTrigger = " + OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) + (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger) ? " Down" : ""); ;
            text += "\nSecondary IndexTrigger = " + OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) + (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) ? " Down" : ""); ;

            // left
            text += "\nL IndexTrigger = " + OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger) + (rawButtonLIndexTriggerDown ? " Down" : "");

            // right
            text += "\nR IndexTrigger = " + OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) + (rawButtonRIndexTriggerDown ? " Down" : "");
            
            // returns true if the secondary gamepad button, typically “B”, is currently touched by the user.
            //text += "\nGetTouchTwo = " + OVRInput.Get(OVRInput.Touch.Two);

            var activeProject = GetActiveProject();

            if (activeProject != null)
            {
                text += "\n\n" + activeProject.name;

                var activePOI = GetActivePOI();

                if (activePOI != null)
                {
                    text += " > " + activePOI.name;
                }
            }

            // Figure out whether there is something to do.
            bool prevProject = button1Down || xButtonDown || lThumbstickDirectionUp || Input.GetKeyDown(KeyCode.DownArrow);
            bool nextProject = yButtonDown || lThumbstickDirectionDown || Input.GetKeyDown(KeyCode.UpArrow);
            bool prev = button2Down || aButtonDown || lThumbstickDirectionLeft || Input.GetKeyDown(KeyCode.LeftArrow);
            bool next = bButtonDown || lThumbstickDirectionRight || Input.GetKeyDown(KeyCode.RightArrow);
            bool toggleModelScale = lThumbstickButtonDown || Input.GetKeyDown(KeyCode.S); ;
            bool toggleCanvas = startButtonDown || Input.GetKeyDown(KeyCode.D); ;

            if (toggleCanvas)
            {
                ToggleCanvas();
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

            m_debugText.text = text;
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
    }
}
