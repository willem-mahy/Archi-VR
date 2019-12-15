﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WM;
using WM.Net;
using ArchiVR.Command;
using ArchiVR.Net;

[assembly: System.Reflection.AssemblyVersion("1.0.*")]

namespace ArchiVR.Application
{
    public class TeleportationSystemArchiVR : WM.Application.ITeleportationSystem
    {
        public ApplicationArchiVR Application;

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
    }

    public class ApplicationArchiVR : WM.Application.UnityApplication
    {
        #region Variables

        // The typed application states.
        public ApplicationStateDefault m_applicationStateDefault = new ApplicationStateDefault();
        public ApplicationStateTeleporting m_applicationStateTeleporting = new ApplicationStateTeleporting();

        /// <summary>
        /// 
        /// </summary>
        public TeleportationSystemArchiVR TeleportationSystem;

        /// <summary>
        /// 
        /// </summary>
        public TeleportCommand TeleportCommand { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected new bool CanProcessCommands
        {
            get
            {
                return (TeleportCommand == null);
            }
        }

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

        /// <summary>
        /// The list of Points-Of-Interest in the active project.
        /// </summary>
        List<GameObject> m_POI = new List<GameObject>();

        #endregion

        #endregion

        //! Start is called before the first frame update
        public override void Init()
        {
            // Teleportation system
            TeleportationSystem = new TeleportationSystemArchiVR(this);

            // Application modes
            m_applicationStateTeleporting.TeleportationSystem = TeleportationSystem;

            m_applicationStates.Add(m_applicationStateDefault);
            m_applicationStates.Add(m_applicationStateTeleporting);

            
            base.Init();

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

            SetActiveImmersionMode(DefaultImmersionModeIndex);

            SetActiveProject(0);
        }

        protected override void UpdateNetwork()
        {
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

        #region Immersion mode

        public const int DefaultImmersionModeIndex = 0;

        // The 'Walkthrough'immersion mode.
        ImmersionModeWalkthrough immersionModeWalkthrough = new ImmersionModeWalkthrough();

        // The 'Maquette'immersion mode.
        ImmersionModeMaquette immersionModeMaquette = new ImmersionModeMaquette();

        // The immersion modes list.
        List<ImmersionMode> m_immersionModes = new List<ImmersionMode>();

        // The active immersion mode index.
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

        // The 'Walkthrough' immersion mode.
        public ImmersionModeWalkthrough ImmersionModeWalkthrough
        {
            get
            {
                return immersionModeWalkthrough;
            }
        }


        // The 'Maquette' immersion mode.
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

        public void OnTeleportFadeOutComplete()
        {
            WM.Logger.Debug("ApplicationArchiVR::OnTeleportFadeInComplete()");

            m_fadeAnimator.ResetTrigger("FadeOut");

            var applicationState = GetActiveApplicationState();
            if (applicationState != null)
            {
                applicationState.OnTeleportFadeOutComplete();
            }
        }

        public void OnTeleportFadeInComplete()
        {
            WM.Logger.Debug("ApplicationArchiVR::OnTeleportFadeInComplete()");

            m_fadeAnimator.ResetTrigger("FadeIn");

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teleportCommand"></param>
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

        /// <summary>
        /// 
        /// </summary>
        public bool NeedFadeOutUponTeleport
        {
            get
            {
                return (ActiveProject != null) && (ActivePOI != null);
            }
        }

        /// <summary>
        /// Checks the current input and toggles the active project if necessary.
        /// </summary>
        /// <returns>'true' if a new project is activated, 'false' otherwise.</returns>
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

        /// <summary>
        /// Checks the current input and toggles the active POI if necessary.
        /// </summary>
        /// <returns>'true' if a new POI is activated, 'false' otherwise.</returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator Teleport()
        {
            WM.Logger.Debug("ApplicationArchiVR::Teleport()");

            if (TeleportCommand == null)
            {
                WM.Logger.Warning("ApplicationArchiVR::Teleport(): TeleportCommand == null!");
                yield break;
            }

            if (ActiveProjectIndex != TeleportCommand.ProjectIndex) // If project changed...
            {
                // Needs to be cached before activating the new project.
                var oldProjectName = ActiveProjectName;

                // First unload the current project
                if (oldProjectName != null)
                {
                    WM.Logger.Debug("Unloading project '" + oldProjectName + "'");

                    AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(oldProjectName);

                    // Wait until asynchronous unloading the old project finishes.
                    while (!asyncUnload.isDone)
                    {
                        yield return null;
                    }
                }

                // Then load the new projct
                var newProjectName = GetProjectName(TeleportCommand.ProjectIndex);

                WM.Logger.Debug("Loading project '" + newProjectName + "'");

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

        #endregion

        #region

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

        #endregion

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

        #region Project management

        /// <summary>
        /// Gathers all projects included in the application.
        /// </summary>
        void GatherProjects()
        {
            m_projectNames = GetProjectNames();
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
            return m_projectNames[projectIndex];
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

                return UtilUnity.TryFindGameObject("Project");
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
    };
}