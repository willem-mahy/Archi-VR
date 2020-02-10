using System;
using System.Collections.Generic;
using UnityEngine;

namespace WM.Application
{
    /// <summary>
    /// A GameObject Factory that creates GameObjects by instantiating pre-registered GameObjects.
    /// </summary>
    public class ResourcePrefabGameObjectFactory : IResourcePathRegistry, IGameObjectFactory
    {
        /// <summary>
        /// The list of Prefabs to instanciate into GameObjects.
        /// </summary>
        private Dictionary<Guid, string> prefabPaths = new Dictionary<Guid, string>();

        /// <summary>
        /// <see cref="IGameObjectFactory.Create(Guid, Vector3, Quaternion)"/> implementation.
        /// </summary>
        public GameObject Create(
            Guid key,
            Vector3 position,
            Quaternion rotation)
        {
            if (key == Guid.Empty)
            {
                throw new Exception("Key cannot be Guid.Empty.");
            }

            if (!prefabPaths.ContainsKey(key))
            {
                throw new Exception("No prefab registered for key (" + key.ToString() + ").");
            }

            var prefab = Resources.Load(prefabPaths[key]);

            return UnityEngine.Object.Instantiate(
                    prefab,
                    position,
                    rotation) as GameObject;
        }

        /// <summary>
        /// <see cref="IGameObjectRegistry.Register(Guid, GameObject)"/> implementation.
        /// </summary>
        public void Register(
            Guid key,
            string resourcePath)
        {
            if (key == Guid.Empty)
            {
                throw new Exception("Key cannot be Guid.Empty.");
            }

            if (resourcePath == null)
            {
                throw new Exception("Resource path cannot be null.");
            }

            if (resourcePath == "")
            {
                throw new Exception("Resource path cannot be empty string.");
            }

            prefabPaths[key] = resourcePath;
        }

        /// <summary>
        /// TODO: add to one of the implemented interfaces?  Which one then?
        /// </summary>
        /// <returns></returns>
        public List<string> GetRegisteredGameObjectNames()
        {
            var names = new List<string>();
            foreach (var id in prefabPaths.Keys)
            {
                var tokens = prefabPaths[id].Split(' ');
                names.Add(tokens[tokens.Length - 1]);
            }
            return names;
        }

        /// <summary>
        /// TODO: add to one of the implemented interfaces?  Which one then?
        /// </summary>
        /// <returns></returns>
        public List<Guid> GetRegisteredIDs()
        {
            return new List<Guid>(prefabPaths.Keys);
        }

        /// <summary>
        /// TODO: add to one of the implemented interfaces?  Which one then?
        /// </summary>
        /// <returns></returns>
        public int NumRegistered
        {
            get
            {
                return prefabPaths.Count;
            }
        }
    }
}
