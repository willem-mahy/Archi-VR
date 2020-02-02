using System;
using UnityEngine;

namespace WM.Application
{
    /// <summary>
    /// A registry that holds GameObject products by Guid.
    /// </summary>
    interface IGameObjectRegistry
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="gameObject"></param>
        void Register(
            Guid guid,
            GameObject gameObject);
    }
}
