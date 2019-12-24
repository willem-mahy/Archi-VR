using System;
using UnityEngine;

namespace WM.Application
{
    /// <summary>
    /// A factory to create GameObject products by Guid.
    /// </summary>
    interface IGameObjectFactory
    {
        GameObject Create(
            Guid guid,
            Vector3 position,
            Quaternion rotation);
    }
}
