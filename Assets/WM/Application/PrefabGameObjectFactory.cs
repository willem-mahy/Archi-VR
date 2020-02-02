using System;
using System.Collections.Generic;
using UnityEngine;

namespace WM.Application
{
    /// <summary>
    /// A GameObject Factory that creates GameObjects by instantiating pre-registered Prefabs.
    /// </summary>
    public class PrefabGameObjectFactory : IGameObjectRegistry, IGameObjectFactory
    {
        /// <summary>
        /// The list of Prefabs to instanciate into GameObjects.
        /// </summary>
        private Dictionary<Guid, GameObject> prefabs = new Dictionary<Guid, GameObject>();

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

            if (!prefabs.ContainsKey(key))
            {
                throw new Exception("No prefab registered for key (" + key.ToString() + ").");
            }

            return UnityEngine.Object.Instantiate(
                    prefabs[key],
                    position,
                    rotation);
        }

        /// <summary>
        /// <see cref="IGameObjectRegistry.Register(Guid, GameObject)"/> implementation.
        /// </summary>
        public void Register(
            Guid key,
            GameObject gameObject)
        {
            if (key == Guid.Empty)
            {
                throw new Exception("Key cannot be Guid.Empty.");
            }

            if (gameObject == null)
            {
                throw new Exception("GameObject cannot be null.");
            }

            prefabs[key] = gameObject;
        }

        /// <summary>
        /// TODO: add to one of the implemented interfaces?  Hich one then?
        /// </summary>
        /// <returns></returns>
        public List<Guid> GetRegisteredIDs()
        {
            return new List<Guid>(prefabs.Keys);
        }
    }
}
