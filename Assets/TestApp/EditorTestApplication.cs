using System;
using System.Collections.Generic;
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
        TryFinalizeApplicationSceneLoading();

        if (Input.GetKey(KeyCode.Tab))
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

    // Update is called once per frame
    void TryFinalizeApplicationSceneLoading()
    {
        if (_applicationInstanceBeingInitializedIndex == -1)
        {
            return; // Nothing to do.
        }

        var tag = "EditorTestApplication.TryFinalizeApplicationSceneLoading(" + _applicationInstanceBeingInitializedIndex + ")";

        var applicationScene = SceneManager.GetSceneByName(ApplicationSceneName);

        if (applicationScene == null)
        {
            return; // Application scene not loaded yet.
        }
        
        if (!applicationScene.isLoaded)
        {
            return; // Application scene not loaded yet.
        }
        
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

        applicationScenes.Add(applicationScene);

        var cameraGO = GameObject.Find("CenterEyeAnchor");
        if (cameraGO == null)
        {
            throw new Exception(tag + "GameObject 'CenterEyeAnchor' not found in application scene!");
        }

        cameraGO.name = "*" + cameraGO.name; // So it is not found anymore next time.

        var camera = cameraGO.GetComponent<Camera>();

        if (camera == null)
        {
            throw new Exception(tag + "GameObject 'CenterEyeAnchor' does not have a Camera component!");
        }
            
        camera.rect = WindowPlacements[_viewLayout][_applicationInstanceBeingInitializedIndex];
        camera.scene = applicationScene;

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
