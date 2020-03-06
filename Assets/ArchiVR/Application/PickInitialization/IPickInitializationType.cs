using System.Collections.Generic;
using UnityEngine;

namespace ArchiVR.Application.PickInitialization
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPickInitializationType
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="picks"></param>
        /// <returns></returns>
        float GetQuality(List<RaycastHit> picks);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Vector3 AnchoringAxis_Local { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Vector3 UpAxis_Local { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="picks"></param>
        /// <param name="rotation"></param>
        void Initialize(
            GameObject gameObject,
            List<RaycastHit> picks,
            float rotation);
    }
}
