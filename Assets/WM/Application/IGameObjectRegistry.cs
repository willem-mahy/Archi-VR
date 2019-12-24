using System;
using UnityEngine;

namespace WM.Application
{
    /// <summary>
    /// A registry that holds GameObject products by Guid.
    /// </summary>
    interface IGameObjectRegistry
    {
        void Register(
            Guid guid,
            GameObject gameObject);
    }
}
