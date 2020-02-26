using ControllerSelection;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WM.Command;
using WM.Net;
using WM.UI;
using WM.Unity;
using WM.Unity.Tracking;
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

        #endregion

        /// <summary>
        /// The application version.
        /// </summary>
        abstract public string Name
        {
            get;
        }

        /// <summary>
        /// The number of frames rendered from start of application.
        /// </summary>
        protected int frame = 0;

        /// <summary>
        /// The application version.
        /// </summary>
        public string Version = "";

        #region VR Tracking

        /// <summary>
        /// 
        /// </summary>
        protected OVRBoundaryRepresentation OvrBoundaryRepresentation;

        /// <summary>
        /// 
        /// </summary>
        protected PlayerHeadCollider LocalPlayerHeadCollider;

        /// <summary>
        /// 
        /// </summary>
        protected Vector3 m_centerEyeAnchorPrev = new Vector3();

        /// <summary>
        /// The teleport area.
        /// </summary>
        public GameObject _teleportAreaGO;

        /// <summary>
        /// The teleport area.
        /// </summary>
        public TeleportAreaVolume _teleportAreaVolume;

        public void InitTeleport()
        {
            if (GetActiveApplicationState() != null)
            {
                GetActiveApplicationState().InitTeleport();
            }
        }

        #region Colocation

        /// <summary>
        /// WHether colocation is enabled.
        /// </summary>
        private bool _colocationEnabled = false;

        /// <summary>
        /// 
        /// </summary>
        public bool ColocationEnabled
        {
            get
            {
                return _colocationEnabled;
            }
            set
            {
                _colocationEnabled = value;

                // When colocation is enabled, recentering the headset must be disabled.
                OVRManager.instance.AllowRecenter = !_colocationEnabled;

                // TODO? Update tracking space location?
            }
        }

        #endregion Colocation

        #endregion TrackingSpace

        #region Player names

        /// <summary>
        /// 
        /// </summary>
        protected List<string> _playerNames = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> PlayerNames
        {
            get { return _playerNames; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int NumPlayerNames
        {
            get { return _playerNames.Count; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameToFind"></param>
        /// <returns></returns>
        public int GetPlayerNameIndex(string nameToFind)
        {
            return _playerNames.IndexOf(nameToFind);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public void SetPlayerName(int playerNameIndex)
        {
            Player.Name = playerNameIndex == -1 ? "" : _playerNames[playerNameIndex];
        }

        #endregion Player Names

        #region Network

        /// <summary>
        /// Flags whether the network has been initialized to any mode.
        /// 'false' at startup, and if initialization into a new network mode has failed.
        /// </summary>
        public bool NetworkInitialized
        {
            get;
            private set;
        } = false;

        /// <summary>
        /// The current network mode.
        /// Only valid if <see cref="NetworkInitialized"/> is true.
        /// </summary>
        public NetworkMode NetworkMode
        {
            get;
            private set;
        } = NetworkMode.Standalone;

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

        /// <summary>
        /// The players from remote connected Clients.
        /// </summary>
        /*private*/
        public Dictionary<Guid, Player> Players = new Dictionary<Guid, Player>();

        #endregion

        #region Game objects

        public Animator m_fadeAnimator;

        /// <summary>
        /// The HUD menu.
        /// This is a world-space menu that acts as an overlay menu, by making sure it is always located in front of the camera.
        /// This contains the FPS and Quality panel.
        /// </summary>
        public HUDMenu HudMenu;

        /// <summary>
        /// The WorldSpace menu.
        /// This contains the menus.
        /// </summary>
        public HUDMenu WorldSpaceMenu;

        public GameObject FpsPanelHUD;

        #region Reference Systems

        /// <summary>
        /// The list of reference systems.
        /// </summary>
        private List<ReferenceSystem6DOF> _referenceSystems = new List<ReferenceSystem6DOF>();

        /// <summary>
        /// 
        /// </summary>
        private void UpdateRefenceSystemsVisibility()
        {
            OvrBoundaryRepresentation.gameObject.SetActive(_showReferenceSystems);

            foreach (var referenceSystem in _referenceSystems)
            {
                referenceSystem.gameObject.SetActive(_showReferenceSystems);
            }
        }

        #region ShowReferenceSystems

        /// <summary>
        /// 
        /// </summary>
        private bool _showReferenceSystems = true;

        /// <summary>
        /// 
        /// </summary>
        public bool ShowReferenceSystems
        {
            get
            {
                return _showReferenceSystems;
            }
            set
            {
                _showReferenceSystems = value;
                UpdateRefenceSystemsVisibility();
            }
        }

        #endregion ShowReferenceSystems

        #endregion Reference Systems

        #region OVR GameObjects

        #region OVRCameraRig GameObjects

        public GameObject m_ovrCameraRig;

        public GameObject trackingSpace;

        public GameObject m_centerEyeAnchor;

        public GameObject m_leftHandAnchor;

        public GameObject m_rightHandAnchor;

        #endregion OVRCameraRig GameObjects

        public OVRPointerVisualizer SelectionVisualizer;

        #endregion OVRCameraRig GameObjects

        #endregion OVR GameObjects

        #region Application state

        // The list of application states.
        protected List<ApplicationState> m_applicationStates = new List<ApplicationState>();

        // The active immersion mode index.
        private int m_activeApplicationStateIndex = -1;

        //! Gets the active application state.  Returns null if no state is active.
        public ApplicationState GetActiveApplicationState()
        {
            if (m_activeApplicationStateIndex == -1)
                return null;

            return m_applicationStates[m_activeApplicationStateIndex];
        }

        /// <summary>
        /// Sets the active application state.
        /// </summary>
        /// <param name="applicationState"></param>
        /// <returns></returns>
        public ApplicationState SetActiveApplicationState(int applicationStateIndex)
        {
            if (GetActiveApplicationState() != null)
            {
                GetActiveApplicationState().Exit();
            }

            m_activeApplicationStateIndex = applicationStateIndex;

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

        /// <summary>
        /// The canvas attached to the center eye anchor.
        /// This acts as the container for HUD UI elements.
        /// </summary>
        public GameObject m_centerEyeCanvas;

        #endregion HUD menu

        #region World-scale menu

        /// <summary>
        /// The TabPanel containing the menus.
        /// Resides in the world-scale menu.
        /// </summary>
        public TabPanel menuPanel;

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
        /// Updates the visibility of the selection visualizer.
        /// </summary>
        private void UpdateSelectionVisualizerVisibility()
        {
            Logger.Debug("UpdateSelectionVisualizerVisibility() -> " + HasPickRaySelectionTargets());

            if (SelectionVisualizer == null)
            {
                return;
            }

            SelectionVisualizer.gameObject.SetActive(HasPickRaySelectionTargets());
        }

        /// <summary>
        /// Returns whether there are any targets for pick ray selection.
        /// </summary>
        private bool HasPickRaySelectionTargets()
        {
            return selectionTargets.Count != 0;
        }

        /// <summary>
        /// Register the given game object as a target for pick ray selection.
        /// </summary>
        /// <param name="selectionTarget"></param>
        public void AddPickRaySelectionTarget(GameObject selectionTarget)
        {
            //Logger.Warning("AddSelectionTarget(" + selectionTarget.name + ")");

            selectionTargets.Add(selectionTarget);

            UpdateSelectionVisualizerVisibility();
        }

        /// <summary>
        /// Unregister the given game object as a target for pick ray selection.
        /// </summary>
        /// <param name="selectionTarget"></param>
        public void RemovePickRaySelectionTarget(GameObject selectionTarget)
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
        public void Teleport(TeleportCommand teleportCommand)
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

            var scene = gameObject.scene;

            #region Get handles to VR Tracking objects
            
            #region Get handles to OVRGameraRig game objects

            if (m_ovrCameraRig == null)
            {
                m_ovrCameraRig = UtilUnity.FindGameObjectElseError(scene, "OVRCameraRig");
            }

            if (trackingSpace == null)
            {
                trackingSpace = UtilUnity.FindGameObjectElseError(scene, "TrackingSpace");
            }

            if (m_centerEyeAnchor == null)
            {
                m_centerEyeAnchor = UtilUnity.FindGameObjectElseError(scene, "CenterEyeAnchor");
            }

            if (m_leftHandAnchor == null)
            {
                m_leftHandAnchor = UtilUnity.FindGameObjectElseError(scene, "LeftHandAnchor");
            }

            if (m_rightHandAnchor == null)
            {
                m_rightHandAnchor = UtilUnity.FindGameObjectElseError(scene, "RightHandAnchor");
            }

            #endregion Get handles to OVRGameraRig game objects

            if (OvrBoundaryRepresentation == null)
            {
                OvrBoundaryRepresentation = UtilUnity.FindGameObjectElseError(scene, "OVRBoundaryRepresentation").GetComponent<OVRBoundaryRepresentation>();
            }

            if (LocalPlayerHeadCollider == null)
            {
                LocalPlayerHeadCollider = UtilUnity.FindGameObjectElseError(scene, "PlayerHeadCollider").GetComponent<PlayerHeadCollider>();
            }

            #endregion Get handles to VR Tracking objects

            if (m_centerEyeCanvas == null)
            {
                m_centerEyeCanvas = UtilUnity.FindGameObject(scene, "CenterEyeCanvas");
            }

            if (menuPanel == null)
            {
                var menuPanelGO = UtilUnity.FindGameObject(scene, "MenuPanel");
                menuPanel = menuPanelGO.GetComponent<TabPanel>();
            }

            if (!UnitTestModeEnabled)
            {
                if (FpsPanelHUD == null)
                {
                    FpsPanelHUD = UtilUnity.FindGameObject(scene, "FPSPanel");
                }
            }

            // Left controller.

            // Pick ray.
            if (!UnitTestModeEnabled)
            {
                var LPickRayGameObject = UtilUnity.FindGameObject(scene, "L PickRay");

                if (LPickRayGameObject != null)
                {
                    LPickRay = LPickRayGameObject.GetComponent<PickRay>();
                }
            }

            m_leftControllerCanvas = UtilUnity.FindGameObject(scene, "LeftControllerCanvas");
            m_leftControllerPanel = UtilUnity.FindGameObject(scene, "LeftControllerPanel");
            m_leftControllerText = UtilUnity.FindGameObject(scene, "LeftControllerText").GetComponent<UnityEngine.UI.Text>();

            // Right controller.

            // Pick ray.
            if (!UnitTestModeEnabled)
            {
                var RPickRayGameObject = UtilUnity.FindGameObject(scene, "R PickRay");

                if (RPickRayGameObject != null)
                {
                    RPickRay = RPickRayGameObject.GetComponent<PickRay>();
                }
            }

            m_rightControllerCanvas = UtilUnity.FindGameObject(scene, "RightControllerCanvas");
            m_rightControllerPanel = UtilUnity.FindGameObject(scene, "RightControllerPanel");
            m_rightControllerText = UtilUnity.FindGameObject(scene, "RightControllerText").GetComponent<UnityEngine.UI.Text>();

            #endregion

            LocalPlayerHeadCollider.PlayerID = Player.ID;

            // Disable all pickrays.
            if (LPickRay != null)
            {
                LPickRay.gameObject.SetActive(false);
            }

            if (RPickRay != null)
            {
                RPickRay.gameObject.SetActive(false);
            }

            CreateReferenceSystems();

            #region Init application states.

            foreach (var applicationState in m_applicationStates)
            {
                applicationState.m_application = this;
                applicationState.Init();
            }

            #endregion

            MenuVisible = false;

            UpdateSelectionVisualizerVisibility();

            new InitNetworkCommand(StartupNetworkMode).Execute(this);
        }

        /// <summary>
        /// To be implemented by concrete applications.
        /// </summary>
        public virtual void SaveApplicationSettings()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="networkMode"></param>
        public void InitNetworkMode(NetworkMode networkMode)
        {
            if (NetworkInitialized) // If the network has never been initialized yet, we do not need to tear down anything.
            {
                if (NetworkMode == networkMode)
                {
                    return; // NOOP: already running in requested network mode...
                }

                // Teardown from current network mode.
                switch (NetworkMode)
                {
                    case NetworkMode.Client:
                        {
                            Client.Disconnect();
                        }
                        break;
                    case NetworkMode.Server:
                        {
                            Client.Disconnect();
                            Server.Shutdown();
                        }
                        break;
                    case NetworkMode.Standalone:
                        {
                            //ServerDiscovery.Stop();
                        }
                        break;
                }
            }

            NetworkInitialized = false;

            // Initialize for new network mode.
            switch (networkMode)
            {
                case NetworkMode.Server:
                    {
                        // Init network server
                        Server.Init();

                        // Init network client
                        // Let client connect to own server. (TODO: connect directly, ie without network middle layer.)
                        Client.ServerInfo = new ServerInfo(
                            NetUtil.GetLocalIPAddress().ToString(),
                            Server.TcpPort,
                            Server.UdpPort);

                        Client.Connect();
                    }
                    break;
                case NetworkMode.Client:
                    {
                        // Init network client only
                        Client.Connect();
                    }
                    break;
                case NetworkMode.Standalone:
                    {
                        // Init no network
                        //ServerDiscovery.Start();
                    }
                    break;
            }

            NetworkInitialized = true;
            NetworkMode = networkMode;
        }

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
            if ((NetworkMode != NetworkMode.Standalone) && ColocationEnabled)
            {
                //Logger.Warning("UnityApplication.Fly: Colocation is enabled -> trackingspace manipulation disabled!");
                return;
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
            bool toggleMenu = m_controllerInput.m_controllerState.startButtonDown;

            if (toggleMenu)
            {
                ToggleMenuVisible();
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
        public void Process(object message)
        {
            if (message is AddPlayerCommand addPlayerCommand)
            {
                if (this.NetworkMode == NetworkMode.Client)
                {
                    Logger.Warning(string.Format("UnityApplication.Process: ClientApp {0} received AddPlayerCmmand for player {1}", Client.LogID, addPlayerCommand.Player.LogID));
                }

                if (this.NetworkMode == NetworkMode.Server)
                {
                    Logger.Warning(string.Format("UnityApplication.Process: ServerApp {0} received AddPlayerCmmand for player {1}", Client.LogID, addPlayerCommand.Player.LogID));
                }

            }

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

        #region Menu Management

        /// <summary>
        /// Sets the active menu.
        /// </summary>
        /// <param name="menuIndex">The index of tha menu to set as the active menu.</param>
        public void SetActiveMenu(int menuIndex)
        {
            if (menuPanel == null)
            {
                return;
            }

            menuPanel.Activate(menuIndex);
        }

        /// <summary>
        /// Whether the menu is visible(true) or not (false).
        /// </summary>
        public bool MenuVisible
        {
            get
            {
                if (menuPanel == null)
                {
                    return false;
                }

                var go = menuPanel.gameObject;

                return go.activeSelf;
            }
            set
            {
                if (menuPanel == null)
                {
                    return;
                }

                var menuPanelGO = menuPanel.gameObject;

                menuPanelGO.SetActive(!menuPanelGO.activeSelf);

                if (MenuVisible)
                {
                    // Whenever we turn the menu visible, we re-anchor it so it is in view for the user.
                    if (WorldSpaceMenu != null)
                    {
                        WorldSpaceMenu.UpdateAnchoring(); // Re-anchor the World-Space menu to be in front of cam, when leaving None mode.
                    }
                }

                if (MenuVisible)
                {
                    AddPickRaySelectionTarget(gameObject);
                }
                else
                {
                    RemovePickRaySelectionTarget(gameObject);
                }

                if (!MenuVisible)
                {
                    SaveApplicationSettings();
                }
            }
        }

        /// <summary>
        /// Toggles menus visible/invisible.
        /// </summary>
        public void ToggleMenuVisible()
        {
            MenuVisible = !MenuVisible;
        }

        /// <summary>
        /// Activates the next menu.
        /// </summary>
        public void ActivateNextMenu()
        {
            if (menuPanel == null)
            {
                return;
            }

            menuPanel.ActivateNext();
        }

        /// <summary>
        /// Activates the previous menu.
        /// </summary>
        public void ActivatePreviousMenu()
        {
            if (menuPanel == null)
            {
                return;
            }

            menuPanel.ActivatePrevious();
        }

        #endregion Menu Management

        #region Tracking Space

        public virtual void OnTeleportFadeOutComplete() { }

        public virtual void OnTeleportFadeInComplete() { }

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
            if ((NetworkMode != NetworkMode.Standalone) && ColocationEnabled)
            {
                Logger.Warning("UnityApplication.UpdateTrackingSpace: Colocation is enabled -> trackingspace manipulation disabled!");
                return;
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

        #endregion Tracking Space

        /// <summary>
        /// To be implemented by concrete application types.
        /// </summary>
        virtual protected void UpdateNetwork()
        { }

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
        public readonly ResourcePrefabGameObjectFactory AvatarFactory = new ResourcePrefabGameObjectFactory();
        
        /// <summary>
        /// 
        /// </summary>
        public float MaxWorldSpaceMenuSize => (UnityEngine.Application.isEditor) ? 0.005f : 0.0035f;

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
            Logger.Warning(string.Format(name + ":AddPlayer(Client:{0}, Player:{1}, Name:'{2}')", WM.Net.NetUtil.ShortID(player.ClientID), player.LogID, player.Name));

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

                #region Add player head collider.

                // Load prefab
                var phcPrefab = Resources.Load<GameObject>("WM/Prefab/VR/HeadCollider");

                // Instanciate GO from prefab
                var phcGO = Instantiate(phcPrefab, Vector3.zero, Quaternion.identity);

                // Attach GO to player's avatar head.
                phcGO.transform.SetParent(player.Avatar.Head.transform, false);

                var phc = phcGO.GetComponent<PlayerHeadCollider>();
                phc.PlayerID = player.ID;

                #endregion Add player head collider.

                // Destroy the old avatar (if any).
                if (oldAvatar != null)
                {
                    UtilUnity.Destroy(oldAvatar.gameObject);
                }
            }
        }

        #endregion

        #region ReferenceSystems

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public void SetSharedReferenceSystemLocalLocation(
            Vector3 position,
            Quaternion rotation)
        {
            SetReferenceSystemLocalLocation(
                SharedReferenceSystem,
                position,
                rotation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public static void SetReferenceSystemLocalLocation(
            ReferenceSystem6DOF referenceSystem,
            Vector3 position,
            Quaternion rotation)
        {
            var referenceSystemGO = referenceSystem.gameObject;

            // Copy over location.
            referenceSystemGO.transform.localPosition = position;
            referenceSystemGO.transform.localRotation = rotation;

            UpdateReferenceSystemCaption(referenceSystem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public void SetSharedReferenceSystemLocation(
            Vector3 position,
            Quaternion rotation)
        {
            SetReferenceSystemLocation(
                SharedReferenceSystem,
                position,
                rotation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public static void SetReferenceSystemLocation(
            ReferenceSystem6DOF referenceSystem,
            Vector3 position,
            Quaternion rotation)
        {
            var referenceSystemGO = referenceSystem.gameObject;

            // Copy over location.
            referenceSystemGO.transform.position = position;
            referenceSystemGO.transform.rotation = rotation;

            UpdateReferenceSystemCaption(referenceSystem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceSystem"></param>
        private static void UpdateReferenceSystemCaption(ReferenceSystem6DOF referenceSystem)
        {
            var referenceSystemGO = referenceSystem.gameObject;
            
            var sharedReferenceSystemLocalPosition = referenceSystemGO.transform.localPosition;
            var captionText = string.Format("{0} {1}", referenceSystemGO.name, UtilUnity.ToString(sharedReferenceSystemLocalPosition));
            referenceSystem.CaptionText = captionText;
        }

        /// <summary>
        /// The tracking space reference system.
        /// </summary>
        public ReferenceSystem6DOF TrackingReferenceSystem;

        /// <summary>
        /// The shared space reference system.
        /// </summary>
        public ReferenceSystem6DOF SharedReferenceSystem;

        /// <summary>
        /// Creates the 'Shared' and 'Tracking' reference systems.
        /// </summary>
        private void CreateReferenceSystems()
        {
            TrackingReferenceSystem = CreateReferenceSystem("TRF", trackingSpace);

            SharedReferenceSystem = CreateReferenceSystem("SRF", trackingSpace);

            UpdateRefenceSystemsVisibility();
        }

        /// <summary>
        /// Creates a reference system and adds it to the given parent (if given).
        /// </summary>
        /// <param name="name">The name for the created reference system.</param>
        /// <param name="parentGO">The parent game object.  Can be null.</param>
        public ReferenceSystem6DOF CreateReferenceSystem(
            string name,
            GameObject parentGO)
        {
            // Load the ReferenceSystem6DOF prefab.
            var referenceSystemPrefab = Resources.Load("WM/Prefab/Geometry/ReferenceSystem6DOF");

            // Create a ReferenceSystem6DOF instance.
            var referenceSystemGO = UnityEngine.GameObject.Instantiate(
                    referenceSystemPrefab,
                    Vector3.zero,
                    Quaternion.identity) as GameObject;

            // Get a handle to the ReferenceSystem6DOF component.
            var referenceSystem = referenceSystemGO.GetComponent<ReferenceSystem6DOF>();

            _referenceSystems.Add(referenceSystem);

            // Give it a descriptive name.
            referenceSystemGO.name = name;

            if (parentGO != null)
            {
                // Attach it as a child to the given parent GameObject
                referenceSystemGO.transform.SetParent(parentGO.transform, false);
            }

            // Initialize its caption.
            var referenceSystemLocalPosition = referenceSystemGO.transform.localPosition;
            var captionText = string.Format("{0} {1}", referenceSystemGO.name, UtilUnity.ToString(referenceSystemLocalPosition));
            
            referenceSystem.CaptionText = captionText;

            // Enable or disable reference system depending on ShowReferenceSystems.
            referenceSystemGO.SetActive(ShowReferenceSystems);

            return referenceSystem;
        }

        #endregion ReferenceSystems

        #region HUD Info

        public GameObject HudInfoPanel;

        public Text HudInfoText;

        #endregion HUD Info
    }
}
