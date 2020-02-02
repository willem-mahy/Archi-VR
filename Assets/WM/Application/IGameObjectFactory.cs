using System;
using UnityEngine;

namespace WM.Application
{
    /// <summary>
    /// A factory to create GameObject products by Guid.
    /// </summary>
    interface IGameObjectFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        GameObject Create(
            Guid guid,
            Vector3 position,
            Quaternion rotation);
    }
}
