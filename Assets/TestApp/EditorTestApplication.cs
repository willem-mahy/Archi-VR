using ArchiVR.Application;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WM;
using WM.Application;
using WM.Command;
using WM.Net;

public class EditorTestApplication : MonoBehaviour
{
    private int DefaultViewLayout = 8;
    private int DefaultNumApplicationInstances = 4;

    private bool _applicationInstancesInitialized = false;

    private const string ApplicationSceneName =
        "Application_ArchiVR";
        //"Application_Demo";

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    void Start()
    {
        SetViewLayout(DefaultViewLayout);

        StartCoroutine(InitializeApplicationInstances());        
    }

    // Update is called once per frame
    void Update()
    {
        if (!_applicationInstancesInitialized)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ActivateNextApplicationInstance();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void ActivateNextApplicationInstance()
    {
        int i = UtilIterate.MakeCycle(_activeApplicationInstanceIndex + 1, 0, _applicationInstances.Count);

        ActivateApplicationInstance(i);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationInstanceIndex"></param>
    private void ActivateApplicationInstance(int applicationInstanceIndex)
    {
        _activeApplicationInstanceIndex = applicationInstanceIndex;

        foreach (var applicationInstance in _applicationInstances)
        {
            applicationInstance.EnableInput = false;
        }

        _applicationInstances[_activeApplicationInstanceIndex].EnableInput = true;

        AttachBorderToView(_activeApplicationInstanceIndex);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="viewIndex"></param>
    private void AttachBorderToView(int viewIndex)
    {
        var camera = GetApplicationCamera(viewIndex);

        var canvas = GameObject.Find("ActiveViewBorderCanvas").GetComponent<Canvas>();

        canvas.worldCamera = camera;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationInstanceIndex"></param>
    /// <returns></returns>
    private Camera GetApplicationCamera(int applicationInstanceIndex)
    {
        var tag = "GetApplicationCamera(" + applicationInstanceIndex + ")";

        var cameraName = "CenterEyeAnchor(" + applicationInstanceIndex + ")";
        var cameraGO = UtilUnity.TryFindGameObject(cameraName);

        if (cameraGO == null)
        {
            throw new Exception(tag + "GameObject '" + cameraName + "' not found!");
        }

        var camera = cameraGO.GetComponent<Camera>();

        if (camera == null)
        {
            throw new Exception(tag + "GameObject '" + cameraName + "' does not contain a 'Camera' component!");
        }

        return camera;
    }

    /// <summary>
    /// If an application instance is being initialized, and its appliction scene is fully loaded,
    /// performs the post-load operations to finalize the initialization.
    /// - Get handle to UnityApplication instance
    /// - Push the viewlayout to the application camera.
    /// - Start initialization of the next application instance.
    /// </summary>
    private IEnumerator InitializeApplicationInstances()
    {
        var tag = "EditorTestApplication.InitializeApplicationInstances()";

        _activeApplicationInstanceIndex = 0;

        for (int applicationInstanceBeingInitializedIndex = 0; applicationInstanceBeingInitializedIndex < DefaultNumApplicationInstances; ++applicationInstanceBeingInitializedIndex)
        {
            var loadApplicationSceneOperation = SceneManager.LoadSceneAsync(ApplicationSceneName, LoadSceneMode.Additive);

            while (!loadApplicationSceneOperation.isDone)
            {
                yield return null;
            }

            var applicationSceneOrig = SceneManager.GetSceneByName(ApplicationSceneName);

            while (applicationSceneOrig == null)
            {
                yield return null; // Application scene not loaded yet.
                applicationSceneOrig = SceneManager.GetSceneByName(ApplicationSceneName);
            }

            while (!applicationSceneOrig.isLoaded)
            {
                yield return null; // Application scene not loaded yet.
            }

            var postFix = "(" + applicationInstanceBeingInitializedIndex + ")";

            // Since renaming a saved scene is not allowed,
            // we merge the loaded application scene into a uniquely named new scene(applicationScene).
            //
            // The originally loaded scene (applicationSceneOrig) will be destroyed automatically
            // by merging it into the final, uniquely named, application scene.
            var uniqueApplicationSceneName = applicationSceneOrig.name + postFix;

            var applicationScene = SceneManager.CreateScene(uniqueApplicationSceneName);
            SceneManager.MergeScenes(applicationSceneOrig, applicationScene);

            // First get the UnityApplication from the loaded application scene.
            UnityApplication applicationInstance = null;
            foreach (var go in applicationScene.GetRootGameObjects())
            {
                applicationInstance = UtilUnity.GetFirstComponentOfType<UnityApplication>(go);

                if (applicationInstance != null)
                {
                    break;
                }
            }

            if (applicationInstance == null)
            {
                throw new Exception(tag + "Loaded application scene does not contain a GameObject with a UnityApplication component!");
            }

            // Only enable input on the first application instance.
            applicationInstance.ID = applicationInstanceBeingInitializedIndex;
            applicationInstance.EnableInput = (applicationInstanceBeingInitializedIndex == 0);

            _applicationInstances.Add(applicationInstance);

            foreach (var go in applicationScene.GetRootGameObjects())
            {
                go.transform.position += applicationInstance.OffsetPerID;
            }

            applicationScenes.Add(applicationScene);

            var cameraGO = GameObject.Find("CenterEyeAnchor");

            if (cameraGO == null)
            {
                throw new Exception(tag + "GameObject 'CenterEyeAnchor' not found in application scene!");
            }

            cameraGO.name = cameraGO.name + postFix; // So it is not found anymore next time.

            var camera = cameraGO.GetComponent<Camera>();

            if (camera == null)
            {
                throw new Exception(tag + "GameObject 'CenterEyeAnchor' does not have a Camera component!");
            }

            camera.rect = WindowPlacements[_viewLayout][applicationInstanceBeingInitializedIndex];

            if (applicationInstanceBeingInitializedIndex == _activeApplicationInstanceIndex)
            {
                AttachBorderToView(applicationInstanceBeingInitializedIndex);
            }

            // Disable the default camera.
            GetDefaultCameraGO(applicationInstanceBeingInitializedIndex).SetActive(false);
        }

        // Now that all application scenes have been loaded, perform the startup logic on them.
        StartCoroutine(PerformStartupLogic());
    }

    /// <summary>
    /// 
    /// </summary>
    private IEnumerator PerformStartupLogic()
    {
        string[] playerNames =
        {
            "Server",
            "Client 1",
            "Client 2",
            "Client 3"
        };

        Guid[] playerAvatars =
        {
            ApplicationArchiVR.AvatarMarioID,
            ApplicationArchiVR.AvatarTuxID,
            ApplicationArchiVR.AvatarWillSmithID,
            ApplicationArchiVR.AvatarIronManID
        };

        /*
        foreach (var application in _applicationInstances)
        {
            application.QueueCommand(new SetMenuModeCommand(UnityApplication.MenuMode.Network));
        }
        */

        for (int i = 0; i < _applicationInstances.Count; ++i)
        {
            var ai = _applicationInstances[i];
            ai.SetPlayerName(playerNames[i]);
            ai.SetPlayerAvatar(playerAvatars[i]);
        }

        _applicationInstances[0].QueueCommand(new InitNetworkCommand(NetworkMode.Server));

        while (_applicationInstances[0].Server.State != Server.ServerState.Running)
        {
            yield return null;
        }

        for (int i = 1; i < _applicationInstances.Count; ++i)
        {
            _applicationInstances[i].QueueCommand(new InitNetworkCommand(NetworkMode.Client));

            while (_applicationInstances[i].Client.State != Client.ClientState.Connected) // (*)
            {
                yield return null;
            }
        }

        // TODO:
        // Design defect: we can only have one Client connecting at a given time!
        // This is why we need the above waits. (*)

        //for (int i = 0; i < _applicationInstances.Count; ++i)
        //{
        //    var ai = _applicationInstances[i];
        //    ai.SetPlayerName(playerNames[i]);
        //    ai.SetPlayerAvatar(playerAvatars[i]);
        //}

        _applicationInstancesInitialized = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="viewLayout"></param>
    void SetViewLayout(int viewLayout)
    {
        if (_viewLayout == viewLayout)
        {
            return;
        }

        _viewLayout = viewLayout;

        for (int i = 0; i < 4; ++i)
        {
            GetDefaultCamera(i).rect = WindowPlacements[viewLayout][i];
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    private GameObject GetDefaultCameraGO(int i)
    {
        return GameObject.Find("DefaultCamera (" + i + ")");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    private Camera GetDefaultCamera(int i)
    {
        return GetDefaultCameraGO(i).GetComponent<Camera>() as Camera;
    }

    //#region FailedExperiment_RenderOnlyAssociatedApplicationSceneForEachViewUsingCameraDelegates

    ///// <summary>
    ///// 
    ///// </summary>
    //private void FailedExperiment_RenderOnlyAssociatedApplicationSceneForEachViewUsingCameraDelegates()
    //{
    //    Camera.onPreCull = this.MyPreCull;
    //    //Camera.onPreRender = this.MyPreRender;
    //    Camera.onPostRender = this.MyPostRender;
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="cam"></param>
    //public void MyPreCull(Camera cam)
    //{
    //    Debug.Log("PreCull " + gameObject.name + " from camera " + cam.gameObject.name);

    //    foreach (var scene in applicationScenes)
    //    {
    //        var enable = (scene == cam.gameObject.scene);

    //        foreach (var go in scene.GetRootGameObjects())
    //        {
    //            if (go.activeSelf != enable)
    //                go.SetActive(enable);
    //        }
    //    }
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="cam"></param>
    //public void MyPreRender(Camera cam)
    //{
    //    Debug.Log("PreRender " + gameObject.name + " from camera " + cam.gameObject.name);

    //    foreach (var scene in applicationScenes)
    //    {
    //        var enable = (scene == cam.gameObject.scene);

    //        foreach (var go in scene.GetRootGameObjects())
    //        {
    //            if (go.activeSelf != enable)
    //                go.SetActive(enable);
    //        }
    //    }
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="cam"></param>
    //public void MyPostRender(Camera cam)
    //{
    //    Debug.Log("PostRender " + gameObject.name + " from camera " + cam.gameObject.name);

    //    foreach (var scene in applicationScenes)
    //    {
    //        foreach (var go in scene.GetRootGameObjects())
    //        {
    //            if (go.activeSelf != true)
    //                go.SetActive(true);
    //        }
    //    }
    //}

    //#endregion FailedExperiment_RenderOnlyAssociatedApplicationSceneForEachViewUsingCameraDelegates

    #region Variables

    /// <summary>
    /// The index (into '_applicationInstances') of the active application instance.
    /// </summary>
    private int _activeApplicationInstanceIndex = -1;

    /// <summary>
    /// The list of application instances.
    /// </summary>
    private List<UnityApplication> _applicationInstances = new List<UnityApplication>();

    /// <summary>
    /// The application scenes.
    /// </summary>
    private List<Scene> applicationScenes = new List<Scene>();

    /// <summary>
    /// The active view layout.
    /// </summary>
    private int _viewLayout = -1;

    static Rect[][] WindowPlacements =
    {
        // 1 view
        new Rect[]
        {
            new Rect(0.0f, 0.0f, 1.0f, 1.0f),
            new Rect(0.0f, 0.0f, 0.0f, 0.0f),
            new Rect(0.0f, 0.0f, 0.0f, 0.0f),
            new Rect(0.0f, 0.0f, 0.0f, 0.0f)
        },
        // 2 views (-)
        new Rect[]
        {
            new Rect(0.0f, 0.0f, 1.0f, 0.5f),
            new Rect(0.0f, 0.5f, 1.0f, 0.5f),
            new Rect(0.0f, 0.0f, 0.0f, 0.0f),
            new Rect(0.0f, 0.0f, 0.0f, 0.0f)
        },
        // 2 views (|)
        new Rect[]
        {
            new Rect(0.0f, 0.0f, 0.5f, 1.0f),
            new Rect(0.5f, 0.0f, 0.5f, 1.0f),
            new Rect(0.0f, 0.0f, 0.0f, 0.0f),
            new Rect(0.0f, 0.0f, 0.0f, 0.0f)
        },
        // 3 views (_|_)
        new Rect[]
        {
            new Rect(0.0f, 0.0f, 1.0f, 0.5f),
            new Rect(0.0f, 0.5f, 0.5f, 0.5f),
            new Rect(0.0f, 0.5f, 0.5f, 0.5f),
            new Rect(0.0f, 0.0f, 0.0f, 0.0f)
        },
        // 3 views (T)
        new Rect[]
        {
            new Rect(0.0f, 0.0f, 0.5f, 0.5f),
            new Rect(0.5f, 0.0f, 0.5f, 0.5f),
            new Rect(0.0f, 0.5f, 1.0f, 0.5f),
            new Rect(0.0f, 0.0f, 0.0f, 0.0f)
        },
        // 3 views (-|)
        new Rect[]
        {
            new Rect(0.0f, 0.0f, 0.5f, 0.5f),
            new Rect(0.0f, 0.5f, 0.5f, 0.5f),
            new Rect(0.5f, 0.0f, 0.5f, 1.0f),
            new Rect(0.0f, 0.0f, 0.0f, 0.0f)
        },
        // 3 views (|-)
        new Rect[]
        {
            new Rect(0.0f, 0.0f, 0.5f, 1.0f),
            new Rect(0.5f, 0.0f, 0.5f, 0.5f),
            new Rect(0.5f, 0.5f, 0.5f, 0.5f),
            new Rect(0.0f, 0.0f, 0.0f, 0.0f)
        },
        // 3 views (-|)
        new Rect[]
        {
            new Rect(0.0f, 0.0f, 0.5f, 0.5f),
            new Rect(0.0f, 0.5f, 0.5f, 1.0f),
            new Rect(0.0f, 0.5f, 0.5f, 0.5f),
            new Rect(0.0f, 0.0f, 0.0f, 0.0f)
        },
        // 4 views
        new Rect[]
        {
            new Rect(0.0f, 0.0f, 0.5f, 0.5f),
            new Rect(0.5f, 0.0f, 0.5f, 0.5f),
            new Rect(0.0f, 0.5f, 0.5f, 0.5f),
            new Rect(0.5f, 0.5f, 0.5f, 0.5f)
        },
    };

    #endregion Variables
}
