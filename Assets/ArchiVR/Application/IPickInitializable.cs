using System.Collections.Generic;
using UnityEngine;

namespace ArchiVR.Application
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPickInitializable
    {
        bool Initialize(List<RaycastHit> picks);
    }
}
