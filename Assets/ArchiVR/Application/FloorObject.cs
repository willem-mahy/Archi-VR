using System.Collections.Generic;
using UnityEngine;

namespace ArchiVR.Application
{
    /// <summary>
    /// 
    /// </summary>
    public class FloorObject
        : MonoBehaviour
        , IPickInitializable
    {
        public bool Initialize(List<RaycastHit> picks)
        {
            var transform = gameObject.transform;

            switch (picks.Count)
            {
                case 0:
                    return false;
                case 1:
                    {
                        transform.position = picks[0].point;
                        transform.rotation = Quaternion.identity;
                        return true;
                    }
                default: // 2 or more picks: just use the first 2
                    {
                        var direction = (picks[1].point - picks[0].point).normalized;

                        transform.position = picks[0].point;
                        transform.LookAt(picks[0].point + direction, Vector3.up);
                        return true;
                    }
            }
        }
    }
}
