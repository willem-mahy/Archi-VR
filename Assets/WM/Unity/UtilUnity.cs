using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using WM.Application;

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

        public static T FindApplication<T>(GameObject gameObject)
        {
            var applicationGO = UtilUnity.FindGameObjectElseError(gameObject.scene, "Application");

            var application = applicationGO.GetComponent<T>();

            if (application == null)
            {
                var errorMessage = "No component of type '" + typeof(T).ToString() + "' found on gameobject 'Application'!";
                Debug.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            return application;
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

    public static class TransformExtensions
    {
        public static void FromMatrix(this Transform transform, Matrix4x4 matrix)
        {
            transform.localScale = matrix.ExtractScale();
            transform.rotation = matrix.ExtractRotation();
            transform.position = matrix.ExtractPosition();
        }
    }

    public static class MatrixExtensions
    {
        public static Quaternion ExtractRotation(this Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return Quaternion.LookRotation(forward, upwards);
        }

        public static Vector3 ExtractPosition(this Matrix4x4 matrix)
        {
            Vector3 position;
            position.x = matrix.m03;
            position.y = matrix.m13;
            position.z = matrix.m23;
            return position;
        }

        public static Vector3 ExtractScale(this Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }
    }
}
