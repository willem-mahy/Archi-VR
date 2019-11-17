using UnityEngine;

namespace WM
{
    /* Utility functions for Unity.
     */
    class UtilUnity
    {
        // Clamps value to range [minValue, maxValue[ by making it cycle, if necessary.
        public static GameObject TryFindGameObject(string name)
        {
            var go = GameObject.Find(name);

            if (!go)
            {
                WM.Logger.Warning("GameObject '" + name + "' not found.");
            }
            return go;
        }
    }
}
