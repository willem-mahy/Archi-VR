using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WM;
using WM.Application;

public class EditorTestApplication : MonoBehaviour
{
    private int DefaultViewLayout = 8;

    private const string ApplicationSceneName = "ArchiVR";

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    void Start()
    {
        HideDefaultScenes();

        SetViewLayout(DefaultViewLayout);

        //CreateApplicationPreviewScenes();

        // Load first scene.
        LoadApplicationScene(ApplicationSceneName, 0);        
    }

    // Update is called once per frame
    void Update()
    {
        TryFinalizeApplicationSceneLoading();

        //RotateCameras();
    }

    /// <summary>
    /// 
    /// </summary>
    private void CreateApplicationPreviewScenes()
    {
        for (int i = 0; i < 4; ++i)
        {
            // Init The necessary resources for processing i'th tested application instance.

            // Create an application preview scene for it...
            var applicationPreviewScene = EditorSceneManager.NewPreviewScene();
            applicationPreviewScene.name = "ApplicationPreview (" + i + ")";
            applicationPreviewScenes.Add(applicationPreviewScene);

            //LoadDefaultApplicationScene(i);

            // Setup the i'th camera to render only its corresponding tested application instance.
            var camera = GetDefaultCamera(i);

            camera.scene = applicationPreviewScene;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void HideDefaultScenes()
    {
        for (int i = 0; i < 4; ++i)
        {
            var defaultSceneGO = GameObject.Find("DefaultScene (" + i + ")");

            if (defaultSceneGO == null)
            {
                throw new System.Exception("EditorTestApplication.HideDefaultScenes(): DefaultScene (" + i + ") not found!");
            }

            defaultSceneGO.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationInstanceIndex"></param>
    private void LoadDefaultApplicationSceneIntoPreviewScene(int applicationInstanceIndex)
    {
        var applicationScene = GameObject.Find("ApplicationScene (" + applicationInstanceIndex + ")");

        if (applicationScene == null)
        {
            throw new Exception("EditorTestApplication.LoadApplicationSceneIntoPreviewScene(): ApplicationScene (" + applicationInstanceIndex + ") not found!");
        }

        EditorSceneManager.MoveGameObjectToScene(applicationScene, applicationPreviewScenes[applicationInstanceIndex]);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="applicationInstanceIndex"></param>
    private void LoadApplicationScene(string sceneName, int applicationInstanceIndex)
    {
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    // Update is called once per frame
    void TryFinalizeApplicationSceneLoading()
    {
        for (int index = 0; index < isSceneMerged.Length; ++index)
        {
            if (!isSceneMerged[index])
            {
                var scene = SceneManager.GetSceneByName(ApplicationSceneName);
                var project = SceneManager.GetSceneByName("ProjectKS047");
                if (scene.isLoaded && project.isLoaded)
                {
                    isSceneMerged[index] = true;

                    // First get the UnityApplication from the loaded application scene.
                    UnityApplication applicationInstance = null;
                    foreach (var go in scene.GetRootGameObjects())
                    {
                        applicationInstance = UtilUnity.GetFirstComponentOfType<WM.Application.UnityApplication>(go);

                        if (applicationInstance != null)
                        {
                            break;
                        }
                    }

                    if (applicationInstance == null)
                    {
                        throw new Exception("EditorTestApplication.TryFinalizeApplicationSceneLoading(): Loaded application scene does not contain a GameObject with a UnityApplication component!");
                    }

                    // Only enable input on the first application instance.
                    applicationInstance.EnableInput = (index == 0);

                    var applicationScene = SceneManager.CreateScene("ApplicationInstance" + index);

                    SceneManager.MergeScenes(scene, applicationScene);
                    SceneManager.MergeScenes(project, applicationScene);

                    applicationScenes.Add(applicationScene);

                    var cameraGO = GameObject.Find("CenterEyeAnchor");
                    if (cameraGO != null)
                    {
                        var camera = cameraGO.GetComponent<Camera>();

                        if (camera == null)
                        {
                            throw new Exception("EditorTestApplication.TryFinalizeApplicationSceneLoading(): GameObject 'CenterEyeAnchor' does not have a Camera component!");
                        }
                        else
                        { 
                            cameraGO.name = "*" + camera.name; // So it is not found anymore next time.
                            camera.rect = WindowPlacements[_viewLayout][index];
                            camera.scene = applicationScene;

                            //GetDefaultCamera(index).scene = applicationScene;
                            GetDefaultCameraGO(index).SetActive(false);
                        }
                    }

                    // Start loading the next application instance (if there is one)
                    if (index != isSceneMerged.Length - 1)
                    {
                        LoadApplicationScene(ApplicationSceneName, index + 1);
                    }
                }
                break;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void RotateDefaultCameras()
    {
        for (int i = 0; i < 4; ++i)
        {
            var t = GetDefaultCamera(i).transform;
            t.Rotate(Vector3.up, 0.01f);
            t.RotateAroundLocal(Vector3.left, 0.01f);
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
    /// The application preview scenes.
    /// </summary>
    private List<Scene> applicationPreviewScenes = new List<Scene>();

    /// <summary>
    /// The application scenes.
    /// </summary>
    private List<Scene> applicationScenes = new List<Scene>();

    /// <summary>
    /// The active view layout.
    /// </summary>
    private int _viewLayout = -1;

    /// <summary>
    /// 
    /// </summary>
    bool[] isSceneMerged = { false, false, false, false };

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
