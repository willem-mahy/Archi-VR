using System.Collections.Generic;
using UnityEngine;

namespace ArchiVR.Application.PickInitialization
{
    /// <summary>
    /// 
    /// </summary>
    public class PickInitializationType_Default
        : IPickInitializationType
    {
        public PickInitializationType_Default()
        {
        }

        public float GetQuality(List<RaycastHit> picks)
        {
            var pickClassifications = PickClassifier.Classify(picks);
            
            switch (picks.Count)
            {
                case 0:
                    {
                        return 0;
                    }
                default:
                    {
                        return 1.0f; // We only need one pick, and it can be of any type.
                    }
            }
        }

        public Vector3 AnchoringAxis_Local => Vector3.up;

        public Vector3 UpAxis_Local => Vector3.forward;

        public void Initialize(
            GameObject gameObject,
            List<RaycastHit> picks,
            float rotation)
        {
            var transform = gameObject.transform;

            switch (picks.Count)
            {
                case 0:
                    break;
                default:
                    {
                        transform.position = picks[0].point;
                        transform.rotation = Quaternion.identity;
                    }
                    break;
            }
        }
    }
}
