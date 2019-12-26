using ControllerSelection;
using System;
using System.Collections.Generic;
using UnityEngine;
using WM.Command;
using WM.Net;
using WM.UI;
using WM.VR;

namespace WM.Application
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITeleportationSystem
    {
        bool NeedFadeOut
        {
            get;
        }
    }

    /// <summary>
    /// Base class for the logic of a multiplayer network-capable VR application.
    /// 
    /// Provides the following functionality:
    /// - Client-Server based network multiplay
    /// - Avatar management: 1 Local, N Remote
    /// - Command management
    /// - Application state
    /// - ???
    /// </summary>
    public class UnityApplication : MonoBehaviour
    {
        #region Variables

        #region Startup options

        //! The startup network mode. (Default: Standalone)
        public NetworkMode StartupNetworkMode = NetworkMode.Standalone;

        //! FPS UI visibility. (Default: false)
        public bool StartupShowFps = false;

        // Menu mode (Default: None)
        public MenuMode StartupMenuMode = MenuMode.None;

        #endregion

        //! The command queue.
        public List<ICommand> CommandQueue = new List<ICommand>();

        // The application version.
        public string Version = "";

        // Whether to show the GFX quality level and FPS as HUD UI.
        private bool enableDebugGFX = false;

        #region Network

        //! The current network mode.
        public NetworkMode NetworkMode = NetworkMode.Standalone; // TODO: make private...

        // The multiplayer server.
        public Server Server;

        // The multiplayer client.
        public Client Client;

        //! The avatar for the local player.
        public Guid AvatarID = Guid.Empty;

        public int AvatarIndex
        {
            get
            {
                return 0; ; // FIXME: TODO: Get Avatar index from avatar id guid!
            }
        }

        /// <summary>
        /// The remote users.
        /// </summary>
        /*private*/
        public Dictionary<string, RemoteUser> remoteUsers = new Dictionary<string, RemoteUser>();

        #region Shared Tracking space

        public bool SharedTrackingSpace = false;

        public GameObject SharedTrackingSpaceReference;

        #endregion

        #endregion

        #region Game objects

        /// <summary>
        /// The list of avatar Prefabs.
        /// Drag Prefabs into this field in the Inspector.
        /// </summary>
        public List<GameObject> avatarPrefabs = new List<GameObject>();

        public Animator m_fadeAnimator;

        public HUDMenu HudMenu;

        public GameObject FpsPanelHUD;

        public GameObject m_ovrCameraRig;

        public GameObject m_centerEyeAnchor;

        public GameObject m_leftHandAnchor;

        public GameObject m_rightHandAnchor;

        public OVRPointerVisualizer SelectionVisualizer;

        #endregion

        #region Application state

        // The application states enumeration. // FIXME: Factor out! UnityApplication should not/cannot have an exhaustive enumeration of possible application states!
        public enum ApplicationStates : int
        {
            None = -1,
            Default,
            Teleporting
        };
                
        // The generic list of application states.
        protected List<ApplicationState> m_applicationStates = new List<ApplicationState>();

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

        #endregion Application state

        #region Controller UI

        #region Button mapping UI

        public ButtonMappingUI leftControllerButtonMapping = null;
        public ButtonMappingUI rightControllerButtonMapping = null;

        #endregion

        #region Pick Ray

        // Right controller pick ray.
        public PickRay RPickRay = null;

        // Right controller pick ray.
        public PickRay LPickRay = null;

        #endregion

        #endregion

        #region HUD menu

        public enum MenuMode
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

        #region Pick selection

        /// <summary>
        /// The list of all selection targets.
        /// </summary>        
        private List<GameObject> selectionTargets = new List<GameObject>();

        /// <summary>
        /// 
        /// </summary>
        private void UpdateSelectionVisualizerVisibility()
        {
            WM.Logger.Debug("UpdateSelectionVisualizerVisibility() -> " + HasSelectionTargets());

            if (SelectionVisualizer == null)
            {
                return;
            }

            SelectionVisualizer.gameObject.SetActive(HasSelectionTargets());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool HasSelectionTargets()
        {
            return selectionTargets.Count != 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectionTarget"></param>
        public void AddSelectionTarget(GameObject selectionTarget)
        {
            //WM.Logger.Warning("AddSelectionTarget(" + selectionTarget.name + ")");

            selectionTargets.Add(selectionTarget);

            UpdateSelectionVisualizerVisibility();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectionTarget"></param>
        public void RemoveSelectionTarget(GameObject selectionTarget)
        {
            //WM.Logger.Warning("RemoveSelectionTarget(" + selectionTarget.name + ")");

            selectionTargets.Remove(selectionTarget);

            UpdateSelectionVisualizerVisibility();
        }

        #endregion Pick selection

        #endregion Variables

        #region GameObject overrides

        public void OnEnable()
        {
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
            WM.Logger.Debug("Application version: " + Version);

            #endregion
        }

        //! Start is called before the first frame update
        public void Start()
        {
            Init();
        }

        public virtual void Init()
        {
            AvatarID = DefaultAvatarID;

            #region Get handles to game objects

            if (m_ovrCameraRig == null)
                m_ovrCameraRig = UtilUnity.TryFindGameObject("OVRCameraRig");

            if (m_centerEyeAnchor == null)
                m_centerEyeAnchor = UtilUnity.TryFindGameObject("CenterEyeAnchor");

            if (m_leftHandAnchor == null)
                m_leftHandAnchor = UtilUnity.TryFindGameObject("LeftHandAnchor");

            if (m_rightHandAnchor == null)
                m_rightHandAnchor = UtilUnity.TryFindGameObject("RightHandAnchor");

            if (m_centerEyeCanvas == null)
            {
                m_centerEyeCanvas = UtilUnity.TryFindGameObject("CenterEyeCanvas");
            }

            if (debugInputMenuPanel == null)
            {
                debugInputMenuPanel = UtilUnity.TryFindGameObject("DebugInputMenuPanel");
            }

            if (debugInputMenuPanel != null)
            {
                menus.Add(debugInputMenuPanel);
            }

            if (debugLogMenuPanel == null)
            {
                debugLogMenuPanel = UtilUnity.TryFindGameObject("DebugLogMenuPanel");
            }

            if (debugLogMenuPanel != null)
            {
                menus.Add(debugLogMenuPanel);
            }

            if (graphicsMenuPanel == null)
            {
                graphicsMenuPanel = UtilUnity.TryFindGameObject("GraphicsMenuPanel");
            }

            if (graphicsMenuPanel != null)
            {
                menus.Add(graphicsMenuPanel);
            }

            if (networkMenuPanel == null)
            {
                networkMenuPanel = UtilUnity.TryFindGameObject("NetworkMenuPanel");
            }

            if (networkMenuPanel != null)
            {
                menus.Add(networkMenuPanel);
            }

            if (infoMenuPanel == null)
            {
                infoMenuPanel = UtilUnity.TryFindGameObject("InfoMenuPanel");
            }

            if (infoMenuPanel != null)
            {
                menus.Add(infoMenuPanel);
            }

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
            {
                var LPickRayGameObject = UtilUnity.TryFindGameObject("L PickRay");

                if (LPickRayGameObject != null)
                {
                    LPickRay = LPickRayGameObject.GetComponent<PickRay>();
                }
            }

            m_leftControllerCanvas = UtilUnity.TryFindGameObject("LeftControllerCanvas");
            m_leftControllerPanel = UtilUnity.TryFindGameObject("LeftControllerPanel");
            m_leftControllerText = UtilUnity.TryFindGameObject("LeftControllerText").GetComponent<UnityEngine.UI.Text>();

            // Right controller.

            // Pick ray.
            {
                var RPickRayGameObject = UtilUnity.TryFindGameObject("R PickRay");

                if (RPickRayGameObject != null)
                {
                    RPickRay = RPickRayGameObject.GetComponent<PickRay>();
                }
            }

            m_rightControllerCanvas = UtilUnity.TryFindGameObject("RightControllerCanvas");
            m_rightControllerPanel = UtilUnity.TryFindGameObject("RightControllerPanel");
            m_rightControllerText = UtilUnity.TryFindGameObject("RightControllerText").GetComponent<UnityEngine.UI.Text>();

            #endregion

            // Disable all pickrays.
            if (LPickRay != null)
            {
                LPickRay.gameObject.SetActive(false);
            }

            if (RPickRay != null)
            {
                RPickRay.gameObject.SetActive(false);
            }

            #region Init application states.

            foreach (var applicationState in m_applicationStates)
            {
                applicationState.m_application = this;
                applicationState.Init();
            }

            #endregion

            UpdateSelectionVisualizerVisibility();

            if (UnityEngine.Application.isEditor)
            {
                if (HudMenu != null)
                {
                    HudMenu.AnchorEnabled = true;
                }
            }

            SetMenuMode(StartupMenuMode);

            new InitNetworkCommand(StartupNetworkMode).Execute(this);
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


        protected Vector3 m_centerEyeAnchorPrev = new Vector3();

        protected int frame = 0;

        protected bool CanProcessCommands
        {
            get { return true; }
        }

        /// <summary>
        /// Update is called once per frame.
        /// 
        /// Made 'public' for unit testing.
        /// </summary>
        public void Update()
        {
            // TODO: WHY THAF is this necessary to make camera work in Editor?
            m_centerEyeAnchor.GetComponent<Camera>().enabled = false;
            m_centerEyeAnchor.GetComponent<Camera>().enabled = true;

            UpdateControllersLocation();

            if (CanProcessCommands)
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
                UpdateNetwork();
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

            #region Update controller state.

            m_controllerInput.Update();

            #endregion

            #region Update Button Mapping UI to current controller state.

            if (leftControllerButtonMapping != null)
            {
                leftControllerButtonMapping.SetControllerState(m_controllerInput.m_controllerState);
            }

            if (rightControllerButtonMapping != null)
            {
                rightControllerButtonMapping.SetControllerState(m_controllerInput.m_controllerState);
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

        #region
        
        //!
        public void ResetTrackingSpacePosition()
        {
            m_ovrCameraRig.transform.position = new Vector3();
            m_ovrCameraRig.transform.rotation = new Quaternion();
        }

        #endregion

        //!
        public void Fly()
        {
            if (SharedTrackingSpace == true)
            {
                Logger.Warning("Unsupported SharedTrackingSpace == true: movement will be impossible in editor mode clients!");
            }

            if ((NetworkMode == NetworkMode.Client) && (SharedTrackingSpace == true))
            {
                return; // Only server can manipulate tracking space!
            }

            #region Compute translation offset vector.

            var controllerState = m_controllerInput.m_controllerState;

            // Translate Forward/Backward using right thumbstick Y.
            float magnitudeForward = controllerState.rThumbStick.y;

            // Translate Left/Right using right thumbstick X.
            float magnitudeRight = controllerState.rThumbStick.x;

            // Compose translation on the horizontal plane.
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

            #endregion

            // Translate tracking space.
            if (offset != Vector3.zero)
            {
                OVRManager.boundary.SetVisible(true);

                TranslateTrackingSpace(m_flySpeedHorizontal * Time.deltaTime * offset);
            }
        }

        /// <summary>
        /// How are we manipulating the tracking space?
        /// - Rotate around local reference and vertical axis
        /// - Translate up/down along vertical axis
        /// </summary>
        private enum TrackingSpaceManipulationMode
        {
            None = 0,
            Rotate,
            TranslateUpDown
        };

        protected virtual void UpdateNetwork()
        {
            /*
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
            ¨*/
        }

        /// <summary>
        /// 
        /// </summary>
        private TrackingSpaceManipulationMode trackingSpaceManipulationMode = TrackingSpaceManipulationMode.None;

        //! Whether or not translating tracking space up/down is enabled (default: false)
        public bool EnableTrackingSpaceTranslationUpDown = false;


        /// <summary>
        /// 
        /// </summary>
        public void UpdateTrackingSpace()
        {
            if ((NetworkMode == NetworkMode.Client) && (SharedTrackingSpace == true))
            {
                return; // Only server can manipulate tracking space!
            }

            if (m_ovrCameraRig == null)
            {
                return; // We have no handle to the tracking space.
            }

            if (m_centerEyeAnchor == null)
            {
                return; // We have no handle to the center eye anchor.
            }

            float rotateSpeed = 45.0f;

            var controllerState = m_controllerInput.m_controllerState;

            // Rotate Left/Right using left thumbstick X.
            float magnitudeRotate = controllerState.lThumbStick.x;

            // Translate Up/Down using left thumbstick Y.
            float magnitudeUp = EnableTrackingSpaceTranslationUpDown ? controllerState.lThumbStick.y : 0.0f;

            // Update maquette manipulationMode
            bool manipulating = (Mathf.Abs(magnitudeRotate) > 0.1f) || (Mathf.Abs(magnitudeUp) > 0.1f);
            if (trackingSpaceManipulationMode == TrackingSpaceManipulationMode.None)
            {
                if (manipulating)
                {
                    trackingSpaceManipulationMode = (Mathf.Abs(magnitudeRotate) > Mathf.Abs(magnitudeUp))
                        ? TrackingSpaceManipulationMode.Rotate
                        : TrackingSpaceManipulationMode.TranslateUpDown;
                }
                else
                    trackingSpaceManipulationMode = TrackingSpaceManipulationMode.None;
            }
            else
            {
                if (!manipulating)
                {
                    trackingSpaceManipulationMode = TrackingSpaceManipulationMode.None;
                }
            }

            switch (trackingSpaceManipulationMode)
            {
                case TrackingSpaceManipulationMode.TranslateUpDown:
                    {
                        var offsetUp = magnitudeUp * m_flySpeedUpDown * Time.deltaTime * Vector3.up;

                        OVRManager.boundary.SetVisible(true);

                        TranslateTrackingSpace(offsetUp);
                    }
                    break;
                case TrackingSpaceManipulationMode.Rotate:
                    {
                        // Rotate around 'up' vector.
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
                    break;
            }
        }

        //! Updates the location of the controllers.
        //  When running in VR -> NOOP.
        //  When running in editor -> anchors the controllers at a fixed offset in front of the center eye.
        void UpdateControllersLocation()
        {
            if (!UnityEngine.Application.isEditor)
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

            if (SelectionVisualizer != null)
            {
                SelectionVisualizer.EditorRay = RPickRay.GetRay();
            }
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
            var newMenuMode = (MenuMode)UtilIterate.MakeCycle((int)menuMode + 1, 0, menus.Count);

            SetMenuMode(newMenuMode);
        }

        void SetMenuMode(MenuMode newMenuMode)
        {
            if (menuMode == MenuMode.None)
            {
                if (HudMenu != null)
                {
                    HudMenu.UpdateAnchoring(); // Re-anchor the HUD menu to be in front of cam, when leaving None mode.
                }
            }

            menuMode = newMenuMode;

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

            if (HudMenu != null)
            {
                HudMenu.gameObject.SetActive(menuMode != MenuMode.None);
            }
        }

        public Guid GetAvatarID(int avatarIndex)
        {
            return DefaultAvatarID; // FIXME: TODO: Convert avatar index to avatar ID Guid!
        }

        /// <summary>
        /// Sets the avatar for the local player.
        /// </summary>
        /// <param name="avatarID"></param>
        public void SetAvatar(int avatarIndex)
        {
            WM.Logger.Debug("SetAvatar(" + avatarIndex + ")");

            var avatarID = GetAvatarID(avatarIndex);
            SetAvatar(avatarID);
        }

        /// <summary>
        /// Sets the avatar for the local player.
        /// </summary>
        /// <param name="avatarID"></param>
        public void SetAvatar(Guid avatarID)
        {
            WM.Logger.Debug("SetAvatar(" + avatarID.ToString() + ")");

            if (NetworkMode == NetworkMode.Standalone)
            {
                WM.Logger.Warning("Network mode should not be 'Standalone'!");
                return;
            }

            AvatarID = avatarID;
            Client.SendCommand(new SetClientAvatarCommand(WM.Net.NetUtil.GetLocalIPAddress(), Client.TcpPort, avatarID));
        }

        #endregion

        #region Avatar management

        public PrefabGameObjectFactory AvatarFactory = new PrefabGameObjectFactory();

        /// <summary>
        /// Instanciates the avatar prefabe at given index, and returns a reference to the avatar instance.
        /// </summary>
        /// <param name="avatarID"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns>A reference to the created avatar instance.</returns>
        GameObject InstantiateAvatarPrefab(
            Guid avatarID,
            Vector3 position,
            Quaternion rotation)
        {
            return AvatarFactory.Create(
                    avatarID,
                    position,
                    rotation);
        }

        /// <summary>
        /// Instantiates all available avatar prefabs.
        /// </summary>
        void InstanciateAllAvatarPrefabs()
        {
            float x = -3;

            foreach (var avatarPrefab in avatarPrefabs)
            {
                Instantiate(
                    avatarPrefab,
                    new Vector3(x, 0, 0),
                    Quaternion.identity);

                x += 2;
            }
        }

        #endregion

        #region Remote user management

        public Guid DefaultAvatarID
        {
            get;
            set;
        }

        /// <summary>
        /// Does whatever needs doing upon new client connected event.
        /// - Creates a representation for a newly connected user.
        /// </summary>
        /// <param name="clientIP"></param>
        /// <param name="clientPort"></param>
        public void ConnectClient(
            string clientIP,
            int clientPort)
        {
            WM.Logger.Debug(string.Format(name + ":ConnectClient({0}:{1})", clientIP, clientPort));

            lock (remoteUsers)
            {
                // TODO first: move avatar instance management (creation, init, destruction) into RemoteUser.Init(avatarIndex, pos, rot)?
                var avatar = InstantiateAvatarPrefab(
                    DefaultAvatarID,
                    Vector3.zero,
                    Quaternion.identity);

                var remoteUser = new RemoteUser();
                remoteUser.remoteIP = clientIP;
                remoteUser.remotePort = clientPort;
                remoteUser.Avatar = avatar.GetComponent<WM.Net.Avatar>();

                var clientID = remoteUser.remoteIP + ":" + remoteUser.remotePort;
                remoteUsers[clientID] = remoteUser;

                WM.Logger.Debug("Client " + clientID + " accepted");
            }
        }

        /// <summary>
        /// Destroys the avatar prefab for the given disconnected client.
        /// </summary>
        /// <param name="clientID"></param>
        public void DisconnectClient(string clientID)
        {
            lock (remoteUsers)
            {
                if (remoteUsers.ContainsKey(clientID))
                {
                    // We need to destroy ojects defferently in Edit Mode, otherwise Edit Mode Unit Tests complain.  :-(
                    if (UnityEngine.Application.isEditor)
                    {
                        DestroyImmediate(remoteUsers[clientID].Avatar.gameObject);
                    }
                    else
                    {
                        Destroy(remoteUsers[clientID].Avatar.gameObject);
                    }

                    remoteUsers.Remove(clientID);
                }
            }
        }

        /// <summary>
        /// Updates the avatar type for the connected client with given ID.
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="avatarID"></param>
        public void SetClientAvatar(
            string clientID,
            Guid avatarID)
        {
            lock (remoteUsers)
            {
                var remoteUser = (remoteUsers.ContainsKey(clientID) ? remoteUsers[clientID] : null);

                if (remoteUser == null)
                {
                    WM.Logger.Warning("SetClientAvatar(): User '" + clientID + "' not found!");
                    return;
                }

                var oldAvatar = remoteUser.Avatar;

                var avatar = InstantiateAvatarPrefab(
                    avatarID,
                    oldAvatar.transform.position,
                    oldAvatar.transform.rotation);

                remoteUser.Avatar = avatar.GetComponent<WM.Net.Avatar>();

                if (oldAvatar != null)
                {
                    // We need to destroy ojects defferently in Edit Mode, otherwise Edit Mode Unit Tests complain.  :-(
                    if (UnityEngine.Application.isEditor)
                        DestroyImmediate(oldAvatar.gameObject);
                    else
                        Destroy(oldAvatar.gameObject);
                }
            }
        }

        #endregion
    }
}
