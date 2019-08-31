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

        // Update is called once per frame
        void Update()
        {
            // Get existing button presses.
            var button1Pressed = OVRInput.Get(OVRInput.Button.One);

            // Get new button presses.
            var button1Down = OVRInput.GetDown(OVRInput.Button.One);
            var button2Down = OVRInput.GetDown(OVRInput.Button.Two);
            var button3Down = OVRInput.GetDown(OVRInput.Button.Three);
            var button4Down = OVRInput.GetDown(OVRInput.Button.Four);
            var aButtonDown = OVRInput.GetDown(OVRInput.RawButton.A);
            var bButtonDown = OVRInput.GetDown(OVRInput.RawButton.B);
            var xButtonDown = OVRInput.GetDown(OVRInput.RawButton.X);
            var yButtonDown = OVRInput.GetDown(OVRInput.RawButton.Y);            
            var lThumbstickButtonDown = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick);
            var rThumbstickButtonDown = OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick);
            var rawButtonLIndexTriggerDown = OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger);
            var rawButtonRIndexTriggerDown = OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger);
            var lThumbstickDirectionUp = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp);
            var lThumbstickDirectionDown = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown);
            var lThumbstickDirectionLeft = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft);
            var lThumbstickDirectionRight = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight);

            // Get thumbstick positions. (X/Y range of -1.0f to 1.0f)
            var lThumbStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            var rThumbStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);


            var text = "[Input]";
            
            text += "\nButton1 Pressed = " + button1Pressed;           

            text += "\nButton1 Down = " + button1Down;
            text += "\nButton2 Down = " + button2Down;
            text += "\nButton3 Down = " + button3Down;

            text += "\nA button Down = " + aButtonDown;
            text += "\nB button Down = " + bButtonDown;

            text += "\nX button Down = " + xButtonDown;
            text += "\nY button Down = " + yButtonDown;
            
            text += "\nL thumbstick: (" + lThumbStick.x + ", " + lThumbStick.y + ")";
            text += "\nR thumbstick: (" + lThumbStick.x + ", " + lThumbStick.y + ")";

            text += "\nL thumbstick Left Down: " + lThumbstickDirectionLeft;            
            text += "\nL thumbstick Right Down: " + lThumbstickDirectionRight;
           

            // returns a float of the secondary (typically the Right) index finger trigger’s current state.  
            // (range of 0.0f to 1.0f)
            text += "\nGetSecondaryIndexTrigger = " + OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);

            // returns a float of the left index finger trigger’s current state.  
            // (range of 0.0f to 1.0f)
            text += "\nGetRawAxis1D.LIndexTrigger = " + OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger);

            // returns true if the left index finger trigger has been pressed more than halfway.  
            // (Interpret the trigger as a button).            
            text += "\nL trigger down = " + rawButtonLIndexTriggerDown;
            text += "\nR trigger down = " + rawButtonRIndexTriggerDown;

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
            bool prevProject = xButtonDown || lThumbstickDirectionUp || Input.GetKeyDown(KeyCode.DownArrow);
            bool nextProject = yButtonDown || lThumbstickDirectionDown || Input.GetKeyDown(KeyCode.UpArrow);
            bool prev = aButtonDown || lThumbstickDirectionLeft || Input.GetKeyDown(KeyCode.LeftArrow);
            bool next = bButtonDown || lThumbstickDirectionRight || Input.GetKeyDown(KeyCode.RightArrow);
            bool toggleModelScale = button3Down || Input.GetKeyDown(KeyCode.S); ;
            bool toggleCanvas = button4Down || Input.GetKeyDown(KeyCode.D); ;

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
