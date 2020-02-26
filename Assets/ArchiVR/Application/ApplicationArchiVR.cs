﻿using ArchiVR.Command;
using ArchiVR.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using WM;
using WM.Application;
using WM.Colocation;
using WM.Command;
using WM.Net;
using WM.Unity;

[assembly: System.Reflection.AssemblyVersion("1.0.*")]

namespace ArchiVR.Application
{
    public class TeleportationSystemArchiVR : WM.Application.ITeleportationSystem
    {
        public ApplicationArchiVR Application;

        /// <summary>
        /// <see cref="ITeleportationSystem.NeedFadeOut"/> implementation.
        /// </summary>
        public bool NeedFadeOut
        {
            get
            {
                return (Application.ActiveProject != null) && (Application.ActivePOI != null);
            }
        }

        public TeleportationSystemArchiVR(ApplicationArchiVR application)
        {
            Application = application;
        }

        void ITeleportationSystem.Teleport(TeleportCommand command)
        {
            if ((Application.ActiveProjectIndex == command.ProjectIndex) && (Application.ActivePOIName == command.POIName))
            {
                return;
            }

            Application.TeleportCommand = command;

            if (Application.m_fadeAnimator != null)
            {
                Application.SetActiveApplicationState(1);
            }
            else
            {
                Application.Teleport();
            }
        }
    }


    /// <summary>
    /// The ArchiVR application.
    /// </summary>
    public class ApplicationArchiVR : UnityApplication
    {
        #region Variables

        /// <summary>
        /// 
        /// </summary>
        public EnvironmentalLighting EnvironmentalLighting;

        /// <summary>
        /// The OVRManger prefab.
        /// </summary>
        public GameObject ovrManagerPrefab;

        /// <summary>
        /// 
        /// </summary>
        public GameObject EventSystem;

        /// <summary>
        /// The typed application states.
        /// </summary>
        public readonly ApplicationStateDefault applicationStateDefault = new ApplicationStateDefault();
        public readonly ApplicationStateTeleporting applicationStateTeleporting = new ApplicationStateTeleporting();
        public readonly ApplicationStateDefineSharedReferenceSystem ApplicationStateDefineSharedReferenceSystem = new ApplicationStateDefineSharedReferenceSystem();

        #region Project

        /// <summary>
        /// The list of names of all projects included in the build.
        /// </summary>
        List<string> projectNames = new List<string>();

        /// <summary>
        /// The index of the currently active project.
        /// </summary>
        public int ActiveProjectIndex { get; set; } = -1;

        #endregion

        #region Layers

        /// <summary>
        /// Represents a layer (eg. "Kelder", "Gelijkvloers", "Verdiep", "Zolder")
        /// </summary>
        public class Layer
        {
            /// <summary>
            /// The gameobject containing the basic model geometry (as imported from sketchup).
            /// </summary>
            public GameObject Model;

            /// <summary>
            /// The gameobject containing the furniture geometry.
            /// </summary>
            public GameObject Furniture;

            /// <summary>
            /// The gameobject containing the lighting definitions and geometry.
            /// </summary>
            public GameObject Lighting;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="active"></param>
            public void SetActive(bool active)
            {
                Model.SetActive(active);
                
                if (Furniture != null)
                {
                    Furniture.SetActive(active);
                }

                if (Lighting != null)
                {
                    Lighting.SetActive(active);
                }
            }
        };

        /// <summary>
        /// The layers.
        /// </summary>
        private Dictionary<string, Layer> m_layers = new Dictionary<string, Layer>();
        
        #endregion

        #region POI

        /// <summary>
        /// The index to the currently active POI.
        /// </summary>
        private int activePOIIndex = -1;

        /// <summary>
        /// The index to the currently active POI.
        /// </summary>
        public int ActivePOIIndex
        {
            get { return activePOIIndex; }
            set
            {
                activePOIIndex = value;
                ActivePOIName = ActivePOI != null ? ActivePOI.name : null;
            }
        }

        /// <summary>
        /// The currently active POI.
        /// </summary>
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

        /// <summary>
        /// The currently active POI's name.
        /// </summary>
        public string ActivePOIName
        {
            get; private set;
        } = "";

        /// <summary>
        /// The list of Points-Of-Interest in the active project.
        /// </summary>
        List<GameObject> m_POI = new List<GameObject>();

        #endregion

        #endregion

        #region Test

        /// <summary>
        /// Load all avatar types.
        /// </summary>
        private void TestLoadAvatarPrefabsFromResources()
        {
            string[] prefabPaths =
            {
                "WM/Prefab/Avatar/Avatar Mario",
                "WM/Prefab/Avatar/Avatar IronMan",
                "WM/Prefab/Avatar/Avatar TUX",
                "WM/Prefab/Avatar/Avatar WillSmith",
            };

            LoadPrefabsFromResources(prefabPaths);
        }

        /// <summary>
        /// 
        /// </summary>
        private void TestLoadGeometryPrefabsFromResources()
        {
            string[] prefabPaths =
            {
                "WM/Prefab/Geometry/PointWithCaption",
                "WM/Prefab/Geometry/ReferenceSystem6DOF",
            };

            LoadPrefabsFromResources(prefabPaths);
        }

        /// <summary>
        /// Instanciate prefabs loaded from the paths in the given array.
        /// The paths must be relative to the Unity 'Resources' folder.
        /// </summary>
        private void LoadPrefabsFromResources(string[] prefabPaths)
        {
            for (int i = 0; i < prefabPaths.Length; ++i)
            {
                var prefab = Resources.Load(prefabPaths[i]);

                var go = Instantiate(prefab, i * Vector3.forward, Quaternion.identity);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void TestRegisteredAvatars()
        {
            var position = Vector3.zero;
            foreach (var avatarDefinition in AvatarDefinitions)
            {
                AvatarFactory.Create(avatarDefinition.ID, position, Quaternion.identity);
                position += Vector3.forward;
            }
        }

        #endregion Test

        /// <summary>
        /// Initialize all necessary stuff before the first frame update.
        /// </summary>
        public override void Init()
        {
            EnvironmentalLighting = UtilUnity.FindGameObjectElseError(gameObject.scene, "EnvironmentalLighting").GetComponent<EnvironmentalLighting>();

            if (SharedEnvironmentalLighting == null)
            {
                SharedEnvironmentalLighting = EnvironmentalLighting;
            }
            else
            {
                EnvironmentalLighting.gameObject.SetActive(false);
            }

            if (!UnitTestModeEnabled && (OVRManager.instance == null))
            {
                Instantiate(ovrManagerPrefab, Vector3.zero, Quaternion.identity);
            }

            // Load the application settings and enable logger ASAP at startup,
            // in order to include appliction initialization into the log.
            var settings = LoadSettings();

            Logger.Enabled = settings.DebugLogSettings.LoggingEnabled;
            
            #region Apply Network settings

            ColocationEnabled = settings.NetworkSettings.ColocationEnabled;

            #endregion Apply Network settings

            #region Apply Player settings

            _playerNames = settings.PlayerNames;

            Player.Name = settings.PlayerSettings.name;

            if (!_playerNames.Contains(Player.Name))
            {
                _playerNames.Add(Player.Name);
            }

            Player.AvatarID = settings.PlayerSettings.avatarID;

            #endregion Apply Player settings

            // Initialize application modes
            applicationStateTeleporting.TeleportationSystem = this.TeleportationSystem = new TeleportationSystemArchiVR(this);

            m_applicationStates.Add(applicationStateDefault);
            m_applicationStates.Add(applicationStateTeleporting);
            m_applicationStates.Add(ApplicationStateDefineSharedReferenceSystem);
            
            base.Init();

            #region Apply Graphics settings

            // Quality level.
            QualitySettings.SetQualityLevel(settings.GraphicsSettings.QualityLevel);

            // Show Reference Systems
            ShowReferenceSystems = settings.GraphicsSettings.ShowReferenceFrames;

            // Show FPS
            FpsPanelHUD.SetActive(settings.GraphicsSettings.ShowFPS);

            // (*) Clamp world-scale menu size and height to sensible value range when loaded from application settings.
            // This is necessary if the application settings file does not contain these values yet.
            // Otherwise we end up with a gigantic menu that is unoperable.

            // World-space menu size
            var worldSpaceMenuSize = Mathf.Clamp(settings.GraphicsSettings.WorldScaleMenuSize, 0.001f, MaxWorldSpaceMenuSize); // (*)
            WorldSpaceMenu.gameObject.transform.localScale = worldSpaceMenuSize * Vector3.one;

            // World-space menu height
            var height = settings.GraphicsSettings.WorldScaleMenuHeight;
            height = Mathf.Clamp(height, -1, 1); // (*) 
            WorldSpaceMenu.Offset.y = height;

            #endregion Apply Graphics settings

            SetSharedReferenceSystemLocalLocation(
                settings.NetworkSettings.SharedReferencePosition,
                settings.NetworkSettings.SharedReferenceRotation);

            #region Init immersion modes.

            m_immersionModes.Add(ImmersionModeWalkthrough);
            m_immersionModes.Add(ImmersionModeMaquette);

            foreach (var immersionMode in m_immersionModes)
            {
                immersionMode.Application = this;
                immersionMode.Init();
            }

            # endregion

            GatherProjects();

            if (!UnitTestModeEnabled)
            {
                RegisterAvatars();
            }
            
            SetActiveImmersionMode(DefaultImmersionModeIndex);

            SetActiveProject(0);

            //TestLoadAvatarPrefabsFromResources();
            //TestLoadGeometryPrefabsFromResources();
            //TestRegisteredAvatars();
        }

        #region GameObject overrides

        /// <summary>
        /// Called when the application is resumed after being paused.
        /// </summary>
        void OnApplicationFocus(bool hasFocus)
        {
            Logger.Debug("ApplicationArchiVR.OnApplicationFocus(" + hasFocus + ")");        
        }

        /// <summary>
        /// Called when the application is paused.
        /// </summary>
        void OnApplicationPause(bool pauseStatus)
        {
            Logger.Debug("ApplicationArchiVR.OnApplicationPause("+pauseStatus+")");
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnApplicationQuit() // TODO: verify whether this works when running on quest (it did not seem to- so added a 'SaveApplicationSettings() call to UnityApplication, when hiding the menu...)
        {
            Logger.Debug("ApplicationArchiVR.OnApplicationQuit()");

            SaveApplicationSettings();
        }

        #endregion GameObject overrides

        #region Avatar management

        /// <summary>
        /// <see cref="UnityApplication.DefaultAvatarID"/> implementation.
        /// </summary>
        public override Guid DefaultAvatarID
        {
            get;
            set;
        } = AvatarMarioID;

        public static readonly Guid AvatarMarioID       = new Guid("{160AB06A-8A58-42AF-BF40-E42CC5E3DD98}");
        public static readonly Guid AvatarWillSmithID   = new Guid("{25871844-B6DD-4AEC-B205-71C811D5960E}");
        public static readonly Guid AvatarTuxID         = new Guid("{354CC70A-1F01-49DC-8CFF-35FFF0CB6D38}");
        public static readonly Guid AvatarIronManID     = new Guid("{4B3C96EB-C854-49AE-BACC-3145CDF743AF}");

        public class AvatarDefinition
        {
            /// <summary>
            /// 
            /// </summary>
            public readonly Guid ID;

            /// <summary>
            /// 
            /// </summary>
            public readonly string ResourcePath;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="ID"></param>
            /// <param name="resourcePath"></param>
            public AvatarDefinition(Guid ID, string resourcePath)
            {
                this.ID = ID;
                this.ResourcePath = resourcePath;
            }
        }

        public static readonly AvatarDefinition[] AvatarDefinitions =
        {
            new AvatarDefinition(AvatarMarioID, "WM/Prefab/Avatar/Avatar Mario"),
            new AvatarDefinition(AvatarIronManID, "WM/Prefab/Avatar/Avatar IronMan"),
            new AvatarDefinition(AvatarTuxID, "WM/Prefab/Avatar/Avatar TUX"),
            new AvatarDefinition(AvatarWillSmithID, "WM/Prefab/Avatar/Avatar WillSmith"),
        };

        /// <summary>
        /// 
        /// </summary>
        private void RegisterAvatars()
        {
            foreach (var avatarDefinition in AvatarDefinitions)
            {
                AvatarFactory.Register(avatarDefinition.ID, avatarDefinition.ResourcePath);
            }
        }

        #endregion Avatar management

        #region Immersion mode

        /// <summary>
        /// 
        /// </summary>
        public const int DefaultImmersionModeIndex = 0;

        /// <summary>
        /// The 'Walkthrough'immersion mode.
        /// </summary>
        ImmersionModeWalkthrough immersionModeWalkthrough = new ImmersionModeWalkthrough();

        /// <summary>
        /// The 'Maquette'immersion mode.
        /// </summary>
        ImmersionModeMaquette immersionModeMaquette = new ImmersionModeMaquette();

        /// <summary>
        /// The immersion modes list.
        /// </summary>
        List<ImmersionMode> m_immersionModes = new List<ImmersionMode>();

        /// <summary>
        /// The active immersion mode index.
        /// </summary>
        public int ActiveImmersionModeIndex { get; set; } = -1;

        // The active immersion mode.
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

        /// <summary>
        /// The 'Walkthrough' immersion mode.
        /// </summary>
        public ImmersionModeWalkthrough ImmersionModeWalkthrough
        {
            get
            {
                return immersionModeWalkthrough;
            }
        }

        /// <summary>
        /// The 'Maquette' immersion mode.
        /// </summary>
        public ImmersionModeMaquette ImmersionModeMaquette
        {
            get
            {
                return immersionModeMaquette;
            }
        }

        #endregion

        #region Teleport

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexOffset"></param>
        public void TeleportToPOIInActiveProjectAtIndexOffset(int indexOffset)
        {
            TeleportToPOIInActiveProject(ActivePOIIndex + indexOffset);
        }

        #region Teleportation fading animation callbacks

        public override void OnTeleportFadeOutComplete()
        {
            Logger.Debug("ApplicationArchiVR::OnTeleportFadeInComplete()");

            m_fadeAnimator.ResetTrigger("FadeOut");

            var applicationState = GetActiveApplicationState();
            if (applicationState != null)
            {
                applicationState.OnTeleportFadeOutComplete();
            }
        }

        public override void OnTeleportFadeInComplete()
        {
            Logger.Debug("ApplicationArchiVR::OnTeleportFadeInComplete()");

            m_fadeAnimator.ResetTrigger("FadeIn");
            
            // This denotifies that we are no longer teleporting, and makes the command processor resume.
            TeleportCommand = null;

            var applicationState = GetActiveApplicationState();
            if (applicationState != null)
            {
                applicationState.OnTeleportFadeInComplete();
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newPOIIndex"></param>
        void TeleportToPOIInActiveProject(int newPOIIndex)
        {
            var tc = GetTeleportCommandForPOI(newPOIIndex);

            if (tc != null)
            {
                Teleport(tc);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool NeedFadeOutUponTeleport // TODO: move to teleportationSystem...
        {
            get
            {
                return (ActiveProject != null) && (ActivePOI != null);
            }
        }

        /// <summary>
        /// Previous project is activate using controller button 'X'
        /// </summary>
        public bool ActivatePreviousProject => m_controllerInput.m_controllerState.xButtonDown;

        /// <summary>
        /// Next project is activate using controller button 'Y'
        /// </summary>
        public bool ActivateNextProject => m_controllerInput.m_controllerState.yButtonDown;

        /// <summary>
        /// Previous POI is activate using controller button 'A'
        /// </summary>
        public bool ActivatePreviousPOI => m_controllerInput.m_controllerState.aButtonDown;

        /// <summary>
        /// Next POI is activate using controller button 'B'
        /// </summary>
        public bool ActivateNextPOI => m_controllerInput.m_controllerState.bButtonDown;


        /// <summary>
        /// Checks the current input and toggles the active project if necessary.
        /// </summary>
        /// <returns>'true' if a new project is activated, 'false' otherwise.</returns>
        public bool ToggleActiveProject()
        {
            // Active project is toggled using X/Y button, F1/F2 keys.
            if (ActivatePreviousProject)
            {
                SetActiveProject(ActiveProjectIndex - 1);
                return true;
            }
            
            if (ActivateNextProject)
            {
                SetActiveProject(ActiveProjectIndex + 1);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks the current input and toggles the active POI if necessary.
        /// </summary>
        /// <returns>'true' if a new POI is activated, 'false' otherwise.</returns>
        public bool ToggleActivePOI()
        {
            if (ActivatePreviousPOI)
            {
                TeleportToPOIInActiveProject(ActivePOIIndex - 1);
                return true;
            }

            if (ActivateNextPOI)
            {
                TeleportToPOIInActiveProject(ActivePOIIndex + 1);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Activates a POI, by name.
        /// </summary>
        /// <param name="newPOIName"></param>
        void SetActivePOI(string newPOIName)
        {
            // Get the POI index by POI name.
            var newPOIIndex = GetPOIIndex(newPOIName);

            if (newPOIIndex == -1)
            {
                if (m_POI.Count > 0)
                {
                    newPOIIndex = 0;
                }
            }

            ActivePOIIndex = newPOIIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator Teleport()
        {
            Logger.Debug("ApplicationArchiVR::Teleport()");

            if (TeleportCommand == null)
            {
                Logger.Warning("ApplicationArchiVR::Teleport(): TeleportCommand == null!");
                yield break;
            }

            if (ActiveProjectIndex != TeleportCommand.ProjectIndex) // If project changed...
            {
                // First unload the current project
                if (_projectScene != null)
                {
                    Logger.Debug("Unloading project sccene '" + _projectScene.Value.name + "'");

                    var asyncUnload = SceneManager.UnloadSceneAsync(_projectScene.Value);

                    // Wait until asynchronous unloading the old project finishes.
                    while (!asyncUnload.isDone)
                    {
                        yield return null;
                    }

                    ProjectScenes.Remove(_projectScene.Value);

                    _projectScene = null;
                }

                // Then load the new projct
                var newProjectName = GetProjectName(TeleportCommand.ProjectIndex);

                Logger.Debug("Loading project '" + newProjectName + "'");

                while (LoadingProject)
                {
                    yield return null;
                }

                LoadingProject = true;

                var asyncLoad = SceneManager.LoadSceneAsync(newProjectName, LoadSceneMode.Additive);

                // Wait until asynchronous loading the new project finishes.
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }

                bool renameProjectScene = false; // Disabled merging the 'saved' project scene into a renamed one, because this seems to break bakd Global Illumination.

                if (renameProjectScene)
                {
                    // Get a handle to the save project scene.
                    var projectScene = SceneManager.GetSceneByName(newProjectName);

                    // To support running multiple ArchiVR application instances in the same parent application (eg. WM TestApp)...

                    // ... 1) Give the project scene an application instance-specific name.
                    _projectScene = SceneManager.CreateScene(GetApplicationInstanceSpecificProjectName(newProjectName));
                    SceneManager.MergeScenes(projectScene, _projectScene.Value);
                }
                else
                {
                    // Get a handle to the save project scene.
                    for (int i = 0; i < SceneManager.sceneCount; ++i)
                    {
                        var scene = SceneManager.GetSceneAt(i);

                        if ((scene.name == newProjectName) & !ProjectScenes.Contains(scene))
                        {
                            _projectScene = scene;
                        }
                    }

                    if (_projectScene == null)
                    {
                        Logger.Error("Failed to locate newly loaded project scene '" + newProjectName + "'");
                    }
                }

                ProjectScenes.Add(_projectScene.Value);

                LoadingProject = false;

                // ... 2) Add spatial offset to project scene content
                foreach (var go in _projectScene.Value.GetRootGameObjects())
                {
                    go.transform.position += OffsetPerID;
                }

                // Update active project index to point to newly activated project.
                ActiveProjectIndex = TeleportCommand.ProjectIndex;

                // Update left controller UI displaying the project name.
                m_leftControllerText.text = (ActiveProjectName != null) ? GetProjectNameShort(ActiveProjectName) : "No project loaded.";
            }

            // Gather the POI from the new project.
            GatherActiveProjectPOI();

            // Gather the layers from the new project.
            GatherActiveProjectLayers();

            SetActivePOI(TeleportCommand.POIName);

            TeleportCommand = null;

            ActiveImmersionMode.UpdateTrackingSpacePosition();

            if (m_fadeAnimator != null)
            {
                m_fadeAnimator.SetTrigger("FadeIn");
            }
        }

        /// <summary>
        /// This flag is set by ArchiVR application instances while loading a new project.
        /// It is also checked by ArchiVR application instances when they start loading a new project:
        /// - if the flag is set, this means another project is loading the same project already.
        /// - since the same project scene cannot be loaded concurrently, any application that wants to start loading
        /// a project scene waits for the flag to be unset again.
        /// - This locking mechanism is not thread-safe.  It does it need to be thread-safe, since all scene loading happens on the same (Main) thread.
        /// </summary>
        static bool LoadingProject = false;
        static HashSet<Scene> ProjectScenes = new HashSet<Scene>();
        static EnvironmentalLighting SharedEnvironmentalLighting;

        /// <summary>
        /// The currently loaded project scene.
        /// 'null' if no project scene loaded.
        /// </summary>
        Scene? _projectScene = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        private string GetApplicationInstanceSpecificProjectName(string projectName)
        {
            return "ArchiVR(" + ID + ") " + projectName;
        }

        #endregion

        #region

        /// <summary>
        /// 
        /// </summary>
        void UpdateTrackingSpacePosition()
        {
            if (ActiveImmersionMode == null)
                return;

            ActiveImmersionMode.UpdateTrackingSpacePosition();
        }

        /// <summary>
        /// 
        /// </summary>
        void UpdateModelLocationAndScale()
        {
            if (ActiveImmersionMode == null)
                return;

            ActiveImmersionMode.UpdateModelLocationAndScale();
        }

        #endregion

        #region Immersion mode management

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// TODO: Investigate whether to rename/factor out...
        /// </summary>
        /// <returns></returns>
        public bool ToggleImmersionModeIfInputAndNetworkModeAllows()
        {
            // Immersion mode is toggled using I key, Left index trigger.
            bool toggleImmersionMode = m_controllerInput.m_controllerState.lIndexTriggerDown || Input.GetKeyDown(KeyCode.I);

            if (toggleImmersionMode)
            {
                ToggleImmersionModeIfNetworkModeAllows();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Activates an immersion mode, by index.
        /// </summary>
        /// <param name="immersionModeIndex"></param>
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

        public void SetModelLayerVisible(
            int layerIndex,
            bool visible)
        {
            var command = new SetModelLayerVisibilityCommand(layerIndex, visible);

            if (NetworkMode == NetworkMode.Server)
            {
                Server.BroadcastCommand(command);
            }
            else
            {
                command.Execute(this);
            }
        }

        #region Project management

        /// <summary>
        /// Gathers all projects included in the application.
        /// </summary>
        void GatherProjects()
        {
            projectNames = GetProjectNames();
        }

        /// <summary>
        /// Gets a list containing the names of all projects included in the application.
        /// </summary>
        /// <returns></returns>
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
            return projectNames[projectIndex];
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

        //! Gets the active project's name, or null if no project active.
        public string ActiveProjectName
        {
            get
            {
                // We deliberately do NOT return the temptingly simple ActiveProject.name here.
                // This returns the name (always "Project") of the gameobject representing the project in the scene.
                return ActiveProjectIndex == -1 ? null : projectNames[ActiveProjectIndex];
            }
        }

        //! Gets the active project.
        public GameObject ActiveProject
        {
            get
            {
                if (!_projectScene.HasValue)
                {
                    return null;
                }

                foreach (var go in _projectScene.Value.GetRootGameObjects())
                {
                    var project = UtilUnity.FindGameObject(go, "Project");

                    if (project!= null)
                    {
                        return project;
                    }
                }

                throw new Exception("Project loaded, but project scene does not contain a 'Project' GameObject!");
            }
        }

        /// <summary>
        /// Get the TeleportCommand to activate a project, by index.
        /// </summary>
        public TeleportCommand GetTeleportCommandForProject(int projectIndex)
        {
            if (projectNames.Count == 0)
            {
                projectIndex = -1;
                return null;
            }

            projectIndex = UtilIterate.MakeCycle(projectIndex, 0, projectNames.Count);

            if (projectIndex == ActiveProjectIndex)
            {
                return null;
            }

            var tc = new TeleportCommand();
            tc.ProjectIndex = projectIndex;
            tc.POIName = ActivePOIName;

            return tc;
        }

        /// <summary>
        /// Get the TeleportCommand to activate a project, by index.
        /// </summary>
        public TeleportInitiatedCommand GetTeleportInitCommandForProject(int projectIndex)
        {
            if (projectNames.Count == 0)
            {
                projectIndex = -1;
                return null;
            }
            
            projectIndex = UtilIterate.MakeCycle(projectIndex, 0, projectNames.Count);
            
            if (projectIndex == ActiveProjectIndex)
            {
                return null;
            }

            var tic = new TeleportInitiatedCommand();
            
            return tic;

            //var tc = new TeleportCommand();
            //tc.ProjectIndex = projectIndex;
            //tc.POIName = ActivePOIName;

            //return tc;
        }

        /// <summary>
        /// Get the TeleportInitiatedCommand to activate a POI, by index.
        /// </summary>        
        public TeleportInitiatedCommand GetTeleportInitCommandForPOI(int poiIndex)
        {
            // Determine the new POI index.
            if (m_POI.Count == 0)
            {
                poiIndex = -1;
                return null;
            }

            poiIndex = UtilIterate.MakeCycle(poiIndex, 0, m_POI.Count);

            var tic = new TeleportInitiatedCommand();

            return tic;
        }

        /// <summary>
        /// Get the TeleportCommand to activate a POI, by index.
        /// </summary>        
        public TeleportCommand GetTeleportCommandForPOI(int poiIndex)
        {
            // Determine the new POI index.
            if (m_POI.Count == 0)
            {
                poiIndex = -1;
                return null;
            }

            poiIndex = UtilIterate.MakeCycle(poiIndex, 0, m_POI.Count);

            var tc = new TeleportCommand();

            tc.ProjectIndex = ActiveProjectIndex;
            tc.POIName = m_POI[poiIndex].name;

            return tc;
        }

        /// <summary>
        /// Activates a project, by index.
        /// </summary>
        void SetActiveProject(int projectIndex)
        {
            var tc = GetTeleportCommandForProject(projectIndex);

            if (tc != null)
            {
                Teleport(tc);
            }
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

        /// <summary>
        /// <see cref="UnityApplication.Name"/> implementation.
        /// </summary>
        public override string Name
        {
            get
            {
                return "Archi-VR";
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

        /// <summary>
        /// Gets a list containing a handle to the layers.
        /// </summary>
        /// <returns></returns>
        public List<Layer> GetModelLayers()
        {
            return new List<Layer>(m_layers.Values);
        }

        /// <summary>
        /// Unhides all model layers.
        /// </summary>
        public void UnhideAllModelLayers()
        {
            foreach (var layer in m_layers.Values)
            {
                layer.SetActive(true);
            }
        }

        /// <summary>
        /// Gathers all model layers for the currently active project.
        /// </summary>
        public void GatherActiveProjectLayers()
        {
            m_layers.Clear();

            var activeProject = ActiveProject;

            if (activeProject == null)
            {
                return;
            }

            // Gather model layers.
            {
                var modelTransform = activeProject.transform.Find("Model");

                if (modelTransform == null)
                {
                    Logger.Error("Active project does not contain a child named 'Model'.");
                }

                var layers = modelTransform.Find("Layers");

                if (layers == null)
                {
                    Logger.Error("Active project's 'Model' does not contain a child named 'Layers'.");
                }

                foreach (Transform layerTransform in layers.transform)
                {
                    var layerModelGO = layerTransform.gameObject;

                    var layer = new Layer();
                    layer.Model = layerModelGO;

                    m_layers.Add(layerModelGO.name, layer);
                }
            }

            // Gather lighting layers.
            {
                var lightingTransform = activeProject.transform.Find("Lighting");

                if (lightingTransform == null)
                {
                    Logger.Warning("Active project does not contain a child named 'Lighting'.");
                }
                else
                {
                    var layers = lightingTransform.Find("Layers");

                    if (layers == null)
                    {
                        Logger.Warning("Active project's 'Lighting' does not contain a child named 'Layers'.");
                    }

                    foreach (Transform layerTransform in layers.transform)
                    {
                        var layerLightingGO = layerTransform.gameObject;

                        if (m_layers.ContainsKey(layerLightingGO.name))
                        {
                            m_layers[layerLightingGO.name].Lighting = layerLightingGO;
                        }
                        else
                        {
                            var warning = string.Format("No corresponding layer '{0}' found for lighting layer.", layerLightingGO.name);
                            Logger.Warning(warning);
                        }
                    }
                }
            }

            // Gather furniture layers.
            {
                var lightingTransform = activeProject.transform.Find("Furniture");

                if (lightingTransform == null)
                {
                    Logger.Warning("Active project does not contain a child named 'Furniture'.");
                }
                else
                {
                    var layers = lightingTransform.Find("Layers");

                    if (layers == null)
                    {
                        Logger.Warning("Active project's 'Furniture' does not contain a child named 'Layers'.");
                    }

                    foreach (Transform layerTransform in layers.transform)
                    {
                        var layerFurnitureGO = layerTransform.gameObject;

                        if (m_layers.ContainsKey(layerFurnitureGO.name))
                        {
                            m_layers[layerFurnitureGO.name].Furniture = layerFurnitureGO;
                        }
                        else
                        {
                            var warning = string.Format("No corresponding layer '{0}' found for furniture layer.", layerFurnitureGO.name);
                            Logger.Warning(warning);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void UpdateNetwork()
        {
            //Logger.Debug(name + ".UpdateNetwork()");

            if (((m_centerEyeAnchor.transform.position - m_centerEyeAnchorPrev).magnitude > 0.01f) || (frame++ % 10 == 0))
            {
                ((ClientArchiVR)Client).SendAvatarStateToUdp(
                    m_centerEyeAnchor,
                    m_leftHandAnchor,
                    m_rightHandAnchor);
                m_centerEyeAnchorPrev = m_centerEyeAnchor.transform.position;
            }

            // Update positions of remote client avatars, with the avatar states received from the server via UDP.
            ((ClientArchiVR)Client).UpdateAvatarStatesFromUdp();
        }

        /// <summary>
        /// <see cref="UnityApplication.OnEnableInputChanged()"/> implementation.
        /// </summary>
        protected override void OnEnableInputChanged()
        {
            m_ovrCameraRig.GetComponent<OVRCameraRig>().enabled = EnableInput;
            m_ovrCameraRig.GetComponent<OVRHeadsetEmulator>().enabled = EnableInput;
            EventSystem.SetActive(EnableInput);

            m_centerEyeAnchor.GetComponent<AudioListener>().enabled = EnableInput;

            if (EnableInput)
            {
                // Restore the position/rotation of the camera in OVRManager.
                if (r != null)
                {
                    OVRManager.instance.headPoseRelativeOffsetRotation = r;
                }

                if (t != null)
                {
                    OVRManager.instance.headPoseRelativeOffsetTranslation = t;
                }
            }
            else
            {
                // Store the position/rotation of the camera in OVRManager.
                r = OVRManager.instance.headPoseRelativeOffsetRotation;
                t = OVRManager.instance.headPoseRelativeOffsetTranslation;
            }
        }

        /// <summary>
        /// Temporary caches the OVRManager headpose rotation while EnableInput is false.
        /// </summary>
        Vector3 r;

        /// <summary>
        /// Temporary caches the OVRManager headpose translation while EnableInput is false.
        /// </summary>
        Vector3 t;

        /// <summary>
        /// 
        /// </summary>
        private string SettingsFilePath
        {
            get
            {
                return Path.Combine(UnityEngine.Application.persistentDataPath, "ApplicationArchiVRSettings" + ID + ".xml");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ApplicationArchiVRSettings LoadSettings()
        {
            ApplicationArchiVRSettings settings = null;

            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(ApplicationArchiVRSettings));

                    using (var reader = new StreamReader(SettingsFilePath))
                    {
                        //reader.ReadToEnd();

                        settings = serializer.Deserialize(reader) as ApplicationArchiVRSettings;
                    }
                }
                catch (Exception e)
                {
                    Logger.Warning("Failed to load settings file!" + e.Message);
                }
            }
            else
            {
                Logger.Warning("Settings file '" + SettingsFilePath + "' not found.");

                settings = new ApplicationArchiVRSettings();

                settings.PlayerNames.Add("Mr.");
                settings.PlayerNames.Add("Ms.");
                settings.PlayerNames.Add("KS");

                settings.PlayerSettings.name = "KS";
                settings.PlayerSettings.avatarID = AvatarMarioID;

                SaveSettings(settings);
            }

            return settings;
        }

        /// <summary>
        /// <see cref="UnityApplication.SaveApplicationSettings()"/> implementation.
        /// </summary>
        public override void SaveApplicationSettings()
        {
            var settings = GetApplicationSettings();

            SaveSettings(settings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        private void SaveSettings(ApplicationArchiVRSettings settings)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(ApplicationArchiVRSettings));

                using (var writer = new StreamWriter(SettingsFilePath))
                {
                    serializer.Serialize(writer, settings);
                }
            }
            catch (Exception e)
            {
                Logger.Warning("Failed to save settings file!" + e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ApplicationArchiVRSettings GetApplicationSettings()
        {
            var settings = new ApplicationArchiVRSettings();

            settings.NetworkSettings.ColocationEnabled = ColocationEnabled;
            settings.NetworkSettings.SharedReferencePosition = SharedReferenceSystem.transform.localPosition;
            settings.NetworkSettings.SharedReferenceRotation = SharedReferenceSystem.transform.localRotation;

            settings.DebugLogSettings.LoggingEnabled = Logger.Enabled;
            
            settings.GraphicsSettings.QualityLevel = QualitySettings.GetQualityLevel();
            settings.GraphicsSettings.ShowFPS = FpsPanelHUD.activeSelf;
            settings.GraphicsSettings.ShowReferenceFrames = ShowReferenceSystems;
            settings.GraphicsSettings.WorldScaleMenuSize = WorldSpaceMenu.gameObject.transform.localScale.x;
            settings.GraphicsSettings.WorldScaleMenuHeight = WorldSpaceMenu.Offset.y;

            settings.PlayerNames = _playerNames;

            settings.PlayerSettings.name = Player.Name;
            settings.PlayerSettings.avatarID = Player.AvatarID;

            return settings;
        }

        #endregion
    };
}