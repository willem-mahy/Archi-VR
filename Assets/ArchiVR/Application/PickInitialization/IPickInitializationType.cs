using System.Collections.Generic;
using UnityEngine;

namespace ArchiVR.Application.PickInitialization
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPickInitializationType
    {
        float GetQuality(List<RaycastHit> picks);

        void Initialize(
            GameObject gameObject,
            List<RaycastHit> picks);
    }
}
