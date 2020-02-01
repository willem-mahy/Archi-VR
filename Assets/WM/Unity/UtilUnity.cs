using UnityEngine;

namespace WM
{
    /* Utility functions for Unity.
     */
    public class UtilUnity
    {
        /// <summary>
        /// Tries to find a GameObject with the given name.
        /// Logs a warning if no such GameObject found.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>The first found GameObject with the given name.</returns>
        public static GameObject TryFindGameObject(string name)
        {
            var go = GameObject.Find(name);

            if (!go)
            {
                WM.Logger.Warning("GameObject '" + name + "' not found.");
            }
            return go;
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
