using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WM
{
    /// <summary>
    /// Unity-related utility functions.
    /// </summary>
    public class UtilUnity
    {
        /// <summary>
        /// Returns a string representation of the given Vector3.
        /// </summary>
        public static string ToString(Vector3 v)
        {
            return string.Format("({0:F3}, {1:F3}, {2:F3})", v.x, v.y, v.z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        public static void Destroy(GameObject gameObject)
        {
            // We need to destroy ojects differently in Edit Mode, otherwise eg. Edit Mode Unit Tests complain.  :-(
            if (UnityEngine.Application.isEditor)
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
            else
            {
                UnityEngine.Object.Destroy(gameObject);
            }
        }

        /// <summary>
        /// Tries to find a GameObject with the given name.
        /// Throws an exception if no such GameObject found.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>The first found GameObject with the given name.</returns>
        public static GameObject TryFindGameObject(string name)
        {
            var go = GameObject.Find(name);

            if (go == null)
            {
                throw new Exception("GameObject '" + name + "' not found.");
            }

            return go;
        }

        /// <summary>
        /// Tries to find a GameObject with the given name in the given scene.
        /// Throws an exception if no such GameObject found.
        /// </summary>
        /// <param name="scene">The scene to search in.</param>
        /// <param name="name">The name to search for.</param>
        /// <returns>The first found GameObject with the given name.</returns>
        public static GameObject FindGameObjectElseError(
            Scene scene,
            string name)
        {
            var go = FindGameObject(scene, name);

            if (go == null)
            {
                throw new Exception("GameObject '" + name + "' not found in scene '" + scene.name + "'.");
            }

            return go;
        }

        /// <summary>
        /// Tries to find a GameObject with the given name in the given scene.
        /// Logs a warning if no such GameObject found.
        /// </summary>
        /// <param name="scene">The scene to search in.</param>
        /// <param name="name">The name to search for.</param>
        /// <returns>The first found GameObject with the given name, or 'null' if no such GameObject found.</returns>
        public static GameObject FindGameObjectElseWarn(
            Scene scene,
            string name,
            WM.Logger log)
        {
            var go = FindGameObject(scene, name);

            if (go == null)
            {
                log.Warning("GameObject '" + name + "' not found in scene '" + scene.name + "'.");
            }

            return go;
        }

        /// <summary>
        /// Tries to find a GameObject with the given name in the given scene.
        /// </summary>
        /// <param name="scene">The scene to search in.</param>
        /// <param name="name">The name to search for.</param>
        /// <returns>The first found GameObject with the given name, or 'null' if no such GameObject found.</returns>
        public static GameObject FindGameObject(
            Scene scene,
            string name)
        {
            foreach (var go in scene.GetRootGameObjects())
            {
                var result = FindGameObject(go, name);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Tries to find the first Component of type 'T' in the given GameObject,
        /// or any GameObject under it, recursively.
        /// </summary>
        /// <typeparam name="T">The type of component to find.</typeparam>
        /// <param name="go">The root GameObject to search under.</param>
        /// <returns>The first component found, or 'null' if no such component found.</returns>
        public static T GetFirstComponentOfType<T>(GameObject go)
        {
            // If you contain it yourself, return the component.
            var c = go.GetComponent<T>();

            if (c != null)
            {
                return c; // Found in self :-)
            }

            // Else recurse into subtree.
            for (int i = 0; i < go.transform.childCount; ++i)
            {
                var childGO = go.transform.GetChild(i).gameObject;

                c = GetFirstComponentOfType<T>(childGO);

                if (c != null)
                {
                    return c; // Found in subtree :-)
                }
            }

            // Not found :-(
            return default(T);
        }

        /// <summary>
        /// Tries to find the first GameObject with given name in the scene hierarchy starting at the given root GameObject.
        /// </summary>
        /// <param name="go">The root GameObject to search under.</param>
        /// <param name="name">The name to search for.</param>
        /// <returns>The first GameObject found, or 'null' if none found.</returns>
        public static GameObject FindGameObject(GameObject go, string name)
        {
            if (go.name == name)
            {
                return go;
            }

            // Else recurse into subtree.
            for (int i = 0; i < go.transform.childCount; ++i)
            {
                var childGO = go.transform.GetChild(i).gameObject;

                var r = FindGameObject(childGO, name);
                
                if (r != null)
                {
                    return r;
                }
            }

            // Not found :-(
            return null;
        }
    }
}
