using ControllerSelection;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        void Teleport(TeleportCommand command);
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
    public abstract class UnityApplication
        : MonoBehaviour
        , IMessageProcessor
    {
        #region Variables

        public static bool EnableLoggerAtStartup = false;

        #region UnitTestModeEnabled

        /// <summary>
        /// In unit test mode, some resources (eg menus, OVRManager,...) will not be initialized.
        /// </summary>
        public static bool UnitTestModeEnabled
        {
            get;
            set;
        } = false;

        #endregion UnitTestModeEnabled

        /// <summary>
        /// The logger.
        /// </summary>
        public readonly Logger Logger = new Logger();

        #region Startup options

        //! The startup network mode. (Default: Standalone)
        public NetworkMode StartupNetworkMode = NetworkMode.Standalone;

        //! FPS UI visibility. (Default: false)
        public bool StartupShowFps = false;

        // Menu mode (Default: None)
        public MenuMode StartupMenuMode = MenuMode.None;

        #endregion

        /// <summary>
        /// The application version.
        /// </summary>
        abstract public string Name
        {
            get;
        }

        /// <summary>
        /// The application version.
        /// </summary>
        public string Version = "";

        /// <summary>
        /// Whether to show the GFX quality level and FPS as HUD UI.
        /// </summary>
        private bool enableDebugGFX = false;

        /// <summary>
        /// 
        /// </summary>
        protected Vector3 m_centerEyeAnchorPrev = new Vector3();

        /// <summary>
        /// 
        /// </summary>
        protected int frame = 0;

        #region Network

        /// <summary>
        /// The current network mode.
        /// </summary>
        public NetworkMode NetworkMode = NetworkMode.Standalone; // TODO: make private...

        /// <summary>
        /// The networking server.
        /// </summary>
        public Server Server;

        /// <summary>
        /// The networking client.
        /// </summary>
        public Client Client;

        /// <summary>
        /// The networking server discovery.
        /// </summary>
        public readonly ServerDiscovery ServerDiscovery = new ServerDiscovery();

        public int AvatarIndex
        {
            get
            {
                return 0; // FIXME: TODO: Get Avatar index from Player.AvatarID!
            }
        }

        /// <summary>
        /// The players from remote connected Clients.
        /// </summary>
        /*private*/
        public Dictionary<Guid, Player> Players = new Dictionary<Guid, Player>();

        #region Shared Tracking space

        public bool SharedTrackingSpace = false;

        public GameObject SharedTrackingSpaceReference;

        #endregion

        #endregion

        #region Game objects

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
            Logger.Debug("UpdateSelectionVisualizerVisibility() -> " + HasSelectionTargets());

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
            //Logger.Warning("AddSelectionTarget(" + selectionTarget.name + ")");

            selectionTargets.Add(selectionTarget);

            UpdateSelectionVisualizerVisibility();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectionTarget"></param>
        public void RemoveSelectionTarget(GameObject selectionTarget)
        {
            //Logger.Warning("RemoveSelectionTarget(" + selectionTarget.name + ")");

            selectionTargets.Remove(selectionTarget);

            UpdateSelectionVisualizerVisibility();
        }

        #endregion Pick selection

        /// <summary>
        /// 
        /// </summary>
        public ITeleportationSystem TeleportationSystem { get; set; }

        #region Command processing

        /// <summary>
        /// The locking object for the command queue.
        /// 
        /// Everyone that wants to use the command queue, should lock() on this object while doing so.
        /// </summary>
        private object commandQueueLock = new object();

        /// <summary>
        /// The command queue.
        /// </summary>
        public List<ICommand> CommandQueue = new List<ICommand>();


        /// <summary>
        /// Queue a command to be executed async.
        /// </summary>
        /// <param name="command"></param>
        public void QueueCommand(ICommand command)
        {
            lock (commandQueueLock)
            {
                CommandQueue.Add(command);
            }
        }

        /// <summary>
        /// While teleporting: reference to the ongoing teleport command.
        /// Else: null
        /// </summary>
        public TeleportCommand TeleportCommand { get; set; } // TODO: find a better place for this (TeleportationSystemBase perhaps?)

        /// <summary>
        /// To be implemented by concrete applications, in order to prevent command processing when needed.
        /// </summary>
        protected bool CanProcessCommands
        {
            get
            {
                bool canProcessCommands = true; // We can always process commands.

                return canProcessCommands;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teleportCommand"></param>
        protected void Teleport(TeleportCommand teleportCommand)
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

        #endregion Command processing

        #endregion Variables

        #region Public API

        /// <summary>
        /// A large spatial offset, based on the application ID.
        /// Used as a (temporary?) workaround by the WM TestApp, in order to achieve the following:
        /// Have only the application scene associated to an appplication instance visible in that application instance's camera.
        /// (The scene content of other application instances will be culled away because it is dislocated by this large spatial offset.)
        /// To be removed as soon as we can do proper per-camera culling of scenes or parts-of-scenes.
        /// </summary>
        public Vector3 OffsetPerID
        {
            get
            {
                return Vector3.right * 10000 * ID;
            }
        }

        /// <summary>
        /// The application ID
        /// 
        /// Note: Used while debugging Unity Apps using the WM test application, in order to identify application instances from each other.
        /// </summary>
        public int ID
        {
            get;
            set;
        } = 0;

        private bool _enableInput = true;

        /// <summary>
        /// Whether the application should react on user input (KB/Mouse/VR or ART tracking sensors...)
        /// 
        /// Note: Used by the test application, to make only the 'active application instance' react on user input.
        /// </summary>
        public bool EnableInput
        {
            get
            {
                return _enableInput;
            }
            set
            {
                if (_enableInput == value)
                {
                    return;
                }

                _enableInput = value;

                if (!EnableInput)
                {
                    m_controllerInput.Reset();
                }

                OnEnableInputChanged();
            }
        }

        /// <summary>
        /// To be implemented by concrete UnityApplication implementations.
        /// Called whenever EnableInput has changed.
        /// All actions necessary to reactivate/deactivate all sorts of user input need to be performed here.
        /// </summary>
        abstract protected void OnEnableInputChanged();

        /// <summary>
        /// 
        /// </summary>
        public virtual void Init()
        {
            Logger.Enabled = EnableLoggerAtStartup;

            Player.AvatarID = DefaultAvatarID;
            Player.ClientID = Client == null ? new Guid() : Client.ID;

            ServerDiscovery.SetLog(Logger);

            if (Server != null)
            {
                Server.Log = Logger;
            }

            if (Client != null)
            {
                Client.Log = Logger;
                Client.MessageProcessor = this;
            }

            #region Get handles to game objects

            #region Get handles to OVRGameraRig game objects

            if (m_ovrCameraRig == null)
            {
                m_ovrCameraRig = UtilUnity.TryFindGameObject("OVRCameraRig");
            }

            if (m_centerEyeAnchor == null)
            {
                m_centerEyeAnchor = UtilUnity.TryFindGameObject("CenterEyeAnchor");
            }

            if (m_leftHandAnchor == null)
            {
                m_leftHandAnchor = UtilUnity.TryFindGameObject("LeftHandAnchor");
            }

            if (m_rightHandAnchor == null)
            {
                m_rightHandAnchor = UtilUnity.TryFindGameObject("RightHandAnchor");
            }

            #endregion Get handles to OVRGameraRig objects

            if (m_centerEyeCanvas == null)
            {
                m_centerEyeCanvas = UtilUnity.TryFindGameObject("CenterEyeCanvas");
            }

            if (!UnitTestModeEnabled)
            {
                #region Get handles to Menu game objects

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

                #endregion Get handles to Menu game objects

                if (FpsPanelHUD == null)
                {
                    FpsPanelHUD = UtilUnity.TryFindGameObject("FPSPanel");
                }

                if (FpsPanelHUD != null)
                {
                    FpsPanelHUD.SetActive(StartupShowFps);
                }
            }

            // Left controller.

            // Pick ray.
            if (!UnitTestModeEnabled)
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
            if (!UnitTestModeEnabled)
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

        /// <summary>
        /// 
        /// </summary>
        public GameObject ActiveMenu { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public void ResetTrackingSpacePosition()
        {
            m_ovrCameraRig.transform.position = new Vector3() + OffsetPerID;
            m_ovrCameraRig.transform.rotation = new Quaternion();
        }

        /// <summary>
        /// 
        /// </summary>
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

        #region GameObject overrides

        /// <summary>
        /// 
        /// </summary>
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
            Logger.Debug("Application version: " + Version);

            #endregion
        }

        /// <summary>
        /// Start is called before the first frame update.
        /// </summary>
        public void Start()
        {
            Init();
        }

        /// <summary>
        /// Update is called once per frame.
        /// 
        /// Note: Made 'public' for unit testing.
        /// </summary>
        public void Update()
        {
            // TODO: WHY THAF is this necessary to make camera work in Editor?
            var camera = m_centerEyeAnchor.GetComponent<Camera>();
            if (camera != null)
            {
                camera.enabled = false;
                camera.enabled = true;
            }

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

            if (EnableInput)
            {
                m_controllerInput.Update();
            }

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
            bool toggleMenu = m_controllerInput.m_controllerState.buttonStartDown;

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

        #endregion GameObject overrides

        #endregion Public API

        /// <summary>
        /// Process the given message (that came in via the network).
        /// </summary>
        /// <param name="message">The message to process.</param>
        void IMessageProcessor.Process(object message)
        {
            // If it's a command, queue it.
            if (message is ICommand command)
            {
                QueueCommand(command);
            }
        }

        #region Player Management

        /// <summary>
        /// 
        /// </summary>
        public Player Player
        {
            get;
        } = new Player();

        #region Player Name

        /// <summary>
        /// Sets the name of a remote player.
        /// </summary>
        /// <param name="name"></param>
        public void SetPlayerName(
            Guid playerID,
            string playerName)
        {
            Logger.Debug("SetPlayerName(" + playerID + ", " + name + ")");

            // Targeted player should be known by the application!
            Debug.Assert(Players.ContainsKey(playerID));

            lock (Players)
            {
                if (Players.ContainsKey(playerID))
                {
                    var player = Players[playerID];
                    player.Name = playerName;
                }
            }
        }

        /// <summary>
        /// Sets the name of the local player.
        /// </summary>
        /// <param name="name"></param>
        public void SetPlayerName(string name)
        {
            Logger.Debug("SetPlayerName(" + name + ")");

            Player.Name = name;

            if (Client.Connected)
            {
                Client.SendCommand(new SetPlayerNameCommand(Player.ID, Player.Name));
            }
        }

        #endregion Player Name

        #region Player Avatar

        /// <summary>
        /// Sets the avatar for the local player.
        /// </summary>
        /// <param name="avatarID"></param>
        public void SetPlayerAvatar(int avatarIndex)
        {
            Logger.Debug("SetAvatar(" + avatarIndex + ")");

            var avatarID = GetAvatarID(avatarIndex);
            SetPlayerAvatar(avatarID);
        }

        /// <summary>
        /// Sets the avatar for the local player.
        /// </summary>
        /// <param name="avatarID"></param>
        public void SetPlayerAvatar(Guid avatarID)
        {
            Logger.Debug("SetAvatar(" + avatarID.ToString() + ")");

            Player.AvatarID = avatarID;

            if (Client.Connected)
            {
                Client.SendCommand(new SetPlayerAvatarCommand(Player.ID, avatarID));
            }
        }

        #endregion Player Avatar

        #endregion Player Management

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activeMenu"></param>
        private void SetActiveMenu(GameObject activeMenu)
        {
            ActiveMenu = activeMenu;

            foreach (var menu in menus)
            {
                menu.SetActive(menu == activeMenu);
            }
        }

        #region
        
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

        /// <summary>
        /// To be implemented by concrete application types.
        /// </summary>
        virtual protected void UpdateNetwork()
        { }

        /// <summary>
        /// 
        /// </summary>
        private TrackingSpaceManipulationMode trackingSpaceManipulationMode = TrackingSpaceManipulationMode.None;

        /// <summary>
        /// Whether or not translating tracking space up/down is enabled (default: false)
        /// </summary>
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
        private void UpdateControllersLocation()
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
        private void TranslateTrackingSpace(Vector3 offset)
        {
            m_ovrCameraRig.transform.position = m_ovrCameraRig.transform.position + offset;
        }

        #endregion

        #region HUD menu

        /// <summary>
        /// Activates the next menu mode.
        /// </summary>
        public void ToggleMenuMode()
        {
            var numMenuModes = menus.Count + 1; // Take into account mode 0: None!
            var newMenuMode = (MenuMode)UtilIterate.MakeCycle((int)menuMode + 1, 0, numMenuModes);

            SetMenuMode(newMenuMode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newMenuMode"></param>
        public void SetMenuMode(MenuMode newMenuMode)
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
                    Logger.Warning("ApplicationArchiVR.ToggleMenuMode(): Unsupported menu mode: " + menuMode.ToString());
                    break;
            }

            if (HudMenu != null)
            {
                HudMenu.gameObject.SetActive(menuMode != MenuMode.None);
            }
        }

        #endregion

        #region Avatar management

        /// <summary>
        /// 
        /// </summary>
        /// <param name="avatarIndex"></param>
        /// <returns></returns>
        public Guid GetAvatarID(int avatarIndex)
        {
            var avatarIDs = AvatarFactory.GetRegisteredIDs();

            return avatarIDs[avatarIndex];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="avatarID"></param>
        /// <returns></returns>
        public int GetAvatarIndex(Guid avatarID)
        {
            var avatarIDs = AvatarFactory.GetRegisteredIDs();

            for (int i = 0; i < avatarIDs.Count; ++i)
            {
                if (avatarIDs[i].Equals(avatarID))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
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

        #endregion

        #region Remote user management

        /// <summary>
        /// 
        /// </summary>
        abstract public Guid DefaultAvatarID
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
        public void AddPlayer(
            Player player)
        {
            Logger.Debug(string.Format(name + ":AddPlayer(Client:{0}, Player:{1}, Name:'{2}')", WM.Net.NetUtil.ShortID(player.ClientID), player.LogID, player.Name));

            lock (Players)
            {
                //Debug.Assert(!Players.ContainsKey(player.ID));

                Players[player.ID] = player;

                SetPlayerAvatar(player.ID, player.AvatarID);
            }
        }

        /// <summary>
        /// Disconnects the Client with given client ID.
        /// </summary>
        /// <param name="clientID"></param>
        public void DisconnectClient(string clientID)
        {
        }

        /// <summary>
        /// Removes the remote Player with given player ID.
        /// </summary>
        /// <param name="clientID"></param>
        public void RemovePlayer(
            Guid playerID)
        {
            lock (Players)
            {
                Debug.Assert(Players.ContainsKey(playerID));
                
                Logger.Debug(string.Format(name + ":RemovePlayer(Player:{0})", Net.NetUtil.ShortID(playerID)));

                if (Players.ContainsKey(playerID))
                {
                    if (playerID != Player.ID)
                    {
                        UtilUnity.Destroy(Players[playerID].Avatar.gameObject);
                    }
                    else
                    {
                        Debug.Assert(Players[playerID].Avatar == null);
                    }

                    Players.Remove(playerID);
                }
                else
                {
                    Logger.Debug(string.Format(name + ":RemovePlayer(Player:{0}): Player '{1}' not found!", Net.NetUtil.ShortID(playerID), playerID));
                }
            }
        }
        
        /// <summary>
        /// Removes the remote Players that are hosted by the Client with given client ID.
        /// </summary>
        /// <param name="clientID"></param>
        public void RemovePlayersByClient(
            Guid clientID)
        {
            var callLogTag = name + ":RemovePlayersByClient(Client:" + WM.Net.NetUtil.ShortID(clientID) + ")";
            Logger.Debug(callLogTag);

            lock (Players)
            {
                var playersToRemove = new List<Guid>();

                foreach (var player in Players.Values)
                {
                    if (player.ClientID == clientID)
                    {
                        playersToRemove.Add(player.ID);
                    }
                }

                foreach (var playerID in playersToRemove)
                {
                    RemovePlayer(playerID);
                }
            }
        }

        /// <summary>
        /// Updates the avatar type for the player with given ID.
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="avatarID"></param>
        public void SetPlayerAvatar(
            Guid playerID,
            Guid avatarID)
        {
            var callLogTag = name + ":SetPlayerAvatar(Player:" + WM.Net.NetUtil.ShortID(playerID) + ", " + WM.Net.NetUtil.ShortID(avatarID) + ")";
            Logger.Debug(callLogTag);

            // Targeted player should be known by the application!
            Debug.Assert(Players.ContainsKey(playerID));

            if (playerID == Player.ID)
            {
                return; // We do not need/use/have an avatar for the local player.
            }

            lock (Players)
            {
                var player = (Players.ContainsKey(playerID) ? Players[playerID] : null);

                if (player == null)
                {
                    Logger.Warning(callLogTag + ": Player '" + playerID + "' not found!");
                    return;
                }

                // Get a handle to the old avatar (if any).
                var oldAvatar = player.Avatar;

                var position = new Vector3();
                var rotation = new Quaternion();

                if (oldAvatar != null)
                {
                    position = oldAvatar.transform.position;
                    rotation = oldAvatar.transform.rotation;
                }

                // Instanciate the new avatar at the location of the old avatar.
                var avatarGO = InstantiateAvatarPrefab(
                    avatarID,
                    position,
                    rotation);
                avatarGO.name = "Player(" + player.ID + ") Avatar";
                SceneManager.MoveGameObjectToScene(avatarGO, gameObject.scene);
                avatarGO.SetActive(true);

                player.Avatar = avatarGO.GetComponent<WM.Net.Avatar>();
                player.AvatarID = avatarID;

                // Destroy the old avatar (if any).
                if (oldAvatar != null)
                {
                    UtilUnity.Destroy(oldAvatar.gameObject);
                }
            }
        }

        #endregion
    }
}
