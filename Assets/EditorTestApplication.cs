using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditorTestApplication : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var scenes = new List<Scene>();

        for (int i = 0; i < 4; ++i)
        {
            // Init The necessary resources for processing i'th tested application instance.
            
            // Create an application preview scene for it...
            var applicationPreviewScene = EditorSceneManager.NewPreviewScene();
            applicationPreviewScene.name = "ApplicationPreview (" + i + ")";
            scenes.Add(applicationPreviewScene);

            // ... initialize with an application scene.
            var applicationScene = GameObject.Find("ApplicationScene (" + i + ")");

            if (applicationScene == null)
            {
                throw new System.Exception("EditorTestApplication.Start(): ApplicationScene ("+ i +") not found!");
            }

            

            //var rootGameObjects = applicationPreviewScene.GetRootGameObjects();
            //applicationScene.transform.parent = rootGameObjects[0].transform;


            //PrefabUtility.LoadPrefabContentsIntoPreviewScene(path, applicationPreviewScene);
            

            EditorSceneManager.MoveGameObjectToScene(applicationScene, applicationPreviewScene);


            // Setup the i'th camera to render only its corresponding tested application instance.
            var camera = GetCamera(i);

            camera.scene = applicationPreviewScene;
        }
    }

    DateTime lastToggle = DateTime.Now;
    int viewLayout = -1;

    // Update is called once per frame
    void Update()
    {
        if ((DateTime.Now - lastToggle).TotalMilliseconds > 2000)
        {
            var numViewLayouts = WindowPlacements.Length;

            // Activate next (cycle-wise spoken) view layout.
            viewLayout = ++viewLayout % numViewLayouts;

            SetViewLayout(viewLayout);

            lastToggle = DateTime.Now;
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
