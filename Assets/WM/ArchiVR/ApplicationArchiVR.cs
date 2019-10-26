using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WM.ArchiVR.Command;
using WM.Net;
using WM.UI;
using WM.VR;

[assembly: System.Reflection.AssemblyVersion("1.0.*")]

namespace WM
{
    namespace ArchiVR
    {
        public class ApplicationArchiVR : MonoBehaviour
        {
            #region Variables

            #region Startup options
            
            //! The startup network mode. (Default: Standalone)
            public NetworkMode StartupNetworkMode = NetworkMode.Standalone;

            //! FPS UI visibility. (Default: false)
            public bool StartupShowFps = false;

            #endregion

            //! The current network mode.
            public NetworkMode NetworkMode = NetworkMode.Standalone; // TODO: make private...

            //! The command queue.
            public List<ICommand> CommandQueue = new List<ICommand>();

            // The application version.
            public string Version = "";

            // Whether to show the GFX quality level and FPS as HUD UI.
            private bool enableDebugGFX = false;

            // The multiplayer server.
            public Server Server;

            // The multiplayer client.
            public Client Client;

            #region Game objects

            // Reference to the avatar Prefabs. Drag Prefabs into this field in the Inspector.
            public List<GameObject> avatarPrefabs = new List<GameObject>();

            //! Instantiates all available avatar prefabs.
            void InstanciateAllAvatarPrefabs()
            {
                float x = -3;

                foreach (var ap in avatarPrefabs)
                {
                    Instantiate(
                        ap,
                        new Vector3(x, 0, 0),
                        Quaternion.identity);

                    x += 2;
                }
            }

            //! The avatar for the local player.
            public int AvatarIndex = 0;

            //! Instanciates an avatar prefab to represent a newly connected client.
            public void ConnectClient(
                string clientIP,
                int avatarIndex)
            {
                lock (avatars)
                {
                    avatars[clientIP] = InstanciateAvatarPrefab(avatarIndex);
                }
            }

            //! Destroys the avatar prefab for the given disconnected client.
            public void DisconnectClient(string clientIP)
            {
                lock (avatars)
                {
                    if (avatars.ContainsKey(clientIP))
                    {
                        GameObject.Destroy(avatars[clientIP]);
                        avatars.Remove(clientIP);
                    }
                }
            }

            //! Updates the avatar type for the given connected client.
            public void SetClientAvatar(
                string ip,
                int avatarIndex)
            {
                lock (avatars)
                {
                    var oldAvatar = (avatars.ContainsKey(ip) ? avatars[ip] : null);

                    if (oldAvatar == null)
                    {
                        Debug.LogWarning("SetClientAvatar(): No existing avatar found for client '" + ip + "'");
                    }

                    avatars[ip] = InstanciateAvatarPrefab(avatarIndex);

                    if (oldAvatar != null)
                    {
                        Destroy(oldAvatar);
                    }
                }
            }

            //! Instanciates the avatar type at given index, and returns a reference to the instance.
            GameObject InstanciateAvatarPrefab(int avatarIndex)
            {
                return Instantiate(
                        avatarPrefabs[avatarIndex],
                        new Vector3(0, 0, 0),
                        Quaternion.identity);
            }

            //! The avatar instanciations, mapped on the IP of the client they represent.
            public Dictionary<string, GameObject> avatars = new Dictionary<string, GameObject>();

            public Animator m_fadeAnimator;

            public HUDMenu HudMenu;

            public GameObject FpsPanelHUD;

            public GameObject m_ovrCameraRig;

            public GameObject m_centerEyeAnchor;

            public GameObject m_leftHandAnchor;

            public GameObject m_rightHandAnchor;

            #endregion

            public TeleportCommand TeleportCommand { get; set; }

            public GameObject Sun { get; set; }

            #region Project

            // The list of names of all projects included in the build.
            List<string> m_projectNames = new List<string>();

            // The index of the currently active project.
            public int ActiveProjectIndex { get; set; } = -1;

            #endregion

            #region Model Layers

            private List<GameObject> m_modelLayers = new List<GameObject>();

            #endregion

            #region POI

            private int activePOIIndex = -1;

            public int ActivePOIIndex
            {
                get { return activePOIIndex; }
                set
                {
                    activePOIIndex = value;
                    ActivePOIName = ActivePOI != null ? ActivePOI.name : null;
                }
            }

            public GameObject ActivePOI
            {
                get
                {
                    if (ActivePOIIndex == -1)
                    {
                        return null;
                    }

                    return m_POI[ActivePOIIndex];
                }
            }

            public string ActivePOIName
            {
                get; private set;
            } = "";

            List<GameObject> m_POI = new List<GameObject>();

            #endregion

            #region Application State

            // The ammication states enumeration.
            public enum ApplicationStates : int
            {
                None = -1,
                Default,
                Teleporting,
                LoadingProject
            };

            // The typed application states.
            ApplicationStateDefault m_applicationStateDefault = new ApplicationStateDefault();
            ApplicationStateTeleporting m_applicationStateTeleporting = new ApplicationStateTeleporting();
            
            // The generic list of application states.
            List<ApplicationState> m_applicationStates = new List<ApplicationState>();

            // The active immersion mode index.
            private int m_activeApplicationStateIndex = (int)ApplicationStates.None;

            //! Gets the active application state.  Returns null if no state is active.
            public ApplicationState GetActiveApplicationState()
            {
                if (m_activeApplicationStateIndex == -1)
                    return null;

                return m_applicationStates[m_activeApplicationStateIndex];
            }

            //! Sets the active application state.
            public ApplicationState SetActiveApplicationState(ApplicationStates applicationState)
            {
                if (GetActiveApplicationState() != null)
                {
                    GetActiveApplicationState().Exit();
                }

                m_activeApplicationStateIndex = (int)applicationState;

                if (GetActiveApplicationState() != null)
                {
                    GetActiveApplicationState().Enter();
                }

                return m_applicationStates[m_activeApplicationStateIndex];
            }

            #endregion

            #region Immersion mode

            public const int DefaultImmersionModeIndex = 0;

            // The immersion mode.
            List<ImmersionMode> m_immersionModes = new List<ImmersionMode>();

            // The active immersion mode index.
            public int ActiveImmersionModeIndex { get; set; } = -1;

            public ImmersionMode ActiveImmersionMode
            {
                get
                {
                    if (ActiveImmersionModeIndex == -1)
                    {
                        return null;
                    }

                    return m_immersionModes[ActiveImmersionModeIndex];
                }
            }

            #endregion

            #region Controller UI

            #region Button mapping UI

            public ButtonMappingUI leftControllerButtonMapping = null;
            public ButtonMappingUI rightControllerButtonMapping = null;

            #endregion

            #region Pick Ray

            // Right controller pick ray.
            public GameObject RPickRayGameObject = null;
            public PickRay RPickRay = null;

            // Right controller pick ray.
            public GameObject LPickRayGameObject = null;
            public PickRay LPickRay = null;

            #endregion

            #endregion

            #region HUD menu

            enum MenuMode
            {
                None = 0,
                Network, 
                Graphics,
                Info,
                DebugLog,
                DebugInput
            }

            // The menu mode.
            private MenuMode menuMode = MenuMode.None;

            public GameObject m_centerEyeCanvas = null;

            List<GameObject> menus = new List<GameObject>();

            GameObject debugLogMenuPanel = null;
            GameObject debugInputMenuPanel = null;
            GameObject graphicsMenuPanel = null;
            GameObject networkMenuPanel = null;
            GameObject infoMenuPanel = null;

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
            public const float DefaultFlySpeedHorizontal = 1.0f;

            public float m_flySpeedUpDown = DefaultFlySpeedUpDown;
            public float m_flySpeedHorizontal = DefaultFlySpeedHorizontal;

            #endregion

            private GameObject Avatar;

            #endregion Variables

            #region GameObject overrides


            //! Start is called before the first frame update
            void Start()
            {
                if (Application.isEditor)
                {
                    HudMenu.AnchorEnabled = true;
                }

                //InstanciateAllAvatarPrefabs();

                //QueueCommand(new InitNetworkCommand(StartupNetworkMode));
                new InitNetworkCommand(StartupNetworkMode).Execute(this);

                #region Automatically get build version

                // Get from assembly meta info.
                var assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

                var buildDate = new DateTime(2000, 1, 1)     // baseline is 01/01/2000
                .AddDays(assemblyVersion.Build)             // build is number of days after baseline
                .AddSeconds(assemblyVersion.Revision * 2);    // revision is half the number of seconds into the day

                //Console.WriteLine("Major   : {0}", assemblyVersion.Major);
                //Console.WriteLine("Minor   : {0}", assemblyVersion.Minor);
                //Console.WriteLine("Build   : {0} = {1}", assemblyVersion.Build, buildDate.ToShortDateString());
                //Console.WriteLine("Revision: {0} = {1}", assemblyVersion.Revision, buildDate.ToLongTimeString());
                Version = buildDate.ToShortDateString() + " " + buildDate.ToLongTimeString();
                Debug.Log("Application version: " + Version);

                #endregion

                #region Get handles to game objects

                if (Avatar == null)
                    Avatar = GameObject.Find("Avatar");

                if (Sun == null)
                    Sun = GameObject.Find("Sun");

                if (m_ovrCameraRig == null)
                    m_ovrCameraRig = GameObject.Find("OVRCameraRig");

                if (m_centerEyeAnchor == null)
                    m_centerEyeAnchor = GameObject.Find("CenterEyeAnchor");

                if (m_leftHandAnchor == null)
                    m_leftHandAnchor = GameObject.Find("LeftHandAnchor");

                if (m_rightHandAnchor == null)
                    m_rightHandAnchor = GameObject.Find("RightHandAnchor");

                m_centerEyeCanvas = GameObject.Find("CenterEyeCanvas");
                
                debugInputMenuPanel = GameObject.Find("DebugInputMenuPanel");
                menus.Add(debugInputMenuPanel);
                debugLogMenuPanel = GameObject.Find("DebugLogMenuPanel");
                menus.Add(debugLogMenuPanel);
                graphicsMenuPanel = GameObject.Find("GraphicsMenuPanel");
                menus.Add(graphicsMenuPanel);
                networkMenuPanel = GameObject.Find("NetworkMenuPanel");
                menus.Add(networkMenuPanel);
                infoMenuPanel = GameObject.Find("InfoMenuPanel");
                menus.Add(infoMenuPanel);

                SetActiveMenu(null);

                // Get reference to FPS panel.
                if (FpsPanelHUD == null)
                {
                    FpsPanelHUD = UtilUnity.TryFindGameObject("FPSPanel");
                }
                if (FpsPanelHUD != null)
                {
                    FpsPanelHUD.SetActive(StartupShowFps);
                }

                // Left controller.

                // Pick ray.
                LPickRayGameObject = GameObject.Find("L PickRay");
                LPickRay = LPickRayGameObject.GetComponent<PickRay>();

                m_leftControllerCanvas = GameObject.Find("LeftControllerCanvas");
                m_leftControllerPanel = GameObject.Find("LeftControllerPanel");
                m_leftControllerText = GameObject.Find("LeftControllerText").GetComponent<UnityEngine.UI.Text>();

                // Right controller.

                // Pick ray.
                RPickRayGameObject = GameObject.Find("R PickRay");
                RPickRay = RPickRayGameObject.GetComponent<PickRay>();

                m_rightControllerCanvas = GameObject.Find("RightControllerCanvas");
                m_rightControllerPanel = GameObject.Find("RightControllerPanel");
                m_rightControllerText = GameObject.Find("RightControllerText").GetComponent<UnityEngine.UI.Text>();

                #endregion

                // Disable all pickrays.
                LPickRayGameObject.SetActive(false);
                RPickRayGameObject.SetActive(false);

                #region Init immersion modes.

                m_immersionModes.Add(new ImmersionModeWalkthrough());
                m_immersionModes.Add(new ImmersionModeMaquette());

                foreach (var immersionMode in m_immersionModes)
                {
                    immersionMode.m_application = this;
                    immersionMode.Init();
                }

                #endregion

                #region Init application states.

                m_applicationStates.Add(m_applicationStateDefault);
                m_applicationStates.Add(m_applicationStateTeleporting);
                //m_applicationStates.Add(m_applicationStateLoadingProject);

                foreach (var applicationState in m_applicationStates)
                {
                    applicationState.m_application = this;
                    applicationState.Init();
                }

                #endregion

                GatherProjects();

                SetActiveImmersionMode(DefaultImmersionModeIndex);

                SetActiveProject(0);
            }

            private object commandQueueLock = new object();

            public void QueueCommand(ICommand command)
            {
                lock (commandQueueLock)
                {
                    CommandQueue.Add(command);
                }
            }

            public GameObject ActiveMenu { get; private set; }

            private void SetActiveMenu(GameObject activeMenu)
            {
                ActiveMenu = activeMenu;

                foreach (var menu in menus)
                {
                    menu.SetActive(menu == activeMenu);
                }
            }


            Vector3 m_centerEyeAnchorPrev = new Vector3();

            int frame = 0;

            //! Update is called once per frame
            void Update()
            {
                // TODO: WHY THAF is this necessary to make camera work in Editor?
                m_centerEyeAnchor.GetComponent<Camera>().enabled = false;
                m_centerEyeAnchor.GetComponent<Camera>().enabled = true;
                
                UpdateControllersLocation();

                if (this.TeleportCommand == null) // TODO? : Move the processing of commands to ApplicationStateDefault:Update()?
                {
                    lock (commandQueueLock)
                    {
                        foreach (var command in CommandQueue)
                        {
                            command.Execute(this);
                        }
                        CommandQueue.Clear();
                    }
                }

                if (NetworkMode != NetworkMode.Standalone)
                {
                    if (((m_centerEyeAnchor.transform.position - m_centerEyeAnchorPrev).magnitude > 0.01f) || (frame++ % 10 == 0))
                    {
                        Client.SendAvatarStateToUdp(
                            m_centerEyeAnchor,
                            m_leftHandAnchor,
                            m_rightHandAnchor);
                        m_centerEyeAnchorPrev = m_centerEyeAnchor.transform.position;
                    }

                    // Update positions of remote client avatars, with the avatar states received from the server via UDP.
                    Client.UpdateAvatarStatesFromUdp();
                }

                if (enableDebugGFX)
                {
                    var qualityLevel = QualitySettings.GetQualityLevel();

                    if (m_controllerInput.m_controllerState.button5Down)
                    {
                        qualityLevel = UtilIterate.MakeCycle(--qualityLevel, 0, QualitySettings.names.Length);
                        QualitySettings.SetQualityLevel(qualityLevel);
                    }
                    if (m_controllerInput.m_controllerState.button6Down)
                    {
                        qualityLevel = UtilIterate.MakeCycle(++qualityLevel, 0, QualitySettings.names.Length);
                        QualitySettings.SetQualityLevel(qualityLevel);
                    }
                }

                #region Animate sun

                var sunSpeed = 0.0f; // 0.01f

                if (sunSpeed != 0.0f)
                {
                    Sun.transform.Rotate(Vector3.up, Time.deltaTime * sunSpeed);
                }

                #endregion

                #region Update controller state.

                m_controllerInput.Update();

                #endregion

                #region Update Button Mapping UI to current controller state.

                if (leftControllerButtonMapping != null)
                {
                    leftControllerButtonMapping.Update(m_controllerInput.m_controllerState);
                }

                if (rightControllerButtonMapping != null)
                {
                    rightControllerButtonMapping.Update(m_controllerInput.m_controllerState);
                }

                #endregion

                #region Toggle HUD menu.

                // HUD menu is toggled using left controller Start button, or F11 button in Editor.
                bool toggleMenu = m_controllerInput.m_controllerState.buttonStartDown || Input.GetKeyDown(KeyCode.F11);

                if (toggleMenu)
                {
                    ToggleMenuMode();
                }

                #endregion

                if (GetActiveApplicationState() != null)
                {
                    GetActiveApplicationState().Update();
                }
            }

            #endregion

            #region Project management

            //! Gathers all projects included in the application.
            void GatherProjects()
            {
                m_projectNames = GetProjectNames();
            }

            //! Gets a list containing the names of all projects included in the application.
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

            //! Gets the name of a project, by index.
            public string GetProjectName(int projectIndex)
            {
                return m_projectNames[projectIndex];
            }

            //! Gets the active project's name, or null if no project active.
            public string ActiveProjectName
            {
                get
                {
                    // We delip-berately do NOT return the temptingly simple ActiveProject.name here.
                    // This returns the name (always "Project) of the gameobjet representing the project in the scene.
                    return ActiveProjectIndex == -1 ? null : m_projectNames[ActiveProjectIndex];
                }
            }

            //! Gets the active project.
            public GameObject ActiveProject
            {
                get
                {
                    if (ActiveProjectIndex == -1)
                    {
                        return null;
                    }

                    return GameObject.Find("Project");
                }
            }

            //! Activates a project, by index.
            void SetActiveProject(int projectIndex)
            {
                if (m_projectNames.Count == 0)
                {
                    projectIndex = -1;
                    return;
                }
                else
                {
                    projectIndex = UtilIterate.MakeCycle(projectIndex, 0, m_projectNames.Count);
                }

                if (projectIndex == ActiveProjectIndex)
                {
                    return;
                }

                var tc = new TeleportCommand();
                tc.ProjectIndex = projectIndex;
                tc.POIName = ActivePOIName;

                Teleport(tc);
            }
            #endregion

            #region POI management

            //! Gathers all POI for the currently active project.
            public void GatherActiveProjectPOI()
            {
                m_POI.Clear();

                var activeProject = ActiveProject;

                if (activeProject == null)
                {
                    return;
                }

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

            /*! Gets the default POI index.
             *
             * If a project is activated, and there is a POI with the same name as the active POI, that POI is activated.
             * Else the POI at the default POI index is activated.
             */
            public int DefaultPOIIndex
            {
                get
                {
                    return m_POI.Count == 0 ? -1 : 0;
                }
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

            #endregion

            #region Model Layer management

            //! Gets a handle to the list of model layers.
            public IList<GameObject> GetModelLayers()
            {
                return m_modelLayers;
            }

            //! Unhides all model layers.
            public void UnhideAllModelLayers()
            {
                foreach (var layer in m_modelLayers)
                {
                    layer.SetActive(true);
                }
            }

            //! Gathers all model layers for the currently active project.
            public void GatherActiveProjectLayers()
            {
                m_modelLayers.Clear();

                var activeProject = ActiveProject;

                if (activeProject == null)
                {
                    return;
                }

                // Gather all POI in the current active project.
                var modelTransform = activeProject.transform.Find("Model");
                var layers = modelTransform.Find("Layers");
                foreach (Transform layerTransform in layers.transform)
                {
                    var layer = layerTransform.gameObject;

                    m_modelLayers.Add(layer);
                }
            }

            #endregion

            //!
            public void ResetTrackingSpacePosition()
            {
                m_ovrCameraRig.transform.position = new Vector3();
                m_ovrCameraRig.transform.rotation = new Quaternion();
            }

            //!
            void UpdateTrackingSpacePosition()
            {
                if (ActiveImmersionMode == null)
                    return;

                ActiveImmersionMode.UpdateTrackingSpacePosition();
            }

            //!
            void UpdateModelLocationAndScale()
            {
                if (ActiveImmersionMode == null)
                    return;

                ActiveImmersionMode.UpdateModelLocationAndScale();
            }

            #region Immersion mode management

            //!
            void ToggleImmersionModeIfNetworkModeAllows()
            {
                var c = new SetImmersionModeCommand();
                c.ImmersionModeIndex = 1 - ActiveImmersionModeIndex;

                switch (NetworkMode)
                {
                    case NetworkMode.Standalone:
                        {
                            QueueCommand(c);
                        }
                        break;
                    case NetworkMode.Server:
                        {
                            Server.BroadcastCommand(c);
                        }
                        break;
                }
            }

            //! TODO: Investigate whether to rename/factor out...
            public bool ToggleImmersionModeIfInputAndNetworkModeAllows()
            {
                // Immersion mode is toggled using I key, Left index trigger.
                bool toggleImmersionMode = m_controllerInput.m_controllerState.button7Down || Input.GetKeyDown(KeyCode.I);

                if (toggleImmersionMode)
                {
                    ToggleImmersionModeIfNetworkModeAllows();
                    return true;
                }

                return false;
            }

            //! Activates an immersion mode, by index.
            public void SetActiveImmersionMode(int immersionModeIndex)
            {
                if (immersionModeIndex == ActiveImmersionModeIndex)
                {
                    return; // Nothing to do.
                }

                var aim = ActiveImmersionMode;

                if (aim != null)
                {
                    aim.Exit();
                }

                ActiveImmersionModeIndex = immersionModeIndex;

                aim = ActiveImmersionMode;

                if (aim != null)
                {
                    aim.Enter();
                }

                UpdateModelLocationAndScale();

                UpdateTrackingSpacePosition();
            }

            #endregion

            //!
            public void Fly()
            {
                #region Compute translation offset vector.

                // Translate Forward/Backward using right thumbstick Y.
                float magnitudeForward = m_controllerInput.m_controllerState.rThumbStick.y;

                // Translate Left/Right using right thumbstick X.
                float magnitudeRight = m_controllerInput.m_controllerState.rThumbStick.x;

                // Translate Up/Down using left thumstick Y.
                float magnitudeUp = m_controllerInput.m_controllerState.lThumbStick.y;

                // First compose translation on the horizontal plane.
                var offsetR = m_centerEyeAnchor.transform.right;
                offsetR.y = 0;
                offsetR.Normalize();
                offsetR *= magnitudeRight;

                var offsetF = m_centerEyeAnchor.transform.forward;
                offsetF.y = 0;
                offsetF.Normalize();
                offsetF *= magnitudeForward;

                var offset = offsetR + offsetF;

                // Clamp offset vector in the horizontal plane to length [0, 1].
                if (offset.magnitude > 1)
                {
                    offset.Normalize();
                }

                // Then add Up/Down translation.
                var offsetUp = magnitudeUp * m_flySpeedUpDown * Vector3.up;

                offset += offsetUp;

                #endregion

                // Translate trasking space.
                if (offset != Vector3.zero)
                {
                    OVRManager.boundary.SetVisible(true);

                    TranslateTrackingSpace(m_flySpeedHorizontal * Time.deltaTime * offset);
                }
            }

            //!
            public void UpdateTrackingSpace()
            {
                if (m_ovrCameraRig == null)
                {
                    return; // We have no handle to the tracking space.
                }

                if (m_centerEyeAnchor == null)
                {
                    return; // We have no handle to the center eye anchor.
                }

                float rotateSpeed = 45.0f;

                float magnitudeRotate = m_controllerInput.m_controllerState.lThumbStick.x;

                var rotateOffset = Time.deltaTime * magnitudeRotate * rotateSpeed;

                var doRotate = (rotateOffset != 0.0f);

                if (doRotate)
                {
                    // Rotate tracking space
                    m_ovrCameraRig.transform.RotateAround(
                        m_centerEyeAnchor.transform.position,
                        Vector3.up,
                        rotateOffset);

                    OVRManager.boundary.SetVisible(true);
                }
            }


            /*! Checks the current input and toggles the active project if necessary.
             *
             * \return 'true' if a new project is activated, 'false' otherwise.
             */
            public bool ToggleActiveProject()
            {
                // Active project is toggled using X/Y button, F1/F2 keys.
                bool activatePrevProject = m_controllerInput.m_controllerState.button3Down || Input.GetKeyDown(KeyCode.F1);

                if (activatePrevProject)
                {
                    SetActiveProject(ActiveProjectIndex - 1);
                    return true;
                }

                bool activateNextProject = m_controllerInput.m_controllerState.button4Down || Input.GetKeyDown(KeyCode.F2);

                if (activateNextProject)
                {
                    SetActiveProject(ActiveProjectIndex + 1);
                    return true;
                }

                return false;
            }

            /*! Checks the current input and toggles the active POI if necessary.
             *
             * \return 'true' if a new POI is activated, 'false' otherwise.
             */
            public bool ToggleActivePOI()
            {
                // Active project is toggled using X/Y button, F1/F2 keys.
                bool activatePrev = m_controllerInput.m_controllerState.button1Down || Input.GetKeyDown(KeyCode.F3);

                if (activatePrev)
                {
                    TeleportToPOIInActiveProject(ActivePOIIndex - 1);
                    return true;
                }

                bool activateNext = m_controllerInput.m_controllerState.button2Down || Input.GetKeyDown(KeyCode.F4);

                if (activateNext)
                {
                    TeleportToPOIInActiveProject(ActivePOIIndex + 1);
                    return true;
                }

                return false;
            }

            //! Activates a POI, by name.
            void SetActivePOI(string newPOIName)
            {
                // Get the POI index by POI name.
                var newPOIIndex = GetPOIIndex(newPOIName);

                if (newPOIIndex == -1)
                    if (m_POI.Count > 0)
                        newPOIIndex = 0;

                ActivePOIIndex = newPOIIndex;
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

            //! Translates the tracking space wby the given offset vector.
            void TranslateTrackingSpace(Vector3 offset)
            {
                m_ovrCameraRig.transform.position = m_ovrCameraRig.transform.position + offset;
            }

            #region HUD menu

            //! Activates the next menu mode.
            void ToggleMenuMode()
            {
                if (menuMode == MenuMode.None)
                {
                    HudMenu.UpdateAnchoring(); // Re-anchor the HUD menu to be in front of cam.
                }

                menuMode = (MenuMode)UtilIterate.MakeCycle((int)menuMode + 1, 0, menus.Count);

                switch (menuMode)
                {
                    case MenuMode.DebugInput:
                        SetActiveMenu(debugInputMenuPanel);
                        break;
                    case MenuMode.DebugLog:
                        SetActiveMenu(debugLogMenuPanel);
                        break;
                    case MenuMode.Graphics:
                        SetActiveMenu(graphicsMenuPanel);
                        break;
                    case MenuMode.Network:
                        SetActiveMenu(networkMenuPanel);
                        break;
                    case MenuMode.Info:
                        SetActiveMenu(infoMenuPanel);
                        break;
                    case MenuMode.None:
                        SetActiveMenu(null);
                        break;
                    default:
                        WM.Logger.Warning("ApplicationArchiVR.ToggleMenuMode(): Unsupported menu mode: " + menuMode.ToString());
                        break;
                }
            }

            //! Sets the avatar for the local player.
            public void SetAvatar(int avatarIndex)
            {
                WM.Logger.Debug("SetAvatar(" + avatarIndex.ToString() + ")");

                if (NetworkMode == NetworkMode.Standalone)
                {
                    WM.Logger.Warning("Network mode should not be 'Standalone'!");
                    return;
                }

                AvatarIndex = avatarIndex;
                Client.SendCommand(new SetClientAvatarCommand(WM.Net.NetUtil.GetLocalIPAddress(), avatarIndex));
            }

            #endregion

            public void TeleportToPOIInActiveProjectAtIndexOffset(int indexOffset)
            {
                TeleportToPOIInActiveProject(ActivePOIIndex + indexOffset);
            }

            #region Teleportation fading animation callbacks

            public void OnTeleportFadeOutComplete()
            {
                Logger.Debug("ApplicationArchiVR::OnTeleportFadeInComplete()");

                m_fadeAnimator.ResetTrigger("FadeOut");

                var applicationState = GetActiveApplicationState();
                if (applicationState != null)
                {
                    applicationState.OnTeleportFadeOutComplete();
                }
            }

            public void OnTeleportFadeInComplete()
            {
                Logger.Debug("ApplicationArchiVR::OnTeleportFadeInComplete()");

                m_fadeAnimator.ResetTrigger("FadeIn");

                var applicationState = GetActiveApplicationState();
                if (applicationState != null)
                {
                    applicationState.OnTeleportFadeInComplete();
                }
            }

            #endregion

            void TeleportToPOIInActiveProject(int newPOIIndex)
            {
                // Determine the new POI index.
                if (m_POI.Count == 0)
                {
                    newPOIIndex = -1;
                }
                else
                {
                    newPOIIndex = (newPOIIndex) % m_POI.Count;

                    while (newPOIIndex < 0)
                    {
                        newPOIIndex += m_POI.Count;
                    }
                }

                var tc = new TeleportCommand();

                tc.ProjectIndex = ActiveProjectIndex;

                tc.POIName = newPOIIndex == -1 ? null : m_POI[newPOIIndex].name;

                Teleport(tc);
            }

            private void Teleport(TeleportCommand teleportCommand)
            {
                switch (NetworkMode)
                {
                    case NetworkMode.Server:
                        Server.BroadcastCommand(teleportCommand);
                        break;
                    case NetworkMode.Client:
                        // NOOP: server has control...
                        break;
                    case NetworkMode.Standalone:
                        QueueCommand(teleportCommand);
                        break;
                }
            }

            public IEnumerator Teleport()
            {
                Logger.Debug("ApplicationArchiVR::Teleport()");

                if (TeleportCommand != null)
                {
                    if (ActiveProjectIndex != TeleportCommand.ProjectIndex) // If project changed...
                    {
                        // Needs to be cached before activating the new project.
                        var oldProjectName = ActiveProjectName;

                        // Option A: first unload, then load...
                        // Unload the old project
                        if (oldProjectName != null)
                        {
                            Logger.Debug("Unloading project '" + oldProjectName + "'");

                            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(oldProjectName);

                            // Wait until asynchronous unloading the old project finishes.
                            while (!asyncUnload.isDone)
                            {
                                yield return null;
                            }
                        }

                        // Load the new projct
                        var newProjectName = GetProjectName(TeleportCommand.ProjectIndex);

                        Logger.Debug("Loading project '" + newProjectName + "'");

                        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(newProjectName, LoadSceneMode.Additive);

                        // Wait until asynchronous loading the new project finishes.
                        while (!asyncLoad.isDone)
                        {
                            yield return null;
                        }

                        // Update active project index to point to newly activated project.
                        ActiveProjectIndex = TeleportCommand.ProjectIndex;

                        // Update left controller UI displaying the project name.
                        m_leftControllerText.text = (ActiveProjectName != null) ? GetProjectNameShort(ActiveProjectName) : "No project loaded.";

                        // Option B: first load, then unload...
                        //// Unload the old project
                        //if (oldProjectName != null)
                        //{
                        //    Logger.Debug("Unloading project '" + oldProjectName + "'");

                        //    AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(oldProjectName);

                        //    // Wait until asynchronous unloading the old project finishes.
                        //    while (!asyncUnload.isDone)
                        //    {
                        //        yield return null;
                        //    }
                        //}
                    }

                    // Gather the POI from the new project.
                    GatherActiveProjectPOI();

                    // Gather the layers from the new project.
                    GatherActiveProjectLayers();

                    SetActivePOI(TeleportCommand.POIName);

                    TeleportCommand = null;

                    ActiveImmersionMode.UpdateTrackingSpacePosition();

                    m_fadeAnimator.SetTrigger("FadeIn");
                }

                //! Get the short-format (excluding prefix 'Project') project name for the given project name.
                string GetProjectNameShort(string projectName)
                {
                    string prefix = "project";

                    if (projectName.ToLower().StartsWith(prefix))
                    {
                        return projectName.Substring(prefix.Length);
                    }
                    else
                    {
                        return projectName;
                    }
                }
            }
        }
    } // namespace ArchiVR
} // namespace WM