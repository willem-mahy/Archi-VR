using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditorTestApplication : MonoBehaviour
{
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

            if (i == 0)
            {
                LoadApplicationScene("Scene", i);
            }
            else
            {
                LoadDefaultApplicationScene(i);
            }

            // Setup the i'th camera to render only its corresponding tested application instance.
            var camera = GetCamera(i);

            camera.scene = applicationPreviewScene;
        }

        SetViewLayout(1);
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
    bool isSceneMerged = false;
    //bool isProjectMerged = false;

    //void Update1()
    //{
    //    if (!isSceneMerged)
    //    {
    //        var scene = SceneManager.GetSceneByName("Scene");
    //        if (scene.isLoaded)
    //        {
    //            var camera = GameObject.Find("CenterEyeAnchor");
    //            if (camera != null)
    //            {
    //                camera.SetActive(false);
    //            }

    //            isSceneMerged = true;
    //            foreach (var go in scene.GetRootGameObjects())
    //            {
    //                EditorSceneManager.MoveGameObjectToScene(go, applicationPreviewScenes[0]);
    //                go.SetActive(true);
    //            }

    //            SceneManager.UnloadSceneAsync(scene);
    //        }
    //    }

    //    if (!isProjectMerged)
    //    {
    //        var scene = SceneManager.GetSceneByName("ProjectKS047");
    //        if (scene.isLoaded)
    //        {
    //            isProjectMerged = true;
    //            foreach (var go in scene.GetRootGameObjects())
    //            {
    //                EditorSceneManager.MoveGameObjectToScene(go, applicationPreviewScenes[0]);
    //                go.SetActive(true);
    //            }

    //            SceneManager.UnloadSceneAsync(scene);
    //        }
    //    }

    //    UpdateRootGameObjects(applicationPreviewScenes[0].GetRootGameObjects());

    //    //if ((DateTime.Now - lastToggle).TotalMilliseconds > 2000)
    //    //{
    //    //    var numViewLayouts = WindowPlacements.Length;

    //    //    // Activate next (cycle-wise spoken) view layout.
    //    //    viewLayout = ++viewLayout % numViewLayouts;

    //    //    SetViewLayout(viewLayout);

    //    //    lastToggle = DateTime.Now;
    //    //}

    //    GetCamera(0).transform.Rotate(Vector3.up, 0.01f);
    //    GetCamera(0).transform.RotateAroundLocal(Vector3.left, 0.01f);
    //    GetCamera(1).transform.Rotate(Vector3.up, 0.01f);
    //    GetCamera(1).transform.RotateAroundLocal(Vector3.left, 0.01f);
    //}

    void Update2()
    {
        if (!isSceneMerged)
        {
            var scene = SceneManager.GetSceneByName("Scene");
            var project = SceneManager.GetSceneByName("ProjectKS047");
            if (scene.isLoaded && project.isLoaded)
            {
                isSceneMerged = true;

                var camera = GameObject.Find("CenterEyeAnchor");
                if (camera != null)
                {
                    camera.SetActive(false);
                }

                SceneManager.MergeScenes(project, scene);

                GetCamera(0).scene = scene;
            }
        }
        GetCamera(0).transform.Rotate(Vector3.up, 0.01f);
        GetCamera(0).transform.RotateAroundLocal(Vector3.left, 0.01f);
        GetCamera(1).transform.Rotate(Vector3.up, 0.01f);
        GetCamera(1).transform.RotateAroundLocal(Vector3.left, 0.01f);

    }

    // Update is called once per frame
    void Update()
    {
        Update2();
    }

    private void UpdateRootGameObjects(GameObject[] gameObjects)
    {
        if (gameObjects == null)
        {
            return;
        }
        foreach (var gameObject in gameObjects)
        {
            UpdateGameObject(gameObject);
        }
    }

    private void UpdateGameObject(GameObject gameObject)
    {
        gameObject.SendMessage("Update");
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
