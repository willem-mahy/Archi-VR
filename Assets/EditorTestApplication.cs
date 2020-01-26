using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditorTestApplication : MonoBehaviour
{
    private const string ApplicationSceneName = "Scene";

    // Start is called before the first frame update
    void Start()
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
            var camera = GetCamera(i);

            camera.scene = applicationPreviewScene;
        }

        // Load first scene.
        LoadApplicationScene(ApplicationSceneName, 0);

        SetViewLayout(8);
    }

    private void LoadDefaultApplicationScene(int applicationInstanceIndex)
    {
        var applicationScene = GameObject.Find("ApplicationScene (" + applicationInstanceIndex + ")");

        if (applicationScene == null)
        {
            throw new System.Exception("EditorTestApplication.Start(): ApplicationScene (" + applicationInstanceIndex + ") not found!");
        }

        EditorSceneManager.MoveGameObjectToScene(applicationScene, applicationPreviewScenes[applicationInstanceIndex]);
    }

    private void LoadApplicationScene(string sceneName, int applicationInstanceIndex)
    {
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    List<Scene> applicationPreviewScenes = new List<Scene>();
    DateTime lastToggle = DateTime.Now;
    int viewLayout = -1;
    bool[] isSceneMerged = { false, false, false, false };

    // Update is called once per frame
    void Update()
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

                    var camera = GameObject.Find("CenterEyeAnchor");
                    if (camera != null)
                    {
                        camera.SetActive(false);
                    }

                    var newScene = SceneManager.CreateScene("ApplicationInstance" + index);

                    SceneManager.MergeScenes(scene, newScene);
                    SceneManager.MergeScenes(project, newScene);

                    GetCamera(index).scene = newScene;

                    if (index != isSceneMerged.Length - 1)
                    {
                        LoadApplicationScene(ApplicationSceneName, index + 1);
                    }
                }
                break;
            }
        }

        for (int i = 0; i < 4; ++i)
        {
            GetCamera(i).transform.Rotate(Vector3.up, 0.01f);
            GetCamera(i).transform.RotateAroundLocal(Vector3.left, 0.01f);
        }
    }

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

    void SetViewLayout(int viewLayoutID)
    {
        for (int i = 0; i < 4; ++i)
        {
            GetCamera(i).rect = WindowPlacements[viewLayoutID][i];
        }
    }

    Camera GetCamera(int i)
    {
        // Setup the i'th camera to render only its corresponding tested application instance.
        var cameraGO = GameObject.Find("Main Camera (" + i + ")");

        return cameraGO.GetComponent<Camera>() as Camera;
    }
}
