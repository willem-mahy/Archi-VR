using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WM;
using WM.Application;

public class EditorTestApplication : MonoBehaviour
{
    private int DefaultViewLayout = 8;

    private const string ApplicationSceneName =
        "Application_ArchiVR";
        //"Application_Demo";

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    void Start()
    {
        SetViewLayout(DefaultViewLayout);

        // Load first scene.
        LoadApplicationScene(ApplicationSceneName, 0);        
    }

    // Update is called once per frame
    void Update()
    {
        TryFinalizeApplicationInstanceInitialisation();

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

        ActivateNextApplicationInstance(i);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationInstanceIndex"></param>
    private void ActivateNextApplicationInstance(int applicationInstanceIndex)
    {
        _activeApplicationInstanceIndex = applicationInstanceIndex;

        foreach (var applicationInstance in _applicationInstances)
        {
            applicationInstance.EnableInput = false;
        }

        _applicationInstances[_activeApplicationInstanceIndex].EnableInput = true;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="applicationInstanceIndex"></param>
    private void LoadApplicationScene(string sceneName, int applicationInstanceIndex)
    {
        _applicationInstanceBeingInitializedIndex = applicationInstanceIndex;
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    /// <summary>
    /// If an application instance is being initialized, and its appliction scene is fully loaded,
    /// performs the post-load operations to finalize the initialization.
    /// - Get handle to UnityApplication instance
    /// - Push the viewlayout to the application camera.
    /// - Start initialization of the next application instance.
    /// </summary>
    void TryFinalizeApplicationInstanceInitialisation()
    {
        if (_applicationInstanceBeingInitializedIndex == -1)
        {
            return; // Nothing to do.
        }

        var tag = "EditorTestApplication.TryFinalizeApplicationSceneLoading(" + _applicationInstanceBeingInitializedIndex + ")";

        var applicationSceneOrig = SceneManager.GetSceneByName(ApplicationSceneName);

        if (applicationSceneOrig == null)
        {
            return; // Application scene not loaded yet.
        }

        if (!applicationSceneOrig.isLoaded)
        {
            return; // Application scene not loaded yet.
        }

        var postFix = "(" + _applicationInstanceBeingInitializedIndex + ")";

        // Since renaming a saved scene is not allowed, we merge the loaded application scene into a uniquely named new scene(applicationScene).
        // The originally loaded scene (applicationSceneOrig) will be destroyed automatically by merging it into the final, uniquely named, application scene.
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
        applicationInstance.ID = _applicationInstanceBeingInitializedIndex;
        applicationInstance.EnableInput = (_applicationInstanceBeingInitializedIndex == 0);
                    
        _applicationInstances.Add(applicationInstance);
                    
        _activeApplicationInstanceIndex = 0;

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
            
        camera.rect = WindowPlacements[_viewLayout][_applicationInstanceBeingInitializedIndex];

        //camera.scene = applicationScene; // Only works with preview scenes :-(

        // Disable the default camera.
        GetDefaultCameraGO(_applicationInstanceBeingInitializedIndex).SetActive(false);

        // Start initialization of the next appliction instance, if there is one left.
        if (_applicationInstanceBeingInitializedIndex < 3) // If it was not the last application instance...
        {
            // Start loading the next application instance.
            LoadApplicationScene(ApplicationSceneName, _applicationInstanceBeingInitializedIndex + 1);
        }
        else
        {
            // We just finalized initialization of the last application instance.
            _applicationInstanceBeingInitializedIndex = -1;

            //FailedExperiment_RenderOnlyAssociatedApplicationSceneForEachViewUsingCameraDelegates();
        }
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

    /// <summary>
    /// The index (into eg. 'applicationScenes', ...) of the application instance being initialized.
    /// </summary>
    private int _applicationInstanceBeingInitializedIndex = -1;

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
